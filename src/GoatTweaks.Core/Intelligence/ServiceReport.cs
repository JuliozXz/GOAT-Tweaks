using System.Net;
using System.Text;

namespace GoatTweaks.Core.Intelligence;

public static class ServiceReport
{
    public static string BuildHtml(ServiceSession session)
    {
        var html = new StringBuilder("<!doctype html><html lang=\"pt-BR\"><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"><title>GOAT TWEAKS · Relatório</title><style>body{background:#09090c;color:#f4f4f6;font:15px/1.65 system-ui,sans-serif;max-width:1120px;margin:40px auto;padding:0 24px}h1,h2{letter-spacing:.02em}h1 span{color:#f04452}h2{margin-top:38px}table{width:100%;border-collapse:collapse;margin:16px 0}td,th{padding:12px;text-align:left;border:1px solid #30303a;vertical-align:top}th{background:#21131a}p,li{color:#c2c2cf}.card{border:1px solid #30303a;background:#121217;padding:18px;border-radius:10px;margin:14px 0}code{word-break:break-all;color:#ffadb8}small{color:#a1a1b2}@media print{body{background:white;color:black}.card,th{background:#eee}p,li,small{color:#333}td,th{border-color:#aaa}}</style><body><h1>GOAT <span>TWEAKS</span></h1><h2>INTELLIGENCE · Relatório de atendimento</h2>");
        static string E(object? value) => WebUtility.HtmlEncode(value?.ToString() ?? "Não disponível");
        void Paragraph(string text) => html.Append("<p>").Append(E(text)).Append("</p>");
        void Table(string[] headers, IEnumerable<string[]> rows)
        {
            html.Append("<table><thead><tr>"); foreach (var header in headers) html.Append("<th>").Append(E(header)).Append("</th>");
            html.Append("</tr></thead><tbody>"); foreach (var row in rows) { html.Append("<tr>"); foreach (var cell in row) html.Append("<td>").Append(E(cell)).Append("</td>"); html.Append("</tr>"); }
            html.Append("</tbody></table>");
        }
        Paragraph($"Atendimento {session.Id} · {session.CreatedAt.ToLocalTime():dd/MM/yyyy HH:mm} · Perfil {session.Profile.Label()} · Etapa {session.Stage.Label()}");
        Paragraph("Este relatório separa configurações verificadas, medições e benefícios esperados. Uma alteração de configuração não comprova melhora de desempenho.");
        var before = session.Before;
        if (before is not null)
        {
            html.Append("<h2>Identificação e condições declaradas</h2>");
            Table(["Item", "Informação"], new List<string[]>
            {
                new[] { "Computador / usuário", before.Machine.Computer + " / " + before.Machine.User }, new[] { "Sistema", before.Machine.Os + " / " + before.Machine.Architecture },
                new[] { "CPU", before.Machine.Cpu }, new[] { "GPU / driver", string.Join(" / ",before.Machine.Gpus) + " / " + string.Join(" / ",before.Machine.GraphicsDrivers) },
                new[] { "Modelo / topologia",before.Machine.ComputerModel+" / "+before.Machine.CpuTopology },
                new[] { "RAM", $"{before.Machine.InstalledRamGb:0.##} GB · {before.Machine.RamConfiguration}" }, new[] { "Placa-mãe / firmware", before.Machine.Board + " / " + before.Machine.Firmware },
                new[] { "Jogo / versão", session.Context.Game + " / " + session.Context.GameVersion }, new[] { "Problema relatado", session.Context.Symptom },
                new[] { "Cena", session.Context.Scenario }, new[] { "Gráficos", session.Context.Resolution + " / " + session.Context.GraphicsPreset + " / " + session.Context.RefreshRate },
                new[] { "FPS / sincronização", session.Context.FpsLimit + " / " + session.Context.Synchronization },
                new[] { "Condições confirmadas", session.Context.HasComparableConditions ? "Sim, declaradas pelo técnico" : "Não; comparação inconclusiva" }
            });
            Table(["Inventário adicional (snapshot)","Estado informado"],
                before.Machine.FirmwareFeatures.Concat(before.Machine.GraphicsFeatures).Select(p=>new[]{p.Key,p.Value})
                .Concat(before.Machine.MemoryModules.Select(m=>new[]{"Módulo de memória",m}))
                .Concat(before.Machine.PhysicalStorage.Select(d=>new[]{"Armazenamento físico",d})));
        }
        html.Append("<h2>Diagnóstico</h2>");
        Table(["Estado", "Observação", "Evidência", "Próximo teste"], session.Findings.Select(f => new[] { f.Kind.Label(), f.Title, f.Evidence, f.NextTest }));
        html.Append("<h2>Plano e resultados por ajuste</h2>");
        foreach (var r in session.Recommendations)
        {
            html.Append("<article class=\"card\"><h3>").Append(E(r.Title)).Append("</h3>");
            var change = session.Changes.LastOrDefault(c => c.RecommendationId == r.Id);
            Paragraph(r.Classification.Label() + " · " + r.LevelLabel);
            Table(["Campo", "Descrição"], new List<string[]>
            {
                new[] { "Problema / evidência",r.Problem + " " + r.Evidence }, new[] { "Ajuste proposto",r.Proposal }, new[] { "Benefício esperado",r.ExpectedBenefit },
                new[] { "Confiança",r.Confidence }, new[] { "Risco e efeitos colaterais",r.Risk }, new[] { "Compatibilidade",r.Compatibility }, new[] { "Pré-requisitos",r.Prerequisites },
                new[] { "Reinicialização",r.RestartRequired ? "Necessária" : "Não prevista; reiniciar jogo/aplicativo pode ser necessário" },
                new[] { "Restauração",r.Restoration }, new[] { "Resultado", change is null ? "Ignorado / não executado neste atendimento" : change.State + (change.Error.Length > 0 ? " · " + change.Error : "") }
            }); html.Append("</article>");
        }
        html.Append("<h2>Resultados medidos</h2>");
        Table(["Fase", "Arquivo", "Quadros", "Duração", "FPS médio", "1% low", "Frametime médio", "p99"], session.Benchmarks.Select(b => new[]
        {
            b.Phase,b.FileName,b.FrameCount.ToString(),$"{b.DurationSeconds:0.0} s",$"{b.AverageFps:0.00}",$"{b.OnePercentLow:0.00}",$"{b.MeanFrametimeMs:0.00} ms",$"{b.P99FrametimeMs:0.00} ms"
        }));
        var comparison = BenchmarkComparer.Compare(session.Benchmarks.LastOrDefault(b => b.Phase == "Antes"), session.Benchmarks.LastOrDefault(b => b.Phase == "Depois"));
        Paragraph(comparison.Conclusion);
        foreach (var reason in comparison.Reasons) Paragraph(reason);
        if (comparison.Comparable)
            Table(["Métrica", "Depois − antes"], new List<string[]>
            {
                new[] { "FPS médio",$"{comparison.AverageFpsDelta:+0.00;-0.00;0.00} FPS" }, new[] { "1% low",$"{comparison.OnePercentLowDelta:+0.00;-0.00;0.00} FPS" },
                new[] { "Frametime médio",$"{comparison.MeanFrametimeDeltaMs:+0.00;-0.00;0.00} ms" }, new[] { "p99",$"{comparison.P99FrametimeDeltaMs:+0.00;-0.00;0.00} ms" }
            });
        Paragraph("Método: FPS = 1000 / média dos intervalos MsBetweenPresents. 1% low = 1000 / média do 1% de intervalos mais lentos. p99 usa posto mais próximo. Não é o FPS interno do jogo nem uma medição direta de latência de entrada.");
        html.Append("<h2>Telemetria e estabilidade</h2>");
        static string State(DiagnosticSnapshot? s,string id)=>s?.Settings.GetValueOrDefault(id) is {} v
            ?v.Exists?$"{v.Kind}: {v.Data}":"Valor ausente (padrão não presumido)":"Consulta não disponível";
        Table(["Configuração verificada","Antes","Depois"],IntelligenceSettings.Managed.Select(id=>new[]{id,State(before,id),State(session.After,id)}));
        static string Avg(DiagnosticSnapshot? s, Func<MetricSample,double?> selector, string unit)
        { var v = s?.Samples.Select(selector).Where(v => v.HasValue).Select(v => v!.Value).ToArray(); return v?.Length > 0 ? $"{v.Average():0.00} {unit} ({v.Length} leituras)" : "Não disponível"; }
        Table(["Leitura observada", "Antes", "Depois"], new List<string[]>
        {
            new[] { "CPU",Avg(before,s=>s.CpuPercent,"%"),Avg(session.After,s=>s.CpuPercent,"%") },
            new[] { "RAM",Avg(before,s=>s.MemoryPercent,"%"),Avg(session.After,s=>s.MemoryPercent,"%") },
            new[] { "GPU · motor 3D mais ocupado",Avg(before,s=>s.Gpu3dPercent,"%"),Avg(session.After,s=>s.Gpu3dPercent,"%") },
            new[] { "CPU · temperatura disponível",Avg(before,s=>s.CpuTemperatureC,"°C"),Avg(session.After,s=>s.CpuTemperatureC,"°C") },
            new[] { "GPU · temperatura disponível",Avg(before,s=>s.GpuTemperatureC,"°C"),Avg(session.After,s=>s.GpuTemperatureC,"°C") },
            new[] { "CPU · frequência do sensor",Avg(before,s=>s.CpuClockMhz,"MHz"),Avg(session.After,s=>s.CpuClockMhz,"MHz") },
            new[] { "GPU · frequência do sensor",Avg(before,s=>s.GpuClockMhz,"MHz"),Avg(session.After,s=>s.GpuClockMhz,"MHz") },
            new[] { "Disco · taxa total",Avg(before,s=>s.DiskBytesPerSecond,"B/s"),Avg(session.After,s=>s.DiskBytesPerSecond,"B/s") },
            new[] { "Disco · latência total",Avg(before,s=>s.DiskLatencyMs,"ms"),Avg(session.After,s=>s.DiskLatencyMs,"ms") }
        });
        Paragraph("As leituras de recursos são observações das respectivas janelas, não ganhos. Só são comparáveis quando carga, duração, condições e método forem equivalentes.");
        if (session.After is not null)
            Table(["Evento após o diagnóstico inicial", "Origem", "Identificador", "Mensagem"], session.After.Issues.Where(i=> before is null || i.At > before.FinishedAt)
                .Select(i=>new[]{i.At.ToLocalTime().ToString("g"),i.Source,i.EventId.ToString(),i.Message}));
        html.Append("<h2>Pendências e limites</h2>");
        if (session.After is null) Paragraph("Diagnóstico posterior ainda não coletado. A validação do atendimento permanece pendente.");
        foreach (var record in session.Changes.Where(c => c.State is "Preparado" or "Revisão necessária")) Paragraph("Revisão necessária: " + record.Title + " · " + record.Error);
        foreach (var unavailable in (before?.Unavailable ?? []).Concat(session.After?.Unavailable ?? []).Distinct()) Paragraph(unavailable);
        foreach (var note in session.Notes) Paragraph(note);
        Paragraph("BIOS, overclock, undervolt, firmware, anticheat e desativação de segurança não são automatizados pelo motor. O catálogo legado não constitui evidência de benefício. Backups específicos não são substituídos por ponto de restauração.");
        html.Append("<small>GOAT TWEAKS · relatório local · sem resultados simulados</small></body></html>");
        return html.ToString();
    }
}
