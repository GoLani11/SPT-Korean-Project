using SPTarkov.Server.Core.Models.Spt.Mod;

namespace KoreanLocalizationStatus;

public record ModMetadata : IModMetadata
{
    public string ModGuid { get; init; } = "com.golani.korean.status";
    public string Name { get; init; } = "SPT Korean Localization Status";
    public string Author { get; init; } = "Golani";
    public List<string>? Contributors { get; init; }
    public SemanticVersioning.Version Version { get; init; } = new("2.2.0");
    public SemanticVersioning.Range SptVersion { get; init; } = new("4.1.0");
    public bool HasPrepatcher { get; init; } = false;
    public List<string>? Incompatibilities { get; init; } = null;
    public Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; } = null;
    public string? Url { get; init; } = null;
    public string License { get; init; } = "MIT";
}
