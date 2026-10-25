using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using GoatTweaks.Core.Catalog;
using GoatTweaks.Core.Execution;
using GoatTweaks.Hardware;
using GoatTweaks.Workflow;

namespace GoatTweaks;

public partial class MainWindow
{
    public DashboardState Dashboard { get; } = new();
    private readonly ServiceCatalog _workflow;
    private readonly CancellationTokenSource _lifetime = new();
    private readonly SystemUsage _usage = new();
    private readonly Queue<UsageSample> _samples = new();
    private readonly DispatcherTimer _usageTimer = new(DispatcherPriority.Background) { Interval = TimeSpan.FromSeconds(2) };
    private string _activeTopic = "all";
    private string _chartMetric = "cpu";
    private bool _navigating;
    private int _tileColumns;

    private void InitializeDashboard()
    {
        AddNavHeading("GERAL");
        AddNavigation("Home", "Visão geral", "\uE80F");
        AddNavigation("Intelligence", "GOAT Intelligence", "\uE945");
        AddNavigation("Scanner", "GOAT Scanner", "\uE9D9");
        AddNavigation("ServiceMode", "GOAT Service Mode", "\uE8F1");
        AddNavigation("Hardware", "Meu hardware", "\uE950");
        AddNavigation("Queue", "Fila de atendimento", "\uE8FD");
        AddNavigation("Logs", "Atividade e registros", "\uE9D9");
        AddNavHeading("SERVIÇOS POR ÁREA");
        foreach (var service in _workflow.Services)
            AddNavigation(service.Id, service.Label, service.Glyph, service.Number, service.ActionCount);
        ServiceTiles.ItemsSource = _workflow.Services;
        var memory = SystemUsage.ReadMemory();
        if (memory is not null) Dashboard.Hardware = Dashboard.Hardware with { UsableMemory = memory.Total };
        _usageTimer.Tick += (_, _) => SampleUsage();
        IntelligencePanel.BusyChanged += (_, _) =>
        {
            if(IntelligencePanel.IsBusy) _usageTimer.Stop();
            else if(!_closed) _usageTimer.Start();
            CanEditQueue = !_isApplying && !IntelligencePanel.IsBusy;
            UpdateQueueState();
        };
        SourceInitialized += (_, _) => SetDarkTitleBar();
        Loaded += async (_, _) =>
        {
            UpdateShellLayout();
            SampleUsage();
            _usageTimer.Start();
            await Task.WhenAll(RefreshLogsAsync(), RefreshHardwareAsync());
        };
        Closed += (_, _) =>
        {
            _closed = true;
            _lifetime.Cancel();
            _usageTimer.Stop();
            _tailTimer.Stop();
            _searchDebounce.Stop();
            IntelligencePanel.StopCollection();
        };
    }

    private void AddNavHeading(string title) => SectionsList.Items.Add(new ListBoxItem
    {
        IsEnabled = false, Focusable = false, Padding = new Thickness(10, 17, 0, 6),
        Content = new TextBlock { Text = title, FontSize = 9, Foreground = (Brush)FindResource("MutedBrush"), FontWeight = FontWeights.SemiBold }
    });

