using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using GoatTweaks.Core.Execution;
using GoatTweaks.Core.Intelligence;
using GoatTweaks.Windows;
using Microsoft.Win32;

namespace GoatTweaks.Intelligence;

public partial class IntelligenceWorkspace : UserControl
{
    private readonly WindowsDiagnostics _diagnostics = new();
    private readonly SessionStore _store = new(System.IO.Path.Combine(GoatPaths.LocalRoot,"Intelligence","sessions"));
    private readonly ObservableCollection<RecommendationRow> _recommendations = [];
    private readonly List<MetricSample> _liveSamples = [];
    private readonly IReadOnlyList<KnowledgeEntry> _knowledge = KnowledgeBase.LoadAll();
    private ServiceSession _session = new();
    private CancellationTokenSource? _operation;
    private bool _ready;
    private bool _filling;
    private bool _legacyExecution;
    public bool IsBusy { get; private set; }
    public bool IsMutating { get; private set; }
    public bool LegacyExecutionActive { get=>_legacyExecution; set { _legacyExecution=value; UpdateControls(); } }
    public event EventHandler? BusyChanged;
    private string CaptureDirectory => System.IO.Path.Combine(GoatPaths.LocalRoot,"Intelligence","captures",_session.Id.ToString("N"));
    public IntelligenceWorkspace()
    {
        InitializeComponent();
        ProfileBox.ItemsSource=Enum.GetValues<OptimizationProfile>().Select(p=>new ProfileChoice(p,p.Label())).ToArray();
        ProfileBox.SelectedItem=((ProfileChoice[])ProfileBox.ItemsSource).Single(p=>p.Value==OptimizationProfile.Diagnostic);
        GameReferenceBox.ItemsSource=GameReferences.All;GameReferenceBox.SelectedIndex=0;
        RecommendationsList.ItemsSource=_recommendations;
        _ready=true;
        UpdateGameReference();
        Context_Changed(this,new RoutedEventArgs());
        RefreshKnowledge();
        RefreshTools();
        RefreshStage();
        Loaded+=async (_,_)=>await RefreshSessionsAsync();
    }
    public void SetServiceMode(bool guided)
    {
        WorkspaceTitle.Text=guided?"GOAT Service Mode · atendimento guiado":"Entenda. Planeje. Meça.";
        if(guided && _session.Before is null) WorkspaceTabs.SelectedIndex=0;
    }
    public void StopCollection() { if(!IsMutating) _operation?.Cancel(); }
    private GameContext ReadContext()=>new()
    {
        GameReferenceId=(GameReferenceBox.SelectedItem as GameReference)?.Id??"",
        Game=GameBox.Text.Trim(), ProcessName=ProcessBox.Text.Trim(), GameVersion=VersionBox.Text.Trim(), Scenario=ScenarioBox.Text.Trim(),
        Resolution=ResolutionBox.Text.Trim(),GraphicsPreset=GraphicsBox.Text.Trim(),RefreshRate=RefreshBox.Text.Trim(),FpsLimit=LimitBox.Text.Trim(),
        Synchronization=SyncBox.Text.Trim(),ShaderWarmupCompleted=ShadersCheck.IsChecked==true,ConditionsConfirmed=ConditionsCheck.IsChecked==true,
        Symptom=SymptomBox.Text.Trim(),CustomObjectives=ObjectivesBox.Text.Trim()
    };
    private OptimizationProfile SelectedProfile => (ProfileBox.SelectedItem as ProfileChoice)?.Value??OptimizationProfile.Diagnostic;
    private TimeSpan Duration => TimeSpan.FromSeconds(int.Parse((DurationBox.SelectedItem as ComboBoxItem)?.Tag?.ToString()??"12"));
    private string Phase => (PhaseBox.SelectedItem as ComboBoxItem)?.Content?.ToString()??"Antes";
    private void Context_Changed(object sender,RoutedEventArgs e)
    {
        if(!_ready || _filling) return;
        _session.InvalidateApproval();_session.Context=ReadContext();_session.Profile=SelectedProfile;
        UpdateProfileDescription();
        if(_session.Before is not null && _session.Changes.Count==0) PopulateAnalysis();
        UpdateControls();
    }
    private void UpdateProfileDescription()
    {
        ProfileDescription.Text=SelectedProfile switch
        {
            OptimizationProfile.MaximumFps=>"Prioriza oportunidades de desempenho com evidência local. Limites térmicos, proteções e estabilidade são preservados.",
            OptimizationProfile.Competitive=>"Prioriza consistência de quadros e redução de tarefas concorrentes justificáveis. Frametime precisa de medição no jogo.",
            OptimizationProfile.Balanced=>"Prioriza compatibilidade e uso diário. Funções como captura de jogos só mudam com seleção explícita.",
            OptimizationProfile.Diagnostic=>"Somente leitura e recomendações. Nenhuma alteração ou restauração é executada neste perfil.",
            _=>"Os objetivos informam a análise; você escolhe cada ajuste relevante no plano. Não há seleção automática de todos os tweaks."
        };
        ObjectivesBox.IsEnabled=SelectedProfile==OptimizationProfile.Custom;
    }
    private void GameReference_Changed(object sender,SelectionChangedEventArgs e)
    {
        if(!_ready || _filling) return;
        var game=GameReferenceBox.SelectedItem as GameReference;
        _filling=true;
        try
        {
            if(game is {Id.Length:>0}){GameBox.Text=game.Name;ProcessBox.Text=game.Process;}
            ShadersCheck.IsChecked=ConditionsCheck.IsChecked=false;
        }
        finally{_filling=false;}
        UpdateGameReference();Context_Changed(this,new RoutedEventArgs());
    }
    private void UpdateGameReference()
    {
        var game=GameReferenceBox.SelectedItem as GameReference;
        GameBox.IsReadOnly=ProcessBox.IsReadOnly=game is {Id.Length:>0};
        GameReferenceSummary.Text=game is {Id.Length:>0}
            ?game.Requirements+"\n"+game.TestGuide+"\nFonte: "+game.Source+" · revisão "+game.Reviewed+". Requisitos não são previsão de FPS."
            :"Outro jogo permite informar nome e executável livremente. Confira requisitos na fonte oficial; nenhum arquivo do jogo é alterado.";
    }
    private async Task WorkAsync(Func<CancellationToken,Task> work,bool mutating=false)
    {
        if(IsBusy || LegacyExecutionActive) { Status("Aguarde a operação em andamento antes de iniciar outra.",true);return; }
        IsBusy=true;IsMutating=mutating;_operation=new();
        WorkProgress.IsIndeterminate=true;UpdateControls();BusyChanged?.Invoke(this,EventArgs.Empty);
        try { await work(_operation.Token); }
        catch(OperationCanceledException) { Status("Coleta interrompida. Dados incompletos não foram usados como resultado final."); }
        catch(Exception ex)
        {
            Status(ex.Message,true);AppendLog(ex.Message);
            _session.Notes.Add(DateTimeOffset.Now.ToString("g")+" · "+ex.Message);
        }
        finally
        {
            IsBusy=IsMutating=false;_operation.Dispose();_operation=null;
            WorkProgress.IsIndeterminate=false;WorkProgress.Value=1;
            if(_liveSamples.Count>0) TelemetryStatus.Text="Última amostra · "+_liveSamples[^1].At.ToLocalTime().ToString("HH:mm:ss")+" · coleta encerrada";
            UpdateControls();RefreshStage();BusyChanged?.Invoke(this,EventArgs.Empty);
        }
    }
    private IProgress<string> Progress()=>new Progress<string>(message=>{Status(message);AppendLog(message);});
    private IProgress<MetricSample> SampleProgress()=>new Progress<MetricSample>(sample=>ShowSample(sample,true));
    private void Status(string text,bool error=false)
    { WorkspaceStatus.Text=text;WorkspaceStatus.Foreground=(Brush)FindResource(error?"WarningBrush":"MutedBrush"); }
    private void AppendLog(string text)
    {
        ExecutionLog.AppendText($"{DateTimeOffset.Now:HH:mm:ss} · {text}\n");
        if(ExecutionLog.Text.Length>16000) ExecutionLog.Text=ExecutionLog.Text[^12000..];
        ExecutionLog.ScrollToEnd();
    }
    private void UpdateControls()
    {
        if(!_ready) return;
        var idle=!IsBusy && !LegacyExecutionActive;
        IdentificationFields.IsEnabled=idle;
        DiagnoseButton.IsEnabled=idle;
        RecommendationsList.IsEnabled=idle;
        ApproveButton.IsEnabled=idle && _session.Before is not null && _session.Profile!=OptimizationProfile.Diagnostic &&
            !_session.Changes.Any(c=>c.State is "Aplicado e verificado" or "Preparado" or "Revisão necessária") && _recommendations.Any(r=>r.Selected && r.CanAuthorize);
        ExecuteButton.IsEnabled=idle && _session.Profile!=OptimizationProfile.Diagnostic && _session.ApprovedPlanHash is not null && _session.ApprovedPlanHash==_session.PlanHash();
        RestoreButton.IsEnabled=idle && _session.Profile!=OptimizationProfile.Diagnostic && _session.Changes.Any(c=>c.State is "Aplicado e verificado" or "Preparado" or "Revisão necessária");
        ImportButton.IsEnabled=CaptureButton.IsEnabled=ValidateButton.IsEnabled=idle && _session.Before is not null;
        ReportButton.IsEnabled=idle && _session.Before is not null;
        RebootButton.IsEnabled=idle && _session.Before is not null;
        WprButton.IsEnabled=idle && PerformanceTools.FindWpr() is not null;
        WpaButton.IsEnabled=idle && PerformanceTools.FindWpa() is not null;
        CancelButton.IsEnabled=IsBusy && !IsMutating;
        PlanStatus.Text=_session.Profile==OptimizationProfile.Diagnostic ? "Perfil Diagnóstico · leitura e recomendações. Alterações e restaurações ficam bloqueadas."
            :_session.ApprovedPlanHash is not null?"Plano autorizado · somente os ajustes selecionados de nível 2 serão executados."
            :_session.Before is null?"Faça o diagnóstico inicial. Nenhum ajuste vem autorizado por padrão."
            :"Revise cada recomendação e selecione os ajustes de nível 2 que deseja autorizar. Os demais serão ignorados.";
    }
    private void RefreshStage()
    {
        StepsList.ItemsSource=Enum.GetValues<ServiceStage>().Select(stage=>new StepRow(stage,stage.Label(),(Brush)FindResource(stage==_session.Stage?"AccentBrush":"MutedBrush"))).ToArray();
        SessionStatus.Text=$"{_session.Stage.Label()} · {_session.Id.ToString("N")[..8]}";
    }
    private async void Diagnose_Click(object sender,RoutedEventArgs e)=>await WorkAsync(async token=>
    {
        if(_session.Before is not null)
        {
            if(_session.Changes.Any(c=>c.State is "Aplicado e verificado" or "Preparado" or "Revisão necessária"))
                throw new InvalidOperationException("Esse atendimento já possui alterações. Use a validação posterior ou restaure antes de abrir outro plano.");
            await _store.SaveAsync(_session,token);
            _session=new ServiceSession{Context=ReadContext(),Profile=SelectedProfile};
        }
        _session.Context=ReadContext();_session.Profile=SelectedProfile;_session.Stage=ServiceStage.Diagnosis;
        RefreshStage();WorkspaceTabs.SelectedIndex=3;_liveSamples.Clear();DrawChart();
        var duration=Duration;
        _session.Before=await _diagnostics.CaptureAsync(duration,SampleProgress(),Progress(),token);
        _session.Stage=ServiceStage.Plan;_session.InvalidateApproval();
        PopulateAnalysis();await _store.SaveAsync(_session,token);
        TelemetryStatus.Text="Última coleta inicial · "+_session.Before.FinishedAt.ToLocalTime().ToString("HH:mm:ss")+" · atualização encerrada";
        WorkspaceTabs.SelectedIndex=1;
        Status($"Diagnóstico salvo: {_session.Findings.Count(f=>f.Kind==EvidenceKind.Confirmed)} observações confirmadas; {_session.Findings.Count(f=>f.Kind==EvidenceKind.Hypothesis)} hipóteses. Nenhuma alteração aplicada.");
    });
    private void PopulateAnalysis(bool recompute=true)
    {
        if(_session.Before is not { } snapshot) return;
        if(recompute)
        {
            _session.Findings=RecommendationEngine.Diagnose(snapshot,_session.Context);
            _session.Recommendations=RecommendationEngine.Build(snapshot,_session.Context,_session.Profile);
        }
        var ordered=_session.Recommendations.OrderBy(r=>ProfilePriority(r)).ThenBy(r=>r.Title).ToArray();
        _filling=true;
        try { _recommendations.Clear();foreach(var r in ordered) _recommendations.Add(new RecommendationRow(r)); }
        finally { _filling=false; }
        FindingsList.ItemsSource=_session.Findings.Select(f=>new {f.Title,f.Evidence,f.NextTest,Status=f.Kind.Label()}).ToArray();
        MachineSummary.Text=$"{snapshot.Machine.Computer} · {snapshot.Machine.User}\n{snapshot.Machine.Os} · {snapshot.Machine.Architecture}\nCPU: {snapshot.Machine.Cpu}\nGPU: {string.Join(" / ",snapshot.Machine.Gpus)}\nDrivers: {string.Join(" / ",snapshot.Machine.GraphicsDrivers)}\nRAM: {snapshot.Machine.InstalledRamGb:0.##} GB · {snapshot.Machine.RamConfiguration}\nPlaca-mãe: {snapshot.Machine.Board}\nFirmware: {snapshot.Machine.Firmware}\nTipo: {(snapshot.Machine.Notebook==true?"Notebook":snapshot.Machine.Notebook==false?"Desktop":"Não confirmado")}";
        MachineSummary.Text+="\n"+snapshot.Machine.CpuTopology+"\n"+string.Join("\n",snapshot.Machine.MemoryModules)+"\n"+
            string.Join("\n",snapshot.Machine.FirmwareFeatures.Concat(snapshot.Machine.GraphicsFeatures).Select(f=>f.Key+": "+f.Value));
        CompatibilitySummary.Text=snapshot.Machine.Supported?"Sistema elegível para as verificações individuais. Compatibilidade de cada ajuste ainda é conferida antes da escrita.":"Diagnóstico disponível. Alterações automáticas não são habilitadas neste sistema/build; requer Windows 10 22H2 ou Windows 11 compatível.";
        ProcessesList.ItemsSource=snapshot.Processes.Take(15).Select(p=>$"{p.Name} · PID {p.Id} · CPU {(p.CpuPercent.HasValue?p.CpuPercent.Value.ToString("0.0")+"%":"não disponível")} · RAM {p.WorkingSetMb:0} MB").ToArray();
        StartupSummary.Text=snapshot.Startup.Count==0?"Nenhuma entrada Run/pasta de inicialização encontrada nas consultas disponíveis.":string.Join("\n",snapshot.Startup.Take(30).Select(s=>$"{s.Name} · {s.Location}"));
        StorageSummary.Text=string.Join("\n",snapshot.Storage.Select(d=>$"{d.Drive}: {d.FreeGb:0.0} / {d.TotalGb:0.0} GB livres").Concat(snapshot.Machine.PhysicalStorage));
        PowerSummary.Text=$"Energia (snapshot): {(snapshot.Machine.OnAcPower==true?"na tomada":snapshot.Machine.OnAcPower==false?"na bateria":"não disponível")} · plano {snapshot.Settings.GetValueOrDefault(IntelligenceSettings.PowerPlan)?.Data??"não disponível"}";
        RefreshComparisons();UpdateControls();
    }
    private int ProfilePriority(Recommendation r)
    {
        if(_session.Profile==OptimizationProfile.Competitive && r.Id.Contains("memory",StringComparison.Ordinal)) return 0;
        if(_session.Profile is OptimizationProfile.MaximumFps or OptimizationProfile.Competitive && r.Id==IntelligenceSettings.BackgroundCapture) return 1;
        if(_session.Profile==OptimizationProfile.Balanced && r.Id==IntelligenceSettings.PowerPlan) return 0;
        if(_session.Profile==OptimizationProfile.Custom && !string.IsNullOrWhiteSpace(_session.Context.CustomObjectives) &&
            _session.Context.CustomObjectives.Split(' ',StringSplitOptions.RemoveEmptyEntries).Any(term=>r.Title.Contains(term,StringComparison.OrdinalIgnoreCase))) return 0;
        return r.Classification==RecommendationClass.Recommended?2:r.Classification==RecommendationClass.NeedsTest?3:5;
    }
    private void PlanSelection_Changed(object sender,RoutedEventArgs e)
    {
        if(!_ready || _filling) return;
        _session.InvalidateApproval();
        Dispatcher.BeginInvoke(new Action(UpdateControls));
    }
    private async void Approve_Click(object sender,RoutedEventArgs e)=>await WorkAsync(async token=>
    {
        ChangeCoordinator.Approve(_session,_recommendations.Where(r=>r.Selected).Select(r=>r.Definition.Id));
        await _store.SaveAsync(_session,token);Status($"Plano salvo e autorizado: {_session.AuthorizedIds.Count} ajuste(s). A autorização expira em 30 minutos e é invalidada por mudanças no plano.");
    });
    private async void Execute_Click(object sender,RoutedEventArgs e)=>await WorkAsync(async token=>
    {
        var coordinator=new ChangeCoordinator(new WindowsSettingsGateway(_diagnostics),_store);
        _session.Stage=ServiceStage.Execution;RefreshStage();
        await coordinator.ExecuteAsync(_session,Progress(),token);
        RefreshChangeResults();
        RestartStatus.Text=_session.Stage==ServiceStage.Restart?"Há ajustes que exigem reinicialização. Salve o atendimento e retome depois de reiniciar manualmente.":"Nenhum ajuste gerenciado exigiu reinicialização. Reinicie o jogo se necessário e repita o teste nas mesmas condições.";
        PhaseBox.SelectedIndex=1;
        Status($"Execução encerrada: {_session.Changes.Count(c=>c.State=="Aplicado e verificado")} ajuste(s) verificado(s). O desempenho ainda precisa de validação.");
    },mutating:true);
    private async void Restore_Click(object sender,RoutedEventArgs e)=>await WorkAsync(async token=>
    {
        if(_session.Profile==OptimizationProfile.Diagnostic) throw new InvalidOperationException("O perfil Diagnóstico não executa restaurações.");
        await new ChangeCoordinator(new WindowsSettingsGateway(_diagnostics),_store).RestoreAsync(_session,Progress(),token);
        RefreshChangeResults();
        Status("Restauração concluída com verificação por ajuste. Confira o diário para operações que exigem revisão.");
    },mutating:true);
    private async void Restart_Click(object sender,RoutedEventArgs e)=>await WorkAsync(async token=>
    {
        if(_session.Before is null) throw new InvalidOperationException("Não há diagnóstico salvo.");
        var machine=await _diagnostics.ReadMachineAsync(token);
        if(machine.MachineId!=_session.Before.Machine.MachineId || machine.UserId!=_session.Before.Machine.UserId) throw new InvalidOperationException("Atendimento pertence a outra máquina/usuário.");
        var required=_session.Changes.Any(c=>c.State=="Aplicado e verificado" && c.RestartRequired);
        _session.RestartConfirmed=!string.IsNullOrWhiteSpace(machine.BootId) && machine.BootId!=_session.Before.Machine.BootId;
        _session.Stage=required && !_session.RestartConfirmed?ServiceStage.Restart:ServiceStage.Validation;
        _session.InvalidateApproval();await _store.SaveAsync(_session,token);
        RestartStatus.Text=_session.RestartConfirmed?"Uma nova inicialização do Windows foi identificada. Continue a validação.":required?"Reinicialização ainda não identificada. O estado foi salvo; reinicie manualmente e retome em Atendimentos.":"Nenhum ajuste aplicado exige reinicialização. O atendimento está salvo e pode seguir para validação.";
        Status(RestartStatus.Text);
    });
    private async void Validate_Click(object sender,RoutedEventArgs e)=>await WorkAsync(async token=>
    {
        if(_session.Before is null) throw new InvalidOperationException("Faça o diagnóstico inicial primeiro.");
        _session.Stage=ServiceStage.Validation;RefreshStage();
        WorkspaceTabs.SelectedIndex=3;_liveSamples.Clear();
        _session.After=await _diagnostics.CaptureAsync(Duration,SampleProgress(),Progress(),token);
        if(_session.After.Machine.MachineId!=_session.Before.Machine.MachineId || _session.After.Machine.UserId!=_session.Before.Machine.UserId)
        { _session.After=null;throw new InvalidOperationException("A coleta posterior pertence a outra máquina/usuário."); }
        foreach(var record in _session.Changes.Where(c=>c.State=="Aplicado e verificado"))
            foreach(var change in record.Changes)
                if(_session.After.Settings.GetValueOrDefault(change.SettingId)!=change.Desired)
                    _session.Notes.Add("Na validação, a configuração deixou de corresponder ao ajuste aplicado: "+record.Title);
        var newFindings=RecommendationEngine.Diagnose(_session.After,ReadContext()).Where(f=>f.Kind==EvidenceKind.Confirmed && !_session.Findings.Any(old=>old.Id==f.Id)).ToList();
        foreach(var f in newFindings) _session.Notes.Add("Observação nova após as alterações (causalidade não provada): "+f.Title+" · "+f.Evidence);
        _session.Stage=ServiceStage.Report;_session.InvalidateApproval();await _store.SaveAsync(_session,token);
        TelemetryStatus.Text="Última coleta posterior · "+_session.After.FinishedAt.ToLocalTime().ToString("HH:mm:ss")+" · atualização encerrada";
        WorkspaceTabs.SelectedIndex=4;RefreshComparisons();
        Status($"Validação de configurações salva. {newFindings.Count} observação(ões) nova(s). Confira a comparação de desempenho e suas pendências.");
    });
    private async void Monitor_Click(object sender,RoutedEventArgs e)=>await WorkAsync(async token=>
    {
        _liveSamples.Clear();WorkspaceTabs.SelectedIndex=3;
        var sampleProgress=SampleProgress();
        await Task.Run(async()=>
        {
            using var counters=new WindowsPerformanceSampler();using var sensors=new SharedSensorReader();
            var clock=Stopwatch.StartNew();
            while(clock.Elapsed<TimeSpan.FromSeconds(30))
            {await Task.Delay(TimeSpan.FromSeconds(2),token);sampleProgress.Report(sensors.Enrich(counters.Read()));}
        },token);
        TelemetryStatus.Text="Última coleta do monitor · "+DateTimeOffset.Now.ToString("HH:mm:ss")+" · atualização encerrada";
        Status("Monitoramento delimitado encerrado. Essas leituras não substituem um benchmark do jogo.");
    });
    private static string Metric(double? value,string unit)=>value.HasValue?$"{value:0.0} {unit}":"Não disponível";
    private void ShowSample(MetricSample sample,bool live)
    {
        _liveSamples.Add(sample);if(_liveSamples.Count>60) _liveSamples.RemoveAt(0);
        CpuMetric.Text=Metric(sample.CpuPercent,"%");RamMetric.Text=Metric(sample.MemoryPercent,"%");GpuMetric.Text=Metric(sample.Gpu3dPercent,"%");
        DiskMetric.Text=Metric(sample.DiskBytesPerSecond/1048576,"MB/s");DiskLatencyMetric.Text="Latência média · "+Metric(sample.DiskLatencyMs,"ms");
        MemoryMetric.Text="Disponível · "+Metric(sample.AvailableMemoryMb,"MB")+" · páginas lidas/s "+Metric(sample.PagesInputPerSecond,"");
        CpuThreadMetric.Text=sample.CpuThreads.Count>0?"Threads mais ocupadas · "+string.Join(" / ",sample.CpuThreads.OrderByDescending(t=>t.Value).Take(3).Select(t=>$"{t.Key}: {t.Value:0}%")):"Leitura por processador lógico não disponível";
        SensorSummary.Text=$"CPU: {Metric(sample.CpuTemperatureC,"°C")} · {Metric(sample.CpuClockMhz,"MHz")}\nGPU: {Metric(sample.GpuTemperatureC,"°C")} · {Metric(sample.GpuClockMhz,"MHz")}\nFonte: {sample.SensorSource}";
        TelemetryStatus.Text=$"{(live?"Medição em andamento":"Última medição")} · {sample.At.ToLocalTime():HH:mm:ss} · leituras reais a cada 2 segundos";
        DrawChart();
    }
    private void DrawChart()
    {
        if(CpuHistory is null) return;CpuHistory.Children.Clear();
        var width=CpuHistory.ActualWidth;var height=CpuHistory.ActualHeight;if(width<=0||height<=0) return;
        for(var i=1;i<=3;i++) CpuHistory.Children.Add(new Line{X1=0,Y1=height*i/4,X2=width,Y2=height*i/4,Stroke=(Brush)FindResource("BorderBrush"),StrokeThickness=1});
        Polyline? line=null;
        for(var i=0;i<_liveSamples.Count;i++)
        {
            if(_liveSamples[i].CpuPercent is not { } value){line=null;continue;}
            if(line is null){line=new Polyline{Stroke=(Brush)FindResource("AccentBrush"),StrokeThickness=2};CpuHistory.Children.Add(line);}
            line.Points.Add(new Point(i*width/Math.Max(1,_liveSamples.Count-1),height*(1-Math.Clamp(value,0,100)/100)));
        }
    }
    private void CpuHistory_SizeChanged(object sender,SizeChangedEventArgs e)=>DrawChart();
    private async void Import_Click(object sender,RoutedEventArgs e)
    {
        if(IsBusy) return;var dialog=new OpenFileDialog{Filter="CSV PresentMon (*.csv)|*.csv",CheckFileExists=true};if(dialog.ShowDialog()!=true) return;
        await WorkAsync(token=>ImportBenchmarkAsync(dialog.FileName,token));
    }
    private async Task ImportBenchmarkAsync(string path,CancellationToken token)
    {
        if(_session.Before is null) throw new InvalidOperationException("Faça o diagnóstico inicial primeiro.");
        var context=ReadContext();var phase=Phase;var chain=SwapChainBox.Text.Trim();
        var machine=await _diagnostics.ReadMachineAsync(token);
        if(machine.MachineId!=_session.Before.Machine.MachineId || machine.UserId!=_session.Before.Machine.UserId) throw new InvalidOperationException("A medição não pode ser associada ao atendimento de outra máquina/usuário.");
        var run=await Task.Run(()=>BenchmarkImporter.Import(path,phase,machine,context,chain),token);
        _session.Benchmarks.Add(run);await _store.SaveAsync(_session,token);RefreshComparisons();Status("Medição real importada. "+run.Summary);
    }
    private async void Capture_Click(object sender,RoutedEventArgs e)
    {
        if(IsBusy) return;
        var select=new OpenFileDialog{Title="Selecionar PresentMon de console instalado a partir da fonte oficial",Filter="PresentMon (*.exe)|PresentMon*.exe",CheckFileExists=true};
        if(select.ShowDialog()!=true) return;
        await WorkAsync(async token=>
        {
            if(_session.Before is null) throw new InvalidOperationException("Faça o diagnóstico inicial primeiro.");
            if(Phase=="Antes" && _session.Changes.Any(c=>c.State is "Aplicado e verificado" or "Preparado" or "Revisão necessária"))
                throw new InvalidOperationException("O atendimento já possui alterações. Selecione a fase Depois para uma nova captura. Um CSV anterior já existente pode ser importado com sua origem declarada.");
            var path=await PerformanceTools.CapturePresentMonAsync(select.FileName,ReadContext().ProcessName,CaptureDirectory,TimeSpan.FromSeconds(30),Progress(),token);
            await ImportBenchmarkAsync(path,token);
        });
    }
    private void RefreshComparisons()
    {
        BenchmarksList.ItemsSource=_session.Benchmarks.Select(b=>b.Summary).ToArray();
        var comparison=BenchmarkComparer.Compare(_session.Benchmarks.LastOrDefault(b=>b.Phase=="Antes"),_session.Benchmarks.LastOrDefault(b=>b.Phase=="Depois"));
        ComparisonSummary.Text=comparison.Conclusion;
        ComparisonDetails.Text=(comparison.Comparable?$"Δ FPS médio: {comparison.AverageFpsDelta:+0.00;-0.00;0.00}\nΔ 1% low: {comparison.OnePercentLowDelta:+0.00;-0.00;0.00}\nΔ frametime médio: {comparison.MeanFrametimeDeltaMs:+0.00;-0.00;0.00} ms\nΔ p99: {comparison.P99FrametimeDeltaMs:+0.00;-0.00;0.00} ms\n\n":"")+string.Join("\n",comparison.Reasons);
    }
    private async void Report_Click(object sender,RoutedEventArgs e)
    {
        if(IsBusy || _session.Before is null) return;
        var dialog=new SaveFileDialog{FileName="GOAT-atendimento-"+_session.Id.ToString("N")[..8]+".html",Filter="Relatório HTML (*.html)|*.html"};
        if(dialog.ShowDialog()!=true) return;
        await WorkAsync(async token=>
        {
            await _store.SaveAsync(_session,token);await File.WriteAllTextAsync(dialog.FileName,ServiceReport.BuildHtml(_session),new UTF8Encoding(false),token);
            Status("Relatório exportado: "+dialog.FileName);
        });
    }
    private async void Wpr_Click(object sender,RoutedEventArgs e)=>await WorkAsync(async token=>
    {
        var trace=await PerformanceTools.CaptureWprAsync(CaptureDirectory,TimeSpan.FromSeconds(30),Progress(),token);
        _session.Notes.Add("WPR CPU.light coletado: "+System.IO.Path.GetFileName(trace)+". Análise no WPA pendente; não é resultado de FPS.");
        await _store.SaveAsync(_session,token);Status("Rastreio WPR salvo: "+trace);
    });
    private void Wpa_Click(object sender,RoutedEventArgs e)
    {
        if(IsBusy) return;
        var dialog=new OpenFileDialog{Filter="Rastreio Windows (*.etl)|*.etl",CheckFileExists=true};if(dialog.ShowDialog()!=true) return;
        try{PerformanceTools.OpenWpa(dialog.FileName);Status("WPA aberto para análise manual do rastreio selecionado.");}catch(Exception ex){Status(ex.Message,true);}
    }
    private void RefreshTools()
    {ToolsSummary.Text=$"WPR: {(PerformanceTools.FindWpr() is not null?"detectado":"não instalado")} · WPA: {(PerformanceTools.FindWpa() is not null?"detectado":"não instalado")}\nPresentMon: seleção opcional do executável de console instalado pelo técnico. Nenhuma ferramenta externa é baixada automaticamente.";}
    private async Task RefreshSessionsAsync()
    {
        try{var result=await _store.ListAsync();SessionsList.ItemsSource=result.Sessions;if(result.Errors.Count>0) Status("Há registros que não puderam ser lidos: "+string.Join(" | ",result.Errors),true);}
        catch(Exception ex) when(ex is IOException or UnauthorizedAccessException){Status(ex.Message,true);}
    }
    private async void RefreshSessions_Click(object sender,RoutedEventArgs e){if(!IsBusy) await RefreshSessionsAsync();}
    private async void WorkspaceTabs_SelectionChanged(object sender,SelectionChangedEventArgs e)
    {
        if(_ready && ReferenceEquals(e.Source,WorkspaceTabs) && WorkspaceTabs.SelectedIndex==5 && !IsBusy)
            await RefreshSessionsAsync();
    }
    private async void Resume_Click(object sender,RoutedEventArgs e)
    {
        if(IsBusy || SessionsList.SelectedItem is not ServiceSession selected) return;
        await WorkAsync(async token=>
        {
            var session=await _store.LoadAsync(selected.Id,token);session.InvalidateApproval();_session=session;
            FillContext();PopulateAnalysis(recompute:false);RefreshComparisons();RefreshChangeResults();
            _liveSamples.Clear();foreach(var sample in (_session.After??_session.Before)?.Samples??[]) ShowSample(sample,false);
            WorkspaceTabs.SelectedIndex=_session.Before is null?0:2;
            await _store.SaveAsync(_session,token);
            Status("Atendimento retomado. Autorizações anteriores foram invalidadas; backups e medições permanecem. Confira operações com revisão necessária.");
        });
    }
    private void FillContext()
    {
        _filling=true;
        try
        {
            var c=_session.Context;GameBox.Text=c.Game;ProcessBox.Text=c.ProcessName;VersionBox.Text=c.GameVersion;ScenarioBox.Text=c.Scenario;
            ResolutionBox.Text=c.Resolution;GraphicsBox.Text=c.GraphicsPreset;RefreshBox.Text=c.RefreshRate;LimitBox.Text=c.FpsLimit;SyncBox.Text=c.Synchronization;
            SymptomBox.Text=c.Symptom;ObjectivesBox.Text=c.CustomObjectives;ShadersCheck.IsChecked=c.ShaderWarmupCompleted;ConditionsCheck.IsChecked=c.ConditionsConfirmed;
            ProfileBox.SelectedItem=((ProfileChoice[])ProfileBox.ItemsSource).Single(p=>p.Value==_session.Profile);
            GameReferenceBox.SelectedItem=GameReferences.All.FirstOrDefault(g=>g.Id==c.GameReferenceId)??GameReferences.All[0];
        }
        finally{_filling=false;}
        UpdateProfileDescription();
        UpdateGameReference();
    }
    private void RefreshChangeResults()
    {
        foreach(var record in _session.Changes)
            AppendLog(record.Title+" · "+record.State+(record.Error.Length>0?" · "+record.Error:""));
    }
    private async void Save_Click(object sender,RoutedEventArgs e)=>await WorkAsync(async token=>{await _store.SaveAsync(_session,token);await RefreshSessionsAsync();Status("Atendimento salvo com configurações, plano, backups e medições disponíveis.");});
    private async void NewSession_Click(object sender,RoutedEventArgs e)
    {
        if(IsBusy || LegacyExecutionActive){Status("Aguarde a operação atual.",true);return;}
        await WorkAsync(async token=>{
        if(_session.Before is not null) await _store.SaveAsync(_session,token);
        _session=new();FillContext();_recommendations.Clear();FindingsList.ItemsSource=null;ProcessesList.ItemsSource=null;_liveSamples.Clear();DrawChart();
        MachineSummary.Text="Execute uma análise para identificar o computador.";CompatibilitySummary.Text="";ExecutionLog.Clear();
        StartupSummary.Text=StorageSummary.Text=PowerSummary.Text="";
        CpuMetric.Text=RamMetric.Text=GpuMetric.Text=DiskMetric.Text="Não disponível";
        CpuThreadMetric.Text=MemoryMetric.Text=DiskLatencyMetric.Text="";
        SensorSummary.Text="Temperaturas e frequências: não disponíveis até detectar sensores compatíveis.";
        TelemetryStatus.Text="Aguardando coleta. O monitor avançado não permanece ativo em segundo plano.";
        Context_Changed(this,new RoutedEventArgs());RefreshComparisons();RefreshStage();UpdateControls();WorkspaceTabs.SelectedIndex=0;Status("Novo atendimento · perfil Diagnóstico por padrão.");
        });
    }
    private void Step_Click(object sender,RoutedEventArgs e)
    {
        if(sender is Button{Tag:ServiceStage stage}) WorkspaceTabs.SelectedIndex=stage switch
        {ServiceStage.Identification=>0,ServiceStage.Diagnosis=>1,ServiceStage.Plan or ServiceStage.Execution or ServiceStage.Restart=>2,_=>4};
    }
    private void History_Click(object sender,RoutedEventArgs e)=>WorkspaceTabs.SelectedIndex=5;
    private void Plan_Click(object sender,RoutedEventArgs e)=>WorkspaceTabs.SelectedIndex=2;
    private void Stop_Click(object sender,RoutedEventArgs e)=>StopCollection();
    private void KnowledgeSearch_Changed(object sender,TextChangedEventArgs e){if(_ready) RefreshKnowledge();}
    private void RefreshKnowledge()
    {
        var query=KnowledgeSearch.Text.Trim();
        KnowledgeList.ItemsSource=_knowledge.Where(k=>query.Length==0 || (k.Name+" "+k.TechnicalName+" "+k.Category).Contains(query,StringComparison.CurrentCultureIgnoreCase))
            .Select(k=>new KnowledgeRow(k)).ToArray();
    }
    public sealed record ProfileChoice(OptimizationProfile Value,string Name) { public override string ToString()=>Name; }
    public sealed record StepRow(ServiceStage Stage,string Label,Brush Brush) { public override string ToString()=>Label; }
    public sealed class RecommendationRow(Recommendation definition):INotifyPropertyChanged
    {
        public override string ToString()=>Title;
        private bool _selected;
        public Recommendation Definition{get;}=definition;
        public string Title=>Definition.Title;
        public string Classification=>Definition.Classification.Label();
        public string Level=>Definition.LevelLabel;
        public string Evidence=>Definition.Evidence;
        public bool CanAuthorize=>Definition.CanAuthorize;
        public bool Selected{get=>_selected;set{_selected=value;PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(Selected)));}}
        public string Details=>$"Problema: {Definition.Problem}\n\nAjuste: {Definition.Proposal}\n\nBenefício esperado: {Definition.ExpectedBenefit}\n\nImpacto provável: {Definition.ProbableImpact}\n\nConfiança: {Definition.Confidence}\n\nRiscos: {Definition.Risk}\n\nCompatibilidade: {Definition.Compatibility}\n\nPré-requisitos: {Definition.Prerequisites}\n\nReversibilidade: {Definition.Reversibility}\n\nRestauração: {Definition.Restoration}\n\nReinicialização: {(Definition.RestartRequired?"Necessária":"Não prevista; reiniciar jogo pode ser necessário")}";
        public event PropertyChangedEventHandler? PropertyChanged;
    }
    public sealed class KnowledgeRow(KnowledgeEntry entry)
    {
        public override string ToString()=>Name;
        public string Name=>entry.Name;public string Category=>entry.Category;public string Reliability=>entry.Reliability;
        public string Details=>$"Nome técnico: {entry.TechnicalName}\n\nDescrição: {entry.Description}\n\nObjetivo: {entry.Objective}\n\nFontes / justificativa: {string.Join("\n",entry.EvidenceSources)}\n\nCompatibilidade: {entry.Compatibility}\n\nEfeitos adversos: {entry.AdverseEffects}\n\nAplicação: {entry.Application}\n\nReversão: {entry.Restoration}\n\nÚltima validação de benefício: {entry.LastValidated??"Não realizada individualmente"}\nÚltima revisão: {entry.LastReviewed}\nNível de automação: {entry.AutomationLevel}";
    }
}
