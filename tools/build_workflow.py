#!/usr/bin/env python3
"""Build the explicit Portuguese service index; never change the upstream catalog or scripts."""
import html
import json
from collections import Counter
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

SERVICES = [
    ("clean", "01", "Limpeza", "Limpeza e remoção de aplicativos", "Arquivos temporários, espaço em disco e remoção de programas. Ferramentas de desinstalação ficam juntas, com a indicação de que precisam ser instaladas.", "\ue74d"),
    ("cpu", "02", "CPU e memória", "Processador e memória RAM", "Diagnóstico de CPU e RAM, compressão de memória, cache e agendamento. Consulte o fabricante para drivers específicos do processador e do chipset.", "\ue950"),
    ("gpu", "03", "Placa de vídeo", "Placa de vídeo e drivers gráficos", "Limpeza e instalação de drivers de vídeo, ferramentas de diagnóstico e ajustes de renderização, separados por assunto.", "\ue7f4"),
    ("devices", "04", "Drivers e periféricos", "Drivers, armazenamento e periféricos", "Drivers do sistema, armazenamento, USB, mouse, teclado, controles, áudio e monitor. As ferramentas são as disponibilizadas pelas origens.", "\ue772"),
    ("network", "05", "Rede e internet", "Rede, internet e DNS", "Adaptadores e drivers de rede, provedores DNS, protocolos, conectividade e reparos da conexão.", "\ue701"),
    ("windows", "06", "Windows e interface", "Windows, interface e inicialização", "Menu Iniciar, barra de tarefas, aparência, recursos opcionais e aplicativos em segundo plano.", "\ue770"),
    ("privacy", "07", "Privacidade e segurança", "Privacidade e segurança", "Telemetria, permissões, proteção do Windows e criptografia. Os controles sensíveis continuam identificados pelo risco e pelo modo avançado.", "\ue72e"),
    ("power", "08", "Energia e suspensão", "Energia, bateria e suspensão", "Planos de energia, hibernação e comportamento de suspensão. Observe as indicações originais para notebooks.", "\ue7e8"),
    ("apps", "09", "Instalar programas", "Instalação de programas", "Programas separados por finalidade: trabalho, documentos, comunicação, multimídia, jogos e desenvolvimento. Cada instalação é adicionada individualmente à fila.", "\ue71d"),
    ("repair", "10", "Preparação e reparos", "Preparação, reparos e recuperação", "Pontos de restauração, manutenção do Windows, verificação de integridade e ferramentas de recuperação. Um ponto de restauração não substitui um backup dos arquivos do cliente.", "\ue90f"),
    ("advanced", "11", "Avançado e BIOS", "Avançado, BIOS e serviços", "Inicialização, BIOS, serviços e rotinas que abrangem várias áreas. O fluxo completo do WinSux permanece isolado, sem mistura com a fila de outros serviços.", "\ue713"),
]

# Exact title overrides avoid assigning a tool to an unrelated area because its description
# happens to mention a component. App installers stay explicitly marked as installers.
AREA = {}
def group(service, topic, titles):
    for title in titles.split("|"):
        title = title.strip()
        if title:
            AREA[title.casefold()] = (service, topic)

