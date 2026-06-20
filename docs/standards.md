# Standards

## Language

Project-facing files are written in English. Direct conversation with the user is Korean by default. Code identifiers, package names, commands, paths, logs, and quoted source text keep their original language.

## PowerShell

Use PowerShell 7 for normal project work:

```powershell
pwsh -NoLogo -NoProfile -NonInteractive
```

Set UTF-8 input and output when commands read or write locale data. Avoid Windows PowerShell 5.1 unless a legacy Windows-only behavior requires it.

## Editing

Keep compatibility edits small. Avoid broad formatting churn in `src\ServerLocaleMod\locale\kr.json`, `src\ServerLocaleMod\KoreanPatcher.cs`, client patch files, and project files. Treat generated `artifacts/`, `bin/`, `obj/`, and `release/` folders as disposable output.

## Verification

Use the real user entry points when possible:

```powershell
dotnet restore
dotnet build -c Release
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\tools\package-release.ps1
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\tools\install-to-spt.ps1 -TargetSptRoot D:\SPT
```

Document any blocked verification separately from successful static checks.

## Git

Keep commits focused. Commit messages for this user are Korean. Do not commit generated build output, release output, or local runtime logs.
