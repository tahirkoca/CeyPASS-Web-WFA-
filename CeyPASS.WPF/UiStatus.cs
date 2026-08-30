using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CeyPASS.WPF;

/// <summary>
/// Ana pencere alt durum çubuğu (toast'a tamamlayıcı).
/// </summary>
public static class UiStatus
{
    private static TextBlock? _message;
    private static TextBlock? _count;
    private static string _lastMessage = "Hazır";

    public static void Register(TextBlock message, TextBlock? count = null)
    {
        _message = message;
        _count = count;
        Set(_lastMessage);
    }

    public static void Unregister()
    {
        _message = null;
        _count = null;
    }

    public static void Set(string message, int? recordCount = null)
    {
        _lastMessage = string.IsNullOrWhiteSpace(message) ? "Hazır" : message.Trim();
        var d = Application.Current?.Dispatcher;
        if (d == null) return;
        if (!d.CheckAccess())
        {
            d.BeginInvoke(() => Set(_lastMessage, recordCount));
            return;
        }

        if (_message != null)
            _message.Text = _lastMessage;

        if (_count != null)
        {
            if (recordCount.HasValue)
            {
                _count.Text = recordCount.Value == 1
                    ? "1 kayıt"
                    : $"{recordCount.Value} kayıt";
                _count.Visibility = Visibility.Visible;
            }
            else
            {
                _count.Text = "";
                _count.Visibility = Visibility.Collapsed;
            }
        }
    }

    public static void SetCount(int recordCount)
        => Set(_lastMessage, recordCount);
}