group("clean", "Arquivos temporários e disco", "Cleanup|Disk Cleanup - Run|Temporary Files - Remove|Disable Reserved Storage|Storage Sense - Disable|Optimize NTFS Settings|Enable Low Disk Space Checks")
group("clean", "Remoção de aplicativos", "Bloatware|Bloatware Legacy Apps Check|Bloatware Legacy Features Check|Bloatware UWP Apps Check|Bloatware UWP Features Check|Bloatware TaskMgr Check|Microsoft Edge - Remove|Microsoft OneDrive - Remove|Windows AI - Disable And Remove|Widgets - Remove|Programs and Features")
group("clean", "Navegadores e componentes", "Brave Browser - Debloat|Microsoft Edge - Debloat|Edge & WebView")
group("clean", "Ferramentas de desinstalação", "Bulk Crap Uninstaller|Revo Uninstaller|Wise Program Uninstaller (WiseCleaner)")
group("cpu", "Diagnóstico de CPU e RAM", "Ram Check|Storage Ram Cpu Test & Bench|CPU-Z|Cinebench R23|HWiNFO|HWMonitor|Windows Memory Diagnostic (restart now)|Windows Memory Diagnostic (next boot)")
group("cpu", "Memória, cache e compressão", "Memory Compression|MMAgent Features|Disable SysMain|Disable Prefetch & Superfetch|Enable Large System Cache|svchost.exe Splitting - Disable|Disable Service Host Splitting")
group("cpu", "Processador e agendamento", "Smt Ht|Core 1 Thread 1|Priority|Timer Resolution|Enable Global Timer Resolution|Disable HPET|Disable Dynamic Tick|Optimize System Profile|Optimize Foreground App Performance")
group("gpu", "Limpeza e instalação de drivers", "Driver Clean|Driver Updated Install|Driver Updated Install & Settings|Driver Debloat Install & Settings|Display Driver Uninstaller|NVCleanstall")
group("gpu", "NVIDIA, AMD e Intel", "Nvidia Settings|Amd Settings|Intel Settings|Hdcp|P0 State|Ulps|GPU Driver Performance Tweaks")
group("gpu", "Renderização e jogos", "Msi Mode|Hags Windowed|Mpo|Hardware Legacy Flip|Hardware Composed Independent Flip|ReBar Force|Multiplane Overlay|Gamemode|Gamebar|Game Mode|Enable Gaming Mode|Enable Fullscreen Optimizations|Disable Game Bar")
group("gpu", "Diagnóstico e ferramentas", "Gpu Check|Gpu Test & Bench|GPU-Z|MSI Afterburner")
group("gpu", "DirectX e bibliotecas", "DirectX|C++")
group("devices", "Drivers do sistema", "Updates Drivers Block|Exclude Drivers From Windows Updates|Snappy Driver Installer Origin|Driver Whql Secure Boot Bypass|Prevent Device Companion Apps|Razer Software Auto-Install - Disable|Logitech Download Assistant Auto-Install - Disable")
group("devices", "Armazenamento e NVMe", "Storage Check|NVME Faster Driver|Write Cache Buffer Flushing|Crystal Disk Info|Crystal Disk Mark|CHKDSK scan (read-only)|CHKDSK /f on next boot (system drive)")
group("devices", "Mouse, teclado e controles", "Pointer Precision|Scaling Higher No Accel|Background Polling Rate Cap|Mouse Polling Rate Test|Controller Overclock|Controller Polling Rate Test|Keyboard Shortcuts|Mouse Acceleration|Mouse Properties|Disable Mouse Hover Time|Optimize Keyboard Latency|Disable Mouse Acceleration|Sticky Keys|Disable Sticky Keys|Disable Windows Ink|Disable Spelling and Typing Features|Num Lock on Startup")
group("devices", "USB e gerenciamento de dispositivos", "Device Manager Power Savings & Wake|Disable USB Power Saving|Printer Panel|Disable Print Service|Disable Fax Service|Computer Management")
group("devices", "Monitor e áudio", "Scaling|Monitor Optimization|Resolution Refresh Rate|Sound|Loudness EQ|Sound Settings")
group("network", "Drivers e adaptadores", "Network Driver|Network Adapter Power Savings & Wake|Network Connections")
group("network", "Protocolos e conectividade", "Network IPv4 Only|IPv6 - Set IPv4 as Preferred|Teredo - Disable|IPv6 - Disable|Disable Network Data Usage Monitor|Disable Home Group|Disable SMBv1|Disable SMBv2|Disable Wi-Fi Sense|Network File System (NFS) - Enable|OpenSSH Server - Enable")
group("network", "Reparos de conexão", "Network - Reset|Flush DNS cache|Release / renew IP|Reset TCP/IP and Winsock|Reset DNS to DHCP (adapter)")
group("network", "Diagnóstico e ferramentas", "Network Bufferbloat Test|Advanced IP Scanner|Angry IP Scanner|NetBird|Mullvad VPN|Proton VPN|WireGuard|Wireshark|Tailscale|Nmap")
group("network", "DNS e filtros", "DNS - Set to:")
group("network", "Protocolos e conectividade", "Delivery Optimization - Disable|Disable Cast to Device")
group("privacy", "Proteção do Windows", "Defender|Defender Optimize|Firewall|UAC|Core Isolation|Spectre Meltdown|Data Execution Prevention|File Download Security Warning|RDP Unsigned File Warnings - Disable|Disable Smart Screen|Disable VBS|Microsoft Defender quick scan|Update Defender signatures|Security and Maintenance|Windows Defender Firewall|Disable Safe Search Mode")
group("privacy", "Criptografia e acesso", "BitLocker|BitLocker - Disable|Disable Page File Encryption|Disable Remote Assistance|Disable Remote Registry|Disable Biometrics features|Disable Find My Device|Disable Automatic Restart Sign-On|AutoLogon - Run")
group("privacy", "Telemetria e histórico", "Activity History - Disable|Location Tracking - Disable|Telemetry - Disable|O&O ShutUp10++ - Run|Disable Error Reporting|Disable Quick Access History|Disable Windows Recall|Disable WPBT Execution|Windows Platform Binary Table (WPBT) - Disable")
group("privacy", "Telemetria e permissões", "Adobe URL Block List - Enable|Disable Cloud Clipboard")
group("power", "Planos de energia", "Power Plan|Power Panel|Ultimate Performance Profile - Enable|Ultimate Performance Profile - Disable|Disable Power Throttling")
group("power", "Suspensão e hibernação", "Hibernation - Disable|Disable Hibernation|S0 Sleep Network Connectivity|S3 Sleep|System Tray Battery Percentage")
group("repair", "Pontos de restauração e backup", "Restore Point|Restore Point - Create|Registry Backup (Daily Task 12:30am) - Enable|Windows Restore")
group("repair", "Integridade do Windows", "System Corruption Scan - Run|DISM scan health|DISM restore health|SFC verify only|SFC scannow|DISMTools")
group("repair", "Windows Update e componentes", "Updates Pause|Windows Update - Reset|WinGet - Reinstall|Windows Updates|Updates - Default|Updates - Security (Recommended)|Updates - Disable All|Disable Store Updates|Disable Insider Service")
group("repair", "Recuperação e reinstalação", "Factory Reset|Reinstall|Autounattend|NTLite")
group("repair", "Conta, data e região", "Account Local|Date Language Region Time|Date & Time - Set Time to UTC|Region|Time and Date|NTP Server - Enable")
group("advanced", "BIOS e inicialização", "Bios Check|To Bios|Enable Legacy Boot Menu|Legacy F8 Boot Recovery - Enable|Legacy F8 Boot Recovery - Disable|PC Build Guide")
group("advanced", "Serviços e comportamento do sistema", "Services|Services - Set to Manual|Start Search Shell Mobsync|Disable Compatibility Assistant|Disable Search|Adjust Task Timeouts|Decrease Service Timeouts|Disable Crash Dump|BSoD Verbose Mode|Logon Verbose Mode|Enable Verbose Logon|Process Explorer|Process Monitor|Autoruns")
group("advanced", "PowerShell e edição do Windows", "CTT PowerShell Profile - Install|CTT PowerShell Profile - Remove|Convert Home To Pro|Keys|Activation")
group("advanced", "Recuperação e políticas avançadas", "Disable System Restore")
group("windows", "Inicialização e segundo plano", "Startup Apps|Background Apps|Autoruns Startup Tasks & Apps Check|Background Apps - Disable|Disable Background Apps")
group("windows", "Navegadores e aplicativos do sistema", "Edge Settings|Store Settings|Notepad Settings|Disable Cortana|Disable Edge Discover Bar|Microsoft Outlook New Version")
group("windows", "Navegadores e aplicativos do sistema", "Disable CoPilot AI|Disable Windows AI|Copilot")
group("windows", "Menu Iniciar e barra de tarefas", "Start Menu Taskbar|Start Menu Layout|Start Menu Shortcuts|Start Menu Previous Layout - Enable|End Task With Right Click - Enable|Taskbar Centered Icons|Taskbar Search Icon|Taskbar Task View Icon|Start Menu Recommendations|Disable Recommended Section In Start Menu|Move Taskbar to The Left|Enable EndTask|Disable Chat|Disable My People|Disable Web Results in Search|Start Menu Bing Search")
group("windows", "Explorador de arquivos", "File Explorer Home and Gallery - Disable|File Explorer File Extensions|File Explorer Hidden Files|File Explorer Automatic Folder Discovery - Disable|Enable Long Paths|Hide File Extensions and Hidden Files|Disable Link Resolve|Add Copy/Move Context Menu|Enable Files Compact Mode|Add \"Take Ownership\" to Context Menu|Add \"Open Command Prompt Here\"|Add \"Copy File Path\" to Context Menu")

