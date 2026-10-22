using System.Text.Json;

namespace GoatTweaks.Runner.Logging;

public static class ActionLog
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static readonly AsyncLocal<List<string>?> CurrentErrors = new();

    public static ErrorCollection BeginErrorCollection()
    {
        var previous = CurrentErrors.Value;
        var messages = new List<string>();
        CurrentErrors.Value = messages;
        return new ErrorCollection(messages, previous);
    }

    public static async Task WriteUpstreamAsync(string level, string message, string caller, string file)
    {
        if (level.Equals("Error", StringComparison.OrdinalIgnoreCase))
            CurrentErrors.Value?.Add(message);

        var path = Environment.GetEnvironmentVariable("GOATTWEAKS_ACTION_LOG");
        if (string.IsNullOrWhiteSpace(path))
        {
            Console.Error.WriteLine($"[{level}] {caller} ({file}): {message}");
            return;
        }

        var entry = new
        {
            timestamp = DateTimeOffset.Now,
            id = Environment.GetEnvironmentVariable("GOATTWEAKS_ACTION_ID"),
            source = "RyTuneX",
            command = $"LogHelper.{level} [{caller}]",
            exitCode = (int?)null,
            stdout = "",
            stderr = $"{message} ({file})"
        };

        await Gate.WaitAsync();
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.AppendAllTextAsync(path, JsonSerializer.Serialize(entry) + Environment.NewLine);
        }
        finally
        {
            Gate.Release();
        }
    }

    public sealed class ErrorCollection(List<string> messages, List<string>? previous) : IDisposable
    {
        public IReadOnlyList<string> Messages => messages;

        public void Dispose() => CurrentErrors.Value = previous;
    }
}
