using System.Drawing;
using System.Windows.Forms;

namespace CeyPASS.WFA;

/// <summary>
/// 2026 standartlarına uygun modern tema renkleri ve fontları.
/// Form ve UserControl'larda tutarlı görünüm için kullanılır.
/// </summary>
public static class AppTheme
{
    // Ana renkler (modern, erişilebilir)
    public static Color Primary => Color.FromArgb(0, 120, 212);      // Microsoft Fluent mavi
    public static Color PrimaryHover => Color.FromArgb(0, 99, 197);
    public static Color PrimaryPressed => Color.FromArgb(0, 78, 162);

    // Giriş ekranı (Cey marka kırmızısı)
    public static Color LoginButton => Color.FromArgb(200, 50, 50);
    public static Color LoginButtonHover => Color.FromArgb(180, 40, 40);

    // Arka planlar
    public static Color SidebarBackground => Color.FromArgb(33, 37, 41);
    public static Color SidebarLogoBackground => Color.FromArgb(26, 26, 26);
    public static Color ContentBackground => Color.FromArgb(243, 243, 243);
    public static Color CardBackground => Color.FromArgb(255, 255, 255);
    public static Color CardBackgroundSoft => Color.FromArgb(250, 250, 250);

    // Metin
    public static Color TextPrimary => Color.FromArgb(28, 28, 28);
    public static Color TextSecondary => Color.FromArgb(108, 117, 125);
    public static Color MenuText => Color.FromArgb(211, 211, 211);
    public static Color MenuTextMuted => Color.FromArgb(128, 128, 128);

    // Kenarlık ve ayırıcı
    public static Color Border => Color.FromArgb(222, 226, 230);
    public static Color BorderFocus => Primary;

    // Link / aksiyon
    public static Color Link => Color.FromArgb(0, 102, 179);
    public static Color LinkHover => Primary;

    // Font ailesi (Windows 11 / modern) — Form/Designer'da new Font(AppTheme.FontFamily, 10F) kullanın
    public const string FontFamily = "Segoe UI";

    /// <summary>
    /// UserControl veya Panel için arka plan rengini tema ile uyumlu yapar.
    /// Load veya constructor sonunda çağrılabilir.
    /// </summary>
    public static void ApplyToControl(Control control)
    {
        if (control == null) return;
        control.BackColor = ContentBackground;
    }
}
