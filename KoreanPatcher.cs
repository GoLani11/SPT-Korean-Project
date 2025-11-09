using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using System.Text.Json;

namespace SPT_Korean_Localization;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.golani.makina.korean";
    public override string Name { get; init; } = "SPT_Korean_Localization_(G&M)";
    public override string Author { get; init; } = "Golani, Makina";
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new("1.4.0");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; } = null;
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; } = null;
    public override string? Url { get; init; } = null;
    public override bool? IsBundleMod { get; init; } = null;
    public override string License { get; init; } = "MIT";
}

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader + 1)]
public class KoreanPatcher(
    ISptLogger<KoreanPatcher> logger,
    DatabaseService databaseService)
    : IOnLoad
{
    public Task OnLoad()
    {
        var startTime = DateTime.Now;

        try
        {
            // 한국어 로케일 데이터 가져오기
            if (!databaseService.GetLocales().Global.TryGetValue("kr", out var koreanLocale))
            {
                logger.Error("기존 한국어 언어파일을 찾을 수 없습니다. .../SPT_Data/Server/database/locales/global/kr.json 이 있는지 확인하세요.");
                return Task.CompletedTask;
            }

            // locale/kr.json 파일 경로 - 현재 DLL이 있는 위치 기준
            var assemblyLocation = Path.GetDirectoryName(typeof(KoreanPatcher).Assembly.Location);
            if (assemblyLocation == null)
            {
                logger.Error("어셈블리 위치를 찾을 수 없습니다.");
                return Task.CompletedTask;
            }

            var localePath = Path.Combine(assemblyLocation, "locale", "kr.json");

            if (!File.Exists(localePath))
            {
                logger.Error($"한글 패치 파일을 찾을 수 없습니다: {localePath}");
                return Task.CompletedTask;
            }

            // JSON 파일 읽기
            var jsonContent = File.ReadAllText(localePath);
            var koreanPatch = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);

            if (koreanPatch == null || koreanPatch.Count == 0)
            {
                logger.Warning("한글 패치 파일이 비어있습니다.");
                return Task.CompletedTask;
            }

            // Lazy loaded 로케일에 변환기 추가
            koreanLocale.AddTransformer(localeData =>
            {
                // 기존 로케일에 패치 적용
                foreach (var kvp in koreanPatch)
                {
                    localeData[kvp.Key] = kvp.Value;
                }
                return localeData;
            });

            var endTime = DateTime.Now;
            var elapsed = (endTime - startTime).TotalMilliseconds;

            logger.Success($"고라니 SPT 한글화 프로젝트가 정상적으로 적용되었습니다. 재밌는 SPT되세요!");
            logger.Info($"적용된 항목 줄 수: {koreanPatch.Count} (처리 시간: {elapsed:F2}ms)");
        }
        catch (Exception ex)
        {
            logger.Error($"고라니 SPT 한글화 프로젝트 적용 중 오류 발생: {ex.Message}");
            logger.Error($"상세 정보: {ex.StackTrace}");
        }

        return Task.CompletedTask;
    }
}
