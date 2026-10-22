[CmdletBinding()]
param([string]$OutputDirectory = (Join-Path $PSScriptRoot '..\artifacts\vitor-hud'))
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$OutputDirectory = (Resolve-Path $OutputDirectory).Path
$Manifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'VitorHud-SourceManifest.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$OriginalUrl = 'https://github.com/VitorSaviolli/goat-tweaks/releases/latest/download/goat-tweaks.ps1'
$Report = [ordered]@{
    repository = $Manifest.repository; sourceCommit = $Manifest.commit;
    originalCommand = ('irm "' + $OriginalUrl + '" | iex');
    executionRoute = 'Compiled from pinned public source; release URL unavailable at review';
    os = [Environment]::OSVersion.VersionString; powershell = $PSVersionTable.PSVersion.ToString();
    sourceFilesVerified = 0; captures = @(); appliedOptimizations = 0; complete = $false
}
$App = $null
$script:AppWindow = $null
$script:WindowHandle = [IntPtr]::Zero
$OriginalTls = [Net.ServicePointManager]::SecurityProtocol
[Net.ServicePointManager]::SecurityProtocol = $OriginalTls -bor [Net.SecurityProtocolType]::Tls12
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
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
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
    [DllImport("user32.dll")] private static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extraInfo);
    public static void Click(int x, int y) {
        SetCursorPos(x, y); System.Threading.Thread.Sleep(80);
        mouse_event(2, 0, 0, 0, UIntPtr.Zero); System.Threading.Thread.Sleep(80);
        mouse_event(4, 0, 0, 0, UIntPtr.Zero);
    }
}
'@

