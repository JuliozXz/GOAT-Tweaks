using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GoatTweaks.Core.Intelligence;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OptimizationProfile { MaximumFps, Competitive, Balanced, Diagnostic, Custom }
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RecommendationClass { Recommended, NeedsTest, NoDemonstratedBenefit, Incompatible, NotRecommended }
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EvidenceKind { Confirmed, Hypothesis, Information }
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ServiceStage { Identification, Diagnosis, Plan, Execution, Restart, Validation, Report }

public static class IntelligenceLabels
{
    public static string Label(this OptimizationProfile value) => value switch
    {
        OptimizationProfile.MaximumFps => "FPS Máximo", OptimizationProfile.Competitive => "Competitivo",
        OptimizationProfile.Balanced => "Equilibrado", OptimizationProfile.Diagnostic => "Diagnóstico", _ => "Personalizado"
    };
    public static string Label(this RecommendationClass value) => value switch
    {
        RecommendationClass.Recommended => "Recomendado", RecommendationClass.NeedsTest => "Potencialmente útil · precisa de teste",
        RecommendationClass.NoDemonstratedBenefit => "Sem benefício demonstrado neste computador",
        RecommendationClass.Incompatible => "Incompatível", _ => "Não recomendado por risco ou instabilidade"
    };
    public static string Label(this EvidenceKind value) => value switch { EvidenceKind.Confirmed => "Confirmado", EvidenceKind.Hypothesis => "Suspeita · requer teste", _ => "Informação" };
    public static string Label(this ServiceStage value) => value switch
    {
        ServiceStage.Identification => "1 · Identificação", ServiceStage.Diagnosis => "2 · Diagnóstico", ServiceStage.Plan => "3 · Plano",
        ServiceStage.Execution => "4 · Execução", ServiceStage.Restart => "5 · Reinicialização", ServiceStage.Validation => "6 · Validação", _ => "7 · Relatório"
    };
    public static string Hash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
}

public sealed record GameContext
{
    public string GameReferenceId { get; init; } = "";
    public string Game { get; init; } = "";
    public string ProcessName { get; init; } = "";
    public string GameVersion { get; init; } = "";
    public string Scenario { get; init; } = "";
    public string Resolution { get; init; } = "";
    public string GraphicsPreset { get; init; } = "";
    public string RefreshRate { get; init; } = "";
    public string FpsLimit { get; init; } = "";
    public string Synchronization { get; init; } = "";
    public bool ShaderWarmupCompleted { get; init; }
    public bool ConditionsConfirmed { get; init; }
    public string Symptom { get; init; } = "";
    public string CustomObjectives { get; init; } = "";
    public string ConditionsKey => IntelligenceLabels.Hash(JsonSerializer.Serialize(new
    {
        GameReferenceId, Game, ProcessName, GameVersion, Scenario, Resolution, GraphicsPreset, RefreshRate, FpsLimit, Synchronization, ShaderWarmupCompleted
    }));
    public bool HasComparableConditions => ConditionsConfirmed && ShaderWarmupCompleted &&
        new[] { Game, ProcessName, GameVersion, Scenario, Resolution, GraphicsPreset, RefreshRate, FpsLimit, Synchronization }.All(s => !string.IsNullOrWhiteSpace(s));
}

