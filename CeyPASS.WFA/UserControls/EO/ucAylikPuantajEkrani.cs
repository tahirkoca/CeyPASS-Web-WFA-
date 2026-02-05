using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using CeyPASS.WFA.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace CeyPASS.WFA.UserControls.EO
{
    public partial class ucAylikPuantajEkrani : UserControl
    {
        private readonly ISessionContext _session;
        private readonly IPuantajService _psvc;
        private readonly IFirmaService _fsvc;
        private readonly IIsyeriService _isvc;
        private readonly IKisiService _ksvc;
        private readonly IAuthorizationService _auth;
        AuthorizationHelper authHelp;
        private int _seciliYil;
        private int _seciliAy;
        private int _seciliPersonelId;
        private string _seciliPersonelIdStr;
        private int _ekKayitGun;
        private HashSet<int> _firmaYetkileri = new HashSet<int>();
        private HashSet<int> _isyeriYetkileri = new HashSet<int>();
        private List<FirmaIsyeriYetkiDTO> _kullaniciYetkileri;
        private const string PageName = "AylikPuantaj";
        private const string PageNameUI = "Aylık Puantaj";

        public ucAylikPuantajEkrani(ISessionContext session, IPuantajService psvc, IFirmaService fsvc, IIsyeriService isvc, IKisiService ksvc, IAuthorizationService auth)
        {
            InitializeComponent();
            _session = session;
            _psvc = psvc;
            _fsvc = fsvc;
            _isvc = isvc;
            _ksvc = ksvc;
            _auth = auth;
            authHelp = new AuthorizationHelper(_session, _auth);
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
            btnEkKayitAyarla.Tag = YetkiTipleri.Update;
            btnPuantajYap.Tag = YetkiTipleri.Export;

            SetEkKayitAyarlaButtonVisibility();

            if (!_auth.ViewAbility(PageName))
            {
                LogHelper.Warn("AylikPuantaj", "View", "Görüntüleme yetkisi yok", detayJson: $"{{\"KullaniciId\":{_session.AktifKullaniciId}}}");
                MessageBox.Show("Aylık Puantaj ekranını görüntüleme yetkiniz yok");
                this.Visible = false;
                return;
            }
        }

        private void ucAylikPuantajEkrani_Load(object sender, EventArgs e)
        {
            try
            {
                _seciliYil = DateTime.Now.Year;
                _seciliAy = DateTime.Now.Month;
                _ekKayitGun = _psvc.GetEkKayitGun();
                txtEkKayitGunu.Text = _ekKayitGun.ToString();
                _kullaniciYetkileri = _psvc.GetKullaniciFirmaIsyeriYetkileri((int)_session.AktifKullaniciId);
                _firmaYetkileri = _kullaniciYetkileri.Select(y => y.FirmaId).Distinct().ToHashSet();
                GridHazirla();
                AySecimleriniYukle();
                FirmalariYukle();
                OlaylariBagla();
                AppTheme.ApplyToControl(this);
                LogHelper.Info("AylikPuantaj", "Open", "Ekran açıldı", detayJson: $"{{\"KullaniciId\":{_session.AktifKullaniciId},\"FirmaId\":{_session.AktifFirmaId},\"BaslangicAy\":{_seciliAy},\"BaslangicYil\":{_seciliYil}}}");
            }
            catch (Exception ex)
            {
                LogHelper.Error("AylikPuantaj", "Open", "Ekran yükleme hatası", ex);
                throw;
            }
        }
        private void GridHazirla()
        {
            var grid = Controls.Find("dgPuantaj", true).FirstOrDefault() as DataGridView;
            if (grid == null)
            {
                grid = new DataGridView
                {
                    Name = "dgPuantaj",
                    Dock = DockStyle.Fill,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    RowHeadersVisible = false,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    AutoGenerateColumns = false,
                    ReadOnly = false
                };
                tlpFilters.Controls.Add(grid, 0, 1);
                tlpFilters.SetColumnSpan(grid, tlpFilters.ColumnCount);
            }
            grid.DataBindingComplete += Grid_DataBindingComplete;
            grid.RowPrePaint += Grid_RowPrePaint;

            grid.Columns.Clear();
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tarih", DataPropertyName = "Tarih", Name = "colTarih", Width = 140, DefaultCellStyle = { Format = "d MMM yyyy dddd" } });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Vardiya Türü", DataPropertyName = "VardiyaTuru", Width = 90 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "İlk Giriş", DataPropertyName = "IlkGiris", Width = 60 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Son Çıkış", DataPropertyName = "SonCikis", Width = 60 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Vardiya Başlangıç", DataPropertyName = "VardiyaBaslangic", Width = 100 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Vardiya Bitiş", DataPropertyName = "VardiyaBitis", Width = 90 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Saatlik İzin", DataPropertyName = "SaatlikIzinDakika", Width = 120 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Erken Geliş", DataPropertyName = "ErkenGirisDakika", Width = 80 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Geç Çıkış", DataPropertyName = "GecCikisDakika", Width = 80 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Sistem FM", DataPropertyName = "SistemFMDakika", Width = 80 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Onay Durumu", DataPropertyName = "OnayDurumu", Width = 90 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Düzeltilmiş FM", DataPropertyName = "DuzenlenenFMDakika", Width = 100 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Açıklama", DataPropertyName = "Aciklama", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Çalışma Tipi", DataPropertyName = "CalismaTipi", Width = 90 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Çalışma Saati", DataPropertyName = "Saat", Width = 70, DefaultCellStyle = { Format = "0.##" } });

            var colOnay = new DataGridViewButtonColumn { HeaderText = "", Name = "colOnayBtn", Text = "ONAY", UseColumnTextForButtonValue = true, Width = 60 };
            colOnay.Tag = YetkiTipleri.Approve;
            colOnay.Visible = _auth.Can(PageName, YetkiTipleri.Approve);
            grid.Columns.Add(colOnay);

            var colRet = new DataGridViewButtonColumn { HeaderText = "", Name = "colRetBtn", Text = "RET", UseColumnTextForButtonValue = true, Width = 50 };
            colRet.Tag = YetkiTipleri.Delete;
            colRet.Visible = _auth.Can(PageName, YetkiTipleri.Delete);
            grid.Columns.Add(colRet);

            var colDuzenle = new DataGridViewButtonColumn { HeaderText = "", Name = "colDuzenleBtn", Text = "DÜZENLE", UseColumnTextForButtonValue = true, Width = 75 };
            colDuzenle.Tag = YetkiTipleri.Update;
            colDuzenle.Visible = _auth.Can(PageName, YetkiTipleri.Update);
            grid.Columns.Add(colDuzenle);

            grid.CellContentClick += Grid_CellContentClick;
        }
        private void Grid_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            ApplyLocksAndGreyRows(_ekKayitGun);
            ((DataGridView)sender).Invalidate();
        }
        private void AySecimleriniYukle()
        {
            cmbAySecimi.Items.Clear();
            DateTime baslangic = new DateTime(2025, 1, 1);
            DateTime bugun = DateTime.Now;

            while (baslangic <= bugun)
            {
                string ayYil = baslangic.ToString("MMMM yyyy", new CultureInfo("tr-TR"));
                cmbAySecimi.Items.Add(ayYil);

                baslangic = baslangic.AddMonths(1);
            }

            cmbAySecimi.SelectedItem = bugun.ToString("MMMM yyyy", new CultureInfo("tr-TR"));
        }
        private void FirmalariYukle()
        {
            var firmalar = _fsvc.GetPuantajFirmalar();

            if (_firmaYetkileri.Count > 0)
                firmalar = firmalar.Where(f => _firmaYetkileri.Contains(f.FirmaId)).ToList();

            cmbFirmaSecimi.DisplayMember = "FirmaAdi";
            cmbFirmaSecimi.ValueMember = "FirmaId";
            cmbFirmaSecimi.DataSource = firmalar;
            cmbFirmaSecimi.DropDownStyle = ComboBoxStyle.DropDownList;

            if (firmalar != null && firmalar.Count > 0)
                cmbFirmaSecimi.SelectedIndex = 0;

            IsyerleriniYukle();
        }
        private void IsyerleriniYukle()
        {
            if (cmbFirmaSecimi.SelectedValue == null) return;
            int firmaId = Convert.ToInt32(cmbFirmaSecimi.SelectedValue);

            var isyerleri = _isvc.GetIsyerleriByFirma(firmaId);

            // YENİ FİLTRELEME - Sadece bu firmada yetkili olduğu işyerlerini getir
            var yetkiliIsyerleri = _kullaniciYetkileri
                .Where(y => y.FirmaId == firmaId)
                .ToList();

            // Eğer IsyeriId NULL olan yetki varsa, tüm işyerlerini göster
            bool tumIsyerleriGoster = yetkiliIsyerleri.Any(y => !y.IsyeriId.HasValue);

            if (!tumIsyerleriGoster)
            {
                var yetkiliIsyeriIdler = yetkiliIsyerleri
                    .Where(y => y.IsyeriId.HasValue)
                    .Select(y => y.IsyeriId.Value)
                    .ToHashSet();

                isyerleri = isyerleri.Where(i => yetkiliIsyeriIdler.Contains(i.IsyeriId)).ToList();
            }

            cmbIsyeriSecimi.DisplayMember = "Ad";
            cmbIsyeriSecimi.ValueMember = "IsyeriId";
            cmbIsyeriSecimi.DataSource = isyerleri;

            if (isyerleri.Count > 0)
                cmbIsyeriSecimi.SelectedIndex = 0;

            PersonelleriYukle();
        }
        private void PersonelleriYukle()
        {
            if (cmbFirmaSecimi.SelectedValue == null || cmbIsyeriSecimi.SelectedValue == null) return;

            int firmaId = System.Convert.ToInt32(cmbFirmaSecimi.SelectedValue);
            int isyeriId = System.Convert.ToInt32(cmbIsyeriSecimi.SelectedValue);

            var kisiler = _ksvc.GetKisilerForPuantaj(firmaId, isyeriId, _seciliYil, _seciliAy);

            cmbAdSoyad.DisplayMember = "AdSoyad";
            cmbAdSoyad.ValueMember = "PersonelId";
            cmbAdSoyad.DataSource = kisiler;

            if (kisiler.Count > 0)
            {
                cmbAdSoyad.SelectedIndex = 0;
                _seciliPersonelIdStr = kisiler[0].PersonelId;
                int pid;
                _seciliPersonelId = int.TryParse(_seciliPersonelIdStr, out pid) ? pid : 0;
            }
            else
            {
                _seciliPersonelIdStr = null;
                _seciliPersonelId = 0;
            }

            GridYukle();
        }
        private void OlaylariBagla()
        {
            cmbAySecimi.SelectedIndexChanged += cmbAySecimi_SelectedIndexChanged;
            cmbFirmaSecimi.SelectedValueChanged += (s, e) => IsyerleriniYukle();
            cmbIsyeriSecimi.SelectedValueChanged += (s, e) => PersonelleriYukle();
            cmbAdSoyad.SelectedValueChanged += cmbAdSoyad_SelectedValueChanged;
        }
        private void GridYukle()
        {
            if (string.IsNullOrEmpty(_seciliPersonelIdStr)) return;

            int personelId;
            if (!int.TryParse(_seciliPersonelIdStr, out personelId))
            {
                MessageBox.Show("Seçili personelin ID’si sayısal değil.");
                return;
            }

            var liste = _psvc.GetAy(personelId, _seciliYil, _seciliAy);
            var grid = (DataGridView)Controls.Find("dgPuantaj", true)[0];
            grid.DataSource = new BindingList<PuantajGunSatirDTO>(liste);

            int ekGun = 0;
            int.TryParse(txtEkKayitGunu.Text, out ekGun);
            ApplyLocksAndGreyRows(_ekKayitGun);
        }
        private void Grid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var grid = (DataGridView)sender;
            if (e.RowIndex < 0) return;

            if (grid.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly)
                return;

            var btnCol = grid.Columns[e.ColumnIndex] as DataGridViewButtonColumn;
            if (btnCol == null) return;

            var satir = (PuantajGunSatirDTO)grid.Rows[e.RowIndex].DataBoundItem;
            var text = btnCol.Text;

            var pidObj = cmbAdSoyad.SelectedValue;
            int pid = (pidObj == null) ? _seciliPersonelId : Convert.ToInt32(pidObj);

            string need = text == "ONAY" ? YetkiTipleri.Approve
                   : text == "RET" ? YetkiTipleri.Delete
                   : text == "DÜZENLE" ? YetkiTipleri.Update
                   : YetkiTipleri.View;

            if (!_auth.Can(PageName, need))
            {
                LogHelper.Warn("AylikPuantaj", "ActionDenied",
                   $"Yetki yok: {need}",
                   detayJson: $"{{\"Pid\":{pid},\"Tarih\":\"{satir.Tarih:yyyy-MM-dd}\",\"Buton\":\"{text}\"}}");
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            if (IsLockedRow(satir))
            {
                LogHelper.Warn("AylikPuantaj", "RowLocked",
                   "Kilitli satır işlem denemesi",
                   detayJson: $"{{\"Pid\":{pid},\"Tarih\":\"{satir.Tarih:yyyy-MM-dd}\",\"Buton\":\"{text}\"}}");
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            var cid = Guid.NewGuid().ToString("N");
            if (text == "ONAY")
            {
                if (string.Equals(satir.CalismaTipi, "EKSİK VERİ", StringComparison.OrdinalIgnoreCase))
                {
                    var c = MessageBox.Show("Bu satır 'EKSİK VERİ'. Yine de onaylansın mı?",
                                            "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (c != DialogResult.Yes) return;
                }

                int fmToSave = satir.DuzenlenenFMDakika > 0
                               ? satir.DuzenlenenFMDakika
                               : satir.SistemFMDakika;

                _psvc.Onayla(pid, satir.Tarih, satir.DuzenlenenFMDakika, satir.Aciklama, satir.CalismaTipi, satir.Saat, (int)_session.AktifKullaniciId);
                LogHelper.Info("AylikPuantaj", "Approve",
                   "Satır onaylandı",
                   detayJson: $"{{\"Pid\":{pid},\"Tarih\":\"{satir.Tarih:yyyy-MM-dd}\",\"FM\":{fmToSave},\"CalismaTipi\":\"{satir.CalismaTipi}\",\"Saat\":{satir.Saat}}}",
                   cid: cid);
                satir.DuzenlenenFMDakika = fmToSave;
                satir.OnayDurumu = OnayDurumu.Onaylandı;

                try
                {
                    if (string.Equals(satir.CalismaTipi, "FM1", StringComparison.OrdinalIgnoreCase))
                    {
                        satir.Saat = _psvc.HesaplaFM1CalismaSaati(fmToSave);
                    }
                }
                catch
                { }
            }
            else if (text == "RET")
            {
                using (var f = new reddetmeEkrani(satir))
                {
                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        string sebep = f.RetSebebi;
                        _psvc.Reddet(pid, satir.Tarih, sebep, (int)_session.AktifKullaniciId);
                        LogHelper.Info("AylikPuantaj", "Reject", "Satır reddedildi", detayJson: $"{{\"Pid\":{pid},\"Tarih\":\"{satir.Tarih:yyyy-MM-dd}\",\"Sebep\":\"{sebep}\"}}", cid: cid);
                        satir.OnayDurumu = OnayDurumu.Reddedildi;
                        satir.Aciklama = sebep;
                    }
                }
            }
            else if (text == "DÜZENLE")
            {
                using (var f = new puantajSatirDuzenlemeEkrani(satir, pid, _psvc, _session))
                {
                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        _psvc.Duzenle(pid, satir.Tarih, satir.DuzenlenenFMDakika, satir.Aciklama, (int)_session.AktifKullaniciId);
                        LogHelper.Info("AylikPuantaj", "Update", "Satır güncellendi", detayJson: $"{{\"FirmaId\":{_session.AktifFirmaId},\"KullaniciId\":{_session.AktifKullaniciId},\"PersonelId\":{pid},\"Tarih\":\"{satir.Tarih:yyyy-MM-dd}\",\"YeniVeri\":{satir.DuzenlenenFMDakika},\"YeniAciklama\":\"{satir.Aciklama}\"}}", cid: cid);
                        satir.OnayDurumu = OnayDurumu.Düzeltildi;
                    }
                }
            }
            grid.Refresh();
            ApplyLocksAndGreyRows(_ekKayitGun);
        }
        private void Grid_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            var grid = (DataGridView)sender;
            var row = grid.Rows[e.RowIndex];
            var dto = row.DataBoundItem as PuantajGunSatirDTO;
            if (dto == null) return;

            if (IsLockedRow(dto))
            {
                row.DefaultCellStyle.BackColor = Color.Gainsboro;
                row.DefaultCellStyle.ForeColor = Color.DimGray;
                return;
            }

            row.DefaultCellStyle.ForeColor = Color.Black;
            switch (dto.OnayDurumu)
            {
                case OnayDurumu.Onaylandı: row.DefaultCellStyle.BackColor = Color.LightGreen; break;
                case OnayDurumu.Reddedildi: row.DefaultCellStyle.BackColor = Color.LightCoral; break;
                case OnayDurumu.Düzeltildi: row.DefaultCellStyle.BackColor = Color.LightBlue; break;
                default: row.DefaultCellStyle.BackColor = Color.White; break;
            }
        }
        private void cmbAdSoyad_SelectedValueChanged(object sender, EventArgs e)
        {
            _seciliPersonelIdStr = cmbAdSoyad.SelectedValue == null ? null : cmbAdSoyad.SelectedValue.ToString();
            int tmp;
            if (cmbAdSoyad.SelectedValue is int)
                _seciliPersonelId = (int)cmbAdSoyad.SelectedValue;
            else if (int.TryParse(_seciliPersonelIdStr, out tmp))
                _seciliPersonelId = tmp;
            else
                _seciliPersonelId = 0;

            GridYukle();
        }
        private void cmbAySecimi_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbAySecimi.SelectedItem == null) return;

            var secilen = DateTime.ParseExact(
                cmbAySecimi.SelectedItem.ToString(),
                "MMMM yyyy",
                new CultureInfo("tr-TR")
            );

            _seciliYil = secilen.Year;
            _seciliAy = secilen.Month;

            FirmalariYukle();

            ApplyLocksAndGreyRows(_ekKayitGun);
        }
        private void btnCokluSicileAktar_Click(object sender, EventArgs e)
        {
            if (_seciliPersonelId <= 0)
            {
                MessageBox.Show("Lütfen kişi seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int anaPersonelId = _seciliPersonelId;
            int yil = _seciliYil;
            int ay = _seciliAy;

            var onay = MessageBox.Show(
                $"Seçili kişinin bağlı tüm sicillerine {yil}-{ay:D2} ayının SON GÜNÜNE 'NG 7,5' yazılacak.\n" +
                "Ayrıca ana personelin ayın SON GÜNÜNDEKİ kayıtları kaldırılacaktır. Onaylıyor musunuz?",
                "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (onay != DialogResult.Yes) return;

            try
            {
                _psvc.CokluSicileAktar(anaPersonelId, yil, ay, _session.AktifKullaniciId);
                MessageBox.Show("Aktarım tamamlandı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                GridYukle();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Aktarım sırasında hata oluştu:\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private static bool IsMultiLockNote(string aciklama)
        {
            return !string.IsNullOrWhiteSpace(aciklama)
                && aciklama.StartsWith("Çoklu Sicil Aktarım", StringComparison.OrdinalIgnoreCase);
        }
        private bool IsLockedRow(PuantajGunSatirDTO dto)
        {
            if (dto == null) return true;

            if (dto.Tarih.Date >= DateTime.Today) return true;

            if (dto.OnayDurumu == OnayDurumu.Düzeltildi && IsMultiLockNote(dto.Aciklama))
                return true;

            return false;
        }
        private void ApplyLocksAndGreyRows(int ekGun)
        {
            var grid = Controls.Find("dgPuantaj", true).FirstOrDefault() as DataGridView;
            if (grid == null) return;
            ApplyLocksAndGreyRows(grid, ekGun);
        }
        private void ApplyLocksAndGreyRows(DataGridView grid, int ekGun)
        {
            if (grid == null || grid.Rows.Count == 0) return;

            int colTarih = -1;
            foreach (DataGridViewColumn c in grid.Columns)
            {
                if (string.Equals(c.DataPropertyName, "Tarih", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.Name, "Tarih", StringComparison.OrdinalIgnoreCase))
                { colTarih = c.Index; break; }
            }
            if (colTarih < 0) return;

            int colOnay = FindButtonCol(grid, "ONAY");
            int colRet = FindButtonCol(grid, "RET");
            int colDuz = FindButtonCol(grid, "DÜZENLE");

            DateTime today = DateTime.Today;
            DateTime currMonthBeg = new DateTime(today.Year, today.Month, 1);
            DateTime prevMonthBeg = currMonthBeg.AddMonths(-1);
            DateTime prevMonthEnd = currMonthBeg.AddDays(-1);
            if (ekGun < 0) ekGun = 0;
            DateTime prevEditableBeg = prevMonthEnd.AddDays(-ekGun + 1);

            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.IsNewRow) continue;

                object v = row.Cells[colTarih].Value;
                if (v == null || !(v is DateTime)) continue;
                DateTime d = (DateTime)v;

                bool editable = IsEditable(d, today, currMonthBeg, prevMonthBeg, prevMonthEnd, prevEditableBeg);

                SetButtonCellEnabled(row, colOnay, editable);
                SetButtonCellEnabled(row, colRet, editable);
                SetButtonCellEnabled(row, colDuz, editable);

                if (!editable)
                {
                    row.DefaultCellStyle.BackColor = Color.Gainsboro;
                    row.DefaultCellStyle.ForeColor = Color.DimGray;
                    row.DefaultCellStyle.SelectionBackColor = Color.Silver;
                    row.DefaultCellStyle.SelectionForeColor = Color.Black;
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.White;
                    row.DefaultCellStyle.ForeColor = Color.Black;
                    row.DefaultCellStyle.SelectionBackColor = grid.DefaultCellStyle.SelectionBackColor;
                    row.DefaultCellStyle.SelectionForeColor = grid.DefaultCellStyle.SelectionForeColor;
                }
            }
        }
        private bool IsEditable(DateTime d, DateTime today, DateTime currMonthBeg, DateTime prevMonthBeg, DateTime prevMonthEnd, DateTime _ignored_)
        {
            return _psvc.IsRowEditable(d, _ekKayitGun);
        }
        private int FindButtonCol(DataGridView grid, string text)
        {
            for (int i = 0; i < grid.Columns.Count; i++)
            {
                var b = grid.Columns[i] as DataGridViewButtonColumn;
                if (b != null && string.Equals(b.Text, text, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return -1;
        }
        private void SetButtonCellEnabled(DataGridViewRow row, int col, bool enabled)
        {
            if (col < 0) return;
            var cell = row.Cells[col] as DataGridViewButtonCell;
            if (cell == null) return;

            cell.ReadOnly = !enabled;
            cell.FlatStyle = enabled ? FlatStyle.Standard : FlatStyle.Flat;

            cell.Style.ForeColor = enabled ? Color.Black : Color.DarkGray;
            cell.Style.SelectionForeColor = enabled ? Color.Black : Color.DarkGray;
            cell.Style.BackColor = enabled ? Color.White : Color.Gainsboro;
            cell.Style.SelectionBackColor = enabled ? Color.WhiteSmoke : Color.Silver;
        }
        private void btnEkKayitAyarla_Click(object sender, EventArgs e)
        {
            if (!_auth.Can(PageName, YetkiTipleri.Update)) { System.Media.SystemSounds.Beep.Play(); return; }
            int gun;
            if (!int.TryParse(txtEkKayitGunu.Text, out gun) || gun < 0 || gun > 31)
            {
                LogHelper.Warn("AylikPuantaj", "SettingUpdate",
                       "Geçersiz ek kayıt günü",
                       detayJson: $"{{\"Girilen\":\"{txtEkKayitGunu.Text}\"}}");
                MessageBox.Show("Lütfen 0-31 arasında bir tam sayı girin.", "Uyarı",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _psvc.SetEkKayitGun(gun, (int)_session.AktifKullaniciId);
                _ekKayitGun = gun;
                MessageBox.Show($"Ek Kayıt Süresi güncellendi.\nSüre: {_ekKayitGun} gün.",
                                "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ApplyLocksAndGreyRows(_ekKayitGun);
                LogHelper.Info("AylikPuantaj", "SettingUpdate",
                     "Ek kayıt günü güncellendi",
                     detayJson: $"{{\"YeniDeger\":{gun}}}");
            }
            catch (Exception ex)
            {
                LogHelper.Error("AylikPuantaj", "SettingUpdate",
                        "Ek kayıt günü güncellenemedi", ex,
                        detayJson: $"{{\"YeniDeger\":{gun}}}");
                MessageBox.Show("Güncelleme başarısız:\n" + ex.Message, "Hata",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnPuantajYap_Click(object sender, EventArgs e)
        {
            if (!_auth.Can(PageName, YetkiTipleri.Export)) { System.Media.SystemSounds.Beep.Play(); return; }

            var cid = Guid.NewGuid().ToString("N");

            try
            {
                DateTime secilenTarih = Convert.ToDateTime(cmbAySecimi.SelectedItem.ToString());

                LogHelper.Info("AylikPuantaj", "ExportStart", "Puantaj Excel export başlatıldı", detayJson: $"{{\"Ay\":\"{secilenTarih:MMMM yyyy}\",\"KullaniciId\":{_session.AktifKullaniciId}}}", cid: cid);
                var request = new PuantajExportRequest
                {
                    Yil = secilenTarih.Year,
                    Ay = secilenTarih.Month,
                    Yetkiler = _kullaniciYetkileri
                };

                var exportData = _psvc.PrepareMonthlyExport(request);

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Dosyası | *.xlsx",
                    Title = "Excel Dosyasını Kaydet",
                    FileName = $"{secilenTarih.Year} {secilenTarih:MMMM} Ayı Puantaj Exceli.xlsx"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ExcelHelper.ExceleDonustur(exportData, saveFileDialog.FileName);
                    LogHelper.Info("AylikPuantaj", "ExportDone", "Puantaj Excel export tamamlandı", detayJson: $"{{\"Dosya\":\"{saveFileDialog.FileName.Replace("\\", "\\\\")}\",\"Satir\":{exportData.Count}}}", cid: cid);
                }
                else
                {
                    LogHelper.Warn("AylikPuantaj", "ExportCanceled", "Kullanıcı exportu iptal etti", cid: cid);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("AylikPuantaj", "Export", "Export sırasında hata", ex, cid: cid);
                MessageBox.Show("Export sırasında hata oluştu:\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void SetEkKayitAyarlaButtonVisibility()
        {
            bool canAccess = _session.RolId == 1 || _session.RolId == 2;

            txtEkKayitGunu.Enabled = canAccess;
            btnEkKayitAyarla.Enabled = canAccess;

            if (!canAccess)
            {
                LogHelper.Info("AylikPuantaj", "Access", "Ek Kayıt Ayarla butonuna erişim engellendi",
                    detayJson: $"{{\"KullaniciId\":{_session.AktifKullaniciId},\"RolId\":{_session.RolId}}}");
            }
        }
    }
}
