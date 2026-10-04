using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Entities.Helpers;
using System;
using System.Windows.Forms;

namespace CeyPASS.WFA.UserControls.Canlı_İzleme
{
    /// <summary>Misafir/ziyaretçi puantajsız kart atama ve güncelleme formu.</summary>
    public partial class misafirKartAtama : UserControl
    {
        private enum EMode { Yeni, Guncelle }
        private EMode _mode;
        private readonly ISessionContext _session;
        private readonly IMisafirKartService _msvc;
        private bool _isSaving;
        private string _tamTc;
        private bool _suppressTcChange;

        /// <summary>TC maskeleme ve geçmiş ziyaretçi paneli olaylarını bağlar.</summary>
        public misafirKartAtama(ISessionContext session, IMisafirKartService msvc)
        {
            InitializeComponent();
            _session = session;
            _msvc = msvc;
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
            TcKimlikHelper.ResolveTcOptionalForSave(txtTCKimlikNo.Text, _tamTc);

        private void GecmisZiyaretciPanel_ZiyaretciSecildi(GecmisZiyaretciItem item)
        {
            if (item == null) return;

            txtMisafirAdSoyad.Text = item.AdSoyad ?? "";
            ShowMaskedTc(item.TCKimlikNo);
            txtPasaportNo.Text = item.PasaportNo ?? "";
            txtZiyaretEdilenKisi.Text = item.ZiyaretEdilenKisi ?? "";
            txtAciklama.Text = item.Notlar ?? "";
            dtpGirisSaati.Value = DateTime.Now;
        }

        private void TxtTCKimlikNo_Leave(object sender, EventArgs e)
        {
            TryFillFromTc();
        }

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
                var rec = _msvc.GetMisafirBilgisiByTc(tc);
                if (rec == null) return;

                if (!string.IsNullOrEmpty(rec.MisafirAdSoyad) && string.IsNullOrWhiteSpace(txtMisafirAdSoyad.Text))
                    txtMisafirAdSoyad.Text = rec.MisafirAdSoyad;
                if (!string.IsNullOrEmpty(rec.ZiyaretEdilenKisi) && string.IsNullOrWhiteSpace(txtZiyaretEdilenKisi.Text))
                    txtZiyaretEdilenKisi.Text = rec.ZiyaretEdilenKisi;
                if (!string.IsNullOrEmpty(rec.PasaportNo) && string.IsNullOrWhiteSpace(txtPasaportNo.Text))
                    txtPasaportNo.Text = rec.PasaportNo;
                if (!string.IsNullOrEmpty(rec.Notlar) && string.IsNullOrWhiteSpace(txtAciklama.Text))
                    txtAciklama.Text = rec.Notlar;
            }
            catch
            {
            }
        }

        private void misafirKartAtama_Load(object sender, EventArgs e) { }
        private void btnMisafirKaydet_Click(object sender, EventArgs e)
        {
            if (_isSaving) return;
            _isSaving = true;
            btnMisafirKaydet.Enabled = false;

            try
            {
                var tc = ResolveTcForSave();
                var pasaport = string.IsNullOrWhiteSpace(txtPasaportNo.Text) ? null : txtPasaportNo.Text.Trim();
                var kimeGeldigi = string.IsNullOrWhiteSpace(txtZiyaretEdilenKisi.Text) ? null : txtZiyaretEdilenKisi.Text.Trim();

                if (_mode == EMode.Yeni)
                {
                    if (cmbPuantajsizKartlar.SelectedValue == null)
                        throw new InvalidOperationException("Kart seçiniz.");

                    string kartId = Convert.ToString(cmbPuantajsizKartlar.SelectedValue);
                    _msvc.CreateAssignment(
                        firmaId: (int)_session.AktifFirmaId,
                        personelId: kartId,
                        misafirAdSoyad: txtMisafirAdSoyad.Text,
                        girisSaati: dtpGirisSaati.Value,
                        aciklama: txtAciklama.Text,
                        tcKimlikNo: tc,
                        ziyaretEdilenKisi: kimeGeldigi,
                        pasaportNo: pasaport
                    );

                    MessageBox.Show("Kayıt başarıyla oluşturuldu.");
                    this.FindForm()?.Close();
                    return;
                }
                else
                {
                    var a = cmbPuantajsizKartlar.SelectedItem as PuantajsizKartAtama;
                    if (a == null)
                        throw new InvalidOperationException("Güncellenecek atamayı seçiniz.");

                    var kisitKaldirildi = _msvc.UpdateAssignment(
                        atamaId: a.AtamaId,
                        misafirAdSoyad: txtMisafirAdSoyad.Text,
                        girisSaati: dtpGirisSaati.Value,
                        cikisSaati: dtpCikisSaati.Enabled ? dtpCikisSaati.Value : (DateTime?)null,
                        aciklama: txtAciklama.Text,
                        tcKimlikNo: tc,
                        ziyaretEdilenKisi: kimeGeldigi,
                        pasaportNo: pasaport,
                        kullaniciId: _session.AktifKullaniciId
                    );

                    MessageBox.Show(KartAtamaMesajlari.Guncellendi(kisitKaldirildi));
                    this.FindForm()?.Close();
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isSaving = false;
                if (this.FindForm() != null) btnMisafirKaydet.Enabled = true;
            }
        }
        private void dtpGirisSaati_ValueChanged(object sender, EventArgs e) { }
        private void btnMisafirKayitIptal_Click(object sender, EventArgs e)
        {
            this.FindForm()?.Close();
        }
        private void cmbPuantajsizKartlar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_mode != EMode.Guncelle) return;
            var a = cmbPuantajsizKartlar.SelectedItem as PuantajsizKartAtama;
            if (a == null) return;

