using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EFT;
using System.Runtime.CompilerServices;
using EFT.UI;
using UnityEngine;
using SPT.Core.Patches;
using SPT.Reflection.Patching;
using EFT.UI.Ragfair;
using UnityEngine.UI;
using TMPro;


namespace KoreanPatchFix
{
    // OfferItemDescription 클래스의 method_1을 패치하는 클래스
    public class OfferItemDescriptionPatch : ModulePatch
    {
        // 패치할 대상 메서드를 지정
        protected override MethodBase GetTargetMethod()
        {
            return typeof(OfferItemDescription).GetMethod("method_1", BindingFlags.Public | BindingFlags.Instance);
        }

        // 원본 메서드 실행 후 실행될 패치 메서드
        [PatchPostfix]
        static void PatchPostfix(OfferItemDescription __instance)
        {
            AdjustText(__instance);
            AdjustLayout(__instance);
        }

        // OfferItemDescription 인스턴스의 모든 TextMeshProUGUI 컴포넌트를 조정
        private static void AdjustText(OfferItemDescription instance)
        {
            var fields = typeof(OfferItemDescription).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            TextMeshProUGUI categoryText = null;

            foreach (var field in fields)
            {
                if (field.FieldType == typeof(TextMeshProUGUI))
                {
                    var textComponent = field.GetValue(instance) as TextMeshProUGUI;
                    if (textComponent != null)
                    {
                        SetCommonTextProperties(textComponent);

                        if (field.Name == "_itemCategory")
                        {
                            categoryText = textComponent;
                        }
                    }
                }
            }

            // 카테고리 텍스트에 대한 추가 조정
            if (categoryText != null)
            {
                AdjustCategoryText(categoryText);
            }
        }

        private static void SetCommonTextProperties(TextMeshProUGUI text)
        {
            text.fontSize = 16;
            text.enableWordWrapping = true;
            text.overflowMode = TextOverflowModes.Overflow;
            text.lineSpacing = -30;
            text.alignment = TextAlignmentOptions.Left;
            text.fontSize = 16;
            // 텍스트 길이에 따른 폰트 크기 조정
            if (text.text.Length >= 75 && text.text.Length < 95)
            {
                text.fontSize = 14;
            }
            else if (text.text.Length >= 95 && text.text.Length < 110)
            {
                text.fontSize = 12;
            }
            else if (text.text.Length >= 110)
            {
                text.fontSize = 10;
            }
        }

        private static void AdjustCategoryText(TextMeshProUGUI categoryText)
        {
            categoryText.fontSize = 14;
            if (categoryText.text.Length >= 40)
            {
                categoryText.fontSize = 12;
            }
        }

        // LayoutElement 조정
        private static void AdjustLayout(OfferItemDescription instance)
        {
            var layoutElement = instance.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = instance.gameObject.AddComponent<LayoutElement>();
            }
            layoutElement.minHeight = 100;
        }
    }

    // 패치 플러그인 클래스
    public class FleaMarketItemNameFix : ModulePatch
    {
        // 이 클래스는 직접적인 패치 대상이 없으므로 null 반환
        protected override MethodBase GetTargetMethod()
        {
            return null;
        }

        // 패치 활성화 메서드
        public new static void Enable()
        {
            new OfferItemDescriptionPatch().Enable();
        }
    }
}
