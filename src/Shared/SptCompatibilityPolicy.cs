using System;
using System.Collections.Generic;

namespace KoreanPatchFix
{
    internal static class SptCompatibilityPolicy
    {
#if SPT_410
        internal const string FourOneServerRange = "4.1.0";
        internal const string SupportedVersionsDescription =
            "3.8.3, 3.9.8, 3.10.5, 3.11.4, 4.0.13, and SPT 4.1.0 only";
#else
        internal const string FourOneServerRange = "~4.1.2";
        internal const string SupportedVersionsDescription =
            "3.8.3, 3.9.8, 3.10.5, 3.11.4, 4.0.13, and stable SPT 4.1.2 or newer 4.1.x releases only";
#endif

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

#if SPT_410
            return string.Equals(version, "4.1.0", StringComparison.Ordinal);
#else
            return Version.TryParse(version, out var parsed)
                && parsed.Major == 4
                && parsed.Minor == 1
                && parsed.Build >= 2;
#endif
        }
    }
}
