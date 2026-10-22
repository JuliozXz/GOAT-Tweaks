namespace GoatTweaks.Core.Intelligence;

/// <summary>Deterministic, explainable rules over measured evidence. No estimated FPS, score or universal tweak preset.</summary>
public static class RecommendationEngine
{
    public static List<Finding> Diagnose(DiagnosticSnapshot snapshot, GameContext context)
    {
        var findings = new List<Finding>();
        var samples = snapshot.Samples;
        void Add(string id, string title, EvidenceKind kind, string evidence, string next) => findings.Add(new(id, title, kind, evidence, next));
        if (!snapshot.Machine.Supported)
            Add("compatibility", "Sistema fora do escopo de alterações automáticas", EvidenceKind.Confirmed,
                $"{snapshot.Machine.Os} · build {snapshot.Machine.OsBuild} · {snapshot.Machine.Architecture}.",
                "Diagnóstico continua disponível. Alterações gerenciadas requerem Windows 10 22H2 ou Windows 11 de 64 bits.");
        if (samples.Count < 3)
            Add("sample.short", "Amostras insuficientes", EvidenceKind.Information, $"{samples.Count} amostra(s) coletada(s).",
                "Colete ao menos 10 segundos durante uma cena reproduzível do jogo.");
        var memory = samples.Where(s => s.MemoryPercent.HasValue).ToList();
        if (memory.Count >= 3 && memory.Count(s => s.MemoryPercent >= 90) >= Math.Ceiling(memory.Count * .7))
            Add("memory.pressure", "Uso físico de memória elevado durante a captura", EvidenceKind.Confirmed,
                $"Pelo menos 90% da RAM em {memory.Count(s => s.MemoryPercent >= 90)}/{memory.Count} amostras. Isso não prova a causa de stuttering.",
                "Repita no jogo, compare paginação, memória disponível e processos. Não desative o arquivo de paginação.");
        if (samples.Count(s => s.PagesInputPerSecond > 50) >= 3)
            Add("memory.paging", "Leituras de páginas observadas", EvidenceKind.Hypothesis,
                "Pages Input/sec elevado em pelo menos três amostras; inclui faltas de página atendidas por arquivos, não apenas pelo pagefile.",
                "Correlacione faltas de página, latência do disco e frametime com WPR/WPA antes de atribuir causa.");
        var threads = samples.Where(s => s.CpuThreads.Count > 0).ToList();
        if (threads.Count >= 3 && threads.Count(s => s.CpuThreads.Values.Any(v => v >= 90)) >= Math.Ceiling(threads.Count * .7))
            Add("cpu.thread", "Processador lógico muito ocupado durante a coleta", EvidenceKind.Hypothesis,
                "Uma ou mais threads atingiram 90% em grande parte da amostra. Migração de threads e outros processos também influenciam o contador.",
                "Meça no jogo com limite de FPS e sincronização anotados; correlacione frametime e tempo de GPU. Uso isolado não confirma gargalo.");
        var gpu = samples.Where(s => s.Gpu3dPercent.HasValue).ToList();
        if (gpu.Count >= 3 && gpu.Count(s => s.Gpu3dPercent >= 95) >= Math.Ceiling(gpu.Count * .7))
            Add("gpu.load", "Motor 3D ocupado durante a coleta", EvidenceKind.Hypothesis,
                "O motor 3D mais ocupado permaneceu próximo da capacidade em grande parte das amostras; esse contador não mede sozinho o limite do jogo.",
                "Repita a mesma cena com resolução/qualidade menor, mantendo o limite de FPS. Compare tempo de GPU e apresentação de quadros.");
        if (samples.Any(s => s.CpuTemperatureC >= 90 || s.GpuTemperatureC >= 85))
            Add("thermal", "Temperatura merece investigação", EvidenceKind.Hypothesis,
                "Sensor disponível registrou CPU ≥ 90 °C ou GPU ≥ 85 °C. Esses limiares de triagem não são os limites de todos os fabricantes.",
                "Consultar o limite específico do modelo e flags de throttling; comparar clocks e temperatura sob a mesma carga. Não alterar voltagem.");
        if (samples.Count(s => s.DiskLatencyMs > 20) >= 3)
            Add("storage.latency", "Latência de armazenamento elevada em algumas amostras", EvidenceKind.Hypothesis,
                "Contador global de disco acima de 20 ms em pelo menos três amostras; outras aplicações podem explicar a atividade.",
                "Correlacionar o disco do jogo, I/O por processo, paginação e frametime usando WPA.");
        foreach (var disk in snapshot.Storage.Where(d => d.TotalGb > 0 && d.FreePercent < 10))
            Add("space." + disk.Drive, "Pouco espaço em " + disk.Drive, EvidenceKind.Confirmed,
                $"{disk.FreeGb:0.0} GB livres de {disk.TotalGb:0.0} GB ({disk.FreePercent:0.0}%).",
                "Revisar arquivos e aplicativos com o técnico; não apagar caches de shaders ou arquivos do jogo indiscriminadamente.");
        foreach(var disk in snapshot.Machine.PhysicalDisks.Where(d=>d.HealthStatus is >0))
            Add("storage.health."+IntelligenceLabels.Hash(disk.Model)[..12],"Armazenamento informa alerta de saúde",EvidenceKind.Confirmed,
                $"{disk.Model} · {disk.Media} · HealthStatus {disk.HealthStatus} informado pelo provedor de armazenamento; não substitui diagnóstico do fabricante.",
                "Priorizar backup dos dados e ferramenta oficial do fabricante antes de otimizar ou executar manutenção de escrita. Não tentar corrigir com tweaks.");
        if(snapshot.Machine.Notebook==true)
            Add("hardware.notebook","Limites de notebook preservados",EvidenceKind.Information,
                snapshot.Machine.ComputerModel+" · alimentação: "+(snapshot.Machine.OnAcPower==true?"tomada":snapshot.Machine.OnAcPower==false?"bateria":"não confirmada"),
                "Usar perfis térmicos/energia do fabricante. O motor não altera plano OEM, voltagem ou refrigeração.");
        if(snapshot.Machine.PhysicalDisks.Any(d=>d.Media=="HD"))
            Add("storage.media","HD identificado; localização do jogo precisa ser conferida",EvidenceKind.Information,
                string.Join(" / ",snapshot.Machine.PhysicalDisks.Where(d=>d.Media=="HD").Select(d=>d.Model)),
                "Se o jogo estiver no HD e houver travadas de carregamento, medir I/O do jogo e verificar requisitos de armazenamento oficiais. A presença de HD não confirma gargalo.");
        foreach (var driver in snapshot.Drivers.Where(d => d.ErrorCode != 0))
            Add("driver." + driver.Device, "Dispositivo relata erro", EvidenceKind.Confirmed,
                $"{driver.Device}: ConfigManagerErrorCode = {driver.ErrorCode}; driver {driver.Version}.",
                "Verificar Gerenciador de Dispositivos e fabricante. A existência de erro não demonstra por si só impacto no jogo.");
        foreach (var process in snapshot.Processes.Where(p => p.CpuPercent >= 10 &&
            !p.Name.Equals(System.IO.Path.GetFileNameWithoutExtension(context.ProcessName),StringComparison.OrdinalIgnoreCase) &&
            !new[]{"GOATTweaks","powershell","pwsh","Idle","System"}.Contains(p.Name,StringComparer.OrdinalIgnoreCase)).Take(5))
            Add("process." + process.Id, "Processo com consumo observado: " + process.Name, EvidenceKind.Confirmed,
                $"CPU média no intervalo: {process.CpuPercent:0.0}% da capacidade lógica total; RAM residente: {process.WorkingSetMb:0} MB.",
                "Identifique a função do processo e repita o teste após fechar manualmente somente aplicativos dispensáveis. Não encerrar proteção/anticheat.");
        if (snapshot.Issues.Count > 0)
            Add("stability.events", "Eventos recentes de hardware ou vídeo", EvidenceKind.Confirmed,
                $"{snapshot.Issues.Count} evento(s) localizados na janela consultada; presença no log não prova causalidade.",
                "Comparar horários com falhas relatadas, drivers e novos eventos depois dos ajustes.");
        if (!context.HasComparableConditions)
            Add("game.context", "Contexto do teste ainda incompleto", EvidenceKind.Information,
                "Faltam condições completas/confirmadas do jogo, cena, versão, gráficos, sincronização ou aquecimento dos shaders.",
                "Preencha o contexto na etapa de identificação. Sem ele, a comparação de desempenho será inconclusiva.");
        foreach (var missing in snapshot.Unavailable.Distinct())
            Add("unavailable." + IntelligenceLabels.Hash(missing)[..12], "Dado não disponível", EvidenceKind.Information, missing,
                "Use uma fonte de medição compatível quando esse dado for necessário; não estimar valores ausentes.");
        if (!findings.Any(f => f.Kind == EvidenceKind.Confirmed || f.Kind == EvidenceKind.Hypothesis))
            Add("no.confirmed.problem", "Nenhum problema confirmado nessa amostra", EvidenceKind.Information,
                "A coleta atual não demonstrou uma limitação que justifique aplicar ajustes por tentativa e erro.",
                "Reproduza a queixa no jogo e capture frametimes antes de recomendar mudanças.");
        return findings;
    }

