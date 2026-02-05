using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using CeyPASS.Infrastructure.Helpers;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;

namespace CeyPASS.WFA.UserControls.Izinler
{
    public partial class ucIzinler : UserControl
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;
        private enum ScreenMode { List, Add, Edit }
        private bool _eventsWired = false;
        private readonly ISessionContext _session;
        private readonly IAuthorizationService _auth;
        private readonly IKisiQueryService _ksvc;
        private readonly IIzinTipService _isvc;
        private readonly IFirmaService _fsvc;
        private readonly IKisiIzinService _kisvc;
        private readonly IPuantajService _psvc;
        AuthorizationHelper authHelp;
        private ScreenMode _mode = ScreenMode.List;
        private int? _editingIzinId = null;
        private bool _saving = false;
        private HashSet<int> _firmaYetkileri = new HashSet<int>();
        private List<FirmaIsyeriYetkiDTO> _kullaniciYetkileri;
        private bool _kisilerLoaded = false;
        private bool _izinlerLoaded = false;
        private bool _izinTipleriLoaded = false;
        private const string PageName = "Izinler";
        private const string PageNameUI = "İzinler";
        private const int TUMU_INT = 0;
        private const string TUMU_STR = "ALL";

        public ucIzinler(ISessionContext session, IAuthorizationService auth, IKisiQueryService ksvc, IIzinTipService isvc, IFirmaService fsvc, IKisiIzinService kisvc, IPuantajService psvc)
        {
            InitializeComponent();
            txtAciklama.HandleCreated += (s, e) =>
            {
                try
                {
                    if (txtAciklama.IsHandleCreated)
                        SendMessage(txtAciklama.Handle, EM_SETCUEBANNER, IntPtr.Zero, "İzinler için açıklamaları buradan giriniz");
                }
                catch { }
            };
            _session = session;
            _auth = auth;
            _ksvc = ksvc;
            _isvc = isvc;
            _fsvc = fsvc;
            _kisvc = kisvc;
            _psvc = psvc;

            authHelp = new AuthorizationHelper(_session, _auth);
            if (!_auth.ViewAbility(PageName))
            {
                LogHelper.Warn("Izinler", "View", "Görüntüleme yetkisi yok", detayJson: $"{{\"KullaniciId\":{_session.AktifKullaniciId}}}");
                MessageBox.Show("İzinler sayfasını görüntüleme yetkiniz yok.");
                this.Visible = false;
                return;
            }

            btnIzinleriGoster.Tag = YetkiTipleri.View;
            btnIzinEkle.Tag = YetkiTipleri.Create;
            btnIzinGuncelle.Tag = YetkiTipleri.Update;
            btnIzinSil.Tag = YetkiTipleri.Delete;
            btnKaydet.Tag = YetkiTipleri.Create;

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
            UpdateToolbarState();
            InitTimePickers();
            WireEvents();
            //LoadCombos();
            ExitEditMode();
            BeautifyGrid(dgIzinlerTablosu);
            InitAuthorizationAndCombos();
            AppTheme.ApplyToControl(this);
            LogHelper.Info(PageName, "Open", "İzinler ekranı açıldı", detayJson: $"{{\"FirmaId\":{_session.AktifFirmaId},\"KullaniciId\":{_session.AktifKullaniciId}}}");
        }

