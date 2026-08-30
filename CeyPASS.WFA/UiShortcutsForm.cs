namespace CeyPASS.WFA;

/// <summary>
/// F1 / Ctrl+/ kısayol listesi.
/// </summary>
internal sealed class UiShortcutsForm : Form
{
    public UiShortcutsForm(string? pageKey)
    {
        Text = "Klavye kısayolları";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(440, 360);
        KeyPreview = true;
        Font = SystemFonts.MessageBoxFont;

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 44,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(12, 10, 8, 0)
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var title = new Label
        {
            Text = "Klavye kısayolları",
            AutoSize = true,
            Font = new Font(Font.FontFamily, 11F, FontStyle.Bold),
            Anchor = AnchorStyles.Left | AnchorStyles.Top
        };

        var btnClose = new Button
        {
            Text = "✕",
            AutoSize = true,
            FlatStyle = FlatStyle.Flat,
            DialogResult = DialogResult.OK,
            Padding = new Padding(6, 2, 6, 2),
            TabStop = false
        };
        btnClose.FlatAppearance.BorderSize = 0;

        header.Controls.Add(title, 0, 0);
        header.Controls.Add(btnClose, 1, 0);

        var list = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            HeaderStyle = ColumnHeaderStyle.Nonclickable,
            ShowItemToolTips = true,
            BorderStyle = BorderStyle.FixedSingle
        };
        list.Columns.Add("Kısayol", 140);
        list.Columns.Add("Açıklama", 270);

        foreach (var item in ShortcutCatalog.ForPage(pageKey))
        {
            var li = new ListViewItem(item.Keys);
            li.SubItems.Add(item.Description);
            list.Items.Add(li);
        }

        AcceptButton = btnClose;
        CancelButton = btnClose;

        Controls.Add(list);
        Controls.Add(header);

        KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        };
    }

    public static void ShowFor(IWin32Window? owner, string? pageKey)
    {
        using var dlg = new UiShortcutsForm(pageKey);
        dlg.ShowDialog(owner);
    }
}
