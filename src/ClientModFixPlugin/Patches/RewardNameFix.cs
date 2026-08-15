using HarmonyLib;
using System;
using System.Reflection;
using TMPro;

namespace KoreanPatchFix
{
    internal static class RewardNameFix
    {
        internal static PatchResult Enable(Harmony harmony)
        {
            var target = ResolveTarget();
            if (target == null)
            {
                return PatchResult.Unavailable("prestige rewards do not exist in this client");
            }

            var postfix = typeof(RewardNameFix).GetMethod(
                nameof(AfterRewardShown),
                BindingFlags.Static | BindingFlags.NonPublic);

            harmony.Patch(target, postfix: new HarmonyMethod(postfix));
            return PatchResult.Applied(target);
        }

        internal static PatchResult Probe()
        {
            var target = ResolveTarget();
            return target == null
                ? PatchResult.Unavailable("prestige rewards do not exist in this client")
                : PatchResult.Applied(target);
        }

        private static MethodInfo ResolveTarget()
        {
            var type = ReflectionTools.FindType("EFT.UI.Prestige.PrestigeRewardView");
            return type == null
                ? null
                : ReflectionTools.FindMethod(type, "Show")
                    ?? throw new MissingMethodException(type.FullName, "Show");
        }

        private static void AfterRewardShown(object __instance)
        {
            try
            {
                var field = ReflectionTools.FindField(__instance.GetType(), "_rewardName");
                if (!(field?.GetValue(__instance) is TMP_Text text) || string.IsNullOrEmpty(text.text))
                {
                    return;
                }

                text.enableWordWrapping = true;
                text.overflowMode = TextOverflowModes.Overflow;
                text.fontSize = text.text.Length <= 18 ? 10 : 8;
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Prestige reward-name adjustment failed: {ex}");
            }
        }
    }
}