TRANSLATIONS = {}
def translations(block):
    for line in block.strip().splitlines():
        original, translated = line.split("|", 1)
        TRANSLATIONS[original.strip().casefold()] = translated.strip()

translations("""
Bios Check|Verificar BIOS e UEFI
Storage Check|Verificar armazenamento
Ram Check|Verificar memória RAM
Gpu Check|Identificar placa de vídeo
Storage Ram Cpu Test & Bench|Ferramentas de diagnóstico de disco, RAM e CPU
Gpu Test & Bench|Ferramentas de diagnóstico e benchmark de vídeo
Factory Reset|Restauração de fábrica do Windows
Account Local|Configurar conta local
Reinstall|Reinstalação do Windows
Autounattend|Instalação automatizada do Windows
Updates Drivers Block|Bloquear drivers pelo Windows Update
Network Driver|Driver do adaptador de rede
To Bios|Reiniciar para o BIOS / UEFI
BitLocker|Configurar BitLocker
Memory Compression|Configurar compressão de memória
Convert Home To Pro|Alterar edição Home para Pro
Keys|Consultar e configurar chaves do Windows
Activation|Opções de ativação do Windows
Date Language Region Time|Data, idioma, região e horário
Startup Apps|Gerenciar aplicativos de inicialização
Background Apps|Gerenciar aplicativos em segundo plano
Edge Settings|Configurações do Microsoft Edge
Store Settings|Configurações da Microsoft Store
Updates Pause|Pausar atualizações do Windows
Installers|Instaladores de programas
Driver Clean|Limpar driver de vídeo
Driver Updated Install|Instalar driver de vídeo atualizado
Driver Updated Install & Settings|Atualizar driver de vídeo e configurar
Driver Debloat Install & Settings|Instalar e configurar driver de vídeo enxuto
Nvidia Settings|Configurações NVIDIA
Amd Settings|Configurações AMD
Intel Settings|Configurações Intel
Hdcp|Configurar HDCP
P0 State|Configurar estado de desempenho P0
Msi Mode|Configurar modo de interrupção MSI
Resolution Refresh Rate|Resolução e taxa de atualização
Hags Windowed|Agendamento de GPU e jogos em janela
Start Menu Taskbar|Menu Iniciar e barra de tarefas
Start Menu Layout|Layout do menu Iniciar
Start Menu Shortcuts|Atalhos do menu Iniciar
Context Menu|Configurar menu de contexto
Theme Black|Aplicar tema escuro
Signout Lockscreen Wallpaper Black|Fundo escuro de saída e bloqueio
User Account Pictures Black|Imagem escura da conta do usuário
Widgets|Configurar widgets
Copilot|Configurar Copilot
Gamemode|Configurar modo de jogo
Pointer Precision|Precisão do ponteiro do mouse
Scaling|Escala da tela
Bloatware|Remover aplicativos pré-instalados
Bloatware Legacy Apps Check|Revisar aplicativos clássicos desnecessários
Bloatware Legacy Features Check|Revisar recursos clássicos do Windows
Bloatware UWP Apps Check|Revisar aplicativos UWP
Bloatware UWP Features Check|Revisar recursos UWP
Bloatware TaskMgr Check|Revisar tarefas e aplicativos desnecessários
Gamebar|Configurar Xbox Game Bar
Edge & WebView|Gerenciar Edge e WebView
Notepad Settings|Configurações do Bloco de Notas
Control Panel Settings|Configurações do Painel de Controle
Sound|Configurar áudio
Loudness EQ|Equalização de volume
Device Manager Power Savings & Wake|Energia e ativação dos dispositivos
Network Adapter Power Savings & Wake|Energia e ativação do adaptador de rede
Network IPv4 Only|Configurar rede para IPv4
Write Cache Buffer Flushing|Configurar cache de gravação do disco
Power Plan|Configurar plano de energia
Timer Resolution|Configurar resolução do temporizador
UAC|Configurar Controle de Conta de Usuário
Core Isolation|Configurar isolamento de núcleo
Defender Optimize|Ajustes do Microsoft Defender
Autoruns Startup Tasks & Apps Check|Revisar tarefas e programas com Autoruns
Cleanup|Limpeza geral do computador
Restore Point|Gerenciar pontos de restauração
Scaling Higher No Accel|Escala e aceleração do ponteiro
Background Polling Rate Cap|Limitar polling rate em segundo plano
Mouse Polling Rate Test|Verificar polling rate do mouse
Controller Overclock|Configurar polling rate do controle
Controller Polling Rate Test|Verificar polling rate do controle
Monitor Optimization|Configurar e otimizar monitor
Network Bufferbloat Test|Ferramentas de diagnóstico de bufferbloat
PC Build Guide|Guia de montagem de computador
Defender|Configurar Microsoft Defender
Firewall|Configurar firewall
Spectre Meltdown|Configurar mitigações Spectre e Meltdown
Data Execution Prevention|Configurar prevenção de execução de dados
File Download Security Warning|Avisos de segurança de downloads
MMAgent Features|Recursos de gerenciamento de memória
ReBar Force|Forçar configuração de Resizable BAR
Smt Ht|Configurar SMT / Hyper-Threading
Core 1 Thread 1|Configurar primeiro núcleo e thread
Priority|Configurar prioridades do processador
Mpo|Configurar Multiplane Overlay
Hardware Legacy Flip|Configurar modo Hardware Legacy Flip
Hardware Composed Independent Flip|Configurar Hardware Composed Independent Flip
Ulps|Configurar economia de energia ULPS
Driver Whql Secure Boot Bypass|Contornar exigências WHQL e Secure Boot
Keyboard Shortcuts|Atalhos de teclado
Services|Configurar serviços do Windows
Start Search Shell Mobsync|Configurar Iniciar, pesquisa e shell
NVME Faster Driver|Opções de driver NVMe
WinSux (full pipeline)|WinSux · rotina completa e isolada
Disk Cleanup - Run|Executar Limpeza de Disco
Temporary Files - Remove|Remover arquivos temporários
Restore Point - Create|Criar ponto de restauração
Storage Sense - Disable|Desativar Sensor de Armazenamento
Disable Reserved Storage|Desativar armazenamento reservado
Brave Browser - Debloat|Remover componentes extras do Brave
Microsoft Edge - Debloat|Remover componentes extras do Edge
Microsoft Edge - Remove|Remover Microsoft Edge
Microsoft OneDrive - Remove|Remover Microsoft OneDrive
Widgets - Remove|Remover widgets
Windows AI - Disable And Remove|Desativar e remover recursos de IA do Windows
Activity History - Disable|Desativar histórico de atividades
Hibernation - Disable|Desativar hibernação
Start Menu Previous Layout - Enable|Ativar layout anterior do menu Iniciar
Microsoft Store Recommended Search Results - Disable|Desativar recomendações da Store na pesquisa
Location Tracking - Disable|Desativar rastreamento de localização
Services - Set to Manual|Configurar serviços para início manual
ConsumerFeatures - Disable|Desativar sugestões e recursos de consumo
Telemetry - Disable|Desativar telemetria
Delivery Optimization - Disable|Desativar Otimização de Entrega
BitLocker - Disable|Desativar BitLocker
svchost.exe Splitting - Disable|Desativar divisão de processos svchost
RDP Unsigned File Warnings - Disable|Desativar aviso de arquivos RDP não assinados
Date & Time - Set Time to UTC|Configurar relógio para UTC
File Explorer Home and Gallery - Disable|Desativar Início e Galeria do Explorador
Visual Effects - Set to Best Performance|Configurar efeitos visuais para desempenho
Windows Platform Binary Table (WPBT) - Disable|Desativar Windows Platform Binary Table
Prevent Device Companion Apps|Impedir aplicativos auxiliares de dispositivos
Razer Software Auto-Install - Disable|Bloquear instalação automática de software Razer
Logitech Download Assistant Auto-Install - Disable|Bloquear instalação automática do assistente Logitech
System Tray Notifications & Calendar - Disable|Desativar notificações e calendário da bandeja
Adobe URL Block List - Enable|Ativar lista de bloqueio de URLs Adobe
Right-Click Menu Previous Layout - Enable|Ativar menu de contexto clássico
End Task With Right Click - Enable|Ativar Finalizar tarefa pelo clique direito
IPv6 - Set IPv4 as Preferred|Dar preferência ao IPv4
Teredo - Disable|Desativar Teredo
IPv6 - Disable|Desativar IPv6
Background Apps - Disable|Desativar aplicativos em segundo plano
File Explorer Automatic Folder Discovery - Disable|Desativar descoberta automática de pastas
BSoD Verbose Mode|Exibir detalhes na tela azul
System Tray Battery Percentage|Exibir porcentagem da bateria
Dark Theme for Windows|Tema escuro do Windows
File Explorer File Extensions|Extensões de arquivos no Explorador
File Explorer Hidden Files|Arquivos ocultos no Explorador
Logon Verbose Mode|Exibir detalhes durante o logon
Microsoft Outlook New Version|Nova versão do Microsoft Outlook
Scrollbars Always Visible|Manter barras de rolagem visíveis
Multiplane Overlay|Configurar Multiplane Overlay
Mouse Acceleration|Aceleração do mouse
Num Lock on Startup|Num Lock na inicialização
Window Snapping|Encaixe de janelas
S0 Sleep Network Connectivity|Conexão de rede durante suspensão S0
S3 Sleep|Suspensão S3
Settings Home Page|Página inicial das Configurações
Start Menu Bing Search|Pesquisa Bing no menu Iniciar
Logon Screen Acrylic Blur|Desfoque da tela de logon
Lock Screen - Disable|Desativar tela de bloqueio
Start Menu Recommendations|Recomendações do menu Iniciar
Sticky Keys|Teclas de aderência
Taskbar Centered Icons|Ícones centralizados na barra de tarefas
Taskbar Search Icon|Ícone de pesquisa da barra de tarefas
Taskbar Task View Icon|Ícone de Visão de Tarefas
Game Mode|Modo de jogo do Windows
Enable Long Paths|Ativar caminhos longos
O&O ShutUp10++ - Run|Abrir O&O ShutUp10++
DNS - Set to:|Configurar provedor DNS
Ultimate Performance Profile - Enable|Ativar plano Desempenho Máximo
Ultimate Performance Profile - Disable|Remover plano Desempenho Máximo
.NET Framework (Versions 2, 3, 4) - Enable|Ativar .NET Framework 2, 3 e 4
NTP Server - Enable|Configurar servidor de horário NTP
Hyper-V - Enable|Ativar Hyper-V
Legacy Media Components (WMP, DirectPlay) - Enable|Ativar componentes multimídia legados
Windows Subsystem for Linux (WSL) - Enable|Ativar Subsistema do Windows para Linux
Network File System (NFS) - Enable|Ativar cliente NFS
Registry Backup (Daily Task 12:30am) - Enable|Agendar backup diário do Registro (00h30)
Legacy F8 Boot Recovery - Enable|Ativar recuperação legada pela tecla F8
Legacy F8 Boot Recovery - Disable|Desativar recuperação legada pela tecla F8
Windows Sandbox - Enable|Ativar Área Restrita do Windows
Install Features|Instalar recursos opcionais do Windows
AutoLogon - Run|Abrir configuração de logon automático
Windows Update - Reset|Redefinir Windows Update
Network - Reset|Redefinir rede do Windows
System Corruption Scan - Run|Verificar corrupção de arquivos do sistema
WinGet - Reinstall|Reinstalar WinGet
Computer Management|Abrir Gerenciamento do Computador
Control Panel|Abrir Painel de Controle
Mouse Properties|Abrir propriedades do mouse
Network Connections|Abrir conexões de rede
Power Panel|Abrir opções de energia
Printer Panel|Abrir configurações de impressoras
Programs and Features|Abrir Programas e Recursos
Region|Abrir configurações de região
Security and Maintenance|Abrir Segurança e Manutenção
Sound Settings|Abrir configurações de som
System Properties|Abrir propriedades do sistema
Time and Date|Abrir configurações de data e hora
Windows Defender Firewall|Abrir Firewall do Windows Defender
Windows Restore|Abrir restauração do Windows
CTT PowerShell Profile - Install|Instalar perfil CTT do PowerShell
CTT PowerShell Profile - Remove|Remover perfil CTT do PowerShell
OpenSSH Server - Enable|Ativar servidor OpenSSH
Updates - Default|Restaurar padrão do Windows Update
Updates - Security (Recommended)|Configurar atualizações de segurança
Updates - Disable All|Desativar todas as atualizações
Windows Updates|Configurar atualizações do Windows
Flush DNS cache|Limpar cache DNS
Release / renew IP|Liberar e renovar endereço IP
Reset TCP/IP and Winsock|Redefinir TCP/IP e Winsock
Reset DNS to DHCP (adapter)|Restaurar DNS automático do adaptador
Windows Memory Diagnostic (restart now)|Diagnóstico de memória · reiniciar agora
Windows Memory Diagnostic (next boot)|Diagnóstico de memória · próxima inicialização
Microsoft Defender quick scan|Executar verificação rápida do Defender
Update Defender signatures|Atualizar assinaturas do Defender
DISM scan health|Verificar imagem do Windows com DISM
DISM restore health|Reparar imagem do Windows com DISM
SFC verify only|Verificar arquivos do sistema com SFC
SFC scannow|Verificar e reparar arquivos com SFC
CHKDSK scan (read-only)|Verificar disco com CHKDSK · somente leitura
CHKDSK /f on next boot (system drive)|Agendar reparo de disco para reinicialização
""")

