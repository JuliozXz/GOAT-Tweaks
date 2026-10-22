# GOAT TWEAKS — INTELLIGENCE ENGINE

O motor é local, determinístico e orientado a evidências. Não consulta um modelo de IA externo, não atribui uma nota fictícia ao PC e não promete ganhos de FPS.

## Implementação por requisito

| Tópico | Implementação real |
| --- | --- |
| GOAT Intelligence | Inventário CIM reutilizado, coleta delimitada, regras explicáveis, plano, autorização vinculada ao conteúdo e validação posterior. |
| Perfis | FPS Máximo, Competitivo, Equilibrado, Diagnóstico e Personalizado. Alteram objetivo/prioridade da revisão; nenhum aplica um pacote universal. Diagnóstico bloqueia escritas e restaurações. |
| Recomendações | Evidência, hipótese/observação, benefício esperado, confiança, risco, compatibilidade, pré-requisitos, reinício, método de restauração e resultado separado. |
| Gargalos | CPU por processador lógico, GPU por motor 3D, memória física, paginação, disco, processos, erros de dispositivos, eventos WHEA/vídeo e sensores opcionais. Utilização isolada não confirma gargalo. |
| Antes/depois | Capturas de diagnóstico persistentes; PresentMon instalado ou CSV real; comparação de condições, identidade do hardware/driver, duração e definição das métricas. Condições divergentes produzem resultado inconclusivo. |
| Alterações reversíveis | Lista explícita de alvos; pré-condição de estado; backup específico durável antes da escrita; leitura após aplicar; compensação em falha; reversão em ordem inversa; proteção contra sobrescrever mudanças externas. |
| GOAT Service Mode | Identificação, diagnóstico, plano, execução, decisão sobre reinício, validação e relatório. Histórico local retoma o atendimento sem reaplicar mudanças automaticamente. |
| Hardware | Modelo de CPU/GPU, driver, RAM, módulos/geração quando exposta, placa-mãe, firmware, tipo de computador, alimentação e mídia física. Notebook ou tipo/alimentação desconhecidos impedem a regra automática de energia. |
| Conhecimento | Regras nativas e inventário das 619 ações originais com nome em português, nome técnico, origem, risco, compatibilidade, aplicação, reversão e confiabilidade. Validação individual de benefício não é inventada. |
| Painel | Amostras reais, origem/horário, dados ausentes explícitos, histórico de CPU e comparação de medições. Coleta avançada termina sozinha e não fica rodando por padrão. |
| Jogos | Referências oficiais datadas para CS2/Fortnite, conferência parcial de RAM/threads e orientação específica de teste; outros jogos por contexto manual. Executável, versão, cena, resolução, gráficos/API, frequência, limite de FPS, sincronização e shaders registrados. ETW sem injeção; nenhum arquivo de jogo ou anticheat é alterado. |
| Limites de automação | Nível 1: leitura. Nível 2: somente alvos gerenciados selecionados em plano aprovado. Nível 3: revisão/intervenção manual; fora do Intelligence automático. Ferramentas legadas sensíveis exigem revisão explícita e execução isolada. |
| Preservação e verificação | Inventário completo em `INTELLIGENCE-INVENTORY.md`, testes de políticas/diário/reversão/métricas, verificador Windows e fluxo da interface em VM. Limites de validação devem acompanhar a versão publicada. |

## Alvos gerenciados nesta versão

O catálogo preserva 619 funções, mas elas não se tornam 619 ajustes automáticos e restauráveis por mera importação. O motor só escreve nos seguintes alvos, com transições e pré-requisitos fixos:

1. **Modo de Jogo**: somente preferência DWORD explicitamente `0` para `1`. Preferência ausente não significa desligada.
2. **Captura histórica de jogos**: somente DWORD explicitamente `1` para `0`, com aviso da perda de gravação retroativa. Estar habilitada não prova consumo durante o jogo.
3. **Plano de energia**: somente Economia de energia para Equilibrado já instalado, em desktop identificado e na tomada. Não cria planos nem muda firmware/limites de notebooks.

Recomendação não equivale a autorização. Nenhum ajuste está selecionado inicialmente; somente operações de nível 2 classificadas como recomendadas ou potencialmente úteis podem entrar no plano. Segurança, BIOS/UEFI, firmware, overclock, undervolt e anticheat não são executados pelo motor.

As funções originais permanecem nas áreas existentes. Suas implementações não foram reescritas ou declaradas restauráveis pelo novo motor. A restauração oferecida por um script legado pode apenas definir um padrão, sem recuperar o estado anterior. Isso é informado antes da execução manual. Alterações sensíveis não podem ser agrupadas silenciosamente em uma fila.

## Arquitetura

