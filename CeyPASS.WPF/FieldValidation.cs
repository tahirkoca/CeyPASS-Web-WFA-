using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CeyPASS.WPF;

/// <summary>
/// Alan hata metni — kırmızı çerçeve + tooltip.
/// </summary>
public static class FieldValidation
{
    public static readonly DependencyProperty ErrorProperty =
        DependencyProperty.RegisterAttached(
            "Error",
            typeof(string),
            typeof(FieldValidation),
            new PropertyMetadata(null, OnErrorChanged));

    public static string? GetError(DependencyObject obj) => (string?)obj.GetValue(ErrorProperty);
    public static void SetError(DependencyObject obj, string? value) => obj.SetValue(ErrorProperty, value);

    private static void OnErrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Control c) return;

        var msg = e.NewValue as string;
        if (string.IsNullOrWhiteSpace(msg))
        {
            c.ClearValue(Control.BorderBrushProperty);
            c.ClearValue(Control.BorderThicknessProperty);
            if (c.ToolTip is string)
                c.ToolTip = null;
            return;
        }

        c.BorderBrush = new SolidColorBrush(Color.FromRgb(0xDC, 0x26, 0x26));
        c.BorderThickness = new Thickness(1.5);
        c.ToolTip = msg;
    }

    /// <summary>
    /// Alan doğrulama başarısız: Error banner + toast yok (inline).
    /// </summary>
    public static void ShowFormError(ref string? errorProp, Action<string?> setError, string message)
    {
        setError(message);
        errorProp = message;
    }
}
