# Decision Index

## Accepted

- `0002-dual-runtime-package-layout`: Keep server locale and client UI fixes in one repository while building separate runtime outputs.
- `0003-version-specific-release-layout`: Publish exact-version ZIP files with only the server path used by that SPT release.
- `0004-bounded-4.1-patch-compatibility`: Keep the shared SPT 4.1.2–4.1.3 binaries inside a bounded 4.1 compatibility range.
- `0005-short-release-names-and-4.1-split`: Use short ZIP names, restore an exact 4.1.0 pair, and label the shared 4.1.2–4.1.3 pair explicitly.
- `0006-native-locale-mode-switch`: Combine both locale payloads in each version package and expose them through the native language setting.

## Candidate Topics

- Separating translation refresh work from compatibility maintenance.
- Adding newly verified SPT versions as separate release assets.
