"""Build and verify the installer-free client-only experiment for the explicit supported SPT profile matrix."""
from __future__ import annotations

import argparse
import json
import os
import re
import shutil
import subprocess
import zipfile
from pathlib import Path, PurePosixPath

import package_release_versions as release


PROFILE_FILE = Path(__file__).with_name("client-locale-prototype.json")
PLUGIN_ROOT = PurePosixPath("BepInEx/plugins")
BUNDLE_ROOT = PLUGIN_ROOT / "SPT-Korean"
ARCHIVE_NAME = "SPT-KR-Client-Prototype.zip"


def stage_payloads(translation_root: Path, stage_root: Path, profile_file: Path = PROFILE_FILE) -> dict:
    config = release.load_ordered_json(profile_file)
    if config.get("schemaVersion") != 1 or not config.get("profiles"):
        raise ValueError("unexpected client-only prototype profile matrix")
    manifest = {"schemaVersion": 1, "profiles": {}}
    expected = {}
    for version, source_profile in config["profiles"].items():
        translation_version = source_profile["translationVersion"]
        if not re.fullmatch(r"\d+\.\d+\.\d+", version) or not re.fullmatch(r"\d+\.\d+\.\d+", translation_version):
            raise ValueError(f"invalid SPT or translation version: {version}")
        sources = {
            "en.json": translation_root / "versions" / translation_version / "input" / "en.json",
            "kr.json": translation_root / "output" / translation_version / "kr.generated.json",
            "kr-en.json": translation_root / "output" / translation_version / "kr-en.generated.json",
        }
        for name in ("kr.json", "kr-en.json"):
            release.validate_locale_pair(sources["en.json"], sources[name])
        hashes = {}
        for name, source in sources.items():
            relative = BUNDLE_ROOT / "locales" / translation_version / name
            destination = stage_root.joinpath(*relative.parts)
            destination.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(source, destination)
            hashes[name] = release.sha256_file(source)
            expected[relative.as_posix()] = hashes[name]
        manifest["profiles"][version] = {**source_profile, "sha256": hashes}
    manifest_relative = BUNDLE_ROOT / "manifest.json"
    manifest_path = stage_root.joinpath(*manifest_relative.parts)
    manifest_path.write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    expected[manifest_relative.as_posix()] = release.sha256_file(manifest_path)
    return expected


def validate_archive(archive: Path, expected: dict[str, str]) -> None:
    with zipfile.ZipFile(archive) as package:
        names = package.namelist()
        if len(names) != len(set(names)) or set(names) != set(expected):
            raise ValueError("prototype archive has unexpected, missing, or duplicate entries")
        for name in names:
            path = PurePosixPath(name)
            if "\\" in name or path.is_absolute() or ".." in path.parts or path.parts[:2] != PLUGIN_ROOT.parts:
                raise ValueError(f"unsafe prototype archive path: {name}")
            if path.suffix.lower() in {".bat", ".cmd", ".exe"}:
                raise ValueError(f"installer/executable is not allowed in the prototype archive: {name}")
            if release.sha256_bytes(package.read(name)) != expected[name]:
                raise ValueError(f"prototype archive hash mismatch: {name}")


def run_windows_contract(executable: Path, arguments: list[Path], mono_root: Path | None = None) -> None:
    if os.name == "nt":
        prefix = ["--unity-mono", str(mono_root)] if mono_root is not None else []
        command = [str(executable), *prefix, *(str(path) for path in arguments)]
    elif shutil.which("wslpath"):
        # The game and its Harmony runtime target .NET Framework; execute that runtime through WSL interop.
        executable.chmod(executable.stat().st_mode | 0o111)
        def windows_path(path: Path) -> str:
            return subprocess.check_output(["wslpath", "-w", str(path.resolve())], text=True).strip()

        prefix = ["--unity-mono", windows_path(mono_root)] if mono_root is not None else []
        command = [str(executable), *prefix, *(windows_path(path) for path in arguments)]
    else:
        raise RuntimeError("the client runtime contract requires Windows or WSL interop")
    subprocess.run(command, check=True)


def stage_server_status(project_root: Path, work: Path, dotnet: str, profiles: dict) -> dict:
    """Optional version-specific console companions; excluded from the common client ZIP."""
    output = work / "server-status"
    if output.exists():
        shutil.rmtree(output)
    for variant in ("40", "410", "41"):
        name = f"ServerLocaleStatus{variant}"
        release.run_command([dotnet, "build", str(project_root / "src" / name / f"{name}.csproj"), "-c", "Release", "-v:minimal"], project_root)
    for version in profiles:
        if version.startswith("3."):
            destination = output / version / "user/mods/GoLani.KoreanLocalization.Status"
            shutil.copytree(project_root / "src/ServerLocaleStatus3/src", destination / "src")
            metadata = {"name": "SPT Korean Localization", "author": "Golani", "version": "2.2.0",
                        "main": "src/mod.js", "license": "MIT", "akiVersion" if version == "3.8.3" else "sptVersion": version}
            (destination / "package.json").write_text(json.dumps(metadata, indent=2) + "\n", encoding="utf-8")
        else:
            variant = "40" if version == "4.0.13" else "410" if version == "4.1.0" else "41"
            server = "SPT" if version == "4.0.13" else "SPT_Runtime"
            destination = output / version / server / "user/mods/GoLani.KoreanLocalization.Status"
            destination.mkdir(parents=True)
            shutil.copy2(project_root / "artifacts/build/Release" / f"ServerLocaleStatus{variant}" / "GoLani.KoreanLocalization.Status.dll", destination)
    return {path.relative_to(output).as_posix(): release.sha256_file(path) for path in release.iter_package_files(output)}


