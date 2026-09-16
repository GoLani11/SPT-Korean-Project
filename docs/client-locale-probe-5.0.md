# SPT 5.0 Korean locale probe

This is a separate, deliberately small IL2CPP test plugin. It does not extend the
2.1.1 release support matrix and must not be packaged in the stable release.

The probe supports exactly `5.0.0-BLEEDINGEDGEMODS+ec15a40.20260914`, EFT
`1.1.5.0.47242`, and the recorded original English locale SHA-256. A mismatch
leaves the plugin disabled. It reads those files but never writes game databases,
server settings, profiles, quests, or dialogue progress.

## Visual test

1. Close the game.
2. Extract the probe ZIP's `BepInEx` folder into the **5.0 test game's root**.
3. Start the game normally and select Korean in the interface language setting.
4. On the main screen, look for `캐릭터 [KR5]`, `거래 [KR5]`, and `설정 [KR5]`.
   Entering a raid is unnecessary. English and other languages are untouched.
5. Switch to English, then back to Korean. Reopen the settings screen and verify
   the markers return without stacking. If a mod supplies different text for a
   selected key, that text is intentionally preserved instead of marked.

Inspect `BepInEx/LogOutput.log` if the markers do not appear:

- `KR5 READY`: source/build checks passed and the native update hook was installed.
- `KR5 UPDATE`: the game delivered a locale update, including its language code.
- `KR5 APPLIED`: known original strings were replaced in the incoming Korean data.
- `KR5 LOOKUP: 3/3`: after the original update, the real lookup returns all markers.
- `KR5 DISABLED`: build/source/signature validation or hook installation failed.

These messages distinguish loading, data application, and lookup. They do not prove
font rendering, button layout, or full story translation. Partial updates may
legitimately contain fewer than three keys. If startup reaches READY but no Korean
update occurs, switch languages once and check the log again.

To remove the test, close the game, delete only
`BepInEx/plugins/GoLani.KoreanLocaleProbe50`, and restart. Memory-only marker strings
are rebuilt from the original server data. Hot-unloading while the game is running
is not supported. The existing 4.x plugin should not be copied into the 5.0 game.

## Implementation and build

The actual installed interop assembly exposes:

```text
EFT.LocalizationManager.UpdateLocales(
    string, Il2CppSystem.Collections.Generic.Dictionary<string, string>, bool)
```

A Harmony prefix only replaces three approved values when the update language is
`kr` and the incoming value matches the original Korean or English fallback. The
original method still executes with its original third argument. A postfix checks
the actual lookup when Korean is active. No language entry, custom font, UI widget,
or server response is added by this probe.

Run from this repository with Python 3.10+ and a Windows .NET 9 SDK:

```text
python tools/package_locale_probe50.py --spt-root D:\SPT_5.0_TEST
```

WSL is supported using `/mnt/d/SPT_5.0_TEST` and the installed Windows `dotnet.exe`.
The plugin targets the game's .NET 6 runtime using local runtime/interop references;
no targeting pack or NuGet package is downloaded. The contract runs on .NET 9 and
checks source mismatch handling, other-language/mod-text preservation, idempotence,
and the actual interop/plugin metadata. The package contains only our DLL, license,
and this guide. Game and BepInEx assemblies are never redistributed.

Output: `artifacts/locale-probe50/`. `verification.json` explicitly separates static
and managed contract checks from the pending native-game and visual checks.
