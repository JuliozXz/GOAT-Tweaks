using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using GoatTweaks.Core.Assets;
using GoatTweaks.Core.Catalog;
using GoatTweaks.Core.Conflicts;
using GoatTweaks.Core.Execution;
using GoatTweaks.Workflow;

namespace GoatTweaks;

public partial class MainWindow : Window
{
    private readonly CatalogService _catalog = CatalogService.LoadEmbedded();
    private readonly ConflictDetector _conflicts = ConflictDetector.LoadEmbedded();
    private readonly ObservableCollection<QueuedAction> _queue = [];
    private readonly ObservableCollection<LogRow> _logRows = [];
    private readonly Dictionary<string, ListBoxItem> _sectionItems = new(StringComparer.Ordinal);
    private readonly DispatcherTimer _searchDebounce = new() { Interval = TimeSpan.FromMilliseconds(220) };
    private readonly DispatcherTimer _tailTimer = new(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(800) };
    private string _activeSection = "Home";
    private bool _isApplying;
    private bool _closed;
    private string? _logPath;
    private JsonlLogTailReader _logReader = new();
    private Task _logReadTask = Task.CompletedTask;
    private List<QueuedAction> _runActions = [];
    private Dictionary<string, QueuedAction> _runById = new(StringComparer.Ordinal);

    public static readonly DependencyProperty CanEditQueueProperty =
        DependencyProperty.Register(nameof(CanEditQueue), typeof(bool), typeof(MainWindow), new PropertyMetadata(true));

    public bool CanEditQueue
    {
        get => (bool)GetValue(CanEditQueueProperty);
        private set => SetValue(CanEditQueueProperty, value);
    }

    public MainWindow()
    {
        _workflow = ServiceCatalog.Load(_catalog);
        DataContext = Dashboard;
        InitializeComponent();
        var availableWidth = Math.Max(600, SystemParameters.WorkArea.Width - 24);
        var availableHeight = Math.Max(480, SystemParameters.WorkArea.Height - 24);
        MinWidth = Math.Min(MinWidth, availableWidth);
        MinHeight = Math.Min(MinHeight, availableHeight);
        Width = Math.Min(Width, availableWidth);
        Height = Math.Min(Height, availableHeight);
        _searchDebounce.Tick += (_, _) => { _searchDebounce.Stop(); RenderCards(); };
        _tailTimer.Tick += async (_, _) => await RefreshLogsAsync();
        _queue.CollectionChanged += (_, _) => UpdateQueueState();
        InitializeDashboard();
        QueueList.ItemsSource = _queue;
        LogsList.ItemsSource = _logRows;
        LogsDetailList.ItemsSource = _logRows;
        SectionsList.SelectedItem = _sectionItems[_activeSection];
        RenderCards();
        UpdateQueueState();
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _searchDebounce.Stop();
        _searchDebounce.Start();
    }

    private void UpdateQueueState()
    {
        if (QueueCountLabel is null) return;
        var pending = _queue.Count(action => action.State == "pending");
        QueueCountLabel.Text = _queue.Count.ToString("00");
        Dashboard.QueueButton = $"Revisar fila · {_queue.Count:00}";
        Dashboard.QueueSummary = pending > 0 ? $"{pending} ação(ões) aguardando execução" : _queue.Count > 0 ? "Confira os resultados da execução" : "Sua fila está vazia";
        ApplyButton.IsEnabled = pending > 0 && !_isApplying && !IntelligencePanel.IsBusy;
        ClearQueueButton.IsEnabled = _queue.Count > 0 && !_isApplying;
        if (!_isApplying)
        {
            QueueHintLabel.Text = pending > 0 ? $"{pending} ações prontas para aplicar."
                : _queue.Count > 0 ? "Execução encerrada. Confira o resultado de cada ação."
                : "Adicione as ações que deseja aplicar.";
            if (pending > 0) CurrentActionLabel.Text = "Pronto para executar";
        }
    }

    private void AdvancedModeCheck_Changed(object sender, RoutedEventArgs e)
    {
        if (_isApplying || CardsList is null) return;
        if (AdvancedModeCheck.IsChecked != true)
            foreach (var action in _queue.Where(action => action.State == "pending" && ActionSafetyPolicy.RequiresAdvancedMode(action.Item)).ToList())
                _queue.Remove(action);
        ModeLabel.Text = AdvancedModeCheck.IsChecked == true ? "Modo avançado" : "Modo cliente";
        RenderCards();
    }

