# Operations

## Prerequisites and packaging

Use .NET 10 SDK, Python, Windows .NET Framework 4.8 (directly or through WSL interop), the sibling `spt-korean-translate` generated outputs, and SPT 3.8.3 client references. Complete local game installations supply optional additional Unity Mono runtime coverage.

```powershell
python .\tools\package_release.py --spt-383-root D:\SPT_3.8.3 --spt-415-root D:\SPT
```

`make-release-packages.bat` invokes this entry point using the translation repository's virtual environment. Use `--dotnet`, `--translation-root` and `--installations-root` to override local paths. WSL paths use `/mnt/d/...`.

The command rebuilds the common plugin and contracts, verifies real payloads and native-flow fixtures on Windows and discovered Unity Mono runtimes, creates one ZIP under `artifacts/release-2.1.0`, validates safe paths and hashes, extracts it and reruns the Windows locale contract. It writes `verification.json`, `SHA256SUMS.txt`, release notes and upload instructions. No server module is shipped.

For the full historical solution, pass the client reference root explicitly:

```text
dotnet build SPT-Korean-Project.sln -c Release -p:ClientReferenceSptRoot=<local SPT 3.8.3 path>
python -m unittest discover -s tests -p "test_*.py"
```

## Runtime and handoff verification

Follow the [installation guide](releases/2.1.0-install.md), including backing up and removing previous Korean server modules and duplicate DLLs. Use the normal launcher. Verify adjacent language choices, immediate switching, persisted selection and the adjusted UI. Check the game BepInEx log for version 2.1.0 and locale initialization; individual UI fixes report enabled/unavailable/failed separately. Features absent on older games may be unavailable normally.

Metadata inspection and managed runtime fixtures do not render fonts or game screens. Record missing installations and visual coverage limits explicitly. Current coverage is listed in [status](tracking/status.md). A simulated future patch is not an actual future-client test.

Before handoff, confirm the ZIP checksum, exact contents, version metadata and source commit. Copy the verified ZIP, release notes, upload instructions, checksum and author-only verification/build information into the delivery folder. The author creates the GitHub release/tag and uploads the asset separately. Generated `artifacts`, `bin`, `obj` and release files must not be committed.
