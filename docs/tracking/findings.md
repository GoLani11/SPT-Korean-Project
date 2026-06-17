# Findings

## Confirmed

- The repository is a C# SPT server mod, not a BepInEx client plugin.
- The correct default install root is `D:\SPT`, producing `D:\SPT\SPT\user\mods\SPT_Korean_Localization`.
- The local SPT target reports a 4.x server version compatible with the guarded installer.
- SPT 4.0.13 serves a base Korean locale with `31084` keys.
- The project patch file also contains `31084` keys after the escaped-newline key repair.

## Repaired

- Package references were stale for SPT 4.0.13.
- A user-facing error mentioned an outdated base locale path.
- One patch key had a real newline in the key name instead of the escaped sequence used by the base locale.
- The transformer callback had a nullable warning path.

## Residual Risks

- Runtime verification confirms server load and locale endpoint access, not every in-game screen.
- If SPT changes server mod metadata APIs in a later major version, the current source may require code changes beyond package updates.
- If SPT adds or removes locale keys, translation-preserving compatibility work must separate key repair from translation refresh.