# RyTuneX has stable labels with a small set of verbs; translate the complete subject,
# preserving the action's actual enable/disable direction.
SUBJECTS = """
USB Power Saving|economia de energia USB
Power Throttling|limitação de energia do processador
Menu Show Delay|atraso de abertura dos menus
Mouse Hover Time|atraso ao posicionar o mouse
Keyboard Latency|latência do teclado
Mouse Acceleration|aceleração do mouse
Auto Complete|preenchimento automático
Window Shake|minimização ao agitar janelas
Global Timer Resolution|resolução global do temporizador
HPET|temporizador HPET
Dynamic Tick|tick dinâmico
SysMain|serviço SysMain
Prefetch & Superfetch|Prefetch e Superfetch
Large System Cache|cache de sistema ampliado
Network Data Usage Monitor|monitor de uso de dados de rede
Page File Encryption|criptografia do arquivo de paginação
Gaming Mode|modo de jogo
Fullscreen Optimizations|otimizações de tela cheia
System Profile|perfil de desempenho do sistema
NTFS Settings|configurações do NTFS
Foreground App Performance|desempenho de aplicativos em primeiro plano
Low Disk Space Checks|avisos de pouco espaço em disco
File Extensions and Hidden Files|extensões e arquivos ocultos
Link Resolve|resolução de atalhos
Background Apps|aplicativos em segundo plano
Crash Dump|despejo de memória após falha
Remote Assistance|Assistência Remota
Remote Registry|Registro Remoto
Service Host Splitting|divisão de processos de serviços
Task Timeouts|limites de espera de tarefas
Service Timeouts|tempo de espera dos serviços
WPBT Execution|execução de WPBT
Legacy Boot Menu|menu de inicialização legado
System Restore|Restauração do Sistema
Cortana|Cortana
Store Updates|atualizações da Microsoft Store
Smart Screen|SmartScreen
Windows Transparency|transparência do Windows
Windows Dark Mode|modo escuro do Windows
Verbose Logon|detalhes durante o logon
Home Group|Grupo Doméstico
Print Service|serviço de impressão
Compatibility Assistant|Assistente de Compatibilidade
Search|serviço de pesquisa
Fax Service|serviço de fax
Insider Service|serviço Windows Insider
Hibernation|hibernação
Sticky Keys|teclas de aderência
Windows Ink|Windows Ink
Spelling and Typing Features|recursos de ortografia e digitação
Cloud Clipboard|área de transferência na nuvem
Quick Access History|histórico do Acesso Rápido
My People|Pessoas na barra de tarefas
Cast to Device|transmissão para dispositivos
Error Reporting|relatórios de erros
Game Bar|Xbox Game Bar
Lock Screen Notifications|notificações da tela de bloqueio
All Toast Notifications|todas as notificações de aviso
Notification Center|Central de Notificações
Suggested Actions|ações sugeridas
Classic Context Menu|menu de contexto clássico
Recommended Section In Start Menu|seção Recomendados do menu Iniciar
Snap Assist|assistente de encaixe de janelas
Widgets|widgets
Chat|Chat do Windows
Files Compact Mode|modo compacto do Explorador
EndTask|opção Finalizar tarefa
Stickers|adesivos da área de trabalho
CoPilot AI|Copilot
Windows Recall|Windows Recall
Windows AI|recursos de IA do Windows
Edge Discover Bar|barra Descobrir do Edge
VBS|segurança baseada em virtualização (VBS)
Advertising ID|ID de publicidade
Bluetooth Advertising|publicidade por Bluetooth
News and Interests|Notícias e Interesses
Spotlight Features|Destaque do Windows
Tailored Experiences|experiências personalizadas
Cloud Optimized Content|conteúdo otimizado pela nuvem
Feedback Notifications|notificações de comentários
Telemetry Services|serviços de telemetria
Edge Telemetry|telemetria do Edge
Visual Studio Telemetry|telemetria do Visual Studio
Nvidia Telemetry|telemetria da NVIDIA
Chrome Telemetry|telemetria do Chrome
Firefox Telemetry|telemetria do Firefox
Diagnostics Toast|avisos de diagnóstico
Handwriting Data Sharing|compartilhamento de dados de escrita
Text Input Data Collection|coleta de dados de digitação
Input Personalization|personalização de entrada
Online Speech Privacy|reconhecimento de fala online
Safe Search Mode|Pesquisa Segura
Clipboard Sync|sincronização da área de transferência
Message Sync|sincronização de mensagens
Setting Sync|sincronização de configurações
CDP (Customer Data Platform)|Plataforma de Dados do Cliente (CDP)
Location Features|recursos de localização
Biometrics features|recursos de biometria
Find My Device|Localizar Meu Dispositivo
Voice Activation|ativação por voz
Automatic Restart Sign-On|logon automático após reinicialização
SMBv1|protocolo SMBv1
SMBv2|protocolo SMBv2
Wi-Fi Sense|Sensor de Wi-Fi
Web Results in Search|resultados da web na pesquisa
App Launch Tracking|rastreamento de abertura de aplicativos
Timeline & Activity History|Linha do Tempo e histórico de atividades
"""
subjects = dict(line.split("|", 1) for line in SUBJECTS.strip().splitlines())
for verb, translated in [("Disable", "Desativar"), ("Enable", "Ativar"), ("Optimize", "Otimizar"), ("Hide", "Ocultar"), ("Adjust", "Ajustar"), ("Decrease", "Reduzir")]:
    for subject, pt in subjects.items():
        TRANSLATIONS[(verb + " " + subject).casefold()] = translated + " " + pt
