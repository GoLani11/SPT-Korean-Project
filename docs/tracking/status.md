# Status

## Current State

Date: 2026-08-26

The repository builds exact-version Korean localization packages for SPT 3.8.3, 3.9.8, 3.10.5, 3.11.4, 4.0.13, and 4.1.0. SPT 4.1.2 and 4.1.3 share an explicitly labelled range package. Each release label has one ZIP containing both `kr` and `kr-en`, producing seven ZIP files.

## Completed

- Added SPT 3.x CommonJS, SPT 4.0.13 net9, exact SPT 4.1.0 net10, and shared SPT 4.1.2–4.1.3 net10 server implementations.
- Replaced compile-time EFT/SPT client dependencies with runtime capability detection.
- Preserved the existing server GUID, client plugin GUID, and historical 3.x mod folder.
- Made `spt-korean-translate` generated outputs the only release locale source.
- Added `kr-en` to the native language list with Korean menu inheritance and Korean font fallback.
- Added deterministic generation and internal validation for seven short-named ZIP files containing both locale payloads.
- Removed installer-based distribution and unrelated-version payloads from archives.

## Verification State

- The full solution builds with zero warnings and zero errors.
- Both locale payloads for all seven release labels match their exact English key set, order, and string types; the shared 4.1.2–4.1.3 English, KR, and KR-EN payloads are equivalent.
- All seven ZIP files pass root-layout, safe-path, forbidden-file, manifest-version, both-locale-hash, and target-DLL-hash checks.
- The release build executes separate exact 4.1.0 and bounded 4.1.2+ server/client compatibility matrices and cannot reuse skipped stale binaries.
- SPT 4.1.3 loads server mod 2.1.0 and reports 31,550 entries for each mode. `/client/languages`, `/client/locale/kr-en`, and `/client/menu/locale/kr-en` return the registered language, bilingual global locale, and inherited Korean menu.
- All 4,738 keys that differ between the generated 4.1.3 KR and KR-EN payloads return their matching mode-specific values from the running server.
- Runtime checks completed for the previously supported packages through SPT 4.1.2; the unified 4.1.3 package now passes server startup and endpoint checks, while its in-game visual check remains open.
- Every SPT API reference used by the 4.1 server DLL resolves against both the backed-up 4.1.2 assemblies and the installed 4.1.3 assemblies (15 checked, zero failures).
- A deliberately mismatched SPT 3.x package stayed inactive, logged the expected/actual versions, and allowed the server to keep running.
- The shared client DLL's existing six patch targets remain compatible with SPT 4.1.3, and the new font patch resolves `EFT.LocalizationManager.UpdateFonts(string)` in the installed client assembly.

## Open Work

- Later stable 4.1.x patches may satisfy the bounded binary rule but require explicit release-label, server API, translation, patch-target, and runtime review before distribution. Prereleases and SPT 4.2 are rejected.
- The newly generated exact 4.1.0 artifacts still need release runtime server-log and in-game UI/texture smoke tests when launching that version is available.
- Release smoke testing should visually confirm the adjacent `한국어 (Korean)` and `한국어 (한영 병기)` options, immediate two-way switching, persisted selection, and adjusted UI inside a launcher-started game; the automated client check proves the 4.1.3 font target exists but does not render game screens.
