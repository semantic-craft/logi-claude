"""Existing release bytes and failed attempts must survive subsequent pack calls."""
import importlib.util
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

SPEC = importlib.util.spec_from_file_location('package_release', Path(__file__).resolve().parents[2] / 'tools/package/package_release.py')
package = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(package)

class PackageImmutabilityTests(unittest.TestCase):
    def test_existing_artifact_or_report_rejects_before_tool_and_preserves_bytes(self):
        for existing in ('artifact', 'report'):
            with self.subTest(existing=existing), tempfile.TemporaryDirectory() as directory:
                root = Path(directory)
                artifact, report = root / 'candidate.lplug4', root / 'candidate.report.json'
                target = artifact if existing == 'artifact' else report
                original = b'immutable historical bytes\x00\xff'
                target.write_bytes(original)
                with patch.object(package, 'ARTIFACTS', root), patch.object(package, 'ARTIFACT', artifact), patch.object(package, 'REPORT', report), patch('sys.argv', ['pack', '--tool', __file__, '--release-dir', directory]), patch.object(package.subprocess, 'run') as run:
                    with self.assertRaisesRegex(SystemExit, 'refusing to overwrite'):
                        package.main()
                    run.assert_not_called()
                self.assertEqual(original, target.read_bytes())
                self.assertFalse((root / 'candidate.lplug4.attempt').exists())

    def test_failed_pack_retains_partial_bytes_and_prevents_retry(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            artifact, report = root / 'candidate.lplug4', root / 'candidate.report.json'
            def fail(*args, **kwargs):
                artifact.write_bytes(b'partial output')
                raise RuntimeError('pack failed')
            with patch.object(package, 'ARTIFACTS', root), patch.object(package, 'ARTIFACT', artifact), patch.object(package, 'REPORT', report), patch('sys.argv', ['pack', '--tool', __file__, '--release-dir', directory]), patch.object(package, 'copy_allowlisted_stage', return_value=set()), patch.object(package.subprocess, 'run', side_effect=fail) as run:
                with self.assertRaisesRegex(RuntimeError, 'pack failed'):
                    package.main()
                with self.assertRaisesRegex(SystemExit, 'refusing to overwrite'):
                    package.main()
                self.assertEqual(1, run.call_count)
                self.assertEqual(b'partial output', artifact.read_bytes())
                self.assertTrue((root / 'candidate.lplug4.attempt').is_dir())

    def test_prior_attempt_without_output_rejects_before_tool(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            artifact = root / 'candidate.lplug4'
            (root / 'candidate.lplug4.attempt').mkdir()
            with patch.object(package, 'ARTIFACTS', root), patch.object(package, 'ARTIFACT', artifact), patch.object(package, 'REPORT', root / 'report'), patch('sys.argv', ['pack', '--tool', __file__, '--release-dir', directory]), patch.object(package.subprocess, 'run') as run:
                with self.assertRaisesRegex(SystemExit, 'already attempted'):
                    package.main()
                run.assert_not_called()

if __name__ == '__main__':
    unittest.main()
