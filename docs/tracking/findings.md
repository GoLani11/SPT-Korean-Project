# Findings

## Confirmed

- The repository now contains two runtime components, not one.
- `SPT_Korean_Localization.dll` is a C# SPT server mod.
- `GoLani.KoreanModFix.dll` is a BepInEx client plugin.
- The correct default install root is `D:\SPT`.
- The server mod install path is `D:\SPT\SPT_Runtime\user\mods\SPT_Korean_Localization`.
- The client plugin install path is `D:\SPT\BepInEx\plugins\GoLani.KoreanModFix.dll`.
- The local SPT target reports a 4.1.x server version compatible with the guarded installer.
- SPT 4.1.0 provides the reference DLL surfaces needed for the client plugin build: `BepInEx\core`, `BepInEx\plugins\spt`, and `EscapeFromTarkov_Data\Managed`.
- The bilingual overlay has 31,084 keys; against the SPT 4.1.0 base locale it has 468 missing base keys and 2 extra legacy keys.
- BepInEx loaded Korean Patch Fix 1.3.0, but no custom `Enabled patch` entries followed because the first patch still targeted the removed `OfferItemDescription.method_1` method.

## Repaired

- Package references were aligned with SPT 4.1.0.
- A user-facing error mentioned an outdated base locale path.
- One patch key had a real newline in the key name instead of the escaped sequence used by the base locale.
- The server transformer callback had a nullable warning path.
- The client plugin source was moved from a separate repository into the integrated project and builds cleanly against the local SPT reference set.
- SPT 4.1 renamed the flea-market handbook node and insurance company types; the client patches now target the current public types and method names.
- Korean Patch Fix 1.4.0 targets `OfferItemDescription.SetItemName`, handles the current public TMP fields and `UiPools.Init` overload, loads after SPT.Core, and isolates patch-group failures so one incompatible screen does not disable all later fixes.

## Residual Risks

- Server verification confirms build/package/install and previous server locale behavior, not every in-game screen.
- The 468 SPT 4.1 base keys absent from the bilingual overlay remain on the built-in Korean strings until the translation source is refreshed for the 4.1 key set.
- Client plugin verification currently confirms build/package/install only; actual UI behavior requires launching the SPT client.
- If SPT or EFT changes client UI method names, private fields, or SPT reflection APIs, client patches can fail even when the project compiles.
