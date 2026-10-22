[CmdletBinding()]
param(
    [string]$Executable = (Join-Path $PSScriptRoot '..\dist\GOATTweaks.exe'),
    [string]$ReportDirectory = (Join-Path $PSScriptRoot '..\artifacts\startup'),
    [switch]$PublishedLauncher
)
$ErrorActionPreference = 'Stop'
if ($env:OS -ne 'Windows_NT') { throw 'Esta conferência requer Windows.' }
$Executable = (Resolve-Path -LiteralPath $Executable).Path
New-Item -ItemType Directory -Path $ReportDirectory -Force | Out-Null

# Parse the generated web command with Windows PowerShell 5.1, the user's shell.
$Launcher = Join-Path (Split-Path -Parent $Executable) 'goat.ps1'
$env:GOAT_REVIEW_LAUNCHER = $Launcher
& "$env:WINDIR\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -NonInteractive -Command {
    $tokens = $null
    $errors = $null
    $null = [System.Management.Automation.Language.Parser]::ParseFile($env:GOAT_REVIEW_LAUNCHER, [ref]$tokens, [ref]$errors)
    if ($errors.Count) { $errors | ForEach-Object { Write-Error $_ }; exit 1 }
}
if ($LASTEXITCODE -ne 0) { throw 'O inicializador não é compatível com a sintaxe do Windows PowerShell 5.1.' }

Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes, System.Windows.Forms, System.Drawing
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class GoatReviewWindow {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr window);
}
'@
$ExpectedTitle = 'GOAT Tweaks · Painel de atendimento'
$Process = $null
$Report = [ordered]@{
    executableSha256 = (Get-FileHash -LiteralPath $Executable -Algorithm SHA256).Hash.ToLowerInvariant()
    windows = [Environment]::OSVersion.VersionString
    runnerArchitecture = [Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString()
    launcherWindowsPowerShell51 = 'parsed'
    windowTitle = ''
    opened = $false
    appliedActions = 0
    publishedLauncher = [bool]$PublishedLauncher
}
try {
    # Start only the dashboard: no runner arguments, queue actions or Windows adjustments.
    if ($PublishedLauncher) {
        if ($env:GITHUB_REPOSITORY -notmatch '^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$' -or $env:RELEASE_TAG -notmatch '^v\d+\.\d+\.\d+$') {
            throw 'Repositorio ou versao invalidos para conferir a publicacao.'
        }
        $env:GOAT_REVIEW_PUBLIC_URI = "https://github.com/$env:GITHUB_REPOSITORY/releases/latest/download/goat.ps1"
        $env:GOAT_REVIEW_SCRIPT_HASH = (Get-FileHash $Launcher -Algorithm SHA256).Hash.ToLowerInvariant()
        $Started = [DateTime]::UtcNow
        # Verify the downloaded bytes against this build before evaluating the public one-line entry point.
        & "$env:WINDIR\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -NonInteractive -Command {
            $ErrorActionPreference='Stop'
            $ProgressPreference='SilentlyContinue'
            [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
            $Script = Invoke-RestMethod -Uri $env:GOAT_REVIEW_PUBLIC_URI
            $Hash = [Security.Cryptography.SHA256]::Create()
            try { $Actual = ([BitConverter]::ToString($Hash.ComputeHash([Text.Encoding]::UTF8.GetBytes($Script)))).Replace('-','').ToLowerInvariant() }
            finally { $Hash.Dispose() }
            if ($Actual -ne $env:GOAT_REVIEW_SCRIPT_HASH) { throw 'O inicializador publico difere desta compilacao. Nada foi executado.' }
            Invoke-Expression $Script
        }
        if ($LASTEXITCODE -ne 0) { throw 'O comando publico falhou no Windows PowerShell 5.1.' }
        $Cached = Join-Path $env:LOCALAPPDATA "GoatTweaks\bin\$env:RELEASE_TAG\GOATTweaks.exe"
        if ((Get-FileHash $Cached -Algorithm SHA256).Hash -ne (Get-FileHash $Executable -Algorithm SHA256).Hash) {
            throw 'O executavel baixado pelo comando publico difere da compilacao.'
        }
        $Deadline = [DateTime]::UtcNow.AddSeconds(30)
        do {
            $Process = Get-Process -Name GOATTweaks -ErrorAction SilentlyContinue |
                Where-Object { $_.StartTime.ToUniversalTime() -ge $Started } |
                Select-Object -First 1
            if ($null -eq $Process) { Start-Sleep -Milliseconds 500 }
        } while ($null -eq $Process -and [DateTime]::UtcNow -lt $Deadline)
        if ($null -eq $Process) { throw 'O comando publico nao iniciou o aplicativo esperado.' }
        $Report.launcherSha256=$env:GOAT_REVIEW_SCRIPT_HASH
        $Report.downloadedExeSha256=(Get-FileHash $Cached -Algorithm SHA256).Hash.ToLowerInvariant()
    }
    else { $Process = Start-Process -FilePath $Executable -PassThru }
    $Deadline = [DateTime]::UtcNow.AddSeconds(75)
    do {
        Start-Sleep -Milliseconds 500
        $Process.Refresh()
        if ($Process.HasExited) { throw "O aplicativo encerrou na abertura (código $($Process.ExitCode))." }
        $Report.windowTitle = $Process.MainWindowTitle
        if ($Process.MainWindowHandle -ne [IntPtr]::Zero -and $Process.MainWindowTitle -eq $ExpectedTitle) { break }
        if ($Process.MainWindowHandle -ne [IntPtr]::Zero -and $Process.MainWindowTitle -eq 'GOAT Tweaks') {
            $Dialog = [System.Windows.Automation.AutomationElement]::FromHandle($Process.MainWindowHandle)
            $Text = $Dialog.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition) |
                ForEach-Object { $_.Current.Name } | Where-Object { $_ } | Select-Object -Unique
            throw "O aplicativo mostrou um erro de inicialização: $($Text -join ' | ')"
        }
    } while ([DateTime]::UtcNow -lt $Deadline)
    if ($Process.MainWindowTitle -ne $ExpectedTitle) { throw "O painel não abriu no prazo. Janela: $($Process.MainWindowTitle)" }

    # Allow local hardware collection and the first render to finish before recording evidence.
    Start-Sleep -Seconds 25
    $Process.Refresh()
    if ($Process.HasExited -or $Process.MainWindowTitle -ne $ExpectedTitle -or -not $Process.Responding) {
        throw 'O painel não permaneceu aberto e respondendo após a inicialização.'
    }
    $Window = [System.Windows.Automation.AutomationElement]::FromHandle($Process.MainWindowHandle)
    $Names = @($Window.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition) |
        ForEach-Object { $_.Current.Name } | Where-Object { $_ } | Select-Object -Unique)
    [IO.File]::WriteAllLines((Join-Path $ReportDirectory 'window-text.txt'), [string[]]$Names)
    [GoatReviewWindow]::SetForegroundWindow($Process.MainWindowHandle) | Out-Null
    Start-Sleep -Milliseconds 700
    $Bounds = [System.Windows.Forms.SystemInformation]::VirtualScreen
    $Bitmap = [System.Drawing.Bitmap]::new($Bounds.Width, $Bounds.Height)
    $Graphics = [System.Drawing.Graphics]::FromImage($Bitmap)
    try {
        $Graphics.CopyFromScreen($Bounds.Location, [System.Drawing.Point]::Empty, $Bounds.Size)
        $Bitmap.Save((Join-Path $ReportDirectory 'dashboard.png'), [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally { $Graphics.Dispose(); $Bitmap.Dispose() }
    $Report.opened = $true
    Write-Host "Painel aberto e respondendo: $ExpectedTitle. Nenhum ajuste aplicado."
}
finally {
    $Report | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $ReportDirectory 'startup.json') -Encoding utf8
    if ($null -ne $Process) {
        $Process.Refresh()
        if (-not $Process.HasExited) {
            $null = $Process.CloseMainWindow()
            if (-not $Process.WaitForExit(5000)) { $Process.Kill($true) }
        }
        $Process.Dispose()
    }
    Remove-Item Env:\GOAT_REVIEW_LAUNCHER -ErrorAction SilentlyContinue
    Remove-Item Env:\GOAT_REVIEW_PUBLIC_URI,Env:\GOAT_REVIEW_SCRIPT_HASH -ErrorAction SilentlyContinue
}
