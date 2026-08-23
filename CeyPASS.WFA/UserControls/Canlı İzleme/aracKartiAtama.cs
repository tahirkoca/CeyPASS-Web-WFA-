using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Entities.Helpers;
using System;
using System.Windows.Forms;

namespace CeyPASS.WFA.UserControls.Canlı_İzleme
{
    public partial class aracKartiAtama : UserControl
    {
        private enum EMode { Yeni, Guncelle }
        private EMode _mode;
        private readonly ISessionContext _session;
        private readonly IAracKartiService _svc;
        private bool _isSaving;
        private string _tamTc;
        private bool _suppressTcChange;

        public aracKartiAtama(ISessionContext session, IAracKartiService svc)
        {
            InitializeComponent();
            _session = session;
            _svc = svc;
            cmbPuantajsizKartlar.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPuantajsizKartlar.SelectedIndexChanged += cmbPuantajsizKartlar_SelectedIndexChanged;
            txtTCKimlikNo.Leave += TxtTCKimlikNo_Leave;
            txtTCKimlikNo.KeyDown += TxtTCKimlikNo_KeyDown;
            txtTCKimlikNo.TextChanged += TxtTCKimlikNo_TextChanged;
            gecmisZiyaretciPanel.Visible = false;
            gecmisZiyaretciPanel.ZiyaretciSecildi += GecmisZiyaretciPanel_ZiyaretciSecildi;
        }

        private void TxtTCKimlikNo_TextChanged(object sender, EventArgs e)
        {
            if (_suppressTcChange) return;
            var shown = txtTCKimlikNo.Text?.Trim() ?? "";
            if (!string.IsNullOrEmpty(_tamTc) && shown == TcKimlikHelper.Mask(_tamTc))
                return;
            _tamTc = null;
        }

        private void ShowMaskedTc(string fullTc)
        {
            _tamTc = string.IsNullOrWhiteSpace(fullTc) ? null : fullTc.Trim();
            _suppressTcChange = true;
            txtTCKimlikNo.Text = string.IsNullOrEmpty(_tamTc) ? "" : TcKimlikHelper.Mask(_tamTc);
            _suppressTcChange = false;
        }

        private string ResolveTcForSave() =>
            TcKimlikHelper.ResolveForSave(txtTCKimlikNo.Text, _tamTc);

        private void GecmisZiyaretciPanel_ZiyaretciSecildi(GecmisZiyaretciItem item)
        {
            if (item == null) return;

            txtAdSoyad.Text = item.AdSoyad ?? "";
            ShowMaskedTc(item.TCKimlikNo);
            txtPlaka.Text = item.Plaka ?? "";
            txtZiyaretEdilenKisi.Text = item.ZiyaretEdilenKisi ?? "";
            dtpGirisSaati.Value = DateTime.Now;
        }

        private void TxtTCKimlikNo_Leave(object sender, EventArgs e) => TryFillFromTc();

