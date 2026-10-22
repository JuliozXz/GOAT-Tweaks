using System.Diagnostics;
using System.Text;

namespace GoatTweaks.Windows;

public sealed record CommandResult(int ExitCode, string Output, string Error);
public static class ProcessCommand
{
    public static async Task<CommandResult> RunAsync(string executable, IEnumerable<string> arguments, TimeSpan limit, CancellationToken token)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(limit);
        var start = new ProcessStartInfo(executable)
        {
            UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8
        };
        foreach (var argument in arguments) start.ArgumentList.Add(argument);
        if(Path.GetFileName(executable).Equals("powershell.exe",StringComparison.OrdinalIgnoreCase))
            start.Environment["PSModulePath"]=SystemTool(Path.Combine("WindowsPowerShell","v1.0","Modules"));
        using var process = Process.Start(start) ?? throw new IOException("Não foi possível iniciar a ferramenta local.");
        var stdout = process.StandardOutput.ReadToEndAsync(CancellationToken.None);
        var stderr = process.StandardError.ReadToEndAsync(CancellationToken.None);
        try { await process.WaitForExitAsync(timeout.Token); }
        finally
        {
            if (!process.HasExited)
            {
                try { process.Kill(entireProcessTree: true); } catch (InvalidOperationException) { }
                await process.WaitForExitAsync(CancellationToken.None);
            }
        }
        return new(process.ExitCode, await stdout, await stderr);
    }
    public static string SystemTool(string name) => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), name);
    public static Task<CommandResult> PowerShellAsync(string trustedScript, TimeSpan timeout, CancellationToken token) =>
        RunAsync(SystemTool(Path.Combine("WindowsPowerShell", "v1.0", "powershell.exe")),
            ["-NoLogo", "-NoProfile", "-NonInteractive", "-EncodedCommand", Convert.ToBase64String(Encoding.Unicode.GetBytes(trustedScript))], timeout, token);
}
