& {
    $ErrorActionPreference = 'Stop'
    $Repo = '__GOAT_REPO__'
    $ReleaseTag = '__GOAT_RELEASE__'

    if ($PSVersionTable.PSVersion -lt [Version]'5.1') { throw 'Requer PowerShell 5.1 ou mais recente.' }
    if ($env:OS -ne 'Windows_NT' -or -not [Environment]::Is64BitOperatingSystem) {
        throw 'Requer Windows de 64 bits.'
    }
    $WindowsBuild = [int](Get-ItemPropertyValue -LiteralPath 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion' -Name CurrentBuildNumber)
    if ($WindowsBuild -lt 19045) { throw 'Requer Windows 10 22H2 ou Windows 11.' }
    if ($Repo -notmatch '^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$' -or $ReleaseTag -notmatch '^v\d+\.\d+\.\d+$') {
        throw 'Esta copia do inicializador ainda nao foi configurada para uma release.'
    }

    $PreviousProtocol = [Net.ServicePointManager]::SecurityProtocol
    [Net.ServicePointManager]::SecurityProtocol = $PreviousProtocol -bor [Net.SecurityProtocolType]::Tls12
    $InstallDirectory = Join-Path $env:LOCALAPPDATA "GoatTweaks\bin\$ReleaseTag"
    $Executable = Join-Path $InstallDirectory 'GOATTweaks.exe'
    $DownloadId = [Guid]::NewGuid().ToString('N')
    $ChecksumDownload = Join-Path $InstallDirectory "checksum-$DownloadId.download"
    $ExecutableDownload = Join-Path $InstallDirectory "app-$DownloadId.download"
    $BaseUrl = "https://github.com/$Repo/releases/download/$ReleaseTag"

    try {
        New-Item -ItemType Directory -Path $InstallDirectory -Force | Out-Null
        Write-Host "Obtendo GOAT Tweaks $ReleaseTag..."
        Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/GOATTweaks.exe.sha256" -OutFile $ChecksumDownload
        $ExpectedHash = (Get-Content -LiteralPath $ChecksumDownload -Raw).Trim()
        if ($ExpectedHash -notmatch '^[0-9a-fA-F]{64}$') { throw 'O arquivo SHA-256 publicado esta invalido.' }
        $DownloadRequired = $true
        if (Test-Path -LiteralPath $Executable -PathType Leaf) {
            $CurrentHash = (Get-FileHash -LiteralPath $Executable -Algorithm SHA256).Hash
            $DownloadRequired = -not $CurrentHash.Equals($ExpectedHash, [StringComparison]::OrdinalIgnoreCase)
        }
        if ($DownloadRequired) {
            Write-Host 'Baixando o aplicativo...'
            Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/GOATTweaks.exe" -OutFile $ExecutableDownload
            $DownloadedHash = (Get-FileHash -LiteralPath $ExecutableDownload -Algorithm SHA256).Hash
            if (-not $DownloadedHash.Equals($ExpectedHash, [StringComparison]::OrdinalIgnoreCase)) {
                throw 'O download nao corresponde ao SHA-256 publicado. Nada foi aberto.'
            }
            Move-Item -LiteralPath $ExecutableDownload -Destination $Executable -Force
        }
        Write-Host 'Abrindo GOAT Tweaks...'
        Start-Process -FilePath $Executable -ErrorAction Stop | Out-Null
    }
    finally {
        Remove-Item -LiteralPath $ChecksumDownload -Force -ErrorAction SilentlyContinue
        Remove-Item -LiteralPath $ExecutableDownload -Force -ErrorAction SilentlyContinue
        [Net.ServicePointManager]::SecurityProtocol = $PreviousProtocol
    }
}
