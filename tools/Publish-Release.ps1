[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidatePattern('^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$')][string]$Repository,
    [Parameter(Mandatory)][ValidatePattern('^v\d+\.\d+\.\d+$')][string]$Version,
    [string]$OutputDirectory = (Join-Path $PSScriptRoot '..\dist')
)
$ErrorActionPreference = 'Stop'
$Root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$VersionNumber = $Version.Substring(1)
$PublishDirectory = Join-Path $Root '.publish'
$Dotnet = Get-Command dotnet -ErrorAction Stop
if (Test-Path -LiteralPath $PublishDirectory) { Remove-Item -LiteralPath $PublishDirectory -Recurse -Force }

# Refuse packaging if source scripts differ from the incorporated manifest.
$ThirdParty = Join-Path $Root 'third_party'
foreach ($Line in [IO.File]::ReadAllLines((Join-Path $ThirdParty 'SHA256SUMS'))) {
    if ([string]::IsNullOrWhiteSpace($Line)) { continue }
    if ($Line.Length -lt 67) { throw 'Manifesto de recursos inválido.' }
    $Expected = $Line.Substring(0, 64)
    $Relative = $Line.Substring(66).TrimStart('*')
    $File = Join-Path $ThirdParty $Relative
    if (-not (Test-Path -LiteralPath $File -PathType Leaf) -or
        (Get-FileHash -LiteralPath $File -Algorithm SHA256).Hash -ne $Expected) {
        throw "Um recurso original não corresponde ao manifesto: $Relative"
    }
}

# Compilation and packaging only: no test suites or Windows tuning actions are run.
& $Dotnet.Source publish (Join-Path $Root 'src/GoatTweaks/GoatTweaks.csproj') `
    --configuration Release --runtime win-x64 --self-contained true `
    --disable-build-servers -m:1 -p:UseSharedCompilation=false `
    -p:PublishSingleFile=true -p:EnableWindowsTargeting=true `
    -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=embedded `
    "-p:Version=$VersionNumber" "-p:AssemblyVersion=$VersionNumber.0" `
    --output $PublishDirectory
if ($LASTEXITCODE -ne 0) { throw "A compilação falhou (código $LASTEXITCODE)." }

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$Executable = Join-Path $PublishDirectory 'GOATTweaks.exe'
if (-not (Test-Path -LiteralPath $Executable -PathType Leaf)) { throw 'A compilação não produziu GOATTweaks.exe.' }
$Unexpected = @(Get-ChildItem -LiteralPath $PublishDirectory -Recurse -File | Where-Object {
    $_.Extension -in '.dll', '.json' -and $_.Name -ne 'GoatTweaks.Runner.runtimeconfig.json'
})
if ($Unexpected.Count) { throw 'A publicação ainda contém dependências externas; o pacote de arquivo único não pode ser publicado.' }
Copy-Item -LiteralPath $Executable -Destination (Join-Path $OutputDirectory 'GOATTweaks.exe') -Force
$Hash = (Get-FileHash -LiteralPath $Executable -Algorithm SHA256).Hash.ToLowerInvariant()
[IO.File]::WriteAllText((Join-Path $OutputDirectory 'GOATTweaks.exe.sha256'), "$Hash`n", [Text.Encoding]::ASCII)
$Launcher = [IO.File]::ReadAllText((Join-Path $Root 'launcher/goat.ps1'))
$Launcher = $Launcher.Replace('__GOAT_REPO__', $Repository).Replace('__GOAT_RELEASE__', $Version)
[IO.File]::WriteAllText((Join-Path $OutputDirectory 'goat.ps1'), $Launcher, [Text.UTF8Encoding]::new($true))
Write-Host "Release preparada em $OutputDirectory"
