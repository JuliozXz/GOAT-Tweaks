# GOAT Tweaks

Interface única para revisar e executar 619 ações de Ultimate, winutil, RyTuneX e WinSux, preservando os arquivos originais incorporados.

## Abrir a interface

Depois que uma release completa estiver publicada neste repositório, execute no PowerShell:

```powershell
irm https://github.com/JuliozXz/GOAT-Tweaks/releases/latest/download/goat.ps1 | iex
```

Requer Windows 10 22H2 ou Windows 11 de 64 bits e PowerShell 5.1 ou posterior. A versão publicada inclui o runtime .NET; o cliente não precisa instalar o SDK. Abrir a interface não aplica ajustes no Windows. A elevação UAC é solicitada ao aplicar ações administrativas.

O inicializador baixa o executável da mesma versão da release do script, confere o SHA-256 e abre a janela. O cache é separado por versão. Nenhuma política permanente de execução do PowerShell é alterada.

## Uso

1. Pesquise a ação ou escolha uma categoria.
2. Abra **Prévia e adicionar** e confira o que será executado.
3. Revise a **Fila de ações**. É possível remover ações individualmente.
4. Clique em **Aplicar fila** e acompanhe os registros.

O modo cliente inicia com ações sensíveis bloqueadas. Ações com alterações de boot, drivers, segurança, serviços, atualizações, exclusão ou remoção de apps exigem habilitar o modo avançado digitando AVANCADO. Cada ação sensível ainda exige CONFIRMAR. O fluxo completo do WinSux exige também WINSUX e execução isolada.

**Restaurar padrão** usa a reversão fornecida pelo projeto de origem; não recupera um backup das configurações anteriores do cliente. O catálogo e as proteções reduzem riscos, mas não garantem compatibilidade ou ausência de falhas em todas as máquinas. Mantenha um backup adequado antes de aplicar ajustes.

## Interface e execução

- Tema escuro, destaque âmbar e navegação por categorias.
- Lista virtualizada e busca com intervalo curto para evitar recriar centenas de controles a cada tecla.
- Proteção contra ações duplicadas e fila bloqueada durante a execução.
- Conflitos conferidos na interface e novamente no executor elevado.
- Hash da solicitação conferido antes da execução, recursos locais verificados e arquivos administrativos em pasta protegida.
- Falhas registradas em JSONL; as ações não administrativas têm tratamento individual de falhas.

Os registros ficam em `%LOCALAPPDATA%\GoatTweaks\logs` e podem ser abertos pela interface.

## Compilar e publicar

Instale .NET 10 SDK e PowerShell 7 em um ambiente de desenvolvimento e execute:

```powershell
./tools/Publish-Release.ps1 -Repository JuliozXz/GOAT-Tweaks -Version v1.0.0
```

O script executa a compilação e gera `dist/GOATTweaks.exe`, `dist/GOATTweaks.exe.sha256` e `dist/goat.ps1`. Ele não executa testes nem ações de ajuste do Windows.

O workflow **Publicar GOAT Tweaks** em GitHub Actions faz a mesma compilação e publica os três arquivos em uma release. A publicação inicia ao enviar uma alteração de versão em `release.json` para `main`. Também é possível executá-lo manualmente com a versão desejada ou enviar uma tag no formato `v1.0.0`. A release só deixa de ser rascunho depois que todos os arquivos são anexados. Versões já publicadas não são sobrescritas.

## Estado de validação

Esta revisão foi preparada sem executar uma nova suíte de testes nem ajustes no Windows, conforme a solicitação do responsável. O histórico recuperado da Devin informava compilação anterior e testes automáticos aprovados, mas a sessão de ponta a ponta no Windows foi interrompida por falta de créditos. Os testes Windows continuam sem confirmação de conclusão. O plano está em `docs/VM-TEST-PLAN.md`.

## Fontes e licença

Os arquivos originais estão em `third_party/`, com o manifesto `SHA256SUMS`. As revisões e os créditos estão em `NOTICE`. O projeto mantém a licença AGPL-3.0 e as licenças dos componentes incorporados. A documentação de análise e de cobertura está em `docs/`.
