namespace GoatTweaks.Core.Execution;

public sealed class ActionRequest
{
    public string ItemId { get; set; } = "";
    public string? OptionKey { get; set; }
    public List<string> Inputs { get; set; } = [];
    public bool Revert { get; set; }
    public bool ConsoleWindow { get; set; }
    public bool? Enabled { get; set; }
    public bool AdvancedModeEnabled { get; set; }
    public string? Confirmation { get; set; }
    public string? WinSuxConfirmation { get; set; }
}

public sealed class ActionBatch
{
    public List<ActionRequest> Actions { get; set; } = [];
}

public sealed record ActionLogEntry(
    DateTimeOffset Timestamp,
    string Id,
    string Source,
    string Command,
    int ExitCode,
    string Stdout,
    string Stderr);

public static class GoatPaths
{
    public static string LocalRoot =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GoatTweaks");

    public static string Logs => Path.Combine(LocalRoot, "logs");

    public static string NewLogPath() =>
        Path.Combine(Logs, $"run-{DateTimeOffset.Now:yyyyMMdd-HHmmss-fff}.jsonl");
}
