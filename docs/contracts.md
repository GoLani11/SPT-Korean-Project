# Contracts

## Stable Identities

The server mod GUID remains `com.golani.makina.korean`. The BepInEx identity remains `com.GoLani.koreanpatchfix` / `Korean Patch Fix`. Both components use package version `2.0.1`.

## Package Names

```text
SPT-KR-<version-or-range>.zip
SPT-KR-EN-<version-or-range>.zip
```

Exactly 14 archives are produced for five legacy exact versions, exact SPT 4.1.0, and the shared SPT 4.1.2–4.1.3 range. `KR` copies `kr.generated.json`; `KR-EN` copies `kr-en.generated.json`. The `KR` source still preserves reference-formatted quest titles, objectives, exceptional quest headers, item-description English headers, and verified raid-exfil names.

## Archive Layouts

```text
SPT 3.x:    BepInEx/plugins + user/mods/spt_korean_localization_G&M
SPT 4.0.13: BepInEx/plugins + SPT/user/mods/SPT_Korean_Localization
SPT 4.1.0 and 4.1.2–4.1.3: BepInEx/plugins + SPT_Runtime/user/mods/SPT_Korean_Localization
```

Each archive contains exactly one `locale/kr.json`. The client payload is always `BepInEx/plugins/GoLani.KoreanModFix.dll`. A 3.x `package.json` contains only the exact loader field for that release: `akiVersion` for 3.8.3 and `sptVersion` for 3.9.8–3.11.4.

Archive entries must be relative, remain under the two expected root folders, and contain no `.bat`, `.cmd`, or `.exe` file. The packaged locale, client DLL, server DLL, and dependency manifest must match the selected build sources by SHA-256.
