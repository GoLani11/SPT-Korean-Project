using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace KoreanPatchFix
{
    // Only the prototype references this file. Released server/client packages keep their existing behavior.
    internal sealed class ClientLocaleBundle
    {
        internal const string Korean = "kr";
        internal const string Bilingual = "kr-en";
        internal const string KoreanName = "한국어 (Korean)";
        internal const string BilingualName = "한국어 (한영 병기)";

        private readonly Dictionary<string, string> korean;
        private readonly Dictionary<string, string> bilingual;

        private ClientLocaleBundle(string sptVersion, string translationVersion,
            JObject koreanJson, JObject bilingualJson)
        {
            SptVersion = sptVersion;
            TranslationVersion = translationVersion;
            SourceKeyCount = koreanJson.Count;
            korean = FoldAliases(ReadEntries(koreanJson));
            bilingual = FoldAliases(ReadEntries(bilingualJson));
        }

        internal string SptVersion { get; }
        internal string TranslationVersion { get; }
        internal int SourceKeyCount { get; }

        internal static ClientLocaleBundle Load(string bundleRoot, string gameRoot,
            string sptVersion, string eftVersion)
        {
            var manifest = ReadObject(Path.Combine(bundleRoot, "manifest.json"));
            if ((int?)manifest["schemaVersion"] != 1)
            {
                throw new InvalidDataException("Unsupported client locale manifest schema.");
            }

            var profile = sptVersion == null ? null : manifest["profiles"]?[sptVersion] as JObject;
            if (profile == null)
            {
                throw new InvalidDataException($"This client-only prototype has no profile for SPT {sptVersion ?? "unknown"}.");
            }
            if (!string.Equals((string)profile["eftVersion"], eftVersion, StringComparison.Ordinal))
            {
                throw new InvalidDataException($"EFT build mismatch: expected {profile["eftVersion"]}, detected {eftVersion ?? "unknown"}.");
            }

            var translationVersion = (string)profile["translationVersion"];
            if (translationVersion == null || !Regex.IsMatch(translationVersion, @"\A\d+\.\d+\.\d+\z"))
            {
                throw new InvalidDataException("Invalid translation version in the client locale manifest.");
            }

            var files = new Dictionary<string, JObject>(StringComparer.Ordinal);
            foreach (var name in new[] { "en.json", "kr.json", "kr-en.json" })
            {
                var path = Path.Combine(bundleRoot, "locales", translationVersion, name);
                var bytes = File.ReadAllBytes(path);
                var expectedHash = (string)profile["sha256"]?[name];
                using (var hash = SHA256.Create())
                {
                    var actualHash = BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
                    if (!string.Equals(actualHash, expectedHash, StringComparison.Ordinal))
                    {
                        throw new InvalidDataException($"Client locale payload hash mismatch: {name}.");
                    }
                }
                files.Add(name, ReadObject(new StreamReader(new MemoryStream(bytes), Encoding.UTF8, true)));
            }

            var englishEntries = ReadEntries(files["en.json"]);
            var english = englishEntries.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
            foreach (var name in new[] { "kr.json", "kr-en.json" })
            {
                var entries = ReadEntries(files[name]);
                if (!entries.Select(entry => entry.Key).SequenceEqual(englishEntries.Select(entry => entry.Key)))
                {
                    throw new InvalidDataException($"Client locale key set/order mismatch: {name}.");
                }
            }

            // Compare JSON values, not whitespace or file bytes, against the unmodified server database.
            var relativeSource = (string)profile["englishPath"];
            var allowedSources = new[]
            {
                "Aki_Data/Server/database/locales/global/en.json",
                "SPT_Data/Server/database/locales/global/en.json",
                "SPT/SPT_Data/database/locales/global/en.json",
                "SPT_Runtime/SPT_Data/database/locales/global/en.json"
            };
            if (!allowedSources.Contains(relativeSource, StringComparer.Ordinal))
            {
                throw new InvalidDataException("Invalid installed English locale path in the client locale manifest.");
            }
            var installed = ReadEntries(ReadObject(Path.Combine(gameRoot, relativeSource.Replace('/', Path.DirectorySeparatorChar))))
                .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
            if (installed.Count != english.Count || english.Any(entry =>
                !installed.TryGetValue(entry.Key, out var value) || value != entry.Value))
            {
                throw new InvalidDataException("Installed English locale differs from the verified translation source; the prototype was not enabled.");
            }

            return new ClientLocaleBundle(sptVersion, translationVersion, files["kr.json"], files["kr-en.json"]);
        }

        internal Dictionary<string, string> MergeGlobal(string locale, IDictionary<string, string> source)
        {
            var result = FoldAliases(source);
            var patch = Is(locale, Korean) ? korean : Is(locale, Bilingual) ? bilingual : null;
            if (patch != null)
            {
                foreach (var entry in patch)
                {
                    result[entry.Key] = entry.Value;
                }
            }
            AddLanguageNames(result);
            return result;
        }

        internal static Dictionary<string, string> MenuWithNames(IDictionary<string, string> source)
        {
            var result = FoldAliases(source);
            AddLanguageNames(result);
            return result;
        }

        internal static Dictionary<string, string> WithBilingualLanguage(IDictionary<string, string> source)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            var inserted = false;
            foreach (var entry in source)
            {
                if (Is(entry.Key, Bilingual))
                {
                    continue;
                }
                result[entry.Key] = entry.Value;
                if (Is(entry.Key, Korean))
                {
                    result[Bilingual] = "Korean-English";
                    inserted = true;
                }
            }
            if (!inserted)
            {
                result[Korean] = "Korean";
                result[Bilingual] = "Korean-English";
            }
            return result;
        }

        internal static bool Is(string value, string locale)
        {
            return string.Equals(value, locale, StringComparison.OrdinalIgnoreCase);
        }

        private static void AddLanguageNames(IDictionary<string, string> values)
        {
            values[Korean] = KoreanName;
            values[Bilingual] = BilingualName;
        }

        private static Dictionary<string, string> FoldAliases(IEnumerable<KeyValuePair<string, string>> values)
        {
            // EFT.Locale's constructor rejects case aliases. Match SPT's last-value-wins overlay explicitly.
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in values)
            {
                result[entry.Key] = entry.Value;
            }
            return result;
        }

        private static KeyValuePair<string, string>[] ReadEntries(JObject json)
        {
            return json.Properties().Select(property =>
            {
                if (property.Value.Type != JTokenType.String)
                {
                    throw new InvalidDataException($"Non-string locale value: {property.Name}.");
                }
                return new KeyValuePair<string, string>(property.Name, (string)property.Value);
            }).ToArray();
        }

        private static JObject ReadObject(string path)
        {
            return ReadObject(new StreamReader(path, Encoding.UTF8, true));
        }

        private static JObject ReadObject(TextReader input)
        {
            using (input)
            using (var reader = new JsonTextReader(input) { DateParseHandling = DateParseHandling.None })
            {
                var value = JObject.Load(reader, new JsonLoadSettings { DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error });
                if (reader.Read())
                {
                    throw new InvalidDataException("Unexpected content after the client locale JSON object.");
                }
                return value;
            }
        }
    }
}
