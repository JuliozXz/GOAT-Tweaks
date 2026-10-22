using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace GoatTweaks.Hardware;

/// <summary>Read-only local CIM inventory. Never invokes installers, tuning scripts or elevation.</summary>
public static class HardwareDiscovery
{
    // Each class emits its own packet so a slow/denied provider cannot discard earlier results.
    // Explicit arrays retain the same JSON shape with zero, one or several devices (PowerShell 5.1).
    private const string Script = """
        $ProgressPreference = 'SilentlyContinue'
        $ErrorActionPreference = 'Stop'
        [Console]::OutputEncoding = New-Object System.Text.UTF8Encoding($false)
        function Read-HardwareClass($kind, $class, $properties) {
            try {
                $values = @(Get-CimInstance -ClassName $class -Property $properties -OperationTimeoutSec 3 -ErrorAction Stop | Select-Object -Property $properties)
                @{ kind = $kind; values = $values; failed = $false } | ConvertTo-Json -Compress -Depth 5
            } catch {
                @{ kind = $kind; values = @(); failed = $true } | ConvertTo-Json -Compress -Depth 5
            }
        }
        Read-HardwareClass 'cpu' 'Win32_Processor' @('Name','NumberOfCores','NumberOfLogicalProcessors')
        Read-HardwareClass 'gpu' 'Win32_VideoController' @('Name','DriverVersion')
        Read-HardwareClass 'ram' 'Win32_PhysicalMemory' @('Capacity','SMBIOSMemoryType','MemoryType','ConfiguredClockSpeed','DeviceLocator','Manufacturer')
        Read-HardwareClass 'board' 'Win32_BaseBoard' @('Manufacturer','Product')
        Read-HardwareClass 'os' 'Win32_OperatingSystem' @('Caption','Version','BuildNumber')
        Read-HardwareClass 'bios' 'Win32_BIOS' @('SMBIOSBIOSVersion')
        """;

    public static Task<HardwareSnapshot> ReadAsync(HardwareSnapshot previous, CancellationToken cancellationToken) =>
        Task.Run(() => CollectAsync(previous, cancellationToken), cancellationToken);

    private static async Task<HardwareSnapshot> CollectAsync(HardwareSnapshot previous, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(22));
        var packets = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        var failures = 0;
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),
                "WindowsPowerShell", "v1.0", "powershell.exe"))
            {
                UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8,
                Arguments = "-NoLogo -NoProfile -NonInteractive -EncodedCommand " + Convert.ToBase64String(Encoding.Unicode.GetBytes(Script))
            }
        };
        try
        {
            process.StartInfo.Environment["PSModulePath"] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),"WindowsPowerShell","v1.0","Modules");
            process.Start();
            var stderr = process.StandardError.ReadToEndAsync(CancellationToken.None);
            try
            {
                while (await process.StandardOutput.ReadLineAsync(timeout.Token) is { } line)
                {
                    if (line.Length > 262144) continue;
                    try
                    {
                        using var document = JsonDocument.Parse(line);
                        var root = document.RootElement;
                        var kind = root.GetProperty("kind").GetString();
                        if (kind is null) continue;
                        if (root.GetProperty("failed").GetBoolean()) { failures++; continue; }
                        packets[kind] = root.GetProperty("values").Clone();
                    }
                    catch (JsonException) { }
                    catch (KeyNotFoundException) { }
                    catch (InvalidOperationException) { }
                }
                await process.WaitForExitAsync(timeout.Token);
                if (process.ExitCode != 0) failures++;
            }
            finally
            {
                if (!process.HasExited)
                {
                    try { process.Kill(entireProcessTree: true); }
                    catch (InvalidOperationException) { }
                    catch (System.ComponentModel.Win32Exception) { }
                }
                await stderr;
            }
        }
        catch (OperationCanceledException) { failures++; }
        catch (System.ComponentModel.Win32Exception) { failures++; }
        catch (IOException) { failures++; }
        catch (InvalidOperationException) { failures++; }
        cancellationToken.ThrowIfCancellationRequested();

        IEnumerable<JsonElement> Values(string kind) => packets.TryGetValue(kind, out var values) && values.ValueKind == JsonValueKind.Array
            ? values.EnumerateArray().ToArray() : [];
        var processors = Values("cpu").Select(p => new ProcessorInfo(Text(p, "Name"), (int)Number(p, "NumberOfCores"), (int)Number(p, "NumberOfLogicalProcessors")))
            .Where(p => p.Name.Length > 0).ToArray();
        var graphics = Values("gpu").Select(g => new GraphicsInfo(Text(g, "Name"), Text(g, "DriverVersion") is { Length: > 0 } driver ? driver : HardwareSnapshot.Unknown))
            .Where(g => g.Name.Length > 0).Distinct().ToArray();
        var modules = Values("ram").Select((m, index) => new MemoryModule(
            Text(m, "DeviceLocator") is { Length: > 0 } slot ? slot : "Módulo " + (index + 1), Number(m, "Capacity"),
            NormalizeMemoryType(m),
            (uint)Number(m, "ConfiguredClockSpeed"), Text(m, "Manufacturer")))
            .Where(m => m.Capacity > 0).ToArray();
        var board = Values("board").FirstOrDefault();
        var os = Values("os").FirstOrDefault();
        var bios = Values("bios").FirstOrDefault();
        var memory = SystemUsage.ReadMemory();
        return previous with
        {
            Processors = packets.ContainsKey("cpu") ? processors : previous.Processors,
            Graphics = packets.ContainsKey("gpu") ? graphics : previous.Graphics,
            Modules = packets.ContainsKey("ram") ? modules : previous.Modules,
            BoardManufacturer = packets.ContainsKey("board") ? Text(board, "Manufacturer") : previous.BoardManufacturer,
            BoardProduct = packets.ContainsKey("board") ? Text(board, "Product") : previous.BoardProduct,
            WindowsName = Text(os, "Caption") is { Length: > 0 } caption ? caption + " · build " + Text(os, "BuildNumber") : previous.WindowsName,
            BiosVersion = packets.ContainsKey("bios") ? Text(bios, "SMBIOSBIOSVersion") : previous.BiosVersion,
            UsableMemory = memory?.Total ?? previous.UsableMemory,
            Detected = true, FailedQueries = Math.Max(failures, 6 - packets.Count), UpdatedAt = DateTimeOffset.Now
        };
    }

    private static string Text(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out var value)) return "";
        var text = value.ToString().Trim();
        return text.ToLowerInvariant() is "to be filled by o.e.m." or "to be filled by oem" or "default string" or "system product name" or "not applicable" or "unknown" or "none" ? "" : text;
    }

    private static ulong Number(JsonElement element, string name) => element.ValueKind == JsonValueKind.Object &&
        element.TryGetProperty(name, out var value) && ulong.TryParse(value.ToString(), out var number) ? number : 0;

    private static uint NormalizeMemoryType(JsonElement module)
    {
        var smbios = (uint)Number(module, "SMBIOSMemoryType");
        if (smbios > 2) return smbios;
        // CIM's older enumeration offsets DDR/DDR2; do not interpret it as raw SMBIOS.
        return Number(module, "MemoryType") switch { 20 => 18, 21 => 19, 22 => 20, 24 => 24, 26 => 26, _ => 0 };
    }
}
