GOAT Tweaks v3.1.0 — GOAT Scanner e login local

- Adiciona a tela de login local solicitada (usuário: admin; senha inicial: 321). Esse bloqueio é apenas local e não substitui autenticação real; altere as credenciais antes de uso em ambiente compartilhado.
- Adiciona o GOAT Scanner com progresso por etapas e recomendações baseadas no motor local existente.
- Remove o fluxo de fila de espera da navegação; ações do catálogo continuam disponíveis e são revisadas/executadas individualmente.
- Finalização de atendimento remove o registro da sessão e sua pasta de captura delimitada, preservando backups e logs não relacionados.
- Resultados do scanner são temporários; apenas dados mínimos de recuperação são mantidos quando uma sugestão é autorizada.
- Mantém o GOAT Intelligence, os controles de segurança, o catálogo original e os relatórios.

A publicação executa os testes de regressão, confere a compilação e os recursos incorporados, analisa o pacote com Microsoft Defender e valida o comando público no Windows PowerShell 5.1. O pacote inclui o runtime, portanto não exige instalar o SDK .NET.

Abrir no PowerShell:

```powershell
irm https://github.com/JuliozXz/GOAT-Tweaks/releases/latest/download/goat.ps1 | iex
```

Requer Windows 10 22H2 ou Windows 11 de 64 bits e PowerShell 5.1 ou posterior. O comando baixa automaticamente o pacote assinado por hashes publicados na primeira execução da versão e depois abre a cópia local. Isso evita downloads manuais, mas depende de internet na primeira abertura.

O login é uma barreira de conveniência local, não uma identidade autenticada por servidor. Uma VM de validação não comprova estabilidade em todo hardware, nem garante ganho de FPS.
