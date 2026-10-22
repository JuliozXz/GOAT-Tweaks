using System.Text.Json;

namespace GoatTweaks.Core.Intelligence;

public static class IntelligenceSettings
{
    public const string GameMode = "windows.game-mode";
    public const string BackgroundCapture = "windows.background-capture";
    public const string PowerPlan = "windows.power-plan";
    public const string BalancedGuid = "381b4222-f694-41f0-9685-ff5bb260df2e";
    public const string PowerSaverGuid = "a1841308-3541-4fab-bc81-f71556f20b4a";
    public static readonly string[] Managed = [GameMode, BackgroundCapture, PowerPlan];
}

public static class KnowledgeBase
{
    private static KnowledgeEntry Entry(string id, string name, string technical, string category, string objective,
        string evidence, string adverse, string apply, string restore, int level = 2) => new()
    {
        Id = id, Name = name, TechnicalName = technical, Category = category, Description = objective, Objective = objective,
        EvidenceSources = [evidence], AdverseEffects = adverse, Application = apply, Restoration = restore,
        AutomationLevel = level, Compatibility = "Windows 10 22H2 / Windows 11; disponibilidade verificada localmente",
        Reliability = "Mecanismo verificável; benefício no jogo depende de comparação controlada"
    };
    public static IReadOnlyList<KnowledgeEntry> Native { get; } =
    [
        Entry(IntelligenceSettings.GameMode, "Avaliar o Modo de Jogo", "AutoGameModeEnabled", "Jogos",
            "Habilitar a preferência de Modo de Jogo somente quando ela estiver explicitamente desativada.",
            "https://support.xbox.com/help/games-apps/game-setup-and-play/use-game-mode-gaming-on-pc",
            "O comportamento depende do Windows e do jogo. Não há garantia de ganho de FPS.",
            "HKCU\\Software\\Microsoft\\GameBar: AutoGameModeEnabled = DWORD 1",
            "Restaurar existência, tipo e valor originais do valor do Registro."),
        Entry(IntelligenceSettings.BackgroundCapture, "Avaliar captura de jogos em segundo plano", "HistoricalCaptureEnabled", "Jogos",
            "Desativar a gravação histórica quando estiver habilitada e o técnico dispensar essa função.",
            "https://support.xbox.com/help/games-apps/game-setup-and-play/adjust-capture-settings-windows-10",
            "A gravação retroativa deixa de estar disponível. A redução de custo só existe se a captura estiver sendo usada.",
            "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\GameDVR: HistoricalCaptureEnabled = DWORD 0",
            "Restaurar existência, tipo e valor originais do valor do Registro."),
        Entry(IntelligenceSettings.PowerPlan, "Revisar economia de energia no desktop", "PowerSetActiveScheme / powercfg", "Energia",
            "Oferecer o plano Equilibrado se um desktop na tomada estiver usando explicitamente Economia de energia.",
            "https://learn.microsoft.com/windows/win32/power/power-schemes",
            "O consumo e o aquecimento podem aumentar. Não substitui limites térmicos e não altera políticas OEM de notebooks.",
            "Ativar somente o GUID Equilibrado já instalado; sem importar ou modificar planos.",
            "Reativar o GUID exato do plano anterior, preservado no diário."),
        Entry("diagnosis.performance", "Analisar limitações de desempenho", "Windows performance counters / ETW", "Diagnóstico",
            "Correlacionar amostras por processador lógico, GPU 3D, memória e disco; pedir teste direcionado quando faltar contexto.",
            "https://learn.microsoft.com/windows-hardware/test/wpt/windows-performance-recorder",
            "A própria coleta pode influenciar a carga; gravações são delimitadas e não rodam continuamente por padrão.",
            "Leitura de métricas disponíveis. Sem alterações de configuração.", "Não altera configurações.", 1),
        Entry("test.frametimes", "Medir apresentação de quadros", "PresentMon CSV / ETW", "Validação",
            "Importar intervalos de apresentação reais ou capturar com PresentMon instalado, sem injetar código no jogo.",
            "https://github.com/GameTechDev/PresentMon/blob/main/README-ConsoleApplication.md",
            "Requer compatibilidade da ferramenta, permissões de coleta e condições de teste declaradas. Um teste não prova causalidade.",
            "CSV MsBetweenPresents; FPS = 1000 / média dos intervalos. 1% low = 1000 / média do 1% de intervalos mais lentos.",
            "A coleta não altera o jogo. Encerrar a captura iniciada pelo GOAT.", 1),
        Entry("manual.firmware", "BIOS, firmware e voltagem", "UEFI / OC / UV", "Manual obrigatório",
            "Somente revisão manual com informações do fabricante; não executar alterações automaticamente.",
            "https://learn.microsoft.com/windows-hardware/drivers/bringup/uefi-firmware",
            "Pode comprometer inicialização, estabilidade e hardware. Reversão depende do fabricante e não é garantida.",
            "Nenhum comando automático. Intervenção do técnico fora do motor.", "Consultar procedimento e backup específicos do fabricante.", 3),
        Entry("manual.security", "Mecanismos de segurança", "Defender / SmartScreen / UAC / VBS", "Manual obrigatório",
            "Preservar proteções; não recomendar desativação como ajuste genérico de desempenho.",
            "https://learn.microsoft.com/windows/security/",
            "Reduz proteção do Windows e pode afetar compatibilidade de jogos e políticas da organização.",
            "Não executável pelo GOAT Intelligence. Ferramentas legadas continuam identificadas para análise manual.",
            "Não há reversão central garantida para scripts legados; revisar cada implementação.", 3)
    ];

    public static IReadOnlyList<KnowledgeEntry> LoadAll()
    {
        using var stream = typeof(KnowledgeBase).Assembly.GetManifestResourceStream("GoatTweaks.Core.intelligence-inventory.json")
            ?? throw new InvalidDataException("Inventário legado não incorporado ao motor.");
        var legacy = JsonSerializer.Deserialize<List<KnowledgeEntry>>(stream) ?? throw new InvalidDataException("Inventário inválido.");
        return Native.Concat(legacy).ToArray();
    }
}
