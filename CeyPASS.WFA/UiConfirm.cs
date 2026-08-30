namespace CeyPASS.WFA;

/// <summary>
/// MessageBox Yes/No metinlerini özelleştiren basit onay diyaloğu.
/// </summary>
public static class UiConfirm
{
    public static bool Confirm(
        IWin32Window? owner,
        string message,
        string title = "Onay",
        string yesText = "Evet",
        string noText = "Hayır")
    {
        using var dlg = new ConfirmForm(message, title, yesText, noText);
        return dlg.ShowDialog(owner) == DialogResult.Yes;
    }

    private sealed class ConfirmForm : Form
    {
        public ConfirmForm(string message, string title, string yesText, string noText)
        {
            Text = title;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(420, 160);
            Font = SystemFonts.MessageBoxFont;

            var lbl = new Label
            {
                AutoSize = false,
                Text = message,
                Location = new Point(16, 16),
                Size = new Size(388, 80),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var btnYes = new Button
            {
                Text = yesText,
                DialogResult = DialogResult.Yes,
                Size = new Size(110, 32),
                Location = new Point(186, 110)
            };

            var btnNo = new Button
            {
                Text = noText,
                DialogResult = DialogResult.No,
                Size = new Size(110, 32),
                Location = new Point(302, 110)
            };

            AcceptButton = btnYes;
            CancelButton = btnNo;

            Controls.Add(lbl);
            Controls.Add(btnYes);
            Controls.Add(btnNo);
        }
    }
}
