# GOAT Tweaks

Painel de atendimento para Windows com identificação do computador e 619 ações de Ultimate, winutil, RyTuneX e WinSux organizadas por área, preservando os arquivos originais incorporados.

## Abrir a interface

Execute no PowerShell:

```powershell
irm https://github.com/JuliozXz/GOAT-Tweaks/releases/latest/download/goat.ps1 | iex
```

Requer Windows 10 22H2 ou Windows 11 de 64 bits e PowerShell 5.1 ou posterior. A versão publicada inclui o runtime .NET; o cliente não precisa instalar o SDK. Abrir a interface não aplica ajustes no Windows. A elevação UAC é solicitada ao aplicar ações administrativas.

Na primeira abertura de cada versão, o inicializador baixa um pacote ZIP compacto, confere o SHA-256 do pacote e do executável extraído e abre a janela. Nas próximas aberturas, ele verifica e reaproveita o executável local sem baixar novamente o aplicativo ou consultar um arquivo de checksum separado. O comando ainda obtém o pequeno inicializador para localizar a versão mais recente. O cache é separado por versão. Nenhuma política permanente de execução do PowerShell é alterada.

O runtime .NET está incluído no aplicativo e é responsável por boa parte do tamanho. O pacote comprimido reduz o download, e os recursos dos scripts só são preparados ao executar a fila. A primeira extração e a análise do Windows podem levar mais tempo do que as próximas aberturas.

## Uso

1. Confira os cartões da **Visão geral** ou abra **Meu hardware**: usuário, processador, GPU, RAM/geração e placa-mãe. A identificação ocorre em segundo plano.
2. Use **Preparar o atendimento** para encontrar criação/gerenciamento de pontos de restauração e backup do Registro. O atalho apenas abre o assunto; escolha a ação adequada ao cliente.
3. Abra um dos **11 serviços por área**, selecione o assunto e clique em **Adicionar à fila**. Parâmetros aparecem quando necessários; **Detalhes** é opcional. A busca encontra ações de todas as áreas, inclusive sem digitar acentos.
4. Abra **Revisar fila**, remova o que não deseja e clique em **Aplicar fila**. Acompanhe o resultado individual de cada ação. Os resultados permanecem visíveis até limpar ou remover os itens.

Os serviços são: **Limpeza**, **CPU e memória**, **Placa de vídeo**, **Drivers e periféricos**, **Rede e internet**, **Windows e interface**, **Privacidade e segurança**, **Energia e suspensão**, **Instalar programas**, **Preparação e reparos** e **Avançado e BIOS**. Cada serviço reúne assuntos específicos e permite filtrar pela origem.

Ferramentas como DDU, CPU-Z e desinstaladores ficam na área em que são usadas, com a indicação **Instalar programa / ferramenta**. A instalação da ferramenta e o atendimento realizado dentro dela são operações distintas.

O modo cliente inicia com ações sensíveis bloqueadas. Use o seletor **Avançado** para liberar essas ações nesta sessão. Foram removidas as confirmações digitadas AVANCADO, CONFIRMAR e WINSUX. O UAC do Windows e as interações exigidas pelos scripts originais continuam fazendo parte das ações correspondentes. O fluxo completo do WinSux exige execução isolada.

**Restaurar padrão** usa a reversão fornecida pelo projeto de origem; não recupera um backup das configurações anteriores do cliente. O catálogo e as proteções reduzem riscos, mas não garantem compatibilidade ou ausência de falhas em todas as máquinas. Mantenha um backup adequado antes de aplicar ajustes.

## Interface e execução

- HUD preto e vermelho com cartões de hardware, etapas de atendimento e navegação por área.
- CPU, GPUs e drivers gráficos, RAM instalada/utilizável, geração e módulos, placa-mãe, Windows e BIOS identificados localmente. Dados indisponíveis são informados claramente. A geração da RAM usa SMBIOS; a geração de CPU só aparece quando declarada no nome informado pelo fabricante.
- Gráficos reais de CPU e RAM dos últimos 60 segundos, com amostragem nativa a cada dois segundos, sem instalar serviços de monitoramento.
- Layout adaptado à área útil e à escala da tela, com fila em uma página própria e também ao lado do catálogo em janelas maiores.
- Lista virtualizada inclusive nos grupos por assunto, busca com intervalo curto e filtro por origem.
- A biblioteca continua navegável durante a execução; alterações na fila ficam bloqueadas até a conclusão.
- Progresso determinado e estados por ação: na fila, em execução, concluída, falhou ou cancelada.
- Leitura incremental dos registros em segundo plano. Somente novos registros atualizam a tela, sem reconstruir as listas a cada intervalo.
- A seleção mais recente substitui escolhas conflitantes ainda pendentes, com aviso na barra de status. Ações exclusivas continuam bloqueadas em filas mistas. O executor elevado confere os conflitos novamente.
- Hash da solicitação conferido antes da execução, recursos locais verificados e arquivos administrativos em pasta protegida.
- Falhas registradas em JSONL; as ações não administrativas têm tratamento individual de falhas.

Os registros ficam em `%LOCALAPPDATA%\GoatTweaks\logs` e podem ser abertos pela interface.

Detalhes da organização, identificação e limitações de hardware estão em [docs/DASHBOARD.md](docs/DASHBOARD.md). O índice de apresentação é gerado com `python tools/build_workflow.py`, sem alterar o catálogo técnico ou os scripts originais.

## Compilar e publicar

Instale .NET 10 SDK e PowerShell 7 em um ambiente de desenvolvimento e execute:

```powershell
./tools/Publish-Release.ps1 -Repository JuliozXz/GOAT-Tweaks -Version v2.0.0
```

O script compila o aplicativo e gera `dist/GOATTweaks.exe`, seu SHA-256, `dist/GOATTweaks-win-x64.zip`, o SHA-256 do ZIP e `dist/goat.ps1`. O inicializador recebe os hashes da mesma compilação e permanece em ASCII sem BOM para compatibilidade com o Windows PowerShell 5.1. O script não executa testes nem ações de ajuste do Windows.

O workflow **Publicar GOAT Tweaks** em GitHub Actions faz a mesma compilação e publica os cinco arquivos em uma release. A publicação inicia ao enviar uma alteração de versão em `release.json` para `main`. Também é possível executá-lo manualmente com a versão desejada ou enviar uma tag no formato `v1.0.0`. A release só deixa de ser rascunho depois que todos os arquivos são anexados. Versões já publicadas não são sobrescritas.

## Estado de validação

Esta revisão foi preparada sem executar uma nova suíte de testes nem ajustes no Windows, conforme a solicitação do responsável. Foram feitas compilação, preparação do pacote e conferência de integridade dos recursos. A aparência e o funcionamento completo desta interface em uma máquina Windows real ainda precisam de conferência. O plano de validação manual está em `docs/VM-TEST-PLAN.md`.

## Fontes e licença

Os arquivos originais estão em `third_party/`, com o manifesto `SHA256SUMS`. As revisões e os créditos estão em `NOTICE`. O projeto mantém a licença AGPL-3.0 e as licenças dos componentes incorporados. A documentação de análise e de cobertura está em `docs/`.
