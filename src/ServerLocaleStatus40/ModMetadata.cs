using SPTarkov.Server.Core.Models.Spt.Mod;

namespace KoreanLocalizationStatus;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.golani.korean.status";
    public override string Name { get; init; } = "SPT Korean Localization Status";
    public override string Author { get; init; } = "Golani";
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new("2.2.0");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("4.0.13");
    public override List<string>? Incompatibilities { get; init; } = null;
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; } = null;
    public override string? Url { get; init; } = null;
    public override bool? IsBundleMod { get; init; } = null;
    public override string License { get; init; } = "MIT";
}
