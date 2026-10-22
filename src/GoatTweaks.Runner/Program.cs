using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Text.Json;
using GoatTweaks.Core.Assets;
using GoatTweaks.Core.Catalog;
using GoatTweaks.Core.Conflicts;
using GoatTweaks.Core.Execution;
using RyTuneX.Helpers;

namespace GoatTweaks.Runner;

internal static class Program
{
    private static async Task<int> Main(string[] args)
        => await RunnerEntry.RunAsync(args);
}

public static class RunnerEntry
{
    private const string BatchAssetRootVariable = "GOATTWEAKS_BATCH_ASSET_ROOT";

    public static async Task<int> RunAsync(string[] args)
    {
        try { return await RunCoreAsync(args); }
        catch (Exception exception)
        {
            var path = args.Length >= 3 ? args[2] : GoatPaths.NewLogPath();
            try
            {
                await new JsonlLogWriter(path).AppendAsync(new ActionLogEntry(DateTimeOffset.Now,
                    "runner:failed", "GOAT Tweaks", "Execução interrompida antes de completar a ação",
                    1, "", JsonlLogWriter.Tail(exception.ToString())));
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return 1;
        }
    }

    private static async Task<int> RunCoreAsync(string[] args)
    {
        if (args.Length == 5 && args[3] == "--sha256")
        {
            if (args[0] == "--runner") return await RunBatchAsync(args[1], args[2], args[4]);
            if (args[0] == "--action") return await RunActionAsync(args[1], args[2], args[4]);
        }
        if (args.Length is 2 or 3 && args[0] == "--runner")
            return await RunBatchAsync(args[1], args.Length == 3 ? args[2] : null);

        if (args.Length == 3 && args[0] == "--action")
            return await RunActionAsync(args[1], args[2]);

        return 2;
    }

    private static async Task<int> RunBatchAsync(string batchPath, string? requestedLogPath, string? expectedHash = null)
    {
        var bytes = await ReadRequestAsync(batchPath, expectedHash);
        var batch = JsonSerializer.Deserialize<ActionBatch>(bytes)
            ?? throw new InvalidDataException("Invalid action batch.");
        var logPath = requestedLogPath ?? Environment.GetEnvironmentVariable("GOATTWEAKS_ACTION_LOG") ?? GoatPaths.NewLogPath();
        Environment.SetEnvironmentVariable("GOATTWEAKS_ACTION_LOG", logPath);
        var runnerPath = Environment.ProcessPath ?? throw new InvalidOperationException("Runner path is unavailable.");
        var catalog = CatalogService.LoadEmbedded();
        var log = new JsonlLogWriter(logPath);
        var ids = batch.Actions.Select(action => action.ItemId).ToList();
        var conflicts = ConflictDetector.LoadEmbedded();
        if (ids.Count != ids.Distinct(StringComparer.Ordinal).Count() ||
            conflicts.FindExclusiveViolation(ids) is not null ||
            conflicts.FindUserChoiceConflicts(ids).Count > 0)
        {
            await log.AppendAsync(new ActionLogEntry(DateTimeOffset.Now, "batch:blocked", "GOAT Tweaks",
                "Fila bloqueada antes da execução", 1, "", "Há ações duplicadas ou conflitantes na fila."));
            return 1;
        }
        foreach (var request in batch.Actions)
        {
            if (!catalog.ById.TryGetValue(request.ItemId, out var selected))
                throw new InvalidDataException($"Unknown catalog item: {request.ItemId}");
            ActionSafetyPolicy.Validate(selected, request);
        }
        if (!IsElevated())
        {
            foreach (var request in batch.Actions)
            {
                var item = catalog.ById.GetValueOrDefault(request.ItemId);
                await log.AppendAsync(new ActionLogEntry(DateTimeOffset.Now, request.ItemId, item?.Source ?? "unknown",
                    "Runner requires administrator privileges", 1, "", "O Runner não está elevado."));
            }
            return 1;
        }

        var assetRoot = CreateProtectedAssetRoot();
        try
        {
            EmbeddedAssetStore.MaterializeThirdParty(assetRoot);
            var failed = await ActionBatchIsolation.RunAsync(batch.Actions, async request =>
            {
                var requestPath = Path.Combine(assetRoot, $"goat-action-{Guid.NewGuid():N}.json");
                try
                {
                    var requestBytes = JsonSerializer.SerializeToUtf8Bytes(request);
                    await File.WriteAllBytesAsync(requestPath, requestBytes);
                    var info = new ProcessStartInfo(runnerPath) { UseShellExecute = false };
                    info.ArgumentList.Add("--action");
                    info.ArgumentList.Add(requestPath);
                    info.ArgumentList.Add(logPath);
                    info.ArgumentList.Add("--sha256");
                    info.ArgumentList.Add(Convert.ToHexString(SHA256.HashData(requestBytes)));
                    info.Environment.Remove("GOATTWEAKS_ASSET_ROOT");
                    info.Environment[BatchAssetRootVariable] = assetRoot;
                    info.Environment["GOATTWEAKS_ACTION_LOG"] = logPath;
                    using var process = Process.Start(info)
                        ?? throw new InvalidOperationException("Could not start action child process.");
                    await process.WaitForExitAsync();
                    return process.ExitCode;
                }
                finally
                {
                    if (File.Exists(requestPath))
                        File.Delete(requestPath);
                }
            }, async (request, exception) =>
            {
                var item = catalog.ById.GetValueOrDefault(request.ItemId);
                await log.AppendAsync(new ActionLogEntry(DateTimeOffset.Now, request.ItemId, item?.Source ?? "unknown",
                    "Runner child process failed", 1, "", exception.Message));
            });
            return failed == 0 ? 0 : 1;
        }
        finally
        {
            if (Directory.Exists(assetRoot))
                Directory.Delete(assetRoot, recursive: true);
        }
    }

    private static async Task<int> RunActionAsync(string requestPath, string logPath, string? expectedHash = null)
    {
        var request = JsonSerializer.Deserialize<ActionRequest>(await ReadRequestAsync(requestPath, expectedHash))
            ?? throw new InvalidDataException("Invalid action request.");
        var log = new JsonlLogWriter(logPath);
        var catalog = CatalogService.LoadEmbedded();
        var elevated = IsElevated();
        var batchRoot = elevated ? Environment.GetEnvironmentVariable(BatchAssetRootVariable) : null;
        var ownsRoot = elevated && string.IsNullOrWhiteSpace(batchRoot);
        string? root = null;
        try
        {
            root = elevated
                ? batchRoot ?? CreateProtectedAssetRoot()
                : Environment.GetEnvironmentVariable("GOATTWEAKS_ASSET_ROOT")
                    ?? EmbeddedAssetStore.MaterializeThirdParty(Path.Combine(GoatPaths.LocalRoot, "assets"));
            if (elevated && batchRoot is not null)
            {
                var trustedRoot = Path.GetFullPath(Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "GoatTweaks", "assets"))
                    + Path.DirectorySeparatorChar;
                root = Path.GetFullPath(root);
                if (!root.StartsWith(trustedRoot, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Elevated action asset root is outside the protected directory.");
            }
            if (ownsRoot)
                EmbeddedAssetStore.MaterializeThirdParty(root);
            var executor = new ActionExecutor(catalog, log, root!);
            var entry = await executor.ExecuteAsync(request);
            await log.AppendAsync(entry);
            return entry.ExitCode;
        }
        catch (Exception exception)
        {
            var item = catalog.ById.GetValueOrDefault(request.ItemId);
            var entry = new ActionLogEntry(DateTimeOffset.Now, request.ItemId, item?.Source ?? "unknown",
                "Action failed", 1, "", JsonlLogWriter.Tail(exception.ToString()));
            await log.AppendAsync(entry);
            return 1;
        }
        finally
        {
            if (ownsRoot && root is not null && Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static bool IsElevated() =>
        new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

    private static async Task<byte[]> ReadRequestAsync(string path, string? expectedHash)
    {
        var bytes = await File.ReadAllBytesAsync(path);
        if (expectedHash is not null &&
            !Convert.ToHexString(SHA256.HashData(bytes)).Equals(expectedHash, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("A fila foi alterada depois da revisão. Nenhuma ação dessa solicitação foi iniciada.");
        return bytes;
    }

    private static string CreateProtectedAssetRoot()
    {
        var appRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "GoatTweaks");
        var assetsRoot = Path.Combine(appRoot, "assets");
        var root = Path.Combine(assetsRoot, Guid.NewGuid().ToString("N"));
        var security = new DirectorySecurity();
        security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
        var inheritance = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;
        security.AddAccessRule(new FileSystemAccessRule(
            new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
            FileSystemRights.FullControl, inheritance, PropagationFlags.None, AccessControlType.Allow));
        security.AddAccessRule(new FileSystemAccessRule(
            new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null),
            FileSystemRights.FullControl, inheritance, PropagationFlags.None, AccessControlType.Allow));
        Directory.CreateDirectory(appRoot);
        new DirectoryInfo(appRoot).SetAccessControl(security);
        Directory.CreateDirectory(assetsRoot);
        new DirectoryInfo(assetsRoot).SetAccessControl(security);
        Directory.CreateDirectory(root);
        new DirectoryInfo(root).SetAccessControl(security);
        return root;
    }
}
