using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// Model the observed native call sequence, including old clients' cache and current clients' reload.
// The contract executes actual Harmony patches against these methods, without starting Unity or a server.
internal sealed class NativeLocale : Dictionary<string, string>
{
    public NativeLocale(IDictionary<string, string> source) : base(source, StringComparer.OrdinalIgnoreCase) { }
}

internal sealed class NativeLocaleManager
{
    public static NativeLocaleManager Instance { get; set; } = new NativeLocaleManager();
    public static string DefaultLanguage { get; set; } = "en";
    public string Culture { get; set; } = "en";
    public Dictionary<string, string> Languages;
    public readonly Dictionary<string, NativeLocale> Locales = new Dictionary<string, NativeLocale>();
    public readonly HashSet<string> MainMenuCultures = new HashSet<string>();
    public readonly HashSet<string> GlobalCultures = new HashSet<string>();
    public string LastFont;
    public string LastApplicationCulture;
    public int ReloadEvents;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Init(Dictionary<string, string> languages) { Languages = languages; }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void UpdateMainMenuLocales(string culture, NativeLocale locale)
    {
        MainMenuCultures.Add(culture);
        Merge(culture, locale);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void UpdateLocales(string culture, Dictionary<string, string> locale)
    {
        GlobalCultures.Add(culture);
        Merge(culture, new NativeLocale(locale));
    }

    private void Merge(string culture, IDictionary<string, string> source)
    {
        if (!Locales.TryGetValue(culture, out var locale))
        {
            Locales[culture] = new NativeLocale(source);
            return;
        }
        foreach (var entry in source) { locale[entry.Key] = entry.Value; }
    }

    public void UpdateApplicationLanguage()
    {
        if (LastApplicationCulture == Culture || !GlobalCultures.Contains(Culture)) { return; }
        LastApplicationCulture = Culture;
        ApplyFonts(Culture);
        ReloadEvents++;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void ApplyFonts(string localeType) { LastFont = localeType; }
}

internal sealed class NativeBackend
{
    public readonly List<string> Requests = new List<string>();
    public readonly Dictionary<string, string> Menu = new Dictionary<string, string> { ["menu-only"] = "메뉴" };
    public readonly Dictionary<string, string> Global = new Dictionary<string, string>
    {
        ["server-mod-key"] = "다른 모드의 문구",
        ["5c1242fa86f7742aa04fed52"] = "SERVER VALUE"
    };
    public bool Fail;

    public async Task<NativeLocale> GetMainMenuLocalization(string locale)
    {
        Record("menu", locale);
        await Task.Yield();
        return new NativeLocale(Menu);
    }

    public async Task<Dictionary<string, string>> GetLocalization(string locale)
    {
        Record("global", locale);
        await Task.Yield();
        return Global;
    }

    private void Record(string endpoint, string locale)
    {
        Requests.Add(endpoint + ":" + locale);
        if (locale == "kr-en") { throw new InvalidOperationException("The fixture server has no kr-en endpoint."); }
        if (Fail) { throw new InvalidOperationException("fixture network failure"); }
    }
}

internal sealed class NativeSession
{
    public NativeBackend Backend;
}

internal static class NativeDataPreparation
{
    public static bool CacheGlobals;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static async Task ReloadBackendLocale(NativeBackend backEnd, NativeSession session, string locale)
    {
        if (string.IsNullOrEmpty(locale)) { locale = NativeLocaleManager.DefaultLanguage; }
        var manager = NativeLocaleManager.Instance;
        if (backEnd != null && !manager.MainMenuCultures.Contains(locale))
        {
            var menu = await backEnd.GetMainMenuLocalization(locale);
            manager.UpdateMainMenuLocales(locale, menu);
        }
        if (session != null && (!CacheGlobals || !manager.GlobalCultures.Contains(locale)))
        {
            var global = await session.Backend.GetLocalization(locale);
            manager.UpdateLocales(locale, global);
        }
        manager.UpdateApplicationLanguage();
    }
}
