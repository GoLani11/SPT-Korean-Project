# Security And Boundaries

## Allowed Write Surface

The installer may replace only this server mod folder:

```text
<TargetSptRoot>\SPT_Runtime\user\mods\SPT_Korean_Localization
```

The installer may copy only this client plugin file:

```text
<TargetSptRoot>\BepInEx\plugins\GoLani.KoreanModFix.dll
```

The package script may write only under the configured release output root.

## Disallowed Surfaces

Do not write to:

```text
<TargetSptRoot>\EscapeFromTarkov.exe
<TargetSptRoot>\SPT_Runtime\SPT.Server.exe
<TargetSptRoot>\SPT_Runtime\SPT_Data
<TargetSptRoot>\BepInEx\core
<TargetSptRoot>\BepInEx\plugins\spt
<TargetSptRoot>\SPT_Runtime\user\mods\<other mod>
```

Do not modify BattlEye, launcher authentication, game executables, managed game assemblies, BepInEx core files, SPT client support plugins, or unrelated SPT files.

## Guard Requirements

Recursive delete operations must resolve full paths and prove the target remains inside the intended output or server mod folder. The installer must reject a target root that does not contain `SPT_Runtime\SPT.Server.exe`. Client plugin installation must be a single-file copy into `BepInEx\plugins`.

## Secrets

This project does not need tokens, cookies, account credentials, or private SPT profile data. Do not record user profile contents or account data in repo files.
