# Server Locale Mod Guide

## Scope

This module builds `SPT_Korean_Localization.dll`, the SPT server mod that overlays Korean locale data into the server database.

## Rules

- Keep runtime output under `SPT\user\mods\SPT_Korean_Localization`.
- Do not add client-side BepInEx references to this project.
- Preserve existing translation values unless the user explicitly asks for translation refresh work.
- Keep `locale\kr.json` beside the built DLL.
- Update SPT package references and `ModMetadata.SptVersion` together when compatibility changes.

## Verification

Build the solution, package the release, and verify the SPT server log shows the localization mod loaded and applied the expected patch count.
