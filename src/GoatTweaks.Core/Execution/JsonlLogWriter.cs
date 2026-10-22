using System.Text.Json;

namespace GoatTweaks.Core.Execution;

public sealed class JsonlLogWriter
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public JsonlLogWriter(string path) => Path = path;

    public string Path { get; }

    public async Task AppendAsync(ActionLogEntry entry, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);
        await Gate.WaitAsync(cancellationToken);
        try
        {
            await File.AppendAllTextAsync(
                Path,
                JsonSerializer.Serialize(entry, JsonOptions) + Environment.NewLine,
                cancellationToken);
        }
        finally
        {
            Gate.Release();
        }
    }

    public async Task<IReadOnlyList<ActionLogEntry>> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(Path))
            return [];

        var entries = new List<ActionLogEntry>();
        foreach (var line in await File.ReadAllLinesAsync(Path, cancellationToken))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            try
            {
                var entry = JsonSerializer.Deserialize<ActionLogEntry>(line, JsonOptions);
                if (entry is not null)
                    entries.Add(entry);
            }
            catch (JsonException)
            {
            }
        }
        return entries;
    }

    public static string Tail(string? value, int maxLength = 4000) =>
        string.IsNullOrEmpty(value) || value.Length <= maxLength ? value ?? "" : value[^maxLength..];
}
