using System.Windows;
using System.Windows.Controls;

namespace CeyPASS.WPF.Controls;

public partial class CeypassBusyOverlay : UserControl
{
    public static readonly DependencyProperty IsBusyProperty =
        DependencyProperty.Register(nameof(IsBusy), typeof(bool), typeof(CeypassBusyOverlay),
            new PropertyMetadata(false, OnIsBusyChanged));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(CeypassBusyOverlay),
            new PropertyMetadata("Yükleniyor...", OnTitleChanged));

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(nameof(Message), typeof(string), typeof(CeypassBusyOverlay),
            new PropertyMetadata("Lütfen bekleyin", OnMessageChanged));

    public CeypassBusyOverlay()
    {
        InitializeComponent();
    }

    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    private static void OnIsBusyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CeypassBusyOverlay o)
            o.Visibility = (bool)e.NewValue! ? Visibility.Visible : Visibility.Collapsed;
    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CeypassBusyOverlay o && e.NewValue is string s)
            o.TxtTitle.Text = s;
    }

    private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CeypassBusyOverlay o && e.NewValue is string s)
            o.TxtMessage.Text = s;
    }
}
