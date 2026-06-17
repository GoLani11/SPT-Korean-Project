# Status

## Current State

Date: 2026-06-17

The project has been updated for SPT 4.0.x server-mod compatibility and verified against local SPT 4.0.13 at `D:\SPT`.

## Completed

- SPT package references updated to `4.0.13`.
- Nullable locale transformer guard added.
- Stale SPT base locale path message corrected.
- Locale key identity mismatch repaired without refreshing translations.
- Release package script added.
- Guarded local SPT installer added.
- README install guidance updated for the SPT 4.x server-mod layout.
- Local install applied to `D:\SPT\SPT\user\mods\SPT_Korean_Localization`.

## Verified Evidence

- `dotnet restore` succeeded.
- `dotnet build -c Release` succeeded with zero warnings and zero errors.
- Packaged `locale\kr.json` parsed as JSON.
- Patch key count: `31084`.
- Base SPT 4.0.13 Korean locale key count: `31084`.
- Missing patch keys in base locale: `0`.
- Base locale keys absent from patch file: `0`.
- SPT server log showed the mod loading and applying `31084` entries.
- SPT server handled `/client/locale/kr` with HTTP `200`.

## Open Work

- Full in-game visual QA has not been performed.
- Translation quality remains outside the compatibility repair scope.
- Future SPT major versions require a fresh API and locale-key comparison.
