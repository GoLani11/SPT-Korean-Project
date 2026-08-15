using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace KoreanPatchFix
{
    internal static class GesturesMenuFix
    {
        private static readonly HashSet<string> GroupHeadings = new HashSet<string>
        {
            "지원 요청",
            "지휘",
            "건강 상태",
            "반응",
            "접촉",
            "적 발견",
            "팀 현황"
        };

        internal static PatchResult Enable(Harmony harmony)
        {
            var target = ResolveTarget();
            var postfix = typeof(GesturesMenuFix).GetMethod(
                nameof(AfterPhraseGroupsInitialized),
                BindingFlags.Static | BindingFlags.NonPublic);

            harmony.Patch(target, postfix: new HarmonyMethod(postfix));
            return PatchResult.Applied(target);
        }

        internal static PatchResult Probe()
        {
            return PatchResult.Applied(ResolveTarget());
        }

        private static MethodInfo ResolveTarget()
        {
            var type = ReflectionTools.FindType("EFT.UI.Gestures.GesturesMenu")
                ?? throw new TypeLoadException("EFT.UI.Gestures.GesturesMenu was not found.");
            return ReflectionTools.FindMethod(
                type,
                "InitPhraseGroups",
                method => method.GetParameters().Length == 0)
                ?? throw new MissingMethodException(type.FullName, "InitPhraseGroups");
        }

        private static void AfterPhraseGroupsInitialized(object __instance)
        {
            try
            {
                if (!(__instance is Component component))
                {
                    return;
                }

                foreach (var text in component.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    text.fontSize = GroupHeadings.Contains(text.text) ? 18 : 10;
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Gesture-menu adjustment failed: {ex}");
            }
        }
    }
}
