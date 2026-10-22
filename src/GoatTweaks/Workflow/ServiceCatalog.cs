using System.Text.Json;
using GoatTweaks.Core.Catalog;

namespace GoatTweaks.Workflow;

public sealed class ServiceDefinition
{
    public string Id { get; set; } = "";
    public string Number { get; set; } = "";
    public string Label { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Glyph { get; set; } = "";
    public int ActionCount { get; set; }
    public string CountLabel => $"{ActionCount} ações disponíveis";
    public string Preview { get; set; } = "";
}

public sealed record ServiceAction(string Id, string Service, string Topic, string Title);
public sealed record TopicChoice(string Topic, string Label, bool Selected);

public sealed class ServiceCatalog
{
    private sealed class Document
    {
        public Document() { }
        public List<ServiceDefinition> Services { get; set; } = [];
        public List<ServiceAction> Actions { get; set; } = [];
    }
    public IReadOnlyList<ServiceDefinition> Services { get; }
    public IReadOnlyDictionary<string, ServiceAction> Actions { get; }
    private ServiceCatalog(Document document, CatalogService catalog)
    {
        Services = document.Services;
        Actions = document.Actions.ToDictionary(a => a.Id, StringComparer.Ordinal);
        var ids = Services.Select(s => s.Id).ToHashSet(StringComparer.Ordinal);
        if (Actions.Count != catalog.Items.Count || catalog.Items.Any(i => !Actions.TryGetValue(i.Id, out var action) || !ids.Contains(action.Service) || string.IsNullOrWhiteSpace(action.Topic)))
            throw new InvalidOperationException("O índice de serviços não corresponde ao catálogo incorporado.");
        foreach (var service in Services)
        {
            var entries = document.Actions.Where(a => a.Service == service.Id).ToArray();
            service.ActionCount = entries.Length;
            service.Preview = string.Join(" · ", entries.Select(a => a.Topic).Distinct().Take(3));
        }
    }
    public ServiceDefinition? Find(string id) => Services.FirstOrDefault(s => s.Id == id);
    public static ServiceCatalog Load(CatalogService catalog)
    {
        using var stream = typeof(ServiceCatalog).Assembly.GetManifestResourceStream("GoatTweaks.workflow.pt-BR.json")
            ?? throw new InvalidOperationException("Índice de serviços ausente.");
        return new ServiceCatalog(JsonSerializer.Deserialize<Document>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("Índice de serviços inválido."), catalog);
    }
}
