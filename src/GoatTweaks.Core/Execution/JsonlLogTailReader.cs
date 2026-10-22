using System.Text;
using System.Text.Json;

namespace GoatTweaks.Core.Execution;

// Reads appended bytes only. Incomplete UTF-8/JSON records wait for the next newline.
public sealed class JsonlLogTailReader
{
    private const int ReadLimit = 512 * 1024;
    private const int RecordLimit = 1024 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private string? _path;
    private long _position;
    private byte[] _pending = [];
    private bool _skipPartial;

    public async Task<IReadOnlyList<LiveActionLogEntry>> ReadNewAsync(string path)
    {
        if (!File.Exists(path)) return [];
        await using var file = new FileStream(path, FileMode.Open, FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete, 16 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        if (!string.Equals(_path, path, StringComparison.Ordinal) || file.Length < _position)
        {
            _path = path;
            _position = Math.Max(0, file.Length - ReadLimit);
            _pending = [];
            _skipPartial = _position > 0;
        }
        file.Position = _position;
        var length = (int)Math.Min(ReadLimit, Math.Max(0, file.Length - _position));
        if (length == 0) return [];
        var bytes = new byte[_pending.Length + length];
        _pending.CopyTo(bytes, 0);
        var read = 0;
        while (read < length)
        {
            var count = await file.ReadAsync(bytes.AsMemory(_pending.Length + read, length - read)).ConfigureAwait(false);
            if (count == 0) break;
            read += count;
        }
        _position += read;
        var available = _pending.Length + read;
        var rows = new List<LiveActionLogEntry>();
        var start = 0;
        for (var index = 0; index < available; index++)
        {
            if (bytes[index] != (byte)'\n') continue;
            if (_skipPartial) _skipPartial = false;
            else if (index > start)
            {
                try
                {
                    var row = JsonSerializer.Deserialize<LiveActionLogEntry>(
                        Encoding.UTF8.GetString(bytes, start, index - start), JsonOptions);
                    if (row is not null) rows.Add(row);
                }
                catch (JsonException) { }
            }
            start = index + 1;
        }
        _pending = available - start <= RecordLimit ? bytes[start..available] : [];
        if (available - start > RecordLimit) _skipPartial = true;
        return rows;
    }
}

public sealed record LiveActionLogEntry(
    DateTimeOffset Timestamp, string Id, string Source, string Command,
    int? ExitCode, string Stdout, string Stderr, string Event = "completed");
