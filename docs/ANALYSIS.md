# Análise dos quatro materiais

Os quatro projetos têm fontes disponíveis para os recursos incorporados. Os scripts também baixam binários externos durante algumas ações; a disponibilidade das fontes dos projetos não certifica esses downloads. Os commits fixados e o inventário de cada ação (arquivo de origem, admin, risco, áreas afetadas, reversão e downloads) estão em [FUNCTION-MAP.md](FUNCTION-MAP.md). Os conflitos estão em [CONFLICTS.md](CONFLICTS.md), e a revisão de procedência em [SECURITY-REVIEW.md](SECURITY-REVIEW.md).

| Material | Tecnologia / licença | Windows | Admin | Itens no GOAT Tweaks |
|---|---|---|---|---|
| RyTuneX | C# WinUI 3, .NET 10, MSIX; AGPL-3.0 | 10 20H1 (19041)+ / 11 (README + `TargetPlatformMinVersion`) | o app inteiro roda elevado | 115 toggles/combos + 14 ferramentas |
| WinSux | PowerShell; MIT | 10/11 Home/Pro/LTSC/IoT/Server (README); precisa de internet | sim (autoelevação) | 1 pipeline (irreversível) |
| Ultimate | PowerShell (104 scripts + `IWR.ps1`, `AllowScripts.cmd`); MIT | 10/11 Home/Pro/LTSC/IoT/Server (README); vários precisam de internet | 87 scripts se autoelevam; 17 só abrem links/configurações | 104 scripts |
| winutil | PowerShell + WPF, gerado por `Compile.ps1`; MIT | 10/11 (README) | sim | 68 tweaks, 33 features/fixes, 34 AppX, 8 DNS, 239 apps, 3 políticas de update |

## Como cada material funciona
- **RyTuneX**: cada toggle tem uma `Tag`. `OptimizationOptions.ExecuteToggleActionAsync(tag, isOn)` chama `OptimizeSystemHelper.EnableX/DisableX`, que executam `cmd.exe /C` oculto (`StartInCmd`) e registram o código de saída. O estado fica em `HKLM\SOFTWARE\RyTuneX\Optimizations`. Para reverter, desliga-se o toggle (método oposto). `OneDrive` e `LocationAccess` existem no dispatcher, mas não aparecem na interface original, por isso não foram expostos.
- **WinSux**: `winsux.ps1` baixa 7-Zip, 12 VC++ redistributables, DDU, Helium e DirectX. Também baixa os próprios estágios (`stepone.ps1`, `steptwo.ps1`, `reg.reg`, …) do branch **main**, sem versão fixada. Em seguida remove apps UWP, registra `RunOnce`, liga `bcdedit safeboot minimal` e reinicia.
  - `stepone` (Modo Seguro) desativa Defender, SmartScreen, VBS, LSA, a lista de bloqueio de drivers vulneráveis e o UAC (via TrustedInstaller), roda o DDU e reinicia.
  - `steptwo` remove Edge, apps e recursos, aplica `reg.reg` e configurações, plano de energia, timer, limpeza e ponto de restauração.
  - `start2.txt` é o layout do menu Iniciar (`start2.bin`) codificado em Base64 com cabeçalho de certificado e decodificado por `certutil`. Não é um certificado de verdade.
  - **Não há reversão automática.**
- **Ultimate**: cada script mostra um menu (`Write-Host "1. …"`) e lê a escolha com `Read-Host`. A opção "Default"/"On", quando existe, é a reversão. Há 58 scripts sem reversão automática. Seis scripts usam `Run-Trusted`, que sequestra temporariamente o `binPath` do TrustedInstaller. Cinco pedem texto livre (exe de jogo, letra de pendrive, nome de conta). Oito usam `ReadKey`/`pause`/GUI e por isso abrem numa janela de console.
- **winutil**: o comportamento é declarado em JSON (`registry` com `OriginalValue`, `service` com `OriginalType`, `InvokeScript`/`UndoScript`) e aplicado por `Invoke-WinUtilTweaks` (com `-undo` para desfazer). Features usam `Enable-WindowsOptionalFeature`. Apps são instalados via winget/choco.

## Binários de terceiros baixados em tempo de execução (não verificáveis aqui)
- **WinSux**: 31 URLs (github releases do autor, wagnardsoft DDU, 7-zip.org, helium.computer, microsoft.com).
- **Ultimate**: 81 URLs (drivers NVIDIA/AMD/Intel, instaladores de apps e launchers, ferramentas como Autoruns, MSI Afterburner, testes de mouse/rede). A lista completa está na coluna "Downloads" do mapa.
- **winutil**: os instaladores vêm via winget/choco (repositórios oficiais); os links `winutil.christitus.com` são documentação.

Nenhum desses binários foi executado no ambiente de desenvolvimento. O comportamento deles só é observável no teste em VM Windows isolada, e o upstream não publica hash para a maioria.

## Itens de alto risco (exigem modo avançado no app, sem confirmação digitada)
UAC (`EnableLUA=0`), Defender/SmartScreen/Firewall, VBS/HVCI/Core Isolation, lista de drivers vulneráveis, Spectre/Meltdown, DEP, bypass de assinatura de driver, `bcdedit`, BitLocker, atualizações do Windows, remoção de drivers (DDU), reset de fábrica/reinstalação/autounattend, conversão de edição/ativação e o WinSux completo.

## Pontos que exigem atenção
- `Ultimate/6 Windows/28 Write Cache Buffer Flushing.ps1` reverte **apagando chaves `Disk` inteiras**, que podem conter valores não criados pelo script. O app avisa isso antes de executar.
- `Ultimate/6 Windows/29 Power Plan.ps1` apaga **todos** os planos de energia. "Default" restaura os planos padrão, não planos personalizados.
- `IWR.ps1`/`AllowScripts.cmd` do Ultimate tornam a ExecutionPolicy `Unrestricted` globalmente. Não foram integrados: o GOAT Tweaks usa `-ExecutionPolicy Bypass` por processo, sem alterar a política do sistema.
- Os estágios do WinSux não têm versão fixada (vêm do main no momento da execução).
- O RyTuneX é AGPL-3.0, então o GOAT Tweaks é distribuído sob AGPL-3.0, mantendo os avisos MIT.

## Não integrado nesta versão (limitação declarada)
- Gerenciadores interativos do RyTuneX: Serviços, Inicialização, Processos, Pacotes, Políticas, Stress test, Informações do sistema e a tela de Debloat (parcialmente coberta pelos itens de AppX do winutil e de bloatware do Ultimate). Também ficaram de fora os botões de plano de energia, CompressOS, limpeza de temporários e DNS personalizado (os provedores DNS do winutil cobrem essa função).
- winutil: criador de ISO/Win11 e presets (os itens individuais estão presentes).
