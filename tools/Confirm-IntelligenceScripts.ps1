$ErrorActionPreference='Stop'
$Root=Split-Path $PSScriptRoot -Parent
foreach($Path in @('src\GoatTweaks.Windows\WindowsDiagnostics.cs','src\GoatTweaks\Hardware\HardwareDiscovery.cs')) {
    $Source=[IO.File]::ReadAllText((Join-Path $Root $Path))
    $Match=[regex]::Match($Source,'(?s)private const string (?:ReadFactsScript|Script) = """\r?\n(.*?)\r?\n\s*""";')
    if(-not $Match.Success){throw "Script local nao encontrado: $Path"}
    $Tokens=$null;$Errors=$null
    $null=[System.Management.Automation.Language.Parser]::ParseInput($Match.Groups[1].Value,[ref]$Tokens,[ref]$Errors)
    if($Errors.Count -gt 0){throw ($Path+': '+(($Errors|ForEach-Object{$_.Message}) -join ' | '))}
    Write-Host "Sintaxe Windows PowerShell conferida: $Path"
}
