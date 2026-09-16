using System;
using System.Collections.Generic;

namespace GoLani.KoreanLocaleProbe50;

internal static class ProbePolicy
{
    // Deliberate visual markers, not production translation content.
    internal static readonly string[] Keys = { "Character", "Trading", "SETTINGS" };
    private static readonly Dictionary<string, (string English, string Korean)> Baselines = new(StringComparer.Ordinal)
    {
        ["Character"] = ("CHARACTER", "캐릭터"),
        ["Trading"] = ("TRADING", "거래"),
        ["SETTINGS"] = ("SETTINGS", "설정"),
    };

    internal static bool TryTranslate(string language, string key, string? current, out string replacement)
    {
        replacement = current ?? "";
        if (language != "kr" || !Baselines.TryGetValue(key, out var baseline)) return false;
        if (current != baseline.Korean && current != baseline.English) return false;
        replacement = baseline.Korean + " [KR5]";
        return true;
    }
}
