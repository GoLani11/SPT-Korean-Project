# Client-side localization — 2.1.0

The former prototype is the implementation shipped in the unified 2.1.0 release. The historical source/document filenames are retained. See the [installation guide](releases/2.1.0-install.md) for migration and rollback. These are the explicit source profiles:

| SPT | EFT executable file version | Translation source |
| --- | --- | --- |
| 3.8.3 | 0.14.1.29197 | 3.8.3 |
| 3.9.8 | 0.14.9.30626 | 3.9.8 |
| 3.10.5 | 0.15.5.33420 | 3.10.5 |
| 3.11.4 | 0.16.1.35392 | 3.11.4 |
| 4.0.13 | 0.16.9.40087 | 4.0.13 |
| 4.1.0 | 0.16.9.40743 | 4.1.0 |
| 4.1.2 | 0.16.9.40743 | 4.1.2 |
| 4.1.3 | 0.16.9.40743 | 4.1.3 |
| 4.1.5 | 0.16.9.40743 | 4.1.3, after matching the installed English locale |

An unlisted stable SPT 4.1.x patch at or above 4.1.2 may reuse the 4.1.5 profile only after the checks below pass. Other unlisted SPT families and prereleases are rejected. This release requires the matching local server database to verify the English source; a client installation without that database is not supported yet.

## Package

```text
stage/ (release ZIP: SPT-KR-2.1.0.zip)
└─ BepInEx/plugins/
   ├─ GoLani.KoreanModFix.dll
   └─ SPT-Korean/
      ├─ manifest.json
      └─ locales/
         ├─ 3.8.3/{en,kr,kr-en}.json
         ├─ ...other translation versions...
         └─ 4.1.3/{en,kr,kr-en}.json
```

The archive contains no server mod, installer, command script, game assembly, or replacement game database. The plugin keeps its existing GUID and filename and uses release version 2.1.0. The generated English and translation payloads remain owned by `spt-korean-translate`.

## Runtime behavior

The plugin selects an exact profile first, otherwise the shared policy permits the 4.1.5 fallback only for stable 4.1.2+ patches below 4.2.0. An incompatible exact profile never falls back to another profile. It then verifies the EFT executable build, three payload hashes, locale key order, and the installed English key/value pairs before enabling localization. English JSON formatting does not affect this comparison.

EFT detection uses the PE's four fixed numeric version fields. Unity Mono truncates the formatted `FileVersion` text in the inspected clients (`40743` becomes `4074`, and `29197` becomes `2919`); accepting that text would incorrectly disable the mod. The exact build check remains enforced.

Fallback selection alone does not enable translation: the EFT build and the complete installed English key/value set must match, and all five native hook targets must resolve. Missing targets disable localization before hooks are installed. A new patch with changed source text needs an updated translation; 4.2, 4.1.1, prereleases, and malformed versions remain blocked.

The client discovers the localization manager by the `UpdateLocales(string, Dictionary<string,string>)` contract, including the obfuscated manager types in older clients. Five native methods are patched:

- `Init` inserts `kr-en` immediately after `kr` in the native language list.
- `ReloadBackendLocale` requests the existing `kr` backend locale when the user selected `kr-en`, including a saved/default selection. The selected culture and native settings persistence remain `kr-en`.
- `UpdateMainMenuLocales` copies the Korean menu into the bilingual locale.
- `UpdateLocales` overlays the selected generated payload and mirrors subsequent Korean dialogue/mod fragments into the bilingual locale. Unknown mod keys survive and cached input dictionaries are not mutated.
- The font update method uses Korean fallback fonts for `kr-en`, without changing the selected culture.

Both locales exist before the native reload event refreshes the screen. Mirroring is limited to updates whose locale ID is `kr`, so it cannot recursively mirror itself. Case aliases are folded in source order before constructing EFT's case-insensitive locale dictionary. Existing UI correction source is shared with the released client plugin; its standalone prototype resolves the current culture through the native localization manager.

The server's HTTP language list and locale endpoints remain unchanged. This approach translates client locale lookups; it does not establish parity for text that a server or another mod already rendered into a literal message.

