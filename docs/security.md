# Security And Boundaries

## Allowed Write Surface

The installer may replace only this mod folder:

```text
<TargetSptRoot>\SPT\user\mods\SPT_Korean_Localization
```

The package script may replace only the configured package folder under its output root.

## Disallowed Surfaces

Do not write to:

```text
<TargetSptRoot>\BepInEx\plugins
<TargetSptRoot>\EscapeFromTarkov.exe
<TargetSptRoot>\SPT\SPT.Server.exe
<TargetSptRoot>\SPT_Data
<TargetSptRoot>\SPT\user\mods\<other mod>
```

Do not modify BattlEye, launcher authentication, game executables, managed game assemblies, or unrelated SPT files.

## Guard Requirements

Recursive delete operations must resolve full paths and prove the target remains inside the intended output or mod folder. Install scripts must reject a target root that does not contain `SPT\SPT.Server.exe`.

## Secrets

This project does not need tokens, cookies, account credentials, or private SPT profile data. Do not record user profile contents or account data in repo files.
