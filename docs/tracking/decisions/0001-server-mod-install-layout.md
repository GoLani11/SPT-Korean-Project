# 0001 Server Mod Install Layout

## Status

Accepted

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

Server packaging mirrors the SPT server mod layout. The installer accepts the outer SPT root, verifies `SPT_Runtime\SPT.Server.exe`, rejects servers outside the `4.1.x` family, and replaces only this server mod folder.

Runtime verification should use server startup logs and the SPT locale endpoint rather than BepInEx client logs.
