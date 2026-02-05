using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using CeyPASS.WFA.Forms;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CeyPASS.WFA
{
    public partial class girisEkrani : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;
        private Label lblVersion;
        private readonly ISessionContext _session;
        private readonly ICanliIzlemeService _svc;
        private readonly ISifreService _ssvc;
        private readonly IKullaniciService _ksvc;
        private readonly IEmailService _esvc;
        private readonly IKisiHareketService _khsvc;
        private readonly IKisiDetayService _kdsvc;
        private readonly IMisafirKartService _mksvc;
        private readonly IServiceProvider _sp;

        public girisEkrani(ISessionContext session,ICanliIzlemeService svc,ISifreService ssvc,IKullaniciService ksvc,IEmailService esvc,IKisiHareketService khsvc,IKisiDetayService kdsvc,IMisafirKartService mksvc, IServiceProvider sp)
        {
            InitializeComponent();
            SendMessage(txtKullaniciAdi.Handle, EM_SETCUEBANNER, 0, "Kullanıcı adınızı giriniz");
            SendMessage(txtSifre.Handle, EM_SETCUEBANNER, 0, "Şifrenizi giriniz");
            this.KeyPreview = true;
            this.KeyDown += girisEkrani_KeyDown;
            _session = session;
            _svc = svc;
            _ssvc = ssvc;
            _ksvc = ksvc;
            _esvc = esvc;
            _khsvc = khsvc;
            _kdsvc = kdsvc;
            _mksvc = mksvc;
            _sp = sp;
        }
        private void girisEkrani_Load(object sender, EventArgs e) 
        {
            CreateVersionLabel();
            // Sürüm: AssemblyVersion ile aynı kaynaktan; Application.ProductVersion AssemblyInformationalVersion kullanır
            var version = Application.ProductVersion ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";
            this.Text = $"CeyPASS v{version}";
        }
        private void btnGiris_Click(object sender, EventArgs e)
        {
            string kullaniciAdi = txtKullaniciAdi.Text.Trim();
            string sifre = txtSifre.Text.Trim();

            Kullanici kullanici = _ksvc.GirisYap(kullaniciAdi, sifre);

            if (kullanici != null)
            {
                if (kullanici.FirmaId == null)
                {
                    MessageBox.Show("Bu kullanıcıya tanımlı firma bulunamadı.");
                    return;
                }

                _session.AktifKullaniciId = kullanici.KullaniciId;
                _session.AktifFirmaId = kullanici.FirmaId;
                _session.AdSoyad = kullanici.AdSoyad;
                _session.RolAdi = kullanici.RolTanimi;
                _session.RolId= kullanici.RolId;

                LogHelper.Info("Giris", "Login", $"Kullanıcı giriş yaptı: {kullanici.AdSoyad}");
                // Faz 4.2: İşlem ekranı kendi scope'unda; kapatılınca scope dispose edilir
                var islemScope = _sp.CreateScope();
                var anaSayfa = islemScope.ServiceProvider.GetRequiredService<islemEkrani>();
                anaSayfa.FormClosed += (_, _) =>
                {
                    islemScope.Dispose();
                    this.Show();
                };
                anaSayfa.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("❌ Hatalı kullanıcı adı veya şifre!");
            }
        }
        private void btnSifremiUnuttum_Click(object sender, EventArgs e)
        {
            string kullaniciAdi = txtKullaniciAdi.Text.Trim();

            if (string.IsNullOrEmpty(kullaniciAdi))
            {
                MessageBox.Show("Lütfen kullanıcı adınızı girin.");
                return;
            }

            // Faz 4.2: Scoped servisler için scope aç
            var scope = _sp.CreateScope();
            var sifremiUnuttumFormu = ActivatorUtilities.CreateInstance<sifremiUnuttumEkrani>(scope.ServiceProvider, kullaniciAdi);
            sifremiUnuttumFormu.FormClosed += (_, _) => scope.Dispose();
            sifremiUnuttumFormu.ShowDialog();
        }
        private void girisEkrani_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
        private void chxSifreGoster_CheckedChanged(object sender, EventArgs e)
        {
            if (chxSifreGoster.Checked)
            {
                txtSifre.PasswordChar = '\0';
            }
            else
            {
                txtSifre.PasswordChar = '*';
            }
        }
        private void girisEkrani_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnGiris.PerformClick();
            }
        }
        private void btnCanliIzleme_Click(object sender, EventArgs e)
        {
            // Faz 4.2: Scoped servisler için scope aç
            var canliScope = _sp.CreateScope();
            var canliIzlemeLoginForm = ActivatorUtilities.CreateInstance<canliIzlemeGirisEkrani>(canliScope.ServiceProvider, this);
            canliIzlemeLoginForm.FormClosed += (_, _) => canliScope.Dispose();
            this.Hide();
            canliIzlemeLoginForm.Show();
        }
        private void CreateVersionLabel()
        {
            // Sürüm: Application.ProductVersion (AssemblyInformationalVersion) = kurulum/giriş ekranında tutarlı
            var version = Application.ProductVersion ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";

            // Label oluştur
            lblVersion = new Label
            {
                Text = $"Ver {version}",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Padding = new Padding(10, 5, 10, 5)
            };

            //pnlBackground.Controls.Add(lblVersion, 2, 4);
            pnlBackground.Controls.Add(lblVersion);

            // Sağ alt köşeye yerleştir
            lblVersion.Location = new Point(
                this.ClientSize.Width - lblVersion.Width - 15,
                this.ClientSize.Height - lblVersion.Height - 10
            );

            // Resize olayında konumu güncelle
            this.Resize += (s, e) =>
            {
                if (lblVersion != null)
                {
                    lblVersion.Location = new Point(
                        this.ClientSize.Width - lblVersion.Width - 15,
                        this.ClientSize.Height - lblVersion.Height - 10
                    );
                }
            };
        }
        private void pnlBackground_Paint(object sender, PaintEventArgs e)
        {
            using (SolidBrush overlay = new SolidBrush(Color.FromArgb(70, 0, 102, 179)))
            {
                e.Graphics.FillRectangle(overlay, pnlBackground.ClientRectangle);
            }
        }
        private void pnlLoginCard_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int radius = 15;
            Rectangle rect = new Rectangle(0, 0, pnlLoginCard.Width - 1, pnlLoginCard.Height - 1);
            GraphicsPath path = GetRoundedRectangle(rect, radius);
            pnlLoginCard.Region = new Region(path);

            using (Pen pen = new Pen(Color.FromArgb(50, 0, 102, 179), 1))
            {
                e.Graphics.DrawPath(pen, path);
            }
        }
        private GraphicsPath GetRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }
        private void txtKullaniciAdi_Enter(object sender, EventArgs e)
        {
            txtKullaniciAdi.BackColor = Color.FromArgb(248, 249, 250);
            lblUsername.ForeColor = AppTheme.BorderFocus;
        }
        private void txtKullaniciAdi_Leave(object sender, EventArgs e)
        {
            txtKullaniciAdi.BackColor = Color.White;
            lblUsername.ForeColor = AppTheme.TextPrimary;
        }
        private void txtSifre_Enter(object sender, EventArgs e)
        {
            txtSifre.BackColor = Color.FromArgb(248, 249, 250);
            lblPassword.ForeColor = AppTheme.BorderFocus;
        }
        private void txtSifre_Leave(object sender, EventArgs e)
        {
            txtSifre.BackColor = Color.White;
            lblPassword.ForeColor = AppTheme.TextPrimary;
        }
        private void btnGiris_MouseEnter(object sender, EventArgs e)
        {
            btnGiris.BackColor = AppTheme.LoginButtonHover;
        }
        private void btnGiris_MouseLeave(object sender, EventArgs e)
        {
            btnGiris.BackColor = AppTheme.LoginButton;
        }
        private void btnCanliIzleme_MouseEnter(object sender, EventArgs e)
        {
            btnCanliIzleme.BackColor = Color.Transparent;
        }
        private void btnCanliIzleme_MouseLeave(object sender, EventArgs e)
        {
            btnCanliIzleme.BackColor = Color.Transparent;
        }
    }
}
