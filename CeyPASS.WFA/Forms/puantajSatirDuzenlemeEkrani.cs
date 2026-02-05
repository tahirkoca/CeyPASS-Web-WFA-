using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CeyPASS.WFA.Forms
{
    public partial class puantajSatirDuzenlemeEkrani : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;
        private readonly PuantajGunSatirDTO _model;
        private readonly IPuantajService _svc;
        private readonly ISessionContext _session;
        private List<PuantajTipDTO> _tipler;
        private readonly int _personelId;

        public puantajSatirDuzenlemeEkrani(PuantajGunSatirDTO model, int personelId, IPuantajService svc, ISessionContext session)
        {
            InitializeComponent();
            SendMessage(txtAciklama.Handle, EM_SETCUEBANNER, 0, "Açıklama giriniz");
            _model = model;
            _personelId = personelId;
            _svc = svc;
            _session = session;
        }

        private void puantajSatirDuzenlemeEkrani_Load(object sender, EventArgs e)
        {
            ApplyTheme();
            _tipler = _svc.GetPuantajTipleri();
            cmbCalismaTipi.DisplayMember = nameof(PuantajTipDTO.AdKod);
            cmbCalismaTipi.ValueMember = nameof(PuantajTipDTO.Kod);
            cmbCalismaTipi.DataSource = _tipler;

            nudCalismaSaati.DecimalPlaces = 2;
            nudCalismaSaati.Increment = 0.25M;
            nudCalismaSaati.Minimum = 0M;
            nudCalismaSaati.Maximum = 24M;

            if (!string.IsNullOrWhiteSpace(_model.CalismaTipi))
                cmbCalismaTipi.SelectedValue = _model.CalismaTipi;

            nudCalismaSaati.Value = _model.Saat > 0 ? (decimal)_model.Saat : 0M;

            cmbCalismaTipi.SelectedValueChanged += CmbCalismaTipi_SelectedValueChanged;

            txtAciklama.Text = _model.Aciklama;
            lblTarih.Text = _model.Tarih.ToString("d MMM yyyy dddd", new CultureInfo("tr-TR"));
        }
        private void btnKaydet_Click(object sender, EventArgs e)
        {
            var tip = cmbCalismaTipi.SelectedItem as PuantajTipDTO;
            if (tip == null)
            {
                MessageBox.Show("Çalışma tipini seçin.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal saat = nudCalismaSaati.Value;

            _model.CalismaTipi = tip.Kod;
            _model.Saat = saat;
            _model.Aciklama = txtAciklama.Text?.Trim();

            int fmDakika = _svc.HesaplaFazlaMesaiDakika(_model.CalismaTipi, _model.Saat);
            _model.DuzenlenenFMDakika = fmDakika;

            _svc.DuzenleOnayla(_personelId, _model.Tarih, _model.DuzenlenenFMDakika, _model.Aciklama, _model.CalismaTipi, _model.Saat, _session.AktifKullaniciId);

            DialogResult = DialogResult.OK;
            Close();
        }
        private void btnIptal_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
        private void ApplyTheme()
        {
            pnlBackground.BackColor = AppTheme.ContentBackground;
            pnlCard.BackColor = AppTheme.CardBackground;
            btnKaydet.BackColor = AppTheme.Primary;
            btnKaydet.ForeColor = Color.White;
        }
        private void CmbCalismaTipi_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cmbCalismaTipi.SelectedItem is PuantajTipDTO tip)
            {
                if (tip.VarsayilanSaat.HasValue)
                    nudCalismaSaati.Value = tip.VarsayilanSaat.Value;
            }
        }
    }
}
