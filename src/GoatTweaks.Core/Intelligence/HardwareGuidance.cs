namespace GoatTweaks.Core.Intelligence;

/// <summary>Hardware-specific review guidance. A model string is never treated as proof of a bottleneck or outdated driver.</summary>
public static class HardwareGuidance
{
    public static string DriverReview(MachineFacts machine)
    {
        var sources=new List<string>();
        var names=string.Join(" ",machine.Gpus);
        if(names.Contains("NVIDIA",StringComparison.OrdinalIgnoreCase)) sources.Add("NVIDIA: https://www.nvidia.com/Download/index.aspx");
        if(names.Contains("AMD",StringComparison.OrdinalIgnoreCase) || names.Contains("Radeon",StringComparison.OrdinalIgnoreCase)) sources.Add("AMD Graphics: https://www.amd.com/en/support");
        if(names.Contains("Intel",StringComparison.OrdinalIgnoreCase)) sources.Add("Intel Graphics: https://www.intel.com/content/www/us/en/download-center/home.html");
        if(machine.Cpu.Contains("AMD",StringComparison.OrdinalIgnoreCase)) sources.Add("Chipset AMD: conferir modelo da placa-mãe e suporte AMD/OEM; CPU não possui um driver de FPS universal.");
        if(machine.Cpu.Contains("Intel",StringComparison.OrdinalIgnoreCase)) sources.Add("Chipset Intel: conferir modelo da placa-mãe e suporte Intel/OEM; não instalar pacote apenas pelo nome da CPU.");
        if(machine.Notebook==true) sources.Insert(0,"Notebook "+machine.ComputerModel+": priorizar drivers e limites do fabricante; confirmar compatibilidade antes de trocar o pacote gráfico.");
        if(sources.Count==0) sources.Add("Fabricante não confirmado: consultar o suporte do modelo antes de instalar drivers.");
        return string.Join("\n",sources)+"\nAs versões instaladas são inventariadas. Este diagnóstico não consultou uma base de versões e não afirma que estejam desatualizadas.";
    }
}
