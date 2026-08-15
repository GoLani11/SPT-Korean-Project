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
            "교전 상황",
            "팀 현황",
            "음성/제스처 메뉴"
        };

        private static readonly Dictionary<string, string> PhraseLabels = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "Look", "주의!" },
            { "Ready", "준비됐어!" },
            { "DontKnow", "모르겠어!" }
        };

        private static readonly Dictionary<string, string> GestureLabels = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "ThatDirection", "저기" },
            { "ThereGesture", "저기" },
            { "Stop", "멈춰!" },
            { "HoldGesture", "대기" },
            { "Hello", "인사" },
            { "FriendlyGesture", "인사" },
            { "FuckYou", "가운뎃손가락" },
            { "GetOffGesture", "가운뎃손가락" },
            { "Good", "엄지 척" },
            { "OkGesture", "엄지 척" },
            { "Bad", "엄지 내리기" },
            { "NoGesture", "엄지 내리기" },
            { "ComeToMe", "따라와" },
            { "ComeWithMeGesture", "따라와" },
            { "RockGesture", "바위" },
            { "ScissorGesture", "가위" },
            { "PaperGesture", "보" },
            { "AllRightGesture", "오케이" }
        };

        internal static PatchResult Enable(Harmony harmony)
        {
            var target = ResolveTarget();
            var groupPostfix = typeof(GesturesMenuFix).GetMethod(
                nameof(AfterPhraseGroupsInitialized),
                BindingFlags.Static | BindingFlags.NonPublic);
            var phrasePostfix = typeof(GesturesMenuFix).GetMethod(
                nameof(AfterPhraseShown),
                BindingFlags.Static | BindingFlags.NonPublic);
            var gesturePostfix = typeof(GesturesMenuFix).GetMethod(
                nameof(AfterGestureShown),
                BindingFlags.Static | BindingFlags.NonPublic);

            harmony.Patch(target, postfix: new HarmonyMethod(groupPostfix));
            harmony.Patch(ResolvePhraseTarget(), postfix: new HarmonyMethod(phrasePostfix));
            harmony.Patch(ResolveGestureTarget(), postfix: new HarmonyMethod(gesturePostfix));
            return PatchResult.Applied(target);
        }

        internal static PatchResult Probe()
        {
            var target = ResolveTarget();
            ResolvePhraseTarget();
            ResolveGestureTarget();
            return PatchResult.Applied(target);
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

        private static MethodInfo ResolvePhraseTarget()
        {
            var type = ReflectionTools.FindType("EFT.UI.Gestures.GesturesAudioSubItem")
                ?? throw new TypeLoadException("EFT.UI.Gestures.GesturesAudioSubItem was not found.");
            return ReflectionTools.FindMethod(type, "Show")
                ?? throw new MissingMethodException(type.FullName, "Show");
        }

        private static MethodInfo ResolveGestureTarget()
        {
            var type = ReflectionTools.FindType("EFT.UI.Gestures.GesturesMenuItem")
                ?? throw new TypeLoadException("EFT.UI.Gestures.GesturesMenuItem was not found.");
            return ReflectionTools.FindMethod(type, "Show")
                ?? throw new MissingMethodException(type.FullName, "Show");
        }

        private static void AfterPhraseGroupsInitialized(object __instance)
        {
            try
            {
                if (!GameLanguageDetector.IsKorean())
                {
                    return;
                }

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

        private static void AfterPhraseShown(object __instance)
        {
            ApplyContextLabel(__instance, "PhraseTrigger", "_textField", PhraseLabels, "voice phrase");
        }

        private static void AfterGestureShown(object __instance)
        {
            ApplyContextLabel(__instance, "Gesture", "_commandLabel", GestureLabels, "hand gesture");
        }

        private static void ApplyContextLabel(
            object instance,
            string valueMember,
            string textField,
            Dictionary<string, string> labels,
            string labelKind)
        {
            try
            {
                if (!GameLanguageDetector.IsKorean())
                {
                    return;
                }

                var value = ReflectionTools.ReadMember(instance, valueMember);
                if (value == null || !labels.TryGetValue(value.ToString(), out var translated))
                {
                    return;
                }

                var field = ReflectionTools.FindField(instance.GetType(), textField)
                    ?? throw new MissingFieldException(instance.GetType().FullName, textField);
                if (!(field.GetValue(instance) is TMP_Text text))
                {
                    throw new InvalidOperationException($"{instance.GetType().FullName}.{textField} is not a TMP text field.");
                }

                text.text = PreserveBilingualSuffix(translated, text.text);
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Could not adjust {labelKind} label: {ex}");
            }
        }

        private static string PreserveBilingualSuffix(string translated, string current)
        {
            if (string.IsNullOrEmpty(current))
            {
                return translated;
            }

            var newlineSuffix = current.IndexOf("\n(", StringComparison.Ordinal);
            if (newlineSuffix >= 0)
            {
                return translated + current.Substring(newlineSuffix);
            }

            var inlineSuffix = current.LastIndexOf(" (", StringComparison.Ordinal);
            if (inlineSuffix >= 0 && current.EndsWith(")", StringComparison.Ordinal))
            {
                return translated + current.Substring(inlineSuffix);
            }

            return translated;
        }
    }
}
