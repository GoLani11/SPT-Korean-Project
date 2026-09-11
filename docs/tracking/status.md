# Status

Date: 2026-09-11

## Current state

2.1.0 is prepared as one common plugin-only ZIP with both language modes. The previous prototype is now the release implementation. No GitHub release/tag or asset upload was performed during preparation; publication remains the author's action.

The client logs version 2.1.0 and translation initialization results in BepInEx. Server locale modules and optional status modules are excluded. Existing UI fixes and native language switching remain included.

The release payloads use translation commit `7c2278519ad3f4af42af1a29a047b8ad1a587d0f`.
All eight translation datasets include the IceBreaker name and Boreas description;
Korean-English mode displays `쇄빙선 (IceBreaker)` and `[IceBreaker, 쇄빙선]`.
The 4.1.5 profile continues to use the 4.1.3 dataset. The earlier delivery is backed
up outside the active `D:\SPT-Korean-Release\2.1.0` handoff folder.

## Verification

- All 24 packaged locale files match the current translation sources byte for byte. Compared with the previous handoff, each of the 16 Korean/Korean-English payloads changes only the IceBreaker name and description.
- All nine translation profiles pass full-payload/native-flow contracts, including both reload behaviors and a simulated compatible 4.1.6 upgrade.
- Windows .NET Framework and five installed Unity Mono runtimes each pass 1,164,129 locale assertions and 28 short-name lifecycle checks using real Harmony.
- Installed client metadata probes pass for 3.8.3, 3.9.8, 3.10.5, 3.11.4 and 4.1.5.
- The final ZIP is reopened for exact file-path/hash validation, extracted, and tested again on Windows .NET Framework.
- Binary assembly version, plugin registration, version constant, manifest version and DLL hash are checked together.
- All nine Python packaging tests pass. The full 14-project solution builds with zero warnings/errors when the local client reference root is supplied; generated reports accompany the local handoff.

## Limits

Local 4.0.13 lacks game assemblies/runtime; 4.1.0, 4.1.2 and 4.1.3 game clients are unavailable. Their payload and fixture coverage does not establish actual-client or visual compatibility. Automated Unity Mono tests do not render game screens. Earlier user-confirmed gameplay tests informed preparation; the final version/log change was verified automatically.

The matching local server database is required. Server-rendered text and every other-mod combination are not guaranteed. Future stable 4.1.x patches must pass runtime compatibility checks; the simulated upgrade does not certify an actual future installation.

See [release notes](../releases/2.1.0-release-notes.md), [installation guide](../releases/2.1.0-install.md) and [architecture](../architecture.md).
