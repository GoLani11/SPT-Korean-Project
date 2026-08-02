# Business Rules

## Package Scope

The project provides a Korean SPT package with two pieces:

- A server locale mod that applies Korean locale strings.
- A BepInEx client plugin that adjusts Korean UI display issues.

It is not a gameplay mod, launcher mod, asset bundle, anti-cheat workaround, or account/profile migration tool.

## Translation Preservation

Existing Korean translations are project data. Do not refresh, rewrite, normalize, or machine-translate the whole locale file during compatibility work. Small data fixes are acceptable only when they repair a compatibility mismatch, JSON validity issue, or known broken key.

## Compatibility Target

The current supported family is SPT 4.1.x. The verified local target is SPT 4.1.0.

## Install Locations

The canonical install paths for the local target are:

```text
D:\SPT\SPT_Runtime\user\mods\SPT_Korean_Localization
D:\SPT\BepInEx\plugins\GoLani.KoreanModFix.dll
```

Equivalent installs under another SPT root must preserve the same server and BepInEx layout.

## User-Facing Promise

The README command should remain the simplest supported path for non-technical users. If scripts, source layout, or output layout change, README instructions must change in the same work.
