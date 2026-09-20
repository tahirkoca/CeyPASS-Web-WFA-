namespace CeyPASS.WFA;

/// <summary>
/// Ana pencere alt durum çubuğu.
/// </summary>
public static class UiStatus
{
    private static ToolStripStatusLabel? _message;
    private static ToolStripStatusLabel? _count;
    private static string _lastMessage = "Hazır";

    /// <summary>Ana pencere durum etiketlerini kaydeder.</summary>
    public static void Register(ToolStripStatusLabel message, ToolStripStatusLabel? count = null)
    {
        _message = message;
        _count = count;
        Set(_lastMessage);
    }

    /// <summary>Durum çubuğu referanslarını temizler.</summary>
    public static void Unregister()
    {
        _message = null;
        _count = null;
    }

    /// <summary>Mesaj ve isteğe bağlı kayıt sayısını günceller (UI thread güvenli).</summary>
    public static void Set(string message, int? recordCount = null)
    {
        _lastMessage = string.IsNullOrWhiteSpace(message) ? "Hazır" : message.Trim();

        void Apply()
        {
            if (_message != null)
                _message.Text = _lastMessage;

            if (_count == null) return;

            if (recordCount.HasValue)
            {
                _count.Text = recordCount.Value == 1
                    ? "1 kayıt"
                    : $"{recordCount.Value} kayıt";
                _count.Visible = true;
            }
            else
            {
                _count.Text = "";
                _count.Visible = false;
            }
        }

        var owner = _message?.GetCurrentParent()?.FindForm();
        if (owner != null && owner.InvokeRequired)
        {
            owner.BeginInvoke(Apply);
            return;
        }

        Apply();
    }

    /// <summary>Sadece kayıt sayısını günceller.</summary>
    public static void SetCount(int recordCount)
        => Set(_lastMessage, recordCount);
}
