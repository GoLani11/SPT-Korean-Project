param(
    [string]$Configuration = "Release",
    [string]$OutputRoot
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version 3.0

$Utf8NoBom = [Text.UTF8Encoding]::new($false)
[Console]::InputEncoding = $Utf8NoBom
[Console]::OutputEncoding = $Utf8NoBom
$OutputEncoding = $Utf8NoBom
$env:PYTHONUTF8 = "1"
$env:PYTHONIOENCODING = "utf-8"

$ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $ProjectRoot "release"
}

$SolutionPath = Join-Path $ProjectRoot "SPT_Korean_Localization.sln"
$BuildOutput = Join-Path $ProjectRoot "bin\$Configuration\SPT_Korean_Localization"
$PackageModRoot = Join-Path $OutputRoot "SPT\user\mods\SPT_Korean_Localization"
$OutputRootFull = [IO.Path]::GetFullPath($OutputRoot)
$PackageModRootFull = [IO.Path]::GetFullPath($PackageModRoot)
$OutputPrefix = $OutputRootFull.TrimEnd([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar

if (-not $PackageModRootFull.StartsWith($OutputPrefix, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing to package outside output root: $PackageModRootFull"
}

dotnet restore $SolutionPath -v:minimal
dotnet build $SolutionPath -c $Configuration --no-restore -v:minimal

if (-not (Test-Path -LiteralPath (Join-Path $BuildOutput "SPT_Korean_Localization.dll"))) {
    throw "Build output is missing SPT_Korean_Localization.dll: $BuildOutput"
}

if (-not (Test-Path -LiteralPath (Join-Path $BuildOutput "locale\kr.json"))) {
    throw "Build output is missing locale\kr.json: $BuildOutput"
}

if (Test-Path -LiteralPath $PackageModRootFull) {
    Remove-Item -LiteralPath $PackageModRootFull -Recurse -Force
}

New-Item -ItemType Directory -Path $PackageModRootFull -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $BuildOutput "SPT_Korean_Localization.dll") -Destination $PackageModRootFull -Force

$DepsPath = Join-Path $BuildOutput "SPT_Korean_Localization.deps.json"
if (Test-Path -LiteralPath $DepsPath) {
    Copy-Item -LiteralPath $DepsPath -Destination $PackageModRootFull -Force
}

Copy-Item -LiteralPath (Join-Path $BuildOutput "locale") -Destination $PackageModRootFull -Recurse -Force

$LocalePath = Join-Path $PackageModRootFull "locale\kr.json"
$null = Get-Content -LiteralPath $LocalePath -Raw | ConvertFrom-Json -AsHashtable

Write-Output "Package created: $PackageModRootFull"
