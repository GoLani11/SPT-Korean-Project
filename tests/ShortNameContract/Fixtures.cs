using System;
using System.Runtime.CompilerServices;

// These stand-ins test lifecycle/targeting/restoration with real Harmony, not Unity rendering.
namespace UnityEngine
{
    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
    }
    public sealed class RectTransform
    {
        public Vector2 offsetMin = new Vector2(4, -14);
        public Vector2 offsetMax = new Vector2(-4, 0);
    }
}
namespace TMPro
{
    public enum TextOverflowModes { Overflow, Ellipsis }
    public class TMP_Text
    {
        private string value;
        public string text { get => value; [MethodImpl(MethodImplOptions.NoInlining)] set => this.value = value; }
    }
    public sealed class TextMeshProUGUI : TMP_Text
    {
        public UnityEngine.RectTransform rectTransform = new UnityEngine.RectTransform();
        public bool enableAutoSizing;
        public float fontSize = 15, fontSizeMin = 10, fontSizeMax = 15, lineSpacing = 0;
        public TextOverflowModes overflowMode = TextOverflowModes.Ellipsis;
    }
}
namespace KoreanPatchFix
{
    internal static class ClientLocaleRuntime { internal static string Culture = "en"; internal static string CurrentCulture() => Culture; }
    internal static class ClientLocaleBundle
    {
        internal const string Korean = "kr", Bilingual = "kr-en";
        internal static bool Is(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }
    internal static class PluginLog
    {
        internal static void Error(string message) => throw new Exception(message);
        internal static void Warning(string message) => throw new Exception(message);
    }
}
namespace EFT.UI.DragAndDrop
{
    internal sealed class GridItemView
    {
        public TMPro.TextMeshProUGUI Caption = new TMPro.TextMeshProUGUI();
        public TMPro.TextMeshProUGUI OtherLabel = new TMPro.TextMeshProUGUI();
        internal string ItemName = "native item name";
        [MethodImpl(MethodImplOptions.NoInlining)] internal void Refresh() => method_26();
        [MethodImpl(MethodImplOptions.NoInlining)] private void method_26() { Caption.text = ItemName; }
        [MethodImpl(MethodImplOptions.NoInlining)] private void OtherWriter() { OtherLabel.text = "unrelated"; }
    }
}
namespace EFT.UI
{
    internal abstract class Window
    {
        public TMPro.TextMeshProUGUI Caption { get; } = new TMPro.TextMeshProUGUI();
        [MethodImpl(MethodImplOptions.NoInlining)] public void Show() { }
    }
    internal sealed class InfoWindow : Window
    {
        [MethodImpl(MethodImplOptions.NoInlining)] public void Show(string title) { Caption.text = title; }
    }
    internal sealed class GridWindow : Window
    {
        [MethodImpl(MethodImplOptions.NoInlining)] public object Show(string title) { Caption.text = title; return this; }
    }
}
