using System.Reflection;
using System.Text.RegularExpressions;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
#if SPT_40
using SPTarkov.Server.Core.Models.Utils;
#else
using SPTarkov.Common.Models.Logging;
#endif

namespace KoreanLocalizationStatus;

#if SPT_40
[Injectable(TypePriority = OnLoadOrder.PostSptModLoader + 1)]
#else
[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
#endif
public class KoreanStatus(ISptLogger<KoreanStatus> logger) : IOnLoad
{
#if SPT_40
    public Task OnLoad()
#else
    public Task OnLoadAsync(CancellationToken cancellationToken)
#endif
    {
        try
        {
            // Start from this mod, so launching a server from another working directory is safe.
            var root = new DirectoryInfo(Path.GetDirectoryName(typeof(KoreanStatus).Assembly.Location)!);
            while (!Directory.Exists(Path.Combine(root.FullName, "BepInEx")))
                root = root.Parent ?? throw new InvalidDataException("BepInEx 게임 폴더를 찾을 수 없습니다.");
            var server = Assembly.GetEntryAssembly() ?? throw new InvalidDataException("서버 버전을 확인할 수 없습니다.");
            var version = server.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            var match = Regex.Match(version ?? "", @"\A(\d+\.\d+\.\d+)(?:-RELEASE)?(?:\+[\w.-]+)?\z");
            if (!match.Success) throw new InvalidDataException($"지원 여부를 확인할 수 없는 서버 버전: {version}");
            var result = StatusVerifier.Verify(root.FullName, match.Groups[1].Value);
            logger.Success(result);
            logger.Info("번역은 게임 실행 시 클라이언트에 적용됩니다. 재밌는 SPT 되세요!");
        }
        catch (Exception error)
        {
            logger.Error($"[고라니 SPT 한글화 v2.2.0] 적용 준비 확인 실패: {error.Message}");
        }
        return Task.CompletedTask;
    }
}
