from __future__ import annotations

import argparse
import hashlib
import json
import os
import shutil
import subprocess
import sys
import zipfile
from dataclasses import dataclass
from pathlib import Path, PurePosixPath
from typing import Any, Iterable


PACKAGE_VERSION = "2.0.0"
CLIENT_DLL_NAME = "GoLani.KoreanModFix.dll"
SERVER_DLL_NAME = "SPT_Korean_Localization.dll"
THREE_X_MOD_FOLDER = "spt_korean_localization_G&M"
FOUR_X_MOD_FOLDER = "SPT_Korean_Localization"


@dataclass(frozen=True)
class VersionSpec:
    version: str
    server_kind: str
    server_mod_root: PurePosixPath
    manifest_version_field: str | None = None

    @property
    def allowed_roots(self) -> set[str]:
        return {"BepInEx", self.server_mod_root.parts[0]}


SUPPORTED_VERSIONS = (
    VersionSpec(
        "3.8.3",
        "node",
        PurePosixPath("user/mods") / THREE_X_MOD_FOLDER,
        "akiVersion",
    ),
    VersionSpec(
        "3.9.8",
        "node",
        PurePosixPath("user/mods") / THREE_X_MOD_FOLDER,
        "sptVersion",
    ),
    VersionSpec(
        "3.10.5",
        "node",
        PurePosixPath("user/mods") / THREE_X_MOD_FOLDER,
        "sptVersion",
    ),
    VersionSpec(
        "3.11.4",
        "node",
        PurePosixPath("user/mods") / THREE_X_MOD_FOLDER,
        "sptVersion",
    ),
    VersionSpec(
        "4.0.13",
        "dotnet40",
        PurePosixPath("SPT/user/mods") / FOUR_X_MOD_FOLDER,
    ),
    VersionSpec(
        "4.1.0",
        "dotnet41",
        PurePosixPath("SPT_Runtime/user/mods") / FOUR_X_MOD_FOLDER,
    ),
)

VARIANTS = {
    "KR": "kr.generated.json",
    "KR-EN": "kr-en.generated.json",
}


class DuplicateJsonKeyError(ValueError):
    pass


def load_ordered_json(path: Path) -> dict[str, Any]:
    def reject_duplicates(pairs: list[tuple[str, Any]]) -> dict[str, Any]:
        result: dict[str, Any] = {}
        for key, value in pairs:
            if key in result:
                raise DuplicateJsonKeyError(f"duplicate JSON key {key!r} in {path}")
            result[key] = value
        return result

    with path.open("r", encoding="utf-8-sig") as handle:
        value = json.load(handle, object_pairs_hook=reject_duplicates)
    if not isinstance(value, dict):
        raise ValueError(f"expected a top-level JSON object: {path}")
    return value


def validate_locale_pair(english_path: Path, locale_path: Path) -> int:
    english = load_ordered_json(english_path)
    locale = load_ordered_json(locale_path)

    english_keys = list(english)
    locale_keys = list(locale)
    if locale_keys != english_keys:
        english_set = set(english_keys)
        locale_set = set(locale_keys)
        missing = [key for key in english_keys if key not in locale_set]
        extra = [key for key in locale_keys if key not in english_set]
        raise ValueError(
            f"locale key set/order mismatch for {locale_path}: "
            f"missing={missing[:5]}, extra={extra[:5]}, "
            f"same_set={english_set == locale_set}"
        )

    for source_name, values in (("English", english), ("locale", locale)):
        invalid = [key for key, value in values.items() if not isinstance(key, str) or not isinstance(value, str)]
        if invalid:
            raise TypeError(
                f"{source_name} JSON contains non-string values in {locale_path}: {invalid[:5]}"
            )

    return len(locale)


def sha256_bytes(value: bytes) -> str:
    return hashlib.sha256(value).hexdigest()


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def ensure_generated_output_path(path: Path, project_root: Path) -> Path:
    resolved = path.resolve()
    artifacts_root = (project_root / "artifacts").resolve()
    if resolved == artifacts_root or artifacts_root not in resolved.parents:
        raise ValueError(f"refusing to write outside the project artifacts directory: {resolved}")
    return resolved


