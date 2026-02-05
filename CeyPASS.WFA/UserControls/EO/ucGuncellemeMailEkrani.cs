using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using System;
using System.IO;
using System.Windows.Forms;

namespace CeyPASS.WFA.UserControls.EO
{
    public partial class ucGuncellemeMailEkrani : UserControl
    {
        private readonly INotificationService _notificationService;
        private readonly IMailService _mailService;
        private readonly IAuthorizationService _auth;
        private readonly ISessionContext _session;
        AuthorizationHelper authHelp;
        private const string PageName = "Guncelleme";
        private const string PageNameUI = "Güncelleme Bildirimi";

        public ucGuncellemeMailEkrani(INotificationService notificationService, IMailService mailService, IAuthorizationService auth, ISessionContext session)
        {
            InitializeComponent();
            _notificationService = notificationService;
            _mailService = mailService;
            _auth = auth;
            _session = session;
            authHelp = new AuthorizationHelper(_session, _auth);
            if (!_auth.ViewAbility(PageName))
            {
                LogHelper.Warn("Guncelleme", "View", "Görüntüleme yetkisi yok", detayJson: $"{{\"KullaniciId\":{_session.AktifKullaniciId}}}");
                MessageBox.Show("Güncelleme Bildirimi ekranını görüntüleme yetkiniz yok");
                this.Visible = false;
                return;
            }
            AppTheme.ApplyToControl(this);
        }

        private void ucGuncellemeMailEkrani_Load(object sender, EventArgs e)
        {
            dtpTarih.Value = DateTime.Now;
            cmbTip.SelectedIndex = 1;
            txtVersiyon.Text = Application.ProductVersion;
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);

