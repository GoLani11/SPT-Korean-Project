using System;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using EFT.UI;
using HarmonyLib;
using EFT.InventoryLogic;
using EFT;

namespace KoreanPatchFix
{
    /// <summary>
    /// 퀵 액세스 패널의 텍스트 크기를 8로 조정하는 패치
    /// </summary>
    public static class QuickAccessPanelFix
    {
        // 패치 활성화 메서드
        public static void Enable()
        {
            try
            {
                Harmony harmony = new Harmony("com.GoLani.koreanpatchfix.quickaccesspanel");

                // InventoryScreenQuickAccessPanel의 Show 메서드에 대한 Postfix 패치 적용
                MethodInfo original = typeof(InventoryScreenQuickAccessPanel).GetMethod("Show",
                    new Type[] { typeof(InventoryController), typeof(ItemUiContext), typeof(GamePlayerOwner), typeof(InsuranceCompanyClass) });

                MethodInfo postfix = typeof(QuickAccessPanelFix).GetMethod(nameof(AfterShow),
                    BindingFlags.Static | BindingFlags.NonPublic);

                harmony.Patch(original, null, new HarmonyMethod(postfix));
            }
            catch (Exception)
            {
            }
        }

        // Show 메서드 실행 후 호출되는 메서드
        private static void AfterShow(InventoryScreenQuickAccessPanel __instance)
        {
            try
            {
                if (__instance == null) return;

                // 모든 텍스트 요소 찾아서 크기 조정
                ResizeAllTexts(__instance.gameObject, 8);

                // 필요한 경우 다음 프레임에 한번 더 적용 (UI가 완전히 로드된 후)
                __instance.StartCoroutine(DelayedResizeTexts(__instance.gameObject, 8));
            }
            catch (Exception)
            {
            }
        }

        // 1프레임 지연 후 텍스트 크기 다시 적용
        private static System.Collections.IEnumerator DelayedResizeTexts(GameObject gameObject, int fontSize)
        {
            yield return null; // 한 프레임 대기
            ResizeAllTexts(gameObject, fontSize);
        }

        // 모든 하위 텍스트 요소를 찾아서 크기 조정
        private static void ResizeAllTexts(GameObject parent, int fontSize)
        {
            if (parent == null) return;

            // Text 컴포넌트 크기 조정
            foreach (Text text in parent.GetComponentsInChildren<Text>(true))
            {
                if (text != null)
                {
                    text.fontSize = fontSize;
                }
            }

            // TextMeshProUGUI 컴포넌트 크기 조정 (탈코프가 사용하는 텍스트 유형)
            foreach (TMPro.TextMeshProUGUI tmpText in parent.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
            {
                if (tmpText != null)
                {
                    tmpText.fontSize = fontSize;
                }
            }
        }
    }
}
