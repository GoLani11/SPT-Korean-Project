using HarmonyLib;
using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KoreanPatchFix
{
    internal static class FleaMarketItemCategoryFix
    {
        internal static PatchResult Enable(Harmony harmony)
        {
            var target = ResolveTarget();
            var postfix = typeof(FleaMarketItemCategoryFix).GetMethod(
                nameof(AfterCategoryUpdated),
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
            var type = ReflectionTools.FindType("EFT.UI.Ragfair.SubcategoryView")
                ?? throw new TypeLoadException("EFT.UI.Ragfair.SubcategoryView was not found.");
            return ReflectionTools.FindMethod(type, "SetExpandedStatus")
                ?? ReflectionTools.FindMethod(type, "Show")
                ?? throw new MissingMethodException(type.FullName, "SetExpandedStatus/Show");
        }

        private static void AfterCategoryUpdated(object __instance)
        {
            try
            {
                var mainLayoutField = ReflectionTools.FindField(__instance.GetType(), "_mainLayoutElement");
                if (mainLayoutField?.GetValue(__instance) is LayoutElement mainLayout)
                {
                    mainLayout.preferredHeight = 45;
                }

                AdjustText(__instance, "CategoryElementName");
                AdjustText(__instance, "CategoryChildCount");

                var component = __instance as Component;
                if (component == null)
                {
                    return;
                }

                var layout = component.GetComponent<LayoutElement>()
                    ?? component.gameObject.AddComponent<LayoutElement>();
                layout.minHeight = 45;

                var rectTransform = component.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 45);
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Flea-market category adjustment failed: {ex}");
            }
        }

        private static void AdjustText(object instance, string fieldName)
        {
            var field = ReflectionTools.FindField(instance.GetType(), fieldName);
            if (!(field?.GetValue(instance) is TMP_Text text))
            {
                return;
            }

            text.enableWordWrapping = true;
            text.overflowMode = TextOverflowModes.Overflow;
            text.alignment = TextAlignmentOptions.Left;

            var length = text.text?.Length ?? 0;
            if (length >= 110)
            {
                text.fontSize = 10;
            }
            else if (length >= 95)
            {
                text.fontSize = 12;
            }
            else if (length >= 65)
            {
                text.fontSize = 14;
            }
            else
            {
                text.fontSize = 16;
            }
        }
    }
}
