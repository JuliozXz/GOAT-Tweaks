using System.Text.Json;
using GoatTweaks.Core.Intelligence;
using GoatTweaks.Windows;
using Microsoft.Win32;

if(!OperatingSystem.IsWindows() || args.Length!=2 || args[0]!="--isolated-github-vm" || Environment.GetEnvironmentVariable("GITHUB_ACTIONS")!="true")
    throw new InvalidOperationException("Este verificador só pode ser executado na VM isolada de CI, com autorização explícita.");
var output=Path.GetFullPath(args[1]);Directory.CreateDirectory(output);
var diagnostics=new WindowsDiagnostics();var gateway=new WindowsSettingsGateway(diagnostics);
var results=new Dictionary<string,object?> { ["isolatedVm"]=true,["physicalGamingBenchmark"]=false,["legacyActionsApplied"]=0 };
var roundtrips=new List<object>();
foreach(var id in new[]{IntelligenceSettings.GameMode,IntelligenceSettings.BackgroundCapture})
{
    var original=await gateway.ReadAsync(id,default);
    var path=id==IntelligenceSettings.GameMode?@"Software\Microsoft\GameBar":@"Software\Microsoft\Windows\CurrentVersion\GameDVR";
    using var existing=Registry.CurrentUser.OpenSubKey(path);
    var keyExisted=existing is not null;
    try
    {
        await gateway.WriteAsync(id,SettingValue.Dword(0),default);
        if(await gateway.ReadAsync(id,default)!=SettingValue.Dword(0)) throw new IOException("Falha na leitura DWORD 0 de "+id);
        await gateway.WriteAsync(id,SettingValue.Dword(1),default);
        if(await gateway.ReadAsync(id,default)!=SettingValue.Dword(1)) throw new IOException("Falha na leitura DWORD 1 de "+id);
    }
    finally
    {
        await gateway.WriteAsync(id,original,default);
        if(await gateway.ReadAsync(id,default)!=original) throw new IOException("A restauração do valor original falhou: "+id);
        if(!keyExisted)
        {
            using(var leaf=Registry.CurrentUser.OpenSubKey(path))
                if(leaf is not null && (leaf.ValueCount!=0 || leaf.SubKeyCount!=0)) throw new IOException("A chave do teste recebeu outra alteração e não será removida.");
            Registry.CurrentUser.DeleteSubKey(path,throwOnMissingSubKey:false);
        }
    }
    roundtrips.Add(new {id,original,restored=await gateway.ReadAsync(id,default),verified=true,keyExisted});
}
results["registryRoundtrips"]=roundtrips;
var originalPower=await gateway.ReadAsync(IntelligenceSettings.PowerPlan,default);
var powerMachine=await diagnostics.ReadMachineAsync(default);
if(powerMachine.AvailablePowerPlans.Contains(IntelligenceSettings.BalancedGuid,StringComparer.OrdinalIgnoreCase) && originalPower.Exists)
{
    try
    {
        await gateway.WriteAsync(IntelligenceSettings.PowerPlan,SettingValue.PowerPlan(IntelligenceSettings.BalancedGuid),default);
        if(await gateway.ReadAsync(IntelligenceSettings.PowerPlan,default)!=SettingValue.PowerPlan(IntelligenceSettings.BalancedGuid))
            throw new IOException("A ativação do plano Equilibrado não foi confirmada.");
    }
    finally
    {
        await gateway.WriteAsync(IntelligenceSettings.PowerPlan,originalPower,default);
        if(await gateway.ReadAsync(IntelligenceSettings.PowerPlan,default)!=originalPower)
            throw new IOException("A restauração do plano de energia original falhou.");
    }
    results["powerPlanRoundtrip"]=new {original=originalPower,restored=await gateway.ReadAsync(IntelligenceSettings.PowerPlan,default),verified=true};
}
else results["powerPlanRoundtripLimitation"]="Plano Equilibrado ou estado original indisponível na VM; reversão não exercitada neste ambiente.";
Console.WriteLine("Coletando diagnóstico inicial real na VM.");
var before=await diagnostics.CaptureAsync(TimeSpan.FromSeconds(12),null,new Progress<string>(Console.WriteLine),default);
if(before.Samples.Count<5 || string.IsNullOrWhiteSpace(before.Machine.Cpu) || before.Machine.Cpu.Contains("não informado",StringComparison.OrdinalIgnoreCase)
    || before.Machine.Gpus.Count==0 || before.Machine.InstalledRamGb<=0 || before.Machine.MachineId.Length==0 || !before.Samples.Any(s=>s.MemoryPercent.HasValue))
    throw new InvalidDataException("Diagnóstico nativo sem dados mínimos.");
var session=new ServiceSession{Before=before,Context=new(){Game="Verificação de interface e diagnóstico na VM",Symptom="Conferir coleta real e relatórios sem benchmark de jogo"},Profile=OptimizationProfile.Diagnostic};
session.Findings=RecommendationEngine.Diagnose(before,session.Context);session.Recommendations=RecommendationEngine.Build(before,session.Context,session.Profile);
session.Stage=ServiceStage.Plan;
var store=new SessionStore(Path.Combine(output,"sessions"));await store.SaveAsync(session);
var resumed=await store.LoadAsync(session.Id);
if(resumed.Before?.Machine.Fingerprint!=before.Machine.Fingerprint) throw new InvalidDataException("Identificação divergiu ao retomar atendimento.");
try{ChangeCoordinator.Approve(resumed,[IntelligenceSettings.GameMode]);throw new InvalidDataException("Diagnóstico permitiu autorização de escrita.");}
catch(InvalidOperationException){results["diagnosticAuthorizationBlocked"]=true;}
Console.WriteLine("Coletando validação posterior real na VM.");
session.After=await diagnostics.CaptureAsync(TimeSpan.FromSeconds(12),null,new Progress<string>(Console.WriteLine),default);
session.Stage=ServiceStage.Report;
await store.SaveAsync(session);
await File.WriteAllTextAsync(Path.Combine(output,"relatorio-real-vm.html"),ServiceReport.BuildHtml(session));
await File.WriteAllTextAsync(Path.Combine(output,"atendimento-real-vm.json"),JsonSerializer.Serialize(session,SessionStore.JsonOptions));
results["hardware"]=before.Machine;results["initialSamples"]=before.Samples.Count;results["afterSamples"]=session.After.Samples.Count;
results["sameMachineAndUser"]=session.After.Machine.MachineId==before.Machine.MachineId && session.After.Machine.UserId==before.Machine.UserId;
results["unavailable"]=before.Unavailable;results["managedOptimizationsApplied"]=session.Changes.Count;
results["benchmarkComparison"]=BenchmarkComparer.Compare(null,null);
results["knowledgeEntries"]=KnowledgeBase.LoadAll().Count;
results["wprDetected"]=PerformanceTools.FindWpr() is not null;results["wpaDetected"]=PerformanceTools.FindWpa() is not null;
if(PerformanceTools.FindWpr() is not null)
{
    try{results["wprTrace"]=await PerformanceTools.CaptureWprAsync(output,TimeSpan.FromSeconds(10),new Progress<string>(Console.WriteLine),default);results["wprCaptureVerified"]=true;}
    catch(Exception ex){results["wprCaptureVerified"]=false;results["wprLimitation"]=ex.Message;}
}
await File.WriteAllTextAsync(Path.Combine(output,"native-verification.json"),JsonSerializer.Serialize(results,SessionStore.JsonOptions));
Console.WriteLine("Coleta, persistência, retomada, bloqueio de escrita em Diagnóstico e reversão dos valores da VM verificados.");
