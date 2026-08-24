# Operations

## Prerequisites

- .NET 10 SDK capable of building net9, net10, and net48 projects
- An SPT install exposing the shared BepInEx, Harmony, Unity, and TextMeshPro client references; `D:\SPT3.8.3` is preferred and the batch entry point falls back to `D:\SPT`
- `spt-korean-translate` checked out beside this repository with its Python virtual environment
- Generated translation outputs for all eight supported SPT versions

## Build And Package

```powershell
..\spt-korean-translate\.venv\Scripts\python.exe .\tools\package_release_versions.py
```

Use `--client-reference-spt-root` when the 3.8.3 install is elsewhere. Use `--dotnet` to select a non-default .NET 10 SDK. `make-release-packages.bat` invokes the same Python entry point.

The command always restores and builds the solution, executes both 4.1 compatibility contracts, validates all locale key and shared-range equivalence contracts, creates 14 ZIP files under `artifacts\release`, reopens every archive for layout and source-hash checks, and writes `release-summary.json`. A build cannot be skipped because stale compatibility binaries must never be reused for a release.

## Runtime Verification

Extract each ZIP into its matching clean SPT install. The server log must report `SPT_Korean_Localization_(G&M)` and the version's expected locale key count. The BepInEx log must report the detected SPT version and a final enabled/unavailable/failed patch summary. Prestige reward adjustment is normally unavailable on SPT 3.8.3–3.10.5.

Before a release, resolve the common client DLL's patch targets against the actual `Assembly-CSharp.dll` from every supported install, then visually smoke-test the adjusted UI through the normal SPT launcher. A reflection target check confirms structural compatibility but does not replace rendered UI verification.

For the exact 4.1.0 package, positively verify 4.1.0 and negatively verify 4.1.1 and 4.1.2. For the shared package, positively verify 4.1.2 and 4.1.3 and negatively verify 4.1.0, 4.1.1, a prerelease, and 4.2.0. For exact-version packages, place the server package outside its declared version and confirm that the loader reports a mismatch without loading the mod. Do not continue using a mismatched installation.

Generated `artifacts`, `bin`, and `obj` content is disposable and must not be committed.
