using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;

namespace GoatTweaks.Core.Catalog;

public sealed class CatalogService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CatalogService(IEnumerable<CatalogItem> items)
    {
        Items = items.ToList();
        ById = Items.ToDictionary(item => item.Id, StringComparer.Ordinal);
    }

    public IReadOnlyList<CatalogItem> Items { get; }
    public IReadOnlyDictionary<string, CatalogItem> ById { get; }

    public static CatalogService Load(string path)
    {
        var document = JsonSerializer.Deserialize<CatalogDocument>(File.ReadAllText(path), JsonOptions)
            ?? throw new InvalidDataException($"Invalid catalog: {path}");
        return new CatalogService(document.Items);
    }

    public static CatalogService LoadEmbedded()
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("GoatTweaks.Core.catalog.json")
            ?? throw new InvalidDataException("Embedded catalog resource is missing.");
        return new CatalogService(JsonSerializer.Deserialize<CatalogDocument>(stream, JsonOptions)?.Items
            ?? throw new InvalidDataException("Embedded catalog is invalid."));
    }

    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        foreach (var item in Items)
        {
            if (string.IsNullOrWhiteSpace(item.Id) || string.IsNullOrWhiteSpace(item.Kind) ||
                string.IsNullOrWhiteSpace(item.Source) || string.IsNullOrWhiteSpace(item.Title))
                errors.Add($"Invalid item metadata: {item.Id}");
            if (!CatalogSections.All.Contains(item.Section, StringComparer.Ordinal))
                errors.Add($"Item has no section: {item.Id}");
        }
        if (ById.Count != Items.Count)
            errors.Add("Catalog item IDs are not unique.");
        return errors;
    }
}

public static class CatalogIntegrity
{
    public static string? ResolveSourcePath(CatalogItem item, string repositoryRoot)
    {
        var sourcePath = item.SourceFile.Split('#', 2)[0].Replace('\\', '/');
        sourcePath = sourcePath.Replace('/', Path.DirectorySeparatorChar);
        return item.Source switch
        {
            "Ultimate" => Path.Combine(repositoryRoot, "third_party", "Ultimate", sourcePath),
            "WinSux" => Path.Combine(repositoryRoot, "third_party", "WinSux", sourcePath),
            "winutil" when item.Sha256 is not null => Path.Combine(repositoryRoot, "third_party", "winutil", sourcePath),
            _ => null
        };
    }

    public static bool Verify(CatalogItem item, string repositoryRoot)
    {
        if (string.IsNullOrWhiteSpace(item.Sha256))
            return true;
        var path = ResolveSourcePath(item, repositoryRoot);
        if (path is null || !File.Exists(path))
            return false;
        var actual = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
        return actual.Equals(item.Sha256, StringComparison.OrdinalIgnoreCase);
    }
}
