param(
    [string]$Configuration = "Release",
    [string]$OutputRoot,
    [string]$SptRoot = "D:\SPT"
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
    $OutputRoot = Join-Path $ProjectRoot "artifacts\release"
}

$SolutionPath = Join-Path $ProjectRoot "SPT-Korean-Project.sln"
$BuildRoot = Join-Path $ProjectRoot "artifacts\build\$Configuration"
$ServerBuildOutput = Join-Path $BuildRoot "ServerLocaleMod"
$ClientBuildOutput = Join-Path $BuildRoot "ClientModFixPlugin"
$PackageModRoot = Join-Path $OutputRoot "SPT_Runtime\user\mods\SPT_Korean_Localization"
$PackageClientPluginsRoot = Join-Path $OutputRoot "BepInEx\plugins"
$PackageClientPluginPath = Join-Path $PackageClientPluginsRoot "GoLani.KoreanModFix.dll"
$OutputRootFull = [IO.Path]::GetFullPath($OutputRoot)
$PackageModRootFull = [IO.Path]::GetFullPath($PackageModRoot)
$PackageClientPluginsRootFull = [IO.Path]::GetFullPath($PackageClientPluginsRoot)
$PackageClientPluginFull = [IO.Path]::GetFullPath($PackageClientPluginPath)
$OutputPrefix = $OutputRootFull.TrimEnd([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar

if (-not $PackageModRootFull.StartsWith($OutputPrefix, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing to package outside output root: $PackageModRootFull"
}

if (-not $PackageClientPluginFull.StartsWith($OutputPrefix, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing to package outside output root: $PackageClientPluginFull"
}

dotnet restore $SolutionPath -p:SptRoot=$SptRoot -v:minimal
if ($LASTEXITCODE -ne 0) {
    throw "dotnet restore failed with exit code $LASTEXITCODE"
}

dotnet build $SolutionPath -c $Configuration --no-restore -p:SptRoot=$SptRoot -v:minimal
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed with exit code $LASTEXITCODE"
}

if (-not (Test-Path -LiteralPath (Join-Path $ServerBuildOutput "SPT_Korean_Localization.dll"))) {
    throw "Build output is missing SPT_Korean_Localization.dll: $ServerBuildOutput"
}

if (-not (Test-Path -LiteralPath (Join-Path $ServerBuildOutput "locale\kr.json"))) {
    throw "Build output is missing locale\kr.json: $ServerBuildOutput"
}

if (-not (Test-Path -LiteralPath (Join-Path $ClientBuildOutput "GoLani.KoreanModFix.dll"))) {
    throw "Build output is missing GoLani.KoreanModFix.dll: $ClientBuildOutput"
}

if (Test-Path -LiteralPath $PackageModRootFull) {
    Remove-Item -LiteralPath $PackageModRootFull -Recurse -Force
}

New-Item -ItemType Directory -Path $PackageModRootFull -Force | Out-Null
New-Item -ItemType Directory -Path $PackageClientPluginsRootFull -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $ServerBuildOutput "SPT_Korean_Localization.dll") -Destination $PackageModRootFull -Force

$DepsPath = Join-Path $ServerBuildOutput "SPT_Korean_Localization.deps.json"
if (Test-Path -LiteralPath $DepsPath) {
    Copy-Item -LiteralPath $DepsPath -Destination $PackageModRootFull -Force
}

Copy-Item -LiteralPath (Join-Path $ServerBuildOutput "locale") -Destination $PackageModRootFull -Recurse -Force
Copy-Item -LiteralPath (Join-Path $ClientBuildOutput "GoLani.KoreanModFix.dll") -Destination $PackageClientPluginFull -Force

$PackagedLocaleRoot = Join-Path $PackageModRootFull "locale"
Get-ChildItem -LiteralPath $PackagedLocaleRoot -Recurse -File |
    Where-Object { $_.Name -in @("AGENTS.md", "CLAUDE.md") } |
    Remove-Item -Force

$LocalePath = Join-Path $PackageModRootFull "locale\kr.json"
$null = Get-Content -LiteralPath $LocalePath -Raw | ConvertFrom-Json -AsHashtable

Write-Output "Package created: $PackageModRootFull"
Write-Output "Client plugin packaged: $PackageClientPluginFull"
