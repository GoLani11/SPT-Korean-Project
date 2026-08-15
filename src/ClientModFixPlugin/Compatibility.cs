using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace KoreanPatchFix
{
    internal readonly struct PatchResult
    {
        private PatchResult(bool enabled, string detail)
        {
            Enabled = enabled;
            Detail = detail;
        }

        internal bool Enabled { get; }
        internal string Detail { get; }

        internal static PatchResult Applied(MethodBase method)
        {
            return new PatchResult(true, $"{method.DeclaringType?.FullName}.{method.Name}");
        }

        internal static PatchResult Unavailable(string detail)
        {
            return new PatchResult(false, detail);
        }
    }

    internal static class SptVersionDetector
    {
        private static readonly HashSet<string> SupportedVersions = new HashSet<string>(StringComparer.Ordinal)
        {
            "3.8.3",
            "3.9.8",
            "3.10.5",
            "3.11.4",
            "4.0.13",
            "4.1.0"
        };

        private static readonly Regex JsonVersionPattern = new Regex(
            "\\\"(?:akiVersion|sptVersion)\\\"\\s*:\\s*\\\"(?<version>\\d+\\.\\d+\\.\\d+)\\\"",
            RegexOptions.CultureInvariant);

        private static readonly Regex SemanticVersionPattern = new Regex(
            "(?<version>\\d+\\.\\d+\\.\\d+)",
            RegexOptions.CultureInvariant);

        internal static string Detect()
        {
            var root = AppDomain.CurrentDomain.BaseDirectory;
            foreach (var relativePath in new[]
            {
                Path.Combine("Aki_Data", "Server", "configs", "core.json"),
                Path.Combine("SPT_Data", "Server", "configs", "core.json")
            })
            {
                var version = ReadJsonVersion(Path.Combine(root, relativePath));
                if (version != null)
                {
                    return version;
                }
            }

            foreach (var relativePath in new[]
            {
                Path.Combine("SPT", "SPT.Server.exe"),
                Path.Combine("SPT_Runtime", "SPT.Server.exe"),
                "SPT.Server.exe",
                "Aki.Server.exe"
            })
            {
                var version = ReadExecutableVersion(Path.Combine(root, relativePath));
                if (version != null)
                {
                    return version;
                }
            }

            return null;
        }

        internal static bool IsSupported(string version)
        {
            return version != null && SupportedVersions.Contains(version);
        }

        private static string ReadJsonVersion(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                var match = JsonVersionPattern.Match(File.ReadAllText(path));
                return match.Success ? match.Groups["version"].Value : null;
            }
            catch (Exception ex)
            {
                PluginLog.Warning($"Could not read SPT version file '{path}': {ex.Message}");
                return null;
            }
        }

        private static string ReadExecutableVersion(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                var info = FileVersionInfo.GetVersionInfo(path);
                var candidate = string.IsNullOrWhiteSpace(info.FileVersion) ? info.ProductVersion : info.FileVersion;
                var match = SemanticVersionPattern.Match(candidate ?? string.Empty);
                return match.Success ? match.Groups["version"].Value : null;
            }
            catch (Exception ex)
            {
                PluginLog.Warning($"Could not read SPT server version from '{path}': {ex.Message}");
                return null;
            }
        }
    }

    internal static class GameLanguageDetector
    {
        private static bool _warningLogged;

        internal static bool IsKorean()
        {
            try
            {
                var singletonType = ReflectionTools.FindType("Comfort.Common.Singleton`1");
                var settingsManagerType = ReflectionTools.FindType("EFT.Settings.SettingsManager");
                if (singletonType == null || settingsManagerType == null)
                {
                    WarnOnce("Could not resolve the game language; applying Korean UI adjustments as a fallback.");
                    return true;
                }

                var closedSingleton = singletonType.MakeGenericType(settingsManagerType);
                var instance = closedSingleton.GetProperty(
                    "Instance",
                    BindingFlags.Public | BindingFlags.Static)?.GetValue(null, null);
                var game = ReflectionTools.ReadMember(instance, "Game");
                var settings = ReflectionTools.ReadMember(game, "Settings");
                var language = ReflectionTools.ReadMember(settings, "Language");
                var value = ReflectionTools.ReadMember(language, "Value") as string;

                if (value == null)
                {
                    WarnOnce("Could not read the game language; applying Korean UI adjustments as a fallback.");
                    return true;
                }

                return string.Equals(value, "kr", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                WarnOnce($"Could not detect the game language; applying Korean UI adjustments: {ex.Message}");
                return true;
            }
        }

        private static void WarnOnce(string message)
        {
            if (_warningLogged)
            {
                return;
            }

            _warningLogged = true;
            PluginLog.Warning(message);
        }
    }

    internal static class ReflectionTools
    {
        private const BindingFlags InstanceMembers =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        internal static Type FindType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName, false);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        internal static MethodInfo FindMethod(Type type, string name, Func<MethodInfo, bool> predicate = null)
        {
            if (type == null)
            {
                return null;
            }

            return type.GetMethods(InstanceMembers)
                .Where(method => method.Name == name && MatchesSafely(method, predicate))
                .OrderByDescending(GetParameterCountSafely)
                .FirstOrDefault();
        }

        private static bool MatchesSafely(MethodInfo method, Func<MethodInfo, bool> predicate)
        {
            if (predicate == null)
            {
                return true;
            }

            try
            {
                return predicate(method);
            }
            catch (Exception ex) when (ex is TypeLoadException || ex is FileLoadException)
            {
                PluginLog.Warning($"Skipped incompatible method signature '{method.DeclaringType?.FullName}.{method.Name}': {ex.Message}");
                return false;
            }
        }

        private static int GetParameterCountSafely(MethodInfo method)
        {
            try
            {
                return method.GetParameters().Length;
            }
            catch (Exception ex) when (ex is TypeLoadException || ex is FileLoadException)
            {
                PluginLog.Warning($"Could not inspect method signature '{method.DeclaringType?.FullName}.{method.Name}': {ex.Message}");
                return -1;
            }
        }

        internal static FieldInfo FindField(Type type, string name)
        {
            while (type != null)
            {
                var field = type.GetField(name, InstanceMembers | BindingFlags.DeclaredOnly);
                if (field != null)
                {
                    return field;
                }

                type = type.BaseType;
            }

            return null;
        }

        internal static IEnumerable<FieldInfo> GetInstanceFields(Type type)
        {
            while (type != null)
            {
                foreach (var field in type.GetFields(InstanceMembers | BindingFlags.DeclaredOnly))
                {
                    yield return field;
                }

                type = type.BaseType;
            }
        }

        internal static object ReadMember(object instance, string name)
        {
            if (instance == null)
            {
                return null;
            }

            var type = instance.GetType();
            var property = type.GetProperty(name, InstanceMembers);
            if (property != null && property.GetIndexParameters().Length == 0)
            {
                return property.GetValue(instance, null);
            }

            return FindField(type, name)?.GetValue(instance);
        }
    }
}
