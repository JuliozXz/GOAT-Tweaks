GOAT Tweaks v2.0.1 melhora a recuperação do download quando a conexão é encerrada antes de o painel abrir.

- O inicializador passa a usar o curl.exe nativo do Windows, com progresso e até três tentativas. Ele usa o caminho completo do executável para evitar o alias curl do Windows PowerShell 5.1.
- Transferências parciais podem ser retomadas dentro da mesma execução. Se o servidor recusar a retomada, a tentativa seguinte começa com um arquivo novo.
- Se o mecanismo nativo falhar ou estiver ausente, há duas tentativas alternativas pelo PowerShell, com UseBasicParsing, DisableKeepAlive e limite de tempo.
- As tentativas têm intervalos curtos e mensagens em português. Se todas falharem, a mensagem fornece o endereço exato do ZIP para baixar pelo navegador, extrair e abrir GOATTweaks.exe.
- Downloads exigem conexão HTTPS e o hash publicado. A validação SHA-256 continua obrigatória antes de extrair o pacote e abrir o executável.
- O inicializador permanece ASCII sem BOM, com cache por versão e limpeza dos próprios arquivos temporários.

O painel v2.0, a identificação de hardware, as 619 ações organizadas em 11 áreas e os scripts originais permanecem nesta versão.

A imagem enviada mostra uma interrupção durante Invoke-WebRequest. Essa mensagem não determina a causa exata na rede do cliente; as mudanças permitem repetir, retomar e alternar o mecanismo de download.

Validação: compilação, empacotamento e conferência dos arquivos publicados. Conforme solicitado, sem executar testes ou ações de Windows. A conexão específica que falhou no computador do usuário não foi reproduzida neste ambiente.
