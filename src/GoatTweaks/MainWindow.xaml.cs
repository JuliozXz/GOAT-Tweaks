using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using GoatTweaks.Core.Catalog;
using GoatTweaks.Core.Conflicts;
using GoatTweaks.Core.Execution;
using GoatTweaks.Runner.Logging;

namespace GoatTweaks;

public partial class MainWindow : Window
{
    private readonly CatalogService _catalog = CatalogService.LoadEmbedded();
    private readonly ConflictDetector _conflicts = ConflictDetector.LoadEmbedded();
    private readonly ObservableCollection<QueuedAction> _queue = [];
    private readonly Dictionary<string, ListBoxItem> _sectionItems = new(StringComparer.Ordinal);
    private string _activeSection = CatalogSections.All[0];
    private readonly DispatcherTimer _searchDebounce = new() { Interval = TimeSpan.FromMilliseconds(220) };
    private bool _isApplying;
    private bool _updatingAdvancedMode;
    private (string Path, long Length, DateTime Written)? _logSignature;

    public MainWindow()
    {
        InitializeComponent();
        Width = Math.Max(MinWidth, Math.Min(Width, SystemParameters.WorkArea.Width - 36));
        Height = Math.Max(MinHeight, Math.Min(Height, SystemParameters.WorkArea.Height - 36));
        _searchDebounce.Tick += (_, _) => { _searchDebounce.Stop(); RenderCards(); };
        _queue.CollectionChanged += (_, _) => UpdateQueueState();
        Closed += (_, _) => _searchDebounce.Stop();
        foreach (var section in CatalogSections.All)
        {
            var label = section switch
            {
                "Optimize" => "Otimizar",
                "Privacy" => "Privacidade",
                "Features" => "Recursos",
                "Debloat (AppX)" => "Remover apps (AppX)",
                "Install Apps" => "Instalar apps",
                "Network/DNS" => "Rede/DNS",
                "Repair" => "Reparar",
                "Security" => "Segurança",
                "Power" => "Energia",
                "Advanced" => "Avançado",
                _ => section
            };
            var item = new ListBoxItem { Content = label, Padding = new Thickness(10, 9, 6, 9) };
            SectionsList.Items.Add(item);
            _sectionItems[section] = item;
        }
        SectionsList.Items.Add(new ListBoxItem { Content = "Registros", Padding = new Thickness(10, 9, 6, 9) });
        QueueList.ItemsSource = _queue;
        SectionsList.SelectedItem = _sectionItems[_activeSection];
        RenderCards();
        RefreshLogs();
    }

