param(
    [string]$TargetSptRoot = "D:\SPT",
    [string]$Configuration = "Release"
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
$TargetRootPath = [IO.Path]::GetFullPath($TargetSptRoot)
$ServerExe = Join-Path $TargetRootPath "SPT\SPT.Server.exe"

if (-not (Test-Path -LiteralPath $ServerExe)) {
    throw "Target does not look like an SPT install. Missing: $ServerExe"
}

$ServerVersion = [Diagnostics.FileVersionInfo]::GetVersionInfo($ServerExe).ProductVersion
if ([string]::IsNullOrWhiteSpace($ServerVersion) -or $ServerVersion -notmatch "^4\.") {
    throw "Target SPT version must be 4.x. Found: $ServerVersion"
}

$PackageScript = Join-Path $PSScriptRoot "package-release.ps1"
& $PackageScript -Configuration $Configuration

$SourceModRoot = Join-Path $ProjectRoot "release\SPT\user\mods\SPT_Korean_Localization"
if (-not (Test-Path -LiteralPath $SourceModRoot)) {
    throw "Package output is missing: $SourceModRoot"
}

$ModsRoot = Join-Path $TargetRootPath "SPT\user\mods"
New-Item -ItemType Directory -Path $ModsRoot -Force | Out-Null

$ModsRootFull = [IO.Path]::GetFullPath((Resolve-Path -LiteralPath $ModsRoot).Path)
$Destination = Join-Path $ModsRootFull "SPT_Korean_Localization"
$DestinationFull = [IO.Path]::GetFullPath($Destination)
$ModsPrefix = $ModsRootFull.TrimEnd([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar

if (-not $DestinationFull.StartsWith($ModsPrefix, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing to install outside SPT user mods folder: $DestinationFull"
}

if (Test-Path -LiteralPath $DestinationFull) {
    Remove-Item -LiteralPath $DestinationFull -Recurse -Force
}

Copy-Item -LiteralPath $SourceModRoot -Destination $ModsRootFull -Recurse -Force

if (-not (Test-Path -LiteralPath (Join-Path $DestinationFull "SPT_Korean_Localization.dll"))) {
    throw "Install failed. DLL missing from: $DestinationFull"
}

if (-not (Test-Path -LiteralPath (Join-Path $DestinationFull "locale\kr.json"))) {
    throw "Install failed. locale\kr.json missing from: $DestinationFull"
}

Write-Output "Installed SPT_Korean_Localization to $DestinationFull"
Write-Output "Target SPT version: $ServerVersion"
