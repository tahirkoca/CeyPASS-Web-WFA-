using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CeyPASS.WPF.Controls;

public partial class CeypassHelpTip : UserControl
{
    public static readonly DependencyProperty TopicProperty =
        DependencyProperty.Register(nameof(Topic), typeof(string), typeof(CeypassHelpTip),
            new PropertyMetadata(null, OnTopicChanged));

    private readonly List<UIElement> _pulsing = new();
    private Window? _hostWindow;

    public CeypassHelpTip()
    {
        InitializeComponent();
        HelpPopup.Opened += HelpPopup_OnOpened;
    }

    public string? Topic
    {
        get => (string?)GetValue(TopicProperty);
        set => SetValue(TopicProperty, value);
    }

    private static void OnTopicChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CeypassHelpTip tip)
            tip.ApplyTopic();
    }

    private void ApplyTopic()
    {
        var topic = PageHelpCatalog.Get(Topic);
        if (topic is null)
        {
            TxtTitle.Text = "İşlem rehberi";
            StepsList.ItemsSource = null;
            return;
        }

        TxtTitle.Text = topic.Title;
        StepsList.ItemsSource = topic.Steps;
    }

    private void BtnHelp_OnClick(object sender, RoutedEventArgs e)
    {
        ApplyTopic();
        HelpPopup.IsOpen = !HelpPopup.IsOpen;
        if (HelpPopup.IsOpen)
            StartPulse();
        else
            CloseHelpPopup();
    }

    private void HelpPopup_OnOpened(object? sender, EventArgs e)
    {
        AttachEscapeHandler();
        if (HelpPopup.Child is UIElement root)
        {
            root.Focusable = true;
            Keyboard.Focus(root);
        }
    }

    private void HelpPopup_OnClosed(object? sender, EventArgs e)
    {
        DetachEscapeHandler();
        StopPulse();
    }

    private void BtnClose_OnClick(object sender, RoutedEventArgs e) => CloseHelpPopup();

    private void CloseHelpPopup()
    {
        HelpPopup.IsOpen = false;
        StopPulse();
    }

    private void AttachEscapeHandler()
    {
        DetachEscapeHandler();
        _hostWindow = Window.GetWindow(this);
        if (_hostWindow is null) return;
        _hostWindow.PreviewKeyDown += HostWindow_OnPreviewKeyDown;
    }

    private void DetachEscapeHandler()
    {
        if (_hostWindow is null) return;
        _hostWindow.PreviewKeyDown -= HostWindow_OnPreviewKeyDown;
        _hostWindow = null;
    }

    private void HostWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (!HelpPopup.IsOpen || e.Key != Key.Escape) return;
        CloseHelpPopup();
        e.Handled = true;
    }

    private void PopupRoot_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape) return;
        CloseHelpPopup();
        e.Handled = true;
    }

    private void StartPulse()
    {
        StopPulse();
        var topic = PageHelpCatalog.Get(Topic);
        if (topic is null || topic.PulseTargetNames.Count == 0)
            return;

        var root = FindHostUserControl();
        if (root is null) return;

        foreach (var name in topic.PulseTargetNames)
        {
            if (root.FindName(name) is UIElement target)
                _pulsing.Add(target);
        }

        foreach (var el in _pulsing)
        {
            var anim = new DoubleAnimation
            {
                From = 1.0,
                To = 0.45,
                Duration = TimeSpan.FromMilliseconds(550),
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(4)
            };
            el.BeginAnimation(UIElement.OpacityProperty, anim);
        }
    }

    private void StopPulse()
    {
        foreach (var el in _pulsing)
            el.BeginAnimation(UIElement.OpacityProperty, null);
        _pulsing.Clear();
    }

    private UserControl? FindHostUserControl()
    {
        var cur = VisualTreeHelper.GetParent(this);
        while (cur != null)
        {
            if (cur is UserControl uc)
                return uc;
            cur = VisualTreeHelper.GetParent(cur);
        }
        return null;
    }
}
