# Business Rules

## Localization Scope

The project exists to make SPT Korean locale text available through the SPT server locale endpoint. It is not a gameplay mod, launcher mod, client patch, asset bundle, or anti-cheat workaround.

## Translation Preservation

Existing Korean translations are project data. Do not refresh, rewrite, normalize, or machine-translate the whole locale file during compatibility work. Small data fixes are acceptable only when they repair a compatibility mismatch, JSON validity issue, or known broken key.

## Compatibility Target

The current supported family is SPT 4.0.x. Package references are pinned to SPT 4.0.13 because the local target install reports SPT 4.0.13.

## Install Location

The canonical install path for the local target is:

```text
D:\SPT\SPT\user\mods\SPT_Korean_Localization
```

Equivalent installs under another SPT root must preserve the same `SPT\user\mods` layout.

## User-Facing Promise

The README command should remain the simplest supported path for non-technical users. If scripts or output layout change, README instructions must change in the same work.
