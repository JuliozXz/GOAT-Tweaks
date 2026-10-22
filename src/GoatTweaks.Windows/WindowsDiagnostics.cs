using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text.Json;
using System.Text.RegularExpressions;
using GoatTweaks.Core.Intelligence;
using GoatTweaks.Hardware;
using Microsoft.Win32;

namespace GoatTweaks.Windows;

public sealed class WindowsDiagnostics
{
    private HardwareSnapshot _hardware = new();
    private const string ReadFactsScript = """
        $ErrorActionPreference = 'Stop'
        $ProgressPreference = 'SilentlyContinue'
        [Console]::OutputEncoding = New-Object System.Text.UTF8Encoding($false)
        function Emit($kind, $values) { @{kind=$kind;values=@($values)} | ConvertTo-Json -Compress -Depth 5 }
        function ReadFact($kind, $class, $properties, $namespace='root/cimv2', $filter=$null) {
            try {
                $p=@{ClassName=$class;Namespace=$namespace;Property=$properties;OperationTimeoutSec=3;ErrorAction='Stop'}
                if ($filter) {$p.Filter=$filter}
                Emit $kind @(Get-CimInstance @p | Select-Object $properties)
            } catch {Emit 'unavailable' @($kind + ': ' + $_.Exception.Message)}
        }
        ReadFact 'system' 'Win32_ComputerSystem' @('PCSystemType','Manufacturer','Model')
        ReadFact 'identity' 'Win32_ComputerSystemProduct' @('UUID')
        ReadFact 'enclosure' 'Win32_SystemEnclosure' @('ChassisTypes')
        ReadFact 'os' 'Win32_OperatingSystem' @('Caption','BuildNumber','ProductType','LastBootUpTime')
        ReadFact 'physical' 'MSFT_PhysicalDisk' @('FriendlyName','MediaType','BusType','HealthStatus') 'root/Microsoft/Windows/Storage'
        ReadFact 'devices' 'Win32_PnPEntity' @('Name','PNPDeviceID','ConfigManagerErrorCode') 'root/cimv2' 'ConfigManagerErrorCode <> 0'
        try {
            $events=@(Get-WinEvent -FilterHashtable @{LogName='System';Id=@(4101,17,18,19);StartTime=(Get-Date).AddDays(-1)} -MaxEvents 100 -ErrorAction SilentlyContinue |
                Where-Object {$_.ProviderName -match 'WHEA|^Display$'} | Select-Object -First 30 |
                Select-Object @{n='At';e={$_.TimeCreated.ToUniversalTime().ToString('o')}},ProviderName,Id,@{n='Message';e={if($_.Message){$_.Message.Substring(0,[Math]::Min(800,$_.Message.Length))}else{''}}})
            Emit 'events' $events
        } catch {Emit 'unavailable' @('Eventos do sistema: ' + $_.Exception.Message)}
        """;

    public async Task<MachineFacts> ReadMachineAsync(CancellationToken token) => (await ReadFactsAsync(token)).Machine;

