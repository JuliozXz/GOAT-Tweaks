namespace GoatTweaks.Core.Intelligence;

public sealed record GameReference(string Id,string Name,string Process,double? MinimumRamGb,int? MinimumThreads,
    string Requirements,string TestGuide,string Source,string Reviewed)
{
    public override string ToString()=>Name;
}

/// <summary>Small, dated publisher references. No hardware ranking, automatic game edits or FPS estimates.</summary>
public static class GameReferences
{
    public static IReadOnlyList<GameReference> All { get; } =
    [
        new("","Outro jogo / objetivo manual","",null,null,"Confira os requisitos atuais na fonte oficial do jogo.",
            "Registre uma cena reproduzível, versão, gráficos, resolução, sincronização, limite de FPS e aquecimento de shaders.","",""),
        new("cs2","Counter-Strike 2","cs2.exe",8,4,
            "Referência mínima publicada: 8 GB de RAM, quatro threads de hardware, CPU da classe i5-750 ou superior, GPU DX11/Shader Model 5.0 com pelo menos 1 GB de memória e 85 GB de armazenamento. O nome da GPU/CPU não comprova equivalência de desempenho.",
            "Repita uma cena no mesmo mapa e com os mesmos parâmetros. Partidas diferentes, número de jogadores e limites de FPS diferentes não formam um teste controlado.",
            "https://store.steampowered.com/app/730/CounterStrike_2/?l=brazilian","2026-10-22"),
        new("fortnite","Fortnite","FortniteClient-Win64-Shipping.exe",8,null,
            "Referência mínima publicada: 8 GB de RAM, CPU i3-3225 e vídeo Intel HD 4000 ou Radeon Vega 8. A recomendação da Epic inclui 16 GB de RAM e SSD NVMe. O Windows e a versão do pacote precisam ser conferidos na fonte; equivalência entre modelos não é inferida.",
            "Repita a mesma ilha/cena, API gráfica e opções visuais. Registre os shaders aquecidos e evite comparar partidas distintas. Recursos gráficos e ferramentas externas precisam de compatibilidade com o jogo.",
            "https://www.epicgames.com/help/c-37599050/c-32343914/a16548002?lang=en-US","2026-10-22")
    ];
    public static GameReference? Resolve(GameContext context)=>All.FirstOrDefault(g=>g.Id.Length>0 && g.Id==context.GameReferenceId &&
        g.Name.Equals(context.Game,StringComparison.OrdinalIgnoreCase) && g.Process.Equals(context.ProcessName,StringComparison.OrdinalIgnoreCase));
    public static IEnumerable<Finding> Diagnose(MachineFacts machine,GameContext context)
    {
        var game=Resolve(context);if(game is null) yield break;
        yield return new("game.requirements","Requisitos de "+game.Name+" · verificação parcial",EvidenceKind.Information,
            game.Requirements+" Fonte: "+game.Source+" · revisão "+game.Reviewed+".",
            "Requisitos podem mudar. RAM/threads são verificações quantitativas parciais; CPU, VRAM, API e capacidade da GPU exigem confirmação. "+game.TestGuide);
        if(game.MinimumRamGb is {} ram && machine.InstalledRamGb is {} installed && installed<ram)
            yield return new("game.minimum-ram","RAM instalada abaixo do mínimo publicado para "+game.Name,EvidenceKind.Confirmed,
                $"Windows informou {installed:0.##} GB instalados; referência mínima {ram:0.##} GB. Fonte revista em {game.Reviewed}: {game.Source}",
                "Tweaks não aumentam a RAM física. Confira o requisito atual e a pressão de memória antes de definir uma intervenção. Nenhum FPS foi estimado.");
        if(game.MinimumThreads is {} threads && machine.LogicalProcessors>0 && machine.LogicalProcessors<threads)
            yield return new("game.minimum-threads","Threads disponíveis abaixo da referência de "+game.Name,EvidenceKind.Confirmed,
                $"Windows informou {machine.LogicalProcessors} processadores lógicos; referência publicada exige {threads} threads de hardware.",
                "Confirmar topologia e CPU suportada na fonte do jogo. A contagem não classifica equivalência entre modelos e nenhum tweak cria capacidade física.");
        if(machine.Architecture!="X64")
            yield return new("game.architecture","Compatibilidade do jogo precisa de verificação nesta arquitetura",EvidenceKind.Information,
                game.Name+" · sistema "+machine.Architecture+". A execução do GOAT não comprova suporte do jogo ou de seu anticheat.",
                "Conferir suporte atual do publicador para o jogo, launcher, emulação e anticheat. Não alterar mecanismos de segurança para tentar contornar incompatibilidades.");
    }
}
