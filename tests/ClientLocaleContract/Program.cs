using HarmonyLib;
using KoreanPatchFix;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

internal static class Program
{
    private static int assertions;
    private static readonly JArray probes = new JArray();

    public static int Main(string[] args)
    {
        Console.OutputEncoding = new UTF8Encoding(false);
        try
        {
            if (args.Length != 4) { throw new ArgumentException("Expected bundle root, SPT 3.8.3 root, SPT 4.1.5 root, and report path."); }
            Run(args).GetAwaiter().GetResult();
            File.WriteAllText(args[3], new JObject
            {
                ["assertions"] = assertions,
                ["harmonyRuntime"] = "Windows .NET Framework; native reload fixture",
                ["installedClientProbes"] = probes,
                ["inGameVisualValidation"] = false
            }.ToString(), new UTF8Encoding(false));
            Console.WriteLine($"Client locale contract passed: {assertions} assertions, real payloads, both native reload behaviors.");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
    }

    private static async Task Run(string[] args)
    {
        foreach (var profile in new[]
        {
            new[] { "3.8.3", "3.8.3", "0.14.1.29197", args[1], "22:00~05:00" },
            new[] { "4.1.5", "4.1.3", "0.16.9.40743", args[2], "21:00-06:00" }
        })
        {
            probes.Add(InstalledClientProbe.Check(profile[3], profile[0], profile[2]));
            var bundle = ClientLocaleBundle.Load(args[0], profile[3], profile[0], profile[2]);
            Expect(bundle.SptVersion == profile[0] && bundle.TranslationVersion == profile[1], "Exact profile selection");
            Reject(() => ClientLocaleBundle.Load(args[0], profile[3], "4.1.99", profile[2]), "Unknown SPT version");
            Reject(() => ClientLocaleBundle.Load(args[0], profile[3], profile[0], "0.0.0.0"), "Wrong EFT build");

            var source = new Dictionary<string, string> { ["mod-only"] = "kept", ["Alias"] = "first", ["alias"] = "last" };
            var folded = bundle.MergeGlobal("kr", source);
            Expect(folded["ALIAS"] == "last" && folded["mod-only"] == "kept", "Case aliases and other mods survive");
            Expect(source.Count == 3 && !source.ContainsKey("kr-en"), "Source dictionary is not mutated");
            var english = bundle.MergeGlobal("en", source);
            Expect(english.Count == 4 && !english.ContainsKey("5c1242fa86f7742aa04fed52"), "English does not receive Korean quest translations");

            var harmony = new Harmony("com.golani.clientlocale.contract");
            try
            {
                ClientLocaleRuntime.Enable(harmony, Assembly.GetExecutingAssembly(), bundle);
                foreach (var cached in new[] { true, false })
                {
                    await CheckNativeFlow(bundle, profile, args[0], cached);
                }
            }
            finally
            {
                harmony.UnpatchSelf();
            }
        }
        CheckInvalidPayloads(args[0], args[1]);
    }

    private static async Task CheckNativeFlow(ClientLocaleBundle bundle, string[] profile, string bundleRoot, bool cached)
    {
        var manager = new NativeLocaleManager { Culture = "kr-en" };
        NativeLocaleManager.Instance = manager;
        NativeLocaleManager.DefaultLanguage = "kr-en";
        NativeDataPreparation.CacheGlobals = cached;
        var languages = new Dictionary<string, string>
        {
            ["en"] = "English", ["kr-en"] = "old bilingual entry", ["kr"] = "Korean", ["de"] = "German"
        };
        manager.Init(languages);
        Expect(manager.Languages.Keys.SequenceEqual(new[] { "en", "kr", "kr-en", "de" }), "Adjacent native language options");
        Expect(languages.Keys.ElementAt(1) == "kr-en", "Language source is not mutated");

        var backend = new NativeBackend();
        // A saved bilingual selection can be loaded before a session exists; null must use the game's default.
        await NativeDataPreparation.ReloadBackendLocale(backend, null, null);
        Expect(backend.Requests.SequenceEqual(new[] { "menu:kr" }), "Cold bilingual menu requests Korean from a plain server");
        Expect(manager.MainMenuCultures.Contains("kr-en") && !manager.GlobalCultures.Contains("kr-en"), "Menu-only startup does not mark globals loaded");
        Expect(manager.Locales["kr-en"]["menu-only"] == "메뉴", "Korean menu inheritance");

        var session = new NativeSession { Backend = backend };
        await NativeDataPreparation.ReloadBackendLocale(backend, session, "kr-en");
        Expect(manager.Culture == "kr-en" && ClientLocaleRuntime.CurrentCulture() == "kr-en", "Native selection survives backend aliasing");
        Expect(manager.LastFont == "kr" && manager.LastApplicationCulture == "kr-en", "Korean font with bilingual culture");
        Expect(manager.ReloadEvents == 1, "Native screen reload occurs after both locales are populated");
        Expect(backend.Global["5c1242fa86f7742aa04fed52"] == "SERVER VALUE", "Cached server response is not overwritten");
        Expect(manager.Locales["kr"]["5c1242fa86f7742aa04fed52"].Contains(profile[4]), "Version-specific Insomnia hours");
        foreach (var mode in new[] { "kr", "kr-en" })
        {
            var expected = JObject.Parse(File.ReadAllText(Path.Combine(bundleRoot, "locales", profile[1], mode + ".json")));
            var folded = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in expected.Properties()) { folded[entry.Name] = (string)entry.Value; }
            folded["kr"] = ClientLocaleBundle.KoreanName;
            folded["kr-en"] = ClientLocaleBundle.BilingualName;
            foreach (var entry in folded)
            {
                Expect(manager.Locales[mode].TryGetValue(entry.Key, out var value) && value == entry.Value,
                    $"{profile[0]}/{mode}: {entry.Key}");
            }
        }

        foreach (var mode in new[] { "kr", "kr-en", "en", "kr-en" })
        {
            manager.Culture = mode;
            await NativeDataPreparation.ReloadBackendLocale(backend, session, mode);
            Expect(manager.LastApplicationCulture == mode, "Immediate language switch: " + mode);
            Expect(manager.LastFont == (mode == "kr-en" ? "kr" : mode), "Font follows selected mode");
        }
        Expect(backend.Requests.All(request => !request.EndsWith(":kr-en", StringComparison.Ordinal)), "No server kr-en endpoint dependency");

        manager.UpdateLocales("kr", new Dictionary<string, string> { ["late-dialogue"] = "새 대화", ["server-mod-key"] = "updated" });
        Expect(manager.Locales["kr-en"]["late-dialogue"] == "새 대화" && manager.Locales["kr-en"]["server-mod-key"] == "updated",
            "Later dialogue and mod fragments are mirrored");
        Expect(manager.Locales["kr-en"]["menu-only"] == "메뉴", "Late globals preserve menu strings");

        backend.Fail = true;
        manager.MainMenuCultures.Clear();
        try
        {
            await NativeDataPreparation.ReloadBackendLocale(backend, session, "kr-en");
            throw new Exception("Expected native network failure.");
        }
        catch (InvalidOperationException error)
        {
            Expect(error.Message == "fixture network failure", "Original async failures propagate");
        }
    }

