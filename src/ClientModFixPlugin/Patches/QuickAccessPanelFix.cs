using HarmonyLib;
using System;
using System.Collections;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KoreanPatchFix
{
    internal static class QuickAccessPanelFix
    {
        internal static PatchResult Enable(Harmony harmony)
        {
            var target = ResolveTarget();
            var postfix = typeof(QuickAccessPanelFix).GetMethod(
                nameof(AfterShow),
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
            var type = ReflectionTools.FindType("EFT.UI.InventoryScreenQuickAccessPanel")
                ?? throw new TypeLoadException("EFT.UI.InventoryScreenQuickAccessPanel was not found.");
            return ReflectionTools.FindMethod(type, "Show")
                ?? throw new MissingMethodException(type.FullName, "Show");
        }

        private static void AfterShow(object __instance)
        {
            try
            {
                if (!(__instance is MonoBehaviour panel))
                {
                    return;
                }

                ResizeAllTexts(panel.gameObject, 8);
                panel.StartCoroutine(DelayedResizeTexts(panel.gameObject, 8));
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Quick-access panel adjustment failed: {ex}");
            }
        }

        private static IEnumerator DelayedResizeTexts(GameObject gameObject, int fontSize)
        {
            yield return null;
            ResizeAllTexts(gameObject, fontSize);
        }

        private static void ResizeAllTexts(GameObject parent, int fontSize)
        {
            if (parent == null)
            {
                return;
            }

            foreach (var text in parent.GetComponentsInChildren<Text>(true))
            {
                text.fontSize = fontSize;
            }

            foreach (var text in parent.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                text.fontSize = fontSize;
            }
        }
    }
}
