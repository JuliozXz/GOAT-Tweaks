namespace GoatTweaks.Hardware;

public sealed record ProcessorInfo(string Name, int Cores, int Threads);
public sealed record GraphicsInfo(string Name, string Driver);
public sealed record MemoryModule(string Slot, ulong Capacity, uint Type, uint Speed, string Manufacturer);
public sealed record HardwareDetail(string Label, string Value);

public sealed record HardwareSnapshot
{
    public const string Unknown = "Não informado pelo fabricante";
    public string UserName { get; init; } = Environment.UserName;
    public string ComputerName { get; init; } = Environment.MachineName;
    public string WindowsName { get; init; } = "Windows · " + Environment.OSVersion.Version;
    public IReadOnlyList<ProcessorInfo> Processors { get; init; } = [];
    public IReadOnlyList<GraphicsInfo> Graphics { get; init; } = [];
    public IReadOnlyList<MemoryModule> Modules { get; init; } = [];
    public string BoardManufacturer { get; init; } = "";
    public string BoardProduct { get; init; } = "";
    public string BiosVersion { get; init; } = "";
    public ulong UsableMemory { get; init; }
    public bool Detected { get; init; }
    public int FailedQueries { get; init; }
    public string DiscoveryWarning { get; init; } = "";
    public DateTimeOffset? UpdatedAt { get; init; }

    public string CpuName => Processors.Count == 0 ? (Detected ? Unknown : "Identificando processador…") : string.Join(" + ", Processors.Select(p => p.Name));
    public string CpuSummary => Processors.Count == 0 ? "Consulta local do Windows" : Processors.Sum(p => p.Cores) == 0 || Processors.Sum(p => p.Threads) == 0
        ? "Topologia não informada pelo Windows" : $"{Processors.Sum(p => p.Cores)} núcleos · {Processors.Sum(p => p.Threads)} processadores lógicos";
    public string GpuName => Graphics.Count == 0 ? (Detected ? Unknown : "Identificando vídeo…") : string.Join(" + ", Graphics.Select(g => g.Name));
    public string GpuSummary => Graphics.Count > 1 ? $"{Graphics.Count} adaptadores identificados" : Graphics.Count == 1 ? "Driver " + Graphics[0].Driver : "Adaptadores informados pelo Windows";
    public ulong InstalledMemory => Modules.Aggregate(0UL, (sum, module) => sum + module.Capacity);
    public string RamCapacity => InstalledMemory > 0 ? FormatBytes(InstalledMemory) : UsableMemory > 0 ? FormatBytes(UsableMemory) + " utilizáveis" : Detected ? Unknown : "Identificando RAM…";
    public string RamGeneration
    {
        get
        {
            var types = Modules.Select(m => MemoryType(m.Type)).Distinct().ToArray();
            return types.Length == 0 ? (Detected ? Unknown : "Consultando geração…") : string.Join(" / ", types);
        }
    }
    public string RamSummary => Modules.Count > 0 ? $"{RamGeneration} · {Modules.Count} módulo(s)" : RamGeneration;
    public string BoardName => string.IsNullOrWhiteSpace(BoardProduct) ? (Detected ? Unknown : "Identificando placa-mãe…") : BoardProduct;
    public string BoardSummary => string.IsNullOrWhiteSpace(BoardManufacturer) ? "Placa-mãe / firmware" : BoardManufacturer;
    public string Avatar => string.IsNullOrWhiteSpace(UserName) ? "G" : UserName[..1].ToUpperInvariant();
    public string Greeting => "Olá, " + UserName + ".";

    public IReadOnlyList<HardwareDetail> Details
    {
        get
        {
            var rows = new List<HardwareDetail>
            {
                new("Usuário da sessão", UserName), new("Computador", ComputerName),
                new("Sistema operacional", WindowsName), new("Arquitetura", "Windows de 64 bits"),
                new("Processador", Detected && Processors.Count == 0 ? Unknown : CpuName),
                new("Núcleos e threads", Processors.Count == 0 ? Unknown : CpuSummary),
                new("Geração do processador", CpuGeneration()),
                new("RAM instalada", InstalledMemory > 0 ? FormatBytes(InstalledMemory) : Unknown),
                new("RAM utilizável pelo Windows", UsableMemory > 0 ? FormatBytes(UsableMemory) : Unknown),
                new("Geração da memória", RamGeneration),
                new("Placa-mãe", BoardName), new("Fabricante da placa-mãe", string.IsNullOrWhiteSpace(BoardManufacturer) ? Unknown : BoardManufacturer),
                new("Versão do BIOS / UEFI", string.IsNullOrWhiteSpace(BiosVersion) ? Unknown : BiosVersion)
            };
            for (var i = 0; i < Graphics.Count; i++)
            {
                rows.Add(new($"Vídeo {i + 1}", Graphics[i].Name));
                rows.Add(new($"Driver de vídeo {i + 1}", Graphics[i].Driver));
            }
            foreach (var module in Modules)
                rows.Add(new("RAM · " + module.Slot,
                    $"{FormatBytes(module.Capacity)} · {MemoryType(module.Type)}" +
                    (module.Speed > 0 ? $" · {module.Speed} MHz informados pelo Windows" : "") +
                    (string.IsNullOrWhiteSpace(module.Manufacturer) ? "" : " · " + module.Manufacturer)));
            return rows;
        }
    }

    private string CpuGeneration()
    {
        var generations = Processors.Select(p => System.Text.RegularExpressions.Regex.Match(p.Name,
            @"\b(\d{1,2})(?:st|nd|rd|th)\s+Gen\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            .Where(m => m.Success).Select(m => m.Groups[1].Value + "ª geração (nome do fabricante)").Distinct().ToArray();
        return generations.Length == 0 ? "Consulte o modelo acima; geração não declarada no nome" : string.Join(" / ", generations);
    }

    public static string MemoryType(uint type) => type switch
    {
        // Raw SMBIOS Type 17 values (DMTF DSP0134), distinct from the legacy CIM MemoryType enum.
        18 => "DDR", 19 => "DDR2", 20 => "DDR2 FB-DIMM", 24 => "DDR3",
        26 => "DDR4", 27 => "LPDDR", 28 => "LPDDR2", 29 => "LPDDR3", 30 => "LPDDR4",
        32 => "HBM", 33 => "HBM2", 34 => "DDR5", 35 => "LPDDR5", 36 => "HBM3", 37 => "MRDIMM",
        0 or 1 or 2 => "Geração não informada", _ => $"Tipo SMBIOS {type}"
    };

    public static string FormatBytes(ulong value) => $"{value / 1073741824d:0.##} GB";
}