def resolve_dotnet(dotnet_argument: str | None, project_root: Path) -> str:
    if dotnet_argument:
        return dotnet_argument

    local_sdk = project_root / "artifacts" / "dotnet10" / ("dotnet.exe" if os.name == "nt" else "dotnet")
    if local_sdk.is_file():
        return str(local_sdk)

    discovered = shutil.which("dotnet") or shutil.which("dotnet.exe")
    if discovered:
        return discovered

    if os.name == "nt":
        candidate = Path(os.environ.get("ProgramFiles", r"C:\Program Files")) / "dotnet" / "dotnet.exe"
        if candidate.is_file():
            return str(candidate)

    raise FileNotFoundError("dotnet SDK was not found; pass --dotnet with its full path")


def run_command(command: list[str], cwd: Path) -> None:
    print("+", subprocess.list2cmdline(command), flush=True)
    subprocess.run(command, cwd=cwd, check=True)


def build_solution(
    project_root: Path,
    configuration: str,
    client_reference_root: Path,
    dotnet: str,
) -> None:
    solution = project_root / "SPT-Korean-Project.sln"
    property_argument = f"-p:ClientReferenceSptRoot={client_reference_root}"
    run_command([dotnet, "restore", str(solution), property_argument, "-v:minimal"], project_root)
    run_command(
        [
            dotnet,
            "build",
            str(solution),
            "-c",
            configuration,
            "--no-restore",
            property_argument,
            "-v:minimal",
        ],
        project_root,
    )


def required_build_outputs(project_root: Path, configuration: str) -> dict[str, Path]:
    build_root = project_root / "artifacts" / "build" / configuration
    outputs = {
        "client": build_root / "ClientModFixPlugin" / CLIENT_DLL_NAME,
        "dotnet40": build_root / "ServerLocaleMod40" / SERVER_DLL_NAME,
        "dotnet41": build_root / "ServerLocaleMod41" / SERVER_DLL_NAME,
    }
    missing = [str(path) for path in outputs.values() if not path.is_file()]
    if missing:
        raise FileNotFoundError(f"required build outputs are missing: {missing}")
    return outputs


def node_manifest(spec: VersionSpec) -> dict[str, Any]:
    if spec.manifest_version_field is None:
        raise ValueError(f"missing manifest field for {spec.version}")

    return {
        "name": "SPT_Korean_Localization_(G&M)",
        "version": PACKAGE_VERSION,
        "description": "SPT Korean localization by Golani and Makina",
        spec.manifest_version_field: spec.version,
        "loadBefore": [],
        "loadAfter": [],
        "incompatibilities": [],
        "isBundleMod": False,
        "main": "src/mod.js",
        "author": "Golani, Makina",
        "contributors": [],
        "license": "MIT",
    }


def copy_dotnet_server_files(source_dll: Path, destination_root: Path) -> None:
    destination_root.mkdir(parents=True, exist_ok=True)
    shutil.copy2(source_dll, destination_root / SERVER_DLL_NAME)
    deps_path = source_dll.with_suffix(".deps.json")
    if deps_path.is_file():
        shutil.copy2(deps_path, destination_root / deps_path.name)


def stage_package(
    project_root: Path,
    work_root: Path,
    spec: VersionSpec,
    variant: str,
    locale_source: Path,
    build_outputs: dict[str, Path],
) -> Path:
    package_name = f"SPT_Korean_Localization.SPT-{spec.version}.{variant}.GM"
    package_root = work_root / package_name
    if package_root.exists():
        shutil.rmtree(package_root)

    client_destination = package_root / "BepInEx" / "plugins" / CLIENT_DLL_NAME
    client_destination.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(build_outputs["client"], client_destination)

    mod_root = package_root.joinpath(*spec.server_mod_root.parts)
    locale_destination = mod_root / "locale" / "kr.json"
    locale_destination.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(locale_source, locale_destination)

    if spec.server_kind == "node":
        source_mod = project_root / "src" / "ServerLocaleMod3" / "src" / "mod.js"
        destination_mod = mod_root / "src" / "mod.js"
        destination_mod.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(source_mod, destination_mod)
        (mod_root / "package.json").write_text(
            json.dumps(node_manifest(spec), ensure_ascii=False, indent=2) + "\n",
            encoding="utf-8",
        )
    else:
        copy_dotnet_server_files(build_outputs[spec.server_kind], mod_root)

    return package_root


