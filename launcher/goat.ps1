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

    function Test-GoatDownload {
        param([string]$Path, [string]$ExpectedHash)
        if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { return $false }
        try {
            return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.Equals($ExpectedHash, [StringComparison]::OrdinalIgnoreCase)
        }
        catch { return $false }
    }

    function Get-GoatPackage {
        param([string]$Uri, [string]$Destination, [string]$ExpectedHash, [string]$CurlExecutable)
        # Scope this preference to the function. Windows PowerShell 5.1 ignores it.
        $PSNativeCommandUseErrorActionPreference = $false
        $LastFailure = 'A conexao com o servidor de download foi interrompida.'
        if (-not [string]::IsNullOrWhiteSpace($CurlExecutable)) {
            for ($Attempt = 1; $Attempt -le 3; $Attempt++) {
                try {
                    Write-Host "Baixando o pacote - tentativa $Attempt de 3..."
                    # Use the real Windows executable, never the PowerShell curl alias.
                    # Resume only this invocation's partial file; authenticate every result by hash.
                    $CurlArguments = @(
                        '--disable', '--fail', '--location', '--proto', '=https', '--proto-redir', '=https',
                        '--tlsv1.2', '--http1.1', '--connect-timeout', '20', '--max-time', '600',
                        '--speed-limit', '1024', '--speed-time', '45', '--continue-at', '-',
                        '--progress-bar', '--output', $Destination, '--url', $Uri
                    )
                    & $CurlExecutable @CurlArguments
                    $DownloadCode = $LASTEXITCODE
                    if (Test-GoatDownload -Path $Destination -ExpectedHash $ExpectedHash) { return }
                    if ($DownloadCode -eq 0) {
                        Remove-Item -LiteralPath $Destination -Force -ErrorAction SilentlyContinue
                        throw 'O pacote recebido nao corresponde ao SHA-256 publicado.'
                    }
                    if ($DownloadCode -in 33, 36) {
                        # A server that refuses byte ranges needs a fresh transfer.
                        Remove-Item -LiteralPath $Destination -Force -ErrorAction SilentlyContinue
                    }
                    throw "Download interrompido (codigo $DownloadCode)."
                }
                catch {
                    $LastFailure = $_.Exception.Message
                    if ($Attempt -lt 3) {
                        Write-Host 'Conexao interrompida. Tentando retomar o download...' -ForegroundColor Yellow
                        Start-Sleep -Seconds (2 * $Attempt)
                    }
                }
            }
            Write-Host 'Tentando outra forma de download...' -ForegroundColor Yellow
        }
        # A separate transport also supports machines without the built-in curl executable.
        for ($Attempt = 1; $Attempt -le 2; $Attempt++) {
            try {
                Remove-Item -LiteralPath $Destination -Force -ErrorAction SilentlyContinue
                Write-Host "Baixando por conexao alternativa - tentativa $Attempt de 2..."
                Invoke-WebRequest -UseBasicParsing -DisableKeepAlive -TimeoutSec 600 -Uri $Uri -OutFile $Destination | Out-Null
                if (Test-GoatDownload -Path $Destination -ExpectedHash $ExpectedHash) { return }
                throw 'O pacote recebido nao corresponde ao SHA-256 publicado.'
            }
            catch {
                $LastFailure = $_.Exception.Message
                Remove-Item -LiteralPath $Destination -Force -ErrorAction SilentlyContinue
                if ($Attempt -lt 2) {
                    Write-Host 'Conexao interrompida. Repetindo o download...' -ForegroundColor Yellow
                    Start-Sleep -Seconds 3
                }
            }
        }
        throw "Nao foi possivel concluir o download. $LastFailure Baixe o ZIP pelo navegador e extraia GOATTweaks.exe: $Uri"
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
    $CurlExecutable = Join-Path ([Environment]::GetFolderPath('System')) 'curl.exe'
    if (-not [Environment]::Is64BitProcess) {
        $NativeCurl = Join-Path ([Environment]::GetFolderPath('Windows')) 'Sysnative\curl.exe'
        if (Test-Path -LiteralPath $NativeCurl -PathType Leaf) { $CurlExecutable = $NativeCurl }
    }
    if (-not (Test-Path -LiteralPath $CurlExecutable -PathType Leaf)) { $CurlExecutable = '' }
    try {
        New-Item -ItemType Directory -Path $InstallDirectory -Force | Out-Null
        Write-Host "Baixando o pacote compacto do GOAT Tweaks $ReleaseTag..."
        Write-Host 'Este download so ocorre na primeira abertura desta versao ou para recuperar uma copia invalida.'
        Get-GoatPackage -Uri "$BaseUrl/GOATTweaks-win-x64.zip" -Destination $PackageDownload -ExpectedHash $ExpectedPackageHash -CurlExecutable $CurlExecutable
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