        private void TxtTCKimlikNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                TryFillFromTc();
            }
        }

        private void TryFillFromTc()
        {
            var tc = txtTCKimlikNo?.Text?.Trim();
            if (string.IsNullOrEmpty(tc) || TcKimlikHelper.LooksMasked(tc)) return;

            try
            {
                var rec = _svc.GetBilgisiByTc(tc);
                if (rec == null) return;

                if (!string.IsNullOrEmpty(rec.MisafirAdSoyad) && string.IsNullOrWhiteSpace(txtAdSoyad.Text))
                    txtAdSoyad.Text = rec.MisafirAdSoyad;
                if (!string.IsNullOrEmpty(rec.ZiyaretEdilenKisi) && string.IsNullOrWhiteSpace(txtZiyaretEdilenKisi.Text))
                    txtZiyaretEdilenKisi.Text = rec.ZiyaretEdilenKisi;
                if (!string.IsNullOrEmpty(rec.Plaka) && string.IsNullOrWhiteSpace(txtPlaka.Text))
                    txtPlaka.Text = rec.Plaka;
            }
            catch
            {
            }
        }

        private void aracKartiAtama_Load(object sender, EventArgs e) { }

        private void btnKaydet_Click(object sender, EventArgs e)
        {
            if (_isSaving) return;
            _isSaving = true;
            btnKaydet.Enabled = false;

            try
            {
                var tc = ResolveTcForSave();
                var kimeGeldigi = string.IsNullOrWhiteSpace(txtZiyaretEdilenKisi.Text) ? null : txtZiyaretEdilenKisi.Text.Trim();
                var plaka = string.IsNullOrWhiteSpace(txtPlaka.Text) ? null : txtPlaka.Text.Trim();
                if (string.IsNullOrWhiteSpace(plaka))
                    throw new InvalidOperationException("Plaka giriniz.");

                if (_mode == EMode.Yeni)
                {
                    if (cmbPuantajsizKartlar.SelectedValue == null)
                        throw new InvalidOperationException("Kart seçiniz.");

                    string kartId = Convert.ToString(cmbPuantajsizKartlar.SelectedValue);
                    _svc.CreateAssignment(
                        firmaId: (int)_session.AktifFirmaId,
                        personelId: kartId,
                        adSoyad: txtAdSoyad.Text,
                        girisSaati: dtpGirisSaati.Value,
                        aciklama: txtAciklama.Text,
                        tcKimlikNo: tc,
                        ziyaretEdilenKisi: kimeGeldigi,
                        plaka: plaka);

                    MessageBox.Show("Kayıt başarıyla oluşturuldu.");
                    this.FindForm()?.Close();
                }
                else
                {
                    var a = cmbPuantajsizKartlar.SelectedItem as PuantajsizKartAtama;
                    if (a == null)
                        throw new InvalidOperationException("Güncellenecek atamayı seçiniz.");

                    _svc.UpdateAssignment(
                        atamaId: a.AtamaId,
                        adSoyad: txtAdSoyad.Text,
                        girisSaati: dtpGirisSaati.Value,
                        cikisSaati: dtpCikisSaati.Enabled ? dtpCikisSaati.Value : (DateTime?)null,
                        aciklama: txtAciklama.Text,
                        tcKimlikNo: tc,
                        ziyaretEdilenKisi: kimeGeldigi,
                        plaka: plaka);

                    MessageBox.Show("Kayıt güncellendi.");
                    this.FindForm()?.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isSaving = false;
                if (this.FindForm() != null) btnKaydet.Enabled = true;
            }
        }

        private void btnIptal_Click(object sender, EventArgs e) => this.FindForm()?.Close();

        private void cmbPuantajsizKartlar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_mode != EMode.Guncelle) return;
            var a = cmbPuantajsizKartlar.SelectedItem as PuantajsizKartAtama;
            if (a == null) return;

            txtAdSoyad.Text = a.MisafirAdSoyad ?? "";
            txtZiyaretEdilenKisi.Text = a.ZiyaretEdilenKisi ?? "";
            ShowMaskedTc(a.TCKimlikNo);
            txtPlaka.Text = a.Plaka ?? "";
            txtAciklama.Text = a.Notlar ?? "";
            dtpGirisSaati.Value = a.Baslangic;
            dtpCikisSaati.Value = DateTime.Now;
        }

        public void InitYeni(int firmaId)
        {
            _mode = EMode.Yeni;
            lblHeader.Text = "Araç Kartı Ver";
            _session.AktifFirmaId = firmaId;

            var cards = _svc.GetCardsForNew(firmaId);
            cmbPuantajsizKartlar.DataSource = null;
            cmbPuantajsizKartlar.DisplayMember = nameof(KisiListItem.AdSoyad);
            cmbPuantajsizKartlar.ValueMember = nameof(KisiListItem.PersonelId);
            cmbPuantajsizKartlar.DataSource = cards;

            if (cards != null && cards.Count > 0)
                cmbPuantajsizKartlar.SelectedIndex = 0;

            txtAdSoyad.Clear();
            ShowMaskedTc(null);
            txtPlaka.Clear();
            txtZiyaretEdilenKisi.Clear();
            txtAciklama.Clear();
            dtpGirisSaati.Value = DateTime.Now;
            dtpCikisSaati.Enabled = false;

            gecmisZiyaretciPanel.Visible = true;
            gecmisZiyaretciPanel.SetSearchPlaceholder("İsim veya plaka ara...");
            gecmisZiyaretciPanel.LoadListe(ad => _svc.SearchGecmisZiyaretciler(firmaId, ad));
        }

        public void InitGuncelleme(int firmaId, DateTime now)
        {
            _mode = EMode.Guncelle;
            lblHeader.Text = "Verilen Araç Kartını Güncelle";
            _session.AktifFirmaId = firmaId;

            gecmisZiyaretciPanel.Visible = false;

            var aktifler = _svc.GetTodayActiveAssignments(now, firmaId);
            cmbPuantajsizKartlar.DataSource = aktifler;
            cmbPuantajsizKartlar.DisplayMember = "KartAdi";
            cmbPuantajsizKartlar.ValueMember = "KartId";
            dtpCikisSaati.Enabled = true;

            if (cmbPuantajsizKartlar.Items.Count > 0)
                cmbPuantajsizKartlar.SelectedIndex = 0;

            btnKaydet.Enabled = cmbPuantajsizKartlar.Items.Count > 0;
        }
    }
}
