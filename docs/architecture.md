# Architecture

## Runtime Shape

The repository builds two independent runtime components from one solution:

```text
src\ServerLocaleMod       -> SPT_Korean_Localization.dll
src\ClientModFixPlugin    -> GoLani.KoreanModFix.dll
```

The server locale mod is loaded by the SPT server. The client mod fix plugin is loaded by BepInEx inside the EFT client.

## Server Load Flow

1. SPT discovers `SPT_Korean_Localization.dll` under `SPT\user\mods\SPT_Korean_Localization`.
2. The dependency injection container creates `KoreanPatcher` with `ISptLogger<KoreanPatcher>` and `DatabaseService`.
3. `OnLoad()` locates the base `kr` locale from `databaseService.GetLocales().Global`.
4. The mod reads `locale\kr.json` next to the loaded DLL.
5. The parsed patch dictionary is attached with `koreanLocale.AddTransformer(...)`.
6. When SPT resolves the lazy locale data, each patch key overwrites or adds a value in the `kr` locale dictionary.

## Client Load Flow

1. BepInEx discovers `GoLani.KoreanModFix.dll` under `BepInEx\plugins`.
2. `Plugin.Awake()` enables the UI fix patches.
3. Harmony and SPT reflection patches adjust Korean text display in affected client UI screens.

## Release Layout

```text
release\SPT\user\mods\SPT_Korean_Localization
release\BepInEx\plugins\GoLani.KoreanModFix.dll
```

The release mirrors the SPT install root so users can copy `SPT` and `BepInEx` into the target install.

## Dependency Shape

The server mod references `SPTarkov.Common`, `SPTarkov.DI`, and `SPTarkov.Server.Core` package version `4.0.13`.

The client plugin targets `.NET Framework 4.8` and references DLLs from the active SPT install through the `SptRoot` MSBuild property. The default `SptRoot` is `D:\SPT`.
