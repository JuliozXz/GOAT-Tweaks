# Painel de atendimento v2.0

O painel usa a estrutura visual da referência fornecida pelo responsável, adaptada à identidade GOAT em preto e vermelho. A página inicial reúne identificação da máquina, CPU/RAM em tempo real, etapas do atendimento e atalhos por área. Nenhuma contagem de backups, jogo em execução, plano pago ou medição de GPU é simulada.

## Organização por serviço e assunto

`catalog/workflow.pt-BR.json` associa cada um dos 619 IDs originais a exatamente um serviço e um assunto, com um título em português. `tools/build_workflow.py` gera esse índice de forma determinística; os nomes técnicos dos programas são preservados. O catálogo original continua sendo a fonte dos comandos, parâmetros, riscos, flags e permissões. O aplicativo confere se o índice cobre o catálogo inteiro ao carregar.

1. Limpeza: arquivos temporários/disco, remoção de programas, apps do Windows, navegadores e desinstaladores.
2. CPU e memória: diagnóstico, cache/compressão e agendamento do processador.
3. Placa de vídeo: limpeza/instalação de drivers, NVIDIA/AMD/Intel, renderização, diagnóstico e bibliotecas.
4. Drivers e periféricos: drivers do sistema, NVMe/armazenamento, USB, mouse, teclado, controles, monitor e áudio.
5. Rede e internet: adaptadores/drivers, DNS, protocolos, reparos e diagnóstico.
6. Windows e interface: inicialização, aparência, menus, Explorador, notificações e recursos opcionais.
7. Privacidade e segurança: criptografia, proteção, telemetria, histórico e permissões.
8. Energia e suspensão: planos de energia, bateria, hibernação e suspensão.
9. Instalar programas: programas por finalidade, separados das ferramentas específicas das demais áreas.
10. Preparação e reparos: pontos de restauração/backup do Registro, integridade, atualizações, reinstalação e conta/região.
11. Avançado e BIOS: firmware, inicialização, serviços, PowerShell e rotina completa WinSux.

Instaladores específicos, como DDU, CPU-Z e desinstaladores, ficam na área em que são usados. O cartão indica **Instalar programa / ferramenta**: adicionar o instalador à fila não equivale a executar uma limpeza ou um diagnóstico. As ações do Ultimate podem abrir ferramentas e exigir interação própria; essa informação também é visível. A classificação não transforma ações de alto risco em recomendações automáticas.

Na busca global, o cabeçalho de cada grupo inclui serviço e assunto. Dentro de uma área, os botões de assuntos e o filtro de origem refinam a lista. Os nomes originais permanecem disponíveis em Detalhes. A lista usa virtualização inclusive no agrupamento.

O atalho de preparação abre as ações de criação/gerenciamento de pontos de restauração e backup do Registro. A opção original de desativar a Restauração do Sistema fica em Avançado. Os títulos mantêm explicitamente a direção de cada operação. Nada é pré-selecionado ou aplicado.

## Identificação de hardware

A consulta é local e somente de leitura. Um processo Windows PowerShell sem perfil, sem janela e sem elevação consulta `Get-CimInstance` com propriedades explícitas para:

- `Win32_Processor`: nome, núcleos e processadores lógicos;
- `Win32_VideoController`: nomes dos adaptadores e versão dos drivers;
- `Win32_PhysicalMemory`: capacidade, geração/tipo SMBIOS, slot, velocidade informada e fabricante;
- `Win32_BaseBoard`: fabricante e produto;
- `Win32_OperatingSystem`: identificação do Windows;
- `Win32_BIOS`: versão do firmware.

Usuário e computador vêm da sessão do aplicativo. A consulta tem limite global de 22 segundos, limite por operação CIM e pacotes separados por classe, preservando resultados anteriores se outra classe falhar. Fechar a janela cancela a consulta. Uma nova leitura preserva os dados conhecidos se uma classe ficar indisponível. Falhas não interrompem a navegação ou a fila.

A quantidade instalada é a soma dos módulos. A memória utilizável pelo Windows vem de `GlobalMemoryStatusEx` e é identificada separadamente: reserva de hardware pode fazer os números diferirem. Se os módulos não forem informados, o cartão mostra apenas o total utilizável com esse rótulo.

A geração da RAM usa o valor bruto `SMBIOSMemoryType` (DMTF Type 17). O enum legado `MemoryType` tem valores diferentes para DDR/DDR2 e é normalizado separadamente quando necessário. Campos desconhecidos e placeholders de OEM não são apresentados como identificação válida. Não se infere a geração a partir da frequência ou do tamanho da memória. A geração do processador só é exibida quando declarada no nome do fabricante; o modelo completo permanece visível.

A propriedade WMI `AdapterRAM` não é usada para informar VRAM: seu tamanho de 32 bits não representa corretamente diversas GPUs modernas. O painel não oferece um instalador universal de driver de CPU; exibe somente as ferramentas que existem no catálogo incorporado.

## Gráficos e renderização

`GetSystemTimes` fornece diferenças de tempo de CPU; o tempo de kernel inclui idle, que é subtraído do total. A primeira amostra não é apresentada como uma medição de uso. Em sistemas com múltiplos grupos de processadores, essa API tem escopo limitado; o painel indica leitura de CPU indisponível em vez de exibir o grupo primário como total da máquina.

A RAM usada é o total utilizável menos o total disponível. A amostragem ocorre a cada dois segundos com APIs nativas, sem repetir WMI/PowerShell e sem instalar serviços. O histórico fica limitado a 60 segundos e 32 registros, usando `Environment.TickCount64` para não ser afetado por ajustes no relógio do Windows.

Os gráficos usam geometrias WPF congeladas, desenhadas somente quando a página está visível. Não há animações contínuas ou biblioteca de gráficos adicional. Cartões reorganizam-se conforme a largura. A fila ocupa uma tela própria em qualquer tamanho e uma coluna lateral no catálogo em janelas a partir de 1260 DIPs. Os controles e a área útil respeitam a escala de exibição do Windows.

A leitura incremental dos registros, o isolamento dos executores, os conflitos, as validações e o cache por versão seguem a versão 1.1.0. O painel continua navegável durante a execução. Somente a edição da fila é bloqueada até o término.

## Fontes técnicas

- Microsoft: [Win32_PhysicalMemory](https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32-physicalmemory), [Win32_Processor](https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32-processor), [Win32_VideoController](https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32-videocontroller) e [Win32_BaseBoard](https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32-baseboard).
- Microsoft: [GetSystemTimes](https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-getsystemtimes) e [GlobalMemoryStatusEx](https://learn.microsoft.com/en-us/windows/win32/api/sysinfoapi/nf-sysinfoapi-globalmemorystatusex).
- DMTF: [SMBIOS 3.9.0, DSP0134, tabela Memory Device — Type](https://www.dmtf.org/sites/default/files/standards/documents/DSP0134_3.9.0.pdf).

## Validação

Esta atualização foi preparada sem executar testes ou ações de Windows, conforme a orientação do responsável. Compilação e empacotamento verificam o código e o XAML; os arquivos originais são conferidos pelo manifesto SHA-256. A aparência e o funcionamento em uma máquina Windows real ainda precisam de conferência. Essa limitação não permite garantir ausência de falhas em todos os computadores.
