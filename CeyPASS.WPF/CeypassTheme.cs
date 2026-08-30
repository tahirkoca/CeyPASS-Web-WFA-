using System.IO;
using System.Windows;
using System.Windows.Media;
using DevExpress.Xpf.Core;

namespace CeyPASS.WPF;

internal static class CeypassTheme
{
    private const string LightSource = "Themes/Theme.Light.xaml";
    private const string DarkSource = "Themes/Theme.Dark.xaml";

    public static bool IsDark { get; private set; }

    public static event EventHandler? Changed;

    public static void ApplySaved() => Apply(Load(), persist: false);

    public static void Toggle() => Apply(!IsDark);

    public static void Apply(bool dark, bool persist = true)
    {
        IsDark = dark;
        ApplicationThemeHelper.ApplicationThemeName = dark ? Theme.Win11DarkName : Theme.Win11LightName;
        SwapDictionary(dark);
        if (persist)
            Save(dark);
        Changed?.Invoke(null, EventArgs.Empty);
    }

    private static void SwapDictionary(bool dark)
    {
        var app = System.Windows.Application.Current;
        if (app?.Resources.MergedDictionaries is null)
            return;

        var source = dark ? DarkSource : LightSource;
        var uri = new Uri(source, UriKind.Relative);
        var dicts = app.Resources.MergedDictionaries;
        for (var i = 0; i < dicts.Count; i++)
        {
            var src = dicts[i].Source?.OriginalString ?? "";
            if (src.Contains("Theme.Light.xaml", StringComparison.OrdinalIgnoreCase)
                || src.Contains("Theme.Dark.xaml", StringComparison.OrdinalIgnoreCase))
            {
                dicts[i] = new ResourceDictionary { Source = uri };
                return;
            }
        }

        dicts.Insert(0, new ResourceDictionary { Source = uri });
    }

    private static bool Load()
    {
        try
        {
            var path = PrefPath();
            if (File.Exists(path))
                return string.Equals(File.ReadAllText(path).Trim(), "dark", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            // yok say
        }

        return false;
    }

    private static void Save(bool dark)
    {
        try
        {
            var path = PrefPath();
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(path, dark ? "dark" : "light");
        }
        catch
        {
            // yok say
        }
    }

    private static string PrefPath()
        => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CeyPASS",
            "ui-theme.txt");
}

internal static class ThemeBrushes
{
    public static Brush Get(string key, Color fallback)
    {
        if (System.Windows.Application.Current?.TryFindResource(key) is Brush b)
            return b;
        return new SolidColorBrush(fallback);
    }

    public static Color ColorOf(string key, Color fallback)
    {
        if (System.Windows.Application.Current?.TryFindResource(key) is SolidColorBrush b)
            return b.Color;
        if (System.Windows.Application.Current?.TryFindResource(key) is Color c)
            return c;
        return fallback;
    }
}
