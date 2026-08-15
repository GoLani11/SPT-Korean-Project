using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace KoreanPatchFix
{
    internal static class ItemViewShortNameFix
    {
        private static Type _itemViewType;
        private static Type _infoWindowType;
        private static Type _gridWindowType;

        internal static PatchResult Enable(Harmony harmony)
        {
            var target = ResolveTarget();
            var postfix = typeof(ItemViewShortNameFix).GetMethod(
                nameof(AfterPoolsInitialized),
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
            var type = ReflectionTools.FindType("EFT.UI.UiPools")
                ?? throw new TypeLoadException("EFT.UI.UiPools was not found.");
            var target = ReflectionTools.FindMethod(
                type,
                "Init",
                method => method.GetParameters().Length == 0 && typeof(Task).IsAssignableFrom(method.ReturnType))
                ?? throw new MissingMethodException(type.FullName, "Init()");

            _itemViewType = ReflectionTools.FindType("EFT.UI.DragAndDrop.ItemView")
                ?? throw new TypeLoadException("EFT.UI.DragAndDrop.ItemView was not found.");
            _infoWindowType = ReflectionTools.FindType("EFT.UI.InfoWindow")
                ?? throw new TypeLoadException("EFT.UI.InfoWindow was not found.");
            _gridWindowType = ReflectionTools.FindType("EFT.UI.GridWindow")
                ?? throw new TypeLoadException("EFT.UI.GridWindow was not found.");

            return target;
        }

        private static async void AfterPoolsInitialized(Task __result)
        {
            try
            {
                if (__result != null)
                {
                    await __result;
                }

                if (!GameLanguageDetector.IsKorean())
                {
                    return;
                }

                foreach (var itemView in Resources.FindObjectsOfTypeAll(_itemViewType).OfType<Component>())
                {
                    AdjustItemViewCaption(itemView);
                }

                foreach (var infoWindow in Resources.FindObjectsOfTypeAll(_infoWindowType).OfType<Component>())
                {
                    AdjustInfoWindowCaption(infoWindow);
                }

                foreach (var gridWindow in Resources.FindObjectsOfTypeAll(_gridWindowType).OfType<Component>())
                {
                    AdjustGridWindowCaption(gridWindow);
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Item short-name adjustment failed: {ex}");
            }
        }

        private static void AdjustItemViewCaption(Component itemView)
        {
            var caption = FindChild(itemView, "Caption", "Name");
            if (caption == null)
            {
                return;
            }

            var rectTransform = caption.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.offsetMax = new Vector2(-3f, -1f);
                rectTransform.offsetMin = new Vector2(1f, -17f);
            }

            var text = caption.GetComponentInChildren<TextMeshProUGUI>(true);
            if (text != null)
            {
                SetAutoSizingText(text);
            }
        }

        private static void AdjustInfoWindowCaption(Component infoWindow)
        {
            var caption = FindChild(infoWindow, "Caption");
            var rectTransform = caption?.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.offsetMax = new Vector2(-25f, 2f);
                rectTransform.offsetMin = new Vector2(25f, -2f);
            }
        }

        private static void AdjustGridWindowCaption(Component gridWindow)
        {
            var caption = FindChild(gridWindow, "Caption");
            var text = caption?.GetComponentInChildren<TextMeshProUGUI>(true);
            if (text != null)
            {
                text.overflowMode = TextOverflowModes.Overflow;
                SetAutoSizingText(text);
            }
        }

        private static Transform FindChild(Component component, params string[] names)
        {
            return component.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(transform => names.Contains(transform.name));
        }

        private static void SetAutoSizingText(TextMeshProUGUI text)
        {
            text.enableAutoSizing = true;
            text.fontSizeMin = 8;
            text.fontSizeMax = 12;
            text.lineSpacing = -15;
        }
    }
}