- `GoatTweaks.Core/Intelligence`: modelos, base de conhecimento, regras, políticas, diário, aplicação/reversão transacional, CSV, comparação e relatório HTML.
- `GoatTweaks.Windows`: adaptadores de leitura/escrita do Windows, PDH, inventário reutilizado, sensores WMI existentes e integrações ETW.
- `GoatTweaks/Intelligence`: fluxo WPF em pt-BR integrado à navegação e aos recursos visuais existentes.
- `GoatTweaks.Intelligence.Tests`: testes isolados; dados sintéticos aparecem somente em fixtures de teste, nunca no painel do cliente.
- `GoatTweaks.IntelligenceProbe`: verificação real em VM descartável; é bloqueado fora do ambiente explícito de CI.

Nenhum comando arbitrário do JSON é executado. Os identificadores de configuração resolvem alvos fixos no adaptador Windows. A autorização expira em 30 minutos e deixa de valer se o conteúdo do plano, o perfil ou o contexto mudar. A máquina/usuário/Windows/driver é conferida antes de aplicar.

O diário é gravado com flush antes da primeira escrita e substituição atômica do arquivo. Se o backup não puder ser persistido, não há aplicação. Uma falha ao salvar após a mudança interrompe as próximas escritas; o registro preparado já salvo contém os valores necessários para a revisão/reversão. Registros corrompidos são apresentados como erro e não sobrescritos silenciosamente. Um bloqueio entre processos evita duas execuções gerenciadas simultâneas.

## Coleta e sensores

CPU/RAM usam mecanismos nativos; PDH coleta contadores com nomes em inglês, independentemente do idioma do Windows. GPU é o motor 3D mais ocupado, agrupando instâncias do mesmo motor; não é uma medida de gargalo, VRAM ou utilização integral de todas as GPUs. A média de CPU por processo é calculada por delta no intervalo, normalizada pela capacidade lógica disponível.

Temperaturas e clocks dependem de uma instância existente do LibreHardwareMonitor compartilhando sensores por WMI. GOAT não baixa nem carrega drivers de monitoramento. Sem uma fonte válida, exibe “Não disponível”. Limites genéricos de triagem térmica produzem hipóteses, nunca confirmação de throttling.

As consultas CIM/eventos/energia têm tempo limite. O monitor avançado coleta a cada 2 segundos, em janelas delimitadas. Durante essas operações e capturas, o monitor simples antigo é pausado para evitar coleta duplicada. Inventário, entradas de inicialização e planos são snapshots identificados como tal.

## Medição de quadros

PresentMon é opcional e deve ser instalado pelo técnico a partir da origem oficial. O processo é identificado pelo nome e recebe uma sessão ETW própria, sem injeção, manipulação de memória, alterações no jogo ou encerramento de sessões de terceiros. Falta de permissão/compatibilidade aparece como falha real.

CSV suportado: formato PresentMon `--v1_metrics`, com `Application`, `ProcessID`, `SwapChainAddress` e `MsBetweenPresents`. Outros campos não são tratados como equivalentes por aproximação. Processos e swap chains não são misturados. Capturas pequenas, malformadas, com excesso de intervalos inválidos ou sem o jogo informado são rejeitadas.

- FPS médio: `1000 / média(MsBetweenPresents)`.
- 1% low: `1000 / média do 1% de intervalos mais lentos` (arredondamento para cima).
- p99: percentil por posto mais próximo dos intervalos em milissegundos.

São métricas de apresentação, não uma medição direta de latência de entrada. A comparação exige condições completas e confirmadas, mesmo hardware/driver/Windows, mesma definição das métricas e duração equivalente (tolerância de 5%). O mesmo arquivo não pode comprovar uma nova medição. Arquivos importados têm procedência declarada pelo técnico. Uma única comparação não prova causalidade ou significância estatística.

## WPR e WPA

WPR grava `CPU.light` em memória por uma janela delimitada, obrigatoriamente com `-instancename GOAT_<identificador>`. Todas as operações de parada/cancelamento usam essa mesma instância. Uma versão que não aceita instâncias próprias não é usada. Nenhuma configuração global de profiling é modificada. WPA abre um ETL escolhido pelo técnico; não há alegação de interpretação automática completa do rastreio.

Documentação consultada:

- https://learn.microsoft.com/pt-br/windows-hardware/test/wpt/windows-performance-recorder
- https://learn.microsoft.com/windows-hardware/test/wpt/wpr-command-line-options
- https://learn.microsoft.com/windows/win32/power/power-schemes
- https://github.com/GameTechDev/PresentMon/blob/main/README-ConsoleApplication.md
- https://github.com/LibreHardwareMonitor/LibreHardwareMonitor

## Referências de jogos

