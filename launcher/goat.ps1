& {
    $ErrorActionPreference = 'Stop'
    $ProgressPreference = 'SilentlyContinue'
    $Repo = '__GOAT_REPO__'
    $ReleaseTag = '__GOAT_RELEASE__'
    $ExpectedExeHash = '__GOAT_EXE_SHA256__'
    $ExpectedPackageHash = '__GOAT_PACKAGE_SHA256__'

    if ($PSVersionTable.PSVersion -lt [Version]'5.1') { throw 'Requer PowerShell 5.1 ou mais recente.' }
    if ($env:OS -ne 'Windows_NT' -or -not [Environment]::Is64BitOperatingSystem) { throw 'Requer Windows de 64 bits.' }
    $WindowsBuild = [int](Get-ItemPropertyValue -LiteralPath 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion' -Name CurrentBuildNumber)
    if ($WindowsBuild -lt 19045) { throw 'Requer Windows 10 22H2 ou Windows 11.' }
    if ($Repo -notmatch '^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$' -or $ReleaseTag -notmatch '^v\d+\.\d+\.\d+$' -or
        $ExpectedExeHash -notmatch '^[0-9a-fA-F]{64}$' -or $ExpectedPackageHash -notmatch '^[0-9a-fA-F]{64}$') {
        throw 'O inicializador nao foi configurado para uma release valida.'
    }

    $InstallDirectory = Join-Path $env:LOCALAPPDATA "GoatTweaks\bin\$ReleaseTag"
    $Executable = Join-Path $InstallDirectory 'GOATTweaks.exe'
    if (Test-Path -LiteralPath $Executable -PathType Leaf) {
        Write-Host 'Verificando a copia local...'
        if ((Get-FileHash -LiteralPath $Executable -Algorithm SHA256).Hash.Equals($ExpectedExeHash, [StringComparison]::OrdinalIgnoreCase)) {
            Write-Host "Abrindo GOAT Tweaks $ReleaseTag (copia local)..."
            Start-Process -FilePath $Executable -ErrorAction Stop | Out-Null
            return
        }
    }

    $PreviousProtocol = [Net.ServicePointManager]::SecurityProtocol
    [Net.ServicePointManager]::SecurityProtocol = $PreviousProtocol -bor [Net.SecurityProtocolType]::Tls12
    $DownloadId = [Guid]::NewGuid().ToString('N')
    $PackageDownload = Join-Path $InstallDirectory "package-$DownloadId.zip"
    $ExecutableDownload = Join-Path $InstallDirectory "app-$DownloadId.download"
    $BaseUrl = "https://github.com/$Repo/releases/download/$ReleaseTag"
    try {
        New-Item -ItemType Directory -Path $InstallDirectory -Force | Out-Null
        Write-Host "Baixando o pacote compacto do GOAT Tweaks $ReleaseTag..."
        Write-Host 'Este download so ocorre na primeira abertura desta versao ou para recuperar uma copia invalida.'
        Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/GOATTweaks-win-x64.zip" -OutFile $PackageDownload
        if (-not (Get-FileHash -LiteralPath $PackageDownload -Algorithm SHA256).Hash.Equals($ExpectedPackageHash, [StringComparison]::OrdinalIgnoreCase)) {
            throw 'O pacote baixado nao corresponde ao SHA-256 publicado. Nada foi aberto.'
        }
        Write-Host 'Preparando o aplicativo...'
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $Archive = [IO.Compression.ZipFile]::OpenRead($PackageDownload)
        try {
            $Entry = $Archive.GetEntry('GOATTweaks.exe')
            if ($null -eq $Entry -or $Archive.Entries.Count -ne 1) { throw 'O pacote publicado possui um formato inesperado.' }
            $Source = $Entry.Open()
            try {
                $Target = [IO.File]::Create($ExecutableDownload)
                try { $Source.CopyTo($Target) }
                finally { $Target.Dispose() }
            }
            finally { $Source.Dispose() }
        }
        finally { $Archive.Dispose() }
        if (-not (Get-FileHash -LiteralPath $ExecutableDownload -Algorithm SHA256).Hash.Equals($ExpectedExeHash, [StringComparison]::OrdinalIgnoreCase)) {
            throw 'O aplicativo extraido nao corresponde ao SHA-256 publicado. Nada foi aberto.'
        }
        Move-Item -LiteralPath $ExecutableDownload -Destination $Executable -Force
        Write-Host "Abrindo GOAT Tweaks $ReleaseTag..."
        Start-Process -FilePath $Executable -ErrorAction Stop | Out-Null
    }
    finally {
        Remove-Item -LiteralPath $PackageDownload -Force -ErrorAction SilentlyContinue
        Remove-Item -LiteralPath $ExecutableDownload -Force -ErrorAction SilentlyContinue
        [Net.ServicePointManager]::SecurityProtocol = $PreviousProtocol
    }
}
