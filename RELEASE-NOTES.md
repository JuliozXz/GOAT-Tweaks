GOAT Tweaks v1.1.0 atualiza a interface, a execução da fila e a inicialização.

- Interface preta com detalhes vermelhos, controles escuros, textos mais compactos e layout que respeita a área útil da tela.
- Adição direta à fila para ações sem parâmetros. Detalhes ficam disponíveis por escolha do usuário.
- Remoção das confirmações digitadas AVANCADO, CONFIRMAR e WINSUX. O modo avançado é liberado por um seletor; o UAC continua sendo solicitado pelo Windows para ações administrativas.
- Progresso determinado e resultado por ação, mantendo a fila visível após a execução.
- A biblioteca continua navegável enquanto a fila roda. A edição da fila permanece bloqueada até terminar.
- Leitura incremental de JSONL em segundo plano, sem reler o arquivo inteiro nem substituir as listas a cada intervalo. Os registros completos continuam salvos no computador.
- Processos iniciados fora da thread da interface, evitando bloquear a renderização ao preparar a execução e solicitar autorização.
- Escolhas conflitantes ainda pendentes são substituídas pela seleção mais recente, com aviso na barra de status. Ações exclusivas, parâmetros e integridade continuam validados.
- Download inicial por ZIP compacto, com verificação do SHA-256 do pacote e do executável. A cópia local válida é reutilizada sem baixar o aplicativo ou consultar um checksum separado.
- Preparação dos scripts adiada até a execução da fila. O inicializador continua em ASCII sem BOM para Windows PowerShell 5.1.

O aplicativo reúne 619 ações dos quatro projetos e mantém seus arquivos originais, créditos e licenças. Interações próprias dos scripts originais continuam disponíveis quando exigidas pela ação.

Requisitos: Windows 10 22H2 ou Windows 11 de 64 bits e PowerShell 5.1 ou posterior. O runtime .NET está incluído.

Abrir a interface não aplica ajustes. Restaurar padrão usa a implementação da origem e não recupera um backup anterior do cliente.

A publicação compila e empacota o aplicativo. Não foram executados novos testes nem ajustes de Windows. As correções de renderização foram feitas a partir dos problemas identificados no código; a conferência visual e a execução completa no Windows do usuário ainda precisam ser realizadas.
