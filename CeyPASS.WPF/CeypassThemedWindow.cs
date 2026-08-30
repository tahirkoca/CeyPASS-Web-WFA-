using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DevExpress.Xpf.Core;

namespace CeyPASS.WPF;

public class CeypassThemedWindow : ThemedWindow
{
    protected CeypassThemedWindow()
    {
        ControlBoxButtonSet = ControlBoxButtons.None;
        ShowIcon = true;
        FontFamily = new FontFamily("Segoe UI");

        try
        {
            Icon = new BitmapImage(new Uri("pack://application:,,,/Assets/ceyLogo.png"));
        }
        catch
        {
            // Paketlenmiş kaynak bulunamazsa uygulama akışını bozmayalım.
        }

        Loaded += OnThemeWindowLoaded;
        Unloaded += OnThemeWindowUnloaded;
    }

    private void OnThemeWindowLoaded(object sender, RoutedEventArgs e)
    {
        ApplyThemeChrome();
        CeypassTheme.Changed -= OnThemeChanged;
        CeypassTheme.Changed += OnThemeChanged;
    }

    private void OnThemeWindowUnloaded(object sender, RoutedEventArgs e)
    {
        CeypassTheme.Changed -= OnThemeChanged;
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(ApplyThemeChrome);
    }

    private void ApplyThemeChrome()
    {
        if (TryFindResource("Brush.HeaderBg") is Brush header)
            HeaderBackground = header;
        if (TryFindResource("Brush.TextPrimary") is Brush fg)
            HeaderForeground = fg;
        if (TryFindResource("Brush.PageBg") is Brush page)
            Background = page;
    }
}
