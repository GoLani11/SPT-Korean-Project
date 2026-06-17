using HarmonyLib;
using System.Reflection;
using EFT.UI.Gestures;
using UnityEngine;
using TMPro;
using SPT.Reflection.Patching;
using System.Collections.Generic;

namespace KoreanPatchFix
{
    public class GesturesMenuPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GesturesMenu).GetMethod("InitPhraseGroups", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        static void PatchPostfix(GesturesMenu __instance)
        {
            AdjustGestureTextSize(__instance);
        }

        private static void AdjustGestureTextSize(GesturesMenu menu)
        {
            var textComponents = menu.GetComponentsInChildren<TextMeshProUGUI>(true);
            HashSet<string> largerTextSet = new HashSet<string>
            {
                "지원 요청", "지휘", "건강 상태", "반응", "접촉", "적 발견", "팀 현황"
            };

            foreach (var textComponent in textComponents)
            {
                if (largerTextSet.Contains(textComponent.text))
                {
                    textComponent.fontSize = 18;
                }
                else
                {
                    textComponent.fontSize = 10;
                }
            }
        }
    }

    public class GesturesMenuFix : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return null;
        }

        public new static void Enable()
        {
            new GesturesMenuPatch().Enable();
        }
    }
}
