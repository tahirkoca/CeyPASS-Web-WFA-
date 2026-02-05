using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CeyPASS.WFA.UserControls
{
    public partial class ucPersonelTanimlama : UserControl
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;
        private readonly ISessionContext _session;
        private readonly IKisiService _kisiSvc;
        private readonly IKisiQueryService _kqsvc;
        private readonly IKisiEkraniLookUpService _iklsvc;
        private readonly IAuthorizationService _auth;
        private readonly ICalismaSekliService _calismaSekliSvc;
        private readonly IFirmaService _firmaSvc;
        private readonly IPuantajsizKartRepository _puantajsizKartRepo;
        AuthorizationHelper authHelp;
        private bool _istenCikisModu = false;
        private bool _isYeniKayit = false;
        private bool _guncelleModu = false;
        private bool _fotoDirty = false;
        private string _aktifFiltre = null;
        private KisiListItem _sonSecilen = null;
        private string _originalPersonelId = null;
        private bool _seciliPuantajsizKartMi = false;
        private const string PageName = "Personeller";
        private const string PageNameUI = "Personeller";

        public ucPersonelTanimlama(ISessionContext session, IKisiService kisiSvc, IKisiQueryService kisiQuerySvc, IKisiEkraniLookUpService lookups, ICalismaSekliService calismaSekliSvc, IAuthorizationService authSvc, IFirmaService firmaSvc, IPuantajsizKartRepository puantajsizKartRepo)
        {
            InitializeComponent();
            SendMessage(txtCepTel.Handle, EM_SETCUEBANNER, 0, "05.. - ... - .. - .. şeklinde giriniz");
            SendMessage(txtKimlikNo.Handle, EM_SETCUEBANNER, 0, "11 Haneli TC Kimlik Numarasını giriniz");
            SendMessage(txtAra.Handle, EM_SETCUEBANNER, 0, "Personeli buradan arayabilirsiniz");
            dtpIstenCikis.Format = DateTimePickerFormat.Custom;
            dtpIstenCikis.CustomFormat = "'Aktif Çalışıyor...'";
            dtpIstenCikis.Enabled = false;

            txtAra.TextChanged += (s, e) =>
            {
                _aktifFiltre = string.IsNullOrWhiteSpace(txtAra.Text) ? null : txtAra.Text.Trim();
                KisileriYukle(GetSeciliFirmaId());
            };

            _session = session;
            _kisiSvc = kisiSvc;
            _kqsvc = kisiQuerySvc;
            _iklsvc = lookups;
            _calismaSekliSvc = calismaSekliSvc;
            _auth = authSvc;
            _firmaSvc = firmaSvc;
            _puantajsizKartRepo = puantajsizKartRepo;
            authHelp = new AuthorizationHelper(_session, _auth);

            if (!_auth.ViewAbility(PageName))
            {
                LogHelper.Warn(PageName, "View", "Personeller ekranını görüntüleme yetkisi yok");
                MessageBox.Show("Personeller ekranını görüntüleme yetkiniz yok");
                this.Visible = false;
                return;
            }
            AppTheme.ApplyToControl(this);
            LogHelper.Info(PageName, "Open", "Personeller ekranı açıldı",
               detayJson: $"{{\"KullaniciId\":{_session.AktifKullaniciId}}}");

            btnKisiEkle.Tag = YetkiTipleri.Create;
            btnKisiGuncelle.Tag = YetkiTipleri.Update;
            btnKisiSil.Tag = YetkiTipleri.Delete;
            btnFotoEkle.Tag = YetkiTipleri.Update;
            btnFotoSil.Tag = YetkiTipleri.Update;

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);

            // --- EVENTLER --- //
            this.Load += ucPersonelTanimlama_Load;
            lstKisiler.SelectedIndexChanged += lstKisiler_SelectedIndexChanged;
            lstKisiler.DrawItem += lstKisiler_DrawItem;
            cmbFirmaFilter.SelectedIndexChanged += cmbFirmaFilter_SelectedIndexChanged;
            cmbKartTipi.SelectedIndexChanged += cmbKartTipi_SelectedIndexChanged;
            cmbIsyeriFilter.SelectedIndexChanged += cmbIsyeriFilter_SelectedIndexChanged;

            btnKisiEkle.Click += pbKisiEkle_Click;
            btnKisiSil.Click += pbKisiSil_Click;
            btnKisiGuncelle.Click += pbKisiGuncelle_Click;
            btnAra.Click += pbKisiAra_Click;
            btnKaydet.Click += dinamikButon_Click;
            btnVazgec.Click += btnVazgec_Click;

            btnFotoEkle.Click += btnFotoEkle_Click;
            btnFotoSil.Click += btnFotoSil_Click;

            chkVardiyalar.CheckOnClick = true;
            chkVardiyalar.IntegralHeight = false;
            chkVardiyalar.Height = 120;
            chkVardiyalar.Dock = DockStyle.Fill;

            chkFirmaPersoneliMi.CheckedChanged += (s, e) => UpdateUIState();
            chkYemekHakkiVarMi.CheckedChanged += (s, e) => UpdateUIState();
            chkPuantajYapilirMi.CheckedChanged += (s, e) => UpdateUIState();

            lstKisiler.DrawMode = DrawMode.OwnerDrawFixed;
            lstKisiler.ItemHeight = 30;

            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(0);
            this.Margin = new Padding(0);
        }

        private void ucPersonelTanimlama_Load(object sender, EventArgs e)
        {
            try
            {
                if (cmbKartTipi.Items.Count > 0 && cmbKartTipi.SelectedIndex < 0)
                    cmbKartTipi.SelectedIndex = 0; // Puantaj Yapılanlar
                FirmaFilteriniYukle();

                int firmaId = GetSeciliFirmaId();
                IsyeriFilteriniYukle(firmaId);
                CombosunuYukle(firmaId);
                VardiyaYukle(firmaId);
                KisileriYukle(firmaId);
                UpdateUIState();
                WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);

                LogHelper.Info(PageName, "Load", $"Ekran yüklendi. FirmaId={firmaId}");
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "Load", "Ekran yüklenirken hata.", ex);
                throw;
            }
        }
        private void FirmaFilteriniYukle()
        {
            cmbFirmaFilter.SelectedIndexChanged -= cmbFirmaFilter_SelectedIndexChanged;

            try
            {
                var tumFirmalar = _firmaSvc.GetAll();
                if (tumFirmalar == null || tumFirmalar.Count == 0)
                    return;

                bool isAdmin = _session.RolId == 1 || _session.RolId == 2;

                List<Firma> liste;
                if (isAdmin)
                {
                    liste = tumFirmalar.OrderBy(f => f.FirmaAdi).ToList();
                    cmbFirmaFilter.Enabled = true;
                }
                else
                {
                    liste = tumFirmalar
                        .Where(f => f.FirmaId == _session.AktifFirmaId)
                        .ToList();
                    cmbFirmaFilter.Enabled = false;
                }

                cmbFirmaFilter.DataSource = null;
                cmbFirmaFilter.DisplayMember = "FirmaAdi";
                cmbFirmaFilter.ValueMember = "FirmaId";
                cmbFirmaFilter.DataSource = liste;

                if (liste.Any(x => x.FirmaId == _session.AktifFirmaId))
                    cmbFirmaFilter.SelectedValue = _session.AktifFirmaId;
                else if (liste.Count > 0)
                    cmbFirmaFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Firma listesi yüklenirken hata: " + ex.Message);
            }
            finally
            {
                cmbFirmaFilter.SelectedIndexChanged += cmbFirmaFilter_SelectedIndexChanged;
            }
        }
        private int GetSeciliFirmaId()
        {
            int firmaId = (int)_session.AktifFirmaId;

            if (cmbFirmaFilter.SelectedValue != null)
            {
                if (cmbFirmaFilter.SelectedValue is int v)
                    firmaId = v;
                else if (int.TryParse(cmbFirmaFilter.SelectedValue.ToString(), out var parsed))
                    firmaId = parsed;
            }

            return firmaId;
        }
        private void cmbFirmaFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            int firmaId = GetSeciliFirmaId();
            IsyeriFilteriniYukle(firmaId);
            CombosunuYukle(firmaId);
            VardiyaYukle(firmaId);
            KisileriYukle(firmaId);
        }
        private void cmbKartTipi_SelectedIndexChanged(object sender, EventArgs e)
        {
            KisileriYukle(GetSeciliFirmaId());
        }
        private void cmbIsyeriFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            KisileriYukle(GetSeciliFirmaId());
        }
        private bool GetPuantajYapilanSecili()
        {
            if (cmbKartTipi.SelectedIndex == 1) return false; // Puantaj Yapılmayan
            return true; // Puantaj Yapılanlar (0) veya varsayılan
        }
        private void lstKisiler_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            string text;

            if (lstKisiler.Items[e.Index] is KisiListItem ki)
                text = ki.AdSoyad;
            else if (lstKisiler.Items[e.Index] is DataRowView drv)
                text = Convert.ToString(drv["Adı Soyadı"]);
            else
                text = lstKisiler.Items[e.Index].ToString();

            Color backColor = (e.State & DrawItemState.Selected) == DrawItemState.Selected
                ? Color.SteelBlue
                : Color.WhiteSmoke;

            Color textColor = (e.State & DrawItemState.Selected) == DrawItemState.Selected
                ? Color.White
                : Color.DarkSlateGray;

            e.Graphics.FillRectangle(new SolidBrush(backColor), e.Bounds);
            e.Graphics.DrawString(
                text,
                new Font("Segoe UI", 10, FontStyle.Regular),
                new SolidBrush(textColor),
                e.Bounds.Left + 5,
                e.Bounds.Top + 5);

            e.Graphics.DrawRectangle(Pens.LightGray, e.Bounds);
        }
        private void lstKisiler_SelectedIndexChanged(object sender, EventArgs e)
        {
            var cid = Guid.NewGuid().ToString("N");
            try
            {
                _isYeniKayit = false;
                _guncelleModu = false;
                _istenCikisModu = false;

                string seciliKisiId = null;

                if (lstKisiler.SelectedItem is KisiListItem ki)
                    seciliKisiId = ki.PersonelId;
                else if (lstKisiler.SelectedItem is DataRowView drv)
                    seciliKisiId = Convert.ToString(drv["PersonelId"]);

                if (!string.IsNullOrWhiteSpace(seciliKisiId))
                {
                    LogHelper.Info(PageName, "Secim", $"Kişi seçildi: {seciliKisiId}", null, cid);
                    _originalPersonelId = seciliKisiId.Trim();
                    KisiyiGetir(seciliKisiId);
                    txtPersonelKartNo.ReadOnly = false;
                    WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
                }

                ApplyCheckboxRules();
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "Secim", "Kişi seçimi sırasında hata.", ex, cid);
                throw;
            }
        }
        private void KisileriYukle(int firmaId)
        {
            var cid = Guid.NewGuid().ToString("N");
            try
            {
                _sonSecilen = lstKisiler.SelectedItem as KisiListItem;

                bool? puantajYapilirMi = GetPuantajYapilanSecili();
                int? isyeriId = GetSeciliIsyeriFilterId();
                var data = _kqsvc.GetAktifKisilerByFirma(firmaId, _aktifFiltre, puantajYapilirMi, isyeriId);

                lstKisiler.DataSource = null;
                lstKisiler.DisplayMember = nameof(KisiListItem.AdSoyad);
                lstKisiler.ValueMember = nameof(KisiListItem.PersonelId);
                lstKisiler.DataSource = data;

                if (_sonSecilen != null)
                {
                    var tekrar = data.FirstOrDefault(x => x.PersonelId == _sonSecilen.PersonelId);
                    if (tekrar != null)
                        lstKisiler.SelectedItem = tekrar;
                    else if (lstKisiler.Items.Count > 0)
                        lstKisiler.SelectedIndex = 0;
                }
                else if (lstKisiler.Items.Count > 0)
                    lstKisiler.SelectedIndex = 0;

                LogHelper.Info(PageName, "KisileriYukle", $"Kişiler yüklendi. Sayı={data?.Count}",
                    $"{{\"filter\":\"{_aktifFiltre}\",\"firmaId\":{firmaId},\"isyeriId\":{(isyeriId.HasValue ? isyeriId.Value.ToString() : "null")}}}", cid);
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "KisileriYukle", "Kişiler yüklenirken hata.", ex, cid);
                throw;
            }
        }

        private void IsyeriFilteriniYukle(int firmaId)
        {
            cmbIsyeriFilter.SelectedIndexChanged -= cmbIsyeriFilter_SelectedIndexChanged;
            try
            {
                var list = _iklsvc.GetIsyerleri(firmaId) ?? new List<LookupItem>();
                // "Tümü" seçeneği
                var data = new List<LookupItem> { new LookupItem { Id = 0, Ad = "Tümü" } };
                data.AddRange(list);

                cmbIsyeriFilter.DropDownStyle = ComboBoxStyle.DropDownList;
                cmbIsyeriFilter.DataSource = null;
                cmbIsyeriFilter.DisplayMember = nameof(LookupItem.Ad);
                cmbIsyeriFilter.ValueMember = nameof(LookupItem.Id);
                cmbIsyeriFilter.DataSource = data;
                cmbIsyeriFilter.SelectedValue = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("İşyeri listesi yüklenirken hata: " + ex.Message);
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
        private void KisiyiGetir(string kisiId)
        {
            var cid = Guid.NewGuid().ToString("N");
            try
            {
                var (d, isPuantajsizKart) = _kqsvc.GetDetayOrPuantajsizKart(kisiId);
                if (d == null)
                {
                    MessageBox.Show("Kişi bulunamadı.");
                    return;
                }

                _seciliPuantajsizKartMi = isPuantajsizKart;

                txtAdSoyad.Text = (d.Ad + " " + d.Soyad).Trim();
                txtSicilNo.Text = d.PersonelId;
                txtPersonelKartNo.Text = d.KartNo ?? "";
                txtKimlikNo.Text = d.TcKimlikNo ?? "";
                txtCepTel.Text = d.CepTel ?? "";
                txtEmail.Text = d.Email ?? "";

                if (d.IstenCikisTarihi.HasValue)
                {
                    dtpIstenCikis.Enabled = false;
                    dtpIstenCikis.Format = DateTimePickerFormat.Long;
                    dtpIstenCikis.CustomFormat = "dd MMMM yyyy dddd";
                    dtpIstenCikis.Value = d.IstenCikisTarihi.Value;
                }
                else
                {
                    dtpIstenCikis.Enabled = false;
                    dtpIstenCikis.Format = DateTimePickerFormat.Custom;
                    dtpIstenCikis.CustomFormat = "'Aktif Çalışıyor...'";
                    dtpIstenCikis.Value = DateTime.Today;
                }

                dtpDogumGunu.Checked = d.DogumTarihi.HasValue;
                if (d.DogumTarihi.HasValue) dtpDogumGunu.Value = d.DogumTarihi.Value;

                dtpIseGiris.Value = d.IseGirisTarihi ?? DateTime.Today;

                dtpIstenCikis.Checked = d.IstenCikisTarihi.HasValue;
                if (d.IstenCikisTarihi.HasValue)
                    dtpIstenCikis.Value = d.IstenCikisTarihi.Value;

                cmbPozisyon.SelectedValue = d.PozisyonId ?? -1;
                cmbDepartman.SelectedValue = d.DepartmanId ?? -1;
                cmbIsyeri.SelectedValue = d.IsyeriId ?? -1;
                cmbFirma.SelectedValue = d.FirmaId;
                cmbBolum.SelectedValue = d.BolumId ?? -1;

                if (int.TryParse(d.CalismaStatusuId?.ToString(), out var cstId))
                    cmbCalismaStatu.SelectedValue = cstId;
                else
                    cmbCalismaStatu.Text = d.CalismaStatusuText ?? "";

                VardiyalariIsaretle(d.CalismaSekliCsv ?? "");

                txtFirmaDisiKartNo.Text = d.TaseronKartNo ?? "";

                chkYemekHakkiVarMi.Checked = d.YemekHakkiVar;
                nudYemekAdedi.Value = d.GunlukYemekAdedi.HasValue ? d.GunlukYemekAdedi.Value : 0;

                chkFirmaPersoneliMi.Checked = d.FirmaPersoneli;
                chkPuantajYapilirMi.Checked = d.PuantajYapilabilir;

                pbKisiFoto.Image = DbHelpers.BytesToImage(d.Fotograf);
                _fotoDirty = false;

                ApplyCheckboxRules();
                WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
                LogHelper.Info(PageName, "KisiyiGetir", $"Kişi detay yüklendi: {kisiId} (PuantajsizKart={_seciliPuantajsizKartMi})", null, cid);
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "KisiyiGetir", $"Kişi detayında hata. KisiId={kisiId}", ex, cid);
                throw;
            }
        }
        private void CombosunuYukle(int firmId)
        {
            Action<ComboBox, List<LookupItem>> bind = (cb, data) =>
            {
                cb.DropDownStyle = ComboBoxStyle.DropDownList;
                cb.DataSource = null;
                cb.DisplayMember = nameof(LookupItem.Ad);
                cb.ValueMember = nameof(LookupItem.Id);
                cb.DataSource = data ?? new List<LookupItem>();
                if (cb.Items.Count > 0)
                    cb.SelectedIndex = 0;
            };

            bind(cmbCalismaStatu, _iklsvc.GetCalismaStatuleri());
            bind(cmbDepartman, _iklsvc.GetDepartmanlar());
            bind(cmbPozisyon, _iklsvc.GetPozisyonlar());
            bind(cmbIsyeri, _iklsvc.GetIsyerleri(firmId));
            bind(cmbFirma, _iklsvc.GetFirma(firmId));
            bind(cmbBolum, _iklsvc.GetBolumler(firmId));
        }
        private void VardiyaYukle(int firmaId)
        {
            var liste = _calismaSekliSvc.GetAll(firmaId, includeGlobal: true) ?? new List<CalismaSekli>();

            chkVardiyalar.BeginUpdate();
            try
            {
                chkVardiyalar.DataSource = null;
                chkVardiyalar.Items.Clear();
                foreach (var v in liste)
                    chkVardiyalar.Items.Add(v, false);
            }
            finally
            {
                chkVardiyalar.EndUpdate();
            }
        }
        private string SecilenVardiyaIds()
        {
            var ids = new List<int>();

            foreach (var item in chkVardiyalar.CheckedItems)
            {
                var vardiya = (CalismaSekli)item;
                ids.Add(vardiya.Id);
            }

            return string.Join(",", ids);
        }
        private void VardiyalariIsaretle(string csvIds)
        {
            var hedef = new HashSet<int>(
                csvIds.Split(',')
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(int.Parse));

            for (int i = 0; i < chkVardiyalar.Items.Count; i++)
            {
                var vardiya = (CalismaSekli)chkVardiyalar.Items[i];
                chkVardiyalar.SetItemChecked(i, hedef.Contains(vardiya.Id));
            }
        }
        private void pbKisiEkle_Click(object sender, EventArgs e)
        {
            var cid = Guid.NewGuid().ToString("N");
            try
            {
                _isYeniKayit = true;

                int firmaId = GetSeciliFirmaId();
                CombosunuYukle(firmaId);
                VardiyaYukle(firmaId);
                YeniKayitModunaGec();
                pbKisiFoto.Image = null;
                _fotoDirty = false;
                IslemButonlariniGoster(true);
                LogHelper.Info(PageName, "YeniKayitModu", "Yeni kayıt moduna geçildi.", null, cid);
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "YeniKayitModu", "Yeni kayıt moduna geçerken hata.", ex, cid);
                throw;
            }
        }
        private void YeniKayitModunaGec()
        {
            if (!_auth.Can(PageName, YetkiTipleri.Create))
            {
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            _isYeniKayit = true;
            _istenCikisModu = false;

            txtAdSoyad.Clear();
            txtSicilNo.Clear();
            txtPersonelKartNo.Clear();
            txtKimlikNo.Clear();
            txtCepTel.Clear();
            txtEmail.Clear();
            txtFirmaDisiKartNo.Clear();

            txtPersonelKartNo.ReadOnly = false;
            txtSicilNo.ReadOnly = false;

            pbKisiFoto.Image = null;

            dtpIseGiris.Value = DateTime.Today;
            dtpDogumGunu.Checked = false;
            dtpIstenCikis.Checked = false;
            dtpIstenCikis.Enabled = false;

            if (cmbFirma.Items.Count > 0) cmbFirma.SelectedIndex = 0;
            if (cmbDepartman.Items.Count > 0) cmbDepartman.SelectedIndex = 0;
            if (cmbPozisyon.Items.Count > 0) cmbPozisyon.SelectedIndex = 0;
            if (cmbIsyeri.Items.Count > 0) cmbIsyeri.SelectedIndex = 0;
            if (cmbCalismaStatu.Items.Count > 0) cmbCalismaStatu.SelectedIndex = 0;
            if (cmbBolum.Items.Count > 0) cmbBolum.SelectedIndex = 0;

            for (int i = 0; i < chkVardiyalar.Items.Count; i++)
                chkVardiyalar.SetItemChecked(i, false);

            chkFirmaPersoneliMi.Checked = true;
            chkPuantajYapilirMi.Checked = true;
            chkYemekHakkiVarMi.Checked = false;
            nudYemekAdedi.Value = 0;

            ApplyCheckboxRules();
            txtAdSoyad.Focus();

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }
        private void pbKisiSil_Click(object sender, EventArgs e)
        {
            var cid = Guid.NewGuid().ToString("N");
            try
            {
                if (!_auth.Can(PageName, YetkiTipleri.Delete))
                {
                    System.Media.SystemSounds.Beep.Play();
                    return;
                }

                if (lstKisiler.SelectedItem == null)
                {
                    LogHelper.Warn(PageName, "IstenCikisBasla", "Seçim yok.", null, cid);
                    MessageBox.Show("Lütfen listeden bir kişi seçiniz.", "Uyarı",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Puantajsız kart: tek adımda pasif et
                if (_seciliPuantajsizKartMi)
                {
                    string kartId = txtSicilNo.Text?.Trim();
                    if (string.IsNullOrWhiteSpace(kartId))
                    {
                        MessageBox.Show("Kart bilgisi alınamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    var onay = MessageBox.Show("Bu puantajsız kartı pasif etmek istediğinize emin misiniz?",
                        "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (onay != DialogResult.Yes)
                        return;
                    _puantajsizKartRepo.PasifEtByKartId(kartId);
                    KisileriYukle(GetSeciliFirmaId());
                    MessageBox.Show("Kart pasif edildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LogHelper.Info(PageName, "PuantajsizKartPasif", $"Kart pasif edildi: {kartId}", null, cid);
                    return;
                }

                _istenCikisModu = true;
                dtpIstenCikis.ShowCheckBox = true;
                dtpIstenCikis.Format = DateTimePickerFormat.Short;
                dtpIstenCikis.CustomFormat = "dd.MM.yyyy";
                dtpIstenCikis.Enabled = true;
                dtpIstenCikis.Checked = true;
                if (dtpIstenCikis.Value.Year < 2000)
                    dtpIstenCikis.Value = DateTime.Today;

                ApplyCheckboxRules();
                dtpIstenCikis.Focus();
                IslemButonlariniGoster(true);
                MessageBox.Show("Lütfen işten çıkış tarihini seçin ve 'Kaydet' butonuna basın.",
                    "Tarih Seçimi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
                LogHelper.Info(PageName, "IstenCikisBasla", "İşten çıkış süreci başlatıldı.", null, cid);
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "IstenCikisBasla", "İşten çıkış moduna geçerken hata.", ex, cid);
                throw;
            }
        }
        private void pbKisiGuncelle_Click(object sender, EventArgs e)
        {
            var cid = Guid.NewGuid().ToString("N");
            try
            {
                if (!_auth.Can(PageName, YetkiTipleri.Update))
                {
                    System.Media.SystemSounds.Beep.Play();
                    return;
                }

                if (lstKisiler.SelectedItem == null)
                {
                    MessageBox.Show("Güncellemek için listeden bir kişi seçiniz.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LogHelper.Warn(PageName, "GuncelleBasla", "Seçim yok.", null, cid);
                    return;
                }

                _isYeniKayit = false;
                _guncelleModu = true;
                _istenCikisModu = false;
                _originalPersonelId = txtSicilNo.Text.Trim();
                MessageBox.Show("Bilgileri düzenleyin ve 'Kaydet' butonuna basın.",
                    "Güncelleme Modu", MessageBoxButtons.OK, MessageBoxIcon.Information);
                IslemButonlariniGoster(true);
                ApplyCheckboxRules();
                WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);

                LogHelper.Info(PageName, "GuncelleBasla", "Güncelleme moduna geçildi.", null, cid);
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "GuncelleBasla", "Güncelleme moduna geçerken hata.", ex, cid);
                throw;
            }
        }
        private void dinamikButon_Click(object sender, EventArgs e)
        {
            var cid = Guid.NewGuid().ToString("N");
            try
            {
                if (_istenCikisModu)
                {
                    if (!_auth.Can(PageName, YetkiTipleri.Delete)) { System.Media.SystemSounds.Beep.Play(); return; }
                }
                else if (_guncelleModu)
                {
                    if (!_auth.Can(PageName, YetkiTipleri.Update)) { System.Media.SystemSounds.Beep.Play(); return; }
                }
                else if (_isYeniKayit)
                {
                    if (!_auth.Can(PageName, YetkiTipleri.Create)) { System.Media.SystemSounds.Beep.Play(); return; }
                }

                // --- İşten çıkış modu --- //
                if (_istenCikisModu)
                {
                    string personelId = null;
                    string adSoyad = null;

                    if (lstKisiler.SelectedItem is KisiListItem ki)
                    {
                        personelId = ki.PersonelId;
                        adSoyad = ki.AdSoyad;
                    }
                    else if (lstKisiler.SelectedItem is DataRowView drv)
                    {
                        personelId = Convert.ToString(drv["PersonelId"]);
                        adSoyad = Convert.ToString(drv["Adı Soyadı"]);
                    }

                    if (string.IsNullOrWhiteSpace(personelId))
                    {
                        MessageBox.Show("Seçili kaydın PersonelId bilgisi yok.", "Uyarı",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (!dtpIstenCikis.Checked)
                    {
                        MessageBox.Show("İşten çıkış tarihini seçiniz.", "Uyarı",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        dtpIstenCikis.Focus();
                        return;
                    }

                    var cikis = dtpIstenCikis.Value.Date;
                    var onay = MessageBox.Show(
                        $"{adSoyad} için işten çıkış tarihi {cikis:dd.MM.yyyy} olarak işlenecek. Onaylıyor musunuz?",
                        "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (onay != DialogResult.Yes)
                    {
                        LogHelper.Warn(PageName, "IstenCikis", "Kullanıcı işten çıkışı iptal etti.", null, cid);
                        return;
                    }

                    var ok = _kisiSvc.KisiIstenCikar(personelId, cikis, txtFirmaDisiKartNo.Text.Trim());

                    if (!ok)
                    {
                        LogHelper.Error(PageName, "IstenCikis", $"İşten çıkış başarısız. KisiId={personelId}", null, cid);
                        MessageBox.Show("İşten çıkış işlemi tamamlanamadı.", "Hata",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    _istenCikisModu = false;
                    ApplyCheckboxRules();
                    KisileriYukle(GetSeciliFirmaId());

                    LogHelper.Info(PageName, "IstenCikis", $"İşten çıkış başarıyla işlendi. KisiId={personelId}", null, cid);
                    MessageBox.Show("İşten çıkış başarıyla işlendi.", "Bilgi",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    IslemButonlariniGoster(false);
                    return;
                }

                // --- Güncelleme modu --- //
                if (_guncelleModu)
                {
                    if (string.IsNullOrWhiteSpace(txtSicilNo.Text))
                        throw new Exception("Sicil No (PersonelId) boş olamaz.");

                    // Puantajsız kart güncelleme (KartNo değişirse: eski pasif + yeni kayıt)
                    if (_seciliPuantajsizKartMi)
                    {
                        string guncelleKartId = txtSicilNo.Text.Trim();
                        string guncelleKartAdi = txtAdSoyad.Text?.Trim() ?? "";
                        string guncelleKartNo = txtPersonelKartNo.Text?.Trim() ?? "";
                        string calismaSekli = SecilenVardiyaIds();
                        try
                        {
                            _puantajsizKartRepo.UpdateByKartId(guncelleKartId, guncelleKartAdi, guncelleKartNo, calismaSekli);
                        }
                        catch (InvalidOperationException ex)
                        {
                            MessageBox.Show(ex.Message, "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        MessageBox.Show("Kart güncellendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        _guncelleModu = false;
                        KisileriYukle(GetSeciliFirmaId());
                        ApplyCheckboxRules();
                        IslemButonlariniGoster(false);
                        LogHelper.Info(PageName, "PuantajsizKartGuncelle", $"Kart güncellendi: {guncelleKartId}", null, cid);
                        return;
                    }

                    AdSoyadAyir(txtAdSoyad.Text, out string ad, out string soyad);

                    var kisi = new Kisi
                    {
                        PersonelId = txtSicilNo.Text.Trim(),
                        Ad = ad,
                        Soyad = soyad,
                        KartNo = txtPersonelKartNo.Text.Trim(),
                        TcKimlikNo = txtKimlikNo.Text.Trim(),
                        PozisyonId = GetComboNullableInt(cmbPozisyon),
                        DepartmanId = GetComboNullableInt(cmbDepartman),
                        IsyeriId = GetComboNullableInt(cmbIsyeri),
                        BolumId = GetComboNullableInt(cmbBolum),
                        FirmaId = GetComboNullableInt(cmbFirma) ?? GetSeciliFirmaId(),
                        IseGirisTarihi = dtpIseGiris.Value.Date,
                        IstenCikisTarihi = dtpIstenCikis.Checked ? (DateTime?)dtpIstenCikis.Value.Date : null,
                        DogumTarihi = dtpDogumGunu.Checked ? (DateTime?)dtpDogumGunu.Value.Date : null,
                        CalismaStatusu = (cmbCalismaStatu.SelectedValue ?? cmbCalismaStatu.Text)?.ToString(),
                        CalismaSekli = SecilenVardiyaIds(),
                        CepTel = txtCepTel.Text.Trim(),
                        Email = txtEmail.Text.Trim(),
                        Fotograf = _fotoDirty ? DbHelpers.ImageToBytes(pbKisiFoto.Image) : null,
                        PuantajYapilirMi = chkPuantajYapilirMi.Checked
                    };

                    bool firma = chkFirmaPersoneliMi.Checked;
                    bool puantaj = chkPuantajYapilirMi.Checked;
                    bool yemek = chkYemekHakkiVarMi.Checked;

                    var ok = _kisiSvc.KisiGuncelle(
                        kisi,
                        originalPersonelId: _originalPersonelId,
                        firma,
                        puantaj,
                        yemek,
                        (int)nudYemekAdedi.Value,
                        txtFirmaDisiKartNo.Text.Trim(),
                        fotoDegisti: _fotoDirty
                    );

                    if (!ok)
                    {
                        MessageBox.Show("Kayıt güncellenemedi!.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    MessageBox.Show("Kayıt güncellendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _originalPersonelId = kisi.PersonelId.Trim();
                    LogHelper.Info(PageName, "Guncelle", $"Kişi güncellendi. PersonelId={kisi.PersonelId}", null, cid);

                    _guncelleModu = false;
                    _fotoDirty = false;
                    KisileriYukle(GetSeciliFirmaId());
                    ApplyCheckboxRules();
                    IslemButonlariniGoster(false);
                    return;
                }

                // --- Yeni kayıt --- //
                // Puantajsız kart ekleme (Puantaj Yapılmayanlar listesindeyken Ekle)
                if (_isYeniKayit && cmbKartTipi.SelectedIndex == 1)
                {
                    string yeniKartId = txtSicilNo.Text?.Trim();
                    string yeniKartAdi = txtAdSoyad.Text?.Trim() ?? "";
                    string yeniKartNo = txtPersonelKartNo.Text?.Trim() ?? "";
                    if (string.IsNullOrWhiteSpace(yeniKartId))
                    {
                        MessageBox.Show("Kart kodu (Sicil No) boş olamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (_puantajsizKartRepo.Exists(yeniKartId))
                    {
                        MessageBox.Show("Bu kart kodu zaten kayıtlı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    int firmaId = GetSeciliFirmaId();
                    string calismaSekliCsv = SecilenVardiyaIds();
                    _puantajsizKartRepo.Insert(yeniKartId, yeniKartNo, yeniKartAdi, firmaId, calismaSekliCsv, ziyaretciMi: false, aracKartMi: false, taseronCalisanMi: false);
                    MessageBox.Show("Puantajsız kart eklendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _isYeniKayit = false;
                    KisileriYukle(GetSeciliFirmaId());
                    IslemButonlariniGoster(false);
                    LogHelper.Info(PageName, "PuantajsizKartEkle", $"Yeni kart eklendi: {yeniKartId}", null, cid);
                    return;
                }

                bool f = chkFirmaPersoneliMi.Checked;
                bool p = chkPuantajYapilirMi.Checked;
                bool y = chkYemekHakkiVarMi.Checked;

                var validasyonDto = new KisiKayitValidasyonDTO
                {
                    PersonelId = txtSicilNo.Text.Trim(),
                    FirmaPersoneli = f,
                    PuantajYapilir = p,
                    YemekHakkiVar = y,
                    YemekAdedi = (int)nudYemekAdedi.Value,
                    FirmaDisiKartNo = txtFirmaDisiKartNo.Text.Trim()
                };

                var validasyonSonuc = _kisiSvc.ValidateKisiKayit(validasyonDto);
                if (!validasyonSonuc.IsValid)
                {
                    MessageBox.Show(validasyonSonuc.Message);
                    return;
                }

                AdSoyadAyir(txtAdSoyad.Text, out string adYeni, out string soyadYeni);

                var yeniKisi = new Kisi
                {
                    PersonelId = txtSicilNo.Text.Trim(),
                    Ad = adYeni,
                    Soyad = soyadYeni,
                    KartNo = txtPersonelKartNo.Text.Trim(),
                    TcKimlikNo = txtKimlikNo.Text.Trim(),
                    PozisyonId = cmbPozisyon.SelectedValue as int?,
                    DogumTarihi = dtpDogumGunu.Checked ? (DateTime?)dtpDogumGunu.Value.Date : null,
                    DepartmanId = cmbDepartman.SelectedValue as int?,
                    IseGirisTarihi = dtpIseGiris.Value.Date,
                    IstenCikisTarihi = dtpIstenCikis.Checked ? (DateTime?)dtpIstenCikis.Value.Date : null,
                    CalismaStatusu = cmbCalismaStatu.SelectedValue != null
                        ? ((int)cmbCalismaStatu.SelectedValue).ToString()
                        : null,
                    FirmaId = (cmbFirma.SelectedValue as int?) ?? GetSeciliFirmaId(),
                    IsyeriId = cmbIsyeri.SelectedValue as int?,
                    BolumId = cmbBolum.SelectedValue as int?,
                    CalismaSekli = SecilenVardiyaIds(),
                    CepTel = txtCepTel.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Fotograf = pbKisiFoto.Image != null ? DbHelpers.ImageToBytes(pbKisiFoto.Image) : null,
                    PuantajYapilirMi = p
                };

                string kartId = txtSicilNo.Text.Trim();
                string kartNo = txtFirmaDisiKartNo.Text.Trim();
                string kartAdi = string.IsNullOrWhiteSpace(txtAdSoyad?.Text)
                    ? (yeniKisi.Ad + " " + yeniKisi.Soyad).Trim()
                    : txtAdSoyad.Text.Trim();

                _kisiSvc.YeniKisiEkle(
                    yeniKisi,
                    f,
                    p,
                    y,
                    (int)nudYemekAdedi.Value,
                    kartId,
                    kartNo,
                    kartAdi
                );

                LogHelper.Info(PageName, "Ekle", $"Yeni kişi eklendi. PersonelId={yeniKisi.PersonelId}", null, cid);
                MessageBox.Show("Kayıt tamamlandı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                KisileriYukle(GetSeciliFirmaId());
                IslemButonlariniGoster(false);
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "DinamikButon", "Genel hata.", ex, cid);
                MessageBox.Show("Hata: " + ex.Message, "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnFotoEkle_Click(object sender, EventArgs e)
        {
            var cid = Guid.NewGuid().ToString("N");
            try
            {
                if (!(_auth.Can(PageName, YetkiTipleri.Update) || _auth.Can(PageName, YetkiTipleri.Create)))
                {
                    System.Media.SystemSounds.Beep.Play();
                    return;
                }

                using (var ofd = new OpenFileDialog())
                {
                    ofd.Title = "Fotoğraf Seç";
                    ofd.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp";

                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            if (pbKisiFoto.Image != null)
                            {
                                pbKisiFoto.Image.Dispose();
                                pbKisiFoto.Image = null;
                            }

                            using (var fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read))
                            using (var img = Image.FromStream(fs))
                            {
                                pbKisiFoto.Image = new Bitmap(img);
                            }

                            pbKisiFoto.SizeMode = PictureBoxSizeMode.Zoom;
                            _fotoDirty = true;
                            LogHelper.Info(PageName, "FotoEkle", $"Fotoğraf yüklendi: {Path.GetFileName(ofd.FileName)}", null, cid);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Warn(PageName, "FotoEkle", "Kullanıcı fotoğraf seçimini iptal etti.", null, cid);
                            MessageBox.Show("Fotoğraf yüklenemedi: " + ex.Message, "Hata",
                                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "FotoEkle", "Fotoğraf yüklenirken hata.", ex, cid);
                throw;
            }

        }
        private void btnFotoSil_Click(object sender, EventArgs e)
        {
            var cid = Guid.NewGuid().ToString("N");
            try
            {
                if (!_auth.Can(PageName, YetkiTipleri.Update))
                {
                    System.Media.SystemSounds.Beep.Play();
                    return;
                }

                if (pbKisiFoto.Image == null)
                {
                    MessageBox.Show("Silinecek bir fotoğraf yok.", "Bilgi",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var cevap = MessageBox.Show("Fotoğrafı silmek istediğinize emin misiniz?",
                                            "Onay",
                                            MessageBoxButtons.YesNo,
                                            MessageBoxIcon.Question);

                if (cevap == DialogResult.Yes)
                {
                    pbKisiFoto.Image.Dispose();
                    pbKisiFoto.Image = null;
                    _fotoDirty = true;
                    LogHelper.Info(PageName, "FotoSil", "Fotoğraf silindi.", null, cid);
                }
                else
                {
                    LogHelper.Warn(PageName, "FotoSil", "Fotoğraf silme kullanıcı tarafından iptal.", null, cid);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "FotoSil", "Fotoğraf silinirken hata.", ex, cid);
                throw;
            }
        }
        private void AdSoyadAyir(string tamAd, out string ad, out string soyad)
        {
            ad = tamAd?.Trim() ?? "";
            soyad = "";

            if (string.IsNullOrEmpty(ad)) return;

            var parts = ad.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
            {
                soyad = "";
                return;
            }

            soyad = parts[parts.Length - 1];
            ad = string.Join(" ", parts, 0, parts.Length - 1);
        }
        private void UpdateUIState()
        {
            bool firma = chkFirmaPersoneliMi.Checked;
            bool puantaj = chkPuantajYapilirMi.Checked;
            bool yemek = chkYemekHakkiVarMi.Checked;

            bool kuralIzinVeriyor = (!firma) || (firma && !puantaj);
            bool alanAktif = kuralIzinVeriyor || _guncelleModu;

            txtFirmaDisiKartNo.Enabled = alanAktif;
            txtFirmaDisiKartNo.ReadOnly = !kuralIzinVeriyor && _guncelleModu ? false : !alanAktif;

            nudYemekAdedi.Enabled = yemek;
            if (!yemek && nudYemekAdedi.Value != 0) nudYemekAdedi.Value = 0;

            dtpIstenCikis.Enabled = _istenCikisModu;

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }
        private void ApplyCheckboxRules() => UpdateUIState();
        private void pbKisiAra_Click(object sender, EventArgs e)
        {
            _aktifFiltre = string.IsNullOrWhiteSpace(txtAra.Text) ? null : txtAra.Text.Trim();
            KisileriYukle(GetSeciliFirmaId());
        }
        private void btnVazgec_Click(object sender, EventArgs e)
        {
            _isYeniKayit = false;
            _guncelleModu = false;
            _istenCikisModu = false;

            if (lstKisiler.Items.Count > 0)
                lstKisiler_SelectedIndexChanged(null, null);

            UpdateUIState();
            IslemButonlariniGoster(false);
        }
        private void IslemButonlariniGoster(bool goster)
        {
            btnKaydet.Visible = goster;
            btnVazgec.Visible = goster;
        }
        private int? GetComboNullableInt(ComboBox cmb)
        {
            if (cmb?.SelectedValue == null)
                return null;

            // Direkt int geldiyse
            if (cmb.SelectedValue is int i)
                return i > 0 ? i : (int?)null;

            // string / object geldiyse
            if (int.TryParse(cmb.SelectedValue.ToString(), out var parsed))
                return parsed > 0 ? parsed : (int?)null;

            return null;
        }
    }
}
