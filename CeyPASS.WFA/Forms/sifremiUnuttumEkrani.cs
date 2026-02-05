using CeyPASS.Business.Abstractions;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CeyPASS.WFA
{
    public partial class sifremiUnuttumEkrani : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;
        private readonly string _kullaniciAdi;
        private readonly ISifreService _ssvc;
        private readonly IEmailService _esvc;

        public sifremiUnuttumEkrani(string kullaniciAdi, ISifreService ssvc, IEmailService esvc)
        {
            InitializeComponent();
            SendMessage(txtDogrulamaKodu.Handle, EM_SETCUEBANNER, 0, "Mailinize gelen doğrulama kodunu giriniz");
            SendMessage(txtYeniSifre.Handle, EM_SETCUEBANNER, 0, "Yeni şifrenizi giriniz");
            SendMessage(txtYeniSifreKontrol.Handle, EM_SETCUEBANNER, 0, "Yeni şifrenizi tekrar giriniz");
            _kullaniciAdi = kullaniciAdi;
            _ssvc = ssvc;
            _esvc = esvc;
        }

        private void btnYeniSifreKaydet_Click(object sender, EventArgs e)
        {
            try
            {
                var sonuc = _ssvc.SifreSifirlamaTamamla(_kullaniciAdi, txtDogrulamaKodu.Text.Trim(), txtYeniSifre.Text, txtYeniSifreKontrol.Text);

                if (!sonuc.Basarili)
                {
                    MessageBox.Show(sonuc.HataMesaji, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show("Şifreniz başarıyla güncellendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Beklenmeyen bir hata oluştu.\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void sifremiUnuttumEkrani_Load(object sender, EventArgs e)
        {
            ApplyTheme();
            try
            {
                var sonuc = _ssvc.SifreSifirlamaBaslat(_kullaniciAdi);

                if (!sonuc.Basarili)
                {
                    MessageBox.Show(sonuc.HataMesaji,"Hata",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    this.Close();
                    return;
                }

                string maskedMail = _esvc.MaskEmail(sonuc.Email);

                MessageBox.Show($"Doğrulama kodu {maskedMail} adresine gönderildi.","Bilgi",MessageBoxButtons.OK,MessageBoxIcon.Information);

                txtDogrulamaKodu.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyin.\n{ex.Message}","Hata",MessageBoxButtons.OK,MessageBoxIcon.Error);
                this.Close();
            }
        }
        private void ApplyTheme()
        {
            pnlBackground.BackColor = AppTheme.ContentBackground;
            pnlCard.BackColor = AppTheme.CardBackground;
            btnYeniSifreKaydet.BackColor = AppTheme.Primary;
            btnYeniSifreKaydet.ForeColor = Color.White;
        }
    }
}
