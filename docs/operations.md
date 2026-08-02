# Operations

## Build

```powershell
dotnet restore .\SPT-Korean-Project.sln -p:SptRoot=D:\SPT
dotnet build .\SPT-Korean-Project.sln -c Release --no-restore -p:SptRoot=D:\SPT
```

Expected build outputs:

```text
artifacts\build\Release\ServerLocaleMod\SPT_Korean_Localization.dll
artifacts\build\Release\ServerLocaleMod\locale\kr.json
artifacts\build\Release\ClientModFixPlugin\GoLani.KoreanModFix.dll
```

## Package

```powershell
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\tools\package-release.ps1 -SptRoot D:\SPT
```

The script restores, builds, packages the server mod, packages the client plugin, and validates the packaged locale JSON.

## GitHub Release Assets

Double-click:

```text
make-release-packages.bat
```

Or run:

```powershell
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\tools\package-release-variants.ps1 -SptRoot D:\SPT
```

Expected zip outputs:

```text
artifacts\release\SPT_Korean_Localization.KR.EN._G.M.zip
artifacts\release\SPT_Korean_Localization.KR._G.M.zip
```

The `KR.EN` zip uses `src\ServerLocaleMod\locale\kr.json`, and the `KR` zip uses `src\ServerLocaleMod\locale\kr-only.json`. The package workflow validates both JSON files and requires each staged locale to match its source SHA-256 hash.

## Install

```powershell
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\tools\install-to-spt.ps1 -TargetSptRoot D:\SPT
```

The script requires `D:\SPT\SPT_Runtime\SPT.Server.exe`, checks that the server version is in the `4.1.x` family, packages the solution, replaces only `D:\SPT\SPT_Runtime\user\mods\SPT_Korean_Localization`, and copies `GoLani.KoreanModFix.dll` into `D:\SPT\BepInEx\plugins`.

Use `-SkipClientPlugin` when only the server locale mod should be installed.

## Runtime Check

Start `D:\SPT\SPT_Runtime\SPT.Server.exe` and inspect the latest log under:

```text
D:\SPT\SPT_Runtime\user\logs\spt
```

Expected server evidence includes:

```text
SPT_Korean_Localization_(G&M)
31084
/client/locale/kr
```

Client plugin runtime evidence requires launching the SPT client and checking the BepInEx log plus the affected UI screens.

## Cleanup

Generated folders may be removed after verification:

```text
artifacts
bin
obj
release
```
