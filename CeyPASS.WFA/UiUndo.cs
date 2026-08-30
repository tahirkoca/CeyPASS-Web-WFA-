namespace CeyPASS.WFA;

/// <summary>
/// Durum çubuğunda kısa süreli Geri al bağlantısı.
/// </summary>
public static class UiUndo
{
    private static ToolStripStatusLabel? _link;
    private static System.Windows.Forms.Timer? _timer;
    private static Action? _pending;

    public static void Register(ToolStripStatusLabel undoLink)
    {
        _link = undoLink;
        undoLink.IsLink = true;
        undoLink.LinkBehavior = LinkBehavior.HoverUnderline;
        undoLink.Visible = false;
        undoLink.Click += (_, _) => Execute();
    }

    public static void Offer(string message, Action undo)
    {
        Cancel();
        _pending = undo;
        UiStatus.Set(message);

        if (_link != null)
        {
            _link.Text = "Geri al";
            _link.Visible = true;
        }

        _timer = new System.Windows.Forms.Timer { Interval = 7000 };
        _timer.Tick += (_, _) => Cancel();
        _timer.Start();
    }

    private static void Execute()
    {
        var action = _pending;
        Cancel();
        action?.Invoke();
    }

    private static void Cancel()
    {
        _timer?.Stop();
        _timer = null;
        _pending = null;
        if (_link != null)
            _link.Visible = false;
    }
}
