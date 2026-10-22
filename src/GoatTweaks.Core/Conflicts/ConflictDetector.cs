using System.Reflection;
using System.Text.Json;

namespace GoatTweaks.Core.Conflicts;

public sealed class ConflictDetector
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ConflictDetector(IEnumerable<ConflictGroup> groups) => Groups = groups.ToList();

    public IReadOnlyList<ConflictGroup> Groups { get; }

    public static ConflictDetector Load(string path)
    {
        var document = JsonSerializer.Deserialize<ConflictDocument>(File.ReadAllText(path), JsonOptions)
            ?? throw new InvalidDataException($"Invalid conflict file: {path}");
        return new ConflictDetector(document.Groups);
    }

    public static ConflictDetector LoadEmbedded()
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("GoatTweaks.Core.conflicts.json")
            ?? throw new InvalidDataException("Embedded conflicts resource is missing.");
        var document = JsonSerializer.Deserialize<ConflictDocument>(stream, JsonOptions)
            ?? throw new InvalidDataException("Embedded conflicts file is invalid.");
        return new ConflictDetector(document.Groups);
    }

    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        foreach (var group in Groups)
        {
            if (string.IsNullOrWhiteSpace(group.Id) || string.IsNullOrWhiteSpace(group.Setting) ||
                group.Note is null || group.Items is null || group.Items.Count == 0)
                errors.Add($"Invalid conflict group: {group.Id}");
            if (group.Decision is not ("duplicate" or "user-choice" or "exclusive"))
                errors.Add($"Unknown conflict decision '{group.Decision}' in {group.Id}");
            if (group.Values is not null && group.Values.Any(value => !value.Contains('=', StringComparison.Ordinal)))
                errors.Add($"Invalid conflict value in {group.Id}");
        }
        return errors;
    }

    public IReadOnlyList<ConflictViolation> FindUserChoiceConflicts(IEnumerable<string> itemIds) =>
        FindSelected("user-choice", itemIds, minimum: 2);

    public IReadOnlyList<ConflictViolation> FindDuplicateInfo(string itemId) =>
        Groups.Where(group => group.Decision == "duplicate" && group.Items.Contains(itemId, StringComparer.Ordinal))
            .Select(group => new ConflictViolation(group,
                group.Items.Where(id => !id.Equals(itemId, StringComparison.Ordinal)).ToList()))
            .ToList();

    public ConflictViolation? FindExclusiveViolation(IEnumerable<string> itemIds)
    {
        var selected = itemIds.ToHashSet(StringComparer.Ordinal);
        return Groups.Where(group => group.Decision == "exclusive")
            .Select(group => new ConflictViolation(group, group.Items.Where(selected.Contains).ToList()))
            .FirstOrDefault(violation => violation.SelectedItems.Count > 0 && selected.Count > 1);
    }

    private IReadOnlyList<ConflictViolation> FindSelected(string decision, IEnumerable<string> itemIds, int minimum)
    {
        var selected = itemIds.ToHashSet(StringComparer.Ordinal);
        return Groups.Where(group => group.Decision == decision)
            .Select(group => new ConflictViolation(group, group.Items.Where(selected.Contains).ToList()))
            .Where(violation => violation.SelectedItems.Count >= minimum)
            .ToList();
    }
}