public sealed record MachineFacts
{
    public string MachineId { get; init; } = "";
    public string UserId { get; init; } = "";
    public string Computer { get; init; } = "";
    public string User { get; init; } = "";
    public string Os { get; init; } = "";
    public int OsBuild { get; init; }
    public bool IsWindowsClient { get; init; }
    public string Architecture { get; init; } = "";
    public string Cpu { get; init; } = "";
    public int LogicalProcessors { get; init; }
    public string CpuTopology { get; init; } = "Não disponível";
    public string ComputerModel { get; init; } = "";
    public List<string> Gpus { get; init; } = [];
    public List<string> GraphicsDrivers { get; init; } = [];
    public double? InstalledRamGb { get; init; }
    public string RamConfiguration { get; init; } = "Não disponível";
    public List<string> MemoryModules { get; init; } = [];
    public string Board { get; init; } = "";
    public string Firmware { get; init; } = "";
    public bool? Notebook { get; init; }
    public bool? OnAcPower { get; init; }
    public string BootId { get; init; } = "";
    public List<string> AvailablePowerPlans { get; init; } = [];
    public List<string> PhysicalStorage { get; init; } = [];
    public List<PhysicalDiskObservation> PhysicalDisks { get; init; } = [];
    public Dictionary<string,string> FirmwareFeatures { get; init; } = [];
    public Dictionary<string,string> GraphicsFeatures { get; init; } = [];
    public List<string> Capabilities { get; init; } = [];
    public bool Supported => IsWindowsClient && OsBuild >= 19045 && Architecture is "X64" or "Arm64";
    public string Fingerprint => IntelligenceLabels.Hash(string.Join("|", MachineId, UserId, OsBuild, Architecture, Cpu,
        LogicalProcessors,string.Join(";", Gpus), string.Join(";", GraphicsDrivers), InstalledRamGb?.ToString("R",System.Globalization.CultureInfo.InvariantCulture),
        RamConfiguration,string.Join(";",MemoryModules),Board,Firmware));
}

public sealed record MetricSample
{
    public DateTimeOffset At { get; init; } = DateTimeOffset.UtcNow;
    public double? CpuPercent { get; init; }
    public double? MemoryPercent { get; init; }
    public double? AvailableMemoryMb { get; init; }
    public Dictionary<string, double> CpuThreads { get; init; } = [];
    public double? Gpu3dPercent { get; init; }
    public double? PagesInputPerSecond { get; init; }
    public double? DiskBytesPerSecond { get; init; }
    public double? DiskLatencyMs { get; init; }
    public double? CpuTemperatureC { get; init; }
    public double? GpuTemperatureC { get; init; }
    public double? CpuClockMhz { get; init; }
    public double? GpuClockMhz { get; init; }
    public string SensorSource { get; init; } = "Não disponível";
}
public sealed record ProcessObservation(int Id, string Name, double? CpuPercent, double WorkingSetMb);
public sealed record StorageObservation(string Drive, double TotalGb, double FreeGb)
{
    public double FreePercent => TotalGb > 0 ? 100 * FreeGb / TotalGb : 0;
}
public sealed record StartupObservation(string Name, string Location);
public sealed record PhysicalDiskObservation(string Model, string Media, int? HealthStatus);
public sealed record DriverObservation(string Device, string Version, int ErrorCode);
public sealed record SystemIssue(DateTimeOffset At, string Source, int EventId, string Message);
public sealed record Finding(string Id, string Title, EvidenceKind Kind, string Evidence, string NextTest);

public sealed record SettingValue(bool Exists, string Kind, string? Data)
{
    public static SettingValue Missing { get; } = new(false, "None", null);
    public static SettingValue Dword(int value) => new(true, "DWord", value.ToString(System.Globalization.CultureInfo.InvariantCulture));
    public static SettingValue PowerPlan(string guid) => new(true, "PowerPlan", guid.ToLowerInvariant());
}
public sealed record DiagnosticSnapshot
{
    public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset FinishedAt { get; init; }
    public MachineFacts Machine { get; init; } = new();
    public List<MetricSample> Samples { get; init; } = [];
    public List<ProcessObservation> Processes { get; init; } = [];
    public List<StorageObservation> Storage { get; init; } = [];
    public List<StartupObservation> Startup { get; init; } = [];
    public List<DriverObservation> Drivers { get; init; } = [];
    public List<SystemIssue> Issues { get; init; } = [];
    public Dictionary<string, SettingValue> Settings { get; init; } = [];
    public List<string> Unavailable { get; init; } = [];
    public string CollectionMode { get; init; } = "Coleta delimitada · 2 segundos entre amostras";
}

