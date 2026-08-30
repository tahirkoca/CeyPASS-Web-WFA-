using System.Windows;
using System.Windows.Controls;

namespace CeyPASS.WPF.Controls;

public partial class CeypassThemeToggle : UserControl
{
    public CeypassThemeToggle()
    {
        InitializeComponent();
        Loaded += (_, _) => RefreshGlyph();
        CeypassTheme.Changed += OnThemeChanged;
        Unloaded += (_, _) => CeypassTheme.Changed -= OnThemeChanged;
    }

    private void OnThemeChanged(object? sender, EventArgs e) => Dispatcher.Invoke(RefreshGlyph);

    private void RefreshGlyph()
    {
        Glyph.Text = CeypassTheme.IsDark ? "☀" : "☾";
        BtnToggle.ToolTip = CeypassTheme.IsDark ? "Açık temaya geç" : "Koyu temaya geç";
    }

    private void BtnToggle_OnClick(object sender, RoutedEventArgs e) => CeypassTheme.Toggle();
}
