# Engineering Notes

## Current Compatibility Notes

The server package references are aligned to `4.1.0`. The server mod metadata keeps `SptVersion` at `~4.1.0`, so it accepts the 4.1.x compatibility family while avoiding a false claim for later SPT release families.

The client plugin source was merged from `GoLani11/GoLani.KoreanModFix` and now builds against the active SPT install through `SptRoot`. It still targets `.NET Framework 4.8`, matching the BepInEx client plugin shape.

## Locale Key Repair

One locale key used an embedded newline where the SPT base locale key contains the escaped sequence. The repaired key preserves the same visible label while matching the base key identity.

The bilingual locale contains 31,084 keys. Compared with the SPT 4.1.0 base locale, 468 new base keys are not overridden and therefore keep their built-in Korean values, while 2 legacy patch keys are added by the overlay.

## Nullable Safety

`AddTransformer` handles a null locale dictionary defensively. The expected path is still a non-null dictionary from SPT, but the guard prevents a nullable warning and logs an explicit failure if SPT supplies no data.

## Client Patch Risk

The client plugin uses Harmony patches, SPT reflection patches, private fields, and game UI method names. These can drift between EFT/SPT versions even when the project compiles. Treat a clean build as a static gate, not full client runtime proof.

## Startup Evidence

A successful server startup reports that the server mod loaded, the Korean localization project applied, and `31084` patch entries were processed. A successful locale request to `/client/locale/kr` confirms the server can serve the merged locale data.

## Risk Notes

The server mod overlays keys at runtime. It does not verify semantic correctness of every translation string. Future SPT releases outside the verified 4.1.x family can change locale key names, server mod APIs, client patch targets, or package names, so each compatibility bump needs a build, key comparison, package, install, server startup check, and client UI check where possible.
