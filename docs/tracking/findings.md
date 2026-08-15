# Findings

## Confirmed Compatibility Facts

- SPT 3.8.3 uses `akiVersion`; SPT 3.9.8–3.11.4 use `sptVersion` in server mod metadata.
- SPT 3.x loads from `user\mods`, SPT 4.0.13 from `SPT\user\mods`, and SPT 4.1.0 from `SPT_Runtime\user\mods`.
- All six clients use Harmony 2.9 and BepInEx 5.4.22 or 5.4.23.
- `OfferItemDescription.SetItemName` and `SubcategoryView.SetExpandedStatus` are 4.1 targets; older clients expose compatible `Show` entry points.
- `PrestigeRewardView` begins with the 3.11 client family.
- Version key counts are 22,561; 23,931; 26,944; 28,659; 31,084; and 31,550 respectively.

## Repaired

- Removed stale 4.1-only locale copies and SPT reflection package coupling.
- Split the incompatible 4.0 and 4.1 server APIs into separate builds.
- Added exact-version manifests and archive structure validation.
- Added safe unsupported-version behavior and per-patch enabled/unavailable/failed client logging.

## Residual Risk

Client patches still depend on game UI object names, private fields, and prefab structure. Static target discovery and a clean build do not replace visual testing in each supported client.
