using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace CeyPASS.WPF;

/// <summary>
/// Form içerikli diyaloglar için UiDialog ile aynı görsel dil (kart, şerit, butonlar).
/// </summary>
public static class UiFormDialog
{
    public static bool Show(
        string title,
        FrameworkElement body,
        Window? owner = null,
        string primaryText = "Tamam",
        string secondaryText = "Vazgeç",
        double width = 420,
        string? subtitle = null,
        Func<bool>? validateOnPrimary = null,
        double? maxBodyHeight = null)
    {
        owner ??= UiDialogChrome.ResolveOwner();
        var dlg = new UiFormDialogWindow(title, subtitle, body, primaryText, secondaryText, width, validateOnPrimary, maxBodyHeight);
        if (owner != null)
        {
            dlg.Owner = owner;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }
        else
        {
            dlg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        return dlg.ShowDialog() == true;
    }

    public static TextBlock CreateLabel(string text) => new()
    {
        Text = text,
        FontSize = 12.5,
        FontWeight = FontWeights.SemiBold,
        Foreground = ThemeBrushes.Get("Brush.TextMuted", Color.FromRgb(0x64, 0x74, 0x8B)),
        Margin = new Thickness(0, 0, 0, 6)
    };

    public static TextBox CreateTextBox(string text = "") => new()
    {
        Text = text,
        FontSize = 14,
        Padding = new Thickness(10, 8, 10, 8),
        BorderBrush = ThemeBrushes.Get("Brush.FieldBorder", Color.FromRgb(0xD0, 0xD7, 0xE2)),
        BorderThickness = new Thickness(1),
        Background = ThemeBrushes.Get("Brush.Card", Colors.White),
        Margin = new Thickness(0, 0, 0, 14)
    };

    public static DatePicker CreateDatePicker(DateTime? selected) => new()
    {
        SelectedDate = selected,
        FontSize = 14,
        Margin = new Thickness(0, 0, 0, 14),
        Padding = new Thickness(8, 6, 8, 6)
    };

    public static ComboBox CreateComboBox(IEnumerable<string> items, string? selected) => new()
    {
        ItemsSource = items.ToList(),
        SelectedItem = selected is not null && items.Contains(selected) ? selected : items.FirstOrDefault(),
        FontSize = 14,
        Padding = new Thickness(8, 6, 8, 6),
        Margin = new Thickness(0, 0, 0, 4),
        BorderBrush = ThemeBrushes.Get("Brush.FieldBorder", Color.FromRgb(0xD0, 0xD7, 0xE2))
    };
}

internal sealed class UiFormDialogWindow : Window
{
    private readonly Func<bool>? _validateOnPrimary;

    public UiFormDialogWindow(
        string title,
        string? subtitle,
        FrameworkElement body,
        string primaryText,
        string secondaryText,
        double width,
        Func<bool>? validateOnPrimary,
        double? maxBodyHeight = null)
    {
        _validateOnPrimary = validateOnPrimary;
        Title = title;
        Width = width;
        SizeToContent = SizeToContent.Height;
        MinHeight = 200;
        ResizeMode = ResizeMode.NoResize;
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        ShowInTaskbar = false;
        FontFamily = new FontFamily("Segoe UI");
        SnapsToDevicePixels = true;

        var accent = Color.FromRgb(0x25, 0x63, 0xEB);
        var soft = ThemeBrushes.ColorOf("Brush.DialogSoft", Color.FromRgb(0xEF, 0xF6, 0xFF));

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

        var footer = new Border { Padding = new Thickness(22, 0, 22, 20) };
        DockPanel.SetDock(footer, Dock.Bottom);
        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        buttons.Children.Add(UiDialogChrome.CreateButton(secondaryText, isPrimary: false, accent, () =>
        {
            DialogResult = false;
        }));

        var primary = UiDialogChrome.CreateButton(primaryText, isPrimary: true, accent, () =>
        {
            if (_validateOnPrimary != null && !_validateOnPrimary())
                return;
            DialogResult = true;
        });
        primary.Margin = new Thickness(8, 0, 0, 0);
        primary.IsDefault = true;
        buttons.Children.Add(primary);
        footer.Child = buttons;
        root.Children.Add(footer);

        var header = new Grid { Margin = new Thickness(22, 18, 22, 8) };
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var titlePanel = new StackPanel();
        titlePanel.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Foreground = ThemeBrushes.Get("Brush.TextPrimary", Color.FromRgb(0x0F, 0x17, 0x2A)),
            TextWrapping = TextWrapping.Wrap
        });
        if (!string.IsNullOrWhiteSpace(subtitle))
        {
            titlePanel.Children.Add(new TextBlock
            {
                Text = subtitle,
                FontSize = 13,
                Foreground = ThemeBrushes.Get("Brush.TextMuted", Color.FromRgb(0x64, 0x74, 0x8B)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 6, 0, 0)
            });
        }
        Grid.SetColumn(titlePanel, 0);
        header.Children.Add(titlePanel);
        var closeBtn = UiDialogChrome.CreateCloseButton(() => DialogResult = false);
        Grid.SetColumn(closeBtn, 1);
        header.Children.Add(closeBtn);
        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        var bodyContent = maxBodyHeight.HasValue
            ? (UIElement)new ScrollViewer
            {
                Content = body,
                MaxHeight = maxBodyHeight.Value,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            }
            : body;

