GOAT Tweaks v2.0.2 corrige a falha de abertura com a mensagem “Índice de serviços ausente”.

O compilador interpretava o sufixo pt-BR do JSON como uma indicação de idioma e colocava o índice em um assembly de recursos separado. O carregador do painel procurava esse conteúdo no assembly principal e encerrava a abertura.

- O índice agora é incorporado explicitamente ao assembly principal, com WithCulture=false e o mesmo nome de manifesto esperado pelo carregador.
- A publicação passa a inspecionar os assemblies compilados sem executar o aplicativo: exige o recurso no local correto, compara os bytes com os JSON de origem e confere a cobertura das 619 ações nas 11 áreas.
- Um índice ausente, em um assembly de idioma, divergente ou incompleto impede a criação do pacote publicado.
- Mantidos o painel preto e vermelho, a identificação de hardware, os serviços por assunto e o download com tentativas, retomada e mecanismo alternativo.

Esta correção trata do empacotamento do painel. Não altera os scripts originais ou suas ações no Windows.

Validação: compilação, conferência dos recursos e abertura real do painel em Windows x64 e Windows 11 ARM64 com emulação x64. A janela permaneceu aberta e respondendo, identificou o hardware local e não aplicou ajustes. O inicializador gerado foi analisado pelo próprio Windows PowerShell 5.1. Não foi executada uma nova suíte de testes ou qualquer ação da fila.

A revisão comparou os 327 arquivos incorporados às versões fixadas dos quatro projetos originais: conteúdos idênticos, com normalização apenas de quebras de linha em 126 arquivos do winutil. Não foi identificado componente malicioso nessa inspeção. As funções originais de Defender, SmartScreen e UAC foram preservadas conforme solicitado.

O workflow de publicação analisa os recursos e o pacote com Microsoft Defender e exige a abertura do painel antes de disponibilizar os downloads. Os resultados, as versões do mecanismo e das assinaturas e a captura da interface ficam nos artefatos da execução. Essa análise não certifica arquivos externos baixados posteriormente pelas ações nem garante o mesmo resultado em todos os antivírus. A revisão anterior desta correção registrou ausência de detecções com assinaturas 1.461.144.0; o bloqueio relatado da versão anterior não pôde ser atribuído sem o nome da ameaça. Detalhes em docs/SECURITY-REVIEW.md.
