# Business Rules

## Supported Releases

The exact supported SPT versions are 3.8.3, 3.9.8, 3.10.5, 3.11.4, and 4.0.13. The current 4.1 archive admits stable SPT releases from 4.1.2 up to, but not including, 4.2 through the bounded `~4.1.2` range. SPT 4.1.2 and 4.1.3 are currently verified; a later 4.1.x patch is not described as verified until its server API, client patch targets, and locale have been checked.

Each version has a Korean-only and a Korean-English ZIP. A universal ZIP is intentionally not published because it would leave unused server-mod folders in the installation.

## Translation Ownership

Release locale files come only from the generated outputs in the sibling `spt-korean-translate` repository. This repository must not keep or hand-edit duplicate locale snapshots.

The current 4.1 archive uses the SPT 4.1.3 generated locale output. Its English and official Korean global locale files are byte-identical to 4.1.2. The server binary is built against the lowest admitted 4.1.2 API and both binaries accept only stable 4.1.x patch versions starting at 4.1.2.

## Installation Promise

Users select the ZIP matching their exact SPT version, except that admitted SPT 4.1.x installs share the latest 4.1 archive. Users of a newly released 4.1.x patch must check the verification status first. Archives have no wrapper directory, installer, script, executable, or payload for another SPT layout family.

The server mod overlays the built-in Korean locale at runtime. The universal client plugin adjusts only UI presentation and safely skips features missing from an older client.
