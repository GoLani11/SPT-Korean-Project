using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;

namespace KoreanPatchFix
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("com.spt-aki.core", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.SPT.core", BepInDependency.DependencyFlags.SoftDependency)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.GoLani.koreanpatchfix";
        public const string PluginName = "Korean Patch Fix";
        public const string PluginVersion = "2.0.1";

        private void Awake()
        {
            PluginLog.Initialize(Logger);

            var detectedVersion = SptVersionDetector.Detect();
            if (!SptVersionDetector.IsSupported(detectedVersion))
            {
                Logger.LogError(
                    $"Unsupported SPT version '{detectedVersion ?? "unknown"}'. " +
                    "Korean Patch Fix supports 3.8.3, 3.9.8, 3.10.5, 3.11.4, 4.0.13, and 4.1.2 only.");
                return;
            }

            Logger.LogInfo($"Loading Korean Patch Fix for SPT {detectedVersion}");
            var harmony = new Harmony(PluginGuid);
            var enabledCount = 0;
            var skippedCount = 0;
            var failedCount = 0;

            EnablePatch(nameof(FleaMarketItemNameFix), () => FleaMarketItemNameFix.Enable(harmony), ref enabledCount, ref skippedCount, ref failedCount);
            EnablePatch(nameof(FleaMarketItemCategoryFix), () => FleaMarketItemCategoryFix.Enable(harmony), ref enabledCount, ref skippedCount, ref failedCount);
            EnablePatch(nameof(GesturesMenuFix), () => GesturesMenuFix.Enable(harmony), ref enabledCount, ref skippedCount, ref failedCount);
            EnablePatch(nameof(RewardNameFix), () => RewardNameFix.Enable(harmony), ref enabledCount, ref skippedCount, ref failedCount);
            EnablePatch(nameof(QuickAccessPanelFix), () => QuickAccessPanelFix.Enable(harmony), ref enabledCount, ref skippedCount, ref failedCount);
            EnablePatch(nameof(ItemViewShortNameFix), () => ItemViewShortNameFix.Enable(harmony), ref enabledCount, ref skippedCount, ref failedCount);

            Logger.LogInfo(
                $"Completed Korean Patch Fix for SPT {detectedVersion}: " +
                $"{enabledCount} enabled, {skippedCount} unavailable, {failedCount} failed");
        }

        private void EnablePatch(
            string patchName,
            Func<PatchResult> enable,
            ref int enabledCount,
            ref int skippedCount,
            ref int failedCount)
        {
            try
            {
                var result = enable();
                if (result.Enabled)
                {
                    enabledCount++;
                    Logger.LogInfo($"Enabled: {patchName} ({result.Detail})");
                    return;
                }

                skippedCount++;
                Logger.LogInfo($"Unavailable on this version: {patchName} ({result.Detail})");
            }
            catch (Exception ex)
            {
                failedCount++;
                Logger.LogError($"Failed: {patchName}\n{ex}");
            }
        }
    }

    internal static class PluginLog
    {
        private static ManualLogSource _logger;

        internal static void Initialize(ManualLogSource logger)
        {
            _logger = logger;
        }

        internal static void Error(string message)
        {
            _logger?.LogError(message);
        }

        internal static void Warning(string message)
        {
            _logger?.LogWarning(message);
        }

        internal static void Debug(string message)
        {
            _logger?.LogDebug(message);
        }
    }
}