translations("""
GPU Driver Performance Tweaks|Ajustes de desempenho do driver de vídeo
Exclude Drivers From Windows Updates|Excluir drivers das atualizações do Windows
Add Copy/Move Context Menu|Adicionar Copiar/Mover ao menu de contexto
Move Taskbar to The Left|Alinhar barra de tarefas à esquerda
Add "Take Ownership" to Context Menu|Adicionar Assumir propriedade ao menu
Add "Open Command Prompt Here"|Adicionar Abrir Prompt de Comando aqui
Add "Copy File Path" to Context Menu|Adicionar Copiar caminho do arquivo ao menu
""")

APP_TOPICS = {"Utilities": "Utilitários", "Document": "Documentos e escritório", "Pro Tools": "Ferramentas técnicas",
              "Multimedia Tools": "Áudio, vídeo e imagem", "Microsoft Tools": "Ferramentas Microsoft",
              "Games": "Jogos e launchers", "Browsers": "Navegadores", "Development": "Desenvolvimento",
              "Communications": "Comunicação", "Selfhosted Tools": "Servidores e compartilhamento"}

def classify(item):
    title = html.unescape(item["title"])
    key = title.casefold()
    if item["source"] == "WinSux":
        return "advanced", "Rotina completa WinSux"
    if item["kind"] == "appx":
        return "clean", "Apps pré-instalados do Windows"
    if item["kind"] == "dns":
        return "network", "DNS e filtros"
    if key in AREA:
        return AREA[key]
    if item["kind"] == "app":
        return "apps", APP_TOPICS.get(item["category"].removeprefix("Install: "), "Utilitários")
    if item["category"] == "Privacy":
        return "privacy", "Telemetria e permissões"
    if item["category"].startswith("Powershell Profile"):
        return "advanced", "PowerShell e edição do Windows"
    if item["category"] == "Updates":
        return "repair", "Windows Update e componentes"
    if item["category"] == "Security":
        return "privacy", "Proteção do Windows"
    if item["category"] == "Features" and item["source"] == "winutil":
        return "windows", "Recursos opcionais do Windows"
    if item["category"] == "8 Advanced":
        return "advanced", "Serviços e comportamento do sistema"
    if item["category"] == "Privacy" or any(word in key for word in ["telemetry", "tracking", "privacy"]):
        return "privacy", "Telemetria e histórico"
    if any(word in key for word in ["notification", "toast", "feedback"]):
        return "windows", "Notificações e sugestões"
    if item["category"] in ["OptimizeSystem", "Features", "Customize Preferences", "Essential Tweaks", "z__Advanced Tweaks - CAUTION", "6 Windows", "Legacy Windows Panels"]:
        return "windows", "Aparência e comportamento"
    if item["category"] == "4 Installers":
        return "apps", "Instaladores da origem"
    raise ValueError("Action needs explicit placement: " + item["id"])

