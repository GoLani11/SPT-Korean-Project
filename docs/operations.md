# Operations

## Prerequisites

- .NET 10 SDK capable of building net9, net10, and net48 projects
- SPT 3.8.3 installed at `D:\SPT3.8.3` for shared client references
- `spt-korean-translate` checked out beside this repository with its Python virtual environment
- Generated translation outputs for all six supported versions

## Build And Package

```powershell
..\spt-korean-translate\.venv\Scripts\python.exe .\tools\package_release_versions.py
```

Use `--client-reference-spt-root` when the 3.8.3 install is elsewhere. Use `--dotnet` to select a non-default .NET 10 SDK. `make-release-packages.bat` invokes the same Python entry point.

The command always restores and builds the solution, executes the shared server/client compatibility contract, validates all locale key contracts, creates 12 ZIP files under `artifacts\release`, reopens every archive for layout and hash checks, and writes `release-summary.json`. A build cannot be skipped because stale compatibility binaries must never be reused for a release.

## Runtime Verification

Extract each ZIP into its matching clean SPT install. The server log must report `SPT_Korean_Localization_(G&M)` and the version's expected locale key count. The BepInEx log must report the detected SPT version and a final enabled/unavailable/failed patch summary. Prestige reward adjustment is normally unavailable on SPT 3.8.3–3.10.5.

Before a release, resolve the common client DLL's patch targets against the actual `Assembly-CSharp.dll` from every supported install, then visually smoke-test the adjusted UI through the normal SPT launcher. A reflection target check confirms structural compatibility but does not replace rendered UI verification.

For the 4.1 package, positively verify stable 4.1.2 and 4.1.3, and negatively verify 4.1.1, a prerelease, and 4.2.0. A newly released stable 4.1.x patch is admitted by the loader but must still pass server API resolution, client target discovery, locale comparison, and later an in-game smoke test before it is marked verified. For older exact-version packages, place the server package outside its declared version and confirm that the loader reports a mismatch without loading the mod. Do not continue using a mismatched installation.

Generated `artifacts`, `bin`, and `obj` content is disposable and must not be committed.
