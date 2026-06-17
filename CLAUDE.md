# SPT Korean Project Agent Guide

## Overview

SPT-Korean-Project is an integrated Korean package for SPT. It builds a server locale mod and a BepInEx client UI fix plugin. The current compatibility target is SPT 4.0.x, validated against a local SPT 4.0.13 install at `D:\SPT`.

The server runtime output belongs under `SPT\user\mods\SPT_Korean_Localization`. The client runtime output belongs under `BepInEx\plugins\GoLani.KoreanModFix.dll`.

## Navigation

- `SPT-Korean-Project.sln`: solution containing both runtime projects.
- `src/ServerLocaleMod/KoreanPatcher.cs`: server mod metadata, dependency injection entry point, locale transformer.
- `src/ServerLocaleMod/SPT_Korean_Localization.csproj`: .NET target, SPT server package versions, locale copy rules.
- `src/ServerLocaleMod/locale/kr.json`: Korean locale patch data.
- `src/ClientModFixPlugin/GoLani.KoreanModFix.csproj`: BepInEx client plugin build project.
- `src/ClientModFixPlugin/Plugin.cs`: client plugin entry point.
- `src/ClientModFixPlugin/Patches/`: client UI display patches.
- `tools/package-release.ps1`: release folder builder.
- `tools/install-to-spt.ps1`: guarded installer for a local SPT root.
- `README.md`: user-facing install and package instructions.
- `docs/architecture.md`: runtime shape and data flow.
- `docs/business-rules.md`: product and localization invariants.
- `docs/security.md`: write boundaries and unsafe surfaces.
- `docs/operations.md`: build, install, and verification routine.
- `docs/contracts.md`: layout and behavior contracts.
- `docs/tracking/status.md`: current compatibility state.
- `docs/tracking/findings.md`: confirmed findings and remaining risks.

## Hard Gates

- Keep project-facing artifacts in English; talk to the user in natural Korean unless they ask otherwise.
- Preserve existing translation values unless the task explicitly asks for translation refresh work.
- Install only the server mod into `SPT\user\mods\SPT_Korean_Localization` and the client plugin into `BepInEx\plugins\GoLani.KoreanModFix.dll`.
- Do not modify game binaries, BattlEye, launcher behavior, BepInEx core files, SPT client support plugins, or unrelated SPT mods.
- Verify JSON validity, package output, and at least one real or guarded SPT install path before claiming compatibility.

## Pre-Work Checklist

- Read `README.md`, the relevant project under `src/`, and the relevant `docs/` file for the task.
- Check `git status --short --ignored` before editing.
- Treat ignored `bin/`, `obj/`, and `release/` output as generated unless the user specifically asks about them.
- Use PowerShell 7 through `pwsh -NoLogo -NoProfile -NonInteractive` for Windows commands.
- Prefer `rg` and `git grep` for search.

## Routing

- Server compatibility change: update SPT package versions, mod metadata range if needed, then build and install-test.
- Client compatibility change: update BepInEx/SPT client references, then build and test with the real SPT client when possible.
- Translation data change: edit `locale/kr.json`, preserve valid JSON, compare keys against the current SPT base locale when available.
- Packaging change: keep script writes inside the configured release or SPT mod folder, then test both success and rejection paths.
- Documentation change: keep README user-facing and keep operational details synchronized with scripts.
