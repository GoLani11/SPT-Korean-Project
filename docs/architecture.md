# Architecture

## Runtime Shape

The project builds one .NET 9 library, `SPT_Korean_Localization.dll`. SPT loads it as a server mod through dependency injection. `KoreanPatcher` implements `IOnLoad`, so the mod applies its locale transformer during server startup after the SPT mod loader has prepared the database services.

## Load Flow

1. SPT discovers `SPT_Korean_Localization.dll` under `SPT\user\mods\SPT_Korean_Localization`.
2. The dependency injection container creates `KoreanPatcher` with `ISptLogger<KoreanPatcher>` and `DatabaseService`.
3. `OnLoad()` locates the base `kr` locale from `databaseService.GetLocales().Global`.
4. The mod reads `locale\kr.json` next to the loaded DLL.
5. The parsed patch dictionary is attached with `koreanLocale.AddTransformer(...)`.
6. When SPT resolves the lazy locale data, each patch key overwrites or adds a value in the `kr` locale dictionary.

## File Layout

The installable mod folder contains:

```text
SPT_Korean_Localization.dll
SPT_Korean_Localization.deps.json
locale\kr.json
```

The release package mirrors the SPT install layout:

```text
release\SPT\user\mods\SPT_Korean_Localization
```

## Dependency Shape

The project references `SPTarkov.Common`, `SPTarkov.DI`, and `SPTarkov.Server.Core`. The current package version set is `4.0.13`, matching the verified local server target.

## Failure Behavior

Startup should continue without crashing if the base Korean locale, assembly path, patch file, or parsed patch data is unavailable. The mod logs a specific error or warning and exits `OnLoad()` early.
