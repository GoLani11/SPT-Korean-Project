using BepInEx;
using System;

namespace KoreanPatchFix
{
    [BepInPlugin("com.GoLani.koreanpatchfix", "Korean Patch Fix", "1.4.0")]
    [BepInDependency("com.SPT.core", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Logger.LogInfo("Loading: Korean Patch Fix");

            var enabledCount = 0;
            enabledCount += EnablePatch(nameof(FleaMarketItemNameFix), FleaMarketItemNameFix.Enable);
            enabledCount += EnablePatch(nameof(FleaMarketItemCategoryFix), FleaMarketItemCategoryFix.Enable);
            enabledCount += EnablePatch(nameof(GesturesMenuFix), GesturesMenuFix.Enable);
            enabledCount += EnablePatch(nameof(RewardNameFix), RewardNameFix.Enable);
            enabledCount += EnablePatch(nameof(QuickAccessPanelFix), QuickAccessPanelFix.Enable);
            enabledCount += EnablePatch(nameof(ItemViewShortNameFix), ItemViewShortNameFix.Enable);

            Logger.LogInfo($"Completed: Korean Patch Fix ({enabledCount}/6 patch groups enabled)");
        }

        private int EnablePatch(string patchName, Action enable)
        {
            try
            {
                enable();
                Logger.LogInfo($"Enabled: {patchName}");
                return 1;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed: {patchName}\n{ex}");
                return 0;
            }
        }
    }
}
