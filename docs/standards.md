# Standards

## Language

Project-facing files are written in English. Direct conversation with the user is Korean by default. Code identifiers, package names, commands, paths, logs, and quoted source text keep their original language.

## PowerShell

Use the translation repository's Python environment for release work:

```text
..\spt-korean-translate\.venv\Scripts\python.exe
```

All locale I/O must use UTF-8. PowerShell is not required for release packaging.

## Editing

Keep compatibility edits small. Locale translations belong to `spt-korean-translate`, not this repository. Treat generated `artifacts/`, `bin/`, `obj/`, and `release/` folders as disposable output.

## Verification

Use the real user entry points when possible:

```powershell
..\spt-korean-translate\.venv\Scripts\python.exe .\tools\package_release_versions.py
```

Document any blocked verification separately from successful static checks.

## Git

Keep commits focused. Commit messages for this user are Korean. Do not commit generated build output, release output, or local runtime logs.
