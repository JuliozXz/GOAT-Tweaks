GOAT Tweaks v3.0.0 — GOAT Intelligence

O painel evolui para um fluxo local de diagnóstico, plano autorizado, alterações rastreáveis e validação. O motor usa regras explicáveis sobre evidências reais; não estima FPS nem aplica todos os tweaks disponíveis.

- GOAT Intelligence e GOAT Service Mode: identificação, diagnóstico, plano, execução isolada, conferência de reinício, validação e relatório HTML.
- Cinco perfis: FPS Máximo, Competitivo, Equilibrado, Diagnóstico e Personalizado. Diagnóstico inicia por padrão e bloqueia alterações e restaurações.
- Inventário de CPU, GPU/drivers, RAM/módulos/geração disponível, placa-mãe, firmware/UEFI, alimentação, armazenamento, inicialização, processos e eventos. Dados não disponíveis são informados explicitamente.
- Recomendações com evidência, impacto provável, confiança, risco, compatibilidade e restauração. Uso elevado de CPU/GPU ou temperatura isolada produz hipótese, sem afirmar um gargalo.
- Três alvos gerenciados: preferência explícita do Modo de Jogo, captura histórica e Economia de energia → Equilibrado em desktop elegível. Cada alteração exige seleção e autorização, backup específico antes da escrita, verificação e reversão protegida contra mudanças externas.
- Atendimentos persistentes, retomada sem reaplicação automática e aprovação de uso único. Nenhuma alteração de BIOS, firmware, voltagem, overclock, undervolt, anticheat ou desativação de segurança pelo motor automático.
- PresentMon opcional/CSV real para FPS médio, 1% low, frametime e p99. Condições divergentes tornam a comparação inconclusiva. WPR/WPA opcionais usam rastreios próprios, sem interferir em gravações de terceiros.
- CS2 e Fortnite possuem referências oficiais datadas e conferência quantitativa parcial dos requisitos; outros jogos continuam disponíveis por contexto manual. Atender um mínimo não comprova compatibilidade completa nem FPS.
- As 619 ações originais permanecem nas 11 áreas. Ferramentas sensíveis exigem revisão manual explícita e execução isolada; não foram declaradas automaticamente reversíveis ou benéficas.
- Identidade preta e vermelha preservada e logo enviada pelo usuário incorporada ao aplicativo.

Validação: 40 testes do motor e 22 regressões do catálogo por ambiente, verificações nativas no Windows x64 e Windows 11 ARM64, WPR e capturas reais da interface. A revisão inclui janela compacta e retomada de atendimento. Métodos, escopo e evidências estão em `docs/INTELLIGENCE.md` e nos artefatos de GitHub Actions. O workflow de publicação também confere Defender, abertura e o comando público no Windows PowerShell 5.1.

Uma VM não comprova estabilidade em todos os computadores ou melhora em jogos. Temperaturas e clocks dependem de sensores existentes; benchmarks dependem de medição e condições comparáveis. Não há garantia de ganho de FPS ou ausência de falhas em todo hardware.

Abrir no PowerShell:

```powershell
irm https://github.com/JuliozXz/GOAT-Tweaks/releases/latest/download/goat.ps1 | iex
```
