using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace CeyPASS.WPF;

public enum UiDialogKind
{
    Info,
    Success,
    Warning,
    Error,
    Confirm
}

public enum UiDialogResult
{
    None,
    Primary,
    Secondary
}

/// <summary>
/// Modern, uygulama temasıyla uyumlu diyalog (eski MessageBox yerine).
/// </summary>
public static class UiDialog
{
    public static void Info(string message, string title = "Bilgi", Window? owner = null)
        => Show(message, title, UiDialogKind.Info, "Tamam", null, owner);

    /// <summary>Başarı: modal yerine toast (akışı kesmez).</summary>
    public static void Success(string message, string title = "Başarılı", Window? owner = null)
        => UiToast.Success(message, title);

    /// <summary>Başarı toast + Geri al (~7 sn).</summary>
    public static void SuccessWithUndo(string message, Action undo, string title = "Başarılı", string undoLabel = "Geri al")
        => UiToast.SuccessWithUndo(message, undo, title, undoLabel);

    /// <summary>Bilgi toast (talimat / onay gerektiren Info için <see cref="Info"/> kullanın).</summary>
    public static void InfoToast(string message, string title = "Bilgi")
        => UiToast.Info(message, title);

    public static void Warning(string message, string title = "Uyarı", Window? owner = null)
        => Show(message, title, UiDialogKind.Warning, "Tamam", null, owner);

    public static void Error(string message, string title = "Hata", Window? owner = null)
        => Show(message, title, UiDialogKind.Error, "Tamam", null, owner);

    public static bool Confirm(
        string message,
        string title = "Onay",
        Window? owner = null,
        string yesText = "Evet",
        string noText = "Hayır")
        => Show(message, title, UiDialogKind.Confirm, yesText, noText, owner) == UiDialogResult.Primary;

    public static UiDialogResult Show(
        string message,
        string title,
        UiDialogKind kind,
        string primaryText = "Tamam",
        string? secondaryText = null,
        Window? owner = null)
    {
        title = UiPageTitles.Friendly(title);
        owner ??= UiDialogChrome.ResolveOwner();
        var dlg = new UiDialogWindow(message, title, kind, primaryText, secondaryText);
        if (owner != null)
        {
            dlg.Owner = owner;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }
        else
        {
            dlg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        dlg.ShowDialog();
        return dlg.Result;
    }
}

internal sealed class UiDialogWindow : Window
{
    public UiDialogResult Result { get; private set; } = UiDialogResult.None;