    public static List<Recommendation> Build(DiagnosticSnapshot snapshot, GameContext context, OptimizationProfile profile)
    {
        var result = new List<Recommendation>();
        var supported = snapshot.Machine.Supported;
        var aggressive = profile is OptimizationProfile.MaximumFps or OptimizationProfile.Competitive;
        void Setting(string id, SettingValue desired, bool relevant, string evidence, string benefit, string risk, bool canApply = true)
        {
            var entry = KnowledgeBase.Native.Single(k => k.Id == id);
            var known = snapshot.Settings.TryGetValue(id, out var before);
            var applicable = supported && known && before!.Exists && canApply;
            var change = applicable && before != desired && relevant;
            result.Add(new Recommendation
            {
                Id = id, Title = entry.Name, Problem = relevant ? entry.Objective : "A configuração atual não justifica esta alteração.",
                Evidence = evidence, Proposal = entry.Application, ExpectedBenefit = benefit, Confidence = "Moderada para a configuração; benefício de desempenho requer teste",
                Risk = risk, Compatibility = entry.Compatibility, Restoration = entry.Restoration,
                Prerequisites = "Mesma máquina/usuário; estado anterior íntegro; configuração ainda igual à prévia; autorização específica no plano.",
                Classification = !supported || !known || !canApply ? RecommendationClass.Incompatible : change ? RecommendationClass.NeedsTest : RecommendationClass.NoDemonstratedBenefit,
                AutomationLevel = 2, RestartRequired = entry.RestartRequired,
                Changes = change ? [new ProposedChange(id, before!, desired)] : []
            });
        }
        snapshot.Settings.TryGetValue(IntelligenceSettings.GameMode, out var gameMode);
        Setting(IntelligenceSettings.GameMode, SettingValue.Dword(1), gameMode == SettingValue.Dword(0),
            gameMode is null ? "Consulta indisponível." : !gameMode.Exists ? "Preferência ausente: o padrão do Windows não foi presumido como desativado." : $"Valor lido: {gameMode.Data} ({gameMode.Kind}).",
            "Pode alterar a priorização de jogos conforme o Windows. Ganho de FPS não demonstrado neste PC.", "Baixo; comportamento pode variar por jogo.");
        snapshot.Settings.TryGetValue(IntelligenceSettings.BackgroundCapture, out var capture);
        Setting(IntelligenceSettings.BackgroundCapture, SettingValue.Dword(0), capture == SettingValue.Dword(1),
            capture is null ? "Consulta indisponível." : !capture.Exists ? "Preferência de captura histórica não declarada; não se presume gravação ativa." : $"Captura histórica configurada como {capture.Data}. Configuração habilitada não prova custo de CPU/GPU.",
            aggressive ? "Pode reduzir trabalho de gravação se ela estava ativa; medir na cena do jogo." : "Útil somente se o cliente dispensar gravação retroativa; não é um ganho comprovado.",
            "Baixo; gravação retroativa indisponível até restaurar.");
        snapshot.Settings.TryGetValue(IntelligenceSettings.PowerPlan, out var power);
        var desktopAc = snapshot.Machine.Notebook == false && snapshot.Machine.OnAcPower == true;
        var balancedAvailable = snapshot.Machine.AvailablePowerPlans.Contains(IntelligenceSettings.BalancedGuid, StringComparer.OrdinalIgnoreCase);
        Setting(IntelligenceSettings.PowerPlan, SettingValue.PowerPlan(IntelligenceSettings.BalancedGuid),
            power?.Data?.Equals(IntelligenceSettings.PowerSaverGuid, StringComparison.OrdinalIgnoreCase) == true,
            $"Plano ativo: {power?.Data ?? "não disponível"}. Notebook: {snapshot.Machine.Notebook?.ToString() ?? "desconhecido"}; tomada: {snapshot.Machine.OnAcPower?.ToString() ?? "desconhecida"}.",
            "Pode remover limitações do plano Economia de energia em desktop. Não altera limites de CPU, firmware, ventilação ou plano OEM de notebook.",
            "Baixo no desktop elegível; consumo e calor podem aumentar.", desktopAc && balancedAvailable);
        if (result.Last().Changes.Count > 0)
            result[^1] = result[^1] with { Classification = RecommendationClass.Recommended, Confidence = "Alta para identificar plano Economia de energia; FPS requer medição" };
        var findings = Diagnose(snapshot, context);
        foreach (var finding in findings.Where(f => f.Kind != EvidenceKind.Information))
            result.Add(new Recommendation
            {
                Id = "review." + finding.Id, Title = finding.Title, Problem = finding.Title, Evidence = finding.Evidence,
                Proposal = finding.NextTest, ExpectedBenefit = "Identificar a causa antes de modificar o sistema; nenhum ganho estimado.",
                Confidence = finding.Kind == EvidenceKind.Confirmed ? "Alta para a observação; causalidade não confirmada" : "Baixa · hipótese",
                Risk = "Leitura e teste direcionado. Intervenções relevantes dependem do técnico.",
                Compatibility = snapshot.Machine.Cpu + " · " + string.Join(" / ", snapshot.Machine.Gpus),
                Restoration = "Análise sem alterações. Qualquer intervenção manual exige procedimento próprio.",
                Classification = RecommendationClass.NeedsTest, AutomationLevel = 1
            });
        result.Add(new Recommendation
        {
            Id="hardware.driver-review",Title="Revisar drivers compatíveis com este hardware",Problem="Compatibilidade e procedência dos drivers precisam de revisão específica do modelo.",
            Evidence=snapshot.Machine.Cpu+" · "+string.Join(" / ",snapshot.Machine.Gpus)+" · drivers "+string.Join(" / ",snapshot.Machine.GraphicsDrivers),
            Proposal=HardwareGuidance.DriverReview(snapshot.Machine),ExpectedBenefit="Corrigir uma incompatibilidade confirmada quando houver; nenhum ganho estimado nem atualização presumida.",
            Confidence="Alta para o inventário disponível; necessidade de atualização não confirmada",Risk="Instalação manual pode exigir reinício e causar regressões. Conferir pacote exato e procedimento do fabricante.",
            Compatibility=snapshot.Machine.Os+" · "+snapshot.Machine.Architecture+" · "+snapshot.Machine.Board,
            Prerequisites="Revisar requisitos oficiais do jogo e notas do driver para o modelo detectado.",Restoration="A análise é somente leitura. Uma instalação externa exige guardar o pacote anterior e verificar a reversão do fabricante.",
            Classification=RecommendationClass.NeedsTest,AutomationLevel=1
        });
        result.Add(new Recommendation
        {
            Id = "manual.security", Title = "Preservar segurança, firmware e proteções do fabricante",
            Problem = "Não há evidência nesta coleta que justifique desativar mecanismos de segurança, alterar BIOS ou voltagens.",
            Evidence = "Telemetria disponível não demonstra benefício controlado dessas intervenções.",
            Proposal = "Defender, SmartScreen, UAC, anticheat, firmware, overclock e undervolt ficam fora da execução automática.",
            ExpectedBenefit = "Estabilidade e integridade preservadas; nenhum ganho de FPS prometido.", Confidence = "Alta para a restrição de automação",
            Risk = "Significativo; possíveis falhas, menor proteção, incompatibilidade ou dano conforme a operação.",
            Compatibility = "Avaliação específica do fabricante e do jogo é obrigatória.", Restoration = "Reversão não garantida; procedimentos específicos devem ser verificados antes de qualquer intervenção.",
            Classification = RecommendationClass.NotRecommended, AutomationLevel = 3
        });
        return result;
    }
}
