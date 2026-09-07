using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace KoreanLocalizationStatus;

// This module only reads files. It never resolves or changes the server locale database.
internal static class StatusVerifier
{
    internal static string Verify(string root, string version)
    {
        var plugins = Path.Combine(root, "BepInEx", "plugins");
        var bundle = Path.Combine(plugins, "SPT-Korean");
        using var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(bundle, "manifest.json")));
        var data = manifest.RootElement;
        if (data.GetProperty("schemaVersion").GetInt32() != 1 || !data.GetProperty("profiles").TryGetProperty(version, out var profile))
            throw new InvalidDataException($"SPT {version} 번역 프로필이 없습니다.");
        if (data.GetProperty("clientVersion").GetString() != "2.2.0")
            throw new InvalidDataException("알림 모듈과 클라이언트 버전이 다릅니다.");
        CheckHash(Path.Combine(plugins, "GoLani.KoreanModFix.dll"), data.GetProperty("clientDllSha256").GetString());
        if (!File.Exists(Path.Combine(root, "EscapeFromTarkov.exe")))
            throw new InvalidDataException("EscapeFromTarkov.exe가 없습니다. 게임 파일 설치 후 테스트할 수 있습니다.");
        var eft = KoreanPatchFix.ClientLocaleBuild.ReadEftVersion(Path.Combine(root, "EscapeFromTarkov.exe"));
        if (eft != profile.GetProperty("eftVersion").GetString())
            throw new InvalidDataException($"EFT 빌드가 다릅니다: {eft}");
        var translation = profile.GetProperty("translationVersion").GetString()!;
        if (!Regex.IsMatch(translation, @"\A\d+\.\d+\.\d+\z")) throw new InvalidDataException("번역 버전 형식 오류");
        var locales = new Dictionary<string, Dictionary<string, string>>();
        foreach (var name in new[] { "en.json", "kr.json", "kr-en.json" })
        {
            var path = Path.Combine(bundle, "locales", translation, name);
            CheckHash(path, profile.GetProperty("sha256").GetProperty(name).GetString());
            locales[name] = ReadLocale(path);
        }
        var english = locales["en.json"];
        foreach (var locale in locales.Values)
            if (!locale.Keys.SequenceEqual(english.Keys)) throw new InvalidDataException("번역 키 또는 순서가 원문과 다릅니다.");
        var relative = profile.GetProperty("englishPath").GetString()!;
        if (relative != "SPT/SPT_Data/database/locales/global/en.json" && relative != "SPT_Runtime/SPT_Data/database/locales/global/en.json")
            throw new InvalidDataException("원문 경로 형식 오류");
        var installed = ReadLocale(Path.Combine(root, relative));
        if (installed.Count != english.Count || english.Any(entry => !installed.TryGetValue(entry.Key, out var value) || value != entry.Value))
            throw new InvalidDataException("설치된 서버 원문과 번역 기준이 다릅니다.");
        return $"[고라니 SPT 한글화 v2.2.0 | SPT {version}] 한글화 파일 검증 및 적용 준비 완료! "
            + $"(번역 기준 {translation}, 한글판 {locales["kr.json"].Count:N0} / 한영 병기판 {locales["kr-en.json"].Count:N0}개)";
    }

    private static void CheckHash(string path, string? expected)
    {
        using var stream = File.OpenRead(path);
        if (!string.Equals(Convert.ToHexString(SHA256.HashData(stream)), expected, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"파일 검증 실패: {Path.GetFileName(path)}");
    }

    private static Dictionary<string, string> ReadLocale(string path)
    {
        using var json = JsonDocument.Parse(File.ReadAllBytes(path));
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var entry in json.RootElement.EnumerateObject())
            if (entry.Value.ValueKind != JsonValueKind.String || !values.TryAdd(entry.Name, entry.Value.GetString()!))
                throw new InvalidDataException($"잘못되거나 중복된 번역 키: {entry.Name}");
        return values;
    }
}
