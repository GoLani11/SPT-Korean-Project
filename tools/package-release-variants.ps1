param(
    [string]$Configuration = "Release",
    [string]$SptRoot = "D:\SPT",
    [string]$ReleaseRoot,
    [string]$BilingualPackageName = "SPT_Korean_Localization.KR.EN._G.M",
    [string]$KoreanOnlyPackageName = "SPT_Korean_Localization.KR._G.M",
    [switch]$RemoveDescriptionEnglishHeader,
    [switch]$KeepWorkFolders
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version 3.0

$Utf8NoBom = [Text.UTF8Encoding]::new($false)
[Console]::InputEncoding = $Utf8NoBom
[Console]::OutputEncoding = $Utf8NoBom
$OutputEncoding = $Utf8NoBom
$env:PYTHONUTF8 = "1"
$env:PYTHONIOENCODING = "utf-8"

function Get-FullPath {
    param([Parameter(Mandatory = $true)][string]$Path)
    return [IO.Path]::GetFullPath($Path)
}

function Assert-UnderRoot {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Root,
        [switch]$AllowRoot
    )

    $FullPath = Get-FullPath $Path
    $FullRoot = (Get-FullPath $Root).TrimEnd(
        [IO.Path]::DirectorySeparatorChar,
        [IO.Path]::AltDirectorySeparatorChar
    )
    $Prefix = $FullRoot + [IO.Path]::DirectorySeparatorChar

    if ($FullPath.Equals($FullRoot, [StringComparison]::OrdinalIgnoreCase)) {
        if ($AllowRoot) {
            return $FullPath
        }
        throw "Refusing to operate on output root itself: $FullPath"
    }

    if (-not $FullPath.StartsWith($Prefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to operate outside release root: $FullPath"
    }

    return $FullPath
}

function Remove-GeneratedPath {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Root
    )

    $FullPath = Assert-UnderRoot -Path $Path -Root $Root
    if (Test-Path -LiteralPath $FullPath) {
        Remove-Item -LiteralPath $FullPath -Recurse -Force
    }
}

function Copy-DirectoryContents {
    param(
        [Parameter(Mandatory = $true)][string]$Source,
        [Parameter(Mandatory = $true)][string]$Destination
    )

    if (-not (Test-Path -LiteralPath $Source)) {
        throw "Source directory is missing: $Source"
    }

    New-Item -ItemType Directory -Path $Destination -Force | Out-Null
    Get-ChildItem -LiteralPath $Source -Force | Copy-Item -Destination $Destination -Recurse -Force
}

function Rename-ModFolder {
    param(
        [Parameter(Mandatory = $true)][string]$PackageRoot,
        [Parameter(Mandatory = $true)][string]$PackageName
    )

    $ModsRoot = Join-Path $PackageRoot "SPT\user\mods"
    $OriginalModRoot = Join-Path $ModsRoot "SPT_Korean_Localization"
    $VariantModRoot = Join-Path $ModsRoot $PackageName

    if (-not (Test-Path -LiteralPath $OriginalModRoot)) {
        throw "Package mod folder is missing: $OriginalModRoot"
    }

    Move-Item -LiteralPath $OriginalModRoot -Destination $VariantModRoot
    return $VariantModRoot
}

function Test-VariantPackage {
    param(
        [Parameter(Mandatory = $true)][string]$PackageRoot,
        [Parameter(Mandatory = $true)][string]$PackageName
    )

    $ModRoot = Join-Path $PackageRoot "SPT\user\mods\$PackageName"
    $LocalePath = Join-Path $ModRoot "locale\kr.json"
    $ServerDllPath = Join-Path $ModRoot "SPT_Korean_Localization.dll"
    $ClientPluginPath = Join-Path $PackageRoot "BepInEx\plugins\GoLani.KoreanModFix.dll"

    foreach ($Path in @($LocalePath, $ServerDllPath, $ClientPluginPath)) {
        if (-not (Test-Path -LiteralPath $Path)) {
            throw "Variant package is missing: $Path"
        }
    }

    $null = Get-Content -LiteralPath $LocalePath -Raw | ConvertFrom-Json -AsHashtable
}

