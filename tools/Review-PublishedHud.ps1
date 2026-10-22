[CmdletBinding()]
param([string]$OutputDirectory = (Join-Path $PSScriptRoot '..\artifacts\hud'))
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$OutputDirectory = (Resolve-Path $OutputDirectory).Path
$ExpectedHash = '83dc0dc1a5ddd8d17c388e142b11f03dcc4ff93b7f1526f98f2ef7001b2f2ba5'
$Title = 'GOAT Tweaks · Painel de atendimento'
$Command = 'irm https://github.com/JuliozXz/GOAT-Tweaks/releases/latest/download/goat.ps1 | iex'
$Executable = Join-Path $env:LOCALAPPDATA 'GoatTweaks\bin\v2.0.2\GOATTweaks.exe'
$WindowsPowerShell = Join-Path $env:WINDIR 'System32\WindowsPowerShell\v1.0\powershell.exe'
$Report = [ordered]@{
    version = 'v2.0.2'; command = $Command; shell = 'Windows PowerShell 5.1';
    os = [Environment]::OSVersion.VersionString; complete = $false;
    initialCachePresent = (Test-Path -LiteralPath $Executable); pages = @();
    queueAdded = 0; queueCleared = $false; appliedActions = 0;
}
$Report.automationPowerShell = $PSVersionTable.PSVersion.ToString()
$Report.hashModule = (Get-Command Get-FileHash -ErrorAction Stop).Module.Path
$App = $null

Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes, System.Windows.Forms, System.Drawing
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class GoatHudDesktop {
    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    public struct Mode {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=32)] public string device;
        public ushort spec, driver, size, extra;
        public uint fields;
        public int x, y;
        public uint orientation, fixedOutput;
        public short color, duplex, yResolution, ttOption, collate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=32)] public string form;
        public ushort logPixels;
        public uint bits, width, height, flags, frequency, icmMethod, icmIntent;
        public uint media, dither, reserved1, reserved2, panningWidth, panningHeight;
    }
    [StructLayout(LayoutKind.Sequential)] public struct Rect { public int Left,Top,Right,Bottom; }
    [DllImport("user32.dll", CharSet=CharSet.Unicode)] static extern bool EnumDisplaySettings(string device, int number, ref Mode mode);
    [DllImport("user32.dll", CharSet=CharSet.Unicode)] static extern int ChangeDisplaySettings(ref Mode mode, uint flags);
    [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr hwnd, IntPtr after, int x, int y, int width, int height, uint flags);
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hwnd);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hwnd, out Rect rect);
    [DllImport("user32.dll")] public static extern int GetSystemMetrics(int index);
    private static Mode original;
    private static bool changed;
    public static int WideScreen() {
        original = new Mode(); original.size = (ushort)Marshal.SizeOf<Mode>();
        if (!EnumDisplaySettings(null, -1, ref original)) return -100;
        var wide = original; wide.width = 1920; wide.height = 1080; wide.fields = 0x180000;
        int result = ChangeDisplaySettings(ref wide, 0); changed = result == 0; return result;
    }
    public static void RestoreScreen() { if (changed) ChangeDisplaySettings(ref original, 0); }
}
'@

