namespace GoatTweaks.Core.Execution;

public static class UltimateRunPolicy
{
    public static bool UseConsoleWindow(bool consoleRequired, bool requested) =>
        consoleRequired || requested;

    public static int TimeoutMinutes(string? configured) =>
        int.TryParse(configured, out var minutes) ? Math.Max(1, minutes) : 120;
}
