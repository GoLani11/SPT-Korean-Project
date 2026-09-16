using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using EFT;
using HarmonyLib;
using NativeDictionary = Il2CppSystem.Collections.Generic.Dictionary<string, string>;

namespace GoLani.KoreanLocaleProbe50;

[BepInPlugin(Id, "Korean Locale Probe for SPT 5.0", "0.1.0")]
[BepInProcess("EscapeFromTarkov.exe")]
public sealed class Plugin : BasePlugin
{
    public const string Id = "golani.korean.localeprobe50";
    private Harmony? _harmony;
    private static ManualLogSource? _log;
    private static int _updates;
    private static int _applications;

    public override void Load()
    {
        _log = Log;
        try
        {
            ProbeGuard.Validate(BepInEx.Paths.GameRootPath);
            var target = typeof(LocalizationManager).GetMethod("UpdateLocales",
                new[] { typeof(string), typeof(NativeDictionary), typeof(bool) });
            if (target == null || target.IsStatic || target.ReturnType != typeof(void))
                throw new MissingMethodException("The expected IL2CPP UpdateLocales signature was not found.");
            _harmony = new Harmony(Id);
            _harmony.Patch(target,
                prefix: new HarmonyMethod(typeof(Plugin).GetMethod(nameof(BeforeUpdate), BindingFlags.Static | BindingFlags.NonPublic)),
                postfix: new HarmonyMethod(typeof(Plugin).GetMethod(nameof(AfterUpdate), BindingFlags.Static | BindingFlags.NonPublic)));
            Log.LogInfo("KR5 READY: exact test build verified; UpdateLocales(string, dictionary, bool) patched. Select Korean and check Character / Trading / Settings for [KR5].");
        }
        catch (Exception error)
        {
            _harmony?.UnpatchSelf();
            Log.LogError("KR5 DISABLED: " + error.Message);
        }
    }

    // Parameter indices avoid dependence on the names emitted by the interop generator.
    private static void BeforeUpdate(string __0, NativeDictionary __1, out int __state)
    {
        __state = 0;
        if (++_updates <= 8) _log?.LogInfo("KR5 UPDATE: language=" + __0);
        if (__0 != "kr" || __1 == null) return;
        var changes = new List<(string Key, string Before, string After)>();
        try
        {
            foreach (string key in ProbePolicy.Keys)
            {
                if (__1.TryGetValue(key, out var current) && ProbePolicy.TryTranslate(__0, key, current, out var replacement))
                    changes.Add((key, current, replacement));
            }
            foreach (var change in changes) __1[change.Key] = change.After;
            __state = changes.Count;
            if (changes.Count > 0 && ++_applications <= 8)
                _log?.LogInfo("KR5 APPLIED: " + changes.Count + " approved menu strings; original update continues.");
        }
        catch (Exception error)
        {
            foreach (var change in changes)
            {
                try { __1[change.Key] = change.Before; }
                catch (Exception rollback) { _log?.LogError("KR5 rollback: " + rollback.Message); }
            }
            _log?.LogError("KR5 update skipped: " + error.Message);
        }
    }

    private static void AfterUpdate(LocalizationManager __instance, string __0, int __state)
    {
        if (__state == 0 || _applications > 8) return;
        try
        {
            // Verify the real lookup after the game's merge, not merely our input dictionary.
            if (__instance.Culture != "kr")
            {
                _log?.LogInfo("KR5 MERGED: Korean data updated while a different display language is active.");
                return;
            }
            int visible = 0;
            foreach (string key in ProbePolicy.Keys)
                if (__instance.LocalizedValue(key).EndsWith(" [KR5]", StringComparison.Ordinal)) visible++;
            _log?.LogInfo("KR5 LOOKUP: " + visible + "/3 marker values available in the active Korean locale.");
        }
        catch (Exception error) { _log?.LogWarning("KR5 lookup check unavailable: " + error.Message); }
    }

    public override bool Unload()
    {
        // Decline hot-unload: restart the process to rebuild dictionaries from original data.
        return false;
    }
}
