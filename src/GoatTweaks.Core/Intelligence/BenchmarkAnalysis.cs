using System.Globalization;
using System.Security.Cryptography;
using Microsoft.VisualBasic.FileIO;

namespace GoatTweaks.Core.Intelligence;

public sealed record BenchmarkRun
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Phase { get; init; } = "Antes";
    public DateTimeOffset ImportedAt { get; init; } = DateTimeOffset.UtcNow;
    public string FileName { get; init; } = "";
    public string Sha256 { get; init; } = "";
    public string Source { get; init; } = "PresentMon CSV · origem declarada pelo técnico";
    public string MetricDefinition { get; init; } = "MsBetweenPresents · intervalo entre apresentações, em ms";
    public string Process { get; init; } = "";
    public string SwapChain { get; init; } = "";
    public string ProcessId { get; init; } = "";
    public string MachineFingerprint { get; init; } = "";
    public GameContext Context { get; init; } = new();
    public int FrameCount { get; init; }
    public int IgnoredRows { get; init; }
    public double DurationSeconds { get; init; }
    public double AverageFps { get; init; }
    public double OnePercentLow { get; init; }
    public double MeanFrametimeMs { get; init; }
    public double P99FrametimeMs { get; init; }
    public string Summary => $"{Phase} · {Context.Game} · {FrameCount:N0} quadros · {AverageFps:0.0} FPS · 1% low {OnePercentLow:0.0} · p99 {P99FrametimeMs:0.00} ms";
}

public static class BenchmarkImporter
{
    public static BenchmarkRun Import(string path, string phase, MachineFacts machine, GameContext context, string? swapChain = null)
    {
        if (string.IsNullOrWhiteSpace(context.ProcessName)) throw new InvalidDataException("Informe o executável do jogo para não misturar processos no CSV.");
        if (phase is not ("Antes" or "Depois")) throw new InvalidDataException("Fase de medição inválida.");
        if (new FileInfo(path).Length > 128 * 1024 * 1024) throw new InvalidDataException("CSV maior que 128 MiB. Exporte uma captura delimitada do jogo.");
        using var input = new TextFieldParser(path) { TextFieldType = FieldType.Delimited, HasFieldsEnclosedInQuotes = true };
        input.SetDelimiters(",");
        var headers = input.ReadFields() ?? throw new InvalidDataException("CSV vazio.");
        int Column(string name) => Array.FindIndex(headers, h => h.Trim().Equals(name, StringComparison.OrdinalIgnoreCase));
        var frameColumn = Column("MsBetweenPresents");
        var appColumn = Column("Application");
        var chainColumn = Column("SwapChainAddress");
        var pidColumn = Column("ProcessID");
        if (frameColumn < 0 || appColumn < 0 || chainColumn < 0 || pidColumn < 0)
            throw new InvalidDataException("CSV incompatível. Use PresentMon com --v1_metrics; são necessários Application, ProcessID, SwapChainAddress e MsBetweenPresents. Não se presume que outros campos medem a mesma coisa.");
        var frames = new List<double>();
        var chains = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pids = new HashSet<string>(StringComparer.Ordinal);
        var ignored = 0;
        var rows = 0;
        var target = Path.GetFileNameWithoutExtension(context.ProcessName.Trim());
        while (!input.EndOfData)
        {
            if (++rows > 1_000_000) throw new InvalidDataException("Limite de um milhão de linhas excedido.");
            var fields = input.ReadFields();
            if (fields is null || fields.Length != headers.Length) throw new InvalidDataException("Linha CSV malformada; a captura não foi usada.");
            if (!Path.GetFileNameWithoutExtension(fields[appColumn]).Equals(target, StringComparison.OrdinalIgnoreCase)) continue;
            if (!string.IsNullOrWhiteSpace(swapChain) && !fields[chainColumn].Equals(swapChain, StringComparison.OrdinalIgnoreCase)) continue;
            chains.Add(fields[chainColumn]);
            pids.Add(fields[pidColumn]);
            if (chains.Count > 1) throw new InvalidDataException("Há mais de uma swap chain para esse jogo. Separe a captura ou informe uma swap chain específica; seus frames não podem ser somados.");
            if (pids.Count > 1) throw new InvalidDataException("Há mais de uma instância do processo no CSV. Exporte somente a execução que deseja medir.");
            if (!double.TryParse(fields[frameColumn], NumberStyles.Float, CultureInfo.InvariantCulture, out var ms) || !double.IsFinite(ms) || ms <= 0)
            { ignored++; continue; }
            frames.Add(ms);
        }
        if (frames.Count < 300) throw new InvalidDataException("Menos de 300 intervalos válidos do jogo. Grave um trecho mais longo e confirme o nome do executável.");
        if (ignored > Math.Max(3, (frames.Count + ignored) * .01)) throw new InvalidDataException("Mais de 1% dos intervalos selecionados é inválido; a captura precisa ser refeita.");
        var sum = frames.Sum();
        if (!double.IsFinite(sum) || sum < 10_000) throw new InvalidDataException("A captura válida deve representar pelo menos 10 segundos.");
        var ordered = frames.OrderBy(v => v).ToArray();
        var slowCount = Math.Max(1, (int)Math.Ceiling(ordered.Length * .01));
        using var file = File.OpenRead(path);
        return new BenchmarkRun
        {
            Phase = phase, FileName = Path.GetFileName(path), Sha256 = Convert.ToHexString(SHA256.HashData(file)),
            Process = context.ProcessName.Trim(), SwapChain = chains.Single(), ProcessId = pids.Single(), MachineFingerprint = machine.Fingerprint,
            Context = context, FrameCount = frames.Count, IgnoredRows = ignored, DurationSeconds = sum / 1000,
            MeanFrametimeMs = sum / frames.Count, AverageFps = 1000 / (sum / frames.Count),
            OnePercentLow = 1000 / ordered.TakeLast(slowCount).Average(),
            P99FrametimeMs = ordered[Math.Clamp((int)Math.Ceiling(ordered.Length * .99) - 1, 0, ordered.Length - 1)]
        };
    }
}