public sealed record KnowledgeEntry
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string TechnicalName { get; init; } = "";
    public string Category { get; init; } = "";
    public string Description { get; init; } = "";
    public string Objective { get; init; } = "";
    public List<string> EvidenceSources { get; init; } = [];
    public string Compatibility { get; init; } = "";
    public string AdverseEffects { get; init; } = "";
    public string Application { get; init; } = "";
    public string Restoration { get; init; } = "";
    public string Reliability { get; init; } = "Não validado individualmente";
    public string? LastValidated { get; init; }
    public string LastReviewed { get; init; } = "2026-10-22";
    public int AutomationLevel { get; init; } = 3;
    public bool RestartRequired { get; init; }
}

public sealed record ProposedChange(string SettingId, SettingValue Before, SettingValue Desired);
public sealed record Recommendation
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public string Problem { get; init; } = "";
    public string Evidence { get; init; } = "";
    public string Proposal { get; init; } = "";
    public string ExpectedBenefit { get; init; } = "";
    public string ProbableImpact { get; init; } = "Não determinado; depende de teste direcionado neste computador";
    public string Reversibility { get; init; } = "Análise sem alteração; intervenções externas exigem procedimento próprio";
    public string Confidence { get; init; } = "Baixa";
    public string Risk { get; init; } = "";
    public string Compatibility { get; init; } = "";
    public string Restoration { get; init; } = "";
    public string Prerequisites { get; init; } = "";
    public RecommendationClass Classification { get; init; }
    public int AutomationLevel { get; init; } = 1;
    public bool RestartRequired { get; init; }
    public List<ProposedChange> Changes { get; init; } = [];
    public string ClassificationLabel => Classification.Label();
    public string LevelLabel => AutomationLevel == 1 ? "Nível 1 · análise" : AutomationLevel == 2 ? "Nível 2 · exige plano aprovado" : "Nível 3 · intervenção manual";
    public bool CanAuthorize => AutomationLevel == 2 && Changes.Count > 0 && Classification is RecommendationClass.Recommended or RecommendationClass.NeedsTest;
}

public sealed class ChangeRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string RecommendationId { get; set; } = "";
    public string Title { get; set; } = "";
    public DateTimeOffset At { get; set; } = DateTimeOffset.UtcNow;
    public List<ProposedChange> Changes { get; set; } = [];
    public string State { get; set; } = "Preparado";
    public string Error { get; set; } = "";
    public bool RestartRequired { get; set; }
    public bool Reversible { get; set; } = true;
}

public sealed class ServiceSession
{
    public int SchemaVersion { get; set; } = 1;
    public bool EphemeralScan { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public OptimizationProfile Profile { get; set; } = OptimizationProfile.Diagnostic;
    public GameContext Context { get; set; } = new();
    public ServiceStage Stage { get; set; } = ServiceStage.Identification;
    public DiagnosticSnapshot? Before { get; set; }
    public DiagnosticSnapshot? After { get; set; }
    public List<Finding> Findings { get; set; } = [];
    public List<Recommendation> Recommendations { get; set; } = [];
    public List<string> AuthorizedIds { get; set; } = [];
    public string? ApprovedPlanHash { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public List<ChangeRecord> Changes { get; set; } = [];
    public List<BenchmarkRun> Benchmarks { get; set; } = [];
    public List<string> Notes { get; set; } = [];
    public bool RestartConfirmed { get; set; }
    public string StageLabel => Stage.Label();
    public string DisplayName => $"{CreatedAt.ToLocalTime():dd/MM HH:mm} · {Before?.Machine.Computer ?? "Novo atendimento"} · {Context.Game} · {Stage.Label()}";
    public void InvalidateApproval() { AuthorizedIds.Clear(); ApprovedAt = null; ApprovedPlanHash = null; }
    public string PlanHash() => IntelligenceLabels.Hash(JsonSerializer.Serialize(new
    {
        Id, Profile, Context, Machine = Before?.Machine.Fingerprint,
        Recommendations = Recommendations.Where(r => AuthorizedIds.Contains(r.Id, StringComparer.Ordinal)).OrderBy(r => r.Id).ToArray()
    }));
}
