# Status

Date: 2026-09-12

## Current state

2.1.0 is prepared as one common plugin-only ZIP with both language modes. The previous prototype is now the release implementation. No GitHub release/tag or asset upload was performed during preparation; publication remains the author's action.

The client logs version 2.1.0 and translation initialization results in BepInEx. Server locale modules and optional status modules are excluded. Existing UI fixes and native language switching remain included.

The release payloads use translation commit `c5fa8fa41eeeff40684632bc2747cdfc80a07e2c`.
This refresh corrects mistranslations and aligns item names, colors, part terms,
Hideout descriptions and quest objective wording. The base dataset changes 736
values; older datasets retain their version-specific overrides and quest facts.
The existing IceBreaker name and Boreas description remain included, and the
4.1.5 profile continues to use the verified 4.1.3 dataset.

The active `D:\SPT-Korean-Release\2.1.0` handoff and six available SPT installation
roots were refreshed in place without a new backup at the user's request. The installed
plugin and locale bundle now report 2.1.0, and the retired test status companions were
removed from those roots.

## Verification

- All 24 packaged locale files match the pinned translation sources byte for byte. All eight English payloads and every locale's key set/order remain unchanged from the previous handoff. The 16 Korean/Korean-English payloads contain the changes listed below.
- All nine translation profiles pass full-payload/native-flow contracts, including both reload behaviors and a simulated compatible 4.1.6 upgrade.
- Windows .NET Framework and five installed Unity Mono runtimes each pass 1,164,129 locale assertions and 28 short-name lifecycle checks using real Harmony.
- Installed client metadata probes pass for 3.8.3, 3.9.8, 3.10.5, 3.11.4 and 4.1.5.
- The final ZIP is reopened for exact file-path/hash validation, extracted, and tested again on Windows .NET Framework.
- Binary assembly version, plugin registration, version constant, manifest version and DLL hash are checked together.
- Translation validation passes all 268 pytest tests and rebuilds all eight supported versions without changing the pinned outputs. All nine Python packaging tests pass. The release plugin and two contract projects build with zero warnings/errors using the local SPT 3.8.3 client references.
- All 30 package files match the release archive on each installed root, and each installed English source matches its selected profile. Windows native-flow verification passes against all six installed bundles. SPT 3.8.3, 3.9.8, 3.10.5, 3.11.4 and 4.1.5 also pass 1,164,129 assertions and 28 short-name checks on their own Unity Mono runtimes.

Changed values in each language mode, compared with the September 11 handoff:

| SPT profile | Translation dataset | Changed values per Korean payload |
| --- | --- | ---: |
| 3.8.3 | 3.8.3 | 447 |
| 3.9.8 | 3.9.8 | 510 |
| 3.10.5 | 3.10.5 | 605 |
| 3.11.4 | 3.11.4 | 669 |
| 4.0.13 | 4.0.13 | 716 |
| 4.1.0 | 4.1.0 | 736 |
| 4.1.2 | 4.1.2 | 736 |
| 4.1.3 | 4.1.3 | 736 |
| 4.1.5 | 4.1.3 (shared) | 736 |

## Limits

Local 4.0.13 lacks game assemblies/runtime; its installed payload passes hash, English-source and Windows fixture checks only. SPT 4.1.0, 4.1.2 and 4.1.3 installation roots are unavailable, so their profiles retain payload and fixture coverage only. Automated Unity Mono tests do not render game screens. This refresh has not been visually tested in the game.

The matching local server database is required. Server-rendered text and every other-mod combination are not guaranteed. Future stable 4.1.x patches must pass runtime compatibility checks; the simulated upgrade does not certify an actual future installation.

See [release notes](../releases/2.1.0-release-notes.md), [installation guide](../releases/2.1.0-install.md) and [architecture](../architecture.md).
