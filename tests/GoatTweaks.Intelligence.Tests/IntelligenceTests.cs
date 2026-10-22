using System.Globalization;
using System.Text;
using System.Text.Json;
using GoatTweaks.Core.Catalog;
using GoatTweaks.Core.Intelligence;
using Xunit;

namespace GoatTweaks.Intelligence.Tests;

public sealed class IntelligenceTests
{
    private static MachineFacts Machine()=>new(){MachineId="test-machine",UserId="test-user",Os="Windows 11",OsBuild=26100,IsWindowsClient=true,Architecture="X64",Cpu="Fixture CPU",Gpus=["Fixture GPU"],GraphicsDrivers=["1"],InstalledRamGb=16,Notebook=false,OnAcPower=true,AvailablePowerPlans=[IntelligenceSettings.BalancedGuid,IntelligenceSettings.PowerSaverGuid]};
    private static GameContext Context()=>new(){Game="Unit test fixture",ProcessName="fixture.exe",GameVersion="1",Scenario="Repeatable",Resolution="1920x1080",GraphicsPreset="High DX12",RefreshRate="144 Hz",FpsLimit="Unlimited",Synchronization="Off",ShaderWarmupCompleted=true,ConditionsConfirmed=true};
    private static DiagnosticSnapshot Snapshot()=>new(){Machine=Machine(),FinishedAt=DateTimeOffset.UtcNow,Settings=new(){[IntelligenceSettings.GameMode]=SettingValue.Dword(0),[IntelligenceSettings.BackgroundCapture]=SettingValue.Dword(1),[IntelligenceSettings.PowerPlan]=SettingValue.PowerPlan(IntelligenceSettings.PowerSaverGuid)}};
    private static ServiceSession Session(OptimizationProfile profile=OptimizationProfile.Balanced)
    {var snapshot=Snapshot();return new(){Before=snapshot,Profile=profile,Context=Context(),Recommendations=RecommendationEngine.Build(snapshot,Context(),profile)};}

