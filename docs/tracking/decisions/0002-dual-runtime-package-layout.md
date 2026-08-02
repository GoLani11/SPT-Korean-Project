# 0002 Dual Runtime Package Layout

## Status

Accepted

## Context

The project historically shipped a server-side Korean locale mod and a separate BepInEx client plugin for Korean UI display fixes. The user wants those files managed together in this repository while preserving the ability to install both into the correct SPT locations.

The two components run in different processes and use different dependency sets. The server mod uses SPT server packages. The client plugin uses BepInEx, Harmony, Unity, EFT client assemblies, and SPT client support DLLs.

## Decision

Use one repository and one solution, but keep two source projects:

```text
src\ServerLocaleMod
src\ClientModFixPlugin
```

Build and package two runtime outputs:

```text
SPT_Runtime\user\mods\SPT_Korean_Localization
BepInEx\plugins\GoLani.KoreanModFix.dll
```

## Consequences

Scripts can offer one command for the full Korean package. Runtime boundaries stay clear, so server compatibility work does not accidentally pull in BepInEx dependencies and client UI patch work does not affect server mod loading.

Client plugin compatibility needs its own runtime QA because private UI patch targets can drift between EFT/SPT releases.