function Get-Root {
    $App.Refresh()
    if ($App.HasExited -or $App.MainWindowTitle -ne $Title -or -not $App.Responding) {
        throw 'O painel encerrou, deixou de responder ou mostrou uma janela inesperada.'
    }
    return [System.Windows.Automation.AutomationElement]::FromHandle($App.MainWindowHandle)
}
function Find-Id([string]$Id) {
    $Condition = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::AutomationIdProperty, $Id)
    $Element = (Get-Root).FindFirst([System.Windows.Automation.TreeScope]::Descendants, $Condition)
    if ($null -eq $Element) { throw "Controle não encontrado: $Id" }
    return $Element
}
function Visible-Text {
    return @((Get-Root).FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition) |
        Where-Object { -not $_.Current.IsOffscreen } | ForEach-Object { $_.Current.Name } | Where-Object { $_ } | Select-Object -Unique)
}
function Capture([string]$Name) {
    $null = Get-Root
    $null = [GoatHudDesktop]::SetForegroundWindow($App.MainWindowHandle)
    Start-Sleep -Milliseconds 600
    $Rect = [GoatHudDesktop+Rect]::new()
    if (-not [GoatHudDesktop]::GetWindowRect($App.MainWindowHandle, [ref]$Rect)) { throw 'Não foi possível obter a posição do painel.' }
    $Width = $Rect.Right - $Rect.Left; $Height = $Rect.Bottom - $Rect.Top
    $Bitmap = [System.Drawing.Bitmap]::new($Width, $Height)
    $Graphics = [System.Drawing.Graphics]::FromImage($Bitmap)
    try {
        $Graphics.CopyFromScreen($Rect.Left, $Rect.Top, 0, 0, [System.Drawing.Size]::new($Width, $Height))
        $Bitmap.Save((Join-Path $OutputDirectory "$Name.png"), [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally { $Graphics.Dispose(); $Bitmap.Dispose() }
    [IO.File]::WriteAllLines((Join-Path $OutputDirectory "$Name.txt"), [string[]](Visible-Text))
    Write-Host "Captura registrada: $Name ($Width x $Height)."
}
function Resize([int]$Width, [int]$Height) {
    if (-not [GoatHudDesktop]::SetWindowPos($App.MainWindowHandle, [IntPtr]::new(-1), 12, 12, $Width, $Height, 0x40)) { throw 'Não foi possível dimensionar o painel.' }
    Start-Sleep -Milliseconds 700
    $null = Get-Root
}
function Navigate([string]$Label, [string]$ExpectedTitle) {
    $Sidebar = Find-Id 'SectionsList'
    $Condition = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::ListItem)
    $Items = $Sidebar.FindAll([System.Windows.Automation.TreeScope]::Children, $Condition)
    foreach ($Item in $Items) {
        $TextCondition = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::NameProperty, $Label)
        if ($null -eq $Item.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $TextCondition)) { continue }
        $Pattern = $Item.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
        $Pattern.Select()
        Start-Sleep -Milliseconds 550
        $Heading = (Find-Id 'SectionTitle').Current.Name
        if ($Heading -ne $ExpectedTitle) { throw "Navegação incorreta: $Label abriu $Heading." }
        $Report.pages += [ordered]@{ navigation = $Label; title = $Heading; responding = $true }
        Write-Host "Área aberta: $Label."
        return
    }
    throw "Área não encontrada na navegação: $Label"
}
function Search([string]$Text) {
    (Find-Id 'SearchBox').GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue($Text)
    Start-Sleep -Milliseconds 800
}
function Invoke-NamedButton([string]$Name) {
    $Condition = [System.Windows.Automation.AndCondition]::new(
        [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::NameProperty, $Name),
        [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::Button))
    $Button = (Get-Root).FindAll([System.Windows.Automation.TreeScope]::Descendants, $Condition) |
        Where-Object { $_.Current.IsEnabled -and -not $_.Current.IsOffscreen } | Select-Object -First 1
    if ($null -eq $Button) { throw "Botão indisponível: $Name" }
    if ($Name -eq 'Aplicar fila') { throw 'A execução de ajustes não faz parte desta conferência.' }
    $Button.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
    Start-Sleep -Milliseconds 500
}
function Open-Published([string]$Phase) {
    $Before = [DateTime]::Now
    $Watch = [Diagnostics.Stopwatch]::StartNew()
    $Bootstrap = [Diagnostics.Process]::new()
    $Bootstrap.StartInfo = [Diagnostics.ProcessStartInfo]::new($WindowsPowerShell, ('-NoLogo -NoProfile -NonInteractive -Command "' + $Command + '"'))
    $Bootstrap.StartInfo.UseShellExecute = $false
    $Bootstrap.StartInfo.CreateNoWindow = $true
    $Bootstrap.StartInfo.RedirectStandardOutput = $true
    $Bootstrap.StartInfo.RedirectStandardError = $true
    $null = $Bootstrap.Start()
    $OutputTask = $Bootstrap.StandardOutput.ReadToEndAsync()
    $ErrorTask = $Bootstrap.StandardError.ReadToEndAsync()
    try {
        if (-not $Bootstrap.WaitForExit(360000)) { $Bootstrap.Kill(); throw 'O comando não concluiu o download no prazo da conferência.' }
        [IO.File]::WriteAllText((Join-Path $OutputDirectory "$Phase-powershell.txt"), $OutputTask.GetAwaiter().GetResult())
        [IO.File]::WriteAllText((Join-Path $OutputDirectory "$Phase-powershell-errors.txt"), $ErrorTask.GetAwaiter().GetResult())
        $Report["${Phase}ExitCode"] = $Bootstrap.ExitCode
        if ($Bootstrap.ExitCode -ne 0) { throw "O comando do PowerShell falhou: $Phase (código $($Bootstrap.ExitCode))." }
    }
    finally { $Bootstrap.Dispose() }
    if ((Get-FileHash -LiteralPath $Executable -Algorithm SHA256).Hash.ToLowerInvariant() -ne $ExpectedHash) { throw 'O executável baixado difere da v2.0.2 publicada.' }
    $Deadline = [DateTime]::Now.AddSeconds(75)
    do {
        $Process = Get-Process -Name GOATTweaks -ErrorAction SilentlyContinue | Where-Object { $_.StartTime -ge $Before.AddSeconds(-2) } | Select-Object -First 1
        if ($null -ne $Process -and $Process.MainWindowTitle -eq $Title) {
            $Watch.Stop()
            $Report["${Phase}OpenSeconds"] = [Math]::Round($Watch.Elapsed.TotalSeconds, 2)
            Write-Host "PowerShell ${Phase}: painel aberto em $($Report["${Phase}OpenSeconds"]) s."
            return $Process
        }
        Start-Sleep -Milliseconds 500
    } while ([DateTime]::Now -lt $Deadline)
    throw "O comando terminou, mas a janela principal não abriu: $Phase."
}
function Close-App {
    if ($null -ne $App) {
        $App.Refresh()
        if (-not $App.HasExited) {
            $null = $App.CloseMainWindow()
            if (-not $App.WaitForExit(5000)) { $App.Kill() }
        }
        $App.Dispose()
    }
}

