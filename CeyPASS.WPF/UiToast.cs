using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace CeyPASS.WPF;

public enum UiToastKind
{
    Success,
    Info
}

/// <summary>
/// Kısa, tıklama gerektirmeyen geri bildirim (kaydet başarı vb.).
/// </summary>
public static class UiToast
{
    private static readonly object Sync = new();
    private static Panel? _host;
    private static DispatcherTimer? _hideTimer;
    private static DispatcherTimer? _undoTimer;
    private static Border? _current;
    private static Action? _pendingUndo;

    public static void RegisterHost(Panel host)
    {
        lock (Sync)
            _host = host;
    }

    public static void UnregisterHost(Panel host)
    {
        lock (Sync)
        {
            if (ReferenceEquals(_host, host))
                _host = null;
        }
    }

    public static void Success(string message, string? title = null)
    {
        var t = string.IsNullOrWhiteSpace(title) ? "Başarılı" : UiPageTitles.Friendly(title);
        Show(message, t, UiToastKind.Success);
        UiStatus.Set(string.IsNullOrWhiteSpace(message) ? t : message);
    }

    /// <summary>Başarı toast + ~7 sn içinde Geri al.</summary>
    public static void SuccessWithUndo(string message, Action undo, string? title = null, string undoLabel = "Geri al")
    {
        var t = string.IsNullOrWhiteSpace(title) ? "Başarılı" : UiPageTitles.Friendly(title);
        ShowWithUndo(message, t, undo, undoLabel);
        UiStatus.Set(string.IsNullOrWhiteSpace(message) ? t : message);
    }

    public static void Info(string message, string? title = null)
    {
        var t = string.IsNullOrWhiteSpace(title) ? "Bilgi" : UiPageTitles.Friendly(title);
        Show(message, t, UiToastKind.Info);
        UiStatus.Set(string.IsNullOrWhiteSpace(message) ? t : message);
    }

    public static void Show(string message, string title, UiToastKind kind)
    {
        title = string.IsNullOrWhiteSpace(title) ? "Bilgi" : UiPageTitles.Friendly(title);
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null)
            return;

        if (!dispatcher.CheckAccess())
        {
            dispatcher.Invoke(() => Show(message, title, kind));
            return;
        }

        Panel? host;
        lock (Sync)
            host = _host;

        if (host == null)
        {
            // Login vb. — kısa süreli modeless pencere
            ShowFloating(message, title, kind);
            return;
        }

