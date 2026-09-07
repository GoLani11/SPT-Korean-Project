using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace KoreanPatchFix
{
    internal static class ClientLocaleRuntime
    {
        private static ClientLocaleBundle bundle;
        private static ClientLocaleTargets targets;

        internal static void Enable(Harmony harmony, Assembly gameAssembly, ClientLocaleBundle localeBundle)
        {
            // Resolve every required target before installing any patch.
            var resolved = ClientLocaleTargets.Resolve(gameAssembly);
            bundle = localeBundle;
            targets = resolved;
            try
            {
                Patch(harmony, targets.Init, nameof(BeforeInit));
                Patch(harmony, targets.Reload, nameof(BeforeReload));
                Patch(harmony, targets.UpdateGlobal, nameof(BeforeGlobal), nameof(AfterGlobal));
                Patch(harmony, targets.UpdateMenu, nameof(BeforeMenu), nameof(AfterMenu));
                Patch(harmony, targets.Fonts, nameof(BeforeFonts));
            }
            catch
            {
                harmony.UnpatchSelf();
                bundle = null;
                targets = null;
                throw;
            }
        }

        internal static string CurrentCulture()
        {
            if (targets == null)
            {
                return null;
            }
            var instance = targets.Instance.GetValue(null, null);
            return instance == null ? null : targets.Culture.GetValue(instance, null) as string;
        }

        private static void BeforeInit(ref Dictionary<string, string> __0)
        {
            __0 = ClientLocaleBundle.WithBilingualLanguage(__0);
        }

        private static void BeforeReload(ref string __2)
        {
            // Change only the backend helper's argument; the game's selected/persisted culture stays kr-en.
            var requested = string.IsNullOrEmpty(__2) ? (string)targets.DefaultLanguage.GetValue(null, null) : __2;
            if (ClientLocaleBundle.Is(requested, ClientLocaleBundle.Bilingual))
            {
                __2 = ClientLocaleBundle.Korean;
            }
        }

        private static void BeforeGlobal(string __0, ref Dictionary<string, string> __1)
        {
            __1 = bundle.MergeGlobal(__0, __1);
        }

        private static void AfterGlobal(object __instance, string __0, Dictionary<string, string> __1)
        {
            if (ClientLocaleBundle.Is(__0, ClientLocaleBundle.Korean))
            {
                // Also mirrors later dialogue/mod fragments. The locale-ID guard prevents recursive mirroring.
                targets.UpdateGlobal.Invoke(__instance, new object[] { ClientLocaleBundle.Bilingual, __1 });
            }
        }

        private static void BeforeMenu(object[] __args)
        {
            __args[1] = targets.CopyMenu((IDictionary<string, string>)__args[1]);
        }

        private static void AfterMenu(object __instance, string __0, object[] __args)
        {
            if (ClientLocaleBundle.Is(__0, ClientLocaleBundle.Korean))
            {
                targets.UpdateMenu.Invoke(__instance, new[] { ClientLocaleBundle.Bilingual, __args[1] });
            }
        }

        private static void BeforeFonts(ref string __0)
        {
            if (ClientLocaleBundle.Is(__0, ClientLocaleBundle.Bilingual))
            {
                __0 = ClientLocaleBundle.Korean;
            }
        }

        private static void Patch(Harmony harmony, MethodBase target, string prefix, string postfix = null)
        {
            harmony.Patch(target, prefix: Hook(prefix), postfix: postfix == null ? null : Hook(postfix));
        }

        private static HarmonyMethod Hook(string name)
        {
            return new HarmonyMethod(typeof(ClientLocaleRuntime).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic))
            {
                priority = Priority.Last
            };
        }
    }
}
