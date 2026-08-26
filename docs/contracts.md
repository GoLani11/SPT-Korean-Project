# Contracts

## Stable Identities

The server mod GUID is `com.golani.korean`. The BepInEx identity remains `com.GoLani.koreanpatchfix` / `Korean Patch Fix`. Both components use package version `2.1.0`.

## Package Names

```text
SPT-KR-<version-or-range>.zip
```

Exactly seven archives are produced for five legacy exact versions, exact SPT 4.1.0, and the shared SPT 4.1.2–4.1.3 range. Every archive copies `kr.generated.json` to `locale/kr.json` and `kr-en.generated.json` to `locale/kr-en.json`. The `KR` source still preserves reference-formatted quest titles, objectives, exceptional quest headers, item-description English headers, and verified raid-exfil names.

The public locale IDs are `kr` for the existing Korean display and `kr-en` for full Korean-English display. The native language list displays `kr` as `한국어 (Korean)` and places `kr-en`, displayed as `한국어 (한영 병기)`, immediately after it. The game owns selection persistence and reload behavior.

## Archive Layouts

```text
SPT 3.x:    BepInEx/plugins + user/mods/spt_korean_localization_G&M
SPT 4.0.13: BepInEx/plugins + SPT/user/mods/SPT_Korean_Localization
SPT 4.1.0 and 4.1.2–4.1.3: BepInEx/plugins + SPT_Runtime/user/mods/SPT_Korean_Localization
```

Each archive contains exactly `locale/kr.json` and `locale/kr-en.json`. The client payload is always `BepInEx/plugins/GoLani.KoreanModFix.dll`. A 3.x `package.json` contains only the exact loader field for that release: `akiVersion` for 3.8.3 and `sptVersion` for 3.9.8–3.11.4.

Archive entries must be relative, remain under the two expected root folders, and contain no `.bat`, `.cmd`, or `.exe` file. Both packaged locales, the client DLL, server DLL, and dependency manifest must match the selected build sources by SHA-256.
