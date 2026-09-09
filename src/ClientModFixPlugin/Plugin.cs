using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
#if CLIENT_LOCALE_PROTOTYPE
using System.IO;
using System.Linq;
#endif

namespace KoreanPatchFix
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("com.spt-aki.core", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.SPT.core", BepInDependency.DependencyFlags.SoftDependency)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.GoLani.koreanpatchfix";
        public const string PluginName = "Korean Patch Fix";
        public const string PluginVersion = "2.1.0";

        private void Awake()
        {
            PluginLog.Initialize(Logger);

            var detectedVersion = SptVersionDetector.Detect();
#if CLIENT_LOCALE_PROTOTYPE
            try
            {
                var gameRoot = AppDomain.CurrentDomain.BaseDirectory;
                var bundleRoot = Path.Combine(Path.GetDirectoryName(typeof(Plugin).Assembly.Location), "SPT-Korean");
                var eftVersion = ClientLocaleBuild.ReadEftVersion(Path.Combine(gameRoot, "EscapeFromTarkov.exe"));
                var bundle = ClientLocaleBundle.Load(bundleRoot, gameRoot, detectedVersion, eftVersion);
                var gameAssembly = AppDomain.CurrentDomain.GetAssemblies().Single(assembly => assembly.GetName().Name == "Assembly-CSharp");
                ClientLocaleRuntime.Enable(new Harmony(PluginGuid + ".clientlocale"), gameAssembly, bundle);
                if (bundle.ProfileVersion != detectedVersion)
                    Logger.LogInfo($"SPT {detectedVersion} | 번역 기준 {bundle.TranslationVersion}: 호환성 검사 통과 — 기존 번역 사용 (프로필 {bundle.ProfileVersion}).");
                Logger.LogInfo($"[고라니 SPT 한글화 v{PluginVersion} | SPT {detectedVersion}] 번역 데이터 로드 및 언어 패치 적용 완료! 번역 기준 {bundle.TranslationVersion}, 한글판 / 한영 병기판 각 {bundle.SourceKeyCount:N0}개.");
            }
            catch (Exception error)
            {
                Logger.LogError($"[고라니 SPT 한글화 v{PluginVersion}] 적용하지 못했습니다: {error}");
                return;
            }
#else
            if (!SptVersionDetector.IsSupported(detectedVersion))
            {
                Logger.LogError(
                    $"Unsupported SPT version '{detectedVersion ?? "unknown"}'. " +
                    $"Korean Patch Fix supports {SptCompatibilityPolicy.SupportedVersionsDescription}.");
                return;
            }
#endif

            Logger.LogInfo($"Loading Korean Patch Fix for SPT {detectedVersion}");
            var harmony = new Harmony(PluginGuid);
            var enabledCount = 0;
            var skippedCount = 0;
            var failedCount = 0;

#if !CLIENT_LOCALE_PROTOTYPE
            EnablePatch(nameof(KoreanBilingualFontFix), () => KoreanBilingualFontFix.Enable(harmony), ref enabledCount, ref skippedCount, ref failedCount);
#endif
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
