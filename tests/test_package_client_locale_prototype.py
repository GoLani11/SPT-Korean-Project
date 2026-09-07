import hashlib
import json
import sys
import tempfile
import unittest
import zipfile
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "tools"))
import package_client_locale_prototype as prototype


class ClientPrototypePackageTests(unittest.TestCase):
    def test_stage_keeps_version_specific_values_and_hashes(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            for version, text in [("3.8.3", "22:00-05:00"), ("4.1.3", "21:00-06:00")]:
                english = root / "source" / "versions" / version / "input" / "en.json"
                english.parent.mkdir(parents=True)
                english.write_text(json.dumps({"quest": text}), encoding="utf-8")
                output = root / "source" / "output" / version
                output.mkdir(parents=True)
                for mode in ("kr", "kr-en"):
                    (output / f"{mode}.generated.json").write_text(json.dumps({"quest": mode + text}), encoding="utf-8")
            expected = prototype.stage_payloads(root / "source", root / "stage")
            self.assertEqual(len(expected), 7)
            for name, digest in expected.items():
                self.assertEqual(hashlib.sha256((root / "stage" / name).read_bytes()).hexdigest(), digest)
            manifest = json.loads((root / "stage/BepInEx/plugins/SPT-Korean/manifest.json").read_text())
            self.assertEqual(manifest["profiles"]["4.1.5"]["translationVersion"], "4.1.3")
            self.assertNotEqual(
                manifest["profiles"]["3.8.3"]["sha256"]["kr.json"],
                manifest["profiles"]["4.1.5"]["sha256"]["kr.json"],
            )
            (root / "source/output/3.8.3/kr.generated.json").write_text('{"wrong-key":"text"}', encoding="utf-8")
            with self.assertRaisesRegex(ValueError, "key set/order mismatch"):
                prototype.stage_payloads(root / "source", root / "stage-invalid")

    def test_archive_rejects_installer_server_paths_and_corrupt_payloads(self):
        with tempfile.TemporaryDirectory() as temporary:
            archive = Path(temporary) / "prototype.zip"
            valid_name = "BepInEx/plugins/GoLani.KoreanModFix.dll"
            digest = hashlib.sha256(b"payload").hexdigest()
            for name in ["../escape.dll", "SPT_Runtime/user/mods/old.dll", "BepInEx/plugins/setup.exe", "BepInEx\\plugins\\x.dll"]:
                with zipfile.ZipFile(archive, "w") as package:
                    package.writestr(name, b"payload")
                with self.assertRaises(ValueError):
                    prototype.validate_archive(archive, {name: digest})
            with zipfile.ZipFile(archive, "w") as package:
                package.writestr(valid_name, b"payload")
            prototype.validate_archive(archive, {valid_name: digest})
            with self.assertRaisesRegex(ValueError, "hash mismatch"):
                prototype.validate_archive(archive, {valid_name: "0" * 64})


if __name__ == "__main__":
    unittest.main()
