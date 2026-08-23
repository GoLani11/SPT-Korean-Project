using System;
using System.Collections.Generic;

namespace KoreanPatchFix
{
    internal static class SptCompatibilityPolicy
    {
        internal const string FourOneServerRange = "~4.1.2";

        private static readonly HashSet<string> ExactSupportedVersions =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "3.8.3",
                "3.9.8",
                "3.10.5",
                "3.11.4",
                "4.0.13"
            };

        internal static bool IsSupportedStableRelease(string version)
        {
            if (version == null)
            {
                return false;
            }

            if (ExactSupportedVersions.Contains(version))
            {
                return true;
            }

            return Version.TryParse(version, out var parsed)
                && parsed.Major == 4
                && parsed.Minor == 1
                && parsed.Build >= 2;
        }
    }
}
