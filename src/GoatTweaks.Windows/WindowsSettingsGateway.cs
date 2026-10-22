using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using GoatTweaks.Core.Intelligence;
using Microsoft.Win32;

namespace GoatTweaks.Windows;

/// <summary>Fixed target list; no executable command strings, arbitrary registry paths or service names from the plan.</summary>
public sealed class WindowsSettingsGateway(WindowsDiagnostics diagnostics) : ISettingsGateway
{
    public Task<MachineFacts> ReadMachineAsync(CancellationToken token) => diagnostics.ReadMachineAsync(token);
    private static (string Path,string Name) RegistryTarget(string id) => id switch
    {
        IntelligenceSettings.GameMode => (@"Software\Microsoft\GameBar","AutoGameModeEnabled"),
        IntelligenceSettings.BackgroundCapture => (@"Software\Microsoft\Windows\CurrentVersion\GameDVR","HistoricalCaptureEnabled"),
        _ => throw new InvalidDataException("Alvo de Registro fora da lista de alterações gerenciadas.")
    };
    public async Task<SettingValue> ReadAsync(string settingId,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(settingId==IntelligenceSettings.PowerPlan)
        {
            var result=await ProcessCommand.RunAsync(ProcessCommand.SystemTool("powercfg.exe"),["/getactivescheme"],TimeSpan.FromSeconds(8),token);
            var guid=Regex.Match(result.Output,@"\b[0-9a-fA-F]{8}(?:-[0-9a-fA-F]{4}){3}-[0-9a-fA-F]{12}\b");
            if(result.ExitCode!=0 || !guid.Success) throw new IOException("Não foi possível ler o plano de energia ativo.");
            return SettingValue.PowerPlan(guid.Value);
        }
        var (path,name)=RegistryTarget(settingId);
        using var key=Registry.CurrentUser.OpenSubKey(path);
        if(key is null || !key.GetValueNames().Contains(name,StringComparer.OrdinalIgnoreCase)) return SettingValue.Missing;
        var kind=key.GetValueKind(name);var value=key.GetValue(name,null,RegistryValueOptions.DoNotExpandEnvironmentNames);
        var data=kind switch
        {
            RegistryValueKind.Binary => Convert.ToBase64String((byte[])value!),
            RegistryValueKind.MultiString => JsonSerializer.Serialize((string[])value!),
            _ => Convert.ToString(value,CultureInfo.InvariantCulture)
        };
        return new(true,kind.ToString(),data);
    }
    public async Task WriteAsync(string settingId,SettingValue value,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(settingId==IntelligenceSettings.PowerPlan)
        {
            if(!value.Exists || value.Kind!="PowerPlan" || !Guid.TryParse(value.Data,out var guid)) throw new InvalidDataException("Plano de energia inválido.");
            var result=await ProcessCommand.RunAsync(ProcessCommand.SystemTool("powercfg.exe"),["/setactive",guid.ToString("D")],TimeSpan.FromSeconds(8),token);
            if(result.ExitCode!=0) throw new IOException("Falha ao ativar plano de energia: "+result.Error+result.Output);
            return;
        }
        var (path,name)=RegistryTarget(settingId);
        if(!value.Exists)
        {
            using var existing=Registry.CurrentUser.OpenSubKey(path,writable:true);
            existing?.DeleteValue(name,throwOnMissingValue:false);
            return;
        }
        if(!Enum.TryParse<RegistryValueKind>(value.Kind,out var kind)) throw new InvalidDataException("Tipo do Registro inválido.");
        object data=kind switch
        {
            RegistryValueKind.DWord => int.Parse(value.Data!,CultureInfo.InvariantCulture),
            RegistryValueKind.QWord => long.Parse(value.Data!,CultureInfo.InvariantCulture),
            RegistryValueKind.Binary => Convert.FromBase64String(value.Data!),
            RegistryValueKind.MultiString => JsonSerializer.Deserialize<string[]>(value.Data!)??[],
            RegistryValueKind.String or RegistryValueKind.ExpandString => value.Data??"",
            _ => throw new InvalidDataException("Tipo do Registro não suportado para restauração.")
        };
        using var key=Registry.CurrentUser.CreateSubKey(path,writable:true);
        key.SetValue(name,data,kind);key.Flush();
    }
}
