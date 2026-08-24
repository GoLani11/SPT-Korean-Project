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
    def test_release_matrix_contains_exactly_fourteen_assets(self):
        self.assertEqual(
            [spec.version for spec in release.SUPPORTED_VERSIONS],
            [
                "3.8.3",
                "3.9.8",
                "3.10.5",
                "3.11.4",
                "4.0.13",
                "4.1.0",
                "4.1.2-4.1.3",
            ],
        )
        self.assertEqual(set(release.VARIANTS), {"KR", "KR-EN"})
        self.assertEqual(release.EXPECTED_RELEASE_ARCHIVES, 14)
        self.assertEqual(
            len(release.SUPPORTED_VERSIONS) * len(release.VARIANTS),
            release.EXPECTED_RELEASE_ARCHIVES,
        )
        self.assertEqual(release.SUPPORTED_VERSIONS[-2].server_kind, "dotnet410")
        self.assertEqual(release.SUPPORTED_VERSIONS[-2].client_kind, "client410")
        self.assertEqual(release.SUPPORTED_VERSIONS[-1].locale_source_version, "4.1.3")
        self.assertEqual(
            release.SUPPORTED_VERSIONS[-1].compatible_translation_versions,
            ("4.1.2",),
        )

        self.assertEqual(
            [
                release.release_package_name(spec, variant) + ".zip"
                for spec in release.SUPPORTED_VERSIONS
                for variant in release.VARIANTS
            ],
            [
                "SPT-KR-3.8.3.zip",
                "SPT-KR-EN-3.8.3.zip",
                "SPT-KR-3.9.8.zip",
                "SPT-KR-EN-3.9.8.zip",
                "SPT-KR-3.10.5.zip",
                "SPT-KR-EN-3.10.5.zip",
                "SPT-KR-3.11.4.zip",
                "SPT-KR-EN-3.11.4.zip",
                "SPT-KR-4.0.13.zip",
                "SPT-KR-EN-4.0.13.zip",
                "SPT-KR-4.1.0.zip",
                "SPT-KR-EN-4.1.0.zip",
                "SPT-KR-4.1.2-4.1.3.zip",
                "SPT-KR-EN-4.1.2-4.1.3.zip",
            ],
        )

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

    def test_shared_package_sources_require_equal_values_and_order(self):
        with tempfile.TemporaryDirectory() as temporary_directory:
            root = Path(temporary_directory)
            reference = root / "reference.json"
            matching = root / "matching.json"
            changed = root / "changed.json"
            reordered = root / "reordered.json"

            reference.write_text(json.dumps({"first": "A", "second": "B"}), encoding="utf-8")
            matching.write_text(json.dumps({"first": "A", "second": "B"}), encoding="utf-8")
            changed.write_text(json.dumps({"first": "A", "second": "C"}), encoding="utf-8")
            reordered.write_text(json.dumps({"second": "B", "first": "A"}), encoding="utf-8")

            release.validate_equivalent_json(reference, matching, "test payload")
            with self.assertRaisesRegex(ValueError, "differs between shared package sources"):
                release.validate_equivalent_json(reference, changed, "test payload")
            with self.assertRaisesRegex(ValueError, "differs between shared package sources"):
                release.validate_equivalent_json(reference, reordered, "test payload")

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

    def test_four_one_compatibility_has_separate_4_1_0_and_shared_targets(self):
        policy_source = (
            PROJECT_ROOT / "src" / "Shared" / "SptCompatibilityPolicy.cs"
        ).read_text(encoding="utf-8")
        self.assertIn("#if SPT_410", policy_source)
        self.assertIn('FourOneServerRange = "4.1.0"', policy_source)
        self.assertIn('FourOneServerRange = "~4.1.2"', policy_source)
        self.assertIn('string.Equals(version, "4.1.0"', policy_source)
        self.assertIn("Version.TryParse(version, out var parsed)", policy_source)
        self.assertIn("parsed.Major == 4", policy_source)
        self.assertIn("parsed.Minor == 1", policy_source)
        self.assertIn("parsed.Build >= 2", policy_source)

        compatibility_source = (
            PROJECT_ROOT / "src" / "ClientModFixPlugin" / "Compatibility.cs"
        ).read_text(encoding="utf-8")
        self.assertIn("SptCompatibilityPolicy.IsSupportedStableRelease(version)", compatibility_source)

        server_source = (
            PROJECT_ROOT / "src" / "ServerLocaleMod" / "KoreanPatcher.cs"
        ).read_text(encoding="utf-8")
        self.assertIn("SptCompatibilityPolicy.FourOneServerRange", server_source)

        server_project = (
            PROJECT_ROOT / "src" / "ServerLocaleMod" / "SPT_Korean_Localization.csproj"
        ).read_text(encoding="utf-8-sig")
        self.assertIn("SptCompatibilityPolicy.cs", server_project)
        for package in ("SPTarkov.Common", "SPTarkov.DI", "SPTarkov.Server.Core"):
            self.assertIn(f'Include="{package}" Version="4.1.2"', server_project)

        server_410_project = (
            PROJECT_ROOT
            / "src"
            / "ServerLocaleMod410"
            / "SPT_Korean_Localization.4.1.0.csproj"
        ).read_text(encoding="utf-8")
        self.assertIn("SPT_410", server_410_project)
        for package in ("SPTarkov.Common", "SPTarkov.DI", "SPTarkov.Server.Core"):
            self.assertIn(f'Include="{package}" Version="4.1.0"', server_410_project)

        client_project = (
            PROJECT_ROOT / "src" / "ClientModFixPlugin" / "GoLani.KoreanModFix.csproj"
        ).read_text(encoding="utf-8")
        self.assertIn("SptCompatibilityPolicy.cs", client_project)

        client_410_project = (
            PROJECT_ROOT
            / "src"
            / "ClientModFixPlugin410"
            / "GoLani.KoreanModFix.4.1.0.csproj"
        ).read_text(encoding="utf-8")
        self.assertIn("SPT_410", client_410_project)
        self.assertIn("ClientModFixPlugin\\*.cs", client_410_project)

        contract_source = (
            PROJECT_ROOT / "tests" / "CompatibilityContract" / "Program.cs"
        ).read_text(encoding="utf-8")
        for case in ('("4.1.0", true)', '("4.1.2", false)', '("4.1.2", true)', '("4.2.0", false)'):
            self.assertIn(case, contract_source)

        contract_410_project = (
            PROJECT_ROOT
            / "tests"
            / "CompatibilityContract410"
            / "CompatibilityContract410.csproj"
        ).read_text(encoding="utf-8")
        self.assertIn("SPT_410", contract_410_project)

        packaging_source = (PROJECT_ROOT / "tools" / "package_release_versions.py").read_text(
            encoding="utf-8"
        )
        self.assertIn("run_compatibility_contract", packaging_source)
        self.assertIn('("CompatibilityContract", "CompatibilityContract410")', packaging_source)
        self.assertNotIn("--skip-build", packaging_source)


if __name__ == "__main__":
    unittest.main()