function New-ZipFromDirectoryContents {
    param(
        [Parameter(Mandatory = $true)][string]$SourceRoot,
        [Parameter(Mandatory = $true)][string]$ZipPath,
        [Parameter(Mandatory = $true)][string]$ReleaseRoot
    )

    $ZipFull = Assert-UnderRoot -Path $ZipPath -Root $ReleaseRoot
    if (Test-Path -LiteralPath $ZipFull) {
        Remove-Item -LiteralPath $ZipFull -Force
    }

    $Items = Get-ChildItem -LiteralPath $SourceRoot -Force
    if ($Items.Count -eq 0) {
        throw "Cannot create an empty zip from: $SourceRoot"
    }

    Compress-Archive -LiteralPath $Items.FullName -DestinationPath $ZipFull -Force
    return $ZipFull
}

$ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
if ([string]::IsNullOrWhiteSpace($ReleaseRoot)) {
    $ReleaseRoot = Join-Path $ProjectRoot "artifacts\release"
}

$ReleaseRootFull = Get-FullPath $ReleaseRoot
New-Item -ItemType Directory -Path $ReleaseRootFull -Force | Out-Null

$BaseRoot = Join-Path $ReleaseRootFull "_base"
$StagingRoot = Join-Path $ReleaseRootFull "_staging"
$BilingualRoot = Join-Path $StagingRoot $BilingualPackageName
$KoreanOnlyRoot = Join-Path $StagingRoot $KoreanOnlyPackageName
$BilingualZip = Join-Path $ReleaseRootFull "$BilingualPackageName.zip"
$KoreanOnlyZip = Join-Path $ReleaseRootFull "$KoreanOnlyPackageName.zip"

try {
    Remove-GeneratedPath -Path $BaseRoot -Root $ReleaseRootFull
    Remove-GeneratedPath -Path $StagingRoot -Root $ReleaseRootFull

    $PackageScript = Join-Path $PSScriptRoot "package-release.ps1"
    & $PackageScript -Configuration $Configuration -OutputRoot $BaseRoot -SptRoot $SptRoot

    Copy-DirectoryContents -Source $BaseRoot -Destination $BilingualRoot
    Copy-DirectoryContents -Source $BaseRoot -Destination $KoreanOnlyRoot

    $null = Rename-ModFolder -PackageRoot $BilingualRoot -PackageName $BilingualPackageName
    $KoreanOnlyModRoot = Rename-ModFolder -PackageRoot $KoreanOnlyRoot -PackageName $KoreanOnlyPackageName

    $KoreanOnlyLocale = Join-Path $KoreanOnlyModRoot "locale\kr.json"
    $ConverterScript = Join-Path $PSScriptRoot "convert-locale-to-korean-only.ps1"
    $ConverterArgs = @{
        InputPath = $KoreanOnlyLocale
        OutputPath = $KoreanOnlyLocale
    }
    if ($RemoveDescriptionEnglishHeader) {
        $ConverterArgs.RemoveDescriptionEnglishHeader = $true
    }
    & $ConverterScript @ConverterArgs

    Test-VariantPackage -PackageRoot $BilingualRoot -PackageName $BilingualPackageName
    Test-VariantPackage -PackageRoot $KoreanOnlyRoot -PackageName $KoreanOnlyPackageName

    $BilingualZipFull = New-ZipFromDirectoryContents -SourceRoot $BilingualRoot -ZipPath $BilingualZip -ReleaseRoot $ReleaseRootFull
    $KoreanOnlyZipFull = New-ZipFromDirectoryContents -SourceRoot $KoreanOnlyRoot -ZipPath $KoreanOnlyZip -ReleaseRoot $ReleaseRootFull

    Write-Output "Release assets created:"
    Write-Output $BilingualZipFull
    Write-Output $KoreanOnlyZipFull
}
finally {
    if (-not $KeepWorkFolders) {
        foreach ($Path in @($BaseRoot, $StagingRoot)) {
            if (Test-Path -LiteralPath $Path) {
                Remove-GeneratedPath -Path $Path -Root $ReleaseRootFull
            }
        }
    }
}
