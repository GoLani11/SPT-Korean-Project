# 0006: Native Locale Mode Switch

## Status

Accepted

## Context

Korean and full Korean-English display modes were distributed as separate ZIP files for every supported release label. Switching modes required overwriting the installed locale payload, and the release matrix contained 14 archives.

## Decision

Publish one `SPT-KR-<version-or-range>.zip` per release label. Each archive contains `locale/kr.json` and `locale/kr-en.json`. The server keeps `kr` as the default Korean locale and registers `kr-en` as `한국어 (한영 병기)`, inheriting the Korean menu locale. The client plugin uses the Korean font fallback for both IDs.

Use the game's existing interface-language setting for selection, reload, and persistence. Do not add a mod configuration file or a separate settings UI.

## Consequences

The release matrix contains seven archives. Existing `kr` users keep their current behavior after updating, while each client can switch display modes without replacing files or restarting. Decisions 0003 and 0005 still define version isolation and short names, but their two-variant package counts are superseded by this decision.
