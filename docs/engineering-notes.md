# Engineering Notes

## Server Compatibility

SPT 3.x uses the historical CommonJS `postDBLoad` contract and exact loader metadata. SPT 4.0.13 and SPT 4.1 use different DI and locale APIs, so they intentionally have separate source projects and target frameworks. The exact SPT 4.1.0 server target compiles against 4.1.0 packages and declares `4.1.0`. The shared SPT 4.1 target compiles against 4.1.2 packages and now declares `~4.1.2`, accepting stable 4.1.x patches from 4.1.2 onward while rejecting prereleases and 4.2.

This source compatibility policy was verified locally on SPT 4.1.5 with a rebuilt server DLL. The published shared ZIP remains labelled 4.1.2–4.1.3; the source change does not rename, replace, or expand the published release assets.

## Client Compatibility

All supported clients use Harmony 2.9 and BepInEx 5.4.22 or 5.4.23. Both client builds compile against the shared API surface and have soft ordering hints for `com.spt-aki.core` and `com.SPT.core`. Their patch implementation is shared, while the current source compatibility gates keep exact 4.1.0 separate from stable 4.1.x patches starting at 4.1.2. The local 4.1.5 update retains the installed client DLL, which already uses this shared compatibility policy.

The following target differences are handled at runtime:

- Flea-market item names use `SetItemName` on 4.1 and `Show` on older clients.
- Flea-market subcategories use `SetExpandedStatus` on 4.1 and `Show` on older clients.
- Prestige rewards do not exist on 3.8.3–3.10.5 and are skipped normally.
- Quick-access and window `Show` signatures drift, so targets are selected by capability rather than EFT parameter types.
- Short-name layout hooks the method that writes `GridItemView.Caption.text` (`method_26` in 3.9.8; `UpdateItemName` in 4.1.5), plus declared `InfoWindow.Show` and `GridWindow.Show`. The former one-shot `UiPools.Init` scan could miss late-created views and could target the wrong child transform. Exact caption references now receive the existing margins/autosizing settings whenever refreshed; weakly held original styles are restored when switching out of Korean. No asynchronous global resource scan is used.
- `ShortNameContract` runs the actual Harmony hooks against UI lifecycle stand-ins, covering late creation, reuse, unrelated labels, both Korean modes, native return values and English restoration. Installed-client metadata probes verify the actual caption writer; these tests do not establish visual rendering quality.

A clean build confirms binary compatibility only. Actual layout still requires in-game inspection because private UI fields and prefab hierarchies can change.

## Locale Integrity

The release builder rejects duplicate JSON keys, non-string values, missing or extra keys, and key-order drift. The packaged locale must also match the generated source file's SHA-256 hash.
