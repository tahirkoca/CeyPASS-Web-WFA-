using CeyPASS.Business.Abstractions;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CeyPASS.WFA.Forms
{
    /// <summary>Canlı İzleme giriş ekranı — bölge, kullanıcı ve şifre ile oturum açar.</summary>
    public partial class canliIzlemeGirisEkrani : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;
        private girisEkrani girisFormuRef;
        private readonly ISessionContext _session;
        private readonly ICanliIzlemeService _svc;
        private readonly IKisiHareketService _khsvc;
        private readonly IKisiDetayService _kdsvc;
        private readonly IMisafirKartService _msvc;
        private readonly IAracKartiService _aracSvc;
        private readonly ICanliIzlemeKartKomutService _kartKomutSvc;

        /// <summary>DI ile servisleri alır; giriş butonunu varsayılan kabul tuşu yapar.</summary>
        public canliIzlemeGirisEkrani(girisEkrani girisFormu, ISessionContext session, ICanliIzlemeService svc, IKisiHareketService khsvc, IKisiDetayService kdsvc, IMisafirKartService msvc, IAracKartiService aracSvc, ICanliIzlemeKartKomutService kartKomutSvc)
        {
            InitializeComponent();
            SendMessage(canliEkranSifre.Handle, EM_SETCUEBANNER, 0, "Şifrenizi giriniz");
            this.girisFormuRef = girisFormu;
            _session = session;
            _svc = svc;
            _khsvc = khsvc;
            _kdsvc = kdsvc;
            _msvc = msvc;
            _aracSvc = aracSvc;
            _kartKomutSvc = kartKomutSvc;
            // İş kuralı: Enter ile giriş (AcceptButton); combo açıkken Enter sadece listeyi kapatır, login tetiklenmez.
            AcceptButton = canliEkranGirisButon;
            KeyPreview = true;
            KeyDown += CanliIzlemeGiris_KeyDown;
        }

        // İş kuralı: Combo DroppedDown iken Enter login değil, dropdown kapatma.
        private void CanliIzlemeGiris_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter && e.KeyCode != Keys.Return)
                return;
            if (canliIzlemeBolgeBox.DroppedDown)
            {
                canliIzlemeBolgeBox.DroppedDown = false;
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            if (canliEkranKullaniciAdi.DroppedDown)
            {
                canliEkranKullaniciAdi.DroppedDown = false;
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
        private void canliIzlemeGirisEkrani_Load(object sender, EventArgs e)
        {
            ApplyTheme();
            pnlCard.Location = new System.Drawing.Point(
                (pnlBackground.ClientSize.Width - pnlCard.Width) / 2,
                (pnlBackground.ClientSize.Height - pnlCard.Height) / 2);
            var dt = _svc.GetFirmalar();
            canliIzlemeBolgeBox.DataSource = dt;
            canliIzlemeBolgeBox.DisplayMember = "FirmaAdi";
            canliIzlemeBolgeBox.ValueMember = "FirmaId";
            canliIzlemeBolgeBox.SelectedIndexChanged += canliIzlemeBolgeBox_SelectedIndexChanged;
            // İlk bölgeyi seçip kullanıcı dropdown'ını hemen doldur (açılışta liste görünsün)
            if (canliIzlemeBolgeBox.Items.Count > 0)
                canliIzlemeBolgeBox.SelectedIndex = 0;
            FillKullaniciCombo();
        }
        private void canliIzlemeBolgeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillKullaniciCombo();
        }
        private void FillKullaniciCombo()
        {
            if (canliIzlemeBolgeBox.SelectedValue == null || !int.TryParse(canliIzlemeBolgeBox.SelectedValue.ToString(), out int firmaId))
            {
                canliEkranKullaniciAdi.DataSource = null;
                canliEkranKullaniciAdi.Items.Clear();
                return;
            }
            var adlar = _svc.GetKullaniciAdlariByFirma(firmaId);
            canliEkranKullaniciAdi.DataSource = adlar ?? new System.Collections.Generic.List<string>();
            if (canliEkranKullaniciAdi.Items.Count > 0)
                canliEkranKullaniciAdi.SelectedIndex = 0;
        }
        private void ApplyTheme()
        {
            pnlBackground.BackColor = AppTheme.ContentBackground;
            pnlCard.BackColor = AppTheme.CardBackground;
            canliEkranGirisButon.BackColor = AppTheme.Primary;
            canliEkranGirisButon.ForeColor = Color.White;
        }
        private void canliIzlemeGirisEkrani_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
        private void canliEkranGirisButon_Click(object sender, EventArgs e)
        {
            if (canliIzlemeBolgeBox.SelectedValue == null) { MessageBox.Show("Lütfen bölge seçin."); return; }

            int firmaId = Convert.ToInt32(canliIzlemeBolgeBox.SelectedValue);
            string user = (canliEkranKullaniciAdi.SelectedValue ?? canliEkranKullaniciAdi.Text)?.ToString()?.Trim() ?? "";
            string password = canliEkranSifre.Text;
            var auth = _svc.Login(firmaId, user, password);

            if (auth == null) { MessageBox.Show("Hatalı kullanıcı adı/şifre veya bu bölge için yetki yok."); return; }

            _session.AktifFirmaId = firmaId;
            _session.AktifKullaniciId = auth.KullaniciId;
            _session.AdSoyad = auth.KullaniciAdi ?? "";
            _session.RolAdi = auth.Rol ?? "";

            girisFormuRef?.Hide();
            this.Hide();
            new canliIzlemeVeriEkrani(_session, _svc, _khsvc, _kdsvc, _msvc, _aracSvc, _kartKomutSvc).Show();
        }
    }
}
