# Contracts

## Server Mod Metadata Contract

`ModMetadata` must provide a stable GUID, display name, author list, version, SPT version range, and license. The current GUID is:

```text
com.golani.makina.korean
```

## Client Plugin Metadata Contract

The BepInEx plugin identity must remain stable unless the user intentionally creates a new plugin lineage:

```text
com.GoLani.koreanpatchfix
Korean Patch Fix
1.4.0
```

## Runtime Data Contract

`src\ServerLocaleMod\locale\kr.json` must deserialize into a `Dictionary<string, string>`. Keys are SPT locale keys. Values are Korean locale strings or preserved source strings.

## Package Contract

The release package root must contain:

```text
SPT_Runtime\user\mods\SPT_Korean_Localization\SPT_Korean_Localization.dll
SPT_Runtime\user\mods\SPT_Korean_Localization\locale\kr.json
BepInEx\plugins\GoLani.KoreanModFix.dll
```

The server package may include:

```text
SPT_Runtime\user\mods\SPT_Korean_Localization\SPT_Korean_Localization.deps.json
```

## GitHub Release Asset Contract

The release asset workflow must create these zip files under `artifacts\release`:

```text
SPT_Korean_Localization.KR.EN._G.M.zip
SPT_Korean_Localization.KR._G.M.zip
```

Inside each zip, the server mod folder name matches the zip base name:

```text
SPT_Runtime\user\mods\SPT_Korean_Localization.KR.EN._G.M
SPT_Runtime\user\mods\SPT_Korean_Localization.KR._G.M
```

The `KR.EN` zip keeps the source bilingual locale. The `KR` zip is derived from the same package output and removes trailing English helper lines that occupy the final line of a value, such as `\n(English item name)`.

## Installer Contract

The installer accepts an SPT root, not the inner runtime folder. For the default local install, the input is `D:\SPT`, and the server executable must exist at `D:\SPT\SPT_Runtime\SPT.Server.exe`.

The installer must reject non-SPT paths and SPT versions outside `4.1.x`.

The installer must replace only the Korean server mod folder and the Korean client plugin DLL.

## README Contract

README install instructions must match the actual script parameters, default target, output layout, and dual-runtime package shape.
