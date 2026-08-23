using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Color = System.Drawing.Color;

namespace CeyPASS.WFA.UserControls.Raporlar
{
    public partial class ucRaporlar : UserControl
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;
        private readonly ISessionContext _session;
        private readonly IRaporService _rsvc;
        private readonly IKullaniciQueryService _kqsvc;
        private readonly IKullaniciFirmaIsyeriYetkiService _yetkiSvc;
        private readonly IKisiEkraniLookUpService _lookupSvc;
        private readonly ICihazService _cihazSvc;
        private readonly IAuthorizationService _auth;
        AuthorizationHelper authHelp;
        private DataTable _report;
        private int? _loadedMultiFirmaId;
        private RaporParametreHelper.MultiSelectKind _loadedMultiKind;
        private IReadOnlyList<string> _aktifParametreler = Array.Empty<string>();
        private const string PageName = "Raporlar";
        private const string PageNameUI = "Raporlar";

        public ucRaporlar(ISessionContext session, IRaporService rsvc, IAuthorizationService auth, IKullaniciQueryService kqsvc, IKullaniciFirmaIsyeriYetkiService yetkiSvc, IKisiEkraniLookUpService lookupSvc, ICihazService cihazSvc)
        {
            var cid = Guid.NewGuid().ToString("N");
            InitializeComponent();
            SendMessage(txtFiltrele.Handle, EM_SETCUEBANNER, 0, "Raporlarda arama yapabilirsiniz");
            _session = session;
            _rsvc = rsvc;
            _auth = auth;
            _kqsvc = kqsvc;
            _yetkiSvc = yetkiSvc;
            _lookupSvc = lookupSvc;
            _cihazSvc = cihazSvc;
            authHelp = new AuthorizationHelper(_session, _auth);
            if (!_auth.ViewAbility(PageName))
            {
                LogHelper.Warn(PageName, "View", "Görüntüleme yetkisi yok", null, cid);
                MessageBox.Show("Raporlar ekranını görüntüleme yetkiniz yok");
                this.Visible = false;
                return;
            }
            AppTheme.ApplyToControl(this);
            VisibleChanged += ucRaporlar_VisibleChanged;
            LogHelper.Info(PageName, "Init", "ucRaporlar ctor",
                $"{{\"firmaId\":{_session.AktifFirmaId},\"kullaniciId\":{_session.AktifKullaniciId}}}", cid);
        }

