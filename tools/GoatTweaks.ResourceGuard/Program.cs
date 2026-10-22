using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text.Json;

// Inspect compiled artifacts without loading or executing the Windows application.
if (args.Length == 2 && args[0] == "--list")
{
    using var stream = File.OpenRead(args[1]);
    using var pe = new PEReader(stream);
    var metadata = pe.GetMetadataReader();
    foreach (var handle in metadata.ManifestResources)
        Console.WriteLine(metadata.GetString(metadata.GetManifestResource(handle).Name));
    return 0;
}
if (args.Length != 2)
{
    Console.Error.WriteLine("Uso: ResourceGuard <diretório dos assemblies> <raiz do repositório>");
    return 2;
}
try
{
    var directory = Path.GetFullPath(args[0]);
    var root = Path.GetFullPath(args[1]);
    var index = ReadResource(Path.Combine(directory, "GOATTweaks.dll"), "GoatTweaks.workflow.pt-BR.json");
    var catalog = ReadResource(Path.Combine(directory, "GoatTweaks.Core.dll"), "GoatTweaks.Core.catalog.json");
    var intelligence = ReadResource(Path.Combine(directory, "GoatTweaks.Core.dll"), "GoatTweaks.Core.intelligence-inventory.json");
    RequireSourceMatch(index, Path.Combine(root, "catalog", "workflow.pt-BR.json"));
    RequireSourceMatch(catalog, Path.Combine(root, "catalog", "catalog.json"));
    RequireSourceMatch(intelligence, Path.Combine(root, "catalog", "intelligence-inventory.json"));
    using var workflowJson = JsonDocument.Parse(index);
    using var catalogJson = JsonDocument.Parse(catalog);
    var services = workflowJson.RootElement.GetProperty("services").EnumerateArray().Select(s => Required(s, "id")).ToArray();
    var areas = services.ToHashSet(StringComparer.Ordinal);
    if (services.Length == 0 || areas.Count != services.Length)
        throw new InvalidDataException("O índice possui serviços vazios ou duplicados.");
    var originalIds = catalogJson.RootElement.GetProperty("items").EnumerateArray().Select(item => Required(item, "id")).ToArray();
    var indexedIds = new HashSet<string>(StringComparer.Ordinal);
    foreach (var action in workflowJson.RootElement.GetProperty("actions").EnumerateArray())
    {
        if (!indexedIds.Add(Required(action, "id")) || !areas.Contains(Required(action, "service")))
            throw new InvalidDataException("O índice possui uma ação duplicada ou um serviço inexistente.");
        _ = Required(action, "topic");
        _ = Required(action, "title");
    }
    if (originalIds.Length != indexedIds.Count || !indexedIds.SetEquals(originalIds))
        throw new InvalidDataException("O índice incorporado não cobre todas as ações do catálogo compilado.");
    using var knowledgeJson = JsonDocument.Parse(intelligence);
    var knowledgeIds = knowledgeJson.RootElement.EnumerateArray().Select(k=>Required(k,"Id")).ToArray();
    if(knowledgeIds.Length!=originalIds.Length || !knowledgeIds.ToHashSet(StringComparer.Ordinal).SetEquals(originalIds.Select(id=>"legacy:"+id)))
        throw new InvalidDataException("O inventário do Intelligence não preserva todas as funções originais.");
    Console.WriteLine($"Recursos compilados conferidos: índice no assembly principal; {indexedIds.Count} ações em {areas.Count} serviços; conteúdo idêntico às fontes.");
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine("Pacote recusado: " + exception.Message);
    return 1;
}

static string Required(JsonElement element, string property) =>
    element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String &&
    value.GetString() is { } text && !string.IsNullOrWhiteSpace(text) ? text
    : throw new InvalidDataException("Campo obrigatório ausente no índice: " + property);

static void RequireSourceMatch(byte[] compiled, string path)
{
    if (!SHA256.HashData(compiled).AsSpan().SequenceEqual(SHA256.HashData(File.ReadAllBytes(path))))
        throw new InvalidDataException("O recurso compilado difere da fonte: " + Path.GetFileName(path));
}

static byte[] ReadResource(string assembly, string name)
{
    using var stream = File.OpenRead(assembly);
    using var pe = new PEReader(stream);
    var metadata = pe.GetMetadataReader();
    foreach (var handle in metadata.ManifestResources)
    {
        var resource = metadata.GetManifestResource(handle);
        if (metadata.GetString(resource.Name) != name) continue;
        if (!resource.Implementation.IsNil)
            throw new InvalidDataException("O recurso está fora do assembly principal: " + name);
        var block = pe.GetSectionData(pe.PEHeaders.CorHeader!.ResourcesDirectory.RelativeVirtualAddress);
        var offset = checked((int)resource.Offset);
        var reader = block.GetReader(offset, block.Length - offset);
        var length = reader.ReadInt32();
        if (length <= 0 || length > reader.RemainingBytes)
            throw new InvalidDataException("Tamanho de recurso inválido: " + name);
        return reader.ReadBytes(length);
    }
    throw new InvalidDataException("Recurso obrigatório ausente em " + Path.GetFileName(assembly) + ": " + name);
}
