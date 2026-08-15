using HarmonyLib;
using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KoreanPatchFix
{
    internal static class FleaMarketItemNameFix
    {
        internal static PatchResult Enable(Harmony harmony)
        {
            var target = ResolveTarget();
            var postfix = typeof(FleaMarketItemNameFix).GetMethod(
                nameof(AfterItemNameUpdated),
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
            var type = ReflectionTools.FindType("EFT.UI.Ragfair.OfferItemDescription")
                ?? throw new TypeLoadException("EFT.UI.Ragfair.OfferItemDescription was not found.");
            return ReflectionTools.FindMethod(type, "SetItemName", method => method.GetParameters().Length == 0)
                ?? ReflectionTools.FindMethod(type, "Show")
                ?? throw new MissingMethodException(type.FullName, "SetItemName/Show");
        }

        private static void AfterItemNameUpdated(object __instance)
        {
            try
            {
                TMP_Text categoryText = null;
                foreach (var field in ReflectionTools.GetInstanceFields(__instance.GetType()))
                {
                    if (!typeof(TMP_Text).IsAssignableFrom(field.FieldType))
                    {
                        continue;
                    }

                    var text = field.GetValue(__instance) as TMP_Text;
                    if (text == null)
                    {
                        continue;
                    }

                    SetCommonTextProperties(text);
                    if (field.Name == "_itemCategory")
                    {
                        categoryText = text;
                    }
                }

                if (categoryText != null)
                {
                    categoryText.fontSize = categoryText.text?.Length >= 40 ? 12 : 14;
                }

                var component = __instance as Component;
                if (component == null)
                {
                    return;
                }

                var layout = component.GetComponent<LayoutElement>()
                    ?? component.gameObject.AddComponent<LayoutElement>();
                layout.minHeight = 100;
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Flea-market item name adjustment failed: {ex}");
            }
        }

        private static void SetCommonTextProperties(TMP_Text text)
        {
            text.enableWordWrapping = true;
            text.overflowMode = TextOverflowModes.Overflow;
            text.lineSpacing = -30;
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
            else if (length >= 75)
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
