# Status

## Current State

Date: 2026-08-23

The repository builds exact-version Korean localization packages for SPT 3.8.3, 3.9.8, 3.10.5, 3.11.4, 4.0.13, and 4.1.3. Each version has Korean-only and Korean-English variants.

## Completed

- Added SPT 3.x CommonJS, SPT 4.0.13 net9, and SPT 4.1.3 net10 server implementations.
- Replaced compile-time EFT/SPT client dependencies with runtime capability detection.
- Preserved the existing server GUID, client plugin GUID, and historical 3.x mod folder.
- Made `spt-korean-translate` generated outputs the only release locale source.
- Added deterministic generation and internal validation for 12 version-specific ZIP files.
- Removed installer-based distribution and unrelated-version payloads from archives.

## Verification State

- The full solution builds with zero warnings and zero errors.
- All 12 locale variants match their exact English key set, order, and string types.
- All 12 ZIP files pass root-layout, safe-path, forbidden-file, manifest-version, and locale-hash checks.
- Runtime checks completed for the previously supported packages through SPT 4.1.2; the 4.1.3 package was installed without starting the server or game.
- A deliberately mismatched SPT 3.x package stayed inactive, logged the expected/actual versions, and allowed the server to keep running.
- The common client DLL resolved all six patch targets against the installed SPT 4.1.3 client assemblies with zero failures; earlier compatibility results remain recorded for older releases.

## Open Work

- Future SPT versions require explicit server API, translation, patch-target, and in-game verification before a new ZIP is added.
- SPT 4.1.3 still needs a later runtime server-log and in-game UI/texture smoke test when launching the game is allowed.
- Release smoke testing should still visually confirm the adjusted UI inside a launcher-started game; the automated client check proves target compatibility but does not render game screens.
