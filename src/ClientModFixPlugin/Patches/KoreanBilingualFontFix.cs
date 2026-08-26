using HarmonyLib;
using System;
using System.Reflection;

namespace KoreanPatchFix
{
    internal static class KoreanBilingualFontFix
    {
        private const string KoreanLocaleId = "kr";
        private const string BilingualLocaleId = "kr-en";

        internal static PatchResult Enable(Harmony harmony)
        {
            var localizationManager = ReflectionTools.FindType("EFT.LocalizationManager")
                ?? throw new TypeLoadException("EFT.LocalizationManager was not found.");
            var target = ReflectionTools.FindMethod(
                localizationManager,
                "UpdateFonts",
                method =>
                {
                    var parameters = method.GetParameters();
                    return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
                })
                ?? throw new MissingMethodException(localizationManager.FullName, "UpdateFonts");
            var prefix = typeof(KoreanBilingualFontFix).GetMethod(
                nameof(UseKoreanFontFallback),
                BindingFlags.Static | BindingFlags.NonPublic);

            harmony.Patch(target, prefix: new HarmonyMethod(prefix));
            return PatchResult.Applied(target);
        }

        private static void UseKoreanFontFallback(ref string __0)
        {
            if (string.Equals(__0, BilingualLocaleId, StringComparison.OrdinalIgnoreCase))
            {
                __0 = KoreanLocaleId;
            }
        }
    }
}
