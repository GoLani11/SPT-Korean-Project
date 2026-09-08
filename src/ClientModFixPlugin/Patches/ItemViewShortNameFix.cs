using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

namespace KoreanPatchFix
{
    internal static class ItemViewShortNameFix
    {
        private static FieldInfo _itemCaption;
        private static readonly ConditionalWeakTable<TextMeshProUGUI, CaptionState> Original =
            new ConditionalWeakTable<TextMeshProUGUI, CaptionState>();

        internal static PatchResult Enable(Harmony harmony)
        {
            var targets = ResolveTargets();
            var hooks = new[] { nameof(AfterItemCaptionUpdated), nameof(AfterInfoWindowShown), nameof(AfterGridWindowShown) };
            var installed = 0;
            try
            {
                for (; installed < targets.Length; installed++)
                    harmony.Patch(targets[installed], postfix: new HarmonyMethod(typeof(ItemViewShortNameFix), hooks[installed]));
            }
            catch
            {
                // Remove only this feature's hooks if a later window hook fails.
                for (var i = 0; i < installed; i++)
                    harmony.Unpatch(targets[i], typeof(ItemViewShortNameFix).GetMethod(hooks[i], BindingFlags.Static | BindingFlags.NonPublic));
                throw;
            }
            return PatchResult.Applied(targets[0]);
        }

        internal static PatchResult Probe() => PatchResult.Applied(ResolveTargets()[0]);

        private static MethodInfo[] ResolveTargets()
        {
            var gridItem = ReflectionTools.FindType("EFT.UI.DragAndDrop.GridItemView")
                ?? throw new TypeLoadException("EFT.UI.DragAndDrop.GridItemView was not found.");
            _itemCaption = ReflectionTools.FindField(gridItem, "Caption");
            if (_itemCaption == null || !typeof(TextMeshProUGUI).IsAssignableFrom(_itemCaption.FieldType))
                throw new MissingFieldException(gridItem.FullName, "Caption");

            // Older clients obfuscate UpdateItemName. Locate the actual caption writer,
            // which also runs for late-created/reused views and native language changes.
            var captionWriter = gridItem.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Where(method => method.ReturnType == typeof(void) && method.GetParameters().Length == 0 && method.GetMethodBody() != null)
                .Single(method =>
                {
                    var instructions = PatchProcessor.GetOriginalInstructions(method);
                    return instructions.Any(instruction => instruction.opcode == OpCodes.Ldfld && Equals(instruction.operand, _itemCaption))
                        && instructions.Any(instruction => instruction.operand is MethodInfo called && called.Name == "set_text"
                            && typeof(TMP_Text).IsAssignableFrom(called.DeclaringType));
                });
            return new[] { captionWriter, WindowShow("EFT.UI.InfoWindow"), WindowShow("EFT.UI.GridWindow") };
        }

        private static MethodInfo WindowShow(string name)
        {
            var type = ReflectionTools.FindType(name) ?? throw new TypeLoadException(name + " was not found.");
            var caption = type.GetProperty("Caption", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (caption == null || !typeof(TextMeshProUGUI).IsAssignableFrom(caption.PropertyType))
                throw new MissingMemberException(name, "Caption");
            return ReflectionTools.FindMethod(type, "Show", method => method.DeclaringType == type)
                ?? throw new MissingMethodException(name, "Show");
        }

        private static void AfterItemCaptionUpdated(object __instance)
        {
            try { Adjust(_itemCaption.GetValue(__instance) as TextMeshProUGUI, CaptionKind.Item); }
            catch (Exception error) { PluginLog.Error($"Item short-name adjustment failed: {error}"); }
        }

        private static void AfterInfoWindowShown(object __instance) => AdjustWindow(__instance, CaptionKind.Info);
        private static void AfterGridWindowShown(object __instance) => AdjustWindow(__instance, CaptionKind.Grid);

        private static void AdjustWindow(object instance, CaptionKind kind)
        {
            try { Adjust(ReflectionTools.ReadMember(instance, "Caption") as TextMeshProUGUI, kind); }
            catch (Exception error) { PluginLog.Error($"Window caption adjustment failed: {error}"); }
        }

        private static void Adjust(TextMeshProUGUI text, CaptionKind kind)
        {
            if (text == null) return;
            if (!GameLanguageDetector.IsKorean())
            {
                if (Original.TryGetValue(text, out var previous))
                {
                    previous.Restore(text);
                    Original.Remove(text);
                }
                return;
            }
            // Keep the original layout so an English switch on the same pooled view restores it.
            Original.GetValue(text, value => new CaptionState(value, kind));
            if (kind == CaptionKind.Item)
            {
                text.rectTransform.offsetMax = new Vector2(-3f, -1f);
                text.rectTransform.offsetMin = new Vector2(1f, -17f);
            }
            else if (kind == CaptionKind.Info)
            {
                text.rectTransform.offsetMax = new Vector2(-25f, 2f);
                text.rectTransform.offsetMin = new Vector2(25f, -2f);
            }
            if (kind != CaptionKind.Info)
            {
                text.enableAutoSizing = true;
                text.fontSizeMin = 8;
                text.fontSizeMax = 12;
                text.lineSpacing = -15;
            }
            if (kind == CaptionKind.Grid) text.overflowMode = TextOverflowModes.Overflow;
        }

        private enum CaptionKind { Item, Info, Grid }

        private sealed class CaptionState
        {
            private readonly CaptionKind kind;
            private readonly Vector2 offsetMin, offsetMax;
            private readonly bool autoSize;
            private readonly float size, minimum, maximum, spacing;
            private readonly TextOverflowModes overflow;

            internal CaptionState(TextMeshProUGUI text, CaptionKind kind)
            {
                this.kind = kind;
                offsetMin = text.rectTransform.offsetMin;
                offsetMax = text.rectTransform.offsetMax;
                autoSize = text.enableAutoSizing;
                size = text.fontSize;
                minimum = text.fontSizeMin;
                maximum = text.fontSizeMax;
                spacing = text.lineSpacing;
                overflow = text.overflowMode;
            }

            internal void Restore(TextMeshProUGUI text)
            {
                if (kind != CaptionKind.Grid)
                {
                    text.rectTransform.offsetMin = offsetMin;
                    text.rectTransform.offsetMax = offsetMax;
                }
                if (kind != CaptionKind.Info)
                {
                    text.enableAutoSizing = autoSize;
                    text.fontSize = size;
                    text.fontSizeMin = minimum;
                    text.fontSizeMax = maximum;
                    text.lineSpacing = spacing;
                }
                if (kind == CaptionKind.Grid) text.overflowMode = overflow;
            }
        }
    }
}
