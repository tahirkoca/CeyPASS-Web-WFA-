using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.WFA.UserControls;
using CeyPASS.WFA.UserControls.Ayarlar;
using CeyPASS.WFA.UserControls.Dashboard;
using CeyPASS.WFA.UserControls.EO;
using CeyPASS.WFA.UserControls.Izinler;
using CeyPASS.WFA.UserControls.Raporlar;
using CeyPASS.WFA.UserControls.VMY;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CeyPASS.WFA.Forms
{
    public partial class islemEkrani : Form
    {
        [DllImport("user32.dll", SetLastError = false)]
        private static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);
        private const int SB_BOTH = 3;
        private const string PageNameUI = "Ana Sayfa";
        private const int SidebarWidthExpanded = 294;
        private const int SidebarWidthCollapsed = 72;
        private const string SidebarCollapsedStorageKey = "ceypass-sidebar-collapsed";
        private readonly ISessionContext _session;
        private readonly IServiceProvider _sp;
        private readonly IAuthorizationService _auth;
        private bool _sidebarCollapsed;

        public islemEkrani(ISessionContext session, IServiceProvider sp, IAuthorizationService auth)
        {
            InitializeComponent();
            _session = session;
            _sp = sp;
            _auth = auth;
            lblSayfaBasligi.Text = PageNameUI;
        }
        private TUserControl AutoOpenTUserControl<TUserControl>(Panel host) where TUserControl : UserControl
        {
            host.Controls.Clear();
            var uc = _sp.GetRequiredService<TUserControl>();
            uc.Dock = DockStyle.Fill;
            host.Controls.Add(uc);

            try
            {
                var fieldInfo = uc.GetType().GetField("PageNameUI",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Static);

                lblSayfaBasligi.Text = fieldInfo != null
                    ? fieldInfo.GetValue(null)?.ToString()
                    : "CeyPASS";
            }
            catch
            {
                lblSayfaBasligi.Text = "İşlem Ekranı";
            }

            return uc;
        }
        private void OpenTUserControl<TUserControl>(Panel host) where TUserControl : UserControl
        {
            host.Controls.Clear();
            var uc = _sp.GetRequiredService<TUserControl>();
            uc.Dock = DockStyle.Fill;
            host.Controls.Add(uc);
            try
            {
                var fieldInfo = uc.GetType().GetField("PageNameUI",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Static);

                if (fieldInfo != null)
                {
                    string sayfaAdi = fieldInfo.GetValue(null)?.ToString();
                    lblSayfaBasligi.Text = sayfaAdi;
                }
                else
                {
                    lblSayfaBasligi.Text = "CeyPASS";
                }
            }
            catch
            {
                lblSayfaBasligi.Text = "İşlem Ekranı";
            }
        }
        private void Dashboard_ReportRequested(object sender, ReportRequest req)
        {
            var raporlar = AutoOpenTUserControl<ucRaporlar>(islemEkraniPanel);
            raporlar.OpenFromDashboard(req);
        }
        private void btnPersonelTanimlama_Click(object sender, EventArgs e)
        {
            OpenTUserControl<ucPersonelTanimlama>(islemEkraniPanel);
        }
        private void btnDepartmanTanimlama_Click(object sender, EventArgs e)
        {
            OpenTUserControl<ucDepartmanTanimlama>(islemEkraniPanel);
        }
        private void btnPozisyonTanimlama_Click(object sender, EventArgs e)
        {
            OpenTUserControl<ucPozisyonTanimla>(islemEkraniPanel);
        }
        private void btnFirmaTanimlama_Click(object sender, EventArgs e)
        {
            OpenTUserControl<ucFirmaTanimlama>(islemEkraniPanel);
        }
        private void btnIsyeriTanimlama_Click(object sender, EventArgs e)
        {
            OpenTUserControl<ucIsyeriTanimlama>(islemEkraniPanel);
        }
        private void btnRaporlar_Click(object sender, EventArgs e)
        {
            AutoOpenTUserControl<ucRaporlar>(islemEkraniPanel);
        }
        private void islemEkrani_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Kapatınca girisEkrani FormClosed ile tekrar gösterilir; Application.Exit() çağırma.
        }
        private void btnIzinler_Click(object sender, EventArgs e)
        {
            OpenTUserControl<ucIzinler>(islemEkraniPanel);
        }
        private void btnAnasayfa_Click(object sender, EventArgs e)
        {
            var dash = AutoOpenTUserControl<ucDashboard>(islemEkraniPanel);
            dash.ReportRequested -= Dashboard_ReportRequested;
            dash.ReportRequested += Dashboard_ReportRequested;
        }

        private void islemEkrani_Load(object sender, EventArgs e)
        {
            ApplyTheme();
            RefreshSidebarUserInfo();
            ApplyMenuVisibilityByPermission();
            LoadSidebarCollapsedState();
            ApplySidebarState();
            HideScrollBars();
            this.Resize += (s, e) => HideScrollBars();
            pnlSidebarToggleEdge.Layout += (s, e) => CenterToggleButtonsInEdge();
            var dash = AutoOpenTUserControl<ucDashboard>(islemEkraniPanel);
            dash.ReportRequested -= Dashboard_ReportRequested;
            dash.ReportRequested += Dashboard_ReportRequested;
        }

        /// <summary>
        /// Web sidebar-footer gibi: kullanıcı adı, rol ve avatar (baş harf) + çıkış butonu.
        /// </summary>
        private void RefreshSidebarUserInfo()
        {
            var displayName = _session.AdSoyad ?? _session.CurrentUser?.AdSoyad ?? _session.CurrentUser?.KullaniciAdi ?? "";
            var role = _session.RolAdi ?? _session.CurrentUser?.Rol ?? "";
            lblSidebarUserName.Text = !string.IsNullOrEmpty(displayName) ? displayName : "Kullanıcı";
            lblSidebarUserRole.Text = !string.IsNullOrEmpty(role) ? role : "Kullanıcı";
            var initial = !string.IsNullOrEmpty(displayName) ? displayName[0].ToString().ToUpperInvariant() : "K";
            lblSidebarUserAvatar.Text = initial;
        }

        /// <summary>
        /// Web tarafındaki gibi yetkisi olmayan modülleri sol menüde gizler (ViewAbility).
        /// </summary>
        private void ApplyMenuVisibilityByPermission()
        {
            btnAnasayfa.Visible = _auth.ViewAbility("Dashboard");
            btnPersonelTanimlama.Visible = _auth.ViewAbility("Personeller");
            btnDepartmanTanimlama.Visible = _auth.ViewAbility("Departmanlar");
            btnPozisyonTanimlama.Visible = _auth.ViewAbility("Pozisyonlar");
            btnFirmaTanimlama.Visible = _auth.ViewAbility("Firmalar");
            btnIsyeriTanimlama.Visible = _auth.ViewAbility("Isyerler");
            btnIzinler.Visible = _auth.ViewAbility("Izinler");
            btnKisiHareketlerEkrani.Visible = _auth.ViewAbility("KisiHareketler");
            btnAylikPuantajEkrani.Visible = _auth.ViewAbility("AylikPuantaj");
            btnRaporlar.Visible = _auth.ViewAbility("Raporlar");
            bool adminVisible = _session.RolId == 1;
            lblAdminBaslik.Visible = adminVisible;
            btnAdminPanel.Visible = adminVisible;
            calismaStatuleriMenu.Visible = _auth.ViewAbility("CalismaStatuleri");
            calismaSekilleriMenu.Visible = _auth.ViewAbility("Vardiyalar");
            btnCihazlarEkrani.Visible = _auth.ViewAbility("Cihazlar");
            btnResmiTatiller.Visible = _auth.ViewAbility("ResmiTatiller");

            lblPOYBaslik.Visible = btnPersonelTanimlama.Visible || btnDepartmanTanimlama.Visible || btnPozisyonTanimlama.Visible || btnFirmaTanimlama.Visible || btnIsyeriTanimlama.Visible || btnIzinler.Visible;
            lblEOBaslik.Visible = btnKisiHareketlerEkrani.Visible || btnAylikPuantajEkrani.Visible || btnRaporlar.Visible;
            lblVMYBaslik.Visible = calismaStatuleriMenu.Visible || calismaSekilleriMenu.Visible;
            lblAyarlarBaslik.Visible = btnCihazlarEkrani.Visible || btnResmiTatiller.Visible;
        }

        private void LoadSidebarCollapsedState()
        {
            try
            {
                var path = Path.Combine(Application.UserAppDataPath, SidebarCollapsedStorageKey + ".txt");
                if (File.Exists(path))
                {
                    var v = File.ReadAllText(path).Trim();
                    _sidebarCollapsed = (v == "1");
                }
            }
            catch { _sidebarCollapsed = false; }
        }

        private void SaveSidebarCollapsedState()
        {
            try
            {
                var dir = Application.UserAppDataPath;
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                var v = _sidebarCollapsed ? "1" : "0";
                File.WriteAllText(Path.Combine(dir, SidebarCollapsedStorageKey + ".txt"), v);
            }
            catch { }
        }

        private void ApplySidebarState()
        {
            bool collapsed = _sidebarCollapsed;
            this.SuspendLayout();
            try
            {
                int w = collapsed ? SidebarWidthCollapsed : SidebarWidthExpanded;
                pnlSidebarContainer.Width = w;
                pnlSidebarContainer.Visible = true;

                // Toggle paneli: web gibi sidebar'ın sağ kenarında, dikey ortada (reveal şeridiyle aynı hiza)
                const int edgeWide = 28;
                const int edgeNarrow = 36;
                int edgeWidth = collapsed ? edgeNarrow : edgeWide;
                int edgeX = w - edgeWidth;
                int edgeH = pnlSidebarContainer.ClientSize.Height;
                if (edgeH <= 0) edgeH = pnlSidebarContainer.Height;
                if (edgeH <= 0) edgeH = this.ClientSize.Height - (pnlUstBaslik?.Height ?? 0);

                pnlSidebarToggleEdge.Visible = true;
                pnlSidebarToggleEdge.BringToFront();
                pnlSidebarToggleEdge.Location = new Point(edgeX, 0);
                pnlSidebarToggleEdge.Size = new Size(edgeWidth, Math.Max(edgeH, 100));
                int centerY = (pnlSidebarToggleEdge.Height - 24) / 2;
                btnSidebarToggle.Text = collapsed ? "\u25B6" : "\u25C0";
                btnSidebarToggle.SetBounds((edgeWidth - 24) / 2, centerY, 24, 24);

                // Dar mod: sadece spacer + footer (avatar + logout) ve sağda toggle; ikonlar gizli
                int menuH = Math.Max(100, pnlSidebarContainer.ClientSize.Height);
                if (menuH <= 0) menuH = pnlSidebarContainer.Height;
                pnlSidebarSpacer.Visible = collapsed;
                if (collapsed)
                {
                    pnlSidebarSpacer.Width = w - edgeWidth;
                    pnlSidebarSpacer.Height = Math.Max(0, menuH - pnlSidebarUserFooter.Height - 16);
                }

                foreach (Control c in pnlSolMenu.Controls)
            {
                if (c == pnlSidebarSpacer) continue;
                if (c is Button btn)
                {
                    if (collapsed)
                    {
                        btn.Visible = false;
                        if (btn.Tag == null) btn.Tag = btn.Text;
                        btn.Text = "";
                        btn.Width = SidebarWidthCollapsed;
                        btn.ImageAlign = ContentAlignment.MiddleCenter;
                    }
                    else
                    {
                        btn.Visible = true;
                        if (btn.Tag != null) { btn.Text = (string)btn.Tag; btn.Tag = null; }
                        btn.Width = 250;
                        btn.ImageAlign = ContentAlignment.MiddleLeft;
                    }
                }
                else if (c is Label lbl)
                {
                    lbl.Visible = !collapsed;
                }
                else if (c == pnlLogo)
                    pnlLogo.Visible = !collapsed;
                else if (c == pnlSidebarUserFooter)
                {
                    pnlSidebarUserFooter.Width = collapsed ? SidebarWidthCollapsed : 294;
                    lblSidebarUserName.Visible = !collapsed;
                    lblSidebarUserRole.Visible = !collapsed;
                    if (collapsed)
                    {
                        btnSidebarLogout.Text = "\u21E1"; // ⤴
                        btnSidebarLogout.SetBounds(46, 12, 10, 28);
                    }
                    else
                    {
                        btnSidebarLogout.Text = "Çıkış";
                        btnSidebarLogout.SetBounds(218, 20, 48, 32);
                    }
                }
            }
            }
            finally
            {
                this.ResumeLayout(true);
                this.PerformLayout();
                pnlSidebarContainer.Refresh();
                pnlSidebarToggleEdge.Refresh();
                HideScrollBars();
                if (!collapsed)
                    ApplyMenuVisibilityByPermission();
            }
        }

        /// <summary>
        /// Toggle panelinde chevron butonunu dikey ortalar.
        /// </summary>
        private void CenterToggleButtonsInEdge()
        {
            if (pnlSidebarToggleEdge.Height < 24) return;
            int cx = (pnlSidebarToggleEdge.Width - 24) / 2;
            int centerY = (pnlSidebarToggleEdge.Height - 24) / 2;
            btnSidebarToggle.SetBounds(cx, centerY, 24, 24);
        }

        /// <summary>
        /// Sol menü ve form scrollbar'larını gizler (kaydırma fare tekerleği ile çalışmaya devam eder).
        /// </summary>
        private void HideScrollBars()
        {
            try
            {
                if (pnlSolMenu.IsHandleCreated)
                    ShowScrollBar(pnlSolMenu.Handle, SB_BOTH, false);
                if (this.IsHandleCreated)
                    ShowScrollBar(this.Handle, SB_BOTH, false);
            }
            catch { }
        }

        private void btnSidebarToggle_Click(object sender, EventArgs e)
        {
            _sidebarCollapsed = !_sidebarCollapsed;
            ApplySidebarState();
            SaveSidebarCollapsedState();
        }


        private void ApplyTheme()
        {
            pnlSidebarContainer.BackColor = AppTheme.SidebarBackground;
            pnlSolMenu.BackColor = AppTheme.SidebarBackground;
            pnlSidebarUserFooter.BackColor = AppTheme.SidebarBackground;
            pnlSidebarToggleEdge.BackColor = AppTheme.SidebarBackground;
            pnlSidebarSpacer.BackColor = AppTheme.SidebarBackground;
            pnlLogo.BackColor = AppTheme.SidebarLogoBackground;
            pnlUstBaslik.BackColor = AppTheme.CardBackground;
            islemEkraniPanel.BackColor = AppTheme.ContentBackground;
            lblSayfaBasligi.ForeColor = AppTheme.TextPrimary;
            lblSidebarUserName.ForeColor = System.Drawing.Color.LightGray;
            lblSidebarUserRole.ForeColor = System.Drawing.Color.Gray;
            lblSidebarUserAvatar.ForeColor = System.Drawing.Color.White;
            btnSidebarLogout.ForeColor = System.Drawing.Color.LightGray;
        }
        private void calismaStatuleriMenu_Click(object sender, EventArgs e)
        {
            OpenTUserControl<ucCalismaStatuleri>(islemEkraniPanel);
        }
        private void calismaSekilleriMenu_Click(object sender, EventArgs e)
        {
            OpenTUserControl<ucCalismaSekilleri>(islemEkraniPanel);
        }
        private void btnKisiHareketlerEkrani_Click(object sender, EventArgs e)
        {
            OpenTUserControl<ucKisiHareketler>(islemEkraniPanel);
        }
        private void btnAylikPuantajEkrani_Click(object sender, EventArgs e)
        {
            OpenTUserControl<ucAylikPuantajEkrani>(islemEkraniPanel);
        }
        private void btnCihazlarEkrani_Click(object sender, EventArgs e)
        {
            OpenTUserControl<ucCihazlar>(islemEkraniPanel);
        }
        private void btnResmiTatiller_Click(object sender, EventArgs e)
        {
            OpenTUserControl<ucResmiTatiller>(islemEkraniPanel);
        }
        private void btnAdminPanel_Click(object sender, EventArgs e)
        {
            OpenTUserControl<CeyPASS.WFA.UserControls.Admin.ucAdminPanel>(islemEkraniPanel);
        }

        private void btnSidebarLogout_Click(object sender, EventArgs e)
        {
            var onay = MessageBox.Show("Çıkış yapmak istediğinize emin misiniz?", "Çıkış",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (onay != DialogResult.Yes) return;
            _session.Clear();
            this.Close();
        }
    }
}
