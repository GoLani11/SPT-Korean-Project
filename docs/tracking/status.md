# Status

## Current State

Date: 2026-06-17

The repository is being reorganized into an integrated SPT Korean package with two runtime outputs: a server locale mod and a BepInEx client UI fix plugin. The current verified target is local SPT 4.0.13 at `D:\SPT`.

## Completed

- Server locale mod moved under `src\ServerLocaleMod`.
- Client UI fix plugin source merged under `src\ClientModFixPlugin`.
- Solution renamed to `SPT-Korean-Project.sln`.
- Client plugin project added with references resolved from `D:\SPT`.
- Release package now contains both `SPT\user\mods\SPT_Korean_Localization` and `BepInEx\plugins\GoLani.KoreanModFix.dll`.
- Installer now installs both outputs by default and supports `-SkipClientPlugin`.
- Local install applied to `D:\SPT\SPT\user\mods\SPT_Korean_Localization` and `D:\SPT\BepInEx\plugins\GoLani.KoreanModFix.dll`.

## Verified Evidence

- `dotnet build .\SPT-Korean-Project.sln -c Release --no-restore -p:SptRoot=D:\SPT` succeeded with zero warnings and zero errors.
- `tools\package-release.ps1 -SptRoot D:\SPT` succeeded.
- `tools\install-to-spt.ps1 -TargetSptRoot D:\SPT` succeeded.
- Packaged `locale\kr.json` parsed as JSON.
- Server output exists under `release\SPT\user\mods\SPT_Korean_Localization`.
- Client output exists under `release\BepInEx\plugins\GoLani.KoreanModFix.dll`.
- Hidden SPT server startup loaded the server mod, `/client/locale/kr` returned HTTP `200`, and the verification server process was stopped.
- Fake non-SPT install target was rejected before any package/install write.

## Open Work

- Full in-game visual QA for `GoLani.KoreanModFix.dll` has not been performed.
- Translation quality remains outside the compatibility repair scope.
- Future SPT releases outside 4.0.x require a fresh API, locale-key, and client patch-target comparison.
