[CmdletBinding()]
param([string]$Executable=(Join-Path $PSScriptRoot '..\dist\GOATTweaks.exe'))
$ErrorActionPreference='Stop'
$Output=Join-Path $PSScriptRoot '..\artifacts\intelligence\hud'
New-Item -ItemType Directory -Path $Output -Force | Out-Null
Add-Type -AssemblyName UIAutomationClient,UIAutomationTypes,System.Drawing,System.Windows.Forms
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
}
'@
$App=$null
$Started=[DateTimeOffset]::UtcNow
$Report=[ordered]@{ opened=$false;diagnosis=$false;validation=$false;captures=@();appliedOptimizations=0;complete=$false }
function Root {
    $App.Refresh()
    if($App.HasExited){throw 'O painel encerrou antes de completar o fluxo.'}
    return [System.Windows.Automation.AutomationElement]::FromHandle($App.MainWindowHandle)
}
function Find([string]$Id) {
    $Element=(Root).FindFirst([System.Windows.Automation.TreeScope]::Descendants,
        [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::AutomationIdProperty,$Id))
    if($null -eq $Element){throw "Controle não encontrado: $Id"}
    return $Element
}
function Capture([string]$Name) {
    $null=[GoatHudDesktop]::SetForegroundWindow($App.MainWindowHandle)
    $null=[GoatHudDesktop]::SetCursorPos(1890,1050)
    Start-Sleep -Milliseconds 600
    $rect=[GoatHudDesktop+Rect]::new()
    $null=[GoatHudDesktop]::GetWindowRect($App.MainWindowHandle,[ref]$rect)
    $bitmap=[Drawing.Bitmap]::new($rect.Right-$rect.Left,$rect.Bottom-$rect.Top)
    $graphics=[Drawing.Graphics]::FromImage($bitmap)
    try{$graphics.CopyFromScreen($rect.Left,$rect.Top,0,0,$bitmap.Size);$bitmap.Save((Join-Path $Output "$Name.png"),[Drawing.Imaging.ImageFormat]::Png)}
    finally{$graphics.Dispose();$bitmap.Dispose()}
    $text=@((Root).FindAll([System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.Condition]::TrueCondition)|
        Where-Object{-not $_.Current.IsOffscreen}|ForEach-Object{$_.Current.Name}|Where-Object{$_}|Select-Object -Unique)
    [IO.File]::WriteAllLines((Join-Path $Output "$Name.txt"),[string[]]$text)
    $Report.captures+="$Name.png"
    Write-Host "Tela capturada: $Name"
}
function Navigate([string]$Label) {
    $items=(Find 'SectionsList').FindAll([System.Windows.Automation.TreeScope]::Children,
        [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty,[System.Windows.Automation.ControlType]::ListItem))
    foreach($item in $items){
        $labelElement=$item.FindFirst([System.Windows.Automation.TreeScope]::Descendants,
            [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::NameProperty,$Label))
        if($null -ne $labelElement){$item.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select();Start-Sleep -Milliseconds 600;return}
    }
    throw "Navegação ausente: $Label"
}
function Tab([string]$Label){
    $items=(Find 'WorkspaceTabs').FindAll([System.Windows.Automation.TreeScope]::Children,
        [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty,[System.Windows.Automation.ControlType]::TabItem))
    foreach($item in $items){if($item.Current.Name -eq $Label){$item.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select();Start-Sleep -Milliseconds 500;return}}
    throw "Aba não encontrada: $Label"
}
function Invoke([string]$Id){(Find $Id).GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()}
function LastSession {
    $folder=Join-Path $env:LOCALAPPDATA 'GoatTweaks\Intelligence\sessions'
    if(-not(Test-Path $folder)){return $null}
    foreach($file in Get-ChildItem $folder -Filter '*.json'|Sort-Object LastWriteTimeUtc -Descending){
        $session=Get-Content $file.FullName -Raw -Encoding UTF8|ConvertFrom-Json
        if([DateTimeOffset]$session.CreatedAt -ge $Started){return $session}
    }
    return $null
}
function AwaitSession([bool]$After){
    $deadline=[DateTime]::UtcNow.AddSeconds(180)
    do{
        Start-Sleep -Seconds 2
        $session=LastSession
        if($null -ne $session -and (($After -and $null -ne $session.After) -or (-not $After -and $null -ne $session.Before))){return $session}
        $null=Root
    }while([DateTime]::UtcNow -lt $deadline)
    throw 'A coleta da interface não gravou um atendimento completo no prazo.'
}
try{
    $null=[GoatHudDesktop]::WideScreen()
    $App=[Diagnostics.Process]::new()
    $App.StartInfo=[Diagnostics.ProcessStartInfo]::new((Resolve-Path $Executable).Path)
    $App.StartInfo.UseShellExecute=$false
    $null=$App.Start()
    $deadline=[DateTime]::UtcNow.AddSeconds(80)
    do{Start-Sleep -Milliseconds 500;$App.Refresh();if($App.HasExited){throw 'Falha na abertura do aplicativo.'}}while(($App.MainWindowHandle -eq [IntPtr]::Zero -or $App.MainWindowTitle -ne 'GOAT Tweaks · Painel de atendimento') -and [DateTime]::UtcNow -lt $deadline)
    if($App.MainWindowTitle -ne 'GOAT Tweaks · Painel de atendimento'){throw "Título inesperado: $($App.MainWindowTitle)"}
    $null=[GoatHudDesktop]::SetWindowPos($App.MainWindowHandle,[IntPtr]::new(-1),12,12,1600,960,0x40)
    Start-Sleep -Seconds 20
    $Report.opened=$true
    Capture '01-visao-geral-preservada'
    Navigate 'GOAT Intelligence'
    Capture '02-intelligence-identificacao'
    # InvokePattern navigates the real WPF command even when the button is below the scroll viewport.
    Invoke 'DiagnoseButton'
    $session=AwaitSession $false
    $Report.diagnosis=$true
    $Report.samples=$session.Before.Samples.Count
    if($Report.samples -lt 5){throw 'A coleta não produziu amostras reais suficientes.'}
    Tab 'Diagnóstico';Capture '03-diagnostico-real'
    Tab 'Plano e execução';Capture '04-plano-personalizado'
    if((Find 'ExecuteButton').Current.IsEnabled){throw 'O perfil Diagnóstico habilitou execução de ajustes.'}
    Tab 'Monitor';Capture '05-monitor-coleta-real'
    Tab 'Testes e relatório';Capture '06-validacao-sem-resultados-inventados'
    Invoke 'ValidateButton'
    $session=AwaitSession $true
    $Report.validation=$true
    if($session.Changes.Count -ne 0){throw 'Foram aplicadas alterações durante a conferência em modo Diagnóstico.'}
    Tab 'Testes e relatório';Capture '07-validacao-posterior'
    Tab 'Atendimentos';Capture '08-historico-persistente'
    Tab 'Conhecimento';Capture '09-base-de-conhecimento'
    Navigate 'GOAT Service Mode';Capture '10-service-mode'
    Navigate 'Limpeza';Capture '11-catalogo-original-preservado'
    $Report.sessionId=$session.Id
    $session|ConvertTo-Json -Depth 30|Set-Content (Join-Path $Output 'atendimento-da-interface.json') -Encoding UTF8
    $Report.complete=$true
}
catch{
    $Report.error=$_.Exception.Message
    try{if($null -ne $App -and -not $App.HasExited){Capture '99-falha'}}catch{}
    throw
}
finally{
    $Report|ConvertTo-Json -Depth 8|Set-Content (Join-Path $Output 'hud-verification.json') -Encoding UTF8
    if($null -ne $App){if(-not $App.HasExited){$null=$App.CloseMainWindow();if(-not $App.WaitForExit(10000)){$App.Kill()}};$App.Dispose()}
    [GoatHudDesktop]::RestoreScreen()
}

