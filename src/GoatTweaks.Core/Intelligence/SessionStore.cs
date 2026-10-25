using System.Text.Json;

namespace GoatTweaks.Core.Intelligence;

public interface ISessionJournal
{
    Task SaveAsync(ServiceSession session, CancellationToken cancellationToken = default);
    IDisposable AcquireMutationLock();
}

/// <summary>Write-ahead state with durable atomic replacement. A damaged file is never silently replaced by a new session.</summary>
public sealed class SessionStore(string directory) : ISessionJournal
{
    public string DirectoryPath { get; } = Path.GetFullPath(directory);
    public static JsonSerializerOptions JsonOptions { get; } = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
    public IDisposable AcquireMutationLock()
    {
        Directory.CreateDirectory(DirectoryPath);
        try { return new FileStream(Path.Combine(DirectoryPath, "mutation.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
        catch (IOException exception) { throw new IOException("Outro processo já está modificando ou restaurando configurações. Aguarde sua conclusão.", exception); }
    }
    public async Task SaveAsync(ServiceSession session, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(DirectoryPath);
        session.UpdatedAt = DateTimeOffset.UtcNow;
        var path = Path.Combine(DirectoryPath, session.Id.ToString("N") + ".json");
        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 16384, FileOptions.WriteThrough))
            {
                await JsonSerializer.SerializeAsync(stream, session, JsonOptions, cancellationToken);
                await stream.FlushAsync(cancellationToken);
                stream.Flush(flushToDisk: true);
            }
            File.Move(temporary, path, overwrite: true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
    public Task DeleteAsync(Guid id, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (id == Guid.Empty) throw new ArgumentException("A identidade do atendimento é inválida.", nameof(id));
        var path = Path.Combine(DirectoryPath, id.ToString("N") + ".json");
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    public async Task<ServiceSession> LoadAsync(Guid id, CancellationToken token = default)
    {
        var path = Path.Combine(DirectoryPath, id.ToString("N") + ".json");
        // Keep a consistent old handle while an atomic journal replacement completes.
        await using var stream = new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete);
        var session = await JsonSerializer.DeserializeAsync<ServiceSession>(stream, JsonOptions, token)
            ?? throw new InvalidDataException("Atendimento vazio ou inválido.");
        if (session.SchemaVersion != 1 || session.Id != id) throw new InvalidDataException("Versão ou identidade de atendimento incompatível.");
        return session;
    }
    public async Task<(List<ServiceSession> Sessions, List<string> Errors)> ListAsync(CancellationToken token = default)
    {
        var sessions = new List<ServiceSession>(); var errors = new List<string>();
        if (!Directory.Exists(DirectoryPath)) return (sessions, errors);
        foreach (var path in Directory.EnumerateFiles(DirectoryPath, "*.json").OrderByDescending(File.GetLastWriteTimeUtc).Take(100))
        {
            if (!Guid.TryParseExact(Path.GetFileNameWithoutExtension(path), "N", out var id)) continue;
            try { sessions.Add(await LoadAsync(id, token)); }
            catch (Exception ex) when (ex is JsonException or IOException or InvalidDataException or UnauthorizedAccessException)
            { errors.Add($"{Path.GetFileName(path)}: {ex.Message}"); }
        }
        return (sessions, errors);
    }
}