    private void AddNavigation(string id, string label, string glyph, string? number = null, int? count = null)
    {
        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(27) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var icon = new TextBlock
        {
            Text = number ?? glyph, FontFamily = new FontFamily(number is null ? "Segoe MDL2 Assets" : "Segoe UI"),
            FontSize = number is null ? 15 : 10, VerticalAlignment = VerticalAlignment.Center,
            Foreground = (Brush)FindResource("AccentBrush"), FontWeight = number is null ? FontWeights.Normal : FontWeights.SemiBold
        };
        row.Children.Add(icon);
        var title = new TextBlock { Text = label, FontSize = 11, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
        Grid.SetColumn(title, 1);
        row.Children.Add(title);
        var item = new ListBoxItem
        {
            Content = row, Tag = id, Padding = new Thickness(11, 10, 8, 10),
            ToolTip = count.HasValue ? $"{number} · {label} · {count} ações" : label
        };
        SectionsList.Items.Add(item);
        _sectionItems[id] = item;
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateShellLayout();

    private void UpdateShellLayout()
    {
        if (QueuePanel is null || SearchBox is null || HardwareCardsGrid is null) return;
        var searching = !string.IsNullOrWhiteSpace(SearchBox.Text);
        var home = _activeSection == "Home" && !searching;
        var hardware = _activeSection == "Hardware" && !searching;
        var review = _activeSection == "Queue" && !searching;
        var logs = _activeSection == "Logs" && !searching;
        var intelligence = _activeSection is "Intelligence" or "ServiceMode" or "Scanner" && !searching;
        var library = !home && !hardware && !review && !intelligence;
        var dockQueue = library && !logs && ActualWidth >= 1260;
        SidebarColumn.Width = new GridLength(ActualWidth < 1000 ? 166 : ActualWidth < 1260 ? 195 : 218);
        BrandVersionBadge.Visibility=ActualWidth<1000?Visibility.Collapsed:Visibility.Visible;
        QueueColumn.Width = new GridLength(dockQueue ? 312 : 0);
        SearchColumn.Width = new GridLength(dockQueue ? 210 : ActualWidth < 1080 ? 190 : 260);
        DashboardPanel.Visibility = home ? Visibility.Visible : Visibility.Collapsed;
        HardwarePanel.Visibility = hardware ? Visibility.Visible : Visibility.Collapsed;
        LibraryPanel.Visibility = library ? Visibility.Visible : Visibility.Collapsed;
        IntelligencePanel.Visibility = intelligence ? Visibility.Visible : Visibility.Collapsed;
        QueuePanel.Visibility = review || dockQueue ? Visibility.Visible : Visibility.Collapsed;
        Grid.SetColumn(QueuePanel, review ? 1 : 2);
        Grid.SetRow(QueuePanel, review ? 1 : 0);
        Grid.SetRowSpan(QueuePanel, review ? 1 : 3);
        QueuePanel.Margin = review ? new Thickness(26, 0, 26, 8) : new Thickness(0);
        QueuePanel.CornerRadius = review ? new CornerRadius(12) : new CornerRadius(0);
        QueuePanel.BorderThickness = review ? new Thickness(1) : new Thickness(1, 0, 0, 0);
        var showRecent = ActualHeight >= 710;
        RecentActivityRow.Height = new GridLength(showRecent ? ActualHeight < 820 ? 80 : 125 : 0);
        RecentActivityHeading.Visibility = LogsList.Visibility = showRecent ? Visibility.Visible : Visibility.Collapsed;

        var contentWidth = Math.Max(200, ActualWidth - SidebarColumn.Width.Value - 60);
        HardwareCardsGrid.Columns = contentWidth >= 1020 ? 4 : contentWidth >= 550 ? 2 : 1;
        var stackMonitor = contentWidth < 880;
        GuideColumn.Width = new GridLength(stackMonitor ? 0 : 1, stackMonitor ? GridUnitType.Pixel : GridUnitType.Star);
        Grid.SetColumn(GuideCard, stackMonitor ? 0 : 1);
        Grid.SetRow(GuideCard, stackMonitor ? 1 : 0);
        MonitorCard.Margin = stackMonitor ? new Thickness(0, 0, 0, 15) : new Thickness(0, 0, 16, 0);
        GuideCard.MinHeight = stackMonitor ? 0 : 340;
        var columns = contentWidth >= 970 ? 3 : contentWidth >= 570 ? 2 : 1;
        if (_tileColumns != columns)
        {
            _tileColumns = columns;
            var panel = new FrameworkElementFactory(typeof(UniformGrid));
            panel.SetValue(UniformGrid.ColumnsProperty, columns);
            ServiceTiles.ItemsPanel = new ItemsPanelTemplate(panel);
        }
        if (home) DrawTrend();
    }

    private async void SectionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SectionsList.SelectedItem is not ListBoxItem { Tag: string id }) return;
        _navigating = true;
        _activeSection = id;
        _activeTopic = "all";
        SearchBox.Clear();
        _searchDebounce.Stop();
        OriginFilter.SelectedIndex = 0;
        _navigating = false;
        RenderCards();
        if (id is "Intelligence" or "ServiceMode" or "Scanner")
        {
            IntelligencePanel.SetScannerMode(id == "Scanner");
            IntelligencePanel.SetServiceMode(id == "ServiceMode");
        }
        if (id == "Logs") await RefreshLogsAsync();
    }