        var bodyHost = new Border
        {
            Child = bodyContent,
            Margin = new Thickness(22, 4, 22, 12),
            Background = new SolidColorBrush(soft),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(14)
        };
        root.Children.Add(bodyHost);

        card.Child = root;
        Content = card;

        PreviewKeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                e.Handled = true;
            }
        };

        MouseLeftButtonDown += (_, e) =>
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        };
    }
}

/// <summary>UiDialog / UiFormDialog ortak görsel yardımcıları.</summary>
internal static class UiDialogChrome
{
    public static Window? ResolveOwner()
    {
        if (Application.Current?.Windows == null)
            return null;

        foreach (Window w in Application.Current.Windows)
        {
            if (w.IsActive && w.IsVisible)
                return w;
        }

        return Application.Current.MainWindow is { IsVisible: true } main
            ? main
            : Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsVisible);
    }

    public static Button CreateCloseButton(Action onClick)
    {
        var icon = new Path
        {
            Data = Geometry.Parse("M 2,2 L 10,10 M 10,2 L 2,10"),
            Stroke = ThemeBrushes.Get("Brush.TextMuted", Color.FromRgb(0x64, 0x74, 0x8B)),
            StrokeThickness = 1.4,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
            Width = 12,
            Height = 12,
            Stretch = Stretch.Uniform,
            Fill = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var slice = new Border
        {
            Background = Brushes.Transparent,
            CornerRadius = new CornerRadius(7),
            Width = 28,
            Height = 28,
            Child = icon
        };

        var btn = new Button
        {
            Content = slice,
            Width = 28,
            Height = 28,
            Cursor = Cursors.Hand,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
            VerticalAlignment = VerticalAlignment.Top,
            Template = new ControlTemplate(typeof(Button))
            {
                VisualTree = CreatePassthroughFactory()
            }
        };

        var hoverBg = new SolidColorBrush(Color.FromRgb(0xFE, 0xE2, 0xE2));
        var hoverStroke = new SolidColorBrush(Color.FromRgb(0xDC, 0x26, 0x26));
        var normalStroke = ThemeBrushes.Get("Brush.TextMuted", Color.FromRgb(0x64, 0x74, 0x8B));
        btn.MouseEnter += (_, _) =>
        {
            slice.Background = hoverBg;
            icon.Stroke = hoverStroke;
        };
        btn.MouseLeave += (_, _) =>
        {
            slice.Background = Brushes.Transparent;
            icon.Stroke = normalStroke;
        };
        btn.Click += (_, _) => onClick();
        return btn;
    }

    public static Button CreateButton(string text, bool isPrimary, Color accent, Action onClick)
    {
        var normalBg = isPrimary ? accent : ThemeBrushes.ColorOf("Brush.SecondaryBg", Color.FromRgb(0xF1, 0xF5, 0xF9));
        var hoverBg = isPrimary ? Darken(accent, 0.12) : ThemeBrushes.ColorOf("Brush.SecondaryHover", Color.FromRgb(0xE2, 0xE8, 0xF0));
        var secondaryFg = ThemeBrushes.Get("Brush.SecondaryFg", Color.FromRgb(0x33, 0x41, 0x55));

        var bd = new Border
        {
            Background = new SolidColorBrush(normalBg),
            CornerRadius = new CornerRadius(9),
            Padding = new Thickness(18, 9, 18, 9),
            Child = new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.SemiBold,
                FontSize = 13,
                Foreground = isPrimary ? Brushes.White : secondaryFg,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };
        if (!isPrimary)
        {
            bd.BorderBrush = ThemeBrushes.Get("Brush.SecondaryBorder", Color.FromRgb(0xE2, 0xE8, 0xF0));
            bd.BorderThickness = new Thickness(1);
        }

        var btn = new Button
        {
            Content = bd,
            Cursor = Cursors.Hand,
            MinWidth = 88,
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent,
            Padding = new Thickness(0),
            Template = new ControlTemplate(typeof(Button))
            {
                VisualTree = CreatePassthroughFactory()
            }
        };

        btn.MouseEnter += (_, _) => bd.Background = new SolidColorBrush(hoverBg);
        btn.MouseLeave += (_, _) => bd.Background = new SolidColorBrush(normalBg);
        btn.Click += (_, _) => onClick();
        return btn;
    }

    private static FrameworkElementFactory CreatePassthroughFactory()
    {
        var factory = new FrameworkElementFactory(typeof(ContentPresenter));
        factory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
        factory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Stretch);
        return factory;
    }

    private static Color Darken(Color c, double amount)
    {
        byte Scale(byte v) => (byte)Math.Max(0, (int)(v * (1 - amount)));
        return Color.FromRgb(Scale(c.R), Scale(c.G), Scale(c.B));
    }
}
