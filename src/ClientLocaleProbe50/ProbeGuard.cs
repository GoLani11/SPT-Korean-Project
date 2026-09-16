using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace GoLani.KoreanLocaleProbe50;

internal static class ProbeGuard
{
    internal const string PackageVersion = "5.0.0-BLEEDINGEDGEMODS+ec15a40.20260914";
    internal const string EftVersion = "1.1.5.0.47242";
    internal const string EnglishHash = "af3e9fc26a109d2591d94ebcb4c3abb50f97f58dfae2206276a36a38e3d2dd3b";

    internal static void Validate(string gameRoot)
    {
        string runtime = Path.Combine(gameRoot, "SPT_Runtime");
        using var deps = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(runtime, "SPT.Server.deps.json")));
        var servers = deps.RootElement.GetProperty("libraries").EnumerateObject()
            .Where(x => x.Name.StartsWith("SPT.Server/", StringComparison.Ordinal)).ToArray();
        if (servers.Length != 1 || servers[0].Name != "SPT.Server/" + PackageVersion)
            throw new InvalidDataException("Unsupported SPT test build. Probe is disabled.");
        using var core = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(runtime, "SPT_Data", "configs", "core.json")));
        if (core.RootElement.GetProperty("compatibleTarkovVersion").GetString() != EftVersion)
            throw new InvalidDataException("Unsupported EFT build. Probe is disabled.");
        string english = Path.Combine(runtime, "SPT_Data", "database", "locales", "global", "en.json");
        using var stream = File.OpenRead(english);
        using var sha = SHA256.Create();
        string hash = Convert.ToHexString(sha.ComputeHash(stream)).ToLowerInvariant();
        if (hash != EnglishHash)
            throw new InvalidDataException("English source fingerprint differs. Probe is disabled.");
    }
}
