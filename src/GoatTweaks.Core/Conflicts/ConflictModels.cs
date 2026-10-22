using System.Text.Json.Serialization;

namespace GoatTweaks.Core.Conflicts;

public sealed class ConflictDocument
{
    [JsonPropertyName("groups")]
    [JsonRequired]
    public List<ConflictGroup> Groups { get; set; } = [];
}

public sealed class ConflictGroup
{
    [JsonPropertyName("id")]
    [JsonRequired]
    public string Id { get; set; } = "";

    [JsonPropertyName("setting")]
    [JsonRequired]
    public string Setting { get; set; } = "";

    [JsonPropertyName("items")]
    [JsonRequired]
    public List<string> Items { get; set; } = [];

    [JsonPropertyName("decision")]
    [JsonRequired]
    public string Decision { get; set; } = "";

    [JsonPropertyName("note")]
    [JsonRequired]
    public string Note { get; set; } = "";

    [JsonPropertyName("values")]
    public List<string> Values { get; set; } = [];
}

public sealed record ConflictViolation(ConflictGroup Group, IReadOnlyList<string> SelectedItems);
