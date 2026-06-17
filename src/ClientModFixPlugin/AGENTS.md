# Client Mod Fix Plugin Guide

## Scope

This module builds `GoLani.KoreanModFix.dll`, the BepInEx client plugin that adjusts Korean UI display issues in the EFT client.

## Rules

- Keep runtime output under `BepInEx\plugins\GoLani.KoreanModFix.dll`.
- Do not add SPT server package references to this project.
- Build references must come from the active SPT install through the `SptRoot` MSBuild property.
- Treat `BepInEx\core`, `BepInEx\plugins\spt`, and `EscapeFromTarkov_Data\Managed` as reference-only surfaces.
- Patch internals such as method names and private fields can drift between SPT/EFT versions; verify them against the real client when changed.

## Verification

The minimum static gate is a clean `Release` build. Runtime verification requires launching the SPT client and checking BepInEx logs plus the affected UI screens.