public sealed record BenchmarkComparison(bool Comparable, string Conclusion, List<string> Reasons,
    double? AverageFpsDelta = null, double? OnePercentLowDelta = null, double? MeanFrametimeDeltaMs = null, double? P99FrametimeDeltaMs = null);

public static class BenchmarkComparer
{
    public static BenchmarkComparison Compare(BenchmarkRun? before, BenchmarkRun? after)
    {
        var reasons = new List<string>();
        if (before is null || after is null) return new(false, "Inconclusivo · faltam testes antes/depois.", ["Importe ou capture um teste em cada fase."]);
        if (before.Phase != "Antes" || after.Phase != "Depois") reasons.Add("As fases não representam um par antes/depois.");
        if (!before.Context.HasComparableConditions || !after.Context.HasComparableConditions) reasons.Add("Condições do jogo incompletas, não confirmadas ou sem aquecimento de shaders.");
        if (before.Context.ConditionsKey != after.Context.ConditionsKey) reasons.Add("Jogo, versão, cena, resolução, gráficos, limite de FPS ou sincronização diferem.");
        if (before.MachineFingerprint != after.MachineFingerprint) reasons.Add("Hardware, usuário, sistema ou driver gráfico diferem.");
        if (before.MetricDefinition != after.MetricDefinition || before.Source != after.Source) reasons.Add("Fontes ou definições das métricas diferem.");
        if (before.Sha256 == after.Sha256) reasons.Add("O mesmo arquivo foi usado nas duas fases; não demonstra uma nova medição.");
        if (before.FrameCount < 300 || after.FrameCount < 300 || before.DurationSeconds < 10 || after.DurationSeconds < 10) reasons.Add("Duração ou quantidade de amostras insuficiente.");
        if (Math.Abs(before.DurationSeconds - after.DurationSeconds) > Math.Max(before.DurationSeconds, after.DurationSeconds) * .05)
            reasons.Add("As durações diferem em mais de 5%; repita a mesma janela de medição.");
        if (reasons.Count > 0) return new(false, "Inconclusivo · não é possível atribuir ganho às alterações.", reasons);
        return new(true, "Diferenças medidas sob as condições declaradas; uma única repetição não demonstra causalidade ou ganho estatisticamente confiável.",
            ["Repita a cena várias vezes. Variações de temperatura ambiente, rede, carga e cache também podem influenciar os resultados.",
             "FPS de apresentação e 1% low calculados dos intervalos do CSV; arquivos importados têm origem declarada pelo técnico."],
            after.AverageFps - before.AverageFps, after.OnePercentLow - before.OnePercentLow,
            after.MeanFrametimeMs - before.MeanFrametimeMs, after.P99FrametimeMs - before.P99FrametimeMs);
    }
}
