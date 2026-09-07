using System.Diagnostics;
using System.Globalization;

namespace KoreanPatchFix
{
    internal static class ClientLocaleBuild
    {
        internal static string ReadEftVersion(string executable)
        {
            var info = FileVersionInfo.GetVersionInfo(executable);
            // Unity Mono can truncate the localized FileVersion string (40743 becomes 4074).
            // The PE's fixed numeric version fields retain the complete build number.
            return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}.{3}",
                info.FileMajorPart, info.FileMinorPart, info.FileBuildPart, info.FilePrivatePart);
        }
    }
}
