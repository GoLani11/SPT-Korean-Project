# Engineering Notes

## Server Compatibility

SPT 3.x uses the historical CommonJS `postDBLoad` contract and exact loader metadata. SPT 4.0.13 and SPT 4.1 use different DI and locale APIs, so they intentionally have separate source projects and target frameworks. The exact SPT 4.1.0 server target compiles against 4.1.0 packages and declares `4.1.0`. The shared SPT 4.1.2–4.1.3 target compiles against 4.1.2 packages and declares `>=4.1.2 <=4.1.3`, limiting it to the two verified patches.

## Client Compatibility

All supported clients use Harmony 2.9 and BepInEx 5.4.22 or 5.4.23. Both client builds compile against the shared API surface and have soft ordering hints for `com.spt-aki.core` and `com.SPT.core`. Their patch implementation is shared, while compile-time compatibility gates keep 4.1.0 separate from 4.1.2–4.1.3.

The following target differences are handled at runtime:

- Flea-market item names use `SetItemName` on 4.1 and `Show` on older clients.
- Flea-market subcategories use `SetExpandedStatus` on 4.1 and `Show` on older clients.
- Prestige rewards do not exist on 3.8.3–3.10.5 and are skipped normally.
- Quick-access `Show` signatures and `UiPools.Init` overloads drift, so targets are selected by capability rather than EFT parameter types.

A clean build confirms binary compatibility only. Actual layout still requires in-game inspection because private UI fields and prefab hierarchies can change.

## Locale Integrity

The release builder rejects duplicate JSON keys, non-string values, missing or extra keys, and key-order drift. The packaged locale must also match the generated source file's SHA-256 hash.
