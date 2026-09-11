# Contracts

## Stable Identities

The BepInEx identity remains `com.GoLani.koreanpatchfix` / `Korean Patch Fix`, with package version `2.1.0`. The historical server mod GUID is `com.golani.korean`; no server module is included in the unified release.

## Package Names

```text
SPT-KR-2.1.0.zip
```

`tools/package_release.py` produces one plugin-only archive for all nine profiles in `tools/client-locale-prototype.json`. Eight translation datasets are included; SPT 4.1.5 uses the 4.1.3 dataset. The manifest records the translation version, EFT version, installed English path and payload hashes for each profile.

For each dataset, the archive copies the translation repository's `input/en.json`, `kr.generated.json` and `kr-en.generated.json` to `locales/<translationVersion>/en.json`, `kr.json` and `kr-en.json`. The `KR` source still preserves reference-formatted quest titles, objectives, exceptional quest headers, item-description English headers, and verified raid-exfil names. Locale values and key order must match the selected translation sources exactly.

The public locale IDs are `kr` for the existing Korean display and `kr-en` for full Korean-English display. The native language list displays `kr` as `한국어 (Korean)` and places `kr-en`, displayed as `한국어 (한영 병기)`, immediately after it. The game owns selection persistence and reload behavior.

## Archive Layouts

```text
BepInEx/plugins/
├─ GoLani.KoreanModFix.dll
└─ SPT-Korean/
   ├─ manifest.json
   ├─ locales/<translationVersion>/
   │  ├─ en.json
   │  ├─ kr.json
   │  └─ kr-en.json
   ├─ README-ko.md
   ├─ LICENSE-mod.txt
   ├─ LICENSE-client.txt
   └─ LICENSE-translations.txt
```

The archive contains exactly 30 files for the current profile matrix. It includes no server locale module, server status companion, installer, game executable or personal configuration.

Archive entries must be relative, remain under `BepInEx/plugins`, and contain no `.bat`, `.cmd`, or `.exe` file. Every entry must match the verified staging manifest by SHA-256. The locale manifest also records the client DLL hash and version. The completed ZIP is extracted and its payloads are rechecked by the Windows locale contract.

`tools/package_release_versions.py` retains the historical server-based, version-specific packaging implementation; it is not the 2.1.0 release entry point.