    private void NavigateTo(string id)
    {
        var preparing = id == "Prepare";
        if (preparing) id = "repair";
        if (!_sectionItems.TryGetValue(id, out var target)) return;
        if (ReferenceEquals(SectionsList.SelectedItem, target))
        {
            SearchBox.Clear();
            _activeTopic = "all";
            OriginFilter.SelectedIndex = 0;
        }
        else SectionsList.SelectedItem = target;
        if (preparing) _activeTopic = "Pontos de restauração e backup";
        RenderCards();
        if (id == "Home") DashboardPanel.ScrollToTop();
    }
    private void Navigate_Click(object sender, RoutedEventArgs e) { if (sender is Button { Tag: string id }) NavigateTo(id); }
    private void ReviewQueue_Click(object sender, RoutedEventArgs e) => NavigateTo("Queue");
    private void ClearSearch_Click(object sender, RoutedEventArgs e) => SearchBox.Clear();
    private void OriginFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (!_navigating) RenderCards(); }
    private void Topic_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton { Tag: string topic }) return;
        _activeTopic = topic;
        RenderCards();
    }
    private void ResetFilters_Click(object sender, RoutedEventArgs e)
    {
        _navigating = true;
        _activeTopic = "all";
        SearchBox.Clear();
        OriginFilter.SelectedIndex = 0;
        _searchDebounce.Stop();
        _navigating = false;
        RenderCards();
    }

    private void RenderCards()
    {
        if (CardsList is null || TopicsList is null || SearchBox is null || OriginFilter is null) return;
        UpdateShellLayout();
        var query = SearchBox.Text.Trim();
        var searching = query.Length > 0;
        var service = _workflow.Find(_activeSection);
        var showingLogs = _activeSection == "Logs" && !searching;
        SectionEyebrow.Text = searching ? "PESQUISA EM TODAS AS ÁREAS" : service is not null ? "SERVIÇO " + service.Number + " / COMPUTADOR" : "PAINEL DE ATENDIMENTO";
        SectionTitle.Text = searching ? "Pesquisar ações" : service?.Label ?? _activeSection switch
        {
            "Hardware" => "Meu hardware", "Queue" => "Revisar atendimento", "Logs" => "Atividade", "Intelligence" => "GOAT Intelligence", "ServiceMode" => "GOAT Service Mode", "Scanner" => "GOAT Scanner", _ => "Visão geral"
        };
        if (!searching && _activeSection is "Home" or "Hardware" or "Queue" or "Intelligence" or "ServiceMode" or "Scanner") return;
        ServiceHeading.Text = searching ? $"Resultados para “{query}”" : showingLogs ? "Atividade deste computador" : service?.Title ?? "Serviços";
        ServiceDescription.Text = searching ? "Busca por nome, área, assunto e origem. Abra os detalhes para consultar a implementação original."
            : showingLogs ? "Resultados e mensagens recentes. A pasta de registros contém os arquivos completos de cada execução."
            : service?.Description ?? "";
        CardsList.Visibility = showingLogs ? Visibility.Collapsed : Visibility.Visible;
        LogsDetailList.Visibility = showingLogs ? Visibility.Visible : Visibility.Collapsed;
        ModePanel.Visibility = FiltersPanel.Visibility = ResetFiltersButton.Visibility = showingLogs ? Visibility.Collapsed : Visibility.Visible;
        EmptyResultsPanel.Visibility = Visibility.Collapsed;
        if (showingLogs)
        {
            ResultsLabel.Text = "Registros locais · até 80 entradas recentes";
            return;
        }
        var origin = (OriginFilter.SelectedItem as ComboBoxItem)?.Tag as string ?? "all";
        var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var candidates = _catalog.Items.Where(item =>
        {
            var placement = _workflow.Actions[item.Id];
            if (!searching && placement.Service != _activeSection) return false;
            if (origin != "all" && item.Source != origin) return false;
            if (!searching) return true;
            var area = _workflow.Find(placement.Service)!;
            var haystack = string.Join(' ', placement.Title, item.Title, item.Description, item.Id, item.Source, placement.Topic, area.Label, area.Title);
            return terms.All(term => CultureInfo.CurrentCulture.CompareInfo.IndexOf(haystack, term, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0);
        }).ToList();
        TopicsList.Visibility = searching ? Visibility.Collapsed : Visibility.Visible;
        if (!searching)
        {
            var topics = candidates.GroupBy(item => _workflow.Actions[item.Id].Topic)
                .OrderBy(group => TopicRank(group.Key)).ThenBy(group => group.Key).ToArray();
            if (_activeTopic != "all" && !topics.Any(t => t.Key == _activeTopic)) _activeTopic = "all";
            TopicsList.ItemsSource = new[] { new TopicChoice("all", $"Todos · {candidates.Count}", _activeTopic == "all") }
                .Concat(topics.Select(t => new TopicChoice(t.Key, $"{t.Key} · {t.Count()}", _activeTopic == t.Key))).ToList();
        }
        var advanced = AdvancedModeCheck.IsChecked == true;
        var cards = candidates.Where(item => searching || _activeTopic == "all" || _workflow.Actions[item.Id].Topic == _activeTopic)
            .Select(item => new ActionCard(item, _workflow.Actions[item.Id], _workflow.Find(_workflow.Actions[item.Id].Service)!, advanced, searching))
            .OrderBy(card => card.Area.Number).ThenBy(card => TopicRank(card.Placement.Topic)).ThenBy(card => card.Placement.Topic).ThenBy(card => card.Title).ToList();
        var view = new ListCollectionView(cards);
        view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ActionCard.GroupKey)));
        CardsList.ItemsSource = view;
        ResultsLabel.Text = $"{cards.Count} ações · {cards.Count(c => c.IsRestricted)} exigem modo avançado";
        EmptyResultsPanel.Visibility = cards.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private static int TopicRank(string topic) => topic switch
    {
        "Pontos de restauração e backup" or "Arquivos temporários e disco" or "Diagnóstico de CPU e RAM" or "Limpeza e instalação de drivers" or "Drivers do sistema" or "Drivers e adaptadores" => 0,
        "Apps pré-instalados do Windows" or "Memória, cache e compressão" or "NVIDIA, AMD e Intel" or "Armazenamento e NVMe" or "DNS e filtros" or "Integridade do Windows" => 1,
        "Rotina completa WinSux" or "Recuperação e reinstalação" => 9, _ => 5
    };

    public sealed record ActionCard(CatalogItem Item, ServiceAction Placement, ServiceDefinition Area, bool AdvancedEnabled, bool GlobalSearch)
    {
        public string Title => Placement.Title;
        public string Source => Item.Source;
        public string GroupKey => GlobalSearch ? Area.Number + " · " + Area.Label + " / " + Placement.Topic : Placement.Topic;
        public string Operation => Item.Kind == "app" ? "INSTALAR PROGRAMA / FERRAMENTA" : Item.Kind == "appx" ? "REMOVER APLICATIVO DO WINDOWS"
            : Item.ConsoleRequired ? "SCRIPT COM INTERAÇÃO" : Item.Kind == "pipeline" ? "ROTINA COMPLETA / EXECUÇÃO ISOLADA" : "AÇÃO DO SISTEMA";
        public string Description => Item.Kind == "app" ? $"Instala {Item.Title} pelo instalador disponibilizado pela origem. Depois da instalação, abra a ferramenta para realizar o atendimento."
            : Item.Kind == "appx" ? "Remove este aplicativo pré-instalado da forma implementada pela origem. Revise a necessidade do cliente antes de adicioná-lo."
            : System.Net.WebUtility.HtmlDecode(string.IsNullOrWhiteSpace(Item.Description) ? Placement.Topic : Item.Description);
        public string Metadata => $"{RiskLabel(Item.Risk)} · {(Item.RequiresAdmin ? "Administrador" : "Sem elevação")}" + (Item.NeedsInternet ? " · Usa internet" : "");
        public bool CanRevert => MainWindow.CanRevert(Item);
        public bool IsRestricted => ActionSafetyPolicy.RequiresAdvancedMode(Item) && !AdvancedEnabled;
        public bool CanExecute => !IsRestricted;
        public string Restriction => "Modo avançado · " + ActionSafetyPolicy.RestrictionReason(Item);
    }

    private async void RefreshHardware_Click(object sender, RoutedEventArgs e) => await RefreshHardwareAsync();
    private async Task RefreshHardwareAsync()
    {
        if (Dashboard.IsDetecting || _closed) return;
        Dashboard.IsDetecting = true;
        Dashboard.HardwareStatus = "Lendo os dados do computador em segundo plano…";
        try
        {
            var snapshot = await HardwareDiscovery.ReadAsync(Dashboard.Hardware, _lifetime.Token);
            if (_closed) return;
            Dashboard.Hardware = snapshot;
            Dashboard.HardwareStatus = snapshot.FailedQueries > 0
                ? $"Leitura parcial às {snapshot.UpdatedAt:HH:mm} · alguns dados não foram disponibilizados pelo Windows."
                : $"Computador identificado às {snapshot.UpdatedAt:HH:mm} · leitura local";
        }
        catch (OperationCanceledException) { }
        catch (Exception)
        {
            if (!_closed) Dashboard.HardwareStatus = "Não foi possível concluir a identificação. Você pode atualizar a leitura e continuar usando os serviços.";
        }
        finally { Dashboard.IsDetecting = false; }
    }

    private void SampleUsage()
    {
        if (_closed) return;
        var sample = _usage.Read();
        _samples.Enqueue(sample);
        while (_samples.Count > 32 || _samples.Count > 1 && sample.Tick - _samples.Peek().Tick > 60000) _samples.Dequeue();
        Dashboard.CpuPercent = sample.Cpu.HasValue ? $"{sample.Cpu:0}%" : "—";
        Dashboard.RamPercent = sample.Memory is not null ? $"{sample.Memory.Percent:0}%" : "—";
        Dashboard.MemoryUsage = sample.Memory is null ? "Leitura de memória indisponível" : $"{HardwareSnapshot.FormatBytes(sample.Memory.Used)} de {HardwareSnapshot.FormatBytes(sample.Memory.Total)} utilizáveis";
        UpdateChartLabels(sample);
        if (DashboardPanel.Visibility == Visibility.Visible && WindowState != WindowState.Minimized) DrawTrend();
    }
    private void UpdateChartLabels(UsageSample? sample)
    {
        Dashboard.ChartValue = _chartMetric == "ram" ? Dashboard.RamPercent : Dashboard.CpuPercent;
        Dashboard.ChartDetail = _chartMetric == "ram" ? Dashboard.MemoryUsage
            : _usage.MultipleProcessorGroups ? "CPU: leitura indisponível para múltiplos grupos de processadores"
            : sample?.Cpu is null ? "Coletando amostras do processador…" : "Uso total do processador · leitura do Windows";
    }
    private void ChartMetric_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton { Tag: string metric }) return;
        _chartMetric = metric;
        CpuChartButton.IsChecked = metric == "cpu";
        RamChartButton.IsChecked = metric == "ram";
        UpdateChartLabels(_samples.LastOrDefault());
        DrawTrend();
    }
    private void TrendPlot_SizeChanged(object sender, SizeChangedEventArgs e) => DrawTrend();

    private void DrawTrend()
    {
        if (TrendPlot is null || TrendLine is null || TrendPlot.ActualWidth < 2 || TrendPlot.ActualHeight < 2) return;
        var width = TrendPlot.ActualWidth;
        var height = TrendPlot.ActualHeight - 2;
        var grid = new StreamGeometry();
        using (var drawing = grid.Open())
            foreach (var fraction in new[] { 0d, .5d, 1d })
            {
                drawing.BeginFigure(new Point(0, 1 + height * fraction), false, false);
                drawing.LineTo(new Point(width, 1 + height * fraction), true, false);
            }
        grid.Freeze();
        TrendGrid.Data = grid;
        var samples = _samples.ToArray();
        if (samples.Length == 0) return;
        var line = new StreamGeometry();
        var area = new StreamGeometry();
        var segments = new List<List<Point>>();
        List<Point>? current = null;
        var now = samples[^1].Tick;
        foreach (var sample in samples)
        {
            double? value = _chartMetric == "ram" ? sample.Memory?.Percent : sample.Cpu;
            if (!value.HasValue) { current = null; continue; }
            if (current is null) { current = []; segments.Add(current); }
            current.Add(new Point(Math.Clamp(1 - (now - sample.Tick) / 60000d, 0, 1) * width,
                1 + (1 - Math.Clamp(value.Value, 0, 100) / 100d) * height));
        }
        using (var stroke = line.Open())
        using (var fill = area.Open())
        {
            foreach (var points in segments.Where(s => s.Count >= 2))
            {
                stroke.BeginFigure(points[0], false, false);
                stroke.PolyLineTo(points.Skip(1).ToArray(), true, true);
                fill.BeginFigure(new Point(points[0].X, height + 1), true, true);
                fill.PolyLineTo(points, true, true);
                fill.LineTo(new Point(points[^1].X, height + 1), true, false);
            }
        }
        line.Freeze(); area.Freeze();
        TrendLine.Data = line; TrendArea.Data = area;
        var hasHistory = segments.Any(s => s.Count >= 2);
        TrendEmptyLabel.Visibility = hasHistory ? Visibility.Collapsed : Visibility.Visible;
        TrendEmptyLabel.Text = _chartMetric == "cpu" && _usage.MultipleProcessorGroups ? "Leitura de CPU indisponível nesta configuração. A memória continua disponível." : "Coletando as primeiras amostras reais…";
    }

    private void SetDarkTitleBar()
    {
        try
        {
            var dark = 1;
            DwmSetWindowAttribute(new WindowInteropHelper(this).Handle, 20, ref dark, sizeof(int));
        }
        catch (DllNotFoundException) { }
        catch (EntryPointNotFoundException) { }
    }
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int size);
}