def iter_package_files(root: Path) -> Iterable[Path]:
    return sorted(
        (path for path in root.rglob("*") if path.is_file()),
        key=lambda path: path.relative_to(root).as_posix(),
    )


def create_deterministic_zip(source_root: Path, zip_path: Path) -> None:
    if zip_path.exists():
        zip_path.unlink()

    with zipfile.ZipFile(
        zip_path,
        "w",
        compression=zipfile.ZIP_DEFLATED,
        compresslevel=9,
    ) as archive:
        for source_path in iter_package_files(source_root):
            archive_name = source_path.relative_to(source_root).as_posix()
            info = zipfile.ZipInfo(archive_name, date_time=(2024, 1, 1, 0, 0, 0))
            info.compress_type = zipfile.ZIP_DEFLATED
            info.external_attr = 0o100644 << 16
            archive.writestr(info, source_path.read_bytes(), compresslevel=9)


def validate_archive(zip_path: Path, spec: VersionSpec, locale_source: Path) -> None:
    expected_mod_prefix = spec.server_mod_root.as_posix() + "/"
    expected_locale_name = expected_mod_prefix + "locale/kr.json"
    expected_client_name = "BepInEx/plugins/" + CLIENT_DLL_NAME

    with zipfile.ZipFile(zip_path, "r") as archive:
        entries = archive.infolist()
        names = [entry.filename.replace("\\", "/") for entry in entries]
        if not names:
            raise ValueError(f"archive is empty: {zip_path}")
        if len(names) != len(set(names)):
            raise ValueError(f"archive contains duplicate paths: {zip_path}")

        for name in names:
            pure = PurePosixPath(name)
            if pure.is_absolute() or ".." in pure.parts or not pure.parts:
                raise ValueError(f"unsafe archive entry in {zip_path}: {name}")
            if pure.parts[0] not in spec.allowed_roots:
                raise ValueError(f"unexpected root folder in {zip_path}: {name}")
            if pure.suffix.lower() in {".bat", ".cmd", ".exe"}:
                raise ValueError(f"installer/executable is forbidden in release archive: {name}")

        if expected_client_name not in names:
            raise ValueError(f"client plugin is missing from {zip_path}")
        if expected_locale_name not in names:
            raise ValueError(f"version locale is missing from {zip_path}")
        if not any(name.startswith(expected_mod_prefix) for name in names):
            raise ValueError(f"server mod folder is missing from {zip_path}")

        locale_entries = [name for name in names if name.endswith("/locale/kr.json")]
        if locale_entries != [expected_locale_name]:
            raise ValueError(f"archive must contain exactly one locale payload: {locale_entries}")

        packaged_locale = archive.read(expected_locale_name)
        if sha256_bytes(packaged_locale) != sha256_file(locale_source):
            raise ValueError(f"packaged locale hash does not match its source: {zip_path}")

        if spec.server_kind == "node":
            manifest_name = expected_mod_prefix + "package.json"
            mod_name = expected_mod_prefix + "src/mod.js"
            if manifest_name not in names or mod_name not in names:
                raise ValueError(f"3.x server mod files are missing from {zip_path}")
            manifest = json.loads(archive.read(manifest_name).decode("utf-8"))
            if manifest.get(spec.manifest_version_field) != spec.version:
                raise ValueError(f"3.x manifest does not target exactly {spec.version}: {zip_path}")
            other_field = "sptVersion" if spec.manifest_version_field == "akiVersion" else "akiVersion"
            if other_field in manifest:
                raise ValueError(f"3.x manifest contains an unrelated version field: {zip_path}")
            expected_files = {
                expected_client_name,
                expected_locale_name,
                manifest_name,
                mod_name,
            }
        else:
            server_dll_name = expected_mod_prefix + SERVER_DLL_NAME
            server_deps_name = expected_mod_prefix + Path(SERVER_DLL_NAME).with_suffix(".deps.json").name
            if server_dll_name not in names:
                raise ValueError(f"4.x server DLL is missing from {zip_path}")
            if server_deps_name not in names:
                raise ValueError(f"4.x server dependency manifest is missing from {zip_path}")
            expected_files = {
                expected_client_name,
                expected_locale_name,
                server_dll_name,
                server_deps_name,
            }

        if set(names) != expected_files:
            unexpected = sorted(set(names) - expected_files)
            missing = sorted(expected_files - set(names))
            raise ValueError(
                f"archive contains an unexpected payload for {spec.version}: "
                f"unexpected={unexpected}, missing={missing}"
            )


