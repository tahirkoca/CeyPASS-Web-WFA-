using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace CeyPASS.WFA.UserControls.EO
{
    public partial class ucKisiHareketler : UserControl
    {
        private readonly ISessionContext _session;
        private readonly IKisiHareketService _khsvc;
        private readonly IKisiQueryService _kqsvc;
        private readonly IAuthorizationService _auth;
        private readonly IFirmaService _firmaSvc;
        private readonly IKullaniciFirmaIsyeriYetkiService _yetkiSvc;
        private readonly IKisiEkraniLookUpService _iklsvc;
        private List<FirmaIsyeriYetkiDTO> _kullaniciYetkileri = new();
        private bool _isAdmin;
        AuthorizationHelper authHelp;
        private const string PageName = "KisiHareketler";
        private const string PageNameUI = "Kişi Hareketleri";
        private readonly WinFormsFieldErrors _fieldErrors;
        private static readonly Color SilButtonColor = Color.FromArgb(220, 53, 69);
        private static readonly Color AktifEtButtonColor = Color.FromArgb(40, 167, 69);
        private int SelectedFirmaId
        {
            get
            {
                if (cmbFirma.SelectedValue != null && cmbFirma.SelectedValue is int)
                    return (int)cmbFirma.SelectedValue;
                return (int)_session.AktifFirmaId;
            }
        }
        private bool PuantajYapilanlarSecili => cmbKartTipi.SelectedIndex != 1;

        public ucKisiHareketler(ISessionContext session, IKisiHareketService khsvc, IKisiQueryService kqsvc, IAuthorizationService auth, IFirmaService firmaSvc, IKullaniciFirmaIsyeriYetkiService yetkiSvc, IKisiEkraniLookUpService iklsvc)
        {
            InitializeComponent();
            _fieldErrors = new WinFormsFieldErrors(this);
            _session = session;
            _khsvc = khsvc;
            _kqsvc = kqsvc;
            _auth = auth;
            _firmaSvc = firmaSvc;
            _yetkiSvc = yetkiSvc;
            _iklsvc = iklsvc;

            authHelp = new AuthorizationHelper(_session, _auth);
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);

            btnHareketleriGetir.Tag = YetkiTipleri.View;
            btnHareketEkle.Tag = YetkiTipleri.Create;
            btnHareketGuncelle.Tag = YetkiTipleri.Update;
            btnHareketSil.Tag = YetkiTipleri.Delete;

            InitDatePickers();

            dgKisiHareketler.AutoGenerateColumns = true;
            dgKisiHareketler.ReadOnly = true;
            dgKisiHareketler.RowHeadersVisible = false;
            dgKisiHareketler.AllowUserToAddRows = false;
            dgKisiHareketler.AllowUserToDeleteRows = false;
            dgKisiHareketler.MultiSelect = false;
            dgKisiHareketler.CellFormatting += DgKisiHareketler_CellFormatting;

            btnHareketleriGetir.Click += (s, e) => LoadGrid();
            btnHareketEkle.Click += (s, e) => AddForCheckedPersons();
            btnHareketGuncelle.Click += (s, e) => UpdateSelected();
            btnHareketSil.Click += (s, e) => SoftDeleteOrActivateSelected();
            chkKisiler.KeyDown += chkKisiler_KeyDown;
            cmbKartTipi.SelectedIndexChanged += KartTipiPersistHandler;
            cmbIsyeriFilter.SelectedIndexChanged += cmbIsyeriFilter_SelectedIndexChanged;
            chbAktifHareketler.CheckedChanged += (s, e) => ApplySilButtonUi();
            chbPasifHareketler.CheckedChanged += (s, e) => ApplySilButtonUi();
            dgKisiHareketler.SelectionChanged += (s, e) => ApplySilButtonUi();
            dgKisiHareketler.CurrentCellChanged += (s, e) => ApplySilButtonUi();

            ApplySilButtonUi();

            var hareketBilgi = new ToolTip();
            hareketBilgi.SetToolTip(btnHareketleriGetir, "Seçilen personelin tüm firmalardaki hareketleri listelenir (Firma kolonu: kartın okunduğu firma).");
            hareketBilgi.SetToolTip(dgKisiHareketler, "Seçilen personelin tüm firmalardaki hareketleri listelenir.");

            if (!_auth.ViewAbility(PageName))
            {
                LogHelper.Warn("KisiHareketler", "View", "Görüntüleme yetkisi yok", detayJson: $"{{\"KullaniciId\":{_session.AktifKullaniciId}}}");
                MessageBox.Show("Kişi Hareketler ekranını görüntüleme yetkiniz yok");
                this.Visible = false;
                return;
            }
        }

        private void ucKisiHareketler_Load(object sender, EventArgs e)
        {            
            try
            {
                cmbKartTipi.Items.Clear();
                cmbKartTipi.Items.Add("Puantaj Yapılanlar");
                cmbKartTipi.Items.Add("Puantaj Yapılmayanlar");
                cmbKartTipi.DropDownStyle = ComboBoxStyle.DropDownList;
                cmbKartTipi.SelectedIndex = 0;
                _isAdmin = FirmaIsyeriYetkiHelper.IsAdmin(_session.RolId);
                if (_session.AktifKullaniciId.HasValue)
                    _kullaniciYetkileri = _yetkiSvc.GetYetkiler((int)_session.AktifKullaniciId) ?? new List<FirmaIsyeriYetkiDTO>();
                LoadFirmaComboBox();
                RestoreNonCascadePrefs();
                IsyeriFilteriniYukle(SelectedFirmaId);
                LoadPersons();
                PersistFilters();

                AppTheme.ApplyToControl(this);
                LogHelper.Info("KisiHareketler", "Open", "Ekran açıldı", detayJson: $"{{\"FirmaId\":{_session.AktifFirmaId}}}");

                dtpHareketBaslangicTarihi.Format = DateTimePickerFormat.Custom;
                dtpHareketBaslangicTarihi.CustomFormat = "dd.MM.yyyy HH:mm";
                dtpHareketBaslangicTarihi.ShowUpDown = true;

                dtpHareketBitisTarihi.Format = DateTimePickerFormat.Custom;
                dtpHareketBitisTarihi.CustomFormat = "dd.MM.yyyy HH:mm";
                dtpHareketBitisTarihi.ShowUpDown = true;

                dtpHareketBaslangicTarihi.ValueChanged += (s, e2) => PersistFilters();
                dtpHareketBitisTarihi.ValueChanged += (s, e2) => PersistFilters();
                chbAktifHareketler.CheckedChanged += (s, e2) => PersistFilters();
                chbPasifHareketler.CheckedChanged += (s, e2) => PersistFilters();
                chbYemekhaneHareketleri.CheckedChanged += (s, e2) => PersistFilters();
            }
            catch (Exception ex)
            {
                LogHelper.Error("KisiHareketler", "Open", "Ekran yüklenirken hata", ex, detayJson: $"{{\"FirmaId\":{_session.AktifFirmaId}}}");
                throw;
            }
        }

        private void PersistFilters()
        {
            var kart = cmbKartTipi.SelectedItem?.ToString() ?? "";
            if (chbYemekhaneHareketleri.Checked)
                kart += "|Y";

            PageFilterPrefsStore.Save(PageName, new PageFilterPrefs
            {
                FirmaId = SelectedFirmaId > 0 ? SelectedFirmaId : null,
                IsyeriId = GetSeciliIsyeriFilterId(),
                DateA = dtpHareketBaslangicTarihi.Value,
                DateB = dtpHareketBitisTarihi.Value,
                BoolA = chbAktifHareketler.Checked,
                BoolB = chbPasifHareketler.Checked,
                Extra = kart
            });
        }

        /// <summary>Firma/işyeri dışında tarih, aktif/pasif, kart tip tercihlerini uygular.</summary>
        private void RestoreNonCascadePrefs()
        {
            var prefs = PageFilterPrefsStore.Load(PageName);
            if (prefs == null) return;

            if (prefs.DateA.HasValue)
                dtpHareketBaslangicTarihi.Value = prefs.DateA.Value;
            if (prefs.DateB.HasValue)
                dtpHareketBitisTarihi.Value = prefs.DateB.Value;
            if (prefs.BoolA.HasValue)
                chbAktifHareketler.Checked = prefs.BoolA.Value;
            if (prefs.BoolB.HasValue)
                chbPasifHareketler.Checked = prefs.BoolB.Value;

            if (!string.IsNullOrWhiteSpace(prefs.Extra))
            {
                var extra = prefs.Extra;
                bool yemek = extra.EndsWith("|Y", StringComparison.Ordinal);
                chbYemekhaneHareketleri.Checked = yemek;
                var kart = yemek ? extra[..^2] : extra;
                for (int i = 0; i < cmbKartTipi.Items.Count; i++)
                {
                    if (string.Equals(cmbKartTipi.Items[i]?.ToString(), kart, StringComparison.OrdinalIgnoreCase))
                    {
                        cmbKartTipi.SelectedIndexChanged -= KartTipiPersistHandler;
                        cmbKartTipi.SelectedIndex = i;
                        cmbKartTipi.SelectedIndexChanged += KartTipiPersistHandler;
                        break;
                    }
                }
            }
        }

        private void KartTipiPersistHandler(object? sender, EventArgs e)
        {
            LoadPersons();
            PersistFilters();
        }

        private void LoadFirmaComboBox()
        {
            try
            {
                cmbFirma.SelectedIndexChanged -= cmbFirma_SelectedIndexChanged;

                var firmalar = FirmaIsyeriYetkiHelper.FilterFirmalar(_firmaSvc.GetAll(), _kullaniciYetkileri, _isAdmin)
                    .OrderBy(f => f.FirmaAdi)
                    .ToList();

                bool showFirmaCombo = (_isAdmin || firmalar.Count > 1) && firmalar.Any();
                lblFirma.Visible = showFirmaCombo;
                cmbFirma.Visible = showFirmaCombo;

                if (showFirmaCombo)
                {
                    cmbFirma.DataSource = firmalar;
                    cmbFirma.DisplayMember = "FirmaAdi";
                    cmbFirma.ValueMember = "FirmaId";
                    cmbFirma.Enabled = true;

                    var prefs = PageFilterPrefsStore.Load(PageName);
                    int? prefer = prefs?.FirmaId;
                    if (prefer.HasValue && firmalar.Any(f => f.FirmaId == prefer.Value))
                        cmbFirma.SelectedValue = prefer.Value;
                    else if (firmalar.Any(f => f.FirmaId == _session.AktifFirmaId))
                        cmbFirma.SelectedValue = _session.AktifFirmaId;
                    else if (firmalar.Count > 0)
                        cmbFirma.SelectedIndex = 0;
                }

                pnlFirmaFilter.Visible = true;

                cmbFirma.SelectedIndexChanged += cmbFirma_SelectedIndexChanged;
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "LoadFirmaComboBox", "Firma listesi yüklenirken hata", ex);
            }
        }
        private void cmbFirma_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbFirma.SelectedValue != null && cmbFirma.SelectedValue is int)
                {
                    IsyeriFilteriniYukle(SelectedFirmaId);
                    TemizlePersonelSecimi();
                    LoadPersons();
                    PersistFilters();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "cmbFirma_SelectedIndexChanged", "Firma değiştirilirken hata", ex);
            }
        }

        private void cmbIsyeriFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                TemizlePersonelSecimi();
                LoadPersons();
                PersistFilters();
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "cmbIsyeriFilter_SelectedIndexChanged", "İşyeri değiştirilirken hata", ex);
            }
        }

        private void IsyeriFilteriniYukle(int firmaId)
        {
            cmbIsyeriFilter.SelectedIndexChanged -= cmbIsyeriFilter_SelectedIndexChanged;
            try
            {
                var list = _iklsvc.GetIsyerleri(firmaId) ?? new List<LookupItem>();
                list = FirmaIsyeriYetkiHelper.FilterIsyeriLookup(list, firmaId, _kullaniciYetkileri, _isAdmin);
                var data = new List<LookupItem> { new LookupItem { Id = 0, Ad = "Tümü" } };
                data.AddRange(list);

                cmbIsyeriFilter.DropDownStyle = ComboBoxStyle.DropDownList;
                cmbIsyeriFilter.DataSource = null;
                cmbIsyeriFilter.DisplayMember = nameof(LookupItem.Ad);
                cmbIsyeriFilter.ValueMember = nameof(LookupItem.Id);
                cmbIsyeriFilter.DataSource = data;

                var prefs = PageFilterPrefsStore.Load(PageName);
                var preferredIsyeri = prefs?.IsyeriId;
                if (preferredIsyeri.HasValue && data.Any(x => x.Id == preferredIsyeri.Value))
                    cmbIsyeriFilter.SelectedValue = preferredIsyeri.Value;
                else
                    cmbIsyeriFilter.SelectedValue = 0;
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "IsyeriFilteriniYukle", "İşyeri listesi yüklenirken hata", ex);
            }
            finally
            {
                cmbIsyeriFilter.SelectedIndexChanged += cmbIsyeriFilter_SelectedIndexChanged;
            }
        }

        private int? GetSeciliIsyeriFilterId()
        {
            if (cmbIsyeriFilter?.SelectedValue == null)
                return null;

            int val;
            if (cmbIsyeriFilter.SelectedValue is int v)
                val = v;
            else if (!int.TryParse(cmbIsyeriFilter.SelectedValue.ToString(), out val))
                return null;

            return val <= 0 ? (int?)null : val;
        }

        private void TemizlePersonelSecimi()
        {
            if (chkKisiler == null) return;
            for (int i = 0; i < chkKisiler.Items.Count; i++)
                chkKisiler.SetItemChecked(i, false);
        }

        private string BosListeUyariMesaji(int? seciliIsyeriId)
        {
            if (seciliIsyeriId.HasValue && seciliIsyeriId.Value > 0)
            {
                var ad = cmbIsyeriFilter?.Text?.Trim();
                return string.IsNullOrEmpty(ad)
                    ? "Seçili işyerde personel bulunamadı."
                    : $"\"{ad}\" işyerinde personel bulunamadı.";
            }

            return "Seçili filtreye uygun personel bulunamadı.";
        }

        private void LoadPersons()
        {
            if (chkKisiler == null) return;

            using (CeypassBusyPanel.BusyScope(this, "Personeller yükleniyor"))
            {
                bool puantajYapilir = PuantajYapilanlarSecili;
                var seciliIsyeri = GetSeciliIsyeriFilterId();
                var (isyeriId, isyeriIdIn) = FirmaIsyeriYetkiHelper.ResolveKisiQueryIsyeriFilter(
                    SelectedFirmaId, seciliIsyeri, _kullaniciYetkileri, _isAdmin);

                var data = _kqsvc.GetAktifKisilerByFirma(SelectedFirmaId, null, puantajYapilir, isyeriId, isyeriIdIn)
                    ?? new List<KisiListItem>();

                var list = new List<LookupItem>();
                foreach (var k in data)
                {
                    if (string.IsNullOrWhiteSpace(k.PersonelId) || string.IsNullOrWhiteSpace(k.AdSoyad))
                        continue;
                    if (!int.TryParse(k.PersonelId, out int id) || id <= 0)
                        continue;
                    list.Add(new LookupItem { Id = id, Ad = k.AdSoyad });
                }

                chkKisiler.BeginUpdate();
                try
                {
                    chkKisiler.DataSource = null;
                    chkKisiler.Items.Clear();
                    chkKisiler.DisplayMember = nameof(LookupItem.Ad);

                    foreach (var li in list)
                        chkKisiler.Items.Add(li);

                    if (chkKisiler.Items.Count > 0)
                        chkKisiler.SelectedIndex = 0;
                    else
                        MessageBox.Show(BosListeUyariMesaji(seciliIsyeri), "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                finally
                {
                    chkKisiler.EndUpdate();
                }

                chkKisiler.CheckOnClick = true;
                LogHelper.Info("KisiHareketler", "LoadPeople", "Kişi listesi yüklendi",
                    detayJson: $"{{\"FirmaId\":{SelectedFirmaId},\"IsyeriId\":{(seciliIsyeri.HasValue ? seciliIsyeri.Value.ToString() : "null")},\"Adet\":{chkKisiler.Items.Count}}}");
            }
        }
        private void LoadGrid()
        {
            if (!_auth.Can(PageName, YetkiTipleri.View))
            {
                LogHelper.Warn("KisiHareketler", "ActionDenied", "Grid görüntüleme yetkisi yok", detayJson: $"{{\"Yetki\":\"View\"}}");
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            var cid = Guid.NewGuid().ToString("N");

            try
            {
                using (CeypassBusyPanel.BusyScope(this, "Hareketler yükleniyor", "Lütfen bekleyin…"))
                {
                    var ids = GetCheckedPersonIds();
                    var bas = dtpHareketBaslangicTarihi.Value;
                    var bit = dtpHareketBitisTarihi.Value;

                    LogHelper.Info("KisiHareketler", "LoadGrid", "Grid sorgusu başlatıldı",
                        detayJson: $"{{\"SeciliIds\":\"{string.Join(",", ids)}\",\"Baslangic\":\"{bas:yyyy-MM-dd HH:mm}\",\"Bitis\":\"{bit:yyyy-MM-dd HH:mm}\",\"Aktif\":{(chbAktifHareketler.Checked ? 1 : 0)},\"Pasif\":{(chbPasifHareketler.Checked ? 1 : 0)},\"Yemekhane\":{(chbYemekhaneHareketleri.Checked ? 1 : 0)},\"PersonelFirmaId\":{SelectedFirmaId},\"TumFirmalarHareket\":{(ids.Count > 0).ToString().ToLower()}}}",
                        cid: cid);

                    var dt = _khsvc.GetByPersons(ids, bas, bit, chbAktifHareketler.Checked, chbPasifHareketler.Checked, chbYemekhaneHareketleri.Checked, SelectedFirmaId);

                    dgKisiHareketler.DataSource = dt;
                    BeautifyGrid(dgKisiHareketler);

                    SetCol("Tarih", "Tarih", 15, true, "dd.MM.yyyy HH:mm:ss");
                    SetCol("Firma", "Firma", 12, true);             
                    SetCol("SicilNo", "Sicil No", 10, true);       
                    SetCol("AdSoyad", "Adı Soyadı", 18, true);
                    SetCol("CihazAdi", "Turnike", 15, true);
                    SetCol("Tip", "Hareket Tipi", 10, true);
                    SetCol("KayitZamani", "Kayıt Zamanı", 15, true, "dd.MM.yyyy HH:mm:ss");
                    SetCol("AktifMi", "Aktif", 5, false);

                    Hide("Id");
                    Hide("CihazId");
                    Hide("PersonelId");
                    Hide("FirmaId");

                    void Hide(string n) { var c = dgKisiHareketler.Columns[n]; if (c != null) c.Visible = false; }

                    LogHelper.Info("KisiHareketler", "LoadGrid", "Grid yüklendi", detayJson: $"{{\"Satir\":{(dt?.Rows.Count ?? 0)}}}", cid: cid);
                    ApplySilButtonUi();
                }
                PersistFilters();
            }
            catch (Exception ex)
            {
                LogHelper.Error("KisiHareketler", "LoadGrid", "Grid yüklenirken hata", ex);
                throw;
            }
        }
        private List<int> GetCheckedPersonIds()
        {
            var ids = new List<int>();
            foreach (var it in chkKisiler.CheckedItems)
                if (it is LookupItem li) ids.Add(li.Id);
            return ids;
        }
        private void InitDatePickers()
        {
            dtpHareketBaslangicTarihi.Format = DateTimePickerFormat.Custom;
            dtpHareketBaslangicTarihi.CustomFormat = "dd.MM.yyyy HH:mm";
            dtpHareketBaslangicTarihi.ShowUpDown = true;
            dtpHareketBaslangicTarihi.Value = DateTime.Today;

            dtpHareketBitisTarihi.Format = DateTimePickerFormat.Custom;
            dtpHareketBitisTarihi.CustomFormat = "dd.MM.yyyy HH:mm";
            dtpHareketBitisTarihi.ShowUpDown = true;
            dtpHareketBitisTarihi.Value = DateTime.Today.AddDays(1).AddMinutes(-1);
        }
        private void SetCol(string name, string header, float fill, bool visible, string fmt = null)
        {
            var c = dgKisiHareketler.Columns[name];
            if (c == null) return;
            c.HeaderText = header;
            c.Visible = visible;
            c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            c.FillWeight = fill;
            if (!string.IsNullOrEmpty(fmt)) c.DefaultCellStyle.Format = fmt;
        }
        private void DgKisiHareketler_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgKisiHareketler.Rows[e.RowIndex];

            var tipObj = row.Cells["Tip"].Value;
            var aktifObj = row.Cells["AktifMi"].Value;

            string tip = tipObj == DBNull.Value ? null : Convert.ToString(tipObj);
            bool aktif = aktifObj != DBNull.Value && Convert.ToInt32(aktifObj) == 1;

            Color back;
            if (!aktif)
            {
                back = Color.FromArgb(230, 230, 230);
            }
            else if (string.Equals(tip, "Giriş", StringComparison.OrdinalIgnoreCase))
            {
                back = Color.FromArgb(187, 222, 251);
            }
            else if (string.Equals(tip, "Yemekhane", StringComparison.OrdinalIgnoreCase))
            {
                back = Color.FromArgb(200, 230, 201);
            }
            else if (string.Equals(tip, "Çıkış", StringComparison.OrdinalIgnoreCase) || string.Equals(tip, "Cikis", StringComparison.OrdinalIgnoreCase))
            {
                back = Color.FromArgb(255, 205, 210);
            }
            else
            {
                back = Color.FromArgb(255, 249, 196);
            }

            row.DefaultCellStyle.BackColor = back;
        }
        private void AddForCheckedPersons()
        {
            if (!_auth.Can(PageName, YetkiTipleri.Create))
            {
                LogHelper.Warn("KisiHareketler", "ActionDenied", "Manuel ekleme yetkisi yok", detayJson: $"{{\"Yetki\":\"Create\"}}");
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            var ids = GetCheckedPersonIds();
            if (ids.Count == 0)
            {
                LogHelper.Warn("KisiHareketler", "Create", "Kişi seçilmedi");
                _fieldErrors.Clear();
                _fieldErrors.Set(chkKisiler, "Kişi seçiniz.");
                return;
            }

            DateTime tarih;
            string tip;
            if (!ShowInput(out tarih, out tip)) return;

            int ok = 0, fail = 0;
            foreach (var pid in ids)
            {
                try
                {
                    if (_khsvc.InsertManual(SelectedFirmaId, pid, tarih, tip))
                    {
                        ok++;
                    }
                    else
                    {
                        fail++;
                    }
                }
                catch { fail++; }
            }

            LogHelper.Info("KisiHareketler", "Create", "Manuel hareket eklendi", detayJson: $"{{\"SeciliAdet\":{ids.Count},\"Basarili\":{ok},\"Hata\":{fail}}}");
            MessageBox.Show($"Başarılı Ekleme: {ok}, Hata: {fail}");
            LoadGrid();
        }
        private void UpdateSelected()
        {
            if (!_auth.Can(PageName, YetkiTipleri.Update))
            {
                LogHelper.Warn("KisiHareketler", "ActionDenied", "Güncelleme yetkisi yok", detayJson: $"{{\"Yetki\":\"Update\"}}");
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            if (dgKisiHareketler.CurrentRow == null) return;
            var drv = dgKisiHareketler.CurrentRow.DataBoundItem as DataRowView;
            if (drv == null) return;

            int id = Convert.ToInt32(drv["Id"]);
            DateTime tarih = Convert.ToDateTime(drv["Tarih"]);
            string tip = Convert.ToString(drv["Tip"]);

            if (!ShowInput(out tarih, out tip, tarih, tip)) return;

            if (_khsvc.UpdateManual(id, tarih, tip))
            {
                LogHelper.Info("KisiHareketler", "Update", "Manuel hareket güncellendi", detayJson: $"{{\"Id\":{id},\"YeniTarih\":\"{tarih:yyyy-MM-dd HH:mm:ss}\",\"YeniTip\":\"{tip}\"}}");
                LoadGrid();
            }
            else
            {
                LogHelper.Warn("KisiHareketler", "Update", "Güncelleme başarısız", detayJson: $"{{\"Id\":{id}}}");
                MessageBox.Show("Güncelleme başarısız.");
            }
        }
        private bool IsActivateMode()
        {
            if (TryGetSelectedAktifMi(out var aktifMi))
                return !aktifMi;
            return chbPasifHareketler.Checked && !chbAktifHareketler.Checked;
        }

        private bool TryGetSelectedAktifMi(out bool aktifMi)
        {
            aktifMi = true;
            if (dgKisiHareketler.CurrentRow == null) return false;
            var drv = dgKisiHareketler.CurrentRow.DataBoundItem as DataRowView;
            if (drv?.Row?.Table.Columns.Contains("AktifMi") != true) return false;
            var v = drv["AktifMi"];
            if (v == null || v == DBNull.Value) return false;
            aktifMi = Convert.ToBoolean(v);
            return true;
        }

        private void ApplySilButtonUi()
        {
            bool activate = IsActivateMode();
            btnHareketSil.Text = activate ? "Aktif Et" : "Sil";
            btnHareketSil.BackColor = activate ? AktifEtButtonColor : SilButtonColor;
            try
            {
                btnHareketSil.Image = activate
                    ? Properties.Resources.icons8_check_mark_50
                    : Properties.Resources.icons8_minus_50;
            }
            catch
            {
                // kaynak yoksa metin/renk yeterli
            }
        }

        private void SoftDeleteOrActivateSelected()
        {
            if (!_auth.Can(PageName, YetkiTipleri.Delete))
            {
                LogHelper.Warn("KisiHareketler", "ActionDenied",
                    IsActivateMode() ? "Aktif etme yetkisi yok" : "Silme yetkisi yok",
                    detayJson: $"{{\"Yetki\":\"Delete\"}}");
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            if (dgKisiHareketler.CurrentRow == null) return;
            var drv = dgKisiHareketler.CurrentRow.DataBoundItem as DataRowView;
            if (drv == null) return;

            int id = Convert.ToInt32(drv["Id"]);
            bool activate = IsActivateMode();
            string confirm = activate ? "Kayıt tekrar aktif edilsin mi?" : "Kayıt pasif edilsin mi?";
            if (!UiConfirm.Confirm(this, confirm, "Onay", activate ? "Aktif et" : "Pasife al", "Vazgeç"))
            {
                LogHelper.Info("KisiHareketler", activate ? "Activate" : "Delete",
                    "Kullanıcı işlemi iptal etti", detayJson: $"{{\"Id\":{id}}}");
                return;
            }

            bool ok = activate ? _khsvc.AktifYap(id) : _khsvc.PasifYap(id);
            if (ok)
            {
                LogHelper.Info("KisiHareketler", activate ? "Activate" : "Delete",
                    activate ? "Kayıt tekrar aktif edildi" : "Kayıt pasife çekildi",
                    detayJson: $"{{\"Id\":{id}}}");
                LoadGrid();
                ApplySilButtonUi();
                if (!activate)
                {
                    var undoId = id;
                    UiUndo.Offer("Hareket pasife alındı.", () =>
                    {
                        if (_khsvc.AktifYap(undoId))
                        {
                            LoadGrid();
                            ApplySilButtonUi();
                            UiStatus.Set("Geri alındı.");
                        }
                        else
                            MessageBox.Show("Geri alma başarısız.");
                    });
                }
            }
            else
            {
                LogHelper.Warn("KisiHareketler", activate ? "Activate" : "Delete",
                    "İşlem başarısız", detayJson: $"{{\"Id\":{id}}}");
                MessageBox.Show("İşlem başarısız.");
            }
        }

        private bool ShowInput(out DateTime tarih, out string tip, DateTime? defTarih = null, string defTip = null)
        {
            tarih = DateTime.Now;
            tip = "Giriş";
            using (var f = new Form())
            using (var dt = new DateTimePicker())
            using (var cb = new ComboBox())
            using (var ok = new Button())
            using (var cancel = new Button())
            {
                f.Text = "Hareket Bilgisi";
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.StartPosition = FormStartPosition.CenterParent;
                f.ClientSize = new Size(300, 120);
                f.MinimizeBox = false;
                f.MaximizeBox = false;
                dt.Format = DateTimePickerFormat.Custom;
                dt.CustomFormat = "dd.MM.yyyy HH:mm:ss";
                dt.SetBounds(20, 10, 260, 24);
                dt.Value = defTarih ?? DateTime.Now;

                cb.DropDownStyle = ComboBoxStyle.DropDownList;
                cb.Items.AddRange(new object[] { "Giriş", "Çıkış", "Yemekhane" });
                cb.SelectedItem = string.IsNullOrEmpty(defTip) ? "Giriş" : defTip;
                cb.SetBounds(20, 40, 260, 24);

                ok.Text = "Tamam";
                ok.DialogResult = DialogResult.OK;
                ok.SetBounds(120, 75, 70, 26);

                cancel.Text = "Vazgeç";
                cancel.DialogResult = DialogResult.Cancel;
                cancel.SetBounds(200, 75, 80, 26);

                f.Controls.AddRange(new Control[] { dt, cb, ok, cancel });
                f.AcceptButton = ok;
                f.CancelButton = cancel;

                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    tarih = dt.Value;
                    tip = Convert.ToString(cb.SelectedItem);
                    return true;
                }
                return false;
            }
        }
        private void BeautifyGrid(DataGridView g)
        {
            if (g == null) return;

            typeof(DataGridView).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(g, true, null);

            g.BackgroundColor = Color.White;
            g.BorderStyle = BorderStyle.None;
            g.GridColor = Color.FromArgb(230, 234, 240);

            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 246, 248);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(33, 37, 41);
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10f);
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 6, 6, 6);
            g.ColumnHeadersHeight = 34;

            g.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
            g.DefaultCellStyle.ForeColor = Color.FromArgb(33, 37, 41);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(59, 130, 246);
            g.DefaultCellStyle.SelectionForeColor = Color.White;
            g.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);

            g.RowTemplate.Height = 30;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.RowHeadersVisible = false;
            g.MultiSelect = false;
            g.ReadOnly = true;

            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
        }
        private void chkKisiler_KeyDown(object sender, KeyEventArgs e)
        {
            bool harfMi = (e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z);
            bool rakamMi = (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9) ||
                           (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9);

            if (harfMi || rakamMi)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
    }
}
