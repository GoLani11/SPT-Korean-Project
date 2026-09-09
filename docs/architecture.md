# Architecture

## Current release: 2.1.0

`src/ClientLocalePrototype` is the production build for the unified client release; its directory name is retained from development. It links the existing UI fixes in `src/ClientModFixPlugin` and adds client locale loading. It emits the net48 `GoLani.KoreanModFix.dll` with the existing plugin GUID and release version 2.1.0.

The only distributed archive is `SPT-KR-2.1.0.zip`. It contains the plugin, a manifest, English/Korean/bilingual payloads and documentation under `BepInEx/plugins`. No server mod or status companion is built or shipped by this release path.

## Locale flow

The sibling `spt-korean-translate` generated outputs are the translation source. Nine explicit profiles select eight payload sets; 4.1.5 reuses 4.1.3 after English-source validation. Exact profiles take precedence. Unlisted stable 4.1.2+ patches below 4.2.0 can reuse the 4.1.5 profile only when the EFT build, installed English key/value set, payload integrity and native hooks match.

The client verifies numeric PE version fields, manifest hashes and locale key order before enabling localization. Native hooks register `kr-en`, reuse the Korean backend/menu locale, overlay the selected payload and apply Korean font fallback. Native language switching and settings persistence remain in use. Backend dictionaries and unknown mod keys are preserved. The server database and HTTP endpoints are not modified.

See [runtime details](client-locale-prototype.md) for the five hooks and validation boundaries. Text already rendered into literal server/mod messages may remain untranslated.

## Release flow

`tools/package_release.py` calls the common builder in release mode. It builds the client and contracts, verifies Windows .NET Framework plus available Unity Mono runtimes, stages both language modes, creates a deterministic archive, validates exact paths and hashes, and reruns the locale contract on extracted ZIP contents. It also writes release notes, upload instructions, SHA-256 and `verification.json` to `artifacts/release-2.1.0`.

The server locale/status projects and `tools/package_release_versions.py` remain historical implementations. They are not dependencies or contents of the current release ZIP. Publication is a separate author action.
