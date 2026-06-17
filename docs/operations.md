# Operations

## Build

```powershell
dotnet restore .\SPT-Korean-Project.sln -p:SptRoot=D:\SPT
dotnet build .\SPT-Korean-Project.sln -c Release --no-restore -p:SptRoot=D:\SPT
```

Expected build outputs:

```text
bin\Release\ServerLocaleMod\SPT_Korean_Localization.dll
bin\Release\ServerLocaleMod\locale\kr.json
bin\Release\ClientModFixPlugin\GoLani.KoreanModFix.dll
```

## Package

```powershell
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\tools\package-release.ps1 -SptRoot D:\SPT
```

The script restores, builds, packages the server mod, packages the client plugin, and validates the packaged locale JSON.

## Install

```powershell
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\tools\install-to-spt.ps1 -TargetSptRoot D:\SPT
```

The script requires `D:\SPT\SPT\SPT.Server.exe`, checks that the server version is in the `4.0.x` family, packages the solution, replaces only `D:\SPT\SPT\user\mods\SPT_Korean_Localization`, and copies `GoLani.KoreanModFix.dll` into `D:\SPT\BepInEx\plugins`.

Use `-SkipClientPlugin` when only the server locale mod should be installed.

## Runtime Check

Start `D:\SPT\SPT\SPT.Server.exe` and inspect the latest log under:

```text
D:\SPT\SPT\user\logs\spt
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
bin
obj
release
```