        PresentOnHost(host, message, title, kind, null, null);
    }

    private static void ShowWithUndo(string message, string title, Action undo, string undoLabel)
    {
        title = string.IsNullOrWhiteSpace(title) ? "Başarılı" : UiPageTitles.Friendly(title);
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null)
            return;

        if (!dispatcher.CheckAccess())
        {
            dispatcher.Invoke(() => ShowWithUndo(message, title, undo, undoLabel));
            return;
        }

        Panel? host;
        lock (Sync)
            host = _host;

        if (host == null)
        {
            ShowFloating(message, title, UiToastKind.Success, undo, undoLabel);
            return;
        }

        PresentOnHost(host, message, title, UiToastKind.Success, undo, undoLabel);
    }

    private static void CancelPendingUndo()
    {
        _undoTimer?.Stop();
        _undoTimer = null;
        _pendingUndo = null;
    }

    private static void PresentOnHost(Panel host, string message, string title, UiToastKind kind, Action? undo, string? undoLabel)
    {
        _hideTimer?.Stop();
        CancelPendingUndo();
        if (_current != null)
            host.Children.Remove(_current);

        var toast = BuildCard(message, title, kind, undo, undoLabel);
        _current = toast;
        host.Children.Add(toast);

        toast.Opacity = 0;
        toast.RenderTransform = new TranslateTransform(0, -16);
        var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180));
        var slide = new DoubleAnimation(-16, 0, TimeSpan.FromMilliseconds(180));
        toast.BeginAnimation(UIElement.OpacityProperty, fade);
        ((TranslateTransform)toast.RenderTransform).BeginAnimation(TranslateTransform.YProperty, slide);

        var duration = undo != null ? 7.0 : 2.8;
        if (undo != null)
        {
            _pendingUndo = undo;
            _undoTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(duration) };
            _undoTimer.Tick += (_, _) =>
            {
                _undoTimer.Stop();
                CancelPendingUndo();
                Dismiss(host, toast);
            };
            _undoTimer.Start();
        }

        _hideTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(duration) };
        _hideTimer.Tick += (_, _) =>
        {
            _hideTimer.Stop();
            CancelPendingUndo();
            Dismiss(host, toast);
        };
        _hideTimer.Start();
    }

    private static void Dismiss(Panel host, Border toast)
    {
        var fade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(160));
        fade.Completed += (_, _) =>
        {
            host.Children.Remove(toast);
            if (ReferenceEquals(_current, toast))
                _current = null;
        };
        toast.BeginAnimation(UIElement.OpacityProperty, fade);
    }

    private static Border BuildCard(string message, string title, UiToastKind kind, Action? undo = null, string? undoLabel = null)
    {
        var accent = kind == UiToastKind.Success
            ? Color.FromRgb(0x16, 0xA3, 0x4A)
            : Color.FromRgb(0x25, 0x63, 0xEB);
        var soft = kind == UiToastKind.Success
            ? ThemeBrushes.ColorOf("Color.DialogSoftSuccess", Color.FromRgb(0xEC, 0xFD, 0xF5))
            : ThemeBrushes.ColorOf("Color.DialogSoftInfo", Color.FromRgb(0xEF, 0xF6, 0xFF));
        var glyph = kind == UiToastKind.Success ? "✓" : "i";

        var card = new Border
        {
            Width = 340,
            MaxWidth = 400,
            Margin = new Thickness(0, 16, 20, 0),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Background = ThemeBrushes.Get("Brush.Card", Colors.White),
            CornerRadius = new CornerRadius(12),
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x40, accent.R, accent.G, accent.B)),
            BorderThickness = new Thickness(1),
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 20,
                ShadowDepth = 3,
                Opacity = 0.22,
                Color = Color.FromRgb(0x0F, 0x17, 0x2A)
            },
            Padding = new Thickness(14, 12, 14, 12),
            Cursor = System.Windows.Input.Cursors.Hand
        };

        var root = new DockPanel();
        var stripe = new Border
        {
            Width = 4,
            Background = new SolidColorBrush(accent),
            CornerRadius = new CornerRadius(2),
            Margin = new Thickness(0, 0, 12, 0)
        };
        DockPanel.SetDock(stripe, Dock.Left);
        root.Children.Add(stripe);

        var icon = new Border
        {
            Width = 32,
            Height = 32,
            CornerRadius = new CornerRadius(16),
            Background = new SolidColorBrush(soft),
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Top,
            Child = new TextBlock
            {
                Text = glyph,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(accent),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };
        DockPanel.SetDock(icon, Dock.Left);
        root.Children.Add(icon);

        var text = new StackPanel();
        text.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            Foreground = ThemeBrushes.Get("Brush.TextPrimary", Color.FromRgb(0x0F, 0x17, 0x2A))
        });
        text.Children.Add(new TextBlock
        {
            Text = message,
            FontSize = 12.5,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 2, 0, 0),
            Foreground = ThemeBrushes.Get("Brush.TextSecondary", Color.FromRgb(0x47, 0x55, 0x69))
        });

        if (undo != null)
        {
            var undoBtn = new Button
            {
                Content = string.IsNullOrWhiteSpace(undoLabel) ? "Geri al" : undoLabel,
                Margin = new Thickness(0, 10, 0, 0),
                Padding = new Thickness(12, 4, 12, 4),
                HorizontalAlignment = HorizontalAlignment.Left,
                Cursor = System.Windows.Input.Cursors.Hand,
                Background = new SolidColorBrush(soft),
                Foreground = new SolidColorBrush(accent),
                BorderBrush = new SolidColorBrush(accent),
                BorderThickness = new Thickness(1)
            };
            undoBtn.Click += (_, e) =>
            {
                e.Handled = true;
                Panel? host;
                lock (Sync)
                    host = _host;
                var action = _pendingUndo;
                CancelPendingUndo();
                if (host != null)
                {
                    _hideTimer?.Stop();
                    Dismiss(host, card);
                }
                action?.Invoke();
            };
            text.Children.Add(undoBtn);
        }

        root.Children.Add(text);
        card.Child = root;

        card.MouseLeftButtonUp += (_, e) =>
        {
            if (e.OriginalSource is Button)
                return;
            Panel? host;
            lock (Sync)
                host = _host;
            if (host != null)
            {
                _hideTimer?.Stop();
                Dismiss(host, card);
            }
        };

        return card;
    }

    private static void ShowFloating(string message, string title, UiToastKind kind, Action? undo = null, string? undoLabel = null)
    {
        var owner = UiDialogChrome.ResolveOwner();
        var win = new Window
        {
            WindowStyle = WindowStyle.None,
            AllowsTransparency = true,
            Background = Brushes.Transparent,
            ShowInTaskbar = false,
            SizeToContent = SizeToContent.WidthAndHeight,
            Topmost = true,
            ShowActivated = false,
            ResizeMode = ResizeMode.NoResize
        };
        if (owner != null)
            win.Owner = owner;

        var card = BuildCard(message, title, kind, undo, undoLabel);
        card.Margin = new Thickness(0);
        win.Content = card;

        if (owner != null)
        {
            win.WindowStartupLocation = WindowStartupLocation.Manual;
            win.Left = owner.Left + owner.ActualWidth - 380;
            win.Top = owner.Top + 56;
        }
        else
        {
            win.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        win.Show();
        var duration = undo != null ? 7.0 : 2.8;
        if (undo != null)
            _pendingUndo = undo;

        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(duration) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            CancelPendingUndo();
            try { win.Close(); } catch { /* ignore */ }
        };
        timer.Start();
        card.MouseLeftButtonUp += (_, e) =>
        {
            if (e.OriginalSource is Button)
                return;
            timer.Stop();
            CancelPendingUndo();
            try { win.Close(); } catch { /* ignore */ }
        };
    }
}
