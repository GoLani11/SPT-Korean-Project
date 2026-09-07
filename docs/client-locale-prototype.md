# Client-only localization prototype

This experiment installs one client plugin and its locale data at the SPT game root, with no installer. It tests whether Korean localization can move out of the server mod while preserving the game's native Korean / Korean-English language selection.

The prototype is a separate build, not a replacement for the seven published 2.1.0 packages. It admits only these profiles:

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

Other SPT/EFT combinations are rejected. This first experiment requires the matching local server database to verify the English source; a client installation without that database is not supported yet.

## Package

```text
stage/ (optional ZIP: SPT-KR-Client-Prototype.zip)
└─ BepInEx/plugins/
   ├─ GoLani.KoreanModFix.dll
   └─ SPT-Korean/
      ├─ manifest.json
      └─ locales/
         ├─ 3.8.3/{en,kr,kr-en}.json
         ├─ ...other translation versions...
         └─ 4.1.3/{en,kr,kr-en}.json
```

The archive contains no server mod, installer, command script, game assembly, or replacement game database. The plugin keeps its existing GUID and filename and uses version 2.2.0 for the experiment. The generated English and translation payloads remain owned by `spt-korean-translate`.

## Runtime behavior

The plugin verifies the exact SPT profile, EFT executable build, three payload hashes, locale key order, and the installed English key/value pairs before enabling localization. English JSON formatting does not affect this comparison.

EFT detection uses the PE's four fixed numeric version fields. Unity Mono truncates the formatted `FileVersion` text in the inspected clients (`40743` becomes `4074`, and `29197` becomes `2919`); accepting that text would incorrectly disable the mod. The exact build check remains enforced.

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
python .\tools\package_client_locale_prototype.py --no-archive --spt-383-root D:\SPT_3.8.3 --spt-415-root D:\SPT
```

Use `--dotnet` to select an SDK executable and `--translation-root` to select the translation checkout. Add `--no-archive` to stage and verify the files for direct copying without creating a ZIP. The command rebuilds the plugin and contract executable, verifies the real client metadata and payloads, and executes the actual Harmony patches against a native-flow fixture in isolated processes: Windows .NET Framework and every complete installation's bundled Unity Mono runtime. Missing clients are explicitly reported as payload/fixture-only coverage; they are never marked as installed-client probes. It then verifies staged files, builds the optional console companions, and, unless disabled, creates and verifies the common client ZIP.

Output is under `artifacts/client-locale-prototype/`, including `verification.json` and `contract-verification.json`. Generated files are not committed. The contract's JSON dependency is an official signed package because the game's modified JSON DLL is accepted by Unity Mono but fails Windows CLR strong-name validation. The shipped plugin still references the game's existing JSON assembly and does not bundle another copy.

The contract covers cold bilingual startup before/after session creation, null/default language selection, old cached and current reloading clients, repeated native language switches, every generated translation value, case aliases, later dialogue/mod fragments, preservation of backend response objects, async failures, unsupported versions, wrong source/build, corrupt payloads, and duplicate JSON keys. Client probes verify the five locale entry points, native asynchronous reload call sequence, and existing UI patch entry points in the actual game assemblies.

The Mono checks host only the managed test executable in the game's runtime; they do not start the game or Unity graphics, render fonts, prove all server-produced messages are translated, or establish compatibility with every other mod. The user confirmed 3.8.3 works in game. Five installed clients (3.8.3, 3.9.8, 3.10.5, 3.11.4, 4.1.5) pass metadata inspection and Unity Mono fixture tests. Local 4.0.13 has only SPT files, without the EFT executable/managed assemblies/runtime. Local 4.1.0/4.1.2/4.1.3 clients are unavailable. These profiles are prepared for testing, not claimed as visually verified.

## Local game test

1. Close the game and server. Back up `BepInEx/plugins/GoLani.KoreanModFix.dll` outside `BepInEx/plugins`; BepInEx scans plugin subdirectories too.
2. To test server independence, move the previous Korean server mod outside every `user/mods` directory. Its paths are `user/mods/spt_korean_localization_G&M` on 3.8.3 and `SPT_Runtime/user/mods/SPT_Korean_Localization` on 4.1.5. Preserve the folders for rollback.
3. Copy the contents of `artifacts/client-locale-prototype/stage` into the game root. For startup logs, also copy the contents of `server-status/<exact SPT version>` into that same root. Launch normally through SPT's launcher.
4. Confirm the log contains `Client-only localization prototype`, the detected profile, and no failed locale patch. Check both adjacent language choices, switch both ways without restarting, then restart with the bilingual selection saved.
5. Check quest titles, descriptions and objectives, item text, trader dialogue, Korean glyphs, and the existing flea-market/gesture/quick-access UI corrections. The Insomnia objective must show 22:00–05:00 on 3.8.3 and 21:00–06:00 on 4.1.5. Check server-generated messages separately.

To roll back, select ordinary Korean while the prototype is still active, close the game/server, restore the previous client DLL and server mod, and remove the prototype's `BepInEx/plugins/SPT-Korean` data folder and optional `GoLani.KoreanLocalization.Status` server mod folder. A saved `kr-en` selection needs either this prototype or the existing server-based bilingual package.

## Optional server startup log

Translation remains entirely in the common client. A small, optional status mod restores the server console notification without modifying server locales. Its files are staged separately in `server-status/<SPT version>/`: CommonJS for 3.x, net9 for 4.0.13, and separate net10 builds for 4.1.0 and 4.1.2+. Do not mix these server binaries. The common client ZIP still contains only `BepInEx/plugins` files.

At server startup it verifies the selected profile, client DLL and payload hashes, game build, locale keys, and installed English values, then logs:

```text
[고라니 SPT 한글화 v2.2.0 | SPT 4.1.5] 한글화 파일 검증 및 적용 준비 완료! (번역 기준 4.1.3, 한글판 31,550 / 한영 병기판 31,550개)
번역은 게임 실행 시 클라이언트에 적용됩니다. 재밌는 SPT 되세요!
```

This means files are ready, not that a game client has already applied them. Missing/corrupt files and mismatched builds produce an error instead of success. Removing this notifier does not disable client localization.

Additional checks (run the .NET contract on Windows, since Linux FileVersionInfo does not read these native PE versions):

```powershell
node tests/server_locale_status_contract.js artifacts/client-locale-prototype/stage D:/
dotnet run --project tests/ServerLocaleStatusContract -c Release -- artifacts/client-locale-prototype/stage D:/SPT
```

The Node contract invokes the real `postDBLoad` export and logger fallback for all four installed 3.x versions. Both contracts exercise corrupt DLL/payload, mismatched English, and missing-client failures; the .NET contract also checks unsupported SPT and wrong EFT builds. The contracts do not launch servers or change live databases. Separate local smoke tests confirmed the actual startup log on 3.8.3, 3.9.8, 3.10.5, 3.11.4, and 4.1.5; each test server was stopped afterwards. Node servers require console stdout, so these smoke tests read their own log files rather than redirecting stdout.
