using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Windows.Forms;

namespace CeyPASS.WFA.UserControls.Canlı_İzleme
{
    public partial class misafirKartAtama : UserControl
    {
        private enum EMode { Yeni, Guncelle }
        private EMode _mode;
        private readonly ISessionContext _session;
        private readonly IMisafirKartService _msvc;
        private bool _isSaving;

        public misafirKartAtama(ISessionContext session, IMisafirKartService msvc)
        {
            InitializeComponent();
            _session = session;
            _msvc = msvc;
            cmbPuantajsizKartlar.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPuantajsizKartlar.SelectedIndexChanged += cmbPuantajsizKartlar_SelectedIndexChanged;
        }

        private void misafirKartAtama_Load(object sender, EventArgs e) { }
        private void btnMisafirKaydet_Click(object sender, EventArgs e)
        {
            if (_isSaving) return;
            _isSaving = true;
            btnMisafirKaydet.Enabled = false;

            try
            {
                if (_mode == EMode.Yeni)
                {
                    if (cmbPuantajsizKartlar.SelectedValue == null)
                        throw new InvalidOperationException("Kart seçiniz.");

                    string kartId = Convert.ToString(cmbPuantajsizKartlar.SelectedValue);

                    int yeniId = _msvc.CreateAssignment(
                        firmaId: (int)_session.AktifFirmaId,
                        kartId: Convert.ToInt32(kartId),
                        misafirAdSoyad: txtMisafirAdSoyad.Text,
                        girisSaati: dtpGirisSaati.Value,
                        aciklama: txtAciklama.Text
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

                    _msvc.UpdateAssignment(
                        atamaId: a.AtamaId,
                        misafirAdSoyad: txtMisafirAdSoyad.Text,
                        girisSaati: dtpGirisSaati.Value,
                        cikisSaati: dtpCikisSaati.Enabled ? dtpCikisSaati.Value : (DateTime?)null,
                        aciklama: txtAciklama.Text
                    );

                    MessageBox.Show("Kayıt güncellendi.");
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
            txtAciklama.Text = a.Notlar ?? "";
            dtpGirisSaati.Value = a.Baslangic;
            dtpCikisSaati.Value = DateTime.Now;
        }
        public void InitYeni(int firmaId)
        {
            _mode = EMode.Yeni;
            _session.AktifFirmaId = firmaId;

            var cards = _msvc.GetCardsForNew(firmaId);

            cmbPuantajsizKartlar.DataSource = null;
            cmbPuantajsizKartlar.DisplayMember = nameof(PuantajsizKart.KartAdi);
            cmbPuantajsizKartlar.ValueMember = nameof(PuantajsizKart.KartId);
            cmbPuantajsizKartlar.DataSource = cards;
            cmbPuantajsizKartlar.DropDownStyle = ComboBoxStyle.DropDownList;

            if (cards != null && cards.Count > 0)
                cmbPuantajsizKartlar.SelectedIndex = 0;

            dtpCikisSaati.Enabled = false;
        }
        public void InitGuncelleme(int firmaId, DateTime now)
        {
            _mode = EMode.Guncelle;
            _session.AktifFirmaId = firmaId;

            var aktifler = _msvc.GetTodayActiveAssignments(now, firmaId);

            cmbPuantajsizKartlar.DataSource = aktifler;
            cmbPuantajsizKartlar.DisplayMember = "KartAdi";
            cmbPuantajsizKartlar.ValueMember = "KartId";

            dtpCikisSaati.Enabled = true;

            if (cmbPuantajsizKartlar.Items.Count > 0)
                cmbPuantajsizKartlar.SelectedIndex = 0;

            btnMisafirKaydet.Enabled = cmbPuantajsizKartlar.Items.Count > 0;
        }
    }
}
