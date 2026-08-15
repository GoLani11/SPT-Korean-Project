import json
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


if __name__ == "__main__":
    unittest.main()