## Build and automated verification

Use the .NET 10 SDK and a Windows host with .NET Framework 4.8. WSL is supported through Windows interop. The sibling translation repository and a 3.8.3 reference client are required. Game installations are discovered as `D:/SPT_<version>` and `D:/SPT` for 4.1.5; `--installations-root` changes the common parent.

```powershell
python .\tools\package_release.py --spt-383-root D:\SPT_3.8.3 --spt-415-root D:\SPT
```

Use `--dotnet` to select an SDK executable and `--translation-root` to select the translation checkout. Add `--no-archive` to stage and verify the files for direct copying without creating a ZIP. The command rebuilds the plugin and contract executable, verifies the real client metadata and payloads, and executes the actual Harmony patches against a native-flow fixture in isolated processes: Windows .NET Framework and every complete installation's bundled Unity Mono runtime. Missing clients are explicitly reported as payload/fixture-only coverage; they are never marked as installed-client probes. It then verifies staged files and, unless disabled, creates and verifies the common client ZIP. Release mode also extracts the archive and runs the Windows locale contract on its actual contents. No server companion is built.

Output is under `artifacts/release-2.1.0/`, including `verification.json` and `contract-verification.json`. Generated files are not committed. The contract's JSON dependency is an official signed package because the game's modified JSON DLL is accepted by Unity Mono but fails Windows CLR strong-name validation. The shipped plugin still references the game's existing JSON assembly and does not bundle another copy.

The contract covers cold bilingual startup before/after session creation, null/default language selection, old cached and current reloading clients, repeated native language switches, every generated translation value, case aliases, later dialogue/mod fragments, preservation of backend response objects, async failures, unsupported versions, wrong source/build, corrupt payloads, and duplicate JSON keys. Client probes verify the five locale entry points, native asynchronous reload call sequence, and existing UI patch entry points in the actual game assemblies.

The Mono checks host only the managed test executable in the game's runtime; they do not start the game or Unity graphics, render fonts, prove all server-produced messages are translated, or establish compatibility with every other mod. The user confirmed 3.8.3 works in game. Five installed clients (3.8.3, 3.9.8, 3.10.5, 3.11.4, 4.1.5) pass metadata inspection and Unity Mono fixture tests. Local 4.0.13 has only SPT files, without the EFT executable/managed assemblies/runtime. Local 4.1.0/4.1.2/4.1.3 clients are unavailable. These profiles are prepared for testing, not claimed as visually verified.

## Local game test

Follow the [installation guide](releases/2.1.0-install.md), including removal of old server modules. Check both language choices, immediate switching in both directions, persisted selection after restart, quest/item text, trader dialogue, Korean glyphs and existing UI corrections. The Insomnia objective should show 22:00–05:00 on 3.8.3 and 21:00–06:00 on 4.1.5. Check server-generated messages separately.

The game log records `[고라니 SPT 한글화 v2.1.0 | SPT ...] 번역 데이터 로드 및 언어 패치 적용 완료!`. This reports locale initialization; individual UI patch results appear separately. There is no server console notification in this release.

## Automatic patch-upgrade compatibility

The client uses `ClientLocaleProfilePolicy` for version selection. Historical status projects also reference this policy, but are not shipped. The nine listed profiles remain explicit sources; an unlisted stable patch such as 4.1.6 does not need a new manifest entry when its game build and English source are unchanged. Neither the installed SPT version nor its database is rewritten.

After file checks and native hook installation, the client logs the actual SPT version, translation source, and reused profile:

```text
SPT 4.1.6 | 번역 기준 4.1.3: 호환성 검사 통과 — 기존 번역 사용 (프로필 4.1.5).
```

Contracts simulate 4.1.6 against the installed 4.1.5 game/data and exercise both native language reload behaviors. They also cover later patches, boundary/prerelease rejection, changed EFT/English, corrupt files, exact-profile precedence, a missing fallback, and missing native hook targets. This is a simulated upgrade, not a claim that an actual 4.1.6 game installation was tested.
