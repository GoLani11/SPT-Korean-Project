# Engineering Notes

## Current Compatibility Notes

The SPT package references are aligned to `4.0.13`. The mod metadata keeps `SptVersion` at `~4.0.0`, so it accepts the 4.0.x compatibility family while avoiding a false claim for later major versions.

## Locale Key Repair

One locale key used an embedded newline where the SPT base locale key contains the escaped sequence. The repaired key preserves the same visible label while matching the base key identity.

## Nullable Safety

`AddTransformer` now handles a null locale dictionary defensively. The expected path is still a non-null dictionary from SPT, but the guard prevents a nullable warning and logs an explicit failure if SPT supplies no data.

## Startup Evidence

A successful startup reports that the mod loaded, the Korean localization project applied, and `31084` patch entries were processed. A successful locale request to `/client/locale/kr` confirms the server can serve the merged locale data.

## Risk Notes

The mod overlays keys at runtime. It does not verify semantic correctness of every translation string. Future SPT releases can change locale key names, server mod APIs, or package names, so each compatibility bump needs a build, key comparison, package, install, and startup check.
