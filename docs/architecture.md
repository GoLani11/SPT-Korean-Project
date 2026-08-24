# Architecture

## Runtime Components

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

The sibling `spt-korean-translate` repository is the only release locale source. For each release, packaging validates that both generated variants have the exact key set, key order, and string value types of the declared locale source. The shared SPT 4.1.2–4.1.3 archive uses the 4.1.3 output only after the English, KR, and KR-EN JSON values and order have been proven equivalent to 4.1.2.

SPT 3.x applies the selected JSON during `postDBLoad`. SPT 4.0.13 and both 4.1 server builds attach a transformer to the built-in Korean global locale. No package edits SPT's original locale files.

## Release Flow

`tools/package_release_versions.py` builds all binary targets once, stages only the target version's server mod and locale, selects the matching client DLL, and creates 14 deterministic ZIP files. Every archive is reopened and checked for safe paths, exact root folders, source hashes for the locale and both DLL payloads, exact 3.x manifest compatibility, and forbidden installer files.
