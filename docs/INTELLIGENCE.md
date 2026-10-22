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
| Jogos | Contexto do executável, versão, cena, resolução, gráficos/API, frequência, limite de FPS, sincronização e aquecimento de shaders. ETW sem injeção; nenhum arquivo de jogo ou anticheat é alterado. |
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

## Limites que devem permanecer visíveis

Uma VM não valida desempenho em jogos nem sensores de hardware físico. Sistemas Windows Server permitem diagnóstico na conferência, mas não são classificados como clientes elegíveis para ajustes automáticos. Regras não devem ser ampliadas sem teste de compatibilidade, backup e reversão. Ausência de novos eventos de erro não comprova estabilidade de longo prazo. Ferramentas, drivers e jogos externos podem impor restrições que precisam ser verificadas no computador atendido.

O logo enviado pelo usuário permanece como arquivo original preservado. O acabamento visual é a etapa final, após a conferência funcional.
