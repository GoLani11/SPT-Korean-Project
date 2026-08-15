# 0002 Dual Runtime Package Layout

## Status

Accepted

## Context

The project historically shipped a server-side Korean locale mod and a separate BepInEx client plugin for Korean UI display fixes. The user wants those files managed together in this repository while preserving the ability to install both into the correct SPT locations.

The two components run in different processes and use different dependency sets. The server mod uses SPT server packages. The client plugin builds only against the BepInEx, Harmony, Unity, and TextMeshPro surface shared by all supported clients; EFT and SPT types are resolved at runtime.

## Decision

Use one repository and one solution, while keeping client and server runtime boundaries separate. Server source is split where the SPT loader API differs:

```text
src\ServerLocaleMod3
src\ServerLocaleMod40
src\ServerLocaleMod
src\ClientModFixPlugin
```

Each release asset contains two runtime outputs at the paths required by its target version:

```text
<version-specific server mod path>
BepInEx\plugins\GoLani.KoreanModFix.dll
```

## Consequences

Scripts can offer one command for the full Korean package. Runtime boundaries stay clear, so server compatibility work does not accidentally pull in BepInEx dependencies and client UI patch work does not affect server mod loading.

Client plugin compatibility needs its own runtime QA because private UI patch targets can drift between EFT/SPT releases.
