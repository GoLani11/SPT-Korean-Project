using GoLani.KoreanLocaleProbe50;
using Mono.Cecil;
using System.Text.Json.Nodes;

if (args.Length != 2) throw new ArgumentException("Expected game root and compiled plugin path.");
int checks = 0;
void Check(bool value, string label)
{
    if (!value) throw new InvalidOperationException(label);
    checks++;
}
void Reject(Action action, string label)
{
    try { action(); }
    catch (InvalidDataException) { checks++; return; }
    throw new InvalidOperationException(label);
}

Check(ProbePolicy.TryTranslate("kr", "Character", "캐릭터", out var value) && value == "캐릭터 [KR5]", "Native Korean marker");
Check(ProbePolicy.TryTranslate("kr", "Trading", "TRADING", out value) && value == "거래 [KR5]", "English fallback in Korean");
foreach (string language in new[] { "en", "ru", "kr-en", "", "ko" })
    Check(!ProbePolicy.TryTranslate(language, "SETTINGS", "설정", out value), "Other language is untouched");
Check(!ProbePolicy.TryTranslate("kr", "Character", "모드 전용 캐릭터", out value) && value == "모드 전용 캐릭터", "Preserve another mod");
Check(!ProbePolicy.TryTranslate("kr", "Character", "캐릭터 [KR5]", out value) && value == "캐릭터 [KR5]", "Repeated update is idempotent");
Check(!ProbePolicy.TryTranslate("kr", "OtherKey", "캐릭터", out _), "Unrelated key is untouched");
Check(!ProbePolicy.TryTranslate("kr", "Character", null, out _), "Do not fill a missing source");
ProbeGuard.Validate(args[0]);
checks++;

string temporary = Path.Combine(Path.GetTempPath(), "kr5-guard-" + Guid.NewGuid().ToString("N"));
try
{
    foreach (string relative in new[] { "SPT_Runtime/SPT.Server.deps.json", "SPT_Runtime/SPT_Data/configs/core.json", "SPT_Runtime/SPT_Data/database/locales/global/en.json" })
    {
        string target = Path.Combine(temporary, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        File.Copy(Path.Combine(args[0], relative), target);
    }
    string depsPath = Path.Combine(temporary, "SPT_Runtime/SPT.Server.deps.json");
    string originalDeps = File.ReadAllText(depsPath);
    File.WriteAllText(depsPath, originalDeps.Replace(ProbeGuard.PackageVersion, "5.0.0-BLEEDINGEDGE+different"));
    Reject(() => ProbeGuard.Validate(temporary), "Different test build must be rejected");
    File.WriteAllText(depsPath, originalDeps);
    string corePath = Path.Combine(temporary, "SPT_Runtime/SPT_Data/configs/core.json");
    string originalCore = File.ReadAllText(corePath);
    var core = JsonNode.Parse(originalCore)!;
    core["compatibleTarkovVersion"] = "1.1.5.0.99999";
    File.WriteAllText(corePath, core.ToJsonString());
    Reject(() => ProbeGuard.Validate(temporary), "Different client build must be rejected");
    File.WriteAllText(corePath, originalCore);
    File.AppendAllText(Path.Combine(temporary, "SPT_Runtime/SPT_Data/database/locales/global/en.json"), "\n");
    Reject(() => ProbeGuard.Validate(temporary), "Changed source fingerprint must be rejected");
}
finally { Directory.Delete(temporary, true); }

using var game = AssemblyDefinition.ReadAssembly(Path.Combine(args[0], "BepInEx/interop/Assembly-CSharp.dll"));
var manager = game.MainModule.Types.Single(x => x.FullName == "EFT.LocalizationManager");
var update = manager.Methods.Single(x => x.Name == "UpdateLocales");
Check(!update.IsStatic && update.ReturnType.FullName == "System.Void", "Real native update shape");
Check(update.Parameters.Select(x => x.ParameterType.FullName).SequenceEqual(new[] {
    "System.String", "Il2CppSystem.Collections.Generic.Dictionary`2<System.String,System.String>", "System.Boolean"
}), "Real three-argument IL2CPP signature");
Check(manager.Fields.Any(x => x.Name.StartsWith("NativeMethodInfoPtr_UpdateLocales_")), "Native method pointer exists");
Check(manager.Methods.Any(x => x.Name == "LocalizedValue" && x.IsPublic && x.Parameters.Count == 1), "Post-merge lookup exists");
using var plugin = AssemblyDefinition.ReadAssembly(args[1]);
Check(plugin.CustomAttributes.Single(x => x.AttributeType.Name == "TargetFrameworkAttribute").ConstructorArguments[0].Value.ToString() == ".NETCoreApp,Version=v6.0", "Plugin targets bundled .NET 6");
var entry = plugin.MainModule.Types.Single(x => x.Name == "Plugin");
Check(entry.BaseType.FullName == "BepInEx.Unity.IL2CPP.BasePlugin", "IL2CPP entry point");
Check(entry.CustomAttributes.Any(x => x.AttributeType.Name == "BepInPlugin"), "Discoverable BepInEx plugin");
Check(entry.CustomAttributes.Any(x => x.AttributeType.Name == "BepInProcess" && x.ConstructorArguments[0].Value.ToString() == "EscapeFromTarkov.exe"), "Game process restriction");
Check(plugin.MainModule.AssemblyReferences.All(x => x.Name != "BepInEx"), "No legacy Mono BepInEx reference");
Console.WriteLine($"KR5 CONTRACT PASS: {checks} checks. Native game execution and visual verification are still pending.");
