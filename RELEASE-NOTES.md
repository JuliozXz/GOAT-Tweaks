GOAT Tweaks v2.0.0 transforma a biblioteca em um painel de atendimento por área do computador.

- Novo HUD preto e vermelho, com navegação lateral, cartões de hardware, gráficos e serviços organizados. O layout reorganiza os cartões e a fila conforme o espaço disponível e a escala da tela.
- Identificação local do usuário, computador, processador, placas de vídeo, versões dos drivers gráficos, RAM instalada, RAM utilizável, módulos, geração da memória e placa-mãe. A tela detalhada também mostra Windows e BIOS/UEFI.
- Geração de memória obtida do SMBIOS, incluindo DDR3, DDR4, DDR5 e LPDDR. Campos ausentes são identificados. A geração de CPU só é exibida quando consta explicitamente no nome informado pelo fabricante.
- Consulta de hardware em segundo plano, com limite de tempo, resultados parciais e atualização manual. Abrir o painel apenas consulta os dados locais.
- Gráficos reais de CPU e RAM dos últimos 60 segundos, atualizados a cada dois segundos com APIs nativas do Windows. O histórico tem tamanho limitado e usa relógio monotônico. Máquinas com múltiplos grupos de processadores recebem indicação de leitura de CPU indisponível.
- Todas as 619 ações distribuídas em 11 serviços: Limpeza; CPU e memória; Placa de vídeo; Drivers e periféricos; Rede e internet; Windows e interface; Privacidade e segurança; Energia e suspensão; Instalar programas; Preparação e reparos; Avançado e BIOS.
- Assuntos dentro de cada serviço, filtro por origem, busca global sem exigir acentos e nomes das ações em português, preservando os nomes técnicos dos programas e os nomes originais nos detalhes.
- Distinção visível entre instalar uma ferramenta, remover um aplicativo, executar um script interativo e aplicar uma ação do sistema.
- Atendimento guiado em quatro etapas: identificar a máquina, preparar a recuperação, escolher serviços e revisar/aplicar a fila. O atalho de preparação abre o assunto de pontos de restauração e backup do Registro.
- Revisão da fila em tela própria; em janelas maiores, a fila também aparece ao lado do catálogo. Cada ação indica sua área e mantém seu resultado após a execução.
- Mantidos o modo avançado sem confirmação digitada, os parâmetros dos scripts, a elevação UAC do Windows, as verificações de integridade, os conflitos e a execução isolada do WinSux.
- Mantida a abertura por uma linha no PowerShell, com download compacto por versão, verificação dos hashes e reutilização da cópia local válida. Inicializador ASCII sem BOM, compatível com Windows PowerShell 5.1.

Os scripts originais incorporados e seus créditos e licenças permanecem preservados. A classificação organiza o atendimento; ela não certifica que um ajuste seja apropriado para todas as máquinas. Restaurar padrão segue a origem e não recupera um backup anterior do cliente.

Requisitos: Windows 10 22H2 ou Windows 11 de 64 bits e PowerShell 5.1 ou posterior. Runtime .NET incluído.

Validação desta revisão: compilação, empacotamento e conferência de integridade. Conforme solicitado, não foram executados novos testes nem ajustes de Windows. A conferência visual e funcional do painel em uma máquina Windows ainda está pendente.
