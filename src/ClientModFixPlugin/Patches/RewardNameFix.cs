using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using SPT.Reflection.Patching;
using TMPro;

namespace KoreanPatchFix
{
    public class RewardNamePatch : ModulePatch
    {
        private Type prestigeRewardViewType;

        protected override MethodBase GetTargetMethod()
        {
            // 리플렉션으로 PrestigeRewardView 타입 찾기
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType("EFT.UI.Prestige.PrestigeRewardView");
                if (type != null)
                {
                    prestigeRewardViewType = type;
                    return type.GetMethod("Show", BindingFlags.Public | BindingFlags.Instance);
                }
            }

            // 타입을 찾지 못한 경우
            Logger.LogError("PrestigeRewardView 타입을 찾을 수 없습니다");
            return null;
        }

        [PatchPostfix]
        static void PatchPostfix(object __instance)
        {
            try
            {
                // 리플렉션을 통해 _rewardName 필드에 접근
                var rewardNameField = __instance.GetType().GetField("_rewardName", BindingFlags.NonPublic | BindingFlags.Instance);
                if (rewardNameField != null)
                {
                    var rewardNameText = rewardNameField.GetValue(__instance) as TextMeshProUGUI;
                    if (rewardNameText != null)
                    {
                        AdjustTextSizeByLength(rewardNameText);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"RewardNamePatch에서 오류 발생: {ex.Message}");
            }
        }

        private static void AdjustTextSizeByLength(TextMeshProUGUI text)
        {
            // 텍스트가 null이 아닌지 확인
            if (string.IsNullOrEmpty(text.text))
                return;

            // 기본 설정
            text.enableWordWrapping = true;
            text.overflowMode = TextOverflowModes.Overflow;

            // 텍스트 길이에 따른 폰트 크기 조정
            int length = text.text.Length;

            if (length <= 18)
            {
                text.fontSize = 10;
            }
            else
            {
                text.fontSize = 8;
            }
        }
    }

    public class RewardNameFix : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return null;
        }

        public new static void Enable()
        {
            new RewardNamePatch().Enable();
        }
    }
}