def main():
    catalog = json.loads((ROOT / "catalog/catalog.json").read_text(encoding="utf-8"))["items"]
    entries = []
    for item in catalog:
        service, topic = classify(item)
        original = html.unescape(item["title"])
        title = TRANSLATIONS.get(original.casefold(), original)
        if item["kind"] == "appx":
            title = "Remover " + original
        entries.append(dict(id=item["id"], service=service, topic=topic, title=title))
    document = dict(services=[dict(id=s[0], number=s[1], label=s[2], title=s[3], description=s[4], glyph=s[5]) for s in SERVICES], actions=entries)
    (ROOT / "catalog/workflow.pt-BR.json").write_text(json.dumps(document, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    counts = Counter((entry["service"], entry["topic"]) for entry in entries)
    print(f"{len(entries)} ações organizadas em {len(SERVICES)} serviços.")
    for service in SERVICES:
        print(f"{service[1]} · {service[2]}: {sum(v for (s,t),v in counts.items() if s==service[0])}")
        for (s, topic), count in counts.items():
            if s == service[0]: print(f"  {topic}: {count}")
    missing = [i["title"] for i in catalog if i["kind"] not in ["app", "appx", "dns"] and html.unescape(i["title"]).casefold() not in TRANSLATIONS]
    print("Nomes técnicos preservados:", "; ".join(missing))

if __name__ == "__main__":
    main()
