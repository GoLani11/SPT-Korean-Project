# Business Rules

## Supported Releases

These rules describe published releases. The separately built [client-only prototype](client-locale-prototype.md) experiments with one installer-free client bundle across nine exact test profiles; it is not a new release compatibility promise.

The exact supported SPT versions are 3.8.3, 3.9.8, 3.10.5, 3.11.4, 4.0.13, and 4.1.0. SPT 4.1.2 and 4.1.3 share one labelled archive whose server metadata uses the bounded `>=4.1.2 <=4.1.3` range. SPT 4.1.1, prereleases, 4.2, and unlabelled later patches are not release targets.

Each release label has one ZIP containing both the Korean and Korean-English locale payloads. A universal cross-version ZIP is intentionally not published because it would leave unused server-mod folders in the installation.

## Translation Ownership

Release locale files come only from the generated outputs in the sibling `spt-korean-translate` repository. This repository must not keep or hand-edit duplicate locale snapshots.

The SPT 4.1.0 archive uses its own generated output and exact-gated server and client binaries. The shared 4.1.2–4.1.3 archive uses the 4.1.3 generated locale output only when the release builder confirms that its English, KR, and KR-EN values and order match 4.1.2. Its server binary is built against the lowest admitted 4.1.2 API.

## Installation Promise

Users select the ZIP matching their exact SPT version; only SPT 4.1.2 and 4.1.3 share the explicitly labelled `4.1.2-4.1.3` archive. Archives have no wrapper directory, installer, script, executable, or payload for another SPT layout family.

The server mod overlays the built-in `kr` locale and registers `kr-en` in the native language list at runtime. Users switch between them through the existing interface-language setting without restarting. The selected client plugin supplies the Korean font fallback for both locale IDs, adjusts UI presentation, and safely skips features missing from an older client.
