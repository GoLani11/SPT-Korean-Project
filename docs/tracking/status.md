# Status

## Current State

Date: 2026-08-24

The repository builds exact-version Korean localization packages for SPT 3.8.3, 3.9.8, 3.10.5, 3.11.4, 4.0.13, and 4.1.0. SPT 4.1.2 and 4.1.3 share an explicitly labelled range package. Each release label has Korean-only and Korean-English variants, producing 14 ZIP files.

## Completed

- Added SPT 3.x CommonJS, SPT 4.0.13 net9, exact SPT 4.1.0 net10, and shared SPT 4.1.2–4.1.3 net10 server implementations.
- Replaced compile-time EFT/SPT client dependencies with runtime capability detection.
- Preserved the existing server GUID, client plugin GUID, and historical 3.x mod folder.
- Made `spt-korean-translate` generated outputs the only release locale source.
- Added deterministic generation and internal validation for 14 short-named ZIP files.
- Removed installer-based distribution and unrelated-version payloads from archives.

## Verification State

- The full solution builds with zero warnings and zero errors.
- All 14 locale variants match their exact English key set, order, and string types; the shared 4.1.2–4.1.3 English, KR, and KR-EN payloads are equivalent.
- All 14 ZIP files pass root-layout, safe-path, forbidden-file, manifest-version, locale-hash, and target-DLL-hash checks.
- The release build executes separate exact 4.1.0 and bounded 4.1.2+ server/client compatibility matrices and cannot reuse skipped stale binaries.
- Runtime checks completed for the previously supported packages through SPT 4.1.2; the 4.1.3 package was installed without starting the server or game.
- Every SPT API reference used by the 4.1 server DLL resolves against both the backed-up 4.1.2 assemblies and the installed 4.1.3 assemblies (15 checked, zero failures).
- A deliberately mismatched SPT 3.x package stayed inactive, logged the expected/actual versions, and allowed the server to keep running.
- The shared client DLL resolved all six patch targets against the installed SPT 4.1.3 client assemblies with zero failures; earlier compatibility results remain recorded for older releases.

## Open Work

- Later stable 4.1.x patches may satisfy the bounded binary rule but require explicit release-label, server API, translation, patch-target, and runtime review before distribution. Prereleases and SPT 4.2 are rejected.
- The newly generated exact 4.1.0 and shared 4.1.2–4.1.3 artifacts still need release runtime server-log and in-game UI/texture smoke tests when launching the game is allowed.
- Release smoke testing should still visually confirm the adjusted UI inside a launcher-started game; the automated client check proves target compatibility but does not render game screens.
