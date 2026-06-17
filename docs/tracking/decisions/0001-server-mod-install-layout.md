# 0001 Server Mod Install Layout

## Status

Accepted

## Context

The current project output is a .NET library loaded by the SPT server mod loader. The local target install at `D:\SPT` contains the SPT server under `D:\SPT\SPT`. The mod needs access to SPT server services such as `DatabaseService`, so the BepInEx client plugin path is the wrong runtime surface.

## Decision

Install the mod under:

```text
<TargetSptRoot>\SPT\user\mods\SPT_Korean_Localization
```

Do not install this repository output under:

```text
<TargetSptRoot>\BepInEx\plugins
```

## Consequences

Packaging mirrors the SPT server mod layout. The installer accepts the outer SPT root, verifies `SPT\SPT.Server.exe`, rejects non-4.x servers, and replaces only this mod folder.

Runtime verification should use server startup logs and the SPT locale endpoint rather than BepInEx client logs.
