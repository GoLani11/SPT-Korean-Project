using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Utils.Json;
using System.Text.Json;

namespace SPT_Korean_Localization;

public record ModMetadata : IModMetadata
{
    public string ModGuid { get; init; } = "com.golani.makina.korean";
    public string Name { get; init; } = "SPT_Korean_Localization_(G&M)";
    public string Author { get; init; } = "Golani, Makina";
    public List<string>? Contributors { get; init; }
    public SemanticVersioning.Version Version { get; init; } = new("2.1.0");
    public SemanticVersioning.Range SptVersion { get; init; } = new(KoreanPatchFix.SptCompatibilityPolicy.FourOneServerRange);
    public bool HasPrepatcher { get; init; } = false;
    public List<string>? Incompatibilities { get; init; } = null;
    public Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; } = null;
    public string? Url { get; init; } = null;
    public string License { get; init; } = "MIT";
}

[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class KoreanPatcher(
    ISptLogger<KoreanPatcher> logger,
    LocaleTable localeTable)
    : IOnLoad
{
    private const string KoreanLocaleId = "kr";
    private const string BilingualLocaleId = "kr-en";
    private const string BilingualLocaleName = "한국어 (한영 병기)";

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var startTime = DateTime.Now;

        try
        {
            if (!localeTable.Global.TryGetValue(KoreanLocaleId, out var koreanLocale))
            {
                logger.Error("기존 한국어 언어파일을 찾을 수 없습니다. SPT_Runtime/SPT_Data/database/locales/global/kr.json을 확인하세요.");
                return Task.CompletedTask;
            }

            if (!localeTable.Menu.TryGetValue(KoreanLocaleId, out var koreanMenu))
            {
                logger.Error("기존 한국어 메뉴 언어파일을 찾을 수 없습니다.");
                return Task.CompletedTask;
            }

            // locale/kr.json 파일 경로 - 현재 DLL이 있는 위치 기준
            var assemblyLocation = Path.GetDirectoryName(typeof(KoreanPatcher).Assembly.Location);
            if (assemblyLocation == null)
            {
                logger.Error("어셈블리 위치를 찾을 수 없습니다.");
                return Task.CompletedTask;
            }

            var koreanPatch = LoadLocalePatch(assemblyLocation, "kr.json");
            var bilingualPatch = LoadLocalePatch(assemblyLocation, "kr-en.json");

            koreanLocale.AddTransformer(localeData =>
            {
                if (localeData == null)
                {
                    logger.Error("SPT 한국어 로케일 데이터가 비어 있어 한글 패치를 적용하지 못했습니다.");
                    return localeData;
                }

                foreach (var kvp in koreanPatch)
                {
                    localeData[kvp.Key] = kvp.Value;
                }

                localeData[BilingualLocaleId] = BilingualLocaleName;
                return localeData;
            });

            foreach (var (localeId, lazyLocale) in localeTable.Global.ToArray())
            {
                if (string.Equals(localeId, KoreanLocaleId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                lazyLocale.AddTransformer(localeData =>
                {
                    if (localeData != null)
                    {
                        localeData[BilingualLocaleId] = BilingualLocaleName;
                    }

                    return localeData;
                });
            }

            localeTable.Global[BilingualLocaleId] = new LazyLoad<GlobalLocaleDictionary>(() =>
            {
                var bilingualLocale = new GlobalLocaleDictionary();
                var baseKoreanLocale = koreanLocale.Value;
                if (baseKoreanLocale != null)
                {
                    foreach (var entry in baseKoreanLocale)
                    {
                        bilingualLocale[entry.Key] = entry.Value;
                    }
                }

                foreach (var entry in bilingualPatch)
                {
                    bilingualLocale[entry.Key] = entry.Value;
                }

                bilingualLocale[BilingualLocaleId] = BilingualLocaleName;
                return bilingualLocale;
            });
            localeTable.Menu[BilingualLocaleId] = new Dictionary<string, object>(koreanMenu);
            localeTable.Languages[BilingualLocaleId] = "Korean-English";

            var endTime = DateTime.Now;
            var elapsed = (endTime - startTime).TotalMilliseconds;

            logger.Success($"고라니 SPT 한글화 프로젝트가 정상적으로 적용되었습니다. 재밌는 SPT되세요!");
            logger.Info($"적용된 항목 줄 수: 한글판 {koreanPatch.Count}, 한영 병기판 {bilingualPatch.Count} (처리 시간: {elapsed:F2}ms)");
        }
        catch (Exception ex)
        {
            logger.Error($"고라니 SPT 한글화 프로젝트 적용 중 오류 발생: {ex.Message}");
            logger.Error($"상세 정보: {ex.StackTrace}");
        }

        return Task.CompletedTask;
    }

    private static Dictionary<string, string> LoadLocalePatch(string assemblyLocation, string fileName)
    {
        var localePath = Path.Combine(assemblyLocation, "locale", fileName);
        if (!File.Exists(localePath))
        {
            throw new FileNotFoundException($"한글 패치 파일을 찾을 수 없습니다: {localePath}", localePath);
        }

        var patch = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(localePath));
        if (patch == null || patch.Count == 0)
        {
            throw new InvalidDataException($"한글 패치 파일이 비어있습니다: {localePath}");
        }

        return patch;
    }
}
