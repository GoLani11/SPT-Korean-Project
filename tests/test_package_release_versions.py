import json
import re
import tempfile
import unittest
from pathlib import Path
import sys


PROJECT_ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(PROJECT_ROOT / "tools"))

import package_release_versions as release  # noqa: E402


class ReleaseContractTests(unittest.TestCase):
    def test_release_matrix_contains_exactly_twelve_assets(self):
        self.assertEqual(
            [spec.version for spec in release.SUPPORTED_VERSIONS],
            ["3.8.3", "3.9.8", "3.10.5", "3.11.4", "4.0.13", "4.1.0"],
        )
        self.assertEqual(set(release.VARIANTS), {"KR", "KR-EN"})
        self.assertEqual(len(release.SUPPORTED_VERSIONS) * len(release.VARIANTS), 12)

    def test_node_manifests_target_only_the_exact_loader_version(self):
        for spec in release.SUPPORTED_VERSIONS[:4]:
            manifest = release.node_manifest(spec)
            self.assertEqual(manifest[spec.manifest_version_field], spec.version)
            other_field = "sptVersion" if spec.manifest_version_field == "akiVersion" else "akiVersion"
            self.assertNotIn(other_field, manifest)

    def test_locale_validation_requires_key_order_and_string_values(self):
        with tempfile.TemporaryDirectory() as temporary_directory:
            root = Path(temporary_directory)
            english = root / "en.json"
            matching = root / "matching.json"
            reordered = root / "reordered.json"
            invalid_value = root / "invalid-value.json"

            english.write_text(json.dumps({"first": "A", "second": "B"}), encoding="utf-8")
            matching.write_text(json.dumps({"first": "가", "second": "나"}), encoding="utf-8")
            reordered.write_text(json.dumps({"second": "나", "first": "가"}), encoding="utf-8")
            invalid_value.write_text(json.dumps({"first": "가", "second": 2}), encoding="utf-8")

            self.assertEqual(release.validate_locale_pair(english, matching), 2)
            with self.assertRaisesRegex(ValueError, "key set/order mismatch"):
                release.validate_locale_pair(english, reordered)
            with self.assertRaisesRegex(TypeError, "non-string values"):
                release.validate_locale_pair(english, invalid_value)

    def test_gesture_patch_covers_every_supported_client_enum_name(self):
        source = (
            PROJECT_ROOT
            / "src"
            / "ClientModFixPlugin"
            / "Patches"
            / "GesturesMenuFixFix.cs"
        ).read_text(encoding="utf-8")

        def read_map(field_name):
            body = source.split(f"{field_name} =", 1)[1].split("};", 1)[0]
            return dict(re.findall(r'\{ "([^"]+)", "([^"]+)" \}', body))

        self.assertEqual(
            read_map("PhraseLabels"),
            {
                "Look": "주의!",
                "Ready": "준비됐어!",
                "DontKnow": "모르겠어!",
            },
        )
        self.assertEqual(
            read_map("GestureLabels"),
            {
                "ThatDirection": "저기",
                "ThereGesture": "저기",
                "Stop": "멈춰!",
                "HoldGesture": "대기",
                "Hello": "인사",
                "FriendlyGesture": "인사",
                "FuckYou": "가운뎃손가락",
                "GetOffGesture": "가운뎃손가락",
                "Good": "엄지 척",
                "OkGesture": "엄지 척",
                "Bad": "엄지 내리기",
                "NoGesture": "엄지 내리기",
                "ComeToMe": "따라와",
                "ComeWithMeGesture": "따라와",
                "RockGesture": "바위",
                "ScissorGesture": "가위",
                "PaperGesture": "보",
                "AllRightGesture": "오케이",
            },
        )
        self.assertIn("GameLanguageDetector.IsKorean()", source)
        self.assertIn("PreserveBilingualSuffix(translated, text.text)", source)


if __name__ == "__main__":
    unittest.main()
