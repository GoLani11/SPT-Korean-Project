# Tools Guide

## Scope

Scripts in this folder are user entry points for package and local install work. Keep them conservative, Windows-friendly, and safe for non-technical use.

## Rules

- Use PowerShell 7 compatible syntax.
- Set UTF-8 console and Python encoding variables when locale data can appear in output.
- Resolve full paths before recursive delete operations.
- Restrict package writes to the chosen output root.
- Restrict server install writes to the target SPT `user\mods\SPT_Korean_Localization` folder.
- Restrict client install writes to the single target `BepInEx\plugins\GoLani.KoreanModFix.dll` file.
- Do not recursively delete anything under `BepInEx`.
- Reject a target that lacks `SPT\SPT.Server.exe`.
- Reject SPT versions outside the supported `4.0.x` family.

## Verification

Test the package script, a successful install path, and at least one rejected fake target path after changing installer logic.
