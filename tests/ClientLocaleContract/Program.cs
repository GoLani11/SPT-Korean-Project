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
    private static readonly JArray betterKeysChecks = new JArray();

    public static int Main(string[] args)
    {
        Console.OutputEncoding = new UTF8Encoding(false);
        try
        {
            if (args.Length == 5 && args[0] == "--unity-mono")
            {
                return UnityMonoHost.Run(args[1], Assembly.GetExecutingAssembly().Location, args.Skip(2).ToArray());
            }
            if (args.Length != 3) { throw new ArgumentException("Expected bundle root, installation matrix, and report path."); }
            Run(args).GetAwaiter().GetResult();
            File.WriteAllText(args[2], new JObject
            {
                ["assertions"] = assertions,
                ["harmonyRuntime"] = (Type.GetType("Mono.Runtime") != null ? "Mono" : "Windows .NET Framework") + "; native reload fixture",
                ["installedClientProbes"] = probes,
                ["betterKeysChecks"] = betterKeysChecks,
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
        var manifest = JObject.Parse(File.ReadAllText(Path.Combine(args[0], "manifest.json")));
        CheckReleaseBinary(args[0], manifest);
        var installations = JObject.Parse(File.ReadAllText(args[1]));
        var cases = ((JObject)manifest["profiles"]).Properties().Concat(new[]
        {
            new JProperty("4.1.6", manifest["profiles"]["4.1.5"].DeepClone())
        });
        foreach (var entry in cases)
        {
            var definition = entry.Value;
            var simulated = entry.Name == "4.1.6";
            var installation = installations[simulated ? "4.1.5" : entry.Name];
            var profile = new[] { entry.Name, (string)definition["translationVersion"], (string)definition["eftVersion"], (string)installation["root"] };
            var installed = !simulated && (bool)installation["completeClient"];
            if (installed)
            {
                probes.Add(InstalledClientProbe.Check(profile[3], profile[0], profile[2]));
                Expect(ClientLocaleBuild.ReadEftVersion(Path.Combine(profile[3], "EscapeFromTarkov.exe")) == profile[2], "Complete EFT build from fixed version fields");
            }
            else
            {
                probes.Add(new JObject { ["sptVersion"] = profile[0], ["kind"] = simulated ? "simulated SPT patch upgrade on unchanged 4.1.5 game/data; not a real 4.1.6 installation" : "payload/native fixture only; matching game client unavailable" });
            }
            var bundle = ClientLocaleBundle.Load(args[0], profile[3], profile[0], profile[2]);
            Expect(bundle.SptVersion == profile[0] && bundle.TranslationVersion == profile[1], "Selected version and translation source");
            Expect(bundle.ProfileVersion == (simulated ? "4.1.5" : profile[0]), "Exact profiles take precedence; patch upgrades use the verified fallback");
            Reject(() => ClientLocaleBundle.Load(args[0], profile[3], "4.2.0", profile[2]), "Unknown SPT version");
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
                    if (installation["betterKeysLocale"] != null)
                        await CheckBetterKeys(bundle, profile, args[0], (string)installation["betterKeysLocale"], cached);
                }
                CheckSuppressedUpdate();
            }
            finally
            {
                harmony.UnpatchSelf();
            }
        }
        CheckPatchUpgrades(args[0], (string)installations["4.1.5"]["root"]);
        CheckInvalidPayloads(args[0], (string)installations["3.8.3"]["root"]);
    }

    private static void CheckSuppressedUpdate()
    {
        var otherMod = new Harmony("com.golani.contract.other-mod");
        try
        {
            otherMod.Patch(typeof(NativeLocaleManager).GetMethod("UpdateLocales"), prefix:
                new HarmonyMethod(typeof(Program).GetMethod(nameof(SuppressUpdate), BindingFlags.Static | BindingFlags.NonPublic))
                { priority = Priority.First });
            var manager = new NativeLocaleManager();
            manager.UpdateLocales("kr", new Dictionary<string, string> { ["mod-owned"] = "handled elsewhere" });
            Expect(manager.GlobalCultures.Count == 0, "A mod suppressing the original update does not cause a null-state mirror");
        }
        finally { otherMod.UnpatchSelf(); }
    }

    private static bool SuppressUpdate() => false;

    private static void CheckReleaseBinary(string bundleRoot, JObject manifest)
    {
        var path = Path.Combine(Path.GetDirectoryName(bundleRoot), "GoLani.KoreanModFix.dll");
        using (var assembly = Mono.Cecil.AssemblyDefinition.ReadAssembly(path))
        {
            Expect(assembly.Name.Version == new Version(2, 1, 1, 0), "Release assembly version 2.1.1");
            var plugin = assembly.MainModule.Types.Single(type => type.FullName == "KoreanPatchFix.Plugin");
            var registration = plugin.CustomAttributes.Single(attribute => attribute.AttributeType.FullName == "BepInEx.BepInPlugin");
            Expect((string)registration.ConstructorArguments[2].Value == "2.1.1", "BepInEx plugin version 2.1.1");
            Expect((string)plugin.Fields.Single(field => field.Name == "PluginVersion").Constant == "2.1.1", "Log version 2.1.1");
        }
        Expect((string)manifest["clientVersion"] == "2.1.1", "Manifest version 2.1.1");
        using (var sha = SHA256.Create())
            Expect(BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant()
                == (string)manifest["clientDllSha256"], "Manifest references the shipped client binary");
    }

    private static void CheckPatchUpgrades(string bundleRoot, string gameRoot)
    {
        foreach (var version in new[] { "4.1.4", "4.1.99" })
            Expect(ClientLocaleBundle.Load(bundleRoot, gameRoot, version, "0.16.9.40743").ProfileVersion == "4.1.5", "Stable patch fallback: " + version);
        foreach (var version in new[] { "4.1.1", "4.2.0", "5.0.0", "4.1.6-pre", "4.1.6.0", "4.1.06", "unknown" })
            Reject(() => ClientLocaleBundle.Load(bundleRoot, gameRoot, version, "0.16.9.40743"), "Unsupported patch family or unstable version");
        Reject(() => ClientLocaleBundle.Load(bundleRoot, gameRoot, "4.1.6", "0.16.9.99999"), "Fallback must verify EFT build");
        var temporary = Path.Combine(Path.GetTempPath(), "spt-patch-upgrade-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporary);
        try
        {
            var english = Path.Combine(temporary, "SPT_Runtime/SPT_Data/database/locales/global/en.json");
            Directory.CreateDirectory(Path.GetDirectoryName(english));
            File.WriteAllText(english, "{\"new-or-changed\":\"source\"}");
            Reject(() => ClientLocaleBundle.Load(bundleRoot, temporary, "4.1.6", "0.16.9.40743"), "Fallback must verify installed English");
            var manifest = JObject.Parse(File.ReadAllText(Path.Combine(bundleRoot, "manifest.json")));
            manifest["profiles"]["4.1.6"] = manifest["profiles"]["4.1.5"].DeepClone();
            manifest["profiles"]["4.1.6"]["eftVersion"] = "0.0.0.0";
            File.WriteAllText(Path.Combine(temporary, "manifest.json"), manifest.ToString());
            Reject(() => ClientLocaleBundle.Load(temporary, gameRoot, "4.1.6", "0.16.9.40743"), "An incompatible exact profile must not fall back");
            ((JObject)manifest["profiles"]).Remove("4.1.6");
            ((JObject)manifest["profiles"]).Remove("4.1.5");
            File.WriteAllText(Path.Combine(temporary, "manifest.json"), manifest.ToString());
            Reject(() => ClientLocaleBundle.Load(temporary, gameRoot, "4.1.6", "0.16.9.40743"), "Missing verified fallback must not choose arbitrary data");
            var bundle = ClientLocaleBundle.Load(bundleRoot, gameRoot, "4.1.6", "0.16.9.40743");
            var harmony = new Harmony("com.golani.clientlocale.missing-target-contract");
            try
            {
                try { ClientLocaleRuntime.Enable(harmony, typeof(object).Assembly, bundle); }
                catch (InvalidOperationException) { assertions++; return; }
                throw new Exception("Missing game hooks must disable fallback localization.");
            }
            finally { harmony.UnpatchSelf(); }
        }
        finally { Directory.Delete(temporary, true); }
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
        backend.Global.Clear();
        var manifestProfile = JObject.Parse(File.ReadAllText(Path.Combine(bundleRoot, "manifest.json")))["profiles"][bundle.ProfileVersion];
        var baselinePath = Path.Combine(profile[3], ((string)manifestProfile["englishPath"]).Replace('/', Path.DirectorySeparatorChar));
        foreach (var entry in JObject.Parse(File.ReadAllText(baselinePath)).Properties())
            backend.Global[entry.Name] = (string)entry.Value;
        var baselineKrPath = Path.Combine(Path.GetDirectoryName(baselinePath), "kr.json");
        if (File.Exists(baselineKrPath))
            foreach (var entry in JObject.Parse(File.ReadAllText(baselineKrPath)).Properties())
                backend.Global[entry.Name] = (string)entry.Value;
        var originalBackend = new Dictionary<string, string>(backend.Global);
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
        Expect(originalBackend.Count == backend.Global.Count && originalBackend.All(p => backend.Global[p.Key] == p.Value), "Cached server response is not overwritten");
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

        const string changedKey = "5c1242fa86f7742aa04fed52";
        manager.UpdateLocales("kr", new Dictionary<string, string> { [changedKey.ToUpperInvariant()] = "MOD REPLACEMENT" });
        manager.UpdateLocales("kr", new Dictionary<string, string> { ["unrelated-fragment"] = "later" });
        foreach (var mode in new[] { "kr", "kr-en" })
            Expect(manager.Locales[mode][changedKey] == "MOD REPLACEMENT", "Existing mod text survives aliases and unrelated fragments");
        var definition = JObject.Parse(File.ReadAllText(Path.Combine(bundleRoot, "manifest.json")))["profiles"][bundle.ProfileVersion];
        var sourcePath = Path.Combine(profile[3], ((string)definition["englishPath"]).Replace('/', Path.DirectorySeparatorChar));
        var original = JObject.Parse(File.ReadAllText(sourcePath));
        var krPath = Path.Combine(Path.GetDirectoryName(sourcePath), "kr.json");
        var originalKr = File.Exists(krPath) ? JObject.Parse(File.ReadAllText(krPath)) : new JObject();
        manager.UpdateLocales("kr", new Dictionary<string, string> { [changedKey] = (string)(originalKr[changedKey] ?? original[changedKey]) });
        foreach (var mode in new[] { "kr", "kr-en" })
        {
            var expected = JObject.Parse(File.ReadAllText(Path.Combine(bundleRoot, "locales", profile[1], mode + ".json")));
            Expect(manager.Locales[mode][changedKey] == (string)expected[changedKey], "Original text restores translation after a mod is removed");
        }

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

    private static async Task CheckBetterKeys(ClientLocaleBundle bundle, string[] profile,
        string bundleRoot, string snapshotPath, bool cached)
    {
        var snapshot = JObject.Parse(File.ReadAllText(snapshotPath));
        Expect((string)snapshot["sptVersion"] == profile[0], "BetterKeys snapshot matches SPT profile");
        var before = ((JObject)snapshot["before"]).Properties().ToDictionary(p => p.Name, p => (string)p.Value);
        var after = ((JObject)snapshot["after"]).Properties().ToDictionary(p => p.Name, p => (string)p.Value);
        var changed = after.Where(p => !before.TryGetValue(p.Key, out var value) || value != p.Value).ToArray();
        Expect(changed.Length > 0, "Actual BetterKeys server modified locale entries");
        var backend = new NativeBackend();
        backend.Global.Clear();
        foreach (var entry in after) backend.Global[entry.Key] = entry.Value;
        var manager = new NativeLocaleManager { Culture = "kr-en" };
        NativeLocaleManager.Instance = manager;
        NativeDataPreparation.CacheGlobals = cached;
        var session = new NativeSession { Backend = backend };
        foreach (var mode in new[] { "kr-en", "kr", "en", "kr-en" })
        {
            manager.Culture = mode;
            await NativeDataPreparation.ReloadBackendLocale(backend, session, mode);
            if (mode == "en") continue;
            foreach (var entry in changed)
                Expect(manager.Locales[mode][entry.Key] == entry.Value, "BetterKeys information survives " + mode + ": " + entry.Key);
        }
        manager.UpdateLocales("kr", new Dictionary<string, string> { ["later-mod-fragment"] = "later" });
        foreach (var mode in new[] { "kr", "kr-en" })
        {
            foreach (var entry in changed)
                Expect(manager.Locales[mode][entry.Key] == entry.Value, "BetterKeys survives partial updates");
            var translation = JObject.Parse(File.ReadAllText(Path.Combine(bundleRoot, "locales", profile[1], mode + ".json")));
            var untouched = before.First(p => after.TryGetValue(p.Key, out var value) && value == p.Value && translation[p.Key] != null && (string)translation[p.Key] != p.Value);
            Expect(manager.Locales[mode][untouched.Key] == (string)translation[untouched.Key], "Unmodified text is still translated with BetterKeys installed");
        }
        Expect(after.All(p => backend.Global[p.Key] == p.Value), "BetterKeys server snapshot is not mutated");
        betterKeysChecks.Add(new JObject { ["sptVersion"] = profile[0], ["mod"] = snapshot["mod"],
            ["modVersion"] = snapshot["modVersion"], ["sourceCommit"] = snapshot["sourceCommit"],
            ["changedEntries"] = changed.Length, ["cachedBackend"] = cached,
            ["validation"] = "actual server output through Harmony native-flow fixture; not in-game rendering" });
        Console.WriteLine($"BetterKeys actual server snapshot passed: {changed.Length} changed entries, cached={cached}.");
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
