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
        private readonly IAuthorizationService _auth;
        AuthorizationHelper authHelp;
        private DataTable _report;
        private int? _dashboardFirmaId = null;
        private const string PageName = "Raporlar";
        private const string PageNameUI = "Raporlar";

        public ucRaporlar(ISessionContext session, IRaporService rsvc, IAuthorizationService auth, IKullaniciQueryService kqsvc)
        {
            var cid = Guid.NewGuid().ToString("N");
            InitializeComponent();
            SendMessage(txtFiltrele.Handle, EM_SETCUEBANNER, 0, "Raporlarda arama yapabilirsiniz");
            _session = session;
            _rsvc = rsvc;
            _auth = auth;
            _kqsvc = kqsvc;
            authHelp = new AuthorizationHelper(_session, _auth);
            if (!_auth.ViewAbility(PageName))
            {
                LogHelper.Warn(PageName, "View", "Görüntüleme yetkisi yok", null, cid);
                MessageBox.Show("Raporlar ekranını görüntüleme yetkiniz yok");
                this.Visible = false;
                return;
            }
            AppTheme.ApplyToControl(this);
            LogHelper.Info(PageName, "Init", "ucRaporlar ctor",
                $"{{\"firmaId\":{_session.AktifFirmaId},\"kullaniciId\":{_session.AktifKullaniciId}}}", cid);
        }

        private void ucRaporlar_Load(object sender, EventArgs e)
        {
            var cid = Guid.NewGuid().ToString("N");
            

            btnRaporGetir.Tag = YetkiTipleri.View;
            btnExceleDonustur.Tag = YetkiTipleri.Export;
            btnPdfDonustur.Tag = YetkiTipleri.Export;

            RaporlariYukle();
            StilVerDataGridView(dgRaporlar);

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
            int firmaId = _dashboardFirmaId ?? (int)_session.AktifFirmaId;
            var isyeriIdList = _kqsvc.GetFirmayaAitIsyeriIdleri(firmaId);

            string firmaIdCsv = firmaId > 0 ? firmaId.ToString() : "";
            string isyeriIdCsv = (isyeriIdList != null && isyeriIdList.Count > 0) ? string.Join(",", isyeriIdList) : "";

            var parametreler = new Dictionary<string, object>
            {
                { "@FirmaIdList",   firmaIdCsv },
                { "@IsyeriIdList",  isyeriIdCsv },
                { "@TarihBaslangic", dtpGirisTarihi.Value },
                { "@TarihBitis",     dtpCikisTarihi.Value }
            };

            if (dtpGirisTarihi.Value.Date > dtpCikisTarihi.Value.Date)
            {
                MessageBox.Show("Başlangıç tarihi, bitiş tarihinden büyük olamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _report = _rsvc.CalistirRapor(procedureAdi, parametreler);
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
        public void OpenFromDashboard(ReportRequest req)
        {
            _dashboardFirmaId = req.FirmaId;

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
