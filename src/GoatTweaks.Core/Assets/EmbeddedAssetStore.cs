using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace GoatTweaks.Core.Assets;

public static class EmbeddedAssetStore
{
    private const string ResourcePrefix = "GoatTweaks.Core.Assets.";

    public static string MaterializeThirdParty(string assetRoot)
    {
        var assembly = typeof(EmbeddedAssetStore).Assembly;
        var resources = assembly.GetManifestResourceNames()
            .Where(name => Normalize(name).StartsWith(ResourcePrefix + "third_party/", StringComparison.Ordinal))
            .ToList();
        var manifestName = resources.FirstOrDefault(name =>
            Normalize(name).EndsWith("third_party/SHA256SUMS", StringComparison.Ordinal))
            ?? throw new InvalidDataException("Embedded third-party checksum manifest is missing.");
        var harnessName = assembly.GetManifestResourceNames().FirstOrDefault(name =>
            name.Equals("GoatTweaks.Core.Assets.Harness.winutil-harness.ps1", StringComparison.Ordinal))
            ?? throw new InvalidDataException("Embedded winutil harness is missing.");
        using var manifest = assembly.GetManifestResourceStream(manifestName)
            ?? throw new InvalidDataException("Embedded third-party checksum manifest cannot be read.");
        using var harnessVersion = assembly.GetManifestResourceStream(harnessName)
            ?? throw new InvalidDataException("Embedded winutil harness cannot be read.");
        using var manifestBuffer = new MemoryStream();
        manifest.CopyTo(manifestBuffer);
        var manifestBytes = manifestBuffer.ToArray();
        using var harnessBuffer = new MemoryStream();
        harnessVersion.CopyTo(harnessBuffer);
        var harnessBytes = harnessBuffer.ToArray();
        var version = Convert.ToHexString(SHA256.HashData(
            SHA256.HashData(manifestBytes).Concat(SHA256.HashData(harnessBytes)).ToArray()));
        var marker = Path.Combine(assetRoot, ".assets-version");
        var thirdPartyRoot = Path.Combine(assetRoot, "third_party");
        if (Directory.Exists(thirdPartyRoot) && AssetsMatch(thirdPartyRoot, manifestBytes, harnessBytes) &&
            File.Exists(marker) && File.ReadAllText(marker).Equals(version, StringComparison.Ordinal))
            return assetRoot;

        Directory.CreateDirectory(thirdPartyRoot);
        foreach (var resourceName in resources)
        {
            var normalized = Normalize(resourceName);
            var relative = normalized[(ResourcePrefix.Length + "third_party/".Length)..]
                .Replace('/', Path.DirectorySeparatorChar);
            var destination = Path.GetFullPath(Path.Combine(thirdPartyRoot, relative));
            var fullRoot = Path.GetFullPath(thirdPartyRoot) + Path.DirectorySeparatorChar;
            if (!destination.StartsWith(fullRoot, StringComparison.Ordinal))
                throw new InvalidDataException($"Invalid embedded asset path: {resourceName}");

            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            using var source = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidDataException($"Embedded asset cannot be read: {resourceName}");
            using var output = File.Create(destination);
            source.CopyTo(output);
        }
        var harnessPath = Path.Combine(thirdPartyRoot, "winutil", "harness.ps1");
        Directory.CreateDirectory(Path.GetDirectoryName(harnessPath)!);
        using (var source = assembly.GetManifestResourceStream(harnessName)
            ?? throw new InvalidDataException("Embedded winutil harness cannot be read."))
        using (var output = File.Create(harnessPath))
            source.CopyTo(output);
        File.WriteAllText(marker, version);
        return assetRoot;
    }

    private static string Normalize(string value) => value.Replace('\\', '/');

    private static bool AssetsMatch(string root, byte[] manifest, byte[] harness)
    {
        var expected = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var line in Encoding.UTF8.GetString(manifest).Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.Trim().Length == 0) continue;
            if (line.Length < 67) throw new InvalidDataException("Manifesto de recursos inválido.");
            expected[line[66..].TrimEnd('\r').TrimStart('*').Replace("./", "", StringComparison.Ordinal)] = line[..64];
        }
        expected["SHA256SUMS"] = Convert.ToHexString(SHA256.HashData(manifest));
        expected["winutil/harness.ps1"] = Convert.ToHexString(SHA256.HashData(harness));
        foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            if (!expected.ContainsKey(relative))
                throw new InvalidDataException("Os recursos locais contêm arquivos inesperados. Reinstale a versão oficial.");
        }
        var fullRoot = Path.GetFullPath(root) + Path.DirectorySeparatorChar;
        foreach (var (relative, hash) in expected)
        {
            var file = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
            if (!file.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Caminho de recurso inválido.");
            if (!File.Exists(file) ||
                !Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file))).Equals(hash, StringComparison.OrdinalIgnoreCase))
                return false;
        }
        return true;
    }
}