    private void SectionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SectionsList.SelectedItem is not ListBoxItem selected)
            return;
        SectionTitle.Text = selected.Content?.ToString() ?? "GOAT Tweaks";
        var section = _sectionItems.FirstOrDefault(pair => ReferenceEquals(pair.Value, selected)).Key;
        if (section is not null)
        {
            _activeSection = section;
            RenderCards();
        }
        else
        {
            _activeSection = "Logs";
            RenderCards();
            RefreshLogs();
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _searchDebounce.Stop();
        _searchDebounce.Start();
    }

    private void RenderCards()
    {
        if (CardsList is null || SearchBox is null)
            return;
        var showingLogs = _activeSection == "Logs" && SearchBox.Text.Trim().Length == 0;
        CardsList.Visibility = showingLogs ? Visibility.Collapsed : Visibility.Visible;
        LogsDetailList.Visibility = showingLogs ? Visibility.Visible : Visibility.Collapsed;
        if (showingLogs)
        {
            CardsList.ItemsSource = null;
            ResultsLabel.Text = "Histórico das execuções registradas neste computador";
            return;
        }
        var query = SearchBox.Text.Trim();
        var advanced = AdvancedModeCheck?.IsChecked == true;
        var items = _catalog.Items.Where(item =>
            (query.Length > 0 || item.Section.Equals(_activeSection, StringComparison.Ordinal)) &&
            (query.Length == 0 || item.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
             item.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
             item.Id.Contains(query, StringComparison.OrdinalIgnoreCase)))
            .Select(item => new ActionCard(item, advanced)).ToList();
        CardsList.ItemsSource = items;
        var blocked = items.Count(item => item.IsRestricted);
        ResultsLabel.Text = $"{items.Count} ações · {blocked} bloqueadas no modo atual";
    }

    public sealed record ActionCard(CatalogItem Item, bool AdvancedEnabled)
    {
        public string Title => Item.Title;
        public string Source => Item.Source;
        public string Description => string.IsNullOrWhiteSpace(Item.Description) ? Item.Category : Item.Description;
        public string Metadata => $"{RiskLabel(Item.Risk)} · {(Item.RequiresAdmin ? "Administrador" : "Sem elevação")} · {(CanRevert ? "Restaura o padrão da origem" : "Sem reversão automática")}";
        public bool CanRevert => MainWindow.CanRevert(Item);
        public bool IsRestricted => ActionSafetyPolicy.RequiresAdvancedMode(Item) && !AdvancedEnabled;
        public bool CanExecute => !IsRestricted;
        public string Restriction => ActionSafetyPolicy.RestrictionReason(Item);
    }

    private void UpdateQueueState()
    {
        QueueCountLabel.Text = _queue.Count.ToString();
        ApplyButton.IsEnabled = _queue.Count > 0 && !_isApplying;
    }

    private void AdvancedModeCheck_Changed(object sender, RoutedEventArgs e)
    {
        if (_updatingAdvancedMode || CardsList is null)
            return;
        if (AdvancedModeCheck.IsChecked == true)
        {
            var input = new TextBox { Margin = new Thickness(0, 12, 0, 12) };
            var body = new StackPanel { Margin = new Thickness(22) };
            body.Children.Add(new TextBlock { Text = "Habilitar ações avançadas", FontSize = 20, FontWeight = FontWeights.SemiBold });
            body.Children.Add(new TextBlock
            {
                Text = "Essas ações podem alterar a inicialização, drivers, segurança, serviços ou remover arquivos e aplicativos. Não existe garantia de reversão. Revise a ação e o backup do cliente. Digite AVANCADO para habilitar nesta sessão.",
                TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 12, 0, 0)
            });
            body.Children.Add(input);
            var ok = new Button { Content = "Habilitar nesta sessão", IsDefault = true };
            var cancel = new Button { Content = "Cancelar", IsCancel = true, Margin = new Thickness(8, 0, 0, 0) };
            var buttons = new StackPanel { Orientation = Orientation.Horizontal };
            buttons.Children.Add(ok);
            buttons.Children.Add(cancel);
            body.Children.Add(buttons);
            var dialog = new Window { Title = "GOAT Tweaks", Owner = this, Width = 560, SizeToContent = SizeToContent.Height, WindowStartupLocation = WindowStartupLocation.CenterOwner, Content = body, ResizeMode = ResizeMode.NoResize };
            ok.Click += (_, _) =>
            {
                if (input.Text == "AVANCADO") dialog.DialogResult = true;
                else input.Focus();
            };
            if (dialog.ShowDialog() != true)
            {
                _updatingAdvancedMode = true;
                AdvancedModeCheck.IsChecked = false;
                _updatingAdvancedMode = false;
            }
        }
        if (AdvancedModeCheck.IsChecked != true)
            foreach (var action in _queue.Where(action => ActionSafetyPolicy.RequiresAdvancedMode(action.Item)).ToList())
                _queue.Remove(action);
        ModeLabel.Text = AdvancedModeCheck.IsChecked == true
            ? "Modo avançado · revise cada ação antes de aplicar"
            : "Modo cliente · ações avançadas bloqueadas";
        RenderCards();
    }

    private void RemoveQueueButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_isApplying && sender is Button { Tag: QueuedAction action })
            _queue.Remove(action);
    }

    private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (!_isApplying) return;
        e.Cancel = true;
        MessageBox.Show(this, "Aguarde a conclusão da fila para fechar o aplicativo.", "GOAT Tweaks", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void PreviewButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: CatalogItem item } && await ShowPreviewAsync(item, false) is { } request)
            AddToQueue(item, request);
    }

    private async void RevertButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: CatalogItem item })
            return;
        if (item.Revert is { } id && _catalog.ById.TryGetValue(id, out var revertItem))
        {
            if (await ShowPreviewAsync(revertItem, false) is { } reverseRequest)
                AddToQueue(revertItem, reverseRequest);
        }
        else if (await ShowPreviewAsync(item, true) is { } request)
            AddToQueue(item, request);
    }

    private async Task<ActionRequest?> ShowPreviewAsync(CatalogItem item, bool revert)
    {
        if (_isApplying) return null;
        if (ActionSafetyPolicy.RequiresAdvancedMode(item) && AdvancedModeCheck.IsChecked != true)
        {
            MessageBox.Show(this, ActionSafetyPolicy.RestrictionReason(item), "Ação bloqueada", MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }
        var fields = new List<(string Prompt, TextBox Input)>();
        var body = new StackPanel { Margin = new Thickness(18) };
        body.Children.Add(new TextBlock { Text = item.Title, FontSize = 20, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
        body.Children.Add(new TextBlock { Text = item.Description, Margin = new Thickness(0, 8, 0, 12), TextWrapping = TextWrapping.Wrap });
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
        TextBox? confirmation = null;
        if (ActionSafetyPolicy.RequiresConfirmation(item))
        {
            body.Children.Add(new TextBlock { Text = "Digite CONFIRMAR para continuar.", Margin = new Thickness(0, 10, 0, 2), Foreground = (Brush)FindResource("WarningBrush") });
            confirmation = new TextBox();
            body.Children.Add(confirmation);
        }
        TextBox? winSuxConfirmation = null;
        if (item.Source == "WinSux")
        {
            body.Children.Add(new TextBlock
            {
                Text = "O fluxo reinicia em Modo de Segurança, desativa Defender/UAC/VBS, remove a maioria dos apps UWP e não possui reversão automática. Digite WINSUX.",
                Margin = new Thickness(0, 10, 0, 2), TextWrapping = TextWrapping.Wrap, Foreground = (Brush)FindResource("WarningBrush")
            });
            winSuxConfirmation = new TextBox();
            body.Children.Add(winSuxConfirmation);
        }
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 14, 0, 0) };
        var ok = new Button { Content = "Adicionar à fila", IsDefault = true, Padding = new Thickness(10, 5, 10, 5) };
        var cancel = new Button { Content = "Cancelar", IsCancel = true, Padding = new Thickness(10, 5, 10, 5), Margin = new Thickness(7, 0, 0, 0) };
        buttons.Children.Add(ok);
        buttons.Children.Add(cancel);
        body.Children.Add(buttons);
        var window = new Window { Title = "Prévia da ação", Owner = this, Width = 620, Height = 650, MinHeight = 440, WindowStartupLocation = WindowStartupLocation.CenterOwner, Content = new ScrollViewer { Content = body, VerticalScrollBarVisibility = ScrollBarVisibility.Auto } };
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
                if (confirmation is not null && confirmation.Text != "CONFIRMAR")
                    throw new ArgumentException("Confirmação digitada incorretamente.");
                if (winSuxConfirmation is not null && winSuxConfirmation.Text != "WINSUX")
                    throw new ArgumentException("Confirmação WINSUX digitada incorretamente.");
                request = new ActionRequest
                {
                    ItemId = item.Id,
                    OptionKey = (optionBox?.SelectedItem as CatalogOption)?.Key,
                    Inputs = answers,
                    Revert = revert,
                    ConsoleWindow = item.ConsoleRequired || console?.IsChecked == true,
                    Enabled = enabled?.IsChecked,
                    AdvancedModeEnabled = AdvancedModeCheck.IsChecked == true,
                    Confirmation = confirmation?.Text,
                    WinSuxConfirmation = winSuxConfirmation?.Text
                };
                window.DialogResult = true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(window, exception.Message, "Entrada inválida", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        };
        return window.ShowDialog() == true ? request : null;
    }

    private async void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        DispatcherTimer? tailTimer = null;
        var executionStarted = false;
        try
        {
            if (_queue.Count == 0)
                return;
            _isApplying = true;
            LibraryPanel.IsEnabled = false;
            SectionsList.IsEnabled = false;
            SearchBox.IsEnabled = false;
            QueueList.IsEnabled = false;
            ClearQueueButton.IsEnabled = false;
            ExecutionProgress.Visibility = Visibility.Visible;
            StatusLabel.Text = $"Aplicando {_queue.Count} ações. Aguarde a conclusão.";
            ApplyButton.IsEnabled = false;
            foreach (var action in _queue) ActionSafetyPolicy.Validate(action.Item, action.Request);
            _logPath = GoatPaths.NewLogPath();
            var ids = _queue.Select(action => action.Request.ItemId).ToList();
            var exclusive = _conflicts.FindExclusiveViolation(ids);
            if (exclusive is not null)
            {
                MessageBox.Show(this, $"“{exclusive.Group.Setting}” exige que {string.Join(", ", exclusive.SelectedItems)} seja executado sozinho.\n{exclusive.Group.Note}",
                    "Conflito exclusivo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var userChoices = _conflicts.FindUserChoiceConflicts(ids);
            if (userChoices.Count > 0)
            {
                var kept = ResolveUserChoices(userChoices);
                if (kept is null)
                    return;
                foreach (var violation in userChoices)
                {
                    var chosen = kept[violation.Group.Id];
                    var dropped = violation.SelectedItems.Where(id => id != chosen).ToArray();
                    foreach (var id in dropped)
                        for (var index = _queue.Count - 1; index >= 0; index--)
                            if (_queue[index].Request.ItemId == id)
                                _queue.RemoveAt(index);
                    await new JsonlLogWriter(_logPath ??= GoatPaths.NewLogPath()).AppendAsync(new ActionLogEntry(
                        DateTimeOffset.Now, $"conflict-resolution:{violation.Group.Id}", "GOAT Tweaks",
                        $"setting={violation.Group.Setting}; kept={chosen}; dropped={string.Join(",", dropped)}",
                        0, "", violation.Group.Note));
                }
            }

            _logPath ??= GoatPaths.NewLogPath();
            var runStarted = DateTimeOffset.Now;
            executionStarted = true;
            tailTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            tailTimer.Tick += (_, _) => RefreshLogs();
            tailTimer.Start();
            if (_queue.Any(action => action.Item.RequiresAdmin))
                await RunAdminBatchAsync();
            foreach (var action in _queue.Where(action => !action.Item.RequiresAdmin).ToList())
            {
                try { await RunSingleAsync(action, elevated: false); }
                catch (Exception exception)
                {
                    await new JsonlLogWriter(_logPath).AppendAsync(new ActionLogEntry(DateTimeOffset.Now,
                        action.Item.Id, action.Item.Source, "Falha ao iniciar a ação", 1, "", exception.Message));
                }
            }

            var entries = await new JsonlLogWriter(_logPath).ReadAllAsync();
            var runIds = _queue.Select(action => action.Request.ItemId).ToHashSet(StringComparer.Ordinal);
            var results = entries.Where(entry => runIds.Contains(entry.Id) && entry.Timestamp >= runStarted).ToList();
            var okCount = results.Count(entry => entry.ExitCode == 0);
            var canceledCount = results.Count(entry => entry.ExitCode == 1223);
            var failedCount = results.Count(entry => entry.ExitCode != 0 && entry.ExitCode != 1223);
            _queue.Clear();
            MessageBox.Show(this, $"{okCount} ok, {failedCount} falharam, {canceledCount} canceladas",
                "Resumo da execução", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, exception.Message, "Erro ao aplicar ações", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            tailTimer?.Stop();
            if (executionStarted)
                _queue.Clear();
            _isApplying = false;
            LibraryPanel.IsEnabled = true;
            SectionsList.IsEnabled = true;
            SearchBox.IsEnabled = true;
            QueueList.IsEnabled = true;
            ClearQueueButton.IsEnabled = true;
            ExecutionProgress.Visibility = Visibility.Collapsed;
            StatusLabel.Text = executionStarted ? "Execução encerrada · consulte os registros" : "Pronto · fila aguardando revisão";
            UpdateQueueState();
            RefreshLogs();
        }
    }

    private string? _logPath;

    private Dictionary<string, string>? ResolveUserChoices(IReadOnlyList<ConflictViolation> violations)
    {
        var body = new StackPanel { Margin = new Thickness(16) };
        var selects = new Dictionary<string, ComboBox>(StringComparer.Ordinal);
        foreach (var violation in violations)
        {
            body.Children.Add(new TextBlock { Text = $"{violation.Group.Setting}\n{violation.Group.Note}", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 4) });
            if (violation.Group.Values.Count > 0)
                body.Children.Add(new TextBlock { Text = string.Join(" · ", violation.Group.Values), TextWrapping = TextWrapping.Wrap });
            var combo = new ComboBox { Margin = new Thickness(0, 0, 0, 8) };
            foreach (var id in violation.SelectedItems)
                combo.Items.Add(new ComboBoxItem
                {
                    Content = _catalog.ById.TryGetValue(id, out var item) ? $"{item.Title} ({item.Source})" : id,
                    Tag = id
                });
            combo.SelectedIndex = 0;
            body.Children.Add(combo);
            selects[violation.Group.Id] = combo;
        }
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var ok = new Button { Content = "Manter seleção", IsDefault = true, Padding = new Thickness(10, 5, 10, 5) };
        var cancel = new Button { Content = "Cancelar", IsCancel = true, Margin = new Thickness(6, 0, 0, 0), Padding = new Thickness(10, 5, 10, 5) };
        buttons.Children.Add(ok);
        buttons.Children.Add(cancel);
        body.Children.Add(buttons);
        var window = new Window { Title = "Resolver escolhas conflitantes", Owner = this, Width = 600, Height = 520, WindowStartupLocation = WindowStartupLocation.CenterOwner, Content = new ScrollViewer { Content = body } };
        Dictionary<string, string>? result = null;
        ok.Click += (_, _) =>
        {
            result = selects.ToDictionary(pair => pair.Key,
                pair => (string)((ComboBoxItem)pair.Value.SelectedItem).Tag, StringComparer.Ordinal);
            window.DialogResult = true;
        };
        return window.ShowDialog() == true ? result : null;
    }

    private async Task RunAdminBatchAsync()
    {
        _logPath ??= GoatPaths.NewLogPath();
        var path = Path.Combine(Path.GetTempPath(), $"goat-batch-{Guid.NewGuid():N}.json");
        var bytes = JsonSerializer.SerializeToUtf8Bytes(new ActionBatch
        {
            Actions = _queue.Where(action => action.Item.RequiresAdmin).Select(action => action.Request).ToList()
        });
        await File.WriteAllBytesAsync(path, bytes);
        var hash = Convert.ToHexString(SHA256.HashData(bytes));
        var started = DateTimeOffset.Now;
        try
        {
            var info = new ProcessStartInfo(Environment.ProcessPath!)
            {
                UseShellExecute = true,
                Verb = "runas",
                Arguments = $"--runner \"{path}\" \"{_logPath}\" --sha256 {hash}"
            };
            using var process = Process.Start(info) ?? throw new InvalidOperationException("Não foi possível abrir o Runner elevado.");
            await process.WaitForExitAsync();
            if (process.ExitCode != 0)
            {
                var writer = new JsonlLogWriter(_logPath);
                var rows = await writer.ReadAllAsync();
                foreach (var action in _queue.Where(action => action.Item.RequiresAdmin))
                    if (!rows.Any(row => row.Id == action.Item.Id && row.Timestamp >= started))
                        await writer.AppendAsync(new ActionLogEntry(DateTimeOffset.Now, action.Item.Id,
                            action.Item.Source, "Ação não concluída", process.ExitCode, "",
                            "O executor terminou sem concluir esta ação. Consulte os registros."));
            }
        }
        catch (System.ComponentModel.Win32Exception exception) when (exception.NativeErrorCode == 1223)
        {
            var writer = new JsonlLogWriter(_logPath!);
            foreach (var action in _queue.Where(action => action.Item.RequiresAdmin))
                await writer.AppendAsync(new ActionLogEntry(DateTimeOffset.Now, action.Item.Id, action.Item.Source,
                    "UAC cancelado", 1223, "", "UAC cancelado"));
        }
        finally
        {
            File.Delete(path);
        }
    }

    private async Task RunSingleAsync(QueuedAction action, bool elevated)
    {
        _logPath ??= GoatPaths.NewLogPath();
        var requestPath = Path.Combine(Path.GetTempPath(), $"goat-action-{Guid.NewGuid():N}.json");
        var bytes = JsonSerializer.SerializeToUtf8Bytes(action.Request);
        await File.WriteAllBytesAsync(requestPath, bytes);
        var hash = Convert.ToHexString(SHA256.HashData(bytes));
        try
        {
            var info = new ProcessStartInfo(Environment.ProcessPath!)
            {
                UseShellExecute = elevated,
                Verb = elevated ? "runas" : "",
                Arguments = $"--action \"{requestPath}\" \"{_logPath}\" --sha256 {hash}"
            };
            using var process = Process.Start(info) ?? throw new InvalidOperationException("Não foi possível iniciar o Runner.");
            await process.WaitForExitAsync();
        }
        finally
        {
            File.Delete(requestPath);
        }
    }

    private void AddToQueue(CatalogItem item, ActionRequest request)
    {
        if (_isApplying) return;
        foreach (var previous in _queue.Where(action => action.Item.Id == item.Id).ToList())
            _queue.Remove(previous);
        _queue.Add(new QueuedAction(item, request));
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

    private void ClearQueueButton_Click(object sender, RoutedEventArgs e) { if (!_isApplying) _queue.Clear(); }

    private void OpenLogsFolderButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Directory.CreateDirectory(GoatPaths.Logs);
            Process.Start(new ProcessStartInfo("explorer.exe", $"\"{GoatPaths.Logs}\"") { UseShellExecute = true });
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, exception.Message, "Não foi possível abrir os registros", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RefreshLogs()
    {
        if (LogsList is null) return;
        try
        {
            var path = _logPath;
            if (path is null && Directory.Exists(GoatPaths.Logs))
                path = Directory.GetFiles(GoatPaths.Logs, "*.jsonl")
                    .OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();
            if (path is null || !File.Exists(path)) return;
            var file = new FileInfo(path);
            var signature = (path, file.Length, file.LastWriteTimeUtc);
            if (_logSignature == signature) return;
            _logSignature = signature;
            var lines = new List<string>();
            foreach (var line in File.ReadLines(path).TakeLast(60))
            {
                try
                {
                    using var document = JsonDocument.Parse(line);
                    var row = document.RootElement;
                    var id = row.TryGetProperty("id", out var idValue) ? idValue.ToString() : "";
                    var code = row.TryGetProperty("exitCode", out var exitValue) ? exitValue.ToString() : "";
                    var text = row.TryGetProperty("stderr", out var errValue) ? errValue.ToString() : "";
                    if (string.IsNullOrWhiteSpace(text) && row.TryGetProperty("stdout", out var outputValue)) text = outputValue.ToString();
                    lines.Add($"{id} · código {code} · {text}");
                }
                catch (JsonException) { }
            }
            LogsList.ItemsSource = lines;
            LogsDetailList.ItemsSource = lines;
        }
        catch (IOException) { _logSignature = null; }
        catch (UnauthorizedAccessException) { _logSignature = null; }
    }

    public sealed record QueuedAction(CatalogItem Item, ActionRequest Request)
    {
        public override string ToString() => $"{Item.Title} ({Item.Source})";
    }
}
