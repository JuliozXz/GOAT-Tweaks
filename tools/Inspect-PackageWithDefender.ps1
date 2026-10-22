[CmdletBinding()]
param([string]$ReportDirectory = (Join-Path $PSScriptRoot '..\artifacts\defender'))
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
$Root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
New-Item -ItemType Directory -Path $ReportDirectory -Force | Out-Null
$Status = Get-MpComputerStatus
$Status | Select-Object AMServiceEnabled, AntivirusEnabled, RealTimeProtectionEnabled,
    AMProductVersion, AMEngineVersion, AntivirusSignatureVersion, AntivirusSignatureLastUpdated |
    ConvertTo-Json | Set-Content -LiteralPath (Join-Path $ReportDirectory 'engine.json') -Encoding utf8
if (-not $Status.AMServiceEnabled -or -not $Status.AntivirusEnabled) { throw 'O Microsoft Defender não está disponível para analisar este pacote.' }
$Platform = Join-Path $env:ProgramData 'Microsoft\Windows Defender\Platform'
$Scanner = Get-ChildItem -Path (Join-Path $Platform '*\MpCmdRun.exe') -File -ErrorAction SilentlyContinue |
    Sort-Object FullName -Descending | Select-Object -First 1
if ($null -eq $Scanner) { $Scanner = Get-Item -LiteralPath (Join-Path $env:ProgramFiles 'Windows Defender\MpCmdRun.exe') }
& $Scanner.FullName -SignatureUpdate 2>&1 | Tee-Object -FilePath (Join-Path $ReportDirectory 'signature-update.txt')
if ($LASTEXITCODE -ne 0) { Write-Warning 'A atualização das assinaturas não foi concluída; a versão usada consta em engine-final.json.' }
Get-MpComputerStatus | Select-Object AMProductVersion, AMEngineVersion, AntivirusSignatureVersion, AntivirusSignatureLastUpdated |
    ConvertTo-Json | Set-Content -LiteralPath (Join-Path $ReportDirectory 'engine-final.json') -Encoding utf8
$Failed = @()
foreach ($Relative in 'third_party', 'dist') {
    # Scan-only custom mode: examines archives, ignores exclusions, and reports detections.
    # No exclusions, protection changes, restoration or automatic remediation are requested.
    & $Scanner.FullName -Scan -ScanType 3 -File (Join-Path $Root $Relative) -DisableRemediation 2>&1 |
        Tee-Object -FilePath (Join-Path $ReportDirectory "$Relative.txt")
    $Code = $LASTEXITCODE
    if ($Code -ne 0) { $Failed += "$Relative (código $Code)" }
}
if ($Failed.Count) { throw "A análise não foi aprovada: $($Failed -join ', '). Consulte os registros de detecção/erro." }
Write-Host 'Microsoft Defender: análise concluída sem detecções nos recursos e no pacote desta compilação.'
