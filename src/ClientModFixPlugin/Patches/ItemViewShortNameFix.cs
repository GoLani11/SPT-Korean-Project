using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SPT.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.Settings;
using EFT.UI;
using EFT.UI.DragAndDrop;
using TMPro;
using UnityEngine;

namespace KoreanPatchFix
{
    // UiPools 초기화 후 ItemView의 짧은 이름 표시를 조정하는 패치
    public class UiPoolsInitPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(UiPools).GetMethod(
                nameof(UiPools.Init),
                BindingFlags.Public | BindingFlags.Instance,
                null,
                Type.EmptyTypes,
                null);
        }

        [PatchPostfix]
        public static async void PatchPostfix(Task __result)
        {
            try
            {
                await __result;

                string language = Singleton<SettingsManager>.Instance.Game.Settings.Language.Value;

                // 한국어일 때만 UI 조정
                if (language == "kr")
                {
                    Debug.Log("[KoreanPatchFix] 한국어 감지됨. 아이템 짧은 이름 표시 조정 시작...");
                    AdjustItemViewUI();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[KoreanPatchFix] 아이템 짧은 이름 표시 조정 중 오류 발생: {ex.Message}");
            }
        }

        private static void AdjustItemViewUI()
        {
            try
            {
                // 모든 ItemView의 Caption 조정
                var allItemViews = Resources.FindObjectsOfTypeAll<ItemView>();
                foreach (var itemView in allItemViews)
                {
                    AdjustItemViewCaption(itemView);
                }

                // InfoWindow의 Caption 조정
                var infoWindows = Resources.FindObjectsOfTypeAll<InfoWindow>();
                foreach (var infoWindow in infoWindows)
                {
                    AdjustInfoWindowCaption(infoWindow);
                }

                // GridWindow의 Caption 조정
                var gridWindows = Resources.FindObjectsOfTypeAll<GridWindow>();
                foreach (var gridWindow in gridWindows)
                {
                    AdjustGridWindowCaption(gridWindow);
                }

                Debug.Log("[KoreanPatchFix] 아이템 짧은 이름 표시 조정 완료");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[KoreanPatchFix] UI 조정 중 오류 발생: {ex.Message}");
            }
        }

        // ItemView의 Caption 영역 조정
        private static void AdjustItemViewCaption(ItemView itemView)
        {
            var captionTransform = itemView.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(t => t.name == "Caption" || t.name == "Name");

            if (captionTransform != null)
            {
                var rectTransform = captionTransform.GetComponent<RectTransform>();
                // 텍스트가 잘리지 않도록 여백 조정
                if (rectTransform != null)
                {
                    rectTransform.offsetMax = new Vector2(-3f, -1f); // 오른쪽과 위쪽 여백
                    rectTransform.offsetMin = new Vector2(1f, -17f); // 왼쪽과 아래쪽 여백
                }

                var tmpText = captionTransform.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmpText != null)
                {
                    SetAutoSizingText(tmpText);
                }
            }
        }

        // InfoWindow의 Caption 영역 조정
        private static void AdjustInfoWindowCaption(InfoWindow infoWindow)
        {
            var captionTransform = infoWindow.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(t => t.name == "Caption");

            if (captionTransform != null)
            {
                var rectTransform = captionTransform.GetComponent<RectTransform>();
                // InfoWindow의 Caption 여백 조정
                if (rectTransform != null)
                {
                    rectTransform.offsetMax = new Vector2(-25f, 2f);
                    rectTransform.offsetMin = new Vector2(25f, -2f);
                }
            }
        }

        // GridWindow의 Caption 텍스트 조정
        private static void AdjustGridWindowCaption(GridWindow gridWindow)
        {
            var captionTransform = gridWindow.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(t => t.name == "Caption");

            if (captionTransform != null)
            {
                var tmpText = captionTransform.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmpText != null)
                {
                    tmpText.overflowMode = TextOverflowModes.Overflow;
                    SetAutoSizingText(tmpText);
                }
            }
        }

        // 텍스트 자동 크기 조정 설정
        private static void SetAutoSizingText(TextMeshProUGUI tmpText)
        {
            tmpText.enableAutoSizing = true;
            tmpText.fontSizeMin = 8;  // 최소 폰트 크기
            tmpText.fontSizeMax = 12; // 최대 폰트 크기
            tmpText.lineSpacing = -15f; // 행간 조정
        }
    }

    // 패치 활성화 클래스
    public class ItemViewShortNameFix : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return null;
        }

        public new static void Enable()
        {
            new UiPoolsInitPatch().Enable();
        }
    }
}
