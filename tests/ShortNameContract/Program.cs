using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using KoreanPatchFix;
using EFT.UI;
using EFT.UI.DragAndDrop;
using TMPro;

internal static class Program
{
    private static int checks;
    public static int Main(string[] args)
    {
        if (args.Length >= 2 && args[0] == "--unity-mono")
            return UnityMonoHost.Run(args[1], Assembly.GetExecutingAssembly().Location, args.Skip(2).ToArray());
        var harmony = new Harmony("com.golani.shortname.contract");
        try
        {
            var result = ItemViewShortNameFix.Enable(harmony);
            Expect(result.Enabled && result.Detail.EndsWith("method_26"), "Resolve the obfuscated writer, excluding unrelated text setters");
            var english = new GridItemView(); english.Refresh();
            Expect(!english.Caption.enableAutoSizing && english.Caption.fontSize == 15, "English starts with native layout");
            foreach (var language in new[] { "kr", "kr-en" })
            {
                ClientLocaleRuntime.Culture = language;
                // Construct after patch registration: no pool scan or prefab mutation is available.
                var item = new GridItemView { ItemName = "한글 이름 (English)" }; item.Refresh();
                Expect(item.Caption.text == item.ItemName, "Keep native text, tags, examination state and localization");
                Expect(item.Caption.enableAutoSizing && item.Caption.fontSizeMin == 8 && item.Caption.fontSizeMax == 12, "Late-created item gets sizing");
                Expect(item.Caption.rectTransform.offsetMin.y == -17 && item.Caption.rectTransform.offsetMax.x == -3, "Adjust the exact Caption rect");
                Expect(!item.OtherLabel.enableAutoSizing, "Unrelated child text remains untouched");
                item.Caption.enableAutoSizing = false; item.Refresh();
                Expect(item.Caption.enableAutoSizing, "Reused or refreshed view is corrected again");
                var info = new InfoWindow(); info.Show("정보");
                Expect(info.Caption.rectTransform.offsetMin.x == 25 && !info.Caption.enableAutoSizing, "Late info window gets only its original margin correction");
                var grid = new GridWindow(); Expect(ReferenceEquals(grid.Show("가방"), grid), "Native window return value survives");
                Expect(grid.Caption.enableAutoSizing && grid.Caption.overflowMode == TextOverflowModes.Overflow, "Late grid window gets sizing and overflow");
                ClientLocaleRuntime.Culture = "en";
                item.Refresh(); info.Show("Info"); grid.Show("Bag");
                Expect(!item.Caption.enableAutoSizing && item.Caption.fontSizeMin == 10 && item.Caption.fontSizeMax == 15 && item.Caption.fontSize == 15, "Language switch restores native font values");
                Expect(item.Caption.rectTransform.offsetMin.x == 4 && item.Caption.rectTransform.offsetMin.y == -14, "Language switch restores native item margins");
                Expect(info.Caption.rectTransform.offsetMin.x == 4, "Restore info margins");
                Expect(!grid.Caption.enableAutoSizing && grid.Caption.overflowMode == TextOverflowModes.Ellipsis, "Restore grid overflow");
                ClientLocaleRuntime.Culture = language; item.Refresh();
                Expect(item.Caption.enableAutoSizing, "Switching back reapplies the correction");
            }
            Console.WriteLine($"Short-name contract passed: {checks} checks with real Harmony; UI stand-ins, not visual rendering.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
        finally { harmony.UnpatchSelf(); }
    }
    private static void Expect(bool condition, string description)
    {
        if (!condition) throw new Exception(description);
        checks++;
    }
}
