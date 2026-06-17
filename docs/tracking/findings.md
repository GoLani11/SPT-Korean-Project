# Findings

## Confirmed

- The repository now contains two runtime components, not one.
- `SPT_Korean_Localization.dll` is a C# SPT server mod.
- `GoLani.KoreanModFix.dll` is a BepInEx client plugin.
- The correct default install root is `D:\SPT`.
- The server mod install path is `D:\SPT\SPT\user\mods\SPT_Korean_Localization`.
- The client plugin install path is `D:\SPT\BepInEx\plugins\GoLani.KoreanModFix.dll`.
- The local SPT target reports a 4.0.x server version compatible with the guarded installer.
- SPT 4.0.13 provides the reference DLL surfaces needed for the client plugin build: `BepInEx\core`, `BepInEx\plugins\spt`, and `EscapeFromTarkov_Data\Managed`.

## Repaired

- Package references were stale for SPT 4.0.13.
- A user-facing error mentioned an outdated base locale path.
- One patch key had a real newline in the key name instead of the escaped sequence used by the base locale.
- The server transformer callback had a nullable warning path.
- The client plugin source was moved from a separate repository into the integrated project and builds cleanly against the local SPT reference set.

## Residual Risks

- Server verification confirms build/package/install and previous server locale behavior, not every in-game screen.
- Client plugin verification currently confirms build/package/install only; actual UI behavior requires launching the SPT client.
- If SPT or EFT changes client UI method names, private fields, or SPT reflection APIs, client patches can fail even when the project compiles.