    private void RemoveQueueButton_Click(object sender, RoutedEventArgs e)
    {
        if (CanEditQueue && sender is Button { Tag: QueuedAction action }) _queue.Remove(action);
    }

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (!_isApplying && IntelligencePanel?.IsMutating != true) return;
        e.Cancel = true;
        ShowStatus("Aguarde a conclusão da operação de configurações para fechar o aplicativo.", true);
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (CanEditQueue && sender is Button { Tag: CatalogItem item }) ExecuteCatalogAction(item, false);
    }

    private void PreviewButton_Click(object sender, RoutedEventArgs e)
    {
        if (CanEditQueue && sender is Button { Tag: CatalogItem item } && ShowActionDialog(item, false) is { } request)
            ExecuteCatalogRequest(item, request);
    }

    private void RevertButton_Click(object sender, RoutedEventArgs e)
    {
        if (!CanEditQueue || sender is not Button { Tag: CatalogItem item }) return;
        if (item.Revert is { } id && _catalog.ById.TryGetValue(id, out var revertItem)) ExecuteCatalogAction(revertItem, false);
        else ExecuteCatalogAction(item, true);
    }

    private void ExecuteCatalogAction(CatalogItem item, bool revert)
    {
        if (!CanEditQueue) return;
        _queue.Clear();
        QueueAction(item, revert);
        if (_queue.Any(action => action.State == "pending")) ApplyButton_Click(this, new RoutedEventArgs());
    }

    private void ExecuteCatalogRequest(CatalogItem item, ActionRequest request)
    {
        if (!CanEditQueue) return;
        _queue.Clear();
        AddToQueue(item, request);
        if (_queue.Any(action => action.State == "pending")) ApplyButton_Click(this, new RoutedEventArgs());
    }

    private void QueueAction(CatalogItem item, bool revert)
    {
        if (!CanEditQueue) return;
        var request = ShowActionDialog(item, revert);
        if (request is not null) AddToQueue(item, request);
    }

    private ActionRequest? ShowActionDialog(CatalogItem item, bool revert)
    {
        var readOnly = !CanEditQueue || ActionSafetyPolicy.RequiresAdvancedMode(item) && AdvancedModeCheck.IsChecked != true;
        var fields = new List<(string Prompt, TextBox Input)>();
        var body = new StackPanel { Margin = new Thickness(18) };
        body.Children.Add(new TextBlock { Text = _workflow.Actions[item.Id].Title, FontSize = 20, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
        body.Children.Add(new TextBlock { Text = "Nome original: " + item.Title, FontSize = 11, Foreground = (Brush)FindResource("MutedBrush"), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 6, 0, 0) });
        body.Children.Add(new TextBlock { Text = item.Description, Margin = new Thickness(0, 8, 0, 12), TextWrapping = TextWrapping.Wrap });
        body.Children.Add(new TextBlock { Text = "Ferramenta original preservada. O diário do GOAT Intelligence cobre somente seus ajustes gerenciados; scripts legados não possuem restauração exata garantida. Revise os efeitos e os backups específicos antes de executar.", TextWrapping = TextWrapping.Wrap, Foreground = (Brush)FindResource("WarningBrush"), Margin = new Thickness(0,0,0,12) });
        CheckBox? manualReview = null;
        if(ActionSafetyPolicy.RequiresManualReview(item))
        {
            manualReview = new CheckBox { Content = new TextBlock { Text = "Revisei manualmente a compatibilidade, os riscos e o procedimento de restauração. Executarei esta ferramenta isoladamente.", TextWrapping = TextWrapping.Wrap }, Margin = new Thickness(0,0,0,12) };
            body.Children.Add(manualReview);
        }
        body.Children.Add(new TextBlock
        {
            Text = PreviewDetails(item),
            FontFamily = new FontFamily("Consolas"),
            TextWrapping = TextWrapping.Wrap,
            Background = (Brush)FindResource("InputBrush"),
            Padding = new Thickness(8)
        });
        if (revert)
            body.Children.Add(new TextBlock { Text = "Restaurar padrão segue a implementação da origem e não recupera um backup das configurações anteriores do cliente.", TextWrapping = TextWrapping.Wrap, Foreground = (Brush)FindResource("WarningBrush"), Margin = new Thickness(0, 8, 0, 8) });
        body.Children.Add(new TextBlock { Text = $"Fonte: {item.Source}\nRequer administrador: {(item.RequiresAdmin ? "Sim" : "Não")}\nReversão: {item.Revert ?? "Não há reversão automática."}\nRisco: {RiskLabel(item.Risk)}", TextWrapping = TextWrapping.Wrap });
        body.Children.Add(new TextBlock
        {
            Text = $"Flags: {(item.Flags.Count == 0 ? "nenhuma" : string.Join(", ", item.Flags))}\nDownloads ({item.Downloads.Count}): {(item.Downloads.Count == 0 ? "nenhum" : string.Join(", ", item.Downloads))}\nPrecisa de internet: {(item.NeedsInternet ? "Sim" : "Não")}",
            Margin = new Thickness(0, 8, 0, 0), TextWrapping = TextWrapping.Wrap
        });
        var duplicates = _conflicts.FindDuplicateInfo(item.Id);
        foreach (var duplicate in duplicates)
        {
            var others = duplicate.SelectedItems.Select(id => _catalog.ById.TryGetValue(id, out var other)
                ? $"{other.Title} ({other.Source})" : id);
            body.Children.Add(new TextBlock
            {
                Text = $"Também alterado por: {string.Join(", ", others)}",
                Margin = new Thickness(0, 7, 0, 0),
                TextWrapping = TextWrapping.Wrap
            });
        }

        ComboBox? optionBox = null;
        if (item.Options.Count > 0)
        {
            optionBox = new ComboBox { ItemsSource = item.Options, DisplayMemberPath = nameof(CatalogOption.Label), SelectedIndex = 0, Margin = new Thickness(0, 8, 0, 0) };
            body.Children.Add(optionBox);
        }
        foreach (var prompt in item.FreeInputs)
        {
            body.Children.Add(new TextBlock { Text = prompt, Margin = new Thickness(0, 8, 0, 2) });
            TextBox input;
            if (prompt.Equals("nic", StringComparison.OrdinalIgnoreCase))
            {
                var choices = new ComboBox
                {
                    ItemsSource = NetworkInterface.GetAllNetworkInterfaces().Select(adapter => adapter.Name)
                        .Where(InputValidator.IsSafeNetworkInterface).ToList(),
                    SelectedIndex = 0
                };
                input = new TextBox { Text = choices.SelectedItem as string ?? "", Visibility = Visibility.Collapsed };
                choices.SelectionChanged += (_, _) => input.Text = choices.SelectedItem as string ?? "";
                body.Children.Add(choices);
            }
            else
            {
                input = new TextBox();
                body.Children.Add(input);
            }
            fields.Add((prompt, input));
        }
        TextBlock? stdinPreview = null;
        if (item.Source == "Ultimate")
        {
            stdinPreview = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                FontFamily = new FontFamily("Consolas"),
                Background = (Brush)FindResource("InputBrush"),
                Padding = new Thickness(8),
                Margin = new Thickness(0, 10, 0, 0)
            };
            body.Children.Add(stdinPreview);
            void UpdateStdinPreview()
            {
                try
                {
                    var selectedOption = revert ? item.Revert : (optionBox?.SelectedItem as CatalogOption)?.Key;
                    var stdin = UltimateInputBuilder.Build(item, selectedOption, fields.Select(field => field.Input.Text).ToList());
                    stdinPreview.Text = $"Linhas exatas de stdin:\n{stdin}";
                }
                catch (Exception exception)
                {
                    stdinPreview.Text = $"Linhas exatas de stdin:\nEntrada ainda inválida: {exception.Message}";
                }
            }
            if (optionBox is not null)
                optionBox.SelectionChanged += (_, _) => UpdateStdinPreview();
            foreach (var field in fields)
                field.Input.TextChanged += (_, _) => UpdateStdinPreview();
            UpdateStdinPreview();
        }
        CheckBox? console = null;
        if (item.Source == "Ultimate")
        {
            console = new CheckBox
            {
                Content = item.ConsoleRequired
                    ? new TextBlock { Text = "Este script exige interação no console (ReadKey/pause/janela); responda na janela do PowerShell.", TextWrapping = TextWrapping.Wrap }
                    : "Executar em uma janela visível do PowerShell",
                IsChecked = item.ConsoleRequired,
                IsEnabled = !item.ConsoleRequired,
                Margin = new Thickness(0, 9, 0, 0)
            };
            body.Children.Add(console);
        }
        CheckBox? enabled = null;
        if (item.Source == "RyTuneX" && item.Kind == "toggle")
        {
            enabled = new CheckBox { Content = "Ativar", IsChecked = true, Margin = new Thickness(0, 8, 0, 0) };
            body.Children.Add(enabled);
        }
        if (item.Source == "WinSux")
            body.Children.Add(new TextBlock
            {
                Text = "Este fluxo reinicia o Windows em Modo de Segurança, altera Defender/UAC/VBS, remove aplicativos e não possui reversão automática.",
                Margin = new Thickness(0, 10, 0, 0), TextWrapping = TextWrapping.Wrap,
                Foreground = (Brush)FindResource("WarningBrush")
            });
        var validationMessage = new TextBlock { TextWrapping = TextWrapping.Wrap, Foreground = (Brush)FindResource("WarningBrush"), Margin = new Thickness(0, 10, 0, 0) };
        body.Children.Add(validationMessage);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 14, 0, 0) };
        var ok = new Button { Content = "Executar ação", IsEnabled = !readOnly, IsDefault = !readOnly, Padding = new Thickness(10, 5, 10, 5) };
        var cancel = new Button { Content = "Fechar", IsCancel = true, Padding = new Thickness(10, 5, 10, 5), Margin = new Thickness(7, 0, 0, 0) };
        buttons.Children.Add(ok);
        buttons.Children.Add(cancel);
        body.Children.Add(buttons);
        var window = new Window { Title = "Detalhes da ação", Owner = this, Width = Math.Min(650, SystemParameters.WorkArea.Width - 32), Height = Math.Min(700, SystemParameters.WorkArea.Height - 32), WindowStartupLocation = WindowStartupLocation.CenterOwner, Content = new ScrollViewer { Content = body, VerticalScrollBarVisibility = ScrollBarVisibility.Auto } };
        ActionRequest? request = null;
        ok.Click += (_, _) =>
        {
            try
            {
                var answers = fields.Select(field =>
                {
                    InputValidator.ValidateFreeInput(field.Prompt, field.Input.Text);
                    return field.Input.Text;
                }).ToList();
                request = new ActionRequest
                {
                    ItemId = item.Id,
                    OptionKey = (optionBox?.SelectedItem as CatalogOption)?.Key,
                    Inputs = answers,
                    Revert = revert,
                    ConsoleWindow = item.ConsoleRequired || console?.IsChecked == true || ActionSafetyPolicy.RequiresManualReview(item),
                    Enabled = enabled?.IsChecked,
                    ManualReviewAcknowledged = manualReview?.IsChecked == true,
                    AdvancedModeEnabled = AdvancedModeCheck.IsChecked == true
                };
                if(ActionSafetyPolicy.RequiresManualReview(item) && manualReview?.IsChecked != true)
                    throw new InvalidOperationException("Marque a revisão manual desta ferramenta de nível 3 antes de prosseguir.");
                window.DialogResult = true;
            }
            catch (Exception exception)
            {
                validationMessage.Text = exception.Message;
            }
        };
        return window.ShowDialog() == true ? request : null;
    }

    private async void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isApplying || IntelligencePanel.IsBusy) return;
        var actions = _queue.Where(action => action.State == "pending").ToList();
        if (actions.Count == 0) return;
        try
        {
            foreach (var action in actions) ActionSafetyPolicy.Validate(action.Item, action.Request);
            if(actions.Count>1 && actions.Any(action=>ActionSafetyPolicy.RequiresManualReview(action.Item)))
                throw new InvalidOperationException("Uma ferramenta de nível 3 precisa ser executada manualmente e sozinha.");
            var ids = actions.Select(action => action.Item.Id).ToList();
            if (_conflicts.FindExclusiveViolation(ids) is { } exclusive)
            {
                ShowStatus($"{exclusive.Group.Setting}: esta ação precisa ser executada sozinha.", true);
                return;
            }
            if (_conflicts.FindUserChoiceConflicts(ids).Count > 0)
            {
                ShowStatus("Há escolhas conflitantes. Remova uma delas antes de aplicar.", true);
                return;
            }
        }
        catch (Exception exception) { ShowStatus(exception.Message, true); return; }

        _isApplying = true;
        IntelligencePanel.LegacyExecutionActive = true;
        CanEditQueue = false;
        _runActions = actions;
        _runById = actions.ToDictionary(action => action.Item.Id, StringComparer.Ordinal);
        _logPath = GoatPaths.NewLogPath();
        _logReader = new JsonlLogTailReader();
        _logRows.Clear();
        ExecutionProgress.Maximum = actions.Count;
        ExecutionProgress.Value = 0;
        ApplyButton.Content = "Executando...";
        CurrentActionLabel.Text = "Preparando a ação";
        RunSummaryLabel.Text = "Acompanhe o resultado abaixo";
        ShowStatus("Executando a ação selecionada.");
        UpdateQueueState();
        _tailTimer.Start();

        try
        {
            await System.Windows.Threading.Dispatcher.Yield(DispatcherPriority.Background);
            var admins = actions.Where(action => action.Item.RequiresAdmin).ToList();
            var authorized = admins.Count == 0 || await RunAdminBatchAsync(admins);
            var regular = actions.Where(action => !action.Item.RequiresAdmin).ToList();
            if (!authorized)
            {
                foreach (var action in regular)
                    await new JsonlLogWriter(_logPath).AppendAsync(new ActionLogEntry(DateTimeOffset.Now,
                        action.Item.Id, action.Item.Source, "Ação cancelada", 1223, "", "A autorização do Windows foi cancelada."));
            }
            else if (regular.Count > 0)
            {
                CurrentActionLabel.Text = "Preparando os arquivos das ações";
                var assets = Environment.GetEnvironmentVariable("GOATTWEAKS_ASSET_ROOT")
                    ?? Path.Combine(GoatPaths.LocalRoot, "assets", typeof(App).Assembly.GetName().Version!.ToString());
                await Task.Run(() => EmbeddedAssetStore.MaterializeThirdParty(assets));
                foreach (var action in regular)
                {
                    try { await RunSingleAsync(action); }
                    catch (Exception exception)
                    {
                        await new JsonlLogWriter(_logPath).AppendAsync(new ActionLogEntry(DateTimeOffset.Now,
                            action.Item.Id, action.Item.Source, "Falha ao iniciar a ação", 1, "", exception.Message));
                    }
                }
            }
            var entries = await Task.Run(() => new JsonlLogWriter(_logPath).ReadAllAsync());
            foreach (var action in actions)
            {
                // The log is unique to this run, including actions that correct the Windows clock.
                var result = entries.LastOrDefault(row => row.Id == action.Item.Id &&
                    row.Event != "started" && row.Event != "detail");
                if (result is not null) SetActionResult(action, result.ExitCode);
                else
                {
                    action.SetState("failed", "Sem resultado · consulte os registros");
                    await new JsonlLogWriter(_logPath).AppendAsync(new ActionLogEntry(DateTimeOffset.Now,
                        action.Item.Id, action.Item.Source, "Ação sem resultado", 1, "",
                        "O executor terminou antes de registrar a conclusão desta ação."));
                }
            }
        }
        catch (Exception exception)
        {
            foreach (var action in actions.Where(action => action.State is "pending" or "running"))
                action.SetState("failed", "Interrompida · consulte os registros");
            ShowStatus($"Não foi possível concluir a ação: {exception.Message}", true);
            try
            {
                await new JsonlLogWriter(_logPath).AppendAsync(new ActionLogEntry(DateTimeOffset.Now,
                    "batch:error", "GOAT Tweaks", "Falha na execução da ação", 1, "", exception.ToString()));
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
        finally
        {
            _tailTimer.Stop();
            await _logReadTask;
            await RefreshLogsAsync();
            _isApplying = false;
            IntelligencePanel.LegacyExecutionActive = false;
            CanEditQueue = true;
            ApplyButton.Content = "Executar ferramentas legadas";
            var ok = actions.Count(action => action.State == "success");
            var failed = actions.Count(action => action.State == "failed");
            var canceled = actions.Count(action => action.State == "canceled");
            RunSummaryLabel.Text = $"{ok} concluídas · {failed} falharam · {canceled} canceladas";
            CurrentActionLabel.Text = failed > 0 ? "Ação encerrada com falhas" : canceled > 0 ? "Ação cancelada" : "Ação concluída";
            UpdateRunProgress();
            UpdateQueueState();
            ShowStatus(RunSummaryLabel.Text, failed > 0);
        }
    }

    private async Task<bool> RunAdminBatchAsync(IReadOnlyList<QueuedAction> actions)
    {
        var path = Path.Combine(Path.GetTempPath(), $"goat-batch-{Guid.NewGuid():N}.json");
        var bytes = JsonSerializer.SerializeToUtf8Bytes(new ActionBatch { Actions = actions.Select(action => action.Request).ToList() });
        await File.WriteAllBytesAsync(path, bytes);
        var hash = Convert.ToHexString(SHA256.HashData(bytes));
        try
        {
            CurrentActionLabel.Text = "Aguardando autorização do Windows";
            var info = new ProcessStartInfo(Environment.ProcessPath!)
            {
                UseShellExecute = true, Verb = "runas",
                Arguments = $"--runner \"{path}\" \"{_logPath}\" --sha256 {hash}"
            };
            using var process = await Task.Run(() => Process.Start(info))
                ?? throw new InvalidOperationException("Não foi possível abrir o executor.");
            CurrentActionLabel.Text = "Preparando as ações administrativas";
            await process.WaitForExitAsync();
            if (process.ExitCode != 0)
            {
                var writer = new JsonlLogWriter(_logPath!);
                var rows = await Task.Run(() => writer.ReadAllAsync());
                foreach (var action in actions)
                    if (!rows.Any(row => row.Id == action.Item.Id &&
                        row.Event != "started" && row.Event != "detail"))
                        await writer.AppendAsync(new ActionLogEntry(DateTimeOffset.Now, action.Item.Id, action.Item.Source,
                            "Ação não concluída", process.ExitCode, "", "O executor terminou sem concluir esta ação."));
            }
            return true;
        }
        catch (Win32Exception exception) when (exception.NativeErrorCode == 1223)
        {
            foreach (var action in actions)
                await new JsonlLogWriter(_logPath!).AppendAsync(new ActionLogEntry(DateTimeOffset.Now,
                    action.Item.Id, action.Item.Source, "Autorização cancelada", 1223, "", "UAC cancelado"));
            return false;
        }
        finally { File.Delete(path); }
    }

    private async Task RunSingleAsync(QueuedAction action)
    {
        var path = Path.Combine(Path.GetTempPath(), $"goat-action-{Guid.NewGuid():N}.json");
        var bytes = JsonSerializer.SerializeToUtf8Bytes(action.Request);
        await File.WriteAllBytesAsync(path, bytes);
        var hash = Convert.ToHexString(SHA256.HashData(bytes));
        try
        {
            var info = new ProcessStartInfo(Environment.ProcessPath!)
            {
                UseShellExecute = false, CreateNoWindow = true,
                Arguments = $"--action \"{path}\" \"{_logPath}\" --sha256 {hash}"
            };
            using var process = await Task.Run(() => Process.Start(info))
                ?? throw new InvalidOperationException("Não foi possível iniciar a ação.");
            await process.WaitForExitAsync();
        }
        finally { File.Delete(path); }
    }

    private void AddToQueue(CatalogItem item, ActionRequest request)
    {
        if (!CanEditQueue) return;
        try { ActionSafetyPolicy.Validate(item, request); }
        catch (Exception exception) { ShowStatus(exception.Message, true); return; }
        if(_queue.Any(a=>a.State=="pending" && a.Item.Id!=item.Id) &&
            (ActionSafetyPolicy.RequiresManualReview(item) || _queue.Any(a=>a.State=="pending" && ActionSafetyPolicy.RequiresManualReview(a.Item))))
        {ShowStatus("Ferramentas de nível 3 exigem revisão manual e execução isolada. As demais funções continuam disponíveis no catálogo.",true);return;}
        var ids = _queue.Where(action => action.State == "pending" && action.Item.Id != item.Id)
            .Select(action => action.Item.Id).Append(item.Id).ToList();
        if (_conflicts.FindExclusiveViolation(ids) is { } exclusive)
        {
            ShowStatus($"{exclusive.Group.Setting}: adicione esta ação a uma fila vazia para executá-la sozinha.", true);
            return;
        }
        var conflicts = _conflicts.FindUserChoiceConflicts(ids);
        var replaced = conflicts.SelectMany(conflict => conflict.SelectedItems)
            .Where(id => id != item.Id).ToHashSet(StringComparer.Ordinal);
        foreach (var previous in _queue.Where(action => action.Item.Id == item.Id ||
                     action.State == "pending" && replaced.Contains(action.Item.Id)).ToList())
            _queue.Remove(previous);
        _queue.Add(new QueuedAction(item, request)
        {
            DisplayTitle = _workflow.Actions[item.Id].Title,
            Area = _workflow.Find(_workflow.Actions[item.Id].Service)!.Label + " / " + _workflow.Actions[item.Id].Topic
        });
        ShowStatus(replaced.Count > 0
            ? $"Adicionada: {item.Title}. A escolha conflitante anterior foi substituída."
            : $"Ação selecionada: {item.Title}.");
    }

    private static void SetActionResult(QueuedAction action, int code) =>
        action.SetState(code == 0 ? "success" : code == 1223 ? "canceled" : "failed",
            code == 0 ? "Concluída" : code == 1223 ? "Cancelada" : $"Falhou · código {code}");

    private void UpdateRunProgress()
    {
        if (_runActions.Count == 0) return;
        var done = _runActions.Count(action => action.State is "success" or "failed" or "canceled");
        ExecutionProgress.Value = done;
        if (_isApplying)
        {
            QueueHintLabel.Text = $"{done} de {_runActions.Count} ações finalizadas";
            RunSummaryLabel.Text = $"{_runActions.Count - done} restantes";
            Dashboard.QueueSummary = $"Em execução · {done} de {_runActions.Count} finalizadas";
        }
    }

    private void ShowStatus(string text, bool error = false)
    {
        StatusLabel.Text = text;
        StatusLabel.Foreground = (Brush)FindResource(error ? "WarningBrush" : "MutedBrush");
    }

    private static bool CanRevert(CatalogItem item)
    {
        if (item.Source == "RyTuneX" && item.Kind == "toggle")
            return true;
        return item.Revert is not null &&
               (item.Source == "Ultimate" ||
                item.Source == "RyTuneX" && item.Kind == "combo" ||
                item.Source == "winutil" && item.Kind == "tweak" ||
                item.Revert.StartsWith("winutil:", StringComparison.Ordinal));
    }

    private static string RiskLabel(string risk) => risk.ToLowerInvariant() switch
    {
        "high" => "Risco alto",
        "medium" => "Risco médio",
        _ => "Risco baixo"
    };

    private static string PreviewDetails(CatalogItem item)
    {
        var winutilKey = item.Source == "winutil" ? item.Id.Split(':', 3)[2] : "";
        if (item.Source is "Ultimate" or "WinSux")
            return $"powershell.exe -NoProfile -ExecutionPolicy Bypass -File {item.SourceFile}\nSHA-256: {item.Sha256}";
        if (item.Source == "RyTuneX" && item.Kind == "toggle")
            return $"OptimizationOptions.ExecuteToggleActionAsync(\"{item.Tag}\", isOn)";
        if (item.Source == "RyTuneX" && item.Kind == "combo")
            return $"OptimizeSystemHelper.{string.Join(" / ", item.Methods)}";
        if (item.Source == "RyTuneX" && item.Kind == "command")
            return $"{item.Launcher} {item.FileName ?? ""}\n{string.Join(Environment.NewLine, item.Commands)}";
        if (item.Source == "winutil" && item.Kind == "function")
            return $"Dot-source functions + configs; {item.Function}()";
        if (item.Source == "winutil" && item.Kind == "tweak")
            return $"Invoke-WinUtilTweaks -CheckBox {winutilKey}";
        if (item.Source == "winutil" && item.Kind == "feature")
            return $"Invoke-WinUtilFeatureInstall {winutilKey}";
        if (item.Source == "winutil" && item.Kind == "fix")
            return $"Invoke-WPFButtonAction -Button {winutilKey}";
        if (item.Source == "winutil" && item.Kind == "appx")
            return $"Remove-WinUtilAPPX -Name {item.PackageId}";
        if (item.Source == "winutil" && item.Kind == "dns")
            return $"Set-WinUtilDNS -DNSProvider {winutilKey}";
        if (item.Source == "winutil" && item.Kind == "app")
            return $"Install-WinUtilProgramWinget -Action Install -Programs {item.Winget ?? item.Choco}";
        return $"{item.SourceFile} · {item.Kind}";
    }

    private void ClearQueueButton_Click(object sender, RoutedEventArgs e)
    {
        if (!CanEditQueue) return;
        _queue.Clear();
        _runActions = [];
        _runById.Clear();
        ExecutionProgress.Value = 0;
        CurrentActionLabel.Text = "Pronto para executar";
        RunSummaryLabel.Text = "Nenhuma ação selecionada";
        ShowStatus("Seleção limpa.");
    }

    private void OpenLogsFolderButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Directory.CreateDirectory(GoatPaths.Logs);
            Process.Start(new ProcessStartInfo("explorer.exe", $"\"{GoatPaths.Logs}\"") { UseShellExecute = true });
        }
        catch (Exception exception) { ShowStatus($"Não foi possível abrir os registros: {exception.Message}", true); }
    }

    private Task RefreshLogsAsync()
    {
        if (_closed) return Task.CompletedTask;
        if (!_logReadTask.IsCompleted) return _logReadTask;
        return _logReadTask = ReadNewLogsAsync();
    }

    private async Task ReadNewLogsAsync()
    {
        try
        {
            var path = _logPath ?? await Task.Run(() => Directory.Exists(GoatPaths.Logs)
                ? Directory.EnumerateFiles(GoatPaths.Logs, "*.jsonl").OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault()
                : null);
            if (path is null) return;
            var reader = _logReader;
            var rows = await Task.Run(() => reader.ReadNewAsync(path));
            if (_closed || _logPath is not null && !string.Equals(path, _logPath, StringComparison.Ordinal)) return;
            foreach (var row in rows)
            {
                if (_isApplying && row.Id is not null &&
                    _runById.TryGetValue(row.Id, out var action))
                {
                    if (row.Event == "started" && action.State == "pending")
                    {
                        action.SetState("running", "Em execução");
                        CurrentActionLabel.Text = action.DisplayTitle;
                    }
                    else if (row.Event != "detail" && row.Event != "started" && row.ExitCode is { } code)
                        SetActionResult(action, code);
                }
            }
            foreach (var row in rows.TakeLast(80))
            {
                var title = row.Id is not null && _workflow.Actions.TryGetValue(row.Id, out var placement) ? placement.Title : row.Id ?? "GOAT Tweaks";
                var detail = string.IsNullOrWhiteSpace(row.Stderr) ? row.Stdout ?? "" : row.Stderr;
                var state = row.Event == "started" ? "Em execução" : row.Event == "detail" || row.ExitCode is null
                    ? "Informação" : row.ExitCode == 0 ? "Concluída" : row.ExitCode == 1223 ? "Cancelada" : $"Falhou · código {row.ExitCode}";
                var compact = detail.Replace('\r', ' ').Replace('\n', ' ').Trim();
                if (compact.Length > 200) compact = compact[..200] + "…";
                _logRows.Insert(0, new LogRow(title, compact.Length == 0 ? state : $"{state} · {compact}",
                    detail, row.Timestamp.ToLocalTime().ToString("HH:mm:ss")));
                if (_logRows.Count > 80) _logRows.RemoveAt(_logRows.Count - 1);
            }
            UpdateRunProgress();
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    public sealed record LogRow(string Title, string Summary, string Detail, string Time);

    public sealed class QueuedAction(CatalogItem item, ActionRequest request) : INotifyPropertyChanged
    {
        public CatalogItem Item { get; } = item;
        public string DisplayTitle { get; init; } = item.Title;
        public string Area { get; init; } = "";
        public ActionRequest Request { get; } = request;
        public string State { get; private set; } = "pending";
        public string Status { get; private set; } = "Na fila";
        public event PropertyChangedEventHandler? PropertyChanged;

        public void SetState(string state, string status)
        {
            if (State == state && Status == status) return;
            State = state;
            Status = status;
            Notify(nameof(State));
            Notify(nameof(Status));
        }

        private void Notify([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
