# Revisão dos recursos incorporados — v2.0.2

A revisão distingue alterações administrativas intencionais de componentes maliciosos. As funções originais de Defender, SmartScreen, UAC e outros ajustes do Windows foram preservadas conforme solicitado. Nenhum ajuste foi aplicado durante esta revisão.

## Origem e integridade

Os 327 arquivos de terceiros correspondem ao manifesto SHA-256 incorporado. Seus conteúdos também foram comparados com as árvores Git dos commits fixados no NOTICE:

| Origem | Arquivos | Comparação com a origem fixada |
|---|---:|---|
| [RyTuneX](https://github.com/rayenghanmi/RyTuneX/tree/957885f53beaacfa7322581cafdb9eba5df5726a) | 84 | Bytes idênticos |
| [WinSux](https://github.com/FR33THYFR33THY/WinSux/tree/91adbfd63b7ed2eb5fcf0821fd53405a3cf63573) | 8 | Bytes idênticos |
| [Ultimate](https://github.com/FR33THYFR33THY/Ultimate/tree/a0a33a57078434201f582b0db5107dcbb502292a) | 108 | Bytes idênticos |
| [winutil](https://github.com/ChrisTitusTech/winutil/tree/2f0bdc8fca52e1ebe31a8d98216428061d3f8956) | 127 | 1 arquivo idêntico; 126 idênticos após normalizar CRLF para LF |

Não foram encontradas outras diferenças nos arquivos comparados. O manifesto local mantém os hashes dos bytes efetivamente incorporados, incluindo as quebras de linha locais. Entre esses arquivos não há executáveis PE, ELF ou arquivos ZIP identificados pelo cabeçalho; são fontes e dados. O layout do menu Iniciar codificado no WinSux está descrito em ANALYSIS.md.

A conferência de origem demonstra procedência e integridade; não certifica o comportamento ou a segurança dos projetos de origem.

## Comportamentos examinados

A abertura normal do GOAT Tweaks carrega os catálogos, desenha o painel e consulta informações locais de hardware. Não dispara a fila nem aplica os scripts de ajuste. A identificação de hardware usa consultas CIM locais e leitura de contadores de CPU/RAM; não transmite o inventário a um servidor.

Foram examinadas as ocorrências de alterações de proteção, inicialização, downloads e indicadores de coleta de credenciais, exfiltração e mineração. A ocorrência de TcpClient no benchmark DNS do winutil mede a latência de conexão à porta 53 dos provedores configurados. Não foi identificado componente malicioso por essa inspeção estática. Isso não equivale a uma prova de ausência de malware.

Os scripts de proteção do Ultimate e os estágios do WinSux realmente alteram Defender/SmartScreen/UAC e outras proteções. O WinSux também modifica o modo de inicialização, baixa etapas e reinicia o computador. Essas ações permanecem disponíveis no modo avançado. Seus efeitos são intencionais e constam no catálogo e no mapa de funções; não ocorrem apenas por abrir a interface.

## Limites dos downloads externos

Os scripts originais podem baixar instaladores e ferramentas de terceiros no momento da execução. Alguns links são mutáveis e alguns binários são redistribuídos pelo autor, sem hash fixado pelo upstream. Esses downloads não foram executados ou certificados nesta revisão. Um resultado de análise do pacote do GOAT Tweaks não cobre arquivos baixados posteriormente por uma ação.

O alerta anterior do navegador mostrava “Vírus detectado”, sem identificar a ameaça ou o arquivo interno. Esse aviso não permite atribuir a detecção a um script específico nem classificá-la automaticamente como falso positivo. O aplicativo não deve exigir uma exceção de antivírus para corrigir o erro do índice.

## Conferência no Windows

O workflow `Conferir abertura e pacote Windows` compila o pacote, verifica o recurso incorporado, confere a sintaxe do inicializador no Windows PowerShell 5.1 e abre somente a interface em ambientes isolados. O relatório registra o hash do executável, o título da janela, o sistema e uma captura da tela. Nenhuma ação da fila é aplicada.

A etapa de Microsoft Defender usa análise personalizada sem remediação para registrar resultados sobre os recursos e o pacote. Não desativa proteções, não cria exclusões e não restaura arquivos em quarentena. As versões do mecanismo, das assinaturas e os resultados ficam nos artefatos do workflow.

Na [conferência da correção](https://github.com/JuliozXz/GOAT-Tweaks/actions/runs/39716546291), os dois ambientes abriram o painel e o Microsoft Defender concluiu as análises de `third_party` e `dist` sem detecções, com assinaturas 1.461.144.0. Esse resultado pertence à compilação daquela execução. A publicação repete a análise no pacote efetivamente distribuído e registra seus próprios hashes e versões. O aviso anterior do navegador não foi reproduzido por essas análises.