            txtMisafirAdSoyad.Text = a.MisafirAdSoyad ?? "";
            txtZiyaretEdilenKisi.Text = a.ZiyaretEdilenKisi ?? "";
            ShowMaskedTc(a.TCKimlikNo);
            txtPasaportNo.Text = a.PasaportNo ?? "";
            txtAciklama.Text = a.Notlar ?? "";
            dtpGirisSaati.Value = a.Baslangic;
            dtpCikisSaati.Value = DateTime.Now;
        }
        /// <summary>Boş kart listesi; grid satırından gelen preselectPersonelId ile kart seçilir.</summary>
        public void InitYeni(int firmaId, string preselectPersonelId = null)
        {
            _mode = EMode.Yeni;
            _session.AktifFirmaId = firmaId;

            var cards = _msvc.GetCardsForNew(firmaId);

            cmbPuantajsizKartlar.DataSource = null;
            cmbPuantajsizKartlar.DisplayMember = nameof(KisiListItem.AdSoyad);
            cmbPuantajsizKartlar.ValueMember = nameof(KisiListItem.PersonelId);
            cmbPuantajsizKartlar.DataSource = cards;
            cmbPuantajsizKartlar.DropDownStyle = ComboBoxStyle.DropDownList;

            if (cards != null && cards.Count > 0)
                cmbPuantajsizKartlar.SelectedIndex = 0;
            SelectKart(preselectPersonelId);

            ShowMaskedTc(null);
            txtPasaportNo.Clear();
            txtZiyaretEdilenKisi.Clear();
            dtpCikisSaati.Enabled = false;
            dtpGirisSaati.Value = DateTime.Now;

            gecmisZiyaretciPanel.Visible = true;
            gecmisZiyaretciPanel.LoadListe(ad => _msvc.SearchGecmisZiyaretciler(firmaId, ad));
        }
        /// <summary>Aktif atamalar; preselectAtamaId ile çift tıklanan satır önceden seçilir.</summary>
        public void InitGuncelleme(int firmaId, DateTime now, int? preselectAtamaId = null)
        {
            _mode = EMode.Guncelle;
            _session.AktifFirmaId = firmaId;

            gecmisZiyaretciPanel.Visible = false;

            var aktifler = _msvc.GetOpenActiveAssignments(firmaId);

            cmbPuantajsizKartlar.DataSource = aktifler;
            cmbPuantajsizKartlar.DisplayMember = "KartAdi";
            cmbPuantajsizKartlar.ValueMember = "KartId";

            dtpCikisSaati.Enabled = true;

            if (cmbPuantajsizKartlar.Items.Count > 0)
                cmbPuantajsizKartlar.SelectedIndex = 0;
            SelectAtama(preselectAtamaId);

            btnMisafirKaydet.Enabled = cmbPuantajsizKartlar.Items.Count > 0;
        }

        private void SelectKart(string personelId)
        {
            if (string.IsNullOrWhiteSpace(personelId)) return;
            for (int i = 0; i < cmbPuantajsizKartlar.Items.Count; i++)
            {
                if (cmbPuantajsizKartlar.Items[i] is KisiListItem k
                    && string.Equals(k.PersonelId, personelId.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    cmbPuantajsizKartlar.SelectedIndex = i;
                    return;
                }
            }
        }

        private void SelectAtama(int? atamaId)
        {
            if (!atamaId.HasValue) return;
            for (int i = 0; i < cmbPuantajsizKartlar.Items.Count; i++)
            {
                if (cmbPuantajsizKartlar.Items[i] is PuantajsizKartAtama a && a.AtamaId == atamaId.Value)
                {
                    cmbPuantajsizKartlar.SelectedIndex = i;
                    return;
                }
            }
        }
    }
}
