# Locale Data Guide

## Scope

`locale/kr.json` is the Korean patch data loaded by `KoreanPatcher`. Compatibility work may repair keys or JSON structure, but broad translation refreshes need explicit user approval.

## Rules

- Keep the file valid JSON with string keys and string values.
- Preserve existing Korean values unless a specific issue requires a small correction.
- Do not sort, reformat, or normalize the whole file as part of unrelated work.
- Compare keys against the active SPT base `kr` locale when changing compatibility.
- Record key-count differences when reporting compatibility results.

## Verification

Use JSON parsing before and after edits. For SPT compatibility, compare patch keys with the active base locale and report missing or extra key counts.
