# Status

## Current State

Date: 2026-08-16

The repository builds exact-version Korean localization packages for SPT 3.8.3, 3.9.8, 3.10.5, 3.11.4, 4.0.13, and 4.1.2. Each version has Korean-only and Korean-English variants.

## Completed

- Added SPT 3.x CommonJS, SPT 4.0.13 net9, and SPT 4.1.2 net10 server implementations.
- Replaced compile-time EFT/SPT client dependencies with runtime capability detection.
- Preserved the existing server GUID, client plugin GUID, and historical 3.x mod folder.
- Made `spt-korean-translate` generated outputs the only release locale source.
- Added deterministic generation and internal validation for 12 version-specific ZIP files.
- Removed installer-based distribution and unrelated-version payloads from archives.

## Verification State

- The full solution builds with zero warnings and zero errors.
- All 12 locale variants match their exact English key set, order, and string types.
- All 12 ZIP files pass root-layout, safe-path, forbidden-file, manifest-version, and locale-hash checks.
- Both variants were extracted and server-started on all six matching local SPT installations; each applied the expected locale key count.
- A deliberately mismatched SPT 3.x package stayed inactive, logged the expected/actual versions, and allowed the server to keep running.
- The common client DLL resolved every supported patch target against each installed version's real `Assembly-CSharp.dll`: five enabled and prestige unavailable on 3.8.3–3.10.5, six enabled on 3.11.4–4.1.2, and zero failures.

## Open Work

- Future SPT versions require explicit server API, translation, patch-target, and in-game verification before a new ZIP is added.
- Release smoke testing should still visually confirm the adjusted UI inside a launcher-started game; the automated client check proves target compatibility but does not render game screens.
