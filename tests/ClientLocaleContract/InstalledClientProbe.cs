using Mono.Cecil;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

internal static class InstalledClientProbe
{
    private const string Strings = "System.Collections.Generic.Dictionary`2<System.String,System.String>";
    private const string StringMap = "System.Collections.Generic.IDictionary`2<System.String,System.String>";

    internal static JObject Check(string root, string sptVersion, string eftVersion)
    {
        var detectedBuild = FileVersionInfo.GetVersionInfo(Path.Combine(root, "EscapeFromTarkov.exe")).FileVersion;
        Require(detectedBuild == eftVersion, "Installed EFT executable build");
        if (sptVersion == "3.8.3")
        {
            var core = JObject.Parse(File.ReadAllText(Path.Combine(root, "Aki_Data/Server/configs/core.json")));
            Require((string)core["akiVersion"] == sptVersion, "Installed Aki version");
        }
        else
        {
            var version = FileVersionInfo.GetVersionInfo(Path.Combine(root, "SPT_Runtime/SPT.Server.exe")).ProductVersion;
            Require(version == sptVersion || version.StartsWith(sptVersion + "-RELEASE", StringComparison.Ordinal), "Installed SPT server version");
        }

        var path = Path.Combine(root, "EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll");
        using (var assembly = AssemblyDefinition.ReadAssembly(path))
        {
            var types = assembly.MainModule.Types;
            var manager = types.Single(type => type.Methods.Any(method => Signature(method, "UpdateLocales", "System.Void", "System.String", Strings)));
            var init = manager.Methods.Single(method => Signature(method, "Init", "System.Void", Strings));
            var global = manager.Methods.Single(method => Signature(method, "UpdateLocales", "System.Void", "System.String", Strings));
            var menu = manager.Methods.Single(method => method.Name == "UpdateMainMenuLocales" && !method.IsStatic
                && method.Parameters.Count == 2 && method.Parameters[0].ParameterType.FullName == "System.String");
            var locale = types.Single(type => type.FullName == menu.Parameters[1].ParameterType.FullName);
            Require(locale.BaseType.FullName == Strings, "Menu locale is a string dictionary");
            Require(locale.Methods.Any(method => Signature(method, ".ctor", "System.Void", StringMap)), "Native locale copy constructor");
            var font = manager.Methods.Single(method => !method.IsStatic && method.ReturnType.FullName == "System.Void"
                && method.Parameters.Count == 1 && method.Parameters[0].ParameterType.FullName == "System.String"
                && method.Parameters[0].Name == "localeType");
            Require(manager.Properties.Count(property => property.PropertyType.FullName == manager.FullName
                && property.GetMethod != null && property.GetMethod.IsStatic) == 1, "Manager singleton");
            Require(manager.Properties.Count(property => property.PropertyType.FullName == "System.String"
                && property.GetMethod != null && property.SetMethod != null && !property.GetMethod.IsStatic) == 1, "Native culture property");
            var defaultLanguage = manager.Properties.SingleOrDefault(property => property.Name == "DefaultLanguage")
                ?? types.Single(type => type.FullName == "EFT.UI.WelcomeScreen").Properties.Single(property => property.Name == "DefaultLanguage");
            Require(defaultLanguage.GetMethod.IsStatic && defaultLanguage.PropertyType.FullName == "System.String", "Native default language");

            var reload = types.SelectMany(type => type.Methods).Single(method => method.Name == "ReloadBackendLocale"
                && method.IsStatic && method.ReturnType.FullName == "System.Threading.Tasks.Task"
                && method.Parameters.Count == 3 && method.Parameters[2].ParameterType.FullName == "System.String");
            Require(reload.Parameters[2].Name == "locale", "Reload argument controls the requested backend locale");
            var state = (TypeReference)reload.CustomAttributes.Single(attribute =>
                attribute.AttributeType.FullName == "System.Runtime.CompilerServices.AsyncStateMachineAttribute").ConstructorArguments[0].Value;
            var moveNext = reload.DeclaringType.NestedTypes.Single(type => type.FullName == state.FullName).Methods.Single(method => method.Name == "MoveNext");
            var calls = moveNext.Body.Instructions.Where(instruction => instruction.Operand is MethodReference)
                .Select(instruction => ((MethodReference)instruction.Operand).Name).ToArray();
            foreach (var name in new[] { "GetMainMenuLocalization", "GetLocalization", "UpdateMainMenuLocales", "UpdateLocales", "UpdateApplicationLanguage" })
            {
                Require(calls.Contains(name), "Native reload sequence: " + name);
            }

            var uiTargets = new Dictionary<string, string[]>
            {
                ["EFT.UI.Ragfair.OfferItemDescription"] = new[] { "SetItemName", "Show" },
                ["EFT.UI.Ragfair.SubcategoryView"] = new[] { "SetExpandedStatus", "Show" },
                ["EFT.UI.Gestures.GesturesMenu"] = new[] { "InitPhraseGroups" },
                ["EFT.UI.Gestures.GesturesAudioSubItem"] = new[] { "Show" },
                ["EFT.UI.Gestures.GesturesMenuItem"] = new[] { "Show" },
                ["EFT.UI.InventoryScreenQuickAccessPanel"] = new[] { "Show" },
                ["EFT.UI.UiPools"] = new[] { "Init" }
            };
            foreach (var entry in uiTargets)
            {
                Require(types.Any(type => type.FullName == entry.Key && type.Methods.Any(method => entry.Value.Contains(method.Name))), "Existing UI patch: " + entry.Key);
            }
            foreach (var name in new[] { "EFT.UI.DragAndDrop.ItemView", "EFT.UI.InfoWindow", "EFT.UI.GridWindow" })
            {
                Require(types.Any(type => type.FullName == name), "Existing UI dependency: " + name);
            }

            string hash;
            using (var sha = SHA256.Create())
            using (var input = File.OpenRead(path))
            {
                hash = BitConverter.ToString(sha.ComputeHash(input)).Replace("-", "").ToLowerInvariant();
            }
            Console.WriteLine($"Installed client probe passed: SPT {sptVersion}, EFT {eftVersion}, five locale patch targets and existing UI entry points.");
            return new JObject
            {
                ["sptVersion"] = sptVersion,
                ["eftVersion"] = eftVersion,
                ["assemblySha256"] = hash,
                ["localeTargets"] = new JArray(new[] { init, global, menu, reload, font }.Select(method => method.FullName)),
                ["kind"] = "static metadata and native call-sequence inspection; not an in-game visual test"
            };
        }
    }

    private static bool Signature(MethodDefinition method, string name, string result, params string[] parameters)
    {
        return method.Name == name && !method.IsStatic && method.ReturnType.FullName == result
            && method.Parameters.Select(parameter => parameter.ParameterType.FullName).SequenceEqual(parameters);
    }

    private static void Require(bool condition, string label)
    {
        if (!condition) { throw new InvalidOperationException("Installed client probe failed: " + label); }
    }
}