try {
    $Report.displayChangeResult = [GoatHudDesktop]::WideScreen()
    $Report.desktop = "$([GoatHudDesktop]::GetSystemMetrics(0))x$([GoatHudDesktop]::GetSystemMetrics(1))"
    $App = Open-Published 'cold'
    Start-Sleep -Seconds 25
    $Width = [Math]::Min(1490, [GoatHudDesktop]::GetSystemMetrics(0) - 24)
    $Height = [Math]::Min(960, [GoatHudDesktop]::GetSystemMetrics(1) - 54)
    Resize $Width $Height
    Capture '01-visao-geral'
    Navigate 'Meu hardware' 'Meu hardware'
    $Report.hardwareText = @(Visible-Text)
    Capture '02-hardware'
    Navigate 'Visão geral' 'Visão geral'
    $Scroll = (Find-Id 'DashboardPanel').GetCurrentPattern([System.Windows.Automation.ScrollPattern]::Pattern)
    if ($Scroll.Current.VerticallyScrollable) { $Scroll.SetScrollPercent(-1, 100); Start-Sleep -Milliseconds 400 }
    Capture '03-servicos'
    foreach ($Label in 'Limpeza','CPU e memória','Placa de vídeo','Drivers e periféricos','Rede e internet','Windows e interface','Privacidade e segurança','Energia e suspensão','Instalar programas','Preparação e reparos','Avançado e BIOS') {
        Navigate $Label $Label
        if ($Label -eq 'Limpeza') { Capture '04-limpeza' }
        if ($Label -eq 'Placa de vídeo') { Capture '05-placa-de-video' }
    }
    Navigate 'CPU e memória' 'CPU e memória'
    foreach ($Action in '7-Zip','Autoruns') {
        Search $Action
        Invoke-NamedButton '+ Adicionar à fila'
        $Report.queueAdded++
        Navigate 'Fila de atendimento' 'Revisar atendimento'
        if ([int](Find-Id 'QueueCountLabel').Current.Name -ne $Report.queueAdded) { throw 'A fila não refletiu a ação adicionada.' }
    }
    Navigate 'Fila de atendimento' 'Revisar atendimento'
    Capture '06-fila-com-itens'
    Resize 1024 700
    Capture '07-fila-compacta'
    (Find-Id 'ClearQueueButton').GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
    Start-Sleep -Milliseconds 500
    if ([int](Find-Id 'QueueCountLabel').Current.Name -ne 0) { throw 'A fila não foi limpa.' }
    $Report.queueCleared = $true
    Navigate 'Visão geral' 'Visão geral'
    $Scroll = (Find-Id 'DashboardPanel').GetCurrentPattern([System.Windows.Automation.ScrollPattern]::Pattern)
    if ($Scroll.Current.VerticallyScrollable) { $Scroll.SetScrollPercent(-1, 0) }
    Capture '08-visao-compacta'
    Close-App
    $App = $null
    $App = Open-Published 'warm'
    Start-Sleep -Seconds 3
    $WarmLog = Get-Content -LiteralPath (Join-Path $OutputDirectory 'warm-powershell.txt') -Raw
    if ($WarmLog -notmatch 'copia local' -or $WarmLog -match 'Baixando o pacote') { throw 'A segunda abertura não confirmou o uso da cópia local.' }
    $Report.warmCacheUsed = $true
    $Report.executableSha256 = (Get-FileHash -LiteralPath $Executable -Algorithm SHA256).Hash.ToLowerInvariant()
    $Report.complete = $true
    Write-Host 'Conferência concluída: comando público, 11 áreas, hardware, fila sem execução e reabertura pelo cache.'
}
catch {
    $Report.error = $_.Exception.Message
    try { if ($null -ne $App) { Capture '99-falha' } } catch { Write-Warning 'Não foi possível registrar a janela após a falha.' }
    throw
}
finally {
    $Report | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $OutputDirectory 'resultado.json') -Encoding utf8
    Close-App
    [GoatHudDesktop]::RestoreScreen()
}
