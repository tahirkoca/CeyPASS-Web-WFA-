namespace CeyPASS.WFA;

/// <summary>
/// Uzun yüklemelerde host üzerine yarı saydam busy overlay.
/// </summary>
public static class CeypassBusyPanel
{
    private const string TagKey = "CeypassBusyPanel";

    public static void ShowOn(Control host, string title, string? message = null)
    {
        if (host == null || host.IsDisposed) return;

        HideFrom(host);

        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(160, 240, 240, 240),
            Tag = TagKey
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.Transparent
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 40f));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 60f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

        var content = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Anchor = AnchorStyles.None,
            BackColor = Color.Transparent,
            Padding = new Padding(12)
        };

        var lblTitle = new Label
        {
            AutoSize = true,
            Text = title,
            Font = new Font(SystemFonts.MessageBoxFont.FontFamily, 11f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new Padding(0, 0, 0, 6)
        };
        content.Controls.Add(lblTitle);

        if (!string.IsNullOrWhiteSpace(message))
        {
            content.Controls.Add(new Label
            {
                AutoSize = true,
                Text = message,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 0, 10)
            });
        }

        content.Controls.Add(new ProgressBar
        {
            Style = ProgressBarStyle.Marquee,
            MarqueeAnimationSpeed = 30,
            Width = 220,
            Height = 18
        });

        layout.Controls.Add(content, 0, 1);
        panel.Controls.Add(layout);

        host.Controls.Add(panel);
        panel.BringToFront();
        host.Update();
        Application.DoEvents();
    }

    public static void HideFrom(Control host)
    {
        if (host == null || host.IsDisposed) return;

        var existing = host.Controls
            .Cast<Control>()
            .Where(c => Equals(c.Tag, TagKey))
            .ToList();

        foreach (var c in existing)
        {
            host.Controls.Remove(c);
            c.Dispose();
        }
    }

    /// <summary>using ile otomatik gizlenen busy kapsamı.</summary>
    public static IDisposable BusyScope(Control host, string title, string? message = null)
    {
        ShowOn(host, title, message);
        return new Scope(host);
    }

    private sealed class Scope : IDisposable
    {
        private Control? _host;

        public Scope(Control host) => _host = host;

        public void Dispose()
        {
            var h = _host;
            _host = null;
            if (h != null)
                HideFrom(h);
        }
    }
}
