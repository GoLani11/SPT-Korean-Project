"""Build and verify the installer-free client-only experiment for SPT 3.8.3 and 4.1.5."""
from __future__ import annotations

import argparse
import json
import os
import shutil
import subprocess
import zipfile
from pathlib import Path, PurePosixPath

import package_release_versions as release


PROFILE_FILE = Path(__file__).with_name("client-locale-prototype.json")
PLUGIN_ROOT = PurePosixPath("BepInEx/plugins")
BUNDLE_ROOT = PLUGIN_ROOT / "SPT-Korean"
ARCHIVE_NAME = "SPT-KR-Client-Prototype-3.8.3-4.1.5.zip"


def stage_payloads(translation_root: Path, stage_root: Path, profile_file: Path = PROFILE_FILE) -> dict:
    config = release.load_ordered_json(profile_file)
    if config.get("schemaVersion") != 1 or set(config["profiles"]) != {"3.8.3", "4.1.5"}:
        raise ValueError("unexpected client-only prototype profile matrix")
    manifest = {"schemaVersion": 1, "profiles": {}}
    expected = {}
    for version, source_profile in config["profiles"].items():
        translation_version = source_profile["translationVersion"]
        # The two locale sets are intentionally fixed while the client-only approach is being tested.
        if translation_version != {"3.8.3": "3.8.3", "4.1.5": "4.1.3"}[version]:
            raise ValueError(f"unexpected translation source for SPT {version}")
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


def run_windows_contract(executable: Path, arguments: list[Path]) -> None:
    if os.name == "nt":
        command = [str(executable), *(str(path) for path in arguments)]
    elif shutil.which("wslpath"):
        # The game and its Harmony runtime target .NET Framework; execute that runtime through WSL interop.
        executable.chmod(executable.stat().st_mode | 0o111)
        def windows_path(path: Path) -> str:
            return subprocess.check_output(["wslpath", "-w", str(path.resolve())], text=True).strip()

        command = [str(executable), *(windows_path(path) for path in arguments)]
    else:
        raise RuntimeError("the client runtime contract requires Windows or WSL interop")
    subprocess.run(command, check=True)


def main() -> None:
    project_root = Path(__file__).resolve().parents[1]
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--translation-root", type=Path, default=project_root.parent / "spt-korean-translate")
    parser.add_argument("--dotnet")
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
    contract_report = work / "contract-verification.json"
    if contract_report.exists():
        contract_report.unlink()
    run_windows_contract(build / "ClientLocaleContract/ClientLocaleContract.exe", [
        stage.joinpath(*BUNDLE_ROOT.parts), args.spt_383_root, args.spt_415_root, contract_report,
    ])
    release.create_deterministic_zip(stage, archive)
    validate_archive(archive, expected)
    summary = {
        "kind": "client-only-prototype",
        "profiles": ["3.8.3", "4.1.5"],
        "runtime_contract": "passed: native reload fixture with actual Harmony and locale payloads",
        "contract_details": release.load_ordered_json(contract_report),
        "in_game_visual_validation": "not performed by this command",
        "archive": archive.name,
        "archive_sha256": release.sha256_file(archive),
        "files": expected,
    }
    summary_path.write_text(json.dumps(summary, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"Verified client-only prototype: {archive}")


if __name__ == "__main__":
    main()
