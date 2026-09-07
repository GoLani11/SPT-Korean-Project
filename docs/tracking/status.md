# Status

## Current State

Date: 2026-09-07

The repository builds exact-version Korean localization packages for SPT 3.8.3, 3.9.8, 3.10.5, 3.11.4, 4.0.13, and 4.1.0. SPT 4.1.2 and 4.1.3 share an explicitly labelled range package. Each release label has one ZIP containing both `kr` and `kr-en`, producing seven ZIP files.

The current source policy accepts stable SPT 4.1.x patches from 4.1.2 onward (`~4.1.2`). A server-only rebuild has passed local SPT 4.1.5 runtime checks, retaining the installed client DLL and translation files. No new release was published: the seven existing ZIP names, release labels, and distribution targets remain unchanged.

## Client-only Prototype

A separate installer-free prototype now targets exact SPT 3.8.3 / EFT 0.14.1.29197 and SPT 4.1.5 / EFT 0.16.9.40743. It installs only a client DLL and verified locale data under `BepInEx/plugins`, and uses the native language reload flow to provide `kr` and `kr-en` without the Korean server mod. The current experiment requires the matching local server English database.

- The prototype DLL and full nine-project solution build with zero warnings and errors.
- Both installed clients pass static inspection of the five locale patch targets, native asynchronous reload sequence, and existing UI entry points.
- Actual Harmony patches pass the Windows .NET Framework native-flow contract with both real locale sets, both cache behaviors, cold/default bilingual startup, repeated switches, preserved mod keys, late dialogue updates, and failure cases (216,531 assertions, mostly full-payload comparisons).
- Nine Python tests, both existing compatibility contracts, and all seven existing release ZIP validations pass.
- `artifacts/client-locale-prototype/SPT-KR-Client-Prototype-3.8.3-4.1.5.zip` and its verification reports are generated separately from published packages. No installed game/server files were replaced during these checks.
- In-game font/UI and server-rendered message verification remain open. The fixture is not an actual game launch, and other supported release versions are not yet prototype profiles.

See [client-only localization prototype](../client-locale-prototype.md) for implementation, installation, and rollback details.

## Completed

- Added SPT 3.x CommonJS, SPT 4.0.13 net9, exact SPT 4.1.0 net10, and shared SPT 4.1.2–4.1.3 net10 server implementations.
- Replaced compile-time EFT/SPT client dependencies with runtime capability detection.
- Preserved the existing server GUID, client plugin GUID, and historical 3.x mod folder.
- Made `spt-korean-translate` generated outputs the only release locale source.
- Added `kr-en` to the native language list with Korean menu inheritance and Korean font fallback.
- Added deterministic generation and internal validation for seven short-named ZIP files containing both locale payloads.
- Removed installer-based distribution and unrelated-version payloads from archives.

## Verification State

- Local SPT 4.1.5 verification on 2026-09-07 loaded Korean localization 2.1.0, WTT-ServerCommonLib 3.0.6, and Fika Server 2.4.0, with zero startup errors. Both `/client/locale/kr` and `/client/locale/kr-en` served 31,844 entries; all 31,550 existing translation entries per mode matched after accounting for case-insensitive key aliases and existing language-name overrides. In-game visual validation was not performed.
- The server-only Release build completed with zero warnings and errors. Seven Python tests and both compatibility contract configurations passed (26 cases per configuration). Comparing the installed and rebuilt DLLs confirmed unchanged locale-loading methods and lambdas; only compatibility metadata and policy changed. The dependency manifest stayed identical.
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
