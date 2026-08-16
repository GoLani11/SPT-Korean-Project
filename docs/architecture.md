# Architecture

## Runtime Components

The release contains one client plugin and one server locale mod selected for the target SPT version.

```text
src/ClientModFixPlugin -> universal net48 BepInEx plugin
src/ServerLocaleMod3   -> SPT 3.x CommonJS server mod
src/ServerLocaleMod40  -> SPT 4.0.13 net9 server mod
src/ServerLocaleMod    -> SPT 4.1.2 net10 server mod
```

The client project references only the BepInEx, Harmony, Unity, and TextMeshPro assemblies shared by all six supported installs. EFT and SPT client types are resolved by name at runtime. Public method names are preferred, older clients fall back to stable `Show` entry points, and unavailable features such as pre-3.11 prestige rewards are skipped.

## Locale Flow

The sibling `spt-korean-translate` repository is the only release locale source. For each release, packaging validates that both generated variants have the exact key set, key order, and string value types of the declared locale source. SPT 4.1.2 uses the 4.1.0 generated locale because both versions' English and official Korean global locale files are byte-identical.

SPT 3.x applies the selected JSON during `postDBLoad`. SPT 4.0.13 and 4.1.2 attach a transformer to the built-in Korean global locale. No package edits SPT's original locale files.

## Release Flow

`tools/package_release_versions.py` builds the three binary projects once, stages only the target version's server mod and locale, adds the universal client DLL, and creates 12 deterministic ZIP files. Every archive is reopened and checked for safe paths, exact root folders, the locale source hash, exact 3.x manifest compatibility, and forbidden installer files.
