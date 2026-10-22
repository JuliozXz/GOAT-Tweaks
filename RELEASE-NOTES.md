v1.0.1 corrige a abertura pelo comando de uma linha no Windows PowerShell 5.1. O inicializador agora é publicado em ASCII sem a marca BOM, evitando o erro de análise que ocorria antes de abrir o aplicativo.

GOAT Tweaks reúne 619 ações dos quatro projetos em uma interface com prévia, fila de execução e registros.

Esta revisão acrescenta tema escuro com destaque âmbar, lista virtualizada, busca com intervalo curto, remoção individual da fila e bloqueios de execução concorrente. Ações sensíveis iniciam bloqueadas e exigem habilitação explícita do modo avançado e confirmação por ação. O executor também confere os conflitos e a integridade das solicitações.

Requisitos: Windows 10 22H2 ou Windows 11 de 64 bits e PowerShell 5.1 ou posterior. O executável é autocontido.

Abrir a interface não aplica alterações. A reversão disponível usa os padrões da origem e não substitui o backup do cliente. Não há garantia de ausência de falhas em todas as configurações do Windows.

A compilação desta release não executa testes de ponta a ponta no Windows. O histórico recuperado registrava essa etapa como pendente.
