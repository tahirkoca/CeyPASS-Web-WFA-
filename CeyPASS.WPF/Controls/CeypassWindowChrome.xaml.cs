using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CeyPASS.WPF.Views;

namespace CeyPASS.WPF.Controls;

public enum CeypassChromeVariant
{
    Light,
    Glass
}

public partial class CeypassWindowChrome : UserControl
{
    public static readonly DependencyProperty ShowMinimizeProperty =
        DependencyProperty.Register(nameof(ShowMinimize), typeof(bool), typeof(CeypassWindowChrome),
            new PropertyMetadata(true, OnChromeOptionChanged));

    public static readonly DependencyProperty ShowMaximizeProperty =
        DependencyProperty.Register(nameof(ShowMaximize), typeof(bool), typeof(CeypassWindowChrome),
            new PropertyMetadata(true, OnChromeOptionChanged));

    public static readonly DependencyProperty ShowCloseProperty =
        DependencyProperty.Register(nameof(ShowClose), typeof(bool), typeof(CeypassWindowChrome),
            new PropertyMetadata(true, OnChromeOptionChanged));

    public static readonly DependencyProperty VariantProperty =
        DependencyProperty.Register(nameof(Variant), typeof(CeypassChromeVariant), typeof(CeypassWindowChrome),
            new PropertyMetadata(CeypassChromeVariant.Light));

    public static readonly DependencyProperty TargetWindowProperty =
        DependencyProperty.Register(nameof(TargetWindow), typeof(Window), typeof(CeypassWindowChrome),
            new PropertyMetadata(null, OnTargetWindowChanged));

    private Window? _window;

    public bool ShowMinimize
    {
        get => (bool)GetValue(ShowMinimizeProperty);
        set => SetValue(ShowMinimizeProperty, value);
    }

    public bool ShowMaximize
    {
        get => (bool)GetValue(ShowMaximizeProperty);
        set => SetValue(ShowMaximizeProperty, value);
    }

    public bool ShowClose
    {
        get => (bool)GetValue(ShowCloseProperty);
        set => SetValue(ShowCloseProperty, value);
    }

    public CeypassChromeVariant Variant
    {
        get => (CeypassChromeVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public Window? TargetWindow
    {
        get => (Window?)GetValue(TargetWindowProperty);
        set => SetValue(TargetWindowProperty, value);
    }

    public CeypassWindowChrome()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        ApplyVisibility();
    }

    private static void OnChromeOptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CeypassWindowChrome chrome)
            chrome.ApplyVisibility();
    }

    private static void OnTargetWindowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CeypassWindowChrome chrome)
            chrome.AttachWindow(e.OldValue as Window, e.NewValue as Window);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (TargetWindow == null)
            TargetWindow = FindParentWindow(this);

        ApplyVisibility();
        UpdateMaximizeIcon();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        AttachWindow(_window, null);
    }

    private void AttachWindow(Window? oldWindow, Window? newWindow)
    {
        if (oldWindow != null)
            oldWindow.StateChanged -= Window_OnStateChanged;

        _window = newWindow;

        if (_window != null)
            _window.StateChanged += Window_OnStateChanged;
    }

    private void Window_OnStateChanged(object? sender, EventArgs e) => UpdateMaximizeIcon();

    private void ApplyVisibility()
    {
        BtnMinimize.Visibility = ShowMinimize ? Visibility.Visible : Visibility.Collapsed;
        BtnMaximize.Visibility = ShowMaximize ? Visibility.Visible : Visibility.Collapsed;
        BtnClose.Visibility = ShowClose ? Visibility.Visible : Visibility.Collapsed;

        var min = ShowMinimize;
        var max = ShowMaximize;
        var close = ShowClose;
        SepAfterMin.Visibility = min && (max || close) ? Visibility.Visible : Visibility.Collapsed;
        SepAfterMax.Visibility = max && close ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateMaximizeIcon()
    {
        var maximized = (_window ?? ResolveWindow())?.WindowState == WindowState.Maximized;
        IcoMax.Visibility = maximized ? Visibility.Collapsed : Visibility.Visible;
        IcoRestore.Visibility = maximized ? Visibility.Visible : Visibility.Collapsed;
        BtnMaximize.ToolTip = maximized ? "Geri Yükle" : "Büyüt";
    }

    private Window? ResolveWindow() => TargetWindow ?? _window ?? FindParentWindow(this);

    private static Window? FindParentWindow(DependencyObject child)
    {
        var current = child;
        while (current != null)
        {
            if (current is Window w)
                return w;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    private void BtnMinimize_OnClick(object sender, RoutedEventArgs e)
    {
        var w = ResolveWindow();
        if (w == null) return;
        w.WindowState = WindowState.Minimized;
    }

    private void BtnMaximize_OnClick(object sender, RoutedEventArgs e)
    {
        var w = ResolveWindow();
        if (w == null) return;
        w.WindowState = w.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
        UpdateMaximizeIcon();
    }

    private void BtnClose_OnClick(object sender, RoutedEventArgs e)
    {
        var w = ResolveWindow();
        if (w == null) return;

        if (w is MainWindow)
        {
            System.Windows.Application.Current.Shutdown();
            return;
        }

        w.Close();
    }
}
