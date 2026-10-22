using System.Text.Json.Serialization;

namespace GoatTweaks.Core.Catalog;

public sealed class CatalogDocument
{
    [JsonPropertyName("items")]
    public List<CatalogItem> Items { get; set; } = [];
}

public sealed class CatalogItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("source")]
    public string Source { get; set; } = "";

    [JsonPropertyName("sourceFile")]
    public string SourceFile { get; set; } = "";

    [JsonPropertyName("sha256")]
    public string? Sha256 { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; } = "";

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "";

    [JsonPropertyName("options")]
    public List<CatalogOption> Options { get; set; } = [];

    [JsonPropertyName("looping")]
    public bool Looping { get; set; }

    [JsonPropertyName("exitKey")]
    public string? ExitKey { get; set; }

    [JsonPropertyName("freeInputs")]
    public List<string> FreeInputs { get; set; } = [];

    [JsonPropertyName("requiresAdmin")]
    public bool RequiresAdmin { get; set; }

    [JsonPropertyName("consoleRequired")]
    public bool ConsoleRequired { get; set; }

    [JsonPropertyName("flags")]
    public List<string> Flags { get; set; } = [];

    [JsonPropertyName("downloads")]
    public List<string> Downloads { get; set; } = [];

    [JsonPropertyName("needsInternet")]
    public bool NeedsInternet { get; set; }

    [JsonPropertyName("revert")]
    public string? Revert { get; set; }

    [JsonPropertyName("risk")]
    public string Risk { get; set; } = "low";

    [JsonPropertyName("tag")]
    public string? Tag { get; set; }

    [JsonPropertyName("methods")]
    public List<string> Methods { get; set; } = [];

    [JsonPropertyName("function")]
    public string? Function { get; set; }

    [JsonPropertyName("commands")]
    public List<string> Commands { get; set; } = [];

    [JsonPropertyName("launcher")]
    public string? Launcher { get; set; }

    [JsonPropertyName("fileName")]
    public string? FileName { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    [JsonPropertyName("packageId")]
    public string? PackageId { get; set; }

    [JsonPropertyName("storeId")]
    public string? StoreId { get; set; }

    [JsonPropertyName("winget")]
    public string? Winget { get; set; }

    [JsonPropertyName("choco")]
    public string? Choco { get; set; }

    [JsonPropertyName("dns")]
    public Dictionary<string, object?>? Dns { get; set; }

    [JsonPropertyName("relatedFiles")]
    public Dictionary<string, string>? RelatedFiles { get; set; }

    [JsonPropertyName("inputValidation")]
    public Dictionary<string, string>? InputValidation { get; set; }

    public string Section => CatalogSections.For(this);
}

public sealed class CatalogOption
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = "";

    [JsonPropertyName("label")]
    public string Label { get; set; } = "";
}

public static class CatalogSections
{
    public static readonly string[] All =
    [
        "Optimize", "Privacy", "Features", "Debloat (AppX)", "Install Apps",
        "Network/DNS", "Repair", "Security", "Power", "Advanced", "WinSux"
    ];

    public static string For(CatalogItem item)
    {
        if (item.Source.Equals("WinSux", StringComparison.OrdinalIgnoreCase))
            return "WinSux";
        if (item.Kind == "app")
            return "Install Apps";
        if (item.Kind == "appx" || item.Category.Equals("AppX removal", StringComparison.OrdinalIgnoreCase))
            return "Debloat (AppX)";

        var category = item.Category;
        if (category.StartsWith("Install:", StringComparison.OrdinalIgnoreCase))
            return "Install Apps";
        if (category.Equals("DNS", StringComparison.OrdinalIgnoreCase) ||
            category.Equals("Network", StringComparison.OrdinalIgnoreCase))
            return "Network/DNS";
        if (category.Equals("Repair", StringComparison.OrdinalIgnoreCase) ||
            category.Equals("Fixes", StringComparison.OrdinalIgnoreCase) ||
            category.Equals("1 Check", StringComparison.OrdinalIgnoreCase) ||
            category.Equals("2 Refresh", StringComparison.OrdinalIgnoreCase))
            return "Repair";
        if (category.Equals("Security", StringComparison.OrdinalIgnoreCase) ||
            category.Equals("Updates", StringComparison.OrdinalIgnoreCase) ||
            category.Equals("Remote Access", StringComparison.OrdinalIgnoreCase))
            return "Security";
        if (category.Contains("Power", StringComparison.OrdinalIgnoreCase))
            return "Power";
        if (category.Equals("Privacy", StringComparison.OrdinalIgnoreCase))
            return "Privacy";
        if (category.Equals("Features", StringComparison.OrdinalIgnoreCase) ||
            category.Equals("Customize Preferences", StringComparison.OrdinalIgnoreCase) ||
            category.Equals("Legacy Windows Panels", StringComparison.OrdinalIgnoreCase) ||
            category.Equals("5 Graphics", StringComparison.OrdinalIgnoreCase))
            return "Features";
        if (category.Contains("Advanced", StringComparison.OrdinalIgnoreCase) ||
            category.StartsWith("Powershell Profile", StringComparison.OrdinalIgnoreCase))
            return "Advanced";
        if (category.Equals("OptimizeSystem", StringComparison.OrdinalIgnoreCase) ||
            category.Equals("Essential Tweaks", StringComparison.OrdinalIgnoreCase) ||
            category.Equals("6 Windows", StringComparison.OrdinalIgnoreCase) ||
            category.Equals("7 Hardware", StringComparison.OrdinalIgnoreCase))
            return "Optimize";
        return item.Kind switch
        {
            "dns" or "command" => "Network/DNS",
            "fix" or "feature" => "Repair",
            _ => "Optimize"
        };
    }
}
