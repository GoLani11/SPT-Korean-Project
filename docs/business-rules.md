# Business Rules

## Supported Releases

The exact supported SPT versions are 3.8.3, 3.9.8, 3.10.5, 3.11.4, 4.0.13, and 4.1.2. Compatibility ranges and unverified future-version fallback are not release promises.

Each version has a Korean-only and a Korean-English ZIP. A universal ZIP is intentionally not published because it would leave unused server-mod folders in the installation.

## Translation Ownership

Release locale files come only from the generated outputs in the sibling `spt-korean-translate` repository. This repository must not keep or hand-edit duplicate locale snapshots.

SPT 4.1.2 reuses the generated 4.1.0 locale output because the 4.1.2 English and official Korean global locale files are byte-identical to 4.1.0. The server and client binaries still target 4.1.2 exactly.

## Installation Promise

Users select the ZIP matching their exact SPT version and extract it once at the SPT install root. Archives have no wrapper directory, installer, script, executable, or payload for another SPT version.

The server mod overlays the built-in Korean locale at runtime. The universal client plugin adjusts only UI presentation and safely skips features missing from an older client.
