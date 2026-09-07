# Architecture

## Runtime Components

An experimental `src/ClientLocalePrototype` build shares the existing UI patch source and adds client-side locale loading for nine exact test profiles. Its separate ZIP contains only `BepInEx/plugins` files. It does not change the release matrix below. See [client-only localization prototype](client-locale-prototype.md) for its native reload hooks and verification limits.

The release contains one client plugin and one server locale mod selected for the target SPT version.

```text
src/ClientModFixPlugin -> universal net48 BepInEx plugin
src/ClientModFixPlugin410 -> SPT 4.1.0-gated net48 BepInEx plugin
src/ServerLocaleMod3   -> SPT 3.x CommonJS server mod
src/ServerLocaleMod40  -> SPT 4.0.13 net9 server mod
src/ServerLocaleMod410 -> exact SPT 4.1.0 net10 server mod
src/ServerLocaleMod    -> shared SPT 4.1.2–4.1.3 net10 server mod
```

The client projects reference only the BepInEx, Harmony, Unity, and TextMeshPro assemblies shared by the supported installs. EFT and SPT client types are resolved by name at runtime. The 4.1.0 build has an exact compatibility gate; the other build admits the exact legacy versions and stable 4.1 patches starting at 4.1.2. Public method names are preferred, older clients fall back to stable `Show` entry points, and unavailable features such as pre-3.11 prestige rewards are skipped.

## Locale Flow

The sibling `spt-korean-translate` repository is the only release locale source. For each release, packaging validates that both generated payloads have the exact key set, key order, and string value types of the declared locale source. The shared SPT 4.1.2–4.1.3 archive uses the 4.1.3 output only after the English, KR, and KR-EN JSON values and order have been proven equivalent to 4.1.2.

Every server mod overlays `kr.json` onto the built-in `kr` locale and registers `kr-en` as a second global locale built from the same Korean base plus `kr-en.json`. The new locale inherits the built-in Korean menu locale and is added to the native language list. The client plugin maps `kr-en` to the Korean font fallback while leaving the selected culture unchanged, so the game's normal language reload and persistence flow handles switching. No package edits SPT's original locale files.

## Release Flow

`tools/package_release_versions.py` builds all binary targets once, stages only the target version's server mod with both locale payloads, selects the matching client DLL, and creates seven deterministic ZIP files. Every archive is reopened and checked for safe paths, exact root folders, source hashes for both locales and both DLL payloads, exact 3.x manifest compatibility, and forbidden installer files.
