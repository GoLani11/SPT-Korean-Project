# 0001 Server Mod Install Layout

## Status

Superseded by `0003-version-specific-release-layout`

## Context

The server locale output is a .NET library loaded by the SPT server mod loader. The local target install at `D:\SPT` contains the SPT server under `D:\SPT\SPT_Runtime`. The server mod needs access to SPT server data such as `LocaleTable`, so the BepInEx client plugin path is the wrong runtime surface for this DLL.

## Decision

Install the server locale mod under:

```text
<TargetSptRoot>\SPT_Runtime\user\mods\SPT_Korean_Localization
```

Do not install `SPT_Korean_Localization.dll` under:

```text
<TargetSptRoot>\BepInEx\plugins
```

## Consequences

This was the 4.1-only server layout. Current releases use the version-specific paths in decision 0003 and have no installer.

Server runtime verification uses server startup logs; client UI verification uses BepInEx logs and in-game inspection.
