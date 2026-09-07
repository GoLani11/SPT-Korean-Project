using KoreanLocalizationStatus;
using System.Text.Json.Nodes;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;

Console.OutputEncoding = new System.Text.UTF8Encoding(false);
if (args.Length != 2) throw new ArgumentException("Expected staged client root and installed SPT 4.1.5 root.");
var temporary = Path.Combine(Path.GetTempPath(), "spt-status-contract-" + Guid.NewGuid().ToString("N"));
var checks = 0;
try
{
    using var serverFile = File.OpenRead(Path.Combine(args[1], "SPT_Runtime/SPT.Server.dll"));
    using var pe = new PEReader(serverFile);
    var metadata = pe.GetMetadataReader();
    string? serverVersion = null;
    foreach (var handle in metadata.GetAssemblyDefinition().GetCustomAttributes())
    {
        var attribute = metadata.GetCustomAttribute(handle);
        if (attribute.Constructor.Kind != HandleKind.MemberReference) continue;
        var member = metadata.GetMemberReference((MemberReferenceHandle)attribute.Constructor);
        if (member.Parent.Kind != HandleKind.TypeReference) continue;
        var type = metadata.GetTypeReference((TypeReferenceHandle)member.Parent);
        if (metadata.GetString(type.Name) != "AssemblyInformationalVersionAttribute") continue;
        var blob = metadata.GetBlobReader(attribute.Value);
        blob.ReadUInt16();
        serverVersion = blob.ReadSerializedString();
    }
    var versionMatch = Regex.Match(serverVersion ?? "", @"\A(\d+\.\d+\.\d+)(?:-RELEASE)?(?:\+[\w.-]+)?\z");
    if (!versionMatch.Success || versionMatch.Groups[1].Value != "4.1.5") throw new Exception("Unexpected installed server version: " + serverVersion);
    Console.WriteLine("Installed server informational version: " + serverVersion);
    checks++;
    CopyTree(args[0], temporary);
    File.Copy(Path.Combine(args[1], "EscapeFromTarkov.exe"), Path.Combine(temporary, "EscapeFromTarkov.exe"));
    var english = Path.Combine(temporary, "SPT_Runtime/SPT_Data/database/locales/global/en.json");
    Directory.CreateDirectory(Path.GetDirectoryName(english)!);
    File.Copy(Path.Combine(args[1], "SPT_Runtime/SPT_Data/database/locales/global/en.json"), english);
    var result = StatusVerifier.Verify(temporary, "4.1.5");
    if (!result.Contains("v2.2.0 | SPT 4.1.5") || !result.Contains("적용 준비 완료")) throw new Exception(result);
    checks++;
    Console.WriteLine(result);
    Reject(() => StatusVerifier.Verify(temporary, "4.1.99"), "unknown version");
    Corrupt("BepInEx/plugins/GoLani.KoreanModFix.dll", "dll hash");
    Corrupt("BepInEx/plugins/SPT-Korean/locales/4.1.3/kr.json", "locale hash");
    var original = File.ReadAllText(english);
    File.WriteAllText(english, "{\"different\":\"source\"}");
    Reject(() => StatusVerifier.Verify(temporary, "4.1.5"), "wrong English");
    File.WriteAllText(english, original);
    var manifestPath = Path.Combine(temporary, "BepInEx/plugins/SPT-Korean/manifest.json");
    var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!;
    manifest["profiles"]!["4.1.5"]!["eftVersion"] = "0.0.0.0";
    File.WriteAllText(manifestPath, manifest.ToJsonString());
    Reject(() => StatusVerifier.Verify(temporary, "4.1.5"), "wrong EFT");
    File.Delete(Path.Combine(temporary, "EscapeFromTarkov.exe"));
    Reject(() => StatusVerifier.Verify(temporary, "4.1.5"), "missing client");
    Console.WriteLine($"Server status contract passed: {checks} cases; no database writes.");
}
finally { if (Directory.Exists(temporary)) Directory.Delete(temporary, true); }

void Corrupt(string relative, string label)
{
    var path = Path.Combine(temporary, relative);
    var bytes = File.ReadAllBytes(path);
    File.AppendAllText(path, " ");
    Reject(() => StatusVerifier.Verify(temporary, "4.1.5"), label);
    File.WriteAllBytes(path, bytes);
}
void Reject(Action action, string label)
{
    try { action(); } catch (InvalidDataException) { checks++; return; }
    throw new Exception("Expected rejection: " + label);
}
static void CopyTree(string source, string destination)
{
    foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
    {
        var target = Path.Combine(destination, Path.GetRelativePath(source, file));
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        File.Copy(file, target);
    }
}