    private static void CheckInvalidPayloads(string bundleRoot, string gameRoot)
    {
        var temporary = Path.Combine(Path.GetTempPath(), "spt-client-locale-contract-" + Guid.NewGuid().ToString("N"));
        try
        {
            var localeRoot = Path.Combine(temporary, "locales", "3.8.3");
            Directory.CreateDirectory(localeRoot);
            foreach (var name in new[] { "en.json", "kr.json", "kr-en.json" })
            {
                File.Copy(Path.Combine(bundleRoot, "locales", "3.8.3", name), Path.Combine(localeRoot, name));
            }
            File.Copy(Path.Combine(bundleRoot, "manifest.json"), Path.Combine(temporary, "manifest.json"));
            var koreanPath = Path.Combine(localeRoot, "kr.json");
            File.AppendAllText(koreanPath, " ");
            Reject(() => ClientLocaleBundle.Load(temporary, gameRoot, "3.8.3", "0.14.1.29197"), "Corrupt payload hash");

            File.WriteAllText(koreanPath, "{\"duplicate\":\"first\",\"duplicate\":\"second\"}");
            var manifest = JObject.Parse(File.ReadAllText(Path.Combine(temporary, "manifest.json")));
            using (var hash = SHA256.Create())
            {
                manifest["profiles"]["3.8.3"]["sha256"]["kr.json"] = BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(koreanPath))).Replace("-", "").ToLowerInvariant();
            }
            File.WriteAllText(Path.Combine(temporary, "manifest.json"), manifest.ToString());
            Reject(() => ClientLocaleBundle.Load(temporary, gameRoot, "3.8.3", "0.14.1.29197"), "Duplicate JSON keys");

            var fakeGame = Path.Combine(temporary, "game");
            var english = Path.Combine(fakeGame, "Aki_Data", "Server", "database", "locales", "global", "en.json");
            Directory.CreateDirectory(Path.GetDirectoryName(english));
            File.WriteAllText(english, "{\"different\":\"database\"}");
            Reject(() => ClientLocaleBundle.Load(bundleRoot, fakeGame, "3.8.3", "0.14.1.29197"), "Wrong installed English source");
        }
        finally
        {
            if (Directory.Exists(temporary)) { Directory.Delete(temporary, true); }
        }
    }

    private static void Reject(Action action, string label)
    {
        try { action(); }
        catch (InvalidDataException) { assertions++; return; }
        catch (Newtonsoft.Json.JsonException) { assertions++; return; }
        throw new Exception("Expected rejection: " + label);
    }

    private static void Expect(bool condition, string label)
    {
        if (!condition) { throw new Exception("Contract failed: " + label); }
        assertions++;
    }
}
