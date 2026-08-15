# Contracts

## Stable Identities

The server mod GUID remains `com.golani.makina.korean`. The BepInEx identity remains `com.GoLani.koreanpatchfix` / `Korean Patch Fix`. Both components use package version `2.0.0`.

## Package Names

```text
SPT_Korean_Localization.SPT-<version>.KR.GM.zip
SPT_Korean_Localization.SPT-<version>.KR-EN.GM.zip
```

Exactly 12 archives are produced for the six supported versions. `KR` copies `kr.generated.json`; `KR-EN` copies `kr-en.generated.json`. The `KR` source still preserves reference-formatted quest titles, objectives, exceptional quest headers, item-description English headers, and verified raid-exfil names.

## Archive Layouts

```text
SPT 3.x:    BepInEx/plugins + user/mods/spt_korean_localization_G&M
SPT 4.0.13: BepInEx/plugins + SPT/user/mods/SPT_Korean_Localization
SPT 4.1.0:  BepInEx/plugins + SPT_Runtime/user/mods/SPT_Korean_Localization
```

Each archive contains exactly one `locale/kr.json`. The client payload is always `BepInEx/plugins/GoLani.KoreanModFix.dll`. A 3.x `package.json` contains only the exact loader field for that release: `akiVersion` for 3.8.3 and `sptVersion` for 3.9.8–3.11.4.

Archive entries must be relative, remain under the two expected root folders, and contain no `.bat`, `.cmd`, or `.exe` file.
