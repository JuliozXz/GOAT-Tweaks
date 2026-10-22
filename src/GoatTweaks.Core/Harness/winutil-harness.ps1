param(
    [string]$RequestPath,
    [switch]$ListTweaks
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$sync = [Hashtable]::Synchronized(@{
    configs = @{}
    logPath = [IO.Path]::Combine([IO.Path]::GetTempPath(), "GoatTweaks-winutil.log")
    transcriptPath = [IO.Path]::Combine([IO.Path]::GetTempPath(), "GoatTweaks-winutil-transcript.log")
    winutildir = $root
    LoggedErrors = @{}
    IsLocalCompile = $true
    ToggleStatusCache = @{}
})

Get-ChildItem -LiteralPath (Join-Path $root "config") -Filter "*.json" -File | ForEach-Object {
    $sync.configs[$_.BaseName] = Get-Content -LiteralPath $_.FullName -Raw | ConvertFrom-Json
}

Get-ChildItem -LiteralPath (Join-Path $root "functions") -Filter "*.ps1" -File -Recurse |
    Sort-Object FullName |
    ForEach-Object { . $_.FullName }

if ($ListTweaks) {
    $keys = @($sync.configs.tweaks.PSObject.Properties.Name | Sort-Object)
    Write-Output "Loaded $($keys.Count) tweak keys"
    $keys
    exit 0
}

if ([string]::IsNullOrWhiteSpace($RequestPath)) {
    throw "RequestPath is required unless ListTweaks is specified."
}

$request = Get-Content -LiteralPath $RequestPath -Raw | ConvertFrom-Json
$key = ($request.itemId -split ":", 3)[-1]
$description = switch ($request.kind) {
    "function" { "$($request.function)()" }
    "tweak" { "Invoke-WinUtilTweaks -CheckBox $key" }
    "feature" { "Invoke-WinUtilFeatureInstall $key" }
    "fix" { "Invoke-WPFButtonAction -Button $key" }
    "appx" { "Remove-WinUtilAPPX -Name $($request.packageId)" }
    "dns" { "Set-WinUtilDNS -DNSProvider $key" }
    "app" { "Install app $($request.winget)$($request.choco)" }
    default { "Unsupported kind $($request.kind)" }
}
Write-Output "Executando winutil: $description"

switch ($request.kind) {
    "tweak" {
        if ($request.revert) {
            Invoke-WinUtilTweaks -CheckBox $key -undo
        } else {
            Invoke-WinUtilTweaks -CheckBox $key
        }
    }
    "feature" {
        Invoke-WinUtilFeatureInstall $key
    }
    "fix" {
        Invoke-WPFButtonAction -Button $key
    }
    "function" {
        & $request.function
    }
    "appx" {
        Remove-WinUtilAPPX -Name $request.packageId
    }
    "dns" {
        Set-WinUtilDNS -DNSProvider $key
    }
    "app" {
        if (-not [string]::IsNullOrWhiteSpace($request.winget)) {
            Install-WinUtilProgramWinget -Action Install -Programs $request.winget
        } else {
            Install-WinUtilProgramChoco -Action Install -Programs $request.choco
        }
    }
    default {
        throw "Unsupported winutil action kind: $($request.kind)"
    }
}
