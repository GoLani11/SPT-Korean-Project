# Client-only localization prototype

This experiment installs one client plugin and its locale data by extracting one ZIP at the SPT game root. It tests whether Korean localization can move out of the server mod while preserving the game's native Korean / Korean-English language selection.

The prototype is a separate build, not a replacement for the seven published 2.1.0 packages. It admits only these profiles:

| SPT | EFT executable file version | Translation source |
| --- | --- | --- |
| 3.8.3 | 0.14.1.29197 | 3.8.3 |
| 4.1.5 | 0.16.9.40743 | 4.1.3, after matching the installed English locale |

Other SPT/EFT combinations are rejected. This first experiment requires the matching local server database to verify the English source; a client installation without that database is not supported yet.

## Package

```text
SPT-KR-Client-Prototype-3.8.3-4.1.5.zip
└─ BepInEx/plugins/
   ├─ GoLani.KoreanModFix.dll
   └─ SPT-Korean/
      ├─ manifest.json
      └─ locales/
         ├─ 3.8.3/{en,kr,kr-en}.json
         └─ 4.1.3/{en,kr,kr-en}.json
```

The archive contains no server mod, installer, command script, game assembly, or replacement game database. The plugin keeps its existing GUID and filename and uses version 2.2.0 for the experiment. The generated English and translation payloads remain owned by `spt-korean-translate`.

## Runtime behavior

The plugin verifies the exact SPT profile, EFT executable build, three payload hashes, locale key order, and the installed English key/value pairs before enabling localization. English JSON formatting does not affect this comparison.

The client discovers the localization manager by the `UpdateLocales(string, Dictionary<string,string>)` contract, including the obfuscated manager types in older clients. Five native methods are patched:

- `Init` inserts `kr-en` immediately after `kr` in the native language list.
- `ReloadBackendLocale` requests the existing `kr` backend locale when the user selected `kr-en`, including a saved/default selection. The selected culture and native settings persistence remain `kr-en`.
- `UpdateMainMenuLocales` copies the Korean menu into the bilingual locale.
- `UpdateLocales` overlays the selected generated payload and mirrors subsequent Korean dialogue/mod fragments into the bilingual locale. Unknown mod keys survive and cached input dictionaries are not mutated.
- The font update method uses Korean fallback fonts for `kr-en`, without changing the selected culture.

Both locales exist before the native reload event refreshes the screen. Mirroring is limited to updates whose locale ID is `kr`, so it cannot recursively mirror itself. Case aliases are folded in source order before constructing EFT's case-insensitive locale dictionary. Existing UI correction source is shared with the released client plugin; its standalone prototype resolves the current culture through the native localization manager.

The server's HTTP language list and locale endpoints remain unchanged. This approach translates client locale lookups; it does not establish parity for text that a server or another mod already rendered into a literal message.

## Build and automated verification

Use a .NET SDK capable of building net48 and a Windows host with .NET Framework 4.8. WSL is supported through Windows interop. Both target installations and the sibling translation repository must be available.

```powershell
python .\tools\package_client_locale_prototype.py --spt-383-root D:\SPT_3.8.3 --spt-415-root D:\SPT
```

Use `--dotnet` to select an SDK executable and `--translation-root` to select the translation checkout. The command rebuilds the plugin and contract executable, verifies the real client metadata and payloads, executes the actual Harmony patches against a native-flow fixture on Windows .NET Framework, and only then creates and verifies the ZIP.

Output is under `artifacts/client-locale-prototype/`, including `verification.json` and `contract-verification.json`. Generated files are not committed. The contract's JSON dependency is an official signed package because the game's modified JSON DLL is accepted by Unity Mono but fails Windows CLR strong-name validation. The shipped plugin still references the game's existing JSON assembly and does not bundle another copy.

The contract covers cold bilingual startup before/after session creation, null/default language selection, old cached and current reloading clients, repeated native language switches, every generated translation value, case aliases, later dialogue/mod fragments, preservation of backend response objects, async failures, unsupported versions, wrong source/build, corrupt payloads, and duplicate JSON keys. Client probes verify the five locale entry points, native asynchronous reload call sequence, and existing UI patch entry points in the actual game assemblies.

These checks do not start Unity, render fonts, prove all server-produced messages are translated, or establish compatibility with every other mod. In-game verification is required before expanding the profile list or replacing a published package.

## Local game test

1. Close the game and server. Back up `BepInEx/plugins/GoLani.KoreanModFix.dll` outside `BepInEx/plugins`; BepInEx scans plugin subdirectories too.
2. To test server independence, move the previous Korean server mod outside every `user/mods` directory. Its paths are `user/mods/spt_korean_localization_G&M` on 3.8.3 and `SPT_Runtime/user/mods/SPT_Korean_Localization` on 4.1.5. Preserve the folders for rollback.
3. Extract the prototype ZIP at the game root and launch normally through SPT's launcher.
4. Confirm the log contains `Client-only localization prototype`, the detected profile, and no failed locale patch. Check both adjacent language choices, switch both ways without restarting, then restart with the bilingual selection saved.
5. Check quest titles, descriptions and objectives, item text, trader dialogue, Korean glyphs, and the existing flea-market/gesture/quick-access UI corrections. The Insomnia objective must show 22:00–05:00 on 3.8.3 and 21:00–06:00 on 4.1.5. Check server-generated messages separately.

To roll back, select ordinary Korean while the prototype is still active, close the game/server, restore the previous client DLL and server mod, and remove only the prototype's `BepInEx/plugins/SPT-Korean` data folder. A saved `kr-en` selection needs either this prototype or the existing server-based bilingual package.