`GameReferences.cs` mantém nome, executável, fonte oficial, data de revisão, requisitos quantitativos e orientação de teste separados da interface. CS2 e Fortnite possuem referência inicial; outros jogos continuam disponíveis por entrada manual. Quantidade de RAM e threads pode ser confrontada com a referência, mas equivalência entre modelos de CPU/GPU, VRAM e suporte de API não é inferida. Atender o mínimo quantitativo não comprova compatibilidade completa nem desempenho.

As referências foram conferidas em 22/10/2026 e podem mudar:

- Valve / Steam: https://store.steampowered.com/app/730/CounterStrike_2/?l=brazilian
- Epic Games: https://www.epicgames.com/help/c-37599050/c-32343914/a16548002?lang=en-US

Selecionar uma referência não significa que o jogo foi encontrado ou executado. Versão, cena e condições continuam sob confirmação do técnico. A troca de referência limpa a confirmação e o aquecimento de shaders. Não existe base universal de equivalência de GPUs nem consulta automática a benchmarks externos.

## Evidência de verificação

O [workflow funcional de 22/10/2026](https://github.com/JuliozXz/GOAT-Tweaks/actions/runs/39857491370) passou em Windows Server 2022 x64 (build 20348) e Windows 11 ARM64 (build 26200; aplicativo x64 sob emulação). Em cada ambiente: 36 testes do motor e 22 regressões do catálogo, inspeção de recursos incorporados, diagnóstico inicial/posterior real, persistência, bloqueio de escrita no perfil Diagnóstico, ida/volta dos três alvos no adaptador Windows e captura WPR própria. No Windows 11, aplicação autorizada, diário, retomada e restauração pelo coordenador também foram exercitados para os dois valores do Registro. As 11 telas do fluxo foram capturadas pelo aplicativo real.

A [revisão final com logo e referências de jogos](https://github.com/JuliozXz/GOAT-Tweaks/actions/runs/39860599467) acrescenta quatro testes de referências de jogos, totalizando 40 testes do motor e 22 regressões por ambiente. A navegação real captura 14 telas, incluindo janela compacta (1100 × 800) e retomada de atendimento com a execução ainda bloqueada. [Monitor](screenshots/v3-intelligence-monitor.png) e [janela compacta](screenshots/v3-intelligence-compact.png) são capturas do executável, não mockups. Testes unitários usam fixtures apenas no projeto de testes.

A publicação confere os recursos, analisa o pacote com Microsoft Defender e abre o executável em Windows isolado. Depois de publicar os cinco arquivos, também obtém o inicializador público pelo endereço `releases/latest/download/goat.ps1`, confere seus bytes contra a compilação, executa no Windows PowerShell 5.1 e verifica o hash do aplicativo baixado e a janela aberta. Essas evidências ficam no artefato `comando-v3.0.0` da execução de publicação; o aplicativo é aberto sem aplicar ações.

**Publicação confirmada:** a [execução da v3.0.0](https://github.com/JuliozXz/GOAT-Tweaks/actions/runs/39862092023) concluiu com sucesso em 22/10/2026. Microsoft Defender não encontrou ameaças nos recursos incorporados ou no pacote dessa compilação. O comando público baixou o ZIP, verificou o executável e abriu o painel, que permaneceu respondendo no Windows PowerShell 5.1. Nenhuma ação de otimização foi aplicada nessa conferência. A [release v3.0.0](https://github.com/JuliozXz/GOAT-Tweaks/releases/tag/v3.0.0) contém executável, ZIP, dois arquivos SHA-256 e inicializador.

Limites da conferência: nenhum jogo foi executado na VM; não existem ganhos de FPS medidos. O parser de PresentMon foi testado com fixtures, mas a captura de um jogo real depende da ferramenta instalada e da sua compatibilidade. WPR foi exercitado; análise manual no WPA não foi realizada. Sensores térmicos/clocks e geração de RAM não expostos pela VM permaneceram indisponíveis. A retomada de dados foi conferida, mas a VM não foi reiniciada fisicamente no meio de um atendimento. Estabilidade prolongada e identificação em modelos físicos distintos exigem validação no computador atendido.

## Limites que devem permanecer visíveis

Uma VM não valida desempenho em jogos nem sensores de hardware físico. Sistemas Windows Server permitem diagnóstico na conferência, mas não são classificados como clientes elegíveis para ajustes automáticos. Regras não devem ser ampliadas sem teste de compatibilidade, backup e reversão. Ausência de novos eventos de erro não comprova estabilidade de longo prazo. Ferramentas, drivers e jogos externos podem impor restrições que precisam ser verificadas no computador atendido.

O logo enviado pelo usuário permanece preservado em `Assets/goat-logo.png`. A versão aprimorada para exibição fica em `Assets/goat-logo-enhanced.png`; o desenho é exibido ao lado da marca no painel preto e vermelho.
