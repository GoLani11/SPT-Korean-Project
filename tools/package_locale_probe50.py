"""Build and verify the isolated SPT 5.0 IL2CPP locale probe using local references."""
from __future__ import annotations

import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import zipfile

ROOT = Path(__file__).resolve().parents[1]
DLL_NAME = "GoLani.KoreanLocaleProbe50.dll"


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--spt-root", type=Path, default=Path(r"D:\SPT_5.0_TEST" if os.name == "nt" else "/mnt/d/SPT_5.0_TEST"))
    parser.add_argument("--dotnet", default=shutil.which("dotnet") or "/mnt/c/Program Files/dotnet/dotnet.exe")
    args = parser.parse_args()
    spt_root = args.spt_root.resolve()
    windows_tool = os.name != "nt" and str(args.dotnet).lower().endswith(".exe")

    def native(path: Path) -> str:
        return subprocess.check_output(["wslpath", "-w", str(path)], text=True).strip() if windows_tool else str(path)

    def run(*arguments: str) -> str:
        result = subprocess.run([args.dotnet, *arguments], cwd=ROOT, text=True, capture_output=True)
        print(result.stdout, end="")
        if result.stderr:
            print(result.stderr, end="")
        if result.returncode:
            raise SystemExit(result.returncode)
        return result.stdout

    source = ROOT / "src/ClientLocaleProbe50"
    config = native(source / "NuGet.Config")
    run("build", native(source / "GoLani.KoreanLocaleProbe50.csproj"), "-c", "Release",
        "--configfile", config, "--nologo", "-v", "minimal", "-p:SptRoot=" + native(spt_root))
    dll = source / "bin/Release/net6.0" / DLL_NAME
    contract = ROOT / "tests/LocaleProbe50Contract/LocaleProbe50Contract.csproj"
    run("build", native(contract), "-c", "Release", "--configfile", config,
        "--nologo", "-v", "minimal", "-p:SptRoot=" + native(spt_root))
    contract_output = run(native(contract.parent / "bin/Release/net9.0/LocaleProbe50Contract.dll"), native(spt_root), native(dll))
    if "KR5 CONTRACT PASS:" not in contract_output:
        raise SystemExit("Contract success marker was not returned; no package created.")

    out = ROOT / "artifacts/locale-probe50"
    out.mkdir(parents=True, exist_ok=True)
    archive = out / "SPT-KR5-Probe-0.1.0.zip"
    prefix = "BepInEx/plugins/GoLani.KoreanLocaleProbe50/"
    payloads = {
        prefix + DLL_NAME: dll.read_bytes(),
        prefix + "README.md": (ROOT / "docs/client-locale-probe-5.0.md").read_bytes(),
        prefix + "LICENSE.md": (ROOT / "LICENSE.md").read_bytes(),
    }
    with zipfile.ZipFile(archive, "w", zipfile.ZIP_DEFLATED) as package:
        for name, value in payloads.items():
            package.writestr(name, value)
    with zipfile.ZipFile(archive) as package:
        assert set(package.namelist()) == set(payloads)
        assert package.testzip() is None
        for name, value in payloads.items():
            assert package.read(name) == value
    digest = hashlib.sha256(archive.read_bytes()).hexdigest()
    (out / "SHA256SUMS.txt").write_text(f"{digest}  {archive.name}\n", encoding="utf-8")
    report = {
        "plugin_version": "0.1.0",
        "target_spt_package": "5.0.0-BLEEDINGEDGEMODS+ec15a40.20260914",
        "target_eft": "1.1.5.0.47242",
        "contract": contract_output.strip(),
        "native_game_execution": "pending",
        "visual_verification": "pending",
        "installed_into_game": False,
        "zip_sha256": digest,
        "files": {name: hashlib.sha256(value).hexdigest() for name, value in payloads.items()},
    }
    (out / "verification.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(f"Verified probe package: {archive}")
    print("Not installed. Native game execution and visual verification remain pending.")


if __name__ == "__main__":
    main()