    [Fact] public void EveryOriginalActionHasAnExplicitKnowledgeInventoryEntry()
    {
        var original=CatalogService.LoadEmbedded().Items;var knowledge=KnowledgeBase.LoadAll().Where(k=>k.Id.StartsWith("legacy:",StringComparison.Ordinal)).ToArray();
        Assert.Equal(619,original.Count);Assert.Equal(original.Select(i=>"legacy:"+i.Id).Order(),knowledge.Select(k=>k.Id).Order());
        Assert.All(knowledge,k=>{Assert.Equal(3,k.AutomationLevel);Assert.Null(k.LastValidated);Assert.NotEmpty(k.Restoration);});
    }
    [Fact] public void MissingRegistryValueDoesNotMeanDisabled()
    {
        var s=Snapshot() with{Settings=new(){[IntelligenceSettings.GameMode]=SettingValue.Missing}};
        var r=RecommendationEngine.Build(s,Context(),OptimizationProfile.MaximumFps).Single(r=>r.Id==IntelligenceSettings.GameMode);
        Assert.Empty(r.Changes);Assert.False(r.CanAuthorize);
    }
    [Theory][InlineData(true,true)][InlineData(null,true)][InlineData(false,false)][InlineData(false,null)]
    public void PowerRecommendationsPreserveNotebookAndUnknownPowerLimits(bool? notebook,bool? ac)
    {
        var s=Snapshot() with{Machine=Machine() with{Notebook=notebook,OnAcPower=ac}};
        Assert.False(RecommendationEngine.Build(s,Context(),OptimizationProfile.MaximumFps).Single(r=>r.Id==IntelligenceSettings.PowerPlan).CanAuthorize);
    }
    [Theory][InlineData(false,26100)][InlineData(true,19044)]
    public void UnsupportedSystemsNeverHaveAnAutomaticPlan(bool client,int build)
    {
        var s=Snapshot() with{Machine=Machine() with{IsWindowsClient=client,OsBuild=build}};
        Assert.DoesNotContain(RecommendationEngine.Build(s,Context(),OptimizationProfile.MaximumFps),r=>r.CanAuthorize);
    }
    [Fact] public async Task DiagnosticModeCannotAuthorizeExecuteOrRestore()
    {
        var s=Session(OptimizationProfile.Diagnostic);var gateway=new FakeSettings(s.Before!);var c=new ChangeCoordinator(gateway,new MemoryJournal());
        Assert.Throws<InvalidOperationException>(()=>ChangeCoordinator.Approve(s,[IntelligenceSettings.GameMode]));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>c.ExecuteAsync(s,null,default));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>c.RestoreAsync(s,null,default));Assert.Empty(gateway.Writes);
    }
    [Fact] public void UtilizationAloneDoesNotConfirmBottlenecks()
    {
        var s=Snapshot() with{Samples=Enumerable.Range(0,6).Select(_=>new MetricSample{CpuPercent=100,Gpu3dPercent=99,CpuThreads=new(){["0,0"]=100}}).ToList()};
        var findings=RecommendationEngine.Diagnose(s,Context());
        Assert.Equal(EvidenceKind.Hypothesis,findings.Single(f=>f.Id=="cpu.thread").Kind);
        Assert.Equal(EvidenceKind.Hypothesis,findings.Single(f=>f.Id=="gpu.load").Kind);
    }
    [Fact] public void HighTemperatureDoesNotInventAThermalThrottleSignal()
    {var findings=RecommendationEngine.Diagnose(Snapshot() with{Samples=[new(){CpuTemperatureC=95}]},Context());Assert.Equal(EvidenceKind.Hypothesis,findings.Single(f=>f.Id=="thermal").Kind);}
    [Fact] public void GameAndProtectionProcessesAreNotSuggestedForClosure()
    {
        var s=Snapshot() with{Processes=[new(1,"fixture",90,4000),new(2,"System",20,50),new(3,"GOATTweaks",15,50)]};
        Assert.DoesNotContain(RecommendationEngine.Diagnose(s,Context()),f=>f.Id.StartsWith("process.",StringComparison.Ordinal));
    }
    [Fact] public async Task DurableBackupExistsBeforeTheFirstWrite()
    {
        var s=Session();var journal=new MemoryJournal();var gateway=new FakeSettings(s.Before!);
        gateway.BeforeWrite=()=>Assert.Contains(journal.Snapshots[^1].Changes,c=>c.State=="Preparado" && c.Changes[0].Before==SettingValue.Dword(0));
        ChangeCoordinator.Approve(s,[IntelligenceSettings.GameMode]);await new ChangeCoordinator(gateway,journal).ExecuteAsync(s,null,default);
        Assert.Equal("Aplicado e verificado",Assert.Single(s.Changes).State);Assert.Equal(SettingValue.Dword(1),gateway.Values[IntelligenceSettings.GameMode]);
    }
    [Fact] public async Task AChangedPlanFailsClosedBeforeWriting()
    {
        var s=Session();var gateway=new FakeSettings(s.Before!);ChangeCoordinator.Approve(s,[IntelligenceSettings.GameMode]);s.Context=s.Context with{Resolution="2560x1440"};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>new ChangeCoordinator(gateway,new MemoryJournal()).ExecuteAsync(s,null,default));Assert.Empty(gateway.Writes);
    }
    [Fact] public async Task AnExpiredApprovalFailsClosed()
    {
        var s=Session();var gateway=new FakeSettings(s.Before!);ChangeCoordinator.Approve(s,[IntelligenceSettings.GameMode]);s.ApprovedAt=DateTimeOffset.UtcNow.AddHours(-1);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>new ChangeCoordinator(gateway,new MemoryJournal()).ExecuteAsync(s,null,default));Assert.Empty(gateway.Writes);
    }
    [Fact] public async Task ChangedHardwareOrDriverRequiresANewDiagnosis()
    {
        var s=Session();var gateway=new FakeSettings(s.Before!){Machine=Machine() with{GraphicsDrivers=["2"]}};ChangeCoordinator.Approve(s,[IntelligenceSettings.GameMode]);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>new ChangeCoordinator(gateway,new MemoryJournal()).ExecuteAsync(s,null,default));Assert.Empty(gateway.Writes);
    }
    [Fact] public async Task ChangedSettingIsSkippedWithoutClobberingExternalState()
    {
        var s=Session();var gateway=new FakeSettings(s.Before!);gateway.Values[IntelligenceSettings.GameMode]=SettingValue.Dword(2);
        ChangeCoordinator.Approve(s,[IntelligenceSettings.GameMode]);await new ChangeCoordinator(gateway,new MemoryJournal()).ExecuteAsync(s,null,default);
        Assert.Empty(gateway.Writes);Assert.StartsWith("Ignorado",Assert.Single(s.Changes).State);
    }
    [Fact] public async Task JournalFailureStopsBeforeAnyWrite()
    {
        var s=Session();var gateway=new FakeSettings(s.Before!);ChangeCoordinator.Approve(s,[IntelligenceSettings.GameMode]);
        await Assert.ThrowsAsync<IOException>(()=>new ChangeCoordinator(gateway,new MemoryJournal{FailOnSave=2}).ExecuteAsync(s,null,default));Assert.Empty(gateway.Writes);
    }
    [Fact] public async Task InterruptedPreparedPlanCannotBeReusedWithoutReview()
    {
        var s=Session();var gateway=new FakeSettings(s.Before!);ChangeCoordinator.Approve(s,[IntelligenceSettings.GameMode]);
        var coordinator=new ChangeCoordinator(gateway,new MemoryJournal{FailOnSave=2});
        await Assert.ThrowsAsync<IOException>(()=>coordinator.ExecuteAsync(s,null,default));
        Assert.Null(s.ApprovedPlanHash);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>coordinator.ExecuteAsync(s,null,default));
        Assert.Empty(gateway.Writes);
    }
    [Fact] public void HardwareGuidanceUsesDetectedVendorsAndPreservesNotebookDrivers()
    {
        var guidance=HardwareGuidance.DriverReview(Machine() with{Cpu="AMD Ryzen 7",Gpus=["AMD Radeon", "NVIDIA GeForce"],Notebook=true,ComputerModel="Fixture Notebook"});
        Assert.Contains("Notebook Fixture Notebook",guidance);Assert.Contains("Chipset AMD",guidance);Assert.Contains("NVIDIA:",guidance);Assert.Contains("AMD Graphics:",guidance);
        Assert.DoesNotContain("Intel Graphics:",guidance);Assert.Contains("não afirma que estejam desatualizadas",guidance);
    }
    [Fact] public void ReportedDiskHealthIsAnObservationAndHddPresenceIsNotABottleneck()
    {
        var snapshot=Snapshot() with{Machine=Machine() with{PhysicalDisks=[new("Fixture HD","HD",1)]}};
        var findings=RecommendationEngine.Diagnose(snapshot,Context());
        Assert.Equal(EvidenceKind.Confirmed,findings.Single(f=>f.Id.StartsWith("storage.health.",StringComparison.Ordinal)).Kind);
        Assert.Equal(EvidenceKind.Information,findings.Single(f=>f.Id=="storage.media").Kind);
    }
    [Fact] public async Task FailedWriteIsCompensatedAndOtherIndependentOperationsContinue()
    {
        var s=Session();var gateway=new FakeSettings(s.Before!){FailOnceAfterWrite=IntelligenceSettings.BackgroundCapture};
        ChangeCoordinator.Approve(s,[IntelligenceSettings.BackgroundCapture,IntelligenceSettings.GameMode]);await new ChangeCoordinator(gateway,new MemoryJournal()).ExecuteAsync(s,null,default);
        Assert.Equal(SettingValue.Dword(1),gateway.Values[IntelligenceSettings.BackgroundCapture]);Assert.Equal(SettingValue.Dword(1),gateway.Values[IntelligenceSettings.GameMode]);
        Assert.Contains(s.Changes,c=>c.State=="Falhou · estado anterior restaurado");Assert.Contains(s.Changes,c=>c.State=="Aplicado e verificado");
    }
    [Fact] public async Task RestorationUsesTheSavedOriginalStateAndVerifiesIt()
    {
        var s=Session();var gateway=new FakeSettings(s.Before!);var c=new ChangeCoordinator(gateway,new MemoryJournal());
        ChangeCoordinator.Approve(s,[IntelligenceSettings.GameMode]);await c.ExecuteAsync(s,null,default);await c.RestoreAsync(s,null,default);
        Assert.Equal(SettingValue.Dword(0),gateway.Values[IntelligenceSettings.GameMode]);Assert.Equal("Restaurado e verificado",Assert.Single(s.Changes).State);
    }
    [Fact] public async Task RestorationWillNotOverwriteAnUnexpectedThirdPartyChange()
    {
        var s=Session();var gateway=new FakeSettings(s.Before!);var c=new ChangeCoordinator(gateway,new MemoryJournal());
        ChangeCoordinator.Approve(s,[IntelligenceSettings.GameMode]);await c.ExecuteAsync(s,null,default);gateway.Values[IntelligenceSettings.GameMode]=SettingValue.Dword(2);
        await c.RestoreAsync(s,null,default);Assert.Equal(SettingValue.Dword(2),gateway.Values[IntelligenceSettings.GameMode]);Assert.Equal("Revisão necessária",Assert.Single(s.Changes).State);
    }
    [Fact] public void ArbitraryRegistryPathsOrTransitionsCannotEnterTheManagedExecutor()
    {
        Assert.Throws<InvalidDataException>(()=>ChangeCoordinator.ValidateChanges([new("HKLM\\security",SettingValue.Dword(0),SettingValue.Dword(1))]));
        Assert.Throws<InvalidDataException>(()=>ChangeCoordinator.ValidateChanges([new(IntelligenceSettings.GameMode,SettingValue.Dword(0),SettingValue.Dword(99))]));
    }
    [Fact] public async Task SessionRoundtripPreservesBackupsAndPreparedRecords()
    {
        var directory=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"goat-journal-test-"+Guid.NewGuid().ToString("N"));
        try
        {
            var store=new SessionStore(directory);var s=Session();s.Changes.Add(new(){Changes=[new(IntelligenceSettings.GameMode,SettingValue.Dword(0),SettingValue.Dword(1))]});
            await store.SaveAsync(s);var restored=await store.LoadAsync(s.Id);Assert.Equal(s.Before!.Machine.Fingerprint,restored.Before!.Machine.Fingerprint);Assert.Equal(s.Changes[0].Changes[0].Before,restored.Changes[0].Changes[0].Before);
            using(var lease=store.AcquireMutationLock()) Assert.Throws<IOException>(()=>store.AcquireMutationLock());
            await File.WriteAllTextAsync(System.IO.Path.Combine(directory,s.Id.ToString("N")+".json"),"BROKEN");
            var list=await store.ListAsync();Assert.Empty(list.Sessions);Assert.Single(list.Errors);
        }
        finally{if(Directory.Exists(directory)) Directory.Delete(directory,true);}
    }
    [Fact] public void CsvMetricsUseIntervalsAndTheMeanOfTheSlowestOnePercent()
    {
        var frames=Enumerable.Repeat(16.6666666667,594).Concat(Enumerable.Repeat(100d,6)).ToArray();
        using var file=new CsvFixture(frames);var result=BenchmarkImporter.Import(file.Path,"Antes",Machine(),Context());
        Assert.Equal(600,result.FrameCount);Assert.Equal(10,result.OnePercentLow,5);Assert.Equal(1000/frames.Average(),result.AverageFps,5);
    }
    [Fact] public void MultipleSwapChainsAreNotCombined()
    {
        using var file=new CsvFixture(Enumerable.Repeat(20d,600),multipleChains:true);
        Assert.Throws<InvalidDataException>(()=>BenchmarkImporter.Import(file.Path,"Antes",Machine(),Context()));
    }
    [Fact] public void MissingMeasurementsNeverProduceAPerformanceGain()
    {var result=BenchmarkComparer.Compare(null,null);Assert.False(result.Comparable);Assert.Null(result.AverageFpsDelta);}
    [Fact] public void DifferentConditionsOrTheSameFileMakeComparisonInconclusive()
    {
        using var file=new CsvFixture(Enumerable.Repeat(20d,600));var a=BenchmarkImporter.Import(file.Path,"Antes",Machine(),Context());
        var same=a with{Phase="Depois"};Assert.False(BenchmarkComparer.Compare(a,same).Comparable);
        Assert.False(BenchmarkComparer.Compare(a,same with{Sha256="different",Context=Context() with{Resolution="Other"}}).Comparable);
        Assert.False(BenchmarkComparer.Compare(a,same with{Sha256="different",Context=Context() with{ConditionsConfirmed=false}}).Comparable);
    }
    [Fact] public void ComparableRunsExposeOnlyMeasuredDifferences()
    {
        using var first=new CsvFixture(Enumerable.Repeat(20d,600));using var second=new CsvFixture(Enumerable.Repeat(18d,667));
        var a=BenchmarkImporter.Import(first.Path,"Antes",Machine(),Context());var b=BenchmarkImporter.Import(second.Path,"Depois",Machine(),Context());
        var result=BenchmarkComparer.Compare(a,b);Assert.True(result.Comparable);Assert.Equal(b.AverageFps-a.AverageFps,result.AverageFpsDelta);Assert.Contains("não demonstra causalidade",result.Conclusion);
    }
    [Fact] public void HtmlReportEscapesClientInputAndKeepsMissingDataExplicit()
    {
        var session=Session();session.Context=session.Context with{Symptom="<script>unsafe</script>"};var report=ServiceReport.BuildHtml(session);
        Assert.Contains("&lt;script&gt;",report);Assert.DoesNotContain("<script>",report);Assert.Contains("Inconclusivo",report);Assert.Contains("Não disponível",System.Net.WebUtility.HtmlDecode(report));
    }
    private sealed class CsvFixture:IDisposable
    {
        public string Path{get;}=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"goat-frame-fixture-"+Guid.NewGuid().ToString("N")+".csv");
        public CsvFixture(IEnumerable<double> frames,bool multipleChains=false)
        {var b=new StringBuilder("Application,ProcessID,SwapChainAddress,MsBetweenPresents\n");var i=0;foreach(var v in frames)b.Append("fixture.exe,42,").Append(multipleChains && ++i%2==0?"0x2":"0x1").Append(',').Append(v.ToString("R",CultureInfo.InvariantCulture)).Append('\n');File.WriteAllText(Path,b.ToString());}
        public void Dispose()=>File.Delete(Path);
    }
    private sealed class MemoryJournal:ISessionJournal
    {
        public List<ServiceSession> Snapshots{get;}=[];public int FailOnSave{get;init;}=int.MaxValue;private int _count;
        public Task SaveAsync(ServiceSession session,CancellationToken token=default)
        {if(++_count==FailOnSave)throw new IOException("Fixture disk failure");Snapshots.Add(JsonSerializer.Deserialize<ServiceSession>(JsonSerializer.Serialize(session))!);return Task.CompletedTask;}
        public IDisposable AcquireMutationLock()=>new NoOp();private sealed class NoOp:IDisposable{public void Dispose(){}}
    }
    private sealed class FakeSettings(DiagnosticSnapshot snapshot):ISettingsGateway
    {
        public MachineFacts Machine{get;init;}=snapshot.Machine;public Dictionary<string,SettingValue> Values{get;}=new(snapshot.Settings);
        public List<string> Writes{get;}=[];public Action? BeforeWrite{get;set;}public string? FailOnceAfterWrite{get;set;}
        public Task<MachineFacts> ReadMachineAsync(CancellationToken token)=>Task.FromResult(Machine);
        public Task<SettingValue> ReadAsync(string id,CancellationToken token)=>Task.FromResult(Values.GetValueOrDefault(id)??SettingValue.Missing);
        public Task WriteAsync(string id,SettingValue value,CancellationToken token)
        {BeforeWrite?.Invoke();Values[id]=value;Writes.Add(id);if(FailOnceAfterWrite==id){FailOnceAfterWrite=null;throw new IOException("Fixture apply failure");}return Task.CompletedTask;}
    }
}