        private void ucRaporlar_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
                EnsureMultiSelectGuncel();
        }

        /// <summary>Ana sayfada firma değişince veya rapor ekranına dönünce çoklu seçim listesini yeniler.</summary>
        public void RefreshForActiveFirma()
        {
            _loadedMultiFirmaId = null;
            EnsureMultiSelectGuncel();
        }

        private void ucRaporlar_Load(object sender, EventArgs e)
        {
            var cid = Guid.NewGuid().ToString("N");


            btnRaporGetir.Tag = YetkiTipleri.View;
            btnExceleDonustur.Tag = YetkiTipleri.Export;
            btnPdfDonustur.Tag = YetkiTipleri.Export;

            RaporlariYukle();
            StilVerDataGridView(dgRaporlar);
            cmbRaporTurleri.SelectedIndexChanged += CmbRaporTurleri_SelectedIndexChanged;
            ApplyRaporParametreUi();
            EnsureMultiSelectGuncel();

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
            LogHelper.Info(PageName, "Load", "Sayfa hazır", null, cid);
        }
        private void RaporlariYukle()
        {
            var cid = Guid.NewGuid().ToString("N");
            var raporlar = _rsvc.GetirRaporlar();
            cmbRaporTurleri.DataSource = raporlar;
            cmbRaporTurleri.DisplayMember = "RaporAdi";
            cmbRaporTurleri.ValueMember = "ProcedureAdi";

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
            LogHelper.Info(PageName, "RaporlariYukle", "Tamamlandı", $"{{\"adet\":{(raporlar?.Count ?? 0)}}}", cid);
        }

        private void CmbRaporTurleri_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyRaporParametreUi();
        }

        private void ApplyRaporParametreUi()
        {
            string procedureAdi = cmbRaporTurleri.SelectedValue as string;
            _aktifParametreler = string.IsNullOrEmpty(procedureAdi)
                ? Array.Empty<string>()
                : _rsvc.GetProcedureParameterNames(procedureAdi);
            _loadedMultiFirmaId = null;
            EnsureMultiSelectGuncel();
        }
        public static void StilVerDataGridView(DataGridView dgv)
        {
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;
            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.SelectionBackColor = Color.DarkBlue;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.Navy;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Arial", 10, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;

            dgv.DefaultCellStyle.Font = new System.Drawing.Font("Arial", 10);
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.RowHeadersVisible = false;

            dgv.RowTemplate.Height = 30;

            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
        private void btnRaporGetir_Click(object sender, EventArgs e)
        {
            var cid = Guid.NewGuid().ToString("N");
            if (!_auth.Can(PageName, YetkiTipleri.View)) { System.Media.SystemSounds.Beep.Play(); return; }

            string procedureAdi = cmbRaporTurleri.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(procedureAdi))
            {
                MessageBox.Show("Lütfen bir rapor seçin.");
                return;
            }

            LogHelper.Info(PageName, "RaporGetir", "Başladı", $"{{\"proc\":\"{procedureAdi}\",\"tarihBas\":\"{dtpGirisTarihi.Value:yyyy-MM-dd}\",\"tarihBit\":\"{dtpCikisTarihi.Value:yyyy-MM-dd}\"}}", cid);
            if (!_session.AktifFirmaId.HasValue)
            {
                MessageBox.Show("Firma bilgisi bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int firmaId = _session.AktifFirmaId.Value;
            bool isAdmin = FirmaIsyeriYetkiHelper.IsAdmin(_session.RolId);
            var yetkiler = _yetkiSvc.GetYetkiler((int)_session.AktifKullaniciId);
            if (!FirmaIsyeriYetkiHelper.IsFirmaAuthorized(firmaId, yetkiler, isAdmin))
            {
                MessageBox.Show("Seçili firma için rapor görüntüleme yetkiniz yok.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var kind = RaporParametreHelper.GetMultiSelect(_aktifParametreler);
            string isyeriIdCsv = "";
            string cihazIdCsv = "";

            if (kind == RaporParametreHelper.MultiSelectKind.Isyeri)
            {
                if (!TryBuildRaporIsyeriIdListCsv(firmaId, yetkiler, isAdmin, out isyeriIdCsv))
                    return;
            }
            else if (kind == RaporParametreHelper.MultiSelectKind.Cihaz)
            {
                cihazIdCsv = string.Join(",", GetSeciliRaporCihazIds());
            }

            string firmaIdCsv = firmaId > 0 ? firmaId.ToString() : "";

            var parametreler = new Dictionary<string, object>
            {
                { RaporParametreHelper.FirmaIdList, firmaIdCsv },
                { RaporParametreHelper.IsyeriIdList, isyeriIdCsv },
                { RaporParametreHelper.CihazIdList, cihazIdCsv },
                { RaporParametreHelper.TarihBaslangic, RaporTarihHelper.ToReportRangeStart(dtpGirisTarihi.Value) },
                { RaporParametreHelper.TarihBitis, RaporTarihHelper.ToReportRangeEnd(dtpCikisTarihi.Value) }
            };

            if (dtpGirisTarihi.Value.Date > dtpCikisTarihi.Value.Date)
            {
                MessageBox.Show("Başlangıç tarihi, bitiş tarihinden büyük olamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _report = _rsvc.CalistirRapor(procedureAdi, parametreler);
                // Grid’i sıfırla ki kolonlar yeni DataTable sırasıyla oluşsun
                dgRaporlar.DataSource = null;
                dgRaporlar.Columns.Clear();
                dgRaporlar.AutoGenerateColumns = true;
                dgRaporlar.DataSource = _report;
                LogHelper.Info(PageName, "RaporGetir", "Tamamlandı", $"{{\"rows\":{_report?.Rows.Count ?? 0},\"cols\":{_report?.Columns.Count ?? 0}}}", cid);
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "RaporGetir", "Hata", ex, null, cid);
                MessageBox.Show("Hata oluştu: " + ex.Message);
            }
        }
        private void btnExceleDonustur_Click(object sender, EventArgs e)
        {
            var cid = Guid.NewGuid().ToString("N");
            if (!_auth.Can(PageName, YetkiTipleri.Export))
            {
                System.Media.SystemSounds.Beep.Play();
                return;
            }
            LogHelper.Info(PageName, "Export", "Excel tıklandı", $"{{\"rows\":{_report?.Rows.Count ?? 0}}}", cid);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel Dosyası | *.xlsx";
            saveFileDialog.Title = "Excel Dosyasını Kaydet";
            saveFileDialog.FileName = $"{cmbRaporTurleri.Text.ToString()}({DateTime.Now.ToString("dd.MM.yyyy")}).xlsx";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                var dt = GetDataTableFromGrid(dgRaporlar);
                LogHelper.Info(PageName, "Export", "Excel başlıyor", $"{{\"path\":\"{saveFileDialog.FileName}\"}}", cid);
                ExportHelper.ExportToExcel(dt, saveFileDialog.FileName);
            }
        }
        private void btnPdfDonustur_Click(object sender, EventArgs e)
        {
            var cid = Guid.NewGuid().ToString("N");
            if (!_auth.Can(PageName, YetkiTipleri.Export))
            {
                System.Media.SystemSounds.Beep.Play();
                return;
            }
            LogHelper.Info(PageName, "Export", "PDF tıklandı", $"{{\"rows\":{_report?.Rows.Count ?? 0}}}", cid);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF Dosyası | *.pdf";
            saveFileDialog.Title = "PDF Dosyasını Kaydet";
            saveFileDialog.FileName = $"{cmbRaporTurleri.Text.ToString()}({DateTime.Now.ToString("dd.MM.yyyy")}).pdf";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                var dt = GetDataTableFromGrid(dgRaporlar);
                LogHelper.Info(PageName, "Export", "PDF başlıyor", $"{{\"path\":\"{saveFileDialog.FileName}\"}}", cid);
                ExportHelper.ExportToPdf(dt, saveFileDialog.FileName, cmbRaporTurleri?.Text ?? "Rapor");
            }
        }
        private void txtFiltrele_TextChanged(object sender, EventArgs e)
        {
            var cid = Guid.NewGuid().ToString("N");
            if (_report == null || _report.Columns.Count == 0)
                return;

            string filterText = txtFiltrele.Text.Trim();

            if (string.IsNullOrEmpty(filterText))
            {
                _report.DefaultView.RowFilter = "";
            }
            else
            {
                List<string> filterConditions = new List<string>();
                foreach (DataColumn column in _report.Columns)
                {
                    if (column.DataType == typeof(string) || column.DataType == typeof(int) || column.DataType == typeof(double))
                    {
                        filterConditions.Add($"CONVERT([{column.ColumnName}], System.String) LIKE '%{filterText}%'");
                    }
                }
                _report.DefaultView.RowFilter = string.Join(" OR ", filterConditions);
                dgRaporlar.DataSource = null;
                dgRaporlar.Columns.Clear();
                dgRaporlar.AutoGenerateColumns = true;
                dgRaporlar.DataSource = _report.DefaultView;
            }
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
            LogHelper.Info(PageName, "Filtre", "Uygulandı", $"{{\"sonucSatir\":{(_report?.DefaultView?.Count ?? 0)}}}", cid);
        }
        private DataTable GetDataTableFromGrid(DataGridView grid)
        {
            var ds = grid.DataSource;

            if (ds is DataView dv)
                return dv.ToTable();

            if (ds is BindingSource bs)
            {
                if (bs.List is DataView dv2) return dv2.ToTable();
                if (bs.DataSource is DataTable t2) return t2.DefaultView.ToTable();
            }

            if (ds is DataTable t)
                return t.DefaultView.ToTable();


            var result = new DataTable();
            foreach (DataGridViewColumn col in grid.Columns)
                result.Columns.Add(string.IsNullOrWhiteSpace(col.DataPropertyName) ? col.HeaderText : col.DataPropertyName);

            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.IsNewRow) continue;
                var values = row.Cells.Cast<DataGridViewCell>().Select(c => c.Value).ToArray();
                result.Rows.Add(values);
            }
            return result;
        }
        private void EnsureMultiSelectGuncel()
        {
            if (!_session.AktifFirmaId.HasValue)
                return;

            int firmaId = _session.AktifFirmaId.Value;
            var kind = RaporParametreHelper.GetMultiSelect(_aktifParametreler);
            if (_loadedMultiFirmaId == firmaId && _loadedMultiKind == kind)
                return;

            if (kind == RaporParametreHelper.MultiSelectKind.Cihaz)
                CihazSeciminiYukle(firmaId);
            else if (kind == RaporParametreHelper.MultiSelectKind.Isyeri)
                IsyeriSeciminiYukle(firmaId);
            else
            {
                chkRaporIsyerleri.Items.Clear();
                pnlIsyeriFilters.Visible = false;
                _loadedMultiFirmaId = firmaId;
                _loadedMultiKind = kind;
            }
        }

        private void CihazSeciminiYukle(int firmaId)
        {
            try
            {
                var list = _cihazSvc.GetListe(true, firmaId) ?? new List<CihazListDTO>();
                chkRaporIsyerleri.BeginUpdate();
                chkRaporIsyerleri.Items.Clear();
                foreach (var c in list)
                    chkRaporIsyerleri.Items.Add(c, false);
                chkRaporIsyerleri.DisplayMember = nameof(CihazListDTO.CihazAdi);
                lblIsyerleri.Text = "Cihazlar (işaretlenmezse firmanın tüm aktif cihazları)";
                pnlIsyeriFilters.Visible = chkRaporIsyerleri.Items.Count > 0;
                chkRaporIsyerleri.EndUpdate();
                _loadedMultiFirmaId = firmaId;
                _loadedMultiKind = RaporParametreHelper.MultiSelectKind.Cihaz;
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "CihazSeciminiYukle", "Cihaz listesi yüklenirken hata", ex);
            }
        }

        private void IsyeriSeciminiYukle(int firmaId)
        {
            try
            {
                bool isAdmin = FirmaIsyeriYetkiHelper.IsAdmin(_session.RolId);
                var yetkiler = _yetkiSvc.GetYetkiler((int)_session.AktifKullaniciId);
                var list = _lookupSvc.GetIsyerleri(firmaId) ?? new List<LookupItem>();
                list = FirmaIsyeriYetkiHelper.FilterIsyeriLookup(list, firmaId, yetkiler, isAdmin);

                chkRaporIsyerleri.BeginUpdate();
                chkRaporIsyerleri.Items.Clear();
                foreach (var iy in list.Where(x => x.Id > 0))
                    chkRaporIsyerleri.Items.Add(iy, false);

                chkRaporIsyerleri.DisplayMember = nameof(LookupItem.Ad);
                lblIsyerleri.Text = "İşyerleri (işaretlenmezse yetkili tüm işyerler)";
                pnlIsyeriFilters.Visible = chkRaporIsyerleri.Items.Count > 0;
                chkRaporIsyerleri.EndUpdate();
                _loadedMultiFirmaId = firmaId;
                _loadedMultiKind = RaporParametreHelper.MultiSelectKind.Isyeri;
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "IsyeriSeciminiYukle", "İşyeri listesi yüklenirken hata", ex);
            }
        }

        private List<int> GetSeciliRaporIsyeriIds()
        {
            return chkRaporIsyerleri.CheckedItems
                .OfType<LookupItem>()
                .Select(x => x.Id)
                .Where(id => id > 0)
                .Distinct()
                .ToList();
        }

        private List<int> GetSeciliRaporCihazIds()
        {
            return chkRaporIsyerleri.CheckedItems
                .OfType<CihazListDTO>()
                .Select(x => x.CihazId)
                .Where(id => id > 0)
                .Distinct()
                .ToList();
        }

        private bool TryBuildRaporIsyeriIdListCsv(int firmaId, List<FirmaIsyeriYetkiDTO> yetkiler, bool isAdmin, out string isyeriIdCsv)
        {
            isyeriIdCsv = "";
            var firmaIsyeriIds = _kqsvc.GetFirmayaAitIsyeriIdleri(firmaId) ?? new List<int>();
            var maxCsv = _yetkiSvc.BuildIsyeriIdListCsv(firmaId, yetkiler, isAdmin, firmaIsyeriIds);
            var selectedIds = GetSeciliRaporIsyeriIds();
            var (csv, status) = FirmaIsyeriYetkiHelper.ResolveRaporIsyeriIdListCsv(
                firmaId, selectedIds, maxCsv, yetkiler, isAdmin);

            if (status == FirmaIsyeriYetkiHelper.RaporIsyeriListStatus.UnauthorizedSelection)
            {
                MessageBox.Show("Seçilen işyerlerden bazıları için yetkiniz yok.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (status == FirmaIsyeriYetkiHelper.RaporIsyeriListStatus.NoAccess)
            {
                MessageBox.Show("Seçili firma için rapor görüntüleme yetkiniz yok.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            isyeriIdCsv = csv ?? "";
            return true;
        }

        public void OpenFromDashboard(ReportRequest req)
        {
            if (req.FirmaId > 0)
                _session.AktifFirmaId = req.FirmaId;

            _loadedMultiFirmaId = null;
            EnsureMultiSelectGuncel();

            dtpGirisTarihi.Value = req.Baslangic;
            dtpCikisTarihi.Value = req.Bitis;

            cmbRaporTurleri.SelectedValue = GetProcName(req.Type);
            btnRaporGetir.PerformClick();
        }
        private string GetProcName(DashboardReportTypeHelper type)
        {
            switch (type)
            {
                case DashboardReportTypeHelper.Izinliler: return "sp_GunlukIzinlilerRaporu";
                case DashboardReportTypeHelper.Disaridakiler: return "sp_AnlikDisaridakilerRaporu";
                case DashboardReportTypeHelper.HareketiBulunanlar: return "sp_GunlukHareketiBulunanlarRaporu";
                case DashboardReportTypeHelper.Iceridekiler: return "sp_AnlikIceridekilerRaporu";
                case DashboardReportTypeHelper.GecKalanlar: return "sp_GunlukGecKalanlarRaporu";
                case DashboardReportTypeHelper.Devamsizlar: return "sp_DevamsizlarRaporu";
                case DashboardReportTypeHelper.IseBaslayanlar: return "sp_IseBaslayanlarRaporu";
                case DashboardReportTypeHelper.IstenAyrilanlar: return "sp_IstenAyrilanlarRaporu";
                default: return null;
            }
        }
    }
}
