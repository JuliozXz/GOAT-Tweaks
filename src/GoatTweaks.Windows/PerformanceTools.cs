using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using GoatTweaks.Core.Intelligence;

namespace GoatTweaks.Windows;

public static class PerformanceTools
{
    public static string? FindWpr() => Existing([ProcessCommand.SystemTool("wpr.exe"), .. ToolkitPaths("wpr.exe")]);
    public static string? FindWpa() => Existing(ToolkitPaths("wpa.exe"));
    private static IEnumerable<string> ToolkitPaths(string file) => new[] { Environment.SpecialFolder.ProgramFiles, Environment.SpecialFolder.ProgramFilesX86 }
        .Select(p=>Path.Combine(Environment.GetFolderPath(p),"Windows Kits","10","Windows Performance Toolkit",file));
    private static string? Existing(IEnumerable<string> paths) => paths.FirstOrDefault(File.Exists);

    public static async Task<string> CapturePresentMonAsync(string executable,string processName,string destination,TimeSpan duration,IProgress<string>? progress,CancellationToken token)
    {
        if(!File.Exists(executable) || !Path.GetFileName(executable).StartsWith("PresentMon",StringComparison.OrdinalIgnoreCase) ||
            !Path.GetExtension(executable).Equals(".exe",StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Selecione o executável de console PresentMon instalado a partir da fonte oficial.");
        if(!Regex.IsMatch(processName,@"^[\p{L}\p{N}_][\p{L}\p{N}_. -]{0,126}\.exe$",RegexOptions.IgnoreCase))
            throw new InvalidDataException("Informe somente o nome do executável do jogo, por exemplo cs2.exe.");
        if(duration.TotalSeconds is < 10 or > 120) throw new ArgumentOutOfRangeException(nameof(duration));
        Directory.CreateDirectory(destination);
        var file=Path.Combine(destination,"presentmon-"+Guid.NewGuid().ToString("N")+".csv");
        progress?.Report("Capturando ETW com PresentMon. Mantenha a cena do jogo nas condições registradas; nenhum código é injetado no processo.");
        var result=await ProcessCommand.RunAsync(executable,["--process_name",processName,"--timed",((int)duration.TotalSeconds).ToString(CultureInfo.InvariantCulture),
            "--terminate_after_timed","--no_console_stats","--v1_metrics","--session_name","GOAT_"+Guid.NewGuid().ToString("N"),"--output_file",file],duration+TimeSpan.FromSeconds(30),token);
        if(result.ExitCode!=0 || !File.Exists(file)) throw new IOException("PresentMon não gerou uma captura válida. Verifique versão, permissões ETW e compatibilidade com o jogo. "+result.Error+result.Output);
        return file;
    }

    public static async Task<string> CaptureWprAsync(string destination,TimeSpan duration,IProgress<string>? progress,CancellationToken token)
    {
        var executable=FindWpr()??throw new FileNotFoundException("WPR não está instalado. Instale o Windows Performance Toolkit pelo Windows ADK oficial.");
        if(duration.TotalSeconds is < 10 or > 120) throw new ArgumentOutOfRangeException(nameof(duration));
        Directory.CreateDirectory(destination);
        var instance="GOAT_"+Guid.NewGuid().ToString("N");
        var path=Path.Combine(destination,instance+".etl");
        var started=false;
        try
        {
            // Named instance is mandatory. Older WPR versions that reject it are not used.
            var start=await ProcessCommand.RunAsync(executable,["-start","CPU.light","-instancename",instance],TimeSpan.FromSeconds(15),token);
            if(start.ExitCode!=0) throw new IOException("WPR não iniciou uma instância independente. Verifique instalação, versão e permissões. "+start.Error+start.Output);
            started=true;
            progress?.Report("WPR · captura CPU.light em memória, delimitada. O rastreio precisa ser analisado no WPA; não representa um benchmark de FPS.");
            await Task.Delay(duration,token);
            var stop=await ProcessCommand.RunAsync(executable,["-stop",path,"GOAT TWEAKS - captura autorizada pelo técnico","-skipPdbGen","-instancename",instance],TimeSpan.FromSeconds(50),CancellationToken.None);
            if(stop.ExitCode!=0 || !File.Exists(path)) throw new IOException("WPR não salvou o rastreio. "+stop.Error+stop.Output);
            started=false;
            return path;
        }
        finally
        {
            // Never cancel an unrelated recording or change WPR's global sampling configuration.
            if(started)
                try { await ProcessCommand.RunAsync(executable,["-cancel","-instancename",instance],TimeSpan.FromSeconds(15),CancellationToken.None); }
                catch(Exception ex) when(ex is IOException or OperationCanceledException or System.ComponentModel.Win32Exception) { }
        }
    }
    public static void OpenWpa(string? trace=null)
    {
        var executable=FindWpa()??throw new FileNotFoundException("WPA não está instalado. Instale o Windows Performance Toolkit oficial.");
        var start=new ProcessStartInfo(executable){UseShellExecute=false};
        if(trace is not null)
        {
            if(!File.Exists(trace) || !Path.GetExtension(trace).Equals(".etl",StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Selecione um arquivo ETL existente.");
            start.ArgumentList.Add(trace);
        }
        Process.Start(start);
    }
}