            btnGonder.Tag = YetkiTipleri.Create;
            btnHataDuzeltmeEkle.Tag = YetkiTipleri.Create;
            btnHataDuzeltmeSil.Tag = YetkiTipleri.Delete;
            btnIyilestirmeEkle.Tag = YetkiTipleri.Create;
            btnIyilestirmeSil.Tag = YetkiTipleri.Delete;
            btnKritikDegisiklikEkle.Tag = YetkiTipleri.Create;
            btnKritikDegisiklikSil.Tag = YetkiTipleri.Delete;
            btnOnizleme.Tag = YetkiTipleri.View;
            btnYeniOzellikEkle.Tag = YetkiTipleri.Create;
            btnYeniOzellikSil.Tag = YetkiTipleri.Delete;
        }
        private void btnYeniOzellikEkle_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtYeniOzellik.Text))
            {
                MessageBox.Show("Lütfen bir özellik açıklaması girin.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtYeniOzellik.Focus();
                return;
            }

            lstYeniOzellikler.Items.Add(txtYeniOzellik.Text.Trim());
            txtYeniOzellik.Clear();
            txtYeniOzellik.Focus();
        }
        private void btnYeniOzellikSil_Click(object sender, EventArgs e)
        {
            if (lstYeniOzellikler.SelectedIndex >= 0)
            {
                lstYeniOzellikler.Items.RemoveAt(lstYeniOzellikler.SelectedIndex);
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir öğe seçin.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void btnIyilestirmeEkle_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIyilestirme.Text))
            {
                MessageBox.Show("Lütfen bir iyileştirme açıklaması girin.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIyilestirme.Focus();
                return;
            }

            lstIyilestirmeler.Items.Add(txtIyilestirme.Text.Trim());
            txtIyilestirme.Clear();
            txtIyilestirme.Focus();
        }
        private void btnIyilestirmeSil_Click(object sender, EventArgs e)
        {
            if (lstIyilestirmeler.SelectedIndex >= 0)
            {
                lstIyilestirmeler.Items.RemoveAt(lstIyilestirmeler.SelectedIndex);
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir öğe seçin.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void btnHataDuzeltmeEkle_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtHataDuzeltme.Text))
            {
                MessageBox.Show("Lütfen bir hata düzeltmesi açıklaması girin.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHataDuzeltme.Focus();
                return;
            }

            lstHataDuzeltmeleri.Items.Add(txtHataDuzeltme.Text.Trim());
            txtHataDuzeltme.Clear();
            txtHataDuzeltme.Focus();
        }
        private void btnHataDuzeltmeSil_Click(object sender, EventArgs e)
        {
            if (lstHataDuzeltmeleri.SelectedIndex >= 0)
            {
                lstHataDuzeltmeleri.Items.RemoveAt(lstHataDuzeltmeleri.SelectedIndex);
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir öğe seçin.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void btnKritikDegisiklikEkle_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtKritikDegisiklik.Text))
            {
                MessageBox.Show("Lütfen bir kritik değişiklik açıklaması girin.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtKritikDegisiklik.Focus();
                return;
            }

            lstKritikDegisiklikler.Items.Add(txtKritikDegisiklik.Text.Trim());
            txtKritikDegisiklik.Clear();
            txtKritikDegisiklik.Focus();
        }
        private void btnKritikDegisiklikSil_Click(object sender, EventArgs e)
        {
            if (lstKritikDegisiklikler.SelectedIndex >= 0)
            {
                lstKritikDegisiklikler.Items.RemoveAt(lstKritikDegisiklikler.SelectedIndex);
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir öğe seçin.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void btnOnizleme_Click(object sender, EventArgs e)
        {
            try
            {
                var guncellemeDto = GuncellemeDtoOlustur();

                if (!GuncellemeDogrula(guncellemeDto))
                    return;

                string htmlOnizleme = _notificationService.OnizlemeHtmlOlustur(guncellemeDto);

                string tempPath = Path.Combine(Path.GetTempPath(), "ceypass_email_onizleme.html");
                File.WriteAllText(tempPath, htmlOnizleme, System.Text.Encoding.UTF8);

                System.Diagnostics.Process.Start(tempPath);

                MessageBox.Show("Email önizlemesi tarayıcınızda açıldı.", "Bilgi",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Önizleme oluşturulurken hata: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void btnGonder_Click(object sender, EventArgs e)
        {
            try
            {
                var guncellemeDto = GuncellemeDtoOlustur();

                if (!GuncellemeDogrula(guncellemeDto))
                    return;

                var sonuc = MessageBox.Show(
                    "Güncelleme bildirimi tüm kullanıcılara gönderilecek. Emin misiniz?",
                    "Onay",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (sonuc != DialogResult.Yes)
                    return;

                btnGonder.Enabled = false;
                btnOnizleme.Enabled = false;
                this.Cursor = Cursors.WaitCursor;

                bool basarili = await _notificationService.GuncellemeNotifikasyonuGonderAsync(guncellemeDto);

                this.Cursor = Cursors.Default;
                btnGonder.Enabled = true;
                btnOnizleme.Enabled = true;

                if (basarili)
                {
                    MessageBox.Show("Güncelleme bildirimi başarıyla gönderildi!", "Başarılı",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Mail gönderilirken bir hata oluştu. Lütfen ayarları kontrol edin.", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                btnGonder.Enabled = true;
                btnOnizleme.Enabled = true;
                MessageBox.Show(
                            $"Hata Detayı:\n\n{ex.Message}\n\n" +
                            $"Inner Exception:\n{ex.InnerException?.Message}\n\n" +
                            $"Stack Trace:\n{ex.StackTrace}",
                            "Detaylı Hata",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
            }
        }
        private void btnIptal_Click(object sender, EventArgs e) { }
        private GuncellemeNotifikasyonDTO GuncellemeDtoOlustur()
        {
            var dto = new GuncellemeNotifikasyonDTO
            {
                VersiyonNumarasi = txtVersiyon.Text.Trim(),
                YayinTarihi = dtpTarih.Value,
                GuncellemeTipi = cmbTip.Text,
                EkNotlar = txtEkNotlar.Text.Trim()
            };

            // Yeni Özellikler
            foreach (var item in lstYeniOzellikler.Items)
            {
                dto.YeniOzellikler.Add(item.ToString());
            }

            // İyileştirmeler
            foreach (var item in lstIyilestirmeler.Items)
            {
                dto.Iyilestirmeler.Add(item.ToString());
            }

            // Hata Düzeltmeleri
            foreach (var item in lstHataDuzeltmeleri.Items)
            {
                dto.HataDuzeltmeleri.Add(item.ToString());
            }

            // Kritik Değişiklikler
            foreach (var item in lstKritikDegisiklikler.Items)
            {
                dto.KritikDegisiklikler.Add(item.ToString());
            }

            return dto;
        }
        private bool GuncellemeDogrula(GuncellemeNotifikasyonDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.VersiyonNumarasi))
            {
                MessageBox.Show("Lütfen versiyon numarası girin.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtVersiyon.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(dto.GuncellemeTipi))
            {
                MessageBox.Show("Lütfen güncelleme tipini seçin.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbTip.Focus();
                return false;
            }

            if (dto.YeniOzellikler.Count == 0 && dto.Iyilestirmeler.Count == 0 && dto.HataDuzeltmeleri.Count == 0 && dto.KritikDegisiklikler.Count == 0)
            {
                MessageBox.Show("En az bir kategoriye madde eklemelisiniz.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;

        }
    }
}