function Get-AppWindow {
    if ($null -eq $App) { return $null }
    $App.Refresh()
    if ($App.HasExited) { throw "O PowerShell do aplicativo encerrou (código $($App.ExitCode))." }
    $Condition = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ProcessIdProperty, $App.Id)
    $Windows = [System.Windows.Automation.AutomationElement]::RootElement.FindAll([System.Windows.Automation.TreeScope]::Children, $Condition)
    $Report.windowCandidates = @($Windows | ForEach-Object {
        [ordered]@{ name = $_.Current.Name; className = $_.Current.ClassName; framework = $_.Current.FrameworkId; handle = $_.Current.NativeWindowHandle }
    })
    foreach ($Window in $Windows) {
        # UI Automation reports a WPF Window's class as "Window"; the native
        # HwndWrapper name is not guaranteed to be the automation class name.
        if ($Window.Current.Name -like 'GOAT Tweaks*' -and ($Window.Current.FrameworkId -eq 'WPF' -or $Window.Current.ClassName -like 'HwndWrapper*')) {
            $script:AppWindow = $Window
            $script:WindowHandle = [IntPtr]::new($Window.Current.NativeWindowHandle)
            return $Window
        }
    }
    return $null
}
function Find-Control([string]$Id) {
    $Root = Get-AppWindow
    if ($null -eq $Root) { throw 'A janela WPF não está disponível.' }
    $Condition = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::AutomationIdProperty, $Id)
    $Element = $Root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $Condition)
    if ($null -eq $Element) { throw "Controle de navegação não encontrado: $Id" }
    return $Element
}
function Resize-App([int]$Width, [int]$Height) {
    $null = Get-AppWindow
    if (-not [GoatHudDesktop]::SetWindowPos($script:WindowHandle, [IntPtr]::new(-1), 16, 16, $Width, $Height, 0x40)) { throw 'Não foi possível dimensionar a janela da HUD.' }
    $null = [GoatHudDesktop]::SetForegroundWindow($script:WindowHandle)
    Start-Sleep -Milliseconds 700
}
function Capture-Hud([string]$Name, [string]$Area) {
    $Root = Get-AppWindow
    if ($null -eq $Root) { throw 'A janela desapareceu antes da captura.' }
    $null = [GoatHudDesktop]::SetForegroundWindow($script:WindowHandle)
    $null = [GoatHudDesktop]::SetCursorPos(1890, 1050)
    Start-Sleep -Milliseconds 700
    $Rect = [GoatHudDesktop+Rect]::new()
    if (-not [GoatHudDesktop]::GetWindowRect($script:WindowHandle, [ref]$Rect)) { throw 'Não foi possível obter o tamanho da janela.' }
    $Width = $Rect.Right - $Rect.Left; $Height = $Rect.Bottom - $Rect.Top
    $Bitmap = [System.Drawing.Bitmap]::new($Width, $Height)
    $Graphics = [System.Drawing.Graphics]::FromImage($Bitmap)
    try {
        $Graphics.CopyFromScreen($Rect.Left, $Rect.Top, 0, 0, [System.Drawing.Size]::new($Width, $Height))
        $Bitmap.Save((Join-Path $OutputDirectory "$Name.png"), [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally { $Graphics.Dispose(); $Bitmap.Dispose() }
    $Text = @($Root.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition) |
        Where-Object { -not $_.Current.IsOffscreen } | ForEach-Object { $_.Current.Name } | Where-Object { $_ } | Select-Object -Unique)
    [IO.File]::WriteAllLines((Join-Path $OutputDirectory "$Name.txt"), [string[]]$Text)
    $Report.captures += [ordered]@{ file = "$Name.png"; area = $Area; width = $Width; height = $Height }
    Write-Host "HUD capturada: $Area ($Width x $Height)."
}
function Open-Tab([string]$Id, [string]$Name) {
    # Only the reviewed navigation controls can receive a click. Never invoke action buttons.
    if ($Id -notin 'WPFTab1BT','WPFTab2BT','WPFTab3BT','WPFTab4BT','WPFTab5BT','WPFTab7BT','WPFAppxRemoval') {
        throw "Controle fora do escopo de navegação: $Id"
    }
    $Control = Find-Control $Id
    if (-not $Control.Current.IsEnabled -or $Control.Current.IsOffscreen) { throw "Navegação indisponível: $Id" }
    $Point = $Control.GetClickablePoint()
    $null = [GoatHudDesktop]::SetForegroundWindow($script:WindowHandle)
    [GoatHudDesktop]::Click([int]$Point.X, [int]$Point.Y)
    Start-Sleep -Seconds 2
    try {
        $Tabs = Find-Control 'WPFTabNav'
        $Selection = $Tabs.GetCurrentPattern([System.Windows.Automation.SelectionPattern]::Pattern)
        $Selected = $Selection.Current.GetSelection()
        if ($Selected.Count -gt 0) {
            $Actual = $Selected[0].Current.Name
            if ($Actual -and $Actual -ne $Name) { throw "A navegação selecionou $Actual em vez de $Name." }
        }
    }
    catch [System.InvalidOperationException] { Write-Host 'A navegação será registrada pela captura da área visível.' }
}
function Scroll-Page([double]$Percent) {
    $Candidates = @()
    foreach ($Element in (Get-AppWindow).FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)) {
        if ($Element.Current.IsOffscreen) { continue }
        $Pattern = $null
        if ($Element.TryGetCurrentPattern([System.Windows.Automation.ScrollPattern]::Pattern, [ref]$Pattern) -and $Pattern.Current.VerticallyScrollable) {
            $Rect = $Element.Current.BoundingRectangle
            $Candidates += [pscustomobject]@{ Pattern = $Pattern; Area = $Rect.Width * $Rect.Height }
        }
    }
    $Largest = $Candidates | Sort-Object Area -Descending | Select-Object -First 1
    if ($null -ne $Largest) { $Largest.Pattern.SetScrollPercent(-1, $Percent); Start-Sleep -Milliseconds 700 }
}

try {
    try {
        $Response = Invoke-WebRequest -Uri $OriginalUrl -UseBasicParsing -TimeoutSec 25
        $Report.originalUrlHttpStatus = [int]$Response.StatusCode
    }
    catch {
        if ($_.Exception.Response) { $Report.originalUrlHttpStatus = [int]$_.Exception.Response.StatusCode }
        else { $Report.originalUrlError = $_.Exception.Message }
    }
    $Work = Join-Path $env:RUNNER_TEMP ('vitor-hud-' + [Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $Work -Force | Out-Null
    $Archive = Join-Path $Work 'source.zip'
    $SourceUrl = "https://api.github.com/repos/$($Manifest.repository)/zipball/$($Manifest.commit)"
    Invoke-WebRequest -Uri $SourceUrl -Headers @{ 'User-Agent' = 'GOAT-HUD-Review' } -UseBasicParsing -OutFile $Archive -TimeoutSec 60
    Expand-Archive -LiteralPath $Archive -DestinationPath (Join-Path $Work 'source')
    $SourceRoot = (Get-ChildItem -LiteralPath (Join-Path $Work 'source') -Directory | Select-Object -First 1).FullName
    foreach ($File in $Manifest.files) {
        $Local = Join-Path $SourceRoot $File.path
        if ((Get-FileHash -LiteralPath $Local -Algorithm SHA256).Hash.ToLowerInvariant() -ne $File.sha256) { throw "Fonte divergente: $($File.path)" }
        $Report.sourceFilesVerified++
    }
    Push-Location $SourceRoot
    try { & .\Compile.ps1 -Repository $Manifest.repository }
    finally { Pop-Location }
    $Compiled = Join-Path $SourceRoot 'goat-tweaks.ps1'
    $Tokens = $null; $Errors = $null
    $null = [System.Management.Automation.Language.Parser]::ParseFile($Compiled, [ref]$Tokens, [ref]$Errors)
    if ($Errors.Count) { throw "O script compilado possui erros de sintaxe: $($Errors[0].Message)" }
    $Report.generatedScriptSha256 = (Get-FileHash -LiteralPath $Compiled -Algorithm SHA256).Hash.ToLowerInvariant()
    $Report.displayChangeResult = [GoatHudDesktop]::WideScreen()
    $App = [Diagnostics.Process]::new()
    $PowerShell = Join-Path $env:WINDIR 'System32\WindowsPowerShell\v1.0\powershell.exe'
    $App.StartInfo = [Diagnostics.ProcessStartInfo]::new($PowerShell, ('-NoLogo -NoProfile -STA -ExecutionPolicy Bypass -File "' + $Compiled + '"'))
    $App.StartInfo.WorkingDirectory = $SourceRoot
    $App.StartInfo.UseShellExecute = $false
    $App.StartInfo.CreateNoWindow = $false
    $App.StartInfo.RedirectStandardOutput = $true
    $App.StartInfo.RedirectStandardError = $true
    $null = $App.Start()
    $OutTask = $App.StandardOutput.ReadToEndAsync()
    $ErrTask = $App.StandardError.ReadToEndAsync()
    $Deadline = [DateTime]::Now.AddSeconds(150)
    while ($null -eq (Get-AppWindow) -and [DateTime]::Now -lt $Deadline) { Start-Sleep -Milliseconds 500 }
    if ($script:WindowHandle -eq [IntPtr]::Zero) { throw 'O aplicativo não abriu uma janela WPF no prazo.' }
    Start-Sleep -Seconds 20
    Resize-App 1600 960
    Capture-Hud '01-install' 'Install'
    Open-Tab 'WPFTab2BT' 'Tweaks'
    Capture-Hud '02-tweaks' 'Tweaks'
    Scroll-Page 100
    Capture-Hud '03-tweaks-final' 'Tweaks - parte inferior'
    Open-Tab 'WPFTab7BT' 'Ultimate'
    Capture-Hud '04-ultimate' 'Ultimate'
    Scroll-Page 100
    Capture-Hud '05-ultimate-final' 'Ultimate - parte inferior'
    Open-Tab 'WPFTab3BT' 'Config'
    Capture-Hud '06-config' 'Config'
    Open-Tab 'WPFTab4BT' 'Updates'
    Capture-Hud '07-updates' 'Updates'
    Open-Tab 'WPFTab5BT' 'Win11ISO'
    Capture-Hud '08-win11-creator' 'Win11 Creator'
    Open-Tab 'WPFTab2BT' 'Tweaks'
    # Return to the top so the AppX navigation button is visible.
    Scroll-Page 0
    $Appx = Find-Control 'WPFAppxRemoval'
    if ($Appx.Current.IsOffscreen) {
        $Pattern = $null
        if ($Appx.TryGetCurrentPattern([System.Windows.Automation.ScrollItemPattern]::Pattern, [ref]$Pattern)) { $Pattern.ScrollIntoView() }
    }
    Open-Tab 'WPFAppxRemoval' 'AppX'
    Capture-Hud '09-appx' 'AppX'
    $Report.complete = $true
}
catch {
    $Report.error = $_.Exception.Message
    try { if ($script:WindowHandle -ne [IntPtr]::Zero) { Capture-Hud '99-falha' 'Estado no momento da falha' } } catch { Write-Warning 'Não foi possível capturar o estado após a falha.' }
    throw
}
finally {
    if ($null -ne $App) {
        if (-not $App.HasExited) {
            try { $script:AppWindow.GetCurrentPattern([System.Windows.Automation.WindowPattern]::Pattern).Close() } catch { $null = $App.CloseMainWindow() }
            if (-not $App.WaitForExit(10000)) { $App.Kill() }
        }
        if ($OutTask) { [IO.File]::WriteAllText((Join-Path $OutputDirectory 'powershell.txt'), $OutTask.GetAwaiter().GetResult()) }
        if ($ErrTask) { [IO.File]::WriteAllText((Join-Path $OutputDirectory 'powershell-errors.txt'), $ErrTask.GetAwaiter().GetResult()) }
        $App.Dispose()
    }
    $Report | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $OutputDirectory 'resultado.json') -Encoding UTF8
    [GoatHudDesktop]::RestoreScreen()
    [Net.ServicePointManager]::SecurityProtocol = $OriginalTls
}