def main() -> None:
    project_root = Path(__file__).resolve().parents[1]
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--translation-root", type=Path, default=project_root.parent / "spt-korean-translate")
    parser.add_argument("--dotnet")
    parser.add_argument("--installations-root", type=Path, default=Path("D:/" if os.name == "nt" else "/mnt/d"))
    parser.add_argument("--no-archive", action="store_true", help="stage and verify files for direct copying without creating a ZIP")
    parser.add_argument("--spt-383-root", type=Path, default=Path("D:/SPT_3.8.3" if os.name == "nt" else "/mnt/d/SPT_3.8.3"))
    parser.add_argument("--spt-415-root", type=Path, default=Path("D:/SPT" if os.name == "nt" else "/mnt/d/SPT"))
    args = parser.parse_args()
    dotnet = release.resolve_dotnet(args.dotnet, project_root)
    work = release.ensure_generated_output_path(project_root / "artifacts" / "client-locale-prototype", project_root)
    stage = work / "stage"
    archive = work / ARCHIVE_NAME
    if archive.exists():
        archive.unlink()
    summary_path = work / "verification.json"
    if summary_path.exists():
        summary_path.unlink()
    if stage.exists():
        shutil.rmtree(stage)
    expected = stage_payloads(args.translation_root.resolve(), stage)
    for project in (
        project_root / "src/ClientLocalePrototype/GoLani.KoreanLocalization.Prototype.csproj",
        project_root / "tests/ClientLocaleContract/ClientLocaleContract.csproj",
    ):
        release.run_command([
            dotnet, "build", str(project), "-c", "Release",
            f"-p:ClientReferenceSptRoot={args.spt_383_root.resolve()}", "-v:minimal",
        ], project_root)
    build = project_root / "artifacts/build/Release"
    client = build / "ClientLocalePrototype" / release.CLIENT_DLL_NAME
    destination = stage / "BepInEx/plugins" / release.CLIENT_DLL_NAME
    shutil.copy2(client, destination)
    expected[(PLUGIN_ROOT / release.CLIENT_DLL_NAME).as_posix()] = release.sha256_file(client)
    manifest_path = stage.joinpath(*BUNDLE_ROOT.parts) / "manifest.json"
    manifest = release.load_ordered_json(manifest_path)
    manifest.update(clientVersion="2.2.0", clientDllSha256=release.sha256_file(client))
    manifest_path.write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    expected[(BUNDLE_ROOT / "manifest.json").as_posix()] = release.sha256_file(manifest_path)
    matrix = {}
    complete_roots = {}
    for version, profile in manifest["profiles"].items():
        root = args.spt_383_root if version == "3.8.3" else args.spt_415_root if version == "4.1.5" else args.installations_root / f"SPT_{version}"
        complete = all((root / relative).is_file() for relative in (
            "EscapeFromTarkov.exe", "EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll", "MonoBleedingEdge/EmbedRuntime/mono-2.0-bdwgc.dll"))
        if complete:
            complete_roots[version] = root
        elif not (root / profile["englishPath"]).is_file():
            # Missing game versions still exercise their actual translation payload in the native fixture.
            root = work / "fixture-games" / version
            english = root / profile["englishPath"]
            english.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(stage.joinpath(*BUNDLE_ROOT.parts) / "locales" / profile["translationVersion"] / "en.json", english)
        windows_root = str(root.resolve()) if os.name == "nt" else subprocess.check_output(["wslpath", "-w", str(root.resolve())], text=True).strip()
        matrix[version] = {"root": windows_root, "completeClient": complete}
    matrix_path = work / "installations.json"
    matrix_path.write_text(json.dumps(matrix, indent=2) + "\n", encoding="utf-8")
    contract_report = work / "contract-verification.json"
    run_windows_contract(build / "ClientLocaleContract/ClientLocaleContract.exe", [
        stage.joinpath(*BUNDLE_ROOT.parts), matrix_path, contract_report,
    ])
    mono_reports = []
    for label, mono_root in complete_roots.items():
        mono_report = work / f"mono-{label}-verification.json"
        run_windows_contract(build / "ClientLocaleContract/ClientLocaleContract.exe", [
            stage.joinpath(*BUNDLE_ROOT.parts), matrix_path, mono_report,
        ], mono_root=mono_root)
        mono_reports.append({"hostSptVersion": label, **release.load_ordered_json(mono_report)})
    staged_hashes = {
        path.relative_to(stage).as_posix(): release.sha256_file(path)
        for path in release.iter_package_files(stage)
    }
    if staged_hashes != expected:
        raise ValueError("staged prototype files differ from their verified build sources")
    if not args.no_archive:
        release.create_deterministic_zip(stage, archive)
        validate_archive(archive, expected)
    status_files = stage_server_status(project_root, work, dotnet, manifest["profiles"])
    summary = {
        "kind": "client-only-prototype",
        "profiles": list(manifest["profiles"]),
        "runtime_contract": "passed: native reload fixture with actual Harmony and locale payloads",
        "contract_details": release.load_ordered_json(contract_report),
        "unity_mono_contracts": mono_reports,
        "in_game_visual_validation": "not performed by this command",
        "archive": None if args.no_archive else archive.name,
        "archive_sha256": None if args.no_archive else release.sha256_file(archive),
        "files": expected,
        "optional_server_status_files": status_files,
    }
    summary_path.write_text(json.dumps(summary, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"Verified client-only prototype: {stage if args.no_archive else archive}")


if __name__ == "__main__":
    main()
