# Operations

## Build

```powershell
dotnet restore .\SPT_Korean_Localization.sln
dotnet build .\SPT_Korean_Localization.sln -c Release --no-restore
```

The expected build output is:

```text
bin\Release\SPT_Korean_Localization\SPT_Korean_Localization.dll
bin\Release\SPT_Korean_Localization\locale\kr.json
```

## Package

```powershell
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\tools\package-release.ps1
```

The script restores, builds, copies the DLL and locale folder, copies `.deps.json` when present, and validates the packaged JSON.

## Install

```powershell
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\tools\install-to-spt.ps1 -TargetSptRoot D:\SPT
```

The script requires `D:\SPT\SPT\SPT.Server.exe`, checks that the server version starts with `4.`, packages the mod, then replaces only `D:\SPT\SPT\user\mods\SPT_Korean_Localization`.

## Runtime Check

Start `D:\SPT\SPT\SPT.Server.exe` and inspect the latest log under:

```text
D:\SPT\SPT\user\logs\spt
```

Expected evidence includes:

```text
SPT_Korean_Localization_(G&M)
31084
/client/locale/kr
```

## Cleanup

Generated folders may be removed after verification:

```text
bin
obj
release
```
