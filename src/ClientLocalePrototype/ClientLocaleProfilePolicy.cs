using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace KoreanPatchFix
{
    // Shared by the client and .NET console companion. Selecting a candidate does not
    // bypass EFT build, English source, payload integrity or client hook validation.
    internal static class ClientLocaleProfilePolicy
    {
        internal static string Select(string sptVersion, IEnumerable<string> availableProfiles)
        {
            if (sptVersion == null || !Regex.IsMatch(sptVersion, @"\A(?:0|[1-9][0-9]*)\.(?:0|[1-9][0-9]*)\.(?:0|[1-9][0-9]*)\z")
                || !Version.TryParse(sptVersion, out var version))
                throw new InvalidDataException("Cannot select a translation profile for an unknown or prerelease SPT version.");

            var profiles = availableProfiles.ToArray();
            if (profiles.Contains(sptVersion, StringComparer.Ordinal)) return sptVersion;

            // 4.1.0 has its own server API. Only stable patches in the 4.1.2+ family
            // may reuse the last verified installation's profile; never cross into 4.2.
            const string fallback = "4.1.5";
            if (version.Major == 4 && version.Minor == 1 && version.Build >= 2
                && profiles.Contains(fallback, StringComparer.Ordinal)) return fallback;

            throw new InvalidDataException($"No compatible translation profile for SPT {sptVersion}.");
        }
    }
}