def package_all(
    project_root: Path,
    translation_root: Path,
    output_root: Path,
    work_root: Path,
    build_outputs: dict[str, Path],
) -> list[dict[str, Any]]:
    summary: list[dict[str, Any]] = []

    for spec in SUPPORTED_VERSIONS:
        english_path = translation_root / "versions" / spec.version / "input" / "en.json"
        if not english_path.is_file():
            raise FileNotFoundError(f"English source is missing for SPT {spec.version}: {english_path}")

        for variant, locale_filename in VARIANTS.items():
            locale_source = translation_root / "output" / spec.version / locale_filename
            if not locale_source.is_file():
                raise FileNotFoundError(
                    f"generated locale is missing for SPT {spec.version} {variant}: {locale_source}"
                )

            key_count = validate_locale_pair(english_path, locale_source)
            package_root = stage_package(
                project_root,
                work_root,
                spec,
                variant,
                locale_source,
                build_outputs,
            )
            zip_path = output_root / f"{package_root.name}.zip"
            create_deterministic_zip(package_root, zip_path)
            validate_archive(zip_path, spec, locale_source)
            summary.append(
                {
                    "version": spec.version,
                    "variant": variant,
                    "file": zip_path.name,
                    "keys": key_count,
                    "sha256": sha256_file(zip_path),
                }
            )
            print(f"created {zip_path.name} ({key_count} keys)")

    if len(summary) != 12:
        raise AssertionError(f"expected 12 release archives, created {len(summary)}")
    return summary


def parse_args(argv: list[str]) -> argparse.Namespace:
    project_root = Path(__file__).resolve().parents[1]
    default_translation_root = project_root.parent / "spt-korean-translate"
    parser = argparse.ArgumentParser(
        description="Build and validate the 12 version-specific SPT Korean localization ZIP files."
    )
    parser.add_argument("--translation-root", type=Path, default=default_translation_root)
    parser.add_argument("--client-reference-spt-root", type=Path, default=Path(r"D:\SPT3.8.3"))
    parser.add_argument("--configuration", default="Release")
    parser.add_argument("--output-root", type=Path, default=project_root / "artifacts" / "release")
    parser.add_argument("--dotnet")
    parser.add_argument("--skip-build", action="store_true")
    return parser.parse_args(argv)


def main(argv: list[str] | None = None) -> int:
    args = parse_args(argv or sys.argv[1:])
    project_root = Path(__file__).resolve().parents[1]
    translation_root = args.translation_root.resolve()
    output_root = ensure_generated_output_path(args.output_root, project_root)
    work_root = ensure_generated_output_path(project_root / "artifacts" / "package-work", project_root)

    if not translation_root.is_dir():
        raise FileNotFoundError(f"translation repository was not found: {translation_root}")
    if not args.client_reference_spt_root.is_dir():
        raise FileNotFoundError(
            f"SPT 3.8.3 client reference install was not found: {args.client_reference_spt_root}"
        )

    if output_root.exists():
        shutil.rmtree(output_root)
    if work_root.exists():
        shutil.rmtree(work_root)
    output_root.mkdir(parents=True, exist_ok=True)
    work_root.mkdir(parents=True, exist_ok=True)

    if not args.skip_build:
        build_solution(
            project_root,
            args.configuration,
            args.client_reference_spt_root.resolve(),
            resolve_dotnet(args.dotnet, project_root),
        )

    build_outputs = required_build_outputs(project_root, args.configuration)
    summary = package_all(
        project_root,
        translation_root,
        output_root,
        work_root,
        build_outputs,
    )
    summary_path = output_root / "release-summary.json"
    summary_path.write_text(
        json.dumps(summary, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )
    shutil.rmtree(work_root)
    print(f"validated 12 release archives; summary: {summary_path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
