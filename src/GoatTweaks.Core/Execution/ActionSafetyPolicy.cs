using GoatTweaks.Core.Catalog;

namespace GoatTweaks.Core.Execution;

public static class ActionSafetyPolicy
{
    private static readonly HashSet<string> AdvancedFlags = new(StringComparer.OrdinalIgnoreCase)
    {
        "boot", "bitlocker", "drivers", "security", "trustedInstaller",
        "fileDeletion", "reboot", "updates", "services", "appx", "features"
    };

    public static bool RequiresAdvancedMode(CatalogItem item) =>
        item.Source.Equals("WinSux", StringComparison.OrdinalIgnoreCase) ||
        item.Source.Equals("Ultimate", StringComparison.OrdinalIgnoreCase) && item.RequiresAdmin ||
        item.Risk.Equals("high", StringComparison.OrdinalIgnoreCase) ||
        item.Flags.Any(AdvancedFlags.Contains) ||
        item.Kind.Equals("appx", StringComparison.OrdinalIgnoreCase);

    public static bool RequiresConfirmation(CatalogItem item) => RequiresAdvancedMode(item);

    public static string RestrictionReason(CatalogItem item) =>
        item.Source.Equals("WinSux", StringComparison.OrdinalIgnoreCase)
            ? "Fluxo destrutivo: reinicializa o Windows, altera a segurança e não tem reversão automática."
            : "Ação de impacto elevado ou com mudanças em componentes sensíveis do Windows.";

    public static void Validate(CatalogItem item, ActionRequest request)
    {
        if (RequiresAdvancedMode(item) && !request.AdvancedModeEnabled)
            throw new InvalidOperationException(
                "Ação bloqueada no modo cliente. Habilite o modo avançado e revise a prévia.");
        if (RequiresConfirmation(item) &&
            !string.Equals(request.Confirmation, "CONFIRMAR", StringComparison.Ordinal))
            throw new InvalidOperationException("Esta ação exige a confirmação digitada CONFIRMAR.");
    }
}
