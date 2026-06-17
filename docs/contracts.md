# Contracts

## Mod Metadata Contract

`ModMetadata` must provide a stable GUID, display name, author list, version, SPT version range, and license. The current GUID is:

```text
com.golani.makina.korean
```

## Runtime Data Contract

`locale\kr.json` must deserialize into a `Dictionary<string, string>`. Keys are SPT locale keys. Values are Korean locale strings or preserved source strings.

## Package Contract

The release package root must contain:

```text
SPT\user\mods\SPT_Korean_Localization\SPT_Korean_Localization.dll
SPT\user\mods\SPT_Korean_Localization\locale\kr.json
```

The package may include:

```text
SPT\user\mods\SPT_Korean_Localization\SPT_Korean_Localization.deps.json
```

## Installer Contract

The installer accepts an SPT root, not the inner server folder. For the default local install, the input is `D:\SPT`, and the server executable must exist at `D:\SPT\SPT\SPT.Server.exe`.

The installer must reject non-SPT paths and SPT versions outside `4.0.x`.

## README Contract

README install instructions must match the actual script parameters, default target, output layout, and current server-mod classification.
