using System.Diagnostics;
using System.Security.Principal;
using System.Text.Json;
using GoatTweaks.Core.Catalog;
using GoatTweaks.Core.Execution;
using GoatTweaks.Runner.Logging;
using RyTuneX.Helpers;

namespace GoatTweaks.Runner;

internal sealed class ActionExecutor(CatalogService catalog, JsonlLogWriter log, string root)
{
    public async Task<ActionLogEntry> ExecuteAsync(ActionRequest request)
    {
        if (!catalog.ById.TryGetValue(request.ItemId, out var item))
            throw new InvalidDataException($"Unknown catalog item: {request.ItemId}");
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19045))
            throw new PlatformNotSupportedException("Requer Windows 10 22H2 ou Windows 11 de 64 bits.");
        ActionSafetyPolicy.Validate(item, request);
        if (item.RequiresAdmin &&
            !new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            return new ActionLogEntry(DateTimeOffset.Now, item.Id, item.Source, "Administrator privileges required",
                1, "", "Esta ação requer privilégios de administrador.");
        if (!CatalogIntegrity.Verify(item, root))
            throw new InvalidDataException($"SHA-256 mismatch for {item.SourceFile}");
        var command = Describe(item, request);
        Environment.SetEnvironmentVariable("GOATTWEAKS_ACTION_LOG", log.Path);
        Environment.SetEnvironmentVariable("GOATTWEAKS_ACTION_ID", item.Id);
        await log.AppendAsync(new ActionLogEntry(DateTimeOffset.Now, item.Id, item.Source,
            command, 0, "", "", "started"));

        var (exitCode, stdout, stderr) = item.Source switch
        {
            "Ultimate" => await RunUltimateAsync(item, request, root),
            "WinSux" => await RunWinSuxAsync(item, request, root),
            "winutil" => await RunWinUtilAsync(item, request, root),
            "RyTuneX" => await RunRyTuneXAsync(item, request),
            _ => throw new InvalidDataException($"Unsupported source: {item.Source}")
        };
        return new ActionLogEntry(DateTimeOffset.Now, item.Id, item.Source, command, exitCode,
            JsonlLogWriter.Tail(stdout), JsonlLogWriter.Tail(stderr));
    }

    private static string Describe(CatalogItem item, ActionRequest request) => item.Source switch
    {
        "Ultimate" => $"{item.SourceFile} sha256={item.Sha256}",
        "WinSux" => $"{item.SourceFile} sha256={item.Sha256}",
        "RyTuneX" when item.Kind == "toggle" => $"OptimizationOptions.ExecuteToggleActionAsync({item.Tag}, {(request.Revert ? false : request.Enabled ?? true)})",
        "RyTuneX" when item.Kind == "combo" => $"OptimizeSystemHelper.{SelectComboMethod(item, request)}",
        "RyTuneX" when item.Kind == "command" => string.Join(" ; ", item.Commands),
        "winutil" when item.Kind == "function" => item.Function ?? item.Id,
        _ => item.SourceFile
    };

    private async Task<(int ExitCode, string Stdout, string Stderr)> RunUltimateAsync(
        CatalogItem item, ActionRequest request, string baseDirectory)
    {
        var scriptPath = CatalogIntegrity.ResolveSourcePath(item, baseDirectory)
            ?? throw new InvalidDataException("Ultimate script path is invalid.");
        var option = request.Revert ? item.Revert : request.OptionKey;
        var input = UltimateInputBuilder.Build(item, option, request.Inputs);
        if (ActionSafetyPolicy.RequiresManualReview(item) || UltimateRunPolicy.UseConsoleWindow(item.ConsoleRequired, request.ConsoleWindow))
        {
            var info = new ProcessStartInfo("powershell.exe")
            {
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Normal
            };
            info.ArgumentList.Add("-NoProfile");
            info.ArgumentList.Add("-ExecutionPolicy");
            info.ArgumentList.Add("Bypass");
            info.ArgumentList.Add("-File");
            info.ArgumentList.Add(scriptPath);
            using var console = Process.Start(info) ?? throw new InvalidOperationException("Could not open PowerShell console.");
            using var consoleTimeout = new CancellationTokenSource(TimeSpan.FromMinutes(
                int.TryParse(Environment.GetEnvironmentVariable("GOATTWEAKS_ULTIMATE_TIMEOUT_MINUTES"), out var configuredConsole)
                    ? Math.Max(1, configuredConsole) : 120));
            try
            {
                await console.WaitForExitAsync(consoleTimeout.Token);
            }
            catch (OperationCanceledException)
            {
                console.Kill(entireProcessTree: true);
                return (124, "", "Action timed out.");
            }
            return (console.ExitCode, "", "");
        }

        if (!item.RequiresAdmin && item.Kind == "oneshot")
            return await StartProcessAsync("powershell.exe",
                ["-NoProfile", "-ExecutionPolicy", "Bypass", "-File", scriptPath], null, false);

        var timeoutMinutes = UltimateRunPolicy.TimeoutMinutes(
            Environment.GetEnvironmentVariable("GOATTWEAKS_ULTIMATE_TIMEOUT_MINUTES"));
        return await StartProcessAsync("powershell.exe",
            ["-NoProfile", "-ExecutionPolicy", "Bypass", "-File", scriptPath],
            input, true, TimeSpan.FromMinutes(timeoutMinutes));
    }

    private async Task<(int ExitCode, string Stdout, string Stderr)> RunWinSuxAsync(
        CatalogItem item, ActionRequest request, string baseDirectory)
    {
        var scriptPath = CatalogIntegrity.ResolveSourcePath(item, baseDirectory)
            ?? throw new InvalidDataException("WinSux script path is invalid.");
        var info = new ProcessStartInfo(ProcessCommandPowerShell()) { UseShellExecute = true, WindowStyle = ProcessWindowStyle.Normal };
        foreach(var arg in new[]{"-NoProfile","-ExecutionPolicy","Bypass","-File",scriptPath}) info.ArgumentList.Add(arg);
        using var process=Process.Start(info)??throw new IOException("Não foi possível abrir a ferramenta original em console visível.");
        await process.WaitForExitAsync();
        return (process.ExitCode,"Execução manual da ferramenta original em console visível.","");
    }

    private static string ProcessCommandPowerShell()=>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),"WindowsPowerShell","v1.0","powershell.exe");

    private async Task<(int ExitCode, string Stdout, string Stderr)> RunWinUtilAsync(
        CatalogItem item, ActionRequest request, string baseDirectory)
    {
        var requestPath = Path.Combine(Path.GetTempPath(), $"goat-winutil-{Guid.NewGuid():N}.json");
        var data = new
        {
            itemId = item.Id,
            kind = item.Kind,
            function = item.Function,
            revert = request.Revert,
            packageId = item.PackageId,
            winget = item.Winget,
            choco = item.Choco
        };
        File.WriteAllText(requestPath, JsonSerializer.Serialize(data));
        try
        {
            var harness = Path.Combine(baseDirectory, "third_party", "winutil", "harness.ps1");
            return await StartProcessAsync("powershell.exe",
                ["-NoProfile", "-ExecutionPolicy", "Bypass", "-File", harness, "-RequestPath", requestPath],
                null, true);
        }
        finally
        {
            File.Delete(requestPath);
        }
    }

    private async Task<(int ExitCode, string Stdout, string Stderr)> RunRyTuneXAsync(
        CatalogItem item, ActionRequest request)
    {
        if (item.Kind == "toggle")
        {
            var isOn = request.Revert ? !(request.Enabled ?? true) : request.Enabled ?? true;
            using var errors = ActionLog.BeginErrorCollection();
            await OptimizationOptions.ExecuteToggleActionAsync(item.Tag, isOn);
            return errors.Messages.Count == 0
                ? (0, "", "")
                : (1, "", string.Join(Environment.NewLine, errors.Messages));
        }
        if (item.Kind == "combo")
        {
            var method = SelectComboMethod(item, request);
            var methodInfo = typeof(OptimizeSystemHelper).GetMethod(method)
                ?? throw new InvalidDataException($"Unknown RyTuneX method: {method}");
            using var errors = ActionLog.BeginErrorCollection();
            await (Task)methodInfo.Invoke(null, null)!;
            return errors.Messages.Count == 0
                ? (0, "", "")
                : (1, "", string.Join(Environment.NewLine, errors.Messages));
        }
        if (item.Kind == "command")
        {
            var nic = request.Inputs.FirstOrDefault();
            var commands = RyTuneXCommandBuilder.Build(item, nic, Environment.SystemDirectory);
            var exitCode = 0;
            var stdout = new List<string>();
            var stderr = new List<string>();
            foreach (var command in commands)
            {
                try
                {
                    (var code, var output, var error) = await RunRyTuneXCommandAsync(item, command);
                    if (code != 0)
                        exitCode = code;
                    stdout.Add(output);
                    stderr.Add(error);
                }
                catch (Exception exception)
                {
                    exitCode = 1;
                    stderr.Add(exception.ToString());
                }
            }
            return (exitCode, string.Join(Environment.NewLine, stdout), string.Join(Environment.NewLine, stderr));
        }
        throw new InvalidDataException($"Unsupported RyTuneX kind: {item.Kind}");
    }

    private async Task<(int ExitCode, string Stdout, string Stderr)> RunRyTuneXCommandAsync(
        CatalogItem item, string command)
    {
        switch (item.Launcher)
        {
            case "StartInCmd":
                var result = await OptimizationOptions.StartInCmd(command);
                return (result, "", "");
            case "Process":
                return await StartRawProcessAsync(
                    item.FileName ?? throw new InvalidDataException("RyTuneX fileName is missing."),
                    command, null, capture: false);
            case "SystemTool":
                var executable = Path.Combine(Environment.SystemDirectory,
                    item.FileName ?? throw new InvalidDataException("RyTuneX fileName is missing."));
                var stdin = item.Id.EndsWith(":CHKDSKScan", StringComparison.Ordinal) ? "N\n" : null;
                return await StartRawProcessAsync(executable, command, stdin, capture: true);
            default:
                throw new InvalidDataException($"Unknown RyTuneX launcher: {item.Launcher}");
        }
    }

    private static async Task<(int ExitCode, string Stdout, string Stderr)> StartProcessAsync(
        string fileName,
        IEnumerable<string> arguments,
        string? standardInput,
        bool capture,
        TimeSpan? timeout = null)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = standardInput is not null,
                RedirectStandardOutput = capture,
                RedirectStandardError = capture
            }
        };
        foreach (var argument in arguments)
            process.StartInfo.ArgumentList.Add(argument);
        process.Start();
        var stdoutTask = capture ? process.StandardOutput.ReadToEndAsync() : Task.FromResult("");
        var stderrTask = capture ? process.StandardError.ReadToEndAsync() : Task.FromResult("");
        if (standardInput is not null)
        {
            await process.StandardInput.WriteAsync(standardInput);
            process.StandardInput.Close();
        }
        using var cts = new CancellationTokenSource(timeout ?? TimeSpan.FromMinutes(30));
        try
        {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            return (124, await stdoutTask, await stderrTask + Environment.NewLine + "Action timed out.");
        }
        return (process.ExitCode, await stdoutTask, await stderrTask);
    }

    private static async Task<(int ExitCode, string Stdout, string Stderr)> StartRawProcessAsync(
        string fileName, string arguments, string? standardInput, bool capture)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = standardInput is not null,
                RedirectStandardOutput = capture,
                RedirectStandardError = capture
            }
        };
        process.Start();
        var stdoutTask = capture ? process.StandardOutput.ReadToEndAsync() : Task.FromResult("");
        var stderrTask = capture ? process.StandardError.ReadToEndAsync() : Task.FromResult("");
        if (standardInput is not null)
        {
            await process.StandardInput.WriteAsync(standardInput);
            process.StandardInput.Close();
        }
        await process.WaitForExitAsync();
        return (process.ExitCode, await stdoutTask, await stderrTask);
    }

    private static string SelectComboMethod(CatalogItem item, ActionRequest request)
    {
        var selectedKey = request.Revert ? "Default" : request.OptionKey;
        var index = item.Options.FindIndex(option => option.Key.Equals(selectedKey, StringComparison.OrdinalIgnoreCase));
        if (index < 0 || index >= item.Methods.Count)
            throw new InvalidDataException("Unknown combo option.");
        return item.Methods[index];
    }
}