    public UiDialogWindow(string message, string title, UiDialogKind kind, string primaryText, string? secondaryText)
    {
        Title = title;
        Width = 440;
        SizeToContent = SizeToContent.Height;
        MinHeight = 180;
        ResizeMode = ResizeMode.NoResize;
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        ShowInTaskbar = false;
        FontFamily = new FontFamily("Segoe UI");
        SnapsToDevicePixels = true;

        var (accent, soft, glyph) = ResolveTheme(kind);

        var card = new Border
        {
            Background = ThemeBrushes.Get("Brush.Card", Colors.White),
            CornerRadius = new CornerRadius(16),
            Margin = new Thickness(12),
            Effect = new DropShadowEffect
            {
                BlurRadius = 28,
                ShadowDepth = 4,
                Opacity = 0.22,
                Color = Color.FromRgb(0x0F, 0x17, 0x2A)
            }
        };

        var root = new DockPanel();

        var topStripe = new Border
        {
            Height = 5,
            Background = new SolidColorBrush(accent),
            CornerRadius = new CornerRadius(16, 16, 0, 0)
        };
        DockPanel.SetDock(topStripe, Dock.Top);
        root.Children.Add(topStripe);

        var footer = new Border
        {
            Padding = new Thickness(22, 0, 22, 20)
        };
        DockPanel.SetDock(footer, Dock.Bottom);
        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        if (!string.IsNullOrEmpty(secondaryText))
        {
            buttons.Children.Add(UiDialogChrome.CreateButton(secondaryText!, isPrimary: false, accent, () =>
            {
                Result = UiDialogResult.Secondary;
                DialogResult = false;
            }));
        }

        var primary = UiDialogChrome.CreateButton(primaryText, isPrimary: true, accent, () =>
        {
            Result = UiDialogResult.Primary;
            DialogResult = true;
        });
        if (!string.IsNullOrEmpty(secondaryText))
            primary.Margin = new Thickness(8, 0, 0, 0);
        buttons.Children.Add(primary);
        footer.Child = buttons;
        root.Children.Add(footer);

        var body = new Grid { Margin = new Thickness(22, 20, 22, 16) };
        body.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        body.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        body.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var headerRow = new Grid { Margin = new Thickness(0, 0, 0, 12) };
        headerRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        headerRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        Grid.SetColumnSpan(headerRow, 2);
        Grid.SetRow(headerRow, 0);
        body.Children.Add(headerRow);

        var titleBlock = new TextBlock
        {
            Text = title,
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Foreground = ThemeBrushes.Get("Brush.TextPrimary", Color.FromRgb(0x0F, 0x17, 0x2A)),
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(titleBlock, 0);
        headerRow.Children.Add(titleBlock);
        var closeBtn = UiDialogChrome.CreateCloseButton(() => DialogResult = false);
        Grid.SetColumn(closeBtn, 1);
        headerRow.Children.Add(closeBtn);

        var contentRow = new Grid();
        contentRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        contentRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        Grid.SetRow(contentRow, 1);
        Grid.SetColumnSpan(contentRow, 2);
        body.Children.Add(contentRow);

        var iconCircle = new Border
        {
            Width = 48,
            Height = 48,
            CornerRadius = new CornerRadius(24),
            Background = new SolidColorBrush(soft),
            Margin = new Thickness(0, 0, 14, 0),
            VerticalAlignment = VerticalAlignment.Top,
            Child = new TextBlock
            {
                Text = glyph,
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(accent),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };
        Grid.SetColumn(iconCircle, 0);
        contentRow.Children.Add(iconCircle);

        var textCol = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        textCol.Children.Add(new TextBlock
        {
            Text = message,
            FontSize = 13.5,
            Foreground = ThemeBrushes.Get("Brush.TextSecondary", Color.FromRgb(0x47, 0x55, 0x69)),
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 20
        });
        Grid.SetColumn(textCol, 1);
        contentRow.Children.Add(textCol);
        root.Children.Add(body);

        card.Child = root;
        Content = card;

        PreviewKeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                Result = secondaryText != null ? UiDialogResult.Secondary : UiDialogResult.Primary;
                DialogResult = secondaryText != null ? false : true;
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                Result = UiDialogResult.Primary;
                DialogResult = true;
                e.Handled = true;
            }
        };

        MouseLeftButtonDown += (_, e) =>
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        };
    }

    private static (Color accent, Color soft, string glyph) ResolveTheme(UiDialogKind kind) => kind switch
    {
        UiDialogKind.Success => (Color.FromRgb(0x16, 0xA3, 0x4A), ThemeBrushes.ColorOf("Color.DialogSoftSuccess", Color.FromRgb(0xEC, 0xFD, 0xF5)), "✓"),
        UiDialogKind.Warning => (Color.FromRgb(0xD9, 0x77, 0x06), ThemeBrushes.ColorOf("Color.DialogSoftWarning", Color.FromRgb(0xFF, 0xF7, 0xED)), "!"),
        UiDialogKind.Error => (Color.FromRgb(0xDC, 0x26, 0x26), ThemeBrushes.ColorOf("Color.DialogSoftError", Color.FromRgb(0xFE, 0xF2, 0xF2)), "✕"),
        UiDialogKind.Confirm => (Color.FromRgb(0x25, 0x63, 0xEB), ThemeBrushes.ColorOf("Color.DialogSoftInfo", Color.FromRgb(0xEF, 0xF6, 0xFF)), "?"),
        _ => (Color.FromRgb(0x25, 0x63, 0xEB), ThemeBrushes.ColorOf("Color.DialogSoftInfo", Color.FromRgb(0xEF, 0xF6, 0xFF)), "i")
    };
}
