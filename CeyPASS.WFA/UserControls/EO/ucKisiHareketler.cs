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
        AuthorizationHelper authHelp;
        private const string PageName = "KisiHareketler";
        private const string PageNameUI = "Kişi Hareketleri";
        private int SelectedFirmaId
        {
            get
            {
                if (cmbFirma.SelectedValue != null && cmbFirma.SelectedValue is int)
                    return (int)cmbFirma.SelectedValue;
                return (int)_session.AktifFirmaId;
            }
        }

        public ucKisiHareketler(ISessionContext session, IKisiHareketService khsvc, IKisiQueryService kqsvc, IAuthorizationService auth, IFirmaService firmaSvc)
        {
            InitializeComponent();
            _session = session;
            _khsvc = khsvc;
            _kqsvc = kqsvc;
            _auth = auth;
            _firmaSvc = firmaSvc;

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
            btnHareketSil.Click += (s, e) => SoftDeleteSelected();
            chkKisiler.KeyDown += chkKisiler_KeyDown;

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
                LoadFirmaComboBox();
                LoadPersons();

                AppTheme.ApplyToControl(this);
                LogHelper.Info("KisiHareketler", "Open", "Ekran açıldı", detayJson: $"{{\"FirmaId\":{_session.AktifFirmaId}}}");

                dtpHareketBaslangicTarihi.Format = DateTimePickerFormat.Custom;
                dtpHareketBaslangicTarihi.CustomFormat = "dd.MM.yyyy HH:mm";
                dtpHareketBaslangicTarihi.ShowUpDown = true;

                dtpHareketBitisTarihi.Format = DateTimePickerFormat.Custom;
                dtpHareketBitisTarihi.CustomFormat = "dd.MM.yyyy HH:mm";
                dtpHareketBitisTarihi.ShowUpDown = true;
            }
            catch (Exception ex)
            {
                LogHelper.Error("KisiHareketler", "Open", "Ekran yüklenirken hata", ex, detayJson: $"{{\"FirmaId\":{_session.AktifFirmaId}}}");
                throw;
            }
        }
        private void LoadFirmaComboBox()
        {
            try
            {
                cmbFirma.SelectedIndexChanged -= cmbFirma_SelectedIndexChanged;

                bool isAdmin = _session.RolId == 1 || _session.RolId == 2;

                if (isAdmin)
                {
                    var firmalar = _firmaSvc.GetAll().OrderBy(f => f.FirmaAdi).ToList();

                    if (firmalar != null && firmalar.Any())
                    {
                        cmbFirma.DataSource = firmalar;
                        cmbFirma.DisplayMember = "FirmaAdi";
                        cmbFirma.ValueMember = "FirmaId";
                        cmbFirma.Enabled = true;

                        if (firmalar.Any(f => f.FirmaId == _session.AktifFirmaId))
                        {
                            cmbFirma.SelectedValue = _session.AktifFirmaId;
                        }

                        pnlFirmaFilter.Visible = true;
                    }
                    else
                    {
                        pnlFirmaFilter.Visible = false;
                    }
                }
                else
                {
                    pnlFirmaFilter.Visible = false;
                }

                cmbFirma.SelectedIndexChanged += cmbFirma_SelectedIndexChanged;
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "LoadFirmaComboBox", "Firma listesi yüklenirken hata", ex);
                pnlFirmaFilter.Visible = false;
            }
        }
        private void cmbFirma_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbFirma.SelectedValue != null && cmbFirma.SelectedValue is int)
                {
                    LoadPersons();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "cmbFirma_SelectedIndexChanged", "Firma değiştirilirken hata", ex);
            }
        }
        private void LoadPersons()
        {
            if (chkKisiler == null) return;

            object src = _khsvc.GetAktifKisilerWithSicil(SelectedFirmaId);
            var list = new List<LookupItem>();

            var dt = src as DataTable;
            if (dt != null)
            {
                bool hasId = dt.Columns.Contains("PersonelId");
                bool hasAdSoyad = dt.Columns.Contains("AdSoyad");

                foreach (DataRow r in dt.Rows)
                {
                    int id = 0;
                    string ad = string.Empty;

                    if (hasId && r["PersonelId"] != DBNull.Value)
                        int.TryParse(r["PersonelId"].ToString(), out id);

                    if (hasAdSoyad && r["AdSoyad"] != DBNull.Value)
                        ad = r["AdSoyad"].ToString();

                    if (id > 0 && !string.IsNullOrWhiteSpace(ad))
                        list.Add(new LookupItem { Id = id, Ad = ad });
                }
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
            }
            finally
            {
                chkKisiler.EndUpdate();
            }

            chkKisiler.CheckOnClick = true;
            LogHelper.Info("KisiHareketler", "LoadPeople", "Kişi listesi yüklendi", detayJson: $"{{\"FirmaId\":{SelectedFirmaId},\"Adet\":{chkKisiler.Items.Count}}}");
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
                var ids = GetCheckedPersonIds();
                var bas = dtpHareketBaslangicTarihi.Value;
                var bit = dtpHareketBitisTarihi.Value;

                LogHelper.Info("KisiHareketler", "LoadGrid", "Grid sorgusu başlatıldı",
                    detayJson: $"{{\"SeciliIds\":\"{string.Join(",", ids)}\",\"Baslangic\":\"{bas:yyyy-MM-dd HH:mm}\",\"Bitis\":\"{bit:yyyy-MM-dd HH:mm}\",\"Aktif\":{(chbAktifHareketler.Checked ? 1 : 0)},\"Pasif\":{(chbPasifHareketler.Checked ? 1 : 0)},\"Yemekhane\":{(chbYemekhaneHareketleri.Checked ? 1 : 0)},\"FirmaId\":{SelectedFirmaId}}}",
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
                MessageBox.Show("Kişi seçiniz.");
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
        private void SoftDeleteSelected()
        {
            if (!_auth.Can(PageName, YetkiTipleri.Delete))
            {
                LogHelper.Warn("KisiHareketler", "ActionDenied", "Silme yetkisi yok", detayJson: $"{{\"Yetki\":\"Delete\"}}");
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            if (dgKisiHareketler.CurrentRow == null) return;
            var drv = dgKisiHareketler.CurrentRow.DataBoundItem as DataRowView;
            if (drv == null) return;

            int id = Convert.ToInt32(drv["Id"]);
            if (MessageBox.Show("Kayıt pasif edilsin mi?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                LogHelper.Info("KisiHareketler", "Delete", "Kullanıcı pasife çekmeyi iptal etti", detayJson: $"{{\"Id\":{id}}}");
                return;
            }

            if (_khsvc.PasifYap(id))
            {
                LogHelper.Info("KisiHareketler", "Delete", "Kayıt pasife çekildi", detayJson: $"{{\"Id\":{id}}}");
                LoadGrid();
            }
            else
            {
                LogHelper.Warn("KisiHareketler", "Delete", "Pasife çekme başarısız", detayJson: $"{{\"Id\":{id}}}");
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
