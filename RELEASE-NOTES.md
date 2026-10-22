GOAT Tweaks v2.0.2 corrige a falha de abertura com a mensagem “Índice de serviços ausente”.

O compilador interpretava o sufixo pt-BR do JSON como uma indicação de idioma e colocava o índice em um assembly de recursos separado. O carregador do painel procurava esse conteúdo no assembly principal e encerrava a abertura.

- O índice agora é incorporado explicitamente ao assembly principal, com WithCulture=false e o mesmo nome de manifesto esperado pelo carregador.
- A publicação passa a inspecionar os assemblies compilados sem executar o aplicativo: exige o recurso no local correto, compara os bytes com os JSON de origem e confere a cobertura das 619 ações nas 11 áreas.
- Um índice ausente, em um assembly de idioma, divergente ou incompleto impede a criação do pacote publicado.
- Mantidos o painel preto e vermelho, a identificação de hardware, os serviços por assunto e o download com tentativas, retomada e mecanismo alternativo.

Esta correção trata do empacotamento do painel. Não altera os scripts originais ou suas ações no Windows.

Validação local: compilação e leitura dos recursos efetivamente incorporados; índice presente no assembly principal e correspondência das 619 ações nas 11 áreas. Conforme solicitado, sem executar testes nem ajustes de Windows. A interface não foi aberta neste ambiente.

Publicação pendente: foi recebido um relato de bloqueio do ZIP pelo antivírus para a versão anterior. A captura mostra “Vírus detectado”, sem o nome da ameaça ou o componente identificado. O conteúdo original inclui rotinas que alteram proteções do Windows; essa constatação não determina a causa da detecção. Esta proposta corrige o índice, mas não representa uma liberação ou certificação de segurança do pacote.
