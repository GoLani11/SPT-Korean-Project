# Status

## Current State

Date: 2026-08-02

The repository is an integrated SPT Korean package with two runtime outputs: a server locale mod and a BepInEx client UI fix plugin. The current verified target is local SPT 4.1.0 at `D:\SPT`.

## Completed

- Server locale mod moved under `src\ServerLocaleMod`.
- Client UI fix plugin source merged under `src\ClientModFixPlugin`.
- Solution renamed to `SPT-Korean-Project.sln`.
- Client plugin project added with references resolved from `D:\SPT`.
- Release package now contains both `SPT_Runtime\user\mods\SPT_Korean_Localization` and `BepInEx\plugins\GoLani.KoreanModFix.dll`.
- Installer now installs both outputs by default and supports `-SkipClientPlugin`.
- Local install applied to `D:\SPT\SPT_Runtime\user\mods\SPT_Korean_Localization` and `D:\SPT\BepInEx\plugins\GoLani.KoreanModFix.dll`.
- Server and client sources were updated for the SPT 4.1 API and runtime layout.

## Verified Evidence

- `dotnet build .\SPT-Korean-Project.sln -c Release -p:SptRoot=D:\SPT` succeeded with zero warnings and zero errors using .NET SDK 10.0.302.
- Release output was generated under the SPT 4.1 `SPT_Runtime` layout and its locale JSON was validated independently after PowerShell 5 could not perform the script's PowerShell 7-only `ConvertFrom-Json -AsHashtable` check.
- The validated release files were installed to `D:\SPT` and matched their source SHA-256 hashes.
- Packaged `locale\kr.json` parsed as JSON.
- Server output exists under `artifacts\release\SPT_Runtime\user\mods\SPT_Korean_Localization`.
- Client output exists under `artifacts\release\BepInEx\plugins\GoLani.KoreanModFix.dll`.
- Hidden SPT 4.1.0 server startup loaded the server mod, applied 31,084 entries, started the web server, and the verification server process was stopped.
- Fake non-SPT install target was rejected before any package/install write.

## Open Work

- Full in-game visual QA for `GoLani.KoreanModFix.dll` has not been performed.
- Translation quality remains outside the compatibility repair scope.
- Future SPT releases outside 4.1.x require a fresh API, locale-key, and client patch-target comparison.