    private async Task<(MachineFacts Machine, Dictionary<string,JsonElement> Packets, List<string> Unavailable)> ReadFactsAsync(CancellationToken token)
    {
        var hardwareTask = HardwareDiscovery.ReadAsync(_hardware, token);
        var extraTask = ProcessCommand.PowerShellAsync(ReadFactsScript, TimeSpan.FromSeconds(40), token);
        var plansTask = ProcessCommand.RunAsync(ProcessCommand.SystemTool("powercfg.exe"), ["/list"], TimeSpan.FromSeconds(8), token);
        var packets = new Dictionary<string,JsonElement>(StringComparer.Ordinal);
        var unavailable = new List<string>();
        _hardware = await hardwareTask;
        try
        {
            var extra = await extraTask;
            foreach (var line in extra.Output.Split('\n',StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    using var document = JsonDocument.Parse(line);
                    var kind = document.RootElement.GetProperty("kind").GetString()!;
                    var values = document.RootElement.GetProperty("values").Clone();
                    if (kind == "unavailable") unavailable.AddRange(values.EnumerateArray().Select(v=>v.ToString()));
                    else packets[kind] = values;
                }
                catch (JsonException) { unavailable.Add("Pacote de diagnóstico local ilegível; esse dado não foi usado."); }
            }
            if (extra.ExitCode != 0) unavailable.Add("Uma ou mais consultas de diagnóstico não concluíram: "+extra.Error.Trim()[..Math.Min(1200,extra.Error.Trim().Length)]);
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested) { unavailable.Add("Tempo limite das consultas CIM/eventos. Dados incompletos não são estimados."); }
        string[] plans = [];
        try
        {
            var result = await plansTask;
            if (result.ExitCode == 0) plans = Regex.Matches(result.Output, @"\b[0-9a-fA-F]{8}(?:-[0-9a-fA-F]{4}){3}-[0-9a-fA-F]{12}\b").Select(m=>m.Value.ToLowerInvariant()).Distinct().ToArray();
            else unavailable.Add("Lista de planos de energia indisponível.");
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested) { unavailable.Add("Consulta de planos de energia excedeu o prazo."); }
        var os = Values(packets,"os").FirstOrDefault();
        var system = Values(packets,"system").FirstOrDefault();
        var identity = Text(Values(packets,"identity").FirstOrDefault(),"UUID");
        var chassis = Values(packets,"enclosure").SelectMany(v=>v.TryGetProperty("ChassisTypes",out var types) && types.ValueKind==JsonValueKind.Array ? types.EnumerateArray().Select(NumberValue) : []).ToArray();
        bool? notebook = Number(system,"PCSystemType") == 2 || chassis.Any(c=>c is 8 or 9 or 10 or 14 or 30 or 31 or 32) ? true
            : Number(system,"PCSystemType") == 1 || chassis.Any(c=>c is 3 or 4 or 5 or 6 or 7 or 15 or 16 or 23) ? false : null;
        if (string.IsNullOrWhiteSpace(identity))
        {
            using var machineKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,RegistryView.Registry64).OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
            identity = machineKey?.GetValue("MachineGuid")?.ToString() ?? Environment.MachineName;
        }
        var powerKnown = GetSystemPowerStatus(out var power);
        var facts = new MachineFacts
        {
            MachineId = IntelligenceLabels.Hash(identity + "|" + Environment.MachineName), UserId = WindowsIdentity.GetCurrent().User?.Value ?? Environment.UserName,
            Computer = Environment.MachineName, User = Environment.UserName, Os = _hardware.WindowsName,
            OsBuild = Number(os,"BuildNumber"), IsWindowsClient = Number(os,"ProductType") == 1,
            Architecture = RuntimeInformation.OSArchitecture.ToString(), Cpu = _hardware.CpuName, LogicalProcessors = _hardware.Processors.Sum(p=>p.Threads),
            CpuTopology = _hardware.CpuSummary, ComputerModel = Text(system,"Manufacturer")+" "+Text(system,"Model"),
            Gpus = _hardware.Graphics.Select(g=>g.Name).ToList(), GraphicsDrivers = _hardware.Graphics.Select(g=>g.Driver).ToList(),
            InstalledRamGb = _hardware.InstalledMemory > 0 ? _hardware.InstalledMemory/1073741824d : _hardware.UsableMemory > 0 ? _hardware.UsableMemory/1073741824d : null,
            RamConfiguration = _hardware.RamSummary, Board = _hardware.BoardName, Firmware = _hardware.BiosVersion, Notebook = notebook,
            MemoryModules = _hardware.Modules.Select(m=>$"{m.Slot}: {HardwareSnapshot.FormatBytes(m.Capacity)} · {HardwareSnapshot.MemoryType(m.Type)} · {(m.Speed>0?m.Speed+" MHz informados pelo Windows":"velocidade não declarada")}").ToList(),
            OnAcPower = powerKnown && power.AcLineStatus is 0 or 1 ? power.AcLineStatus == 1 : null,
            BootId = Text(os,"LastBootUpTime"), AvailablePowerPlans = plans.ToList(),
            PhysicalStorage = Values(packets,"physical").Select(d=>$"{Text(d,"FriendlyName")} · {(Number(d,"MediaType") == 4 ? "SSD" : Number(d,"MediaType")==3 ? "HD" : "mídia não declarada")} · estado {Number(d,"HealthStatus")}").ToList(),
            PhysicalDisks = Values(packets,"physical").Select(d=>new PhysicalDiskObservation(Text(d,"FriendlyName"),Number(d,"MediaType")==4?"SSD":Number(d,"MediaType")==3?"HD":"Não declarado",int.TryParse(Text(d,"HealthStatus"),out var health)?health:null)).ToList(),
            FirmwareFeatures = ReadFirmwareFeatures(), GraphicsFeatures = ReadGraphicsFeatures(),
            Capabilities = ["Inventário CIM local", "Contadores Windows quando disponíveis", "Sensores compartilhados opcionais", "PresentMon/WPR opcionais instalados pelo técnico"]
        };
        if (_hardware.FailedQueries > 0) unavailable.Add($"Inventário: {_hardware.FailedQueries} consulta(s) de hardware sem resultado completo.");
        if(_hardware.DiscoveryWarning.Length>0) unavailable.Add(_hardware.DiscoveryWarning);
        if (notebook is null) unavailable.Add("Tipo desktop/notebook não confirmado; ajustes de energia não serão automatizados.");
        return (facts,packets,unavailable);
    }

    public async Task<DiagnosticSnapshot> CaptureAsync(TimeSpan duration, IProgress<MetricSample>? sampleProgress, IProgress<string>? progress, CancellationToken token)
    {
        if (duration < TimeSpan.FromSeconds(10) || duration > TimeSpan.FromMinutes(2)) throw new ArgumentOutOfRangeException(nameof(duration));
        var started = DateTimeOffset.UtcNow;
        progress?.Report("Consultando hardware, compatibilidade, energia, dispositivos e eventos locais…");
        var (machine, packets, unavailable) = await ReadFactsAsync(token);
        var settingReader = new WindowsSettingsGateway(this);
        var settings = new Dictionary<string,SettingValue>();
        foreach (var id in IntelligenceSettings.Managed)
        {
            try { settings[id] = await settingReader.ReadAsync(id,token); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.ComponentModel.Win32Exception)
            { unavailable.Add("Configuração indisponível: " + id + " · " + ex.Message); }
        }
        var baseline = Processes();
        var samples = new List<MetricSample>();
        progress?.Report("Coletando amostras reais a cada 2 segundos. Reproduza a carga que deseja avaliar.");
        await Task.Run(async () =>
        {
            using var counters = new WindowsPerformanceSampler();
            using var sensors = new SharedSensorReader();
            var clock = Stopwatch.StartNew();
            while (clock.Elapsed < duration)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(TimeSpan.FromSeconds(2),token);
                var sample = sensors.Enrich(counters.Read());
                samples.Add(sample); sampleProgress?.Report(sample);
            }
            unavailable.AddRange(counters.Unavailable);
            if (!samples.Any(s=>s.Gpu3dPercent.HasValue)) unavailable.Add("GPU: contador de motor 3D ausente ou sem instância ativa nesta coleta.");
            if (!samples.Any(s=>s.CpuTemperatureC.HasValue)) unavailable.Add("Temperatura da CPU indisponível. " + sensors.Status);
            if (!samples.Any(s=>s.GpuTemperatureC.HasValue)) unavailable.Add("Temperatura da GPU indisponível. " + sensors.Status);
            if (!samples.Any(s=>s.CpuClockMhz.HasValue || s.GpuClockMhz.HasValue)) unavailable.Add("Frequências de sensores indisponíveis; frequência nominal não substitui clock medido.");
        },token);
        var end = Processes();
        var processes = end.Values.Select(p=>
        {
            double? cpu = baseline.TryGetValue(p.Id,out var b) && p.Start == b.Start && p.Cpu >= b.Cpu && p.Tick > b.Tick
                ? Math.Clamp(100*(p.Cpu-b.Cpu).TotalSeconds/((p.Tick-b.Tick)/1000d)/Environment.ProcessorCount,0,100) : null;
            return new ProcessObservation(p.Id,p.Name,cpu,p.Memory/1048576d);
        }).OrderByDescending(p=>p.CpuPercent??-1).ThenByDescending(p=>p.WorkingSetMb).Take(40).ToList();
        var storage = new List<StorageObservation>();
        foreach(var drive in DriveInfo.GetDrives().Where(d=>d.DriveType==DriveType.Fixed))
            try { if(drive.IsReady) storage.Add(new(drive.Name,drive.TotalSize/1073741824d,drive.AvailableFreeSpace/1073741824d)); }
            catch(IOException) { unavailable.Add("Leitura de espaço indisponível em " + drive.Name); }
        var drivers = machine.Gpus.Select((name,i)=>new DriverObservation(name,machine.GraphicsDrivers.ElementAtOrDefault(i)??"Não disponível",0)).ToList();
        drivers.AddRange(Values(packets,"devices").Select(d=>new DriverObservation(
            !string.IsNullOrWhiteSpace(Text(d,"Name"))?Text(d,"Name"):!string.IsNullOrWhiteSpace(Text(d,"PNPDeviceID"))?Text(d,"PNPDeviceID"):"Dispositivo sem nome informado pelo Windows",
            "Consulte o Gerenciador de Dispositivos",Number(d,"ConfigManagerErrorCode"))));
        var issues = Values(packets,"events").Select(e=>new SystemIssue(DateTimeOffset.TryParse(Text(e,"At"),out var at)?at:started,Text(e,"ProviderName"),Number(e,"Id"),Text(e,"Message"))).ToList();
        return new DiagnosticSnapshot { StartedAt=started,FinishedAt=DateTimeOffset.UtcNow,Machine=machine, Samples=samples,Processes=processes,Storage=storage,
            Startup=ReadStartup(unavailable),Drivers=drivers,Issues=issues,Settings=settings,Unavailable=unavailable.Distinct().ToList() };
    }
    private sealed record ProcessPoint(int Id,string Name,TimeSpan Cpu,long Memory,long Tick,DateTime Start);
    private static Dictionary<string,string> ReadFirmwareFeatures()
    {
        var result=new Dictionary<string,string>();
        result["Modo de firmware"]=GetFirmwareType(out var type)?type switch{1=>"BIOS legado",2=>"UEFI",_=>"Não disponível"}:"Não disponível";
        result["Inicialização Segura"]=ReadMachineRegistry(@"SYSTEM\CurrentControlSet\Control\SecureBoot\State","UEFISecureBootEnabled") switch
        {0=>"Desativada (estado informado pelo Windows)",1=>"Ativada (estado informado pelo Windows)",_=>"Não disponível"};
        result["XMP/EXPO, voltagem e limites térmicos"]="Não expostos por esta fonte; consultar firmware/fabricante manualmente";
        return result;
    }
    private static Dictionary<string,string> ReadGraphicsFeatures()=>new()
    {
        ["Preferência HwSchMode (snapshot)"]=ReadMachineRegistry(@"SYSTEM\CurrentControlSet\Control\GraphicsDrivers","HwSchMode") is {} mode
            ?$"DWORD {mode}; não confirma agendamento ativo nem suporte do driver. Conferir em Configurações de gráficos."
            :"Não declarada; padrão e suporte dependem do Windows/driver",
        ["VRR, frequência efetiva e painel do fabricante"]="Consultar as configurações de exibição e do jogo; não inferidos da preferência de registro"
    };
    private static int? ReadMachineRegistry(string path,string name)
    {
        try
        {
            using var root=RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,RegistryView.Registry64);
            using var key=root.OpenSubKey(path);
            return key?.GetValueKind(name)==RegistryValueKind.DWord?key.GetValue(name) as int?:null;
        }
        catch(Exception ex) when(ex is IOException or UnauthorizedAccessException or System.Security.SecurityException){return null;}
    }
    private static Dictionary<int,ProcessPoint> Processes()
    {
        var values = new Dictionary<int,ProcessPoint>();
        foreach(var process in Process.GetProcesses())
        {
            using(process)
                try { values[process.Id]=new(process.Id,process.ProcessName,process.TotalProcessorTime,process.WorkingSet64,Environment.TickCount64,process.StartTime); }
                catch(Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception or NotSupportedException) { }
        }
        return values;
    }
    private static List<StartupObservation> ReadStartup(List<string> unavailable)
    {
        var result=new List<StartupObservation>();
        foreach(var hive in new[]{RegistryHive.CurrentUser,RegistryHive.LocalMachine})
            foreach(var view in new[]{RegistryView.Registry64,RegistryView.Registry32})
                try
                {
                    using var root=RegistryKey.OpenBaseKey(hive,view);using var key=root.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                    if(key is not null) result.AddRange(key.GetValueNames().Select(name=>new StartupObservation(name,$"{hive} · Run · {view}")));
                }
                catch(UnauthorizedAccessException) { unavailable.Add("Algumas entradas de inicialização não puderam ser lidas."); }
        foreach(var path in new[]{Environment.GetFolderPath(Environment.SpecialFolder.Startup),Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup)})
            try { if(Directory.Exists(path)) result.AddRange(Directory.EnumerateFiles(path).Select(p=>new StartupObservation(Path.GetFileName(p),"Pasta de inicialização"))); }
            catch(Exception ex) when(ex is IOException or UnauthorizedAccessException) { unavailable.Add("Pasta de inicialização indisponível."); }
        return result.Distinct().ToList();
    }
    private static IEnumerable<JsonElement> Values(Dictionary<string,JsonElement> packets,string key) => packets.TryGetValue(key,out var array) && array.ValueKind==JsonValueKind.Array ? array.EnumerateArray().ToArray() : [];
    private static string Text(JsonElement e,string name) => e.ValueKind==JsonValueKind.Object && e.TryGetProperty(name,out var value) ? value.ToString() : "";
    private static int Number(JsonElement e,string name) => int.TryParse(Text(e,name),out var value) ? value : 0;
    private static int NumberValue(JsonElement e) => int.TryParse(e.ToString(),out var value) ? value : 0;
    [StructLayout(LayoutKind.Sequential)] private struct PowerStatus { public byte AcLineStatus,BatteryFlag,BatteryLifePercent,StatusFlag;public uint BatteryLifeTime,BatteryFullLifeTime; }
    [DllImport("kernel32.dll")] [return:MarshalAs(UnmanagedType.Bool)] private static extern bool GetSystemPowerStatus(out PowerStatus status);
    [DllImport("kernel32.dll")] [return:MarshalAs(UnmanagedType.Bool)] private static extern bool GetFirmwareType(out uint type);
}