        private void InitAuthorizationAndCombos()
        {
            if (!_auth.ViewAbility(PageName))
            {
                LogHelper.Warn(PageName, "View", "Görüntüleme yetkisi yok",
                    detayJson: $"{{\"KullaniciId\":{_session.AktifKullaniciId}}}");
                MessageBox.Show("İzinler ekranını görüntüleme yetkiniz yok");
                this.Visible = false;
                return;
            }

            _kullaniciYetkileri = _psvc.GetKullaniciFirmaIsyeriYetkileri((int)_session.AktifKullaniciId);
            _firmaYetkileri = _kullaniciYetkileri
                .Select(y => y.FirmaId)
                .Distinct()
                .ToHashSet();

            LoadAuthorizedFirms();
            cmbKisilerSecimi.Enter += cmbKisiListesi_Enter;
            cmbIzinlerSecimi.Enter += cmbIzinTipleri_Enter;
            cmbFirmalarSecimi.SelectedValueChanged += (s, e) =>
            {
                ClearCombo(cmbKisilerSecimi);
                _kisilerLoaded = false;

                ClearCombo(cmbIzinlerSecimi);
                _izinTipleriLoaded = false;
            };

            ClearCombo(cmbKisilerSecimi);
            ClearCombo(cmbIzinlerSecimi);
        }
        private void ClearCombo(ComboBox cb)
        {
            cb.DataSource = null;
            cb.Items.Clear();
            cb.Text = "";
            cb.SelectedIndex = -1;
        }
        private void LoadAuthorizedFirms()
        {
            try
            {
                var list = _fsvc.GetPuantajFirmalar();
                if (_firmaYetkileri.Count > 0)
                    list = list.Where(f => _firmaYetkileri.Contains(f.FirmaId)).ToList();

                if (_firmaYetkileri.Count == 0)
                {
                    list.Insert(0, new Firma { FirmaId = TUMU_INT, FirmaAdi = "— TÜMÜ —" });
                }

                cmbFirmalarSecimi.DisplayMember = "FirmaAdi";
                cmbFirmalarSecimi.ValueMember = "FirmaId";
                cmbFirmalarSecimi.DropDownStyle = ComboBoxStyle.DropDownList;
                cmbFirmalarSecimi.DataSource = list;

                cmbFirmalarSecimi.SelectedIndex = list.Count > 0 ? 0 : -1;

                LogHelper.Info(PageName, "FirmalarLoad", "Firmalar yüklendi", detayJson: $"{{\"adet\":{list.Count}}}");
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "FirmalarLoad", "Hata", ex);
                MessageBox.Show("Firmalar yüklenemedi: " + ex.Message);
            }
        }
        private void cmbKisiListesi_Enter(object sender, EventArgs e)
        {
            if (_kisilerLoaded) return;
            if (cmbFirmalarSecimi.SelectedValue == null ||
                !int.TryParse(cmbFirmalarSecimi.SelectedValue.ToString(), out var firmaId))
                return;
            try
            {
                var kisiler = _ksvc.GetAktifKisilerByFirma(firmaId);
                kisiler.Insert(0, new KisiListItem { PersonelId = TUMU_STR, AdSoyad = "— TÜMÜ —" });
                cmbKisilerSecimi.DisplayMember = "AdSoyad";
                cmbKisilerSecimi.ValueMember = "PersonelId";
                cmbKisilerSecimi.DataSource = kisiler;
                cmbKisilerSecimi.DropDownWidth = Math.Min(Math.Max(cmbKisilerSecimi.Width, 320), 800);
                cmbKisilerSecimi.MaxDropDownItems = 16;
                _kisilerLoaded = true;
                LogHelper.Info(PageName, "KisilerLoad", "Kişiler yüklendi", detayJson: $"{{\"firmaId\":{firmaId},\"adet\":{kisiler?.Count ?? 0}}}");
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "KisilerLoad", "Hata", ex);
                MessageBox.Show("Kişi listesi yüklenemedi: " + ex.Message);
            }
        }
        private void cmbIzinTipleri_Enter(object sender, EventArgs e)
        {
            if (_izinTipleriLoaded) return;
            if (cmbFirmalarSecimi.SelectedValue == null ||
                !int.TryParse(cmbFirmalarSecimi.SelectedValue.ToString(), out _))
                return;
            try
            {
                var tipler = _isvc.GetAktif();
                tipler.Insert(0, new IzinTip { IzinTipId = TUMU_INT, Ad = "— TÜMÜ —" });
                cmbIzinlerSecimi.DisplayMember = "Ad";
                cmbIzinlerSecimi.ValueMember = "IzinTipId";
                cmbIzinlerSecimi.DataSource = tipler;
                cmbIzinlerSecimi.DropDownWidth = Math.Min(Math.Max(cmbIzinlerSecimi.Width, 320), 800);
                cmbIzinlerSecimi.MaxDropDownItems = 16;
                _izinTipleriLoaded = true;
                LogHelper.Info(PageName, "IzinTipleriLoad", "İzin tipleri yüklendi", detayJson: $"{{\"adet\":{tipler?.Count ?? 0}}}");
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "IzinTipleriLoad", "Hata", ex);
                MessageBox.Show("İzin tipleri yüklenemedi: " + ex.Message);
            }
        }
        private void InitTimePickers()
        {
            dtpIzinBaslangicSaati.Format = DateTimePickerFormat.Custom;
            dtpIzinBaslangicSaati.CustomFormat = "HH:mm";
            dtpIzinBaslangicSaati.ShowUpDown = true;

            dtpIzinBitisSaati.Format = DateTimePickerFormat.Custom;
            dtpIzinBitisSaati.CustomFormat = "HH:mm";
            dtpIzinBitisSaati.ShowUpDown = true;

            dtpIzinBaslangicTarihi.Value = DateTime.Today;
            dtpIzinBitisTarihi.Value = DateTime.Today;
            dtpIzinBaslangicSaati.Value = DateTime.Today.AddHours(9);
            dtpIzinBitisSaati.Value = DateTime.Today.AddHours(18);
        }
        private void WireEvents()
        {
            if (_eventsWired) return;

            btnIzinEkle.Click -= btnIzinEkle_Click;
            btnIzinGuncelle.Click -= btnIzinGuncelle_Click;
            btnIzinSil.Click -= btnIzinSil_Click;
            btnIzinleriGoster.Click -= btnIzinleriGoster_Click;

            btnKaydet.Click -= btnKaydet_Click;
            btnVazgec.Click -= btnVazgec_Click;

            chkSaatlikIzinMi.CheckedChanged -= chkSaatlikIzinMi_CheckedChanged;

            dgIzinlerTablosu.DataBindingComplete -= dgIzinlerTablosu_DataBindingComplete;
            dgIzinlerTablosu.DataError -= dgIzinlerTablosu_DataError;

            btnIzinEkle.Click += btnIzinEkle_Click;
            btnIzinGuncelle.Click += btnIzinGuncelle_Click;
            btnIzinSil.Click += btnIzinSil_Click;
            btnIzinleriGoster.Click += btnIzinleriGoster_Click;

            btnKaydet.Click += btnKaydet_Click;
            btnVazgec.Click += btnVazgec_Click;

            chkSaatlikIzinMi.CheckedChanged += chkSaatlikIzinMi_CheckedChanged;

            dgIzinlerTablosu.DataBindingComplete += dgIzinlerTablosu_DataBindingComplete;
            dgIzinlerTablosu.DataError += dgIzinlerTablosu_DataError;

            _eventsWired = true;
        }
        private void EnterEditMode(ScreenMode mode, int? editId = null)
        {
            _mode = mode;
            _editingIzinId = editId;

            btnKaydet.Visible = true;
            btnVazgec.Visible = true;

            btnIzinEkle.Enabled = false;
            btnIzinGuncelle.Enabled = false;
            btnIzinSil.Enabled = false;
            btnIzinleriGoster.Enabled = false;

            if (mode == ScreenMode.Edit && editId.HasValue)
                LoadSelectedRowToInputs(editId.Value);
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);

        }
        private void ExitEditMode()
        {
            _mode = ScreenMode.List;
            _editingIzinId = null;

            btnKaydet.Visible = false;
            btnVazgec.Visible = false;

            btnIzinEkle.Enabled = true;
            btnIzinGuncelle.Enabled = dgIzinlerTablosu.CurrentRow != null;
            btnIzinSil.Enabled = dgIzinlerTablosu.CurrentRow != null;
            btnIzinleriGoster.Enabled = true;
            txtAciklama.Clear();
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }
        private void LoadSelectedRowToInputs(int kisiIzinId)
        {
            if (dgIzinlerTablosu.CurrentRow == null) return;
            if (dgIzinlerTablosu.Columns.Contains("KisiIzinId"))
                dgIzinlerTablosu.Columns["KisiIzinId"].Visible = false;

            var row = dgIzinlerTablosu.CurrentRow;

            DateTime bas = Convert.ToDateTime(row.Cells["Başlangıç Tarihi"].Value);
            DateTime bit = Convert.ToDateTime(row.Cells["Bitiş Tarihi"].Value);

            dtpIzinBaslangicTarihi.Value = bas.Date;
            dtpIzinBaslangicSaati.Value = DateTime.Today + bas.TimeOfDay;

            dtpIzinBitisTarihi.Value = bit.Date;
            dtpIzinBitisSaati.Value = DateTime.Today + bit.TimeOfDay;

            txtAciklama.Text = Convert.ToString(row.Cells["Açıklama"].Value);

            var satIzinText = Convert.ToString(row.Cells["Saatlik İzin Mi"].Value);
            chkSaatlikIzinMi.Checked = satIzinText == "EVET";

            if (!_kisilerLoaded)
                cmbKisiListesi_Enter(cmbKisilerSecimi, EventArgs.Empty);

            DataGridViewCell pidCell = null;
            if (dgIzinlerTablosu.Columns.Contains("PersonelId"))
                pidCell = row.Cells["PersonelId"];
            else if (dgIzinlerTablosu.Columns.Contains("SicilNo"))
                pidCell = row.Cells["SicilNo"];

            if (pidCell != null && pidCell.Value != null && pidCell.Value != DBNull.Value)
            {
                var pid = pidCell.Value.ToString();
                if (!string.IsNullOrWhiteSpace(pid))
                    cmbKisilerSecimi.SelectedValue = pid;
            }

            if (!_izinTipleriLoaded)
                cmbIzinTipleri_Enter(cmbIzinlerSecimi, EventArgs.Empty);

            if (dgIzinlerTablosu.Columns.Contains("IzinId"))
            {
                var izinCell = row.Cells["IzinId"];
                if (izinCell != null && izinCell.Value != null && izinCell.Value != DBNull.Value)
                {
                    int izinId;
                    if (int.TryParse(izinCell.Value.ToString(), out izinId))
                        cmbIzinlerSecimi.SelectedValue = izinId;
                }
            }
        }
        private void ApplySaatlikIzinRule()
        {
            if (cmbIzinlerSecimi.DataSource == null) return;

            bool saatlik = chkSaatlikIzinMi.Checked;

            dtpIzinBaslangicSaati.Enabled = saatlik;
            dtpIzinBitisSaati.Enabled = saatlik;

            if (saatlik)
            {
                var saatlikTipId = _isvc.GetSaatlikIzinTipId();
                if (saatlikTipId.HasValue)
                {
                    cmbIzinlerSecimi.SelectedValue = saatlikTipId.Value;
                    cmbIzinlerSecimi.Enabled = false;
                }
            }
            else
            {
                cmbIzinlerSecimi.Enabled = true;
            }
        }
        private void chkSaatlikIzinMi_CheckedChanged(object sender, EventArgs e) => ApplySaatlikIzinRule();
        private void btnIzinleriGoster_Click(object sender, EventArgs e)
        {
            if (!_auth.Can(PageName, YetkiTipleri.View))
            {
                LogHelper.Warn(PageName, "ActionDenied", "İzinleri görüntüleme yetkisi yok", detayJson: $"{{\"Yetki\":\"View\"}}");
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            DateTime bas = dtpIzinBaslangicTarihi.Value.Date;
            DateTime bit = dtpIzinBitisTarihi.Value.Date.AddDays(1).AddSeconds(-1);

            int? firmaId = null;
            if (cmbFirmalarSecimi.SelectedValue != null &&
                int.TryParse(cmbFirmalarSecimi.SelectedValue.ToString(), out var fId) &&
                fId != TUMU_INT)
                firmaId = fId;

            string personelId = null;
            if (cmbKisilerSecimi.SelectedValue != null &&
                !string.Equals(cmbKisilerSecimi.SelectedValue.ToString(), TUMU_STR, StringComparison.OrdinalIgnoreCase))
                personelId = cmbKisilerSecimi.SelectedValue.ToString();

            int? izinTipId = null;
            if (cmbIzinlerSecimi.SelectedValue != null &&
                int.TryParse(cmbIzinlerSecimi.SelectedValue.ToString(), out var itId) &&
                itId != TUMU_INT)
                izinTipId = itId;

            try
            {
                var dt = _kisvc.GetTumIzinler(firmaId, personelId, izinTipId, bas, bit);
                dgIzinlerTablosu.DataSource = dt;
                UpdateToolbarState();

                if (dgIzinlerTablosu.Columns.Contains("KisiIzinId"))
                    dgIzinlerTablosu.Columns["KisiIzinId"].Visible = false;

                LogHelper.Info(PageName, "Load", "İzin listesi yüklendi", detayJson: $"{{\"SatirSayisi\":{(dt?.Rows.Count ?? 0)}}}");
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "Load", "İzin listesi yüklenirken hata", ex);
                MessageBox.Show($"Hata: {ex.Message}");
            }
        }
        private void btnIzinEkle_Click(object sender, EventArgs e)
        {
            if (!_auth.Can(PageName, YetkiTipleri.Create))
            {
                LogHelper.Warn(PageName, "ActionDenied", "İzin ekleme yetkisi yok", detayJson: $"{{\"Yetki\":\"Create\"}}");
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            LogHelper.Info(PageName, "Create", "Yeni izin girişi başlatıldı");
            txtAciklama.Clear();
            dtpIzinBaslangicTarihi.Value = DateTime.Today;
            dtpIzinBitisTarihi.Value = DateTime.Today;
            dtpIzinBaslangicSaati.Value = DateTime.Today.AddHours(9);
            dtpIzinBitisSaati.Value = DateTime.Today.AddHours(18);
            chkSaatlikIzinMi.Checked = false;
            EnterEditMode(ScreenMode.Add);
        }
        private void btnIzinSil_Click(object sender, EventArgs e)
        {
            if (!_auth.Can(PageName, YetkiTipleri.Delete))
            {
                LogHelper.Warn(PageName, "ActionDenied", "İzin silme yetkisi yok", detayJson: $"{{\"Yetki\":\"Delete\"}}");
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            if (dgIzinlerTablosu.CurrentRow == null)
            {
                LogHelper.Warn(PageName, "Delete", "Silme işlemi için satır seçilmemiş");
                return;
            }

            try
            {
                int id;

                if (dgIzinlerTablosu.CurrentRow.DataBoundItem is DataRowView drv &&
                    drv.Row.Table.Columns.Contains("KisiIzinId"))
                {
                    id = Convert.ToInt32(drv["KisiIzinId"]);
                }
                else
                {
                    var col = dgIzinlerTablosu.Columns
                        .Cast<DataGridViewColumn>()
                        .FirstOrDefault(c =>
                            string.Equals(c.DataPropertyName, "KisiIzinId", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(c.Name, "KisiIzinId", StringComparison.OrdinalIgnoreCase));

                    if (col == null)
                    {
                        MessageBox.Show("Bu listede 'KisiIzinId' kolonu bulunamadı. Lütfen liste sorgusunun KisiIzinId alanını içerdiğinden emin olun.",
                                        "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    id = Convert.ToInt32(dgIzinlerTablosu.CurrentRow.Cells[col.Index].Value);
                }

                if (MessageBox.Show("Seçili izin silinsin mi?", "Onay",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    if (_kisvc.PasifYap(id))
                    {
                        LogHelper.Info(PageName, "Delete", "İzin pasife çekildi", detayJson: $"{{\"IzinId\":{id}}}");
                        ListeyiYenile();
                    }
                    else
                    {
                        LogHelper.Warn(PageName, "Delete", "Pasife çekme başarısız", detayJson: $"{{\"IzinId\":{id}}}");
                        MessageBox.Show("İşlem başarısız. [btnIzinSil]", "Hata 2");
                    }
                }
                else
                {
                    LogHelper.Info(PageName, "Delete", "Kullanıcı silme işlemini iptal etti");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "Delete", "İzin silme sırasında hata", ex);
                MessageBox.Show($"Hata: {ex.Message}", "Hata 3");
            }
        }
        private void btnIzinGuncelle_Click(object sender, EventArgs e)
        {
            if (!_auth.Can(PageName, YetkiTipleri.Update)) { System.Media.SystemSounds.Beep.Play(); return; }

            if (dgIzinlerTablosu.CurrentRow == null) return;

            int id = Convert.ToInt32(dgIzinlerTablosu.CurrentRow.Cells["KisiIzinId"].Value);
            EnterEditMode(ScreenMode.Edit, id);
        }
        private bool ValidateInputs(out string msg)
        {
            msg = "";

            string personelId = null;
            if (cmbKisilerSecimi.SelectedValue != null &&
                !string.Equals(cmbKisilerSecimi.SelectedValue.ToString(),
                               TUMU_STR,
                               StringComparison.OrdinalIgnoreCase))
            {
                personelId = cmbKisilerSecimi.SelectedValue.ToString();
            }

            if (personelId == null &&
                _mode == ScreenMode.Edit &&
                dgIzinlerTablosu.CurrentRow != null)
            {
                var row = dgIzinlerTablosu.CurrentRow;

                string[] kolonAdlari = { "PersonelId", "SicilNo", "Sicil No" };

                foreach (var colName in kolonAdlari)
                {
                    if (dgIzinlerTablosu.Columns.Contains(colName))
                    {
                        var val = row.Cells[colName].Value;
                        if (val != null && val != DBNull.Value)
                        {
                            personelId = val.ToString();
                            break;
                        }
                    }
                }
            }

            int? izinTipId = null;
            if (cmbIzinlerSecimi.SelectedValue != null &&
                int.TryParse(cmbIzinlerSecimi.SelectedValue.ToString(), out var itId) &&
                itId != TUMU_INT)
            {
                izinTipId = itId;
            }

            bool saatlik = chkSaatlikIzinMi.Checked;

            var dto = new IzinKayitValidasyonDTO
            {
                SaatlikIzinMi = saatlik,
                PersonelId = personelId,
                IzinTipId = izinTipId,
                BaslangicTarihi = dtpIzinBaslangicTarihi.Value,
                BitisTarihi = dtpIzinBitisTarihi.Value,
                BaslangicSaati = saatlik ? dtpIzinBaslangicSaati.Value.TimeOfDay : (TimeSpan?)null,
                BitisSaati = saatlik ? dtpIzinBitisSaati.Value.TimeOfDay : (TimeSpan?)null
            };

            var result = _kisvc.ValidateKayit(dto);
            msg = result.Message;
            return result.IsValid;
        }
        private void cmbKisilerSecimi_SelectedIndexChanged(object sender, EventArgs e) { }
        private void btnKaydet_Click(object sender, EventArgs e)
        {
            if (_saving) return;
            _saving = true;

            try
            {
                if (_mode == ScreenMode.Add)
                {
                    if (!_auth.Can(PageName, YetkiTipleri.Create))
                    {
                        LogHelper.Warn(PageName, "ActionDenied", "İzin ekleme yetkisi yok");
                        System.Media.SystemSounds.Beep.Play();
                        return;
                    }
                }
                else if (_mode == ScreenMode.Edit)
                {
                    if (!_auth.Can(PageName, YetkiTipleri.Update))
                    {
                        LogHelper.Warn(PageName, "ActionDenied", "İzin güncelleme yetkisi yok");
                        System.Media.SystemSounds.Beep.Play();
                        return;
                    }
                }

                if (!ValidateInputs(out var m))
                {
                    MessageBox.Show(m);
                    return;
                }

                int selectedFirmaId = (int)_session.AktifFirmaId;
                if (cmbFirmalarSecimi.SelectedValue != null &&
                    int.TryParse(cmbFirmalarSecimi.SelectedValue.ToString(), out var fId) &&
                    fId != TUMU_INT)
                    selectedFirmaId = fId;

                bool saatlik = chkSaatlikIzinMi.Checked;
                string personelId = null;

                if (cmbKisilerSecimi.SelectedValue != null &&
                    !string.Equals(cmbKisilerSecimi.SelectedValue.ToString(),
                                   TUMU_STR,
                                   StringComparison.OrdinalIgnoreCase))
                {
                    personelId = cmbKisilerSecimi.SelectedValue.ToString();
                }
                else if (_mode == ScreenMode.Edit && dgIzinlerTablosu.CurrentRow != null)
                {
                    var row = dgIzinlerTablosu.CurrentRow;
                    string[] kolonAdlari = { "PersonelId", "SicilNo", "Sicil No" };

                    foreach (var colName in kolonAdlari)
                    {
                        if (dgIzinlerTablosu.Columns.Contains(colName))
                        {
                            var val = row.Cells[colName].Value;
                            if (val != null && val != DBNull.Value)
                            {
                                personelId = val.ToString();
                                break;
                            }
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(personelId))
                {
                    MessageBox.Show("Kayıt için lütfen belirli bir kişi seçiniz.");
                    return;
                }

                if (cmbIzinlerSecimi.SelectedValue == null ||
                    !int.TryParse(cmbIzinlerSecimi.SelectedValue.ToString(), out var izinTipId))
                {
                    MessageBox.Show("Lütfen bir izin tipi seçiniz.");
                    return;
                }

                string aciklama = txtAciklama.Text?.Trim() ?? "";

                DateTime bas, bit;
                if (saatlik)
                {
                    bas = dtpIzinBaslangicTarihi.Value.Date + dtpIzinBaslangicSaati.Value.TimeOfDay;
                    bit = dtpIzinBitisTarihi.Value.Date + dtpIzinBitisSaati.Value.TimeOfDay;
                }
                else
                {
                    bas = dtpIzinBaslangicTarihi.Value.Date;
                    bit = dtpIzinBitisTarihi.Value.Date;
                }

                var izin = new KisiIzin
                {
                    KisiIzinId = (_mode == ScreenMode.Edit) ? _editingIzinId : (int?)null,
                    FirmaId = selectedFirmaId,
                    PersonelId = personelId,
                    IzinId = izinTipId,
                    Baslangic = bas,
                    Bitis = bit,
                    Aciklama = aciklama,
                    SaatlikIzinMi = saatlik,
                    OlusturanKullaniciId = (int)_session.AktifKullaniciId
                };

                bool ok = (_mode == ScreenMode.Add) ? _kisvc.Ekle(izin) : _kisvc.Guncelle(izin);

                if (!ok)
                {
                    MessageBox.Show("İşlem başarısız.");
                    return;
                }
                LogHelper.Info(PageName, _mode.ToString(), "İzin kaydı başarıyla tamamlandı", detayJson: $"{{\"IzinTipId\":{izinTipId},\"Saatlik\":{saatlik.ToString().ToLower()},\"Bas\":\"{bas:yyyy-MM-dd HH:mm}\",\"Bit\":\"{bit:yyyy-MM-dd HH:mm}\"}}");
                MessageBox.Show("Kayıt tamamlandı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ExitEditMode();
                ListeyiYenile();
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "Save", "İzin kaydedilirken hata", ex);
                var inner = ex.InnerException?.Message;
                var msg = string.IsNullOrWhiteSpace(inner)
                    ? ex.Message
                    : ex.Message + Environment.NewLine + "Detay: " + inner;
                MessageBox.Show("İşlem başarısız: " + msg);
            }
            finally { _saving = false; }
        }
        private void btnVazgec_Click(object sender, EventArgs e)
        {
            ExitEditMode();
        }
        private void ListeyiYenile()
        {
            try
            {
                DateTime bas = dtpIzinBaslangicTarihi.Value.Date;
                DateTime bit = dtpIzinBitisTarihi.Value.Date.AddDays(1).AddSeconds(-1);

                int? firmaId = null;
                if (cmbFirmalarSecimi.SelectedValue != null &&
                    int.TryParse(cmbFirmalarSecimi.SelectedValue.ToString(), out var fId) &&
                    fId != TUMU_INT)
                    firmaId = fId;

                string personelId = null;
                if (cmbKisilerSecimi.SelectedValue != null)
                {
                    var pid = cmbKisilerSecimi.SelectedValue.ToString();
                    if (!string.Equals(pid, TUMU_STR, StringComparison.OrdinalIgnoreCase))
                        personelId = pid;
                }

                int? izinTipId = null;
                if (cmbIzinlerSecimi.SelectedValue != null &&
                    int.TryParse(cmbIzinlerSecimi.SelectedValue.ToString(), out var itId) &&
                    itId != TUMU_INT)
                    izinTipId = itId;

                var dt = _kisvc.GetTumIzinler(firmaId, personelId, izinTipId, bas, bit);
                dgIzinlerTablosu.DataSource = dt;

                if (dgIzinlerTablosu.Columns.Contains("KisiIzinId"))
                    dgIzinlerTablosu.Columns["KisiIzinId"].Visible = false;

                UpdateToolbarState();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Liste yenileme hatası: " + ex.Message);
            }
        }
        private void ucIzinler_Load(object sender, EventArgs e)
        {
            BeautifyGrid(dgIzinlerTablosu);
        }
        private void dgIzinlerTablosu_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            ConfigureGridColumns(dgIzinlerTablosu);
            UpdateToolbarState();
        }
        private void UpdateToolbarState()
        {
            int rowCount = dgIzinlerTablosu?.Rows?.Count ?? 0;
            bool canEdit = (rowCount > 0) && (_mode == ScreenMode.List);

            if (btnIzinGuncelle != null) btnIzinGuncelle.Enabled = canEdit;
            if (btnIzinSil != null) btnIzinSil.Enabled = canEdit;
            if (btnIzinleriGoster != null) btnIzinleriGoster.Enabled = (_mode == ScreenMode.List);
        }
        private void BeautifyGrid(DataGridView g)
        {
            if (g == null) return;

            typeof(DataGridView).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(g, true, null);

            g.BackgroundColor = Color.White;
            g.BorderStyle = BorderStyle.None;
            g.GridColor = Color.FromArgb(230, 234, 240);

            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 246, 248);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(33, 37, 41);
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10f);
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(4, 6, 4, 6);
            g.ColumnHeadersHeight = 32;

            g.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
            g.DefaultCellStyle.ForeColor = Color.FromArgb(33, 37, 41);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(59, 130, 246);
            g.DefaultCellStyle.SelectionForeColor = Color.White;
            g.DefaultCellStyle.Padding = new Padding(4, 3, 4, 3);

            g.RowTemplate.Height = 30;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.RowHeadersVisible = false;
            g.MultiSelect = false;
            g.ReadOnly = true;

            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
        }
        private void ConfigureGridColumns(DataGridView g)
        {
            if (g?.Columns == null) return;

            string[] hideCols = { "KisiIzinId", "Id", "IzinId", "PersonelGuid", "IslenmeTarihi", "GuncellemeTarihi" };
            foreach (var name in hideCols)
            {
                var c = g.Columns.Cast<DataGridViewColumn>()
                                 .FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
                if (c != null) c.Visible = false;
            }

            DataGridViewColumn Col(string key)
                => g.Columns.Cast<DataGridViewColumn>()
                    .FirstOrDefault(c =>
                        string.Equals(c.Name, key, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(c.DataPropertyName, key, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(c.HeaderText, key, StringComparison.OrdinalIgnoreCase));

            var cSicil = Col("PersonelId") ?? Col("SicilNo");
            var cAdSoyad = Col("AdSoyad");
            var cFirma = Col("FirmaAdi");
            var cIzinTipi = Col("IzinTipi") ?? Col("IzinTipAd");
            var cBas = Col("Başlangıç Tarihi") ?? Col("Baslangic") ?? Col("IzinBaslangic");
            var cBit = Col("Bitiş Tarihi") ?? Col("Bitis") ?? Col("IzinBitis");
            var cSaatlik = Col("Saatlik İzin Mi") ?? Col("SaatlikIzinMi") ?? Col("SaatlikIzin") ?? Col("Saatlik");
            var cAciklama = Col("Açıklama") ?? Col("Aciklama");
            var colSure = Col("Süre(Saat)") ?? g.Columns.Cast<DataGridViewColumn>().FirstOrDefault(c => c.HeaderText.StartsWith("Süre(Saat)", StringComparison.OrdinalIgnoreCase));
            var colGun = Col("Süre(Gün)") ?? g.Columns.Cast<DataGridViewColumn>().FirstOrDefault(c => c.HeaderText.StartsWith("Süre(Gün)", StringComparison.OrdinalIgnoreCase));

            if (cSaatlik != null && cSaatlik.GetType() != typeof(DataGridViewCheckBoxColumn))
            {
                var idx = cSaatlik.Index;
                var chk = new DataGridViewCheckBoxColumn
                {
                    Name = cSaatlik.Name,
                    DataPropertyName = cSaatlik.DataPropertyName ?? cSaatlik.Name,
                    HeaderText = "Saatlik İzin Mi",
                    TrueValue = true,
                    FalseValue = false,
                    IndeterminateValue = false,
                    ThreeState = false
                };
                g.Columns.RemoveAt(idx);
                g.Columns.Insert(idx, chk);
                cSaatlik = chk;
            }

            void Fit(DataGridViewColumn c, string header, float weight,
                     DataGridViewContentAlignment align, string format = null, bool wrap = false)
            {
                if (c == null) return;
                c.HeaderText = header;
                c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                c.FillWeight = weight;
                c.MinimumWidth = 70;
                c.DefaultCellStyle.Alignment = align;
                c.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                c.DefaultCellStyle.Format = format ?? "";
                c.DefaultCellStyle.WrapMode = wrap ? DataGridViewTriState.True : DataGridViewTriState.False;
            }

            Fit(cSicil, "Sicil No", 5f, DataGridViewContentAlignment.MiddleCenter);
            Fit(cAdSoyad, "Adı Soyadı", 18f, DataGridViewContentAlignment.MiddleLeft);
            Fit(cFirma, "Firma Adı", 8f, DataGridViewContentAlignment.MiddleLeft);
            Fit(cIzinTipi, "İzin Tipi", 8f, DataGridViewContentAlignment.MiddleLeft);
            Fit(cBas, "Başlangıç Tarihi", 13f, DataGridViewContentAlignment.MiddleCenter, "dd.MM.yyyy HH:mm");
            Fit(cBit, "Bitiş Tarihi", 13f, DataGridViewContentAlignment.MiddleCenter, "dd.MM.yyyy HH:mm");
            Fit(cSaatlik, "Saatlik İzin Mi", 5f, DataGridViewContentAlignment.MiddleCenter);
            Fit(colSure, "Süre(Saat)", 11f, DataGridViewContentAlignment.MiddleCenter);
            Fit(colGun, "Süre(Gün)", 11f, DataGridViewContentAlignment.MiddleCenter);
            Fit(cAciklama, "Açıklama", 7f, DataGridViewContentAlignment.MiddleLeft, null, wrap: true);

            var order = new[] { cSicil, cAdSoyad, cFirma, cIzinTipi, cBas, cBit, cSaatlik, cAciklama }
                        .Where(c => c != null).ToList();
            for (int i = 0; i < order.Count; i++) order[i].DisplayIndex = i;

            g.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
        }
        private void dgIzinlerTablosu_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
        }
    }
}
