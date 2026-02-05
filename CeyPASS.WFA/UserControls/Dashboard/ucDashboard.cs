using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace CeyPASS.WFA.UserControls.Dashboard
{
    public partial class ucDashboard : UserControl
    {
        private ToolTip dashboardToolTip;
        private int _selectedFirmaId;
        private readonly ISessionContext _session;
        private readonly IDashboardService _svc;
        private readonly IAuthorizationService _auth;
        private readonly IFirmaService _firmaSvc;
        private const string PageName = "Dashboard";
        private const string PageNameUI = "Ana Sayfa";
        public event EventHandler<ReportRequest> ReportRequested;

        public ucDashboard(ISessionContext session, IDashboardService dsvc, IAuthorizationService asvc, IFirmaService firmaSvc)
        {
            InitializeComponent();
            _session = session;
            _svc = dsvc;
            _auth = asvc;
            _firmaSvc = firmaSvc;
        }

        private void ucDashboard_Load(object sender, EventArgs e)
        {
            AppTheme.ApplyToControl(this);
            if (!_auth.ViewAbility(PageName))
            {
                MessageBox.Show("Ana sayfa ekranını görüntüleme yetkiniz yok");
                LogHelper.Warn(PageName, "View", "Dashboard görüntüleme yetkisi yok", detayJson: $"{{\"FirmaId\":{_session.AktifFirmaId}}}");
                this.Visible = false;
                return;
            }
            AuthorizationHelper authHelp = new AuthorizationHelper(_session, _auth);
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
            LogHelper.Info(PageName, "Open", "Dashboard açıldı", detayJson: $"{{\"FirmaId\":{_session.AktifFirmaId}}}");

            LoadFirmaComboBox();
            LoadDashboard();
            InitializeTooltips();
            BindCardClicks();
        }
        private void LoadFirmaComboBox()
        {
            try
            {
                cmbFirma.SelectedIndexChanged -= cmbFirma_SelectedIndexChanged;

                bool isAdmin = _session.RolId == 1 || _session.RolId == 2;
                if (isAdmin)
                {
                    var firmalar = _firmaSvc.GetAll()
                        .OrderBy(f => f.FirmaAdi)
                        .ToList();

                    cmbFirma.DataSource = firmalar;
                    cmbFirma.DisplayMember = "FirmaAdi";
                    cmbFirma.ValueMember = "FirmaId";
                    cmbFirma.Enabled = true;

                    if (firmalar.Any(f => f.FirmaId == _session.AktifFirmaId))
                    {
                        cmbFirma.SelectedValue = _session.AktifFirmaId;
                        _selectedFirmaId = (int)_session.AktifFirmaId;
                    }
                    else if (firmalar.Any())
                    {
                        _selectedFirmaId = firmalar.First().FirmaId;
                        cmbFirma.SelectedValue = _selectedFirmaId;
                    }

                    pnlFirmaFilter.Visible = true;
                }
                else
                {
                    _selectedFirmaId = (int)_session.AktifFirmaId;
                    pnlFirmaFilter.Visible = false;
                }

                cmbFirma.SelectedIndexChanged += cmbFirma_SelectedIndexChanged;
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "LoadFirmaComboBox", "Firma listesi yüklenirken hata", ex);
                MessageBox.Show("Firma listesi yüklenemedi!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);

                _selectedFirmaId = (int)_session.AktifFirmaId;
                pnlFirmaFilter.Visible = false;
            }
        }
        private void cmbFirma_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbFirma.SelectedValue != null && cmbFirma.SelectedValue is int)
                {
                    int yeniFirmaId = (int)cmbFirma.SelectedValue;

                    if (_selectedFirmaId != yeniFirmaId)
                    {
                        _selectedFirmaId = yeniFirmaId;
                        _session.AktifFirmaId = yeniFirmaId;
                        LoadDashboard();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "cmbFirma_SelectedIndexChanged", "Firma değiştirilirken hata", ex);
                MessageBox.Show("Firma değiştirilirken bir hata oluştu!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void InitializeTooltips()
        {
            dashboardToolTip = new ToolTip();

            dashboardToolTip.SetToolTip(label1, "Bugün en az 1 GİRİŞ ya da ÇIKIŞ hareketi bulunan kişi sayısını gösterir.");
            dashboardToolTip.SetToolTip(label2, "Şu anda işyerinde bulunan (son hareketi GİRİŞ olan) kişi sayısıdır.");
            dashboardToolTip.SetToolTip(label3, "Planlanan mesai başlangıcına tolerans süresinden sonra gelen\nçalışanların sayısıdır.");
            dashboardToolTip.SetToolTip(label4, "Şu anda dışarıda olan (son hareketi ÇIKIŞ olan) kişi sayısıdır.");
            dashboardToolTip.SetToolTip(label5, "Bugün hiç hareket yapmamış (gelmemiş) kişi sayısıdır.\nİzinli olanlar bu sayıya dahil değildir.");
            dashboardToolTip.SetToolTip(label6, "Bugün tüm gün izinli olan kişi sayısıdır.\nSaatlik izin kullananlar bu sayıya dahil değildir.");
            dashboardToolTip.SetToolTip(label7, "Bu ay işe başlayan kişi sayısıdır.");
            dashboardToolTip.SetToolTip(label8, "Bu ay işten ayrılan kişi sayısıdır.");
        }
        private void LoadDashboard()
        {
            var cid = Guid.NewGuid().ToString("N");
            try
            {
                LogHelper.Info(PageName, "LoadDashboard", $"Dashboard verisi çekiliyor... FirmaId:{_selectedFirmaId}, CID:{cid}");

                DashboardResult ds = _svc.GetDashboardForToday(_selectedFirmaId);

                FillCards(ds.Cards);
                FillGrids(ds);
                UpdateGridHeaders(ds);

                LogHelper.Info(PageName, "LoadDashboard", $"Dashboard başarıyla yüklendi. FirmaId:{_selectedFirmaId}, CID:{cid}");
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "LoadDashboard", $"Dashboard yüklenirken hata. FirmaId:{_selectedFirmaId}, CID:{cid}", ex);
                MessageBox.Show($"Dashboard verileri yüklenirken hata oluştu:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void FillCards(AnaEkranKartlariDashboard cards)
        {
            // KART 1: Giriş Yapanlar
            lblGirisYapan.Text = cards.GirisYapan.ToString("N0");
            lblGirisYapan.ForeColor = Color.FromArgb(52, 152, 219); // Mavi

            // KART 2: İçeridekiler
            lblIceridekiler.Text = cards.Iceridekiler.ToString("N0");
            lblIceridekiler.ForeColor = Color.FromArgb(46, 204, 113); // Yeşil

            // KART 3: Geç Kalanlar
            lblGecKalanlar.Text = cards.GecKalanlar.ToString("N0");
            lblGecKalanlar.ForeColor = Color.FromArgb(231, 76, 60); // Kırmızı

            // KART 4: Dışarıdakiler
            lblDisaridakiler.Text = cards.Disaridakiler.ToString("N0");
            lblDisaridakiler.ForeColor = Color.FromArgb(149, 165, 166); // Gri

            // KART 5: Devamsızlar
            lblDevamsizlar.Text = cards.Devamsizlar.ToString("N0");
            lblDevamsizlar.ForeColor = Color.FromArgb(192, 57, 43); // Koyu Kırmızı

            // KART 6: İzinliler
            lblIzinliler.Text = cards.Izinli.ToString("N0");
            lblIzinliler.ForeColor = Color.FromArgb(241, 196, 15); // Sarı

            // KART 7: İşe Başlayanlar
            lblIseBaslayan.Text = cards.IseBaslayan.ToString("N0");
            lblIseBaslayan.ForeColor = Color.FromArgb(26, 188, 156); // Turkuaz

            // KART 8: İşten Ayrılanlar
            lblIstenAyrilan.Text = cards.IstenAyrilan.ToString("N0");
            lblIstenAyrilan.ForeColor = Color.FromArgb(155, 89, 182); // Mor
        }
        private void FillGrids(DashboardResult ds)
        {
            // GRID 1: Geç Gelenler
            dgGecGelenler.DataSource = null;
            dgGecGelenler.DataSource = ds.LateList;
            ConfigureLateGrid(dgGecGelenler);

            // GRID 2: Doğum Günleri
            dgDogumGunleri.DataSource = null;
            dgDogumGunleri.DataSource = ds.Birthdays;
            ConfigureBirthdaysGrid(dgDogumGunleri);

            // GRID 3: İşe Başlayanlar
            dgIseBaslayanlar.DataSource = null;
            dgIseBaslayanlar.DataSource = ds.NewHires;
            ConfigureNewHiresGrid(dgIseBaslayanlar);

            // GRID 4: İşten Ayrılanlar
            dgIstenAyrilanlar.DataSource = null;
            dgIstenAyrilanlar.DataSource = ds.Resignations;
            ConfigureResignationsGrid(dgIstenAyrilanlar);
        }
        private void UpdateGridHeaders(DashboardResult ds)
        {
            lblGecGelenlerBaslik.Text = $"🕐 En Geç Gelen Personeller ({ds.LateList.Count} Kayıt)";
            lblDogumGunleriBaslik.Text = $"🎂 Bu Ay Doğum Günü Olanlar ({ds.Birthdays.Count} Kişi)";
            lblIseBaslayanlarBaslik.Text = $"🎉 Bu Ay İşe Başlayanlar ({ds.NewHires.Count} Kişi)";
            lblIstenAyrilanlarBaslik.Text = $"👋 Bu Ay İşten Ayrılanlar ({ds.Resignations.Count} Kişi)";
        }
        private void ConfigureLateGrid(DataGridView g)
        {
            ApplyModernGridTheme(g);

            Hide(g, "FirmaId", "IsyeriId", "PersonelId");

            Rename(g, "Ad", "Ad");
            Rename(g, "Soyad", "Soyad");
            Rename(g, "FazlaDakika", "Geç Kalınan Dakika");

            if (g.Columns["FazlaDakika"] != null)
                g.Columns["FazlaDakika"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            Order(g, "Ad", "Soyad", "FazlaDakika");
            SetMinWidth(g, ("Ad", 140), ("Soyad", 160), ("FazlaDakika", 120));

            g.CellFormatting -= LateGrid_CellFormatting;
            g.CellFormatting += LateGrid_CellFormatting;
        }
        private void LateGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridView g = sender as DataGridView;
            if (g == null) return;

            if (g.Columns[e.ColumnIndex].Name == "FazlaDakika" && e.Value != null)
            {
                if (int.TryParse(e.Value.ToString(), out int val) && val >= 60)
                {
                    g.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 243, 243);
                }
            }
        }
        private void ConfigureBirthdaysGrid(DataGridView g)
        {
            ApplyModernGridTheme(g);

            Hide(g, "FirmaId", "IsyeriId", "PersonelId", "Ay", "Gun", "Yas");

            Rename(g, "Ad", "Ad");
            Rename(g, "Soyad", "Soyad");
            Rename(g, "BuYilDogumGunu", "Doğum Tarihi");

            if (g.Columns["BuYilDogumGunu"] != null)
                g.Columns["BuYilDogumGunu"].DefaultCellStyle.Format = "dd MMMM";

            Order(g, "Ad", "Soyad", "BuYilDogumGunu");
            SetMinWidth(g, ("Ad", 120), ("Soyad", 140), ("BuYilDogumGunu", 120));
        }
        private void ConfigureNewHiresGrid(DataGridView g)
        {
            ApplyModernGridTheme(g);

            Hide(g, "FirmaId", "IsyeriId", "PersonelId");

            Rename(g, "Ad", "Ad");
            Rename(g, "Soyad", "Soyad");
            Rename(g, "BaslamaTarihi", "Başlama Tarihi");

            if (g.Columns["BaslamaTarihi"] != null)
                g.Columns["BaslamaTarihi"].DefaultCellStyle.Format = "dd.MM.yyyy";

            Order(g, "BaslamaTarihi", "Ad", "Soyad");
            SetMinWidth(g, ("BaslamaTarihi", 120), ("Ad", 140), ("Soyad", 160));
        }
        private void ConfigureResignationsGrid(DataGridView g)
        {
            ApplyModernGridTheme(g);

            Hide(g, "FirmaId", "IsyeriId", "PersonelId");

            Rename(g, "Ad", "Ad");
            Rename(g, "Soyad", "Soyad");
            Rename(g, "AyrilmaTarihi", "Ayrılma Tarihi");

            if (g.Columns["AyrilmaTarihi"] != null)
                g.Columns["AyrilmaTarihi"].DefaultCellStyle.Format = "dd.MM.yyyy";

            Order(g, "AyrilmaTarihi", "Ad", "Soyad");
            SetMinWidth(g, ("AyrilmaTarihi", 120), ("Ad", 140), ("Soyad", 160));
        }
        public static void ApplyModernGridTheme(DataGridView g)
        {
            g.BorderStyle = BorderStyle.None;
            g.BackgroundColor = Color.White;
            g.GridColor = Color.FromArgb(235, 238, 241);

            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(33, 37, 41);
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.ColumnHeadersHeight = 34;

            g.DefaultCellStyle.BackColor = Color.White;
            g.DefaultCellStyle.ForeColor = Color.FromArgb(33, 37, 41);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(222, 235, 255);
            g.DefaultCellStyle.SelectionForeColor = Color.FromArgb(33, 37, 41);
            g.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);
            g.RowTemplate.Height = 30;
            g.RowHeadersVisible = false;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.AllowUserToResizeRows = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.MultiSelect = false;
            g.ReadOnly = true;
            g.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            typeof(DataGridView).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(g, true, null);
        }
        private void Hide(DataGridView g, params string[] names)
        {
            foreach (var n in names)
                if (g.Columns[n] != null) g.Columns[n].Visible = false;
        }
        private void Rename(DataGridView g, string name, string header)
        {
            if (g.Columns[name] != null) g.Columns[name].HeaderText = header;
        }
        private void Order(DataGridView g, params string[] order)
        {
            for (int i = 0; i < order.Length; i++)
                if (g.Columns[order[i]] != null) g.Columns[order[i]].DisplayIndex = i;
        }
        private void SetMinWidth(DataGridView g, params (string col, int width)[] pairs)
        {
            foreach (var p in pairs)
                if (g.Columns[p.col] != null) g.Columns[p.col].MinimumWidth = p.width;
        }
        private void RaiseReport(DashboardReportTypeHelper type)
        {
            var req = new ReportRequest
            {
                Type = type,
                Baslangic = DateTime.Today,
                Bitis = DateTime.Today,
                FirmaId = _selectedFirmaId
            };

            ReportRequested?.Invoke(this, req);
        }
        private void BindCardClicks()
        {
            BindClickRecursive(pnlKart1, (s, e) => RaiseReport(DashboardReportTypeHelper.HareketiBulunanlar));
            BindClickRecursive(pnlKart2, (s, e) => RaiseReport(DashboardReportTypeHelper.Iceridekiler));
            BindClickRecursive(pnlKart3, (s, e) => RaiseReport(DashboardReportTypeHelper.GecKalanlar));
            BindClickRecursive(pnlKart4, (s, e) => RaiseReport(DashboardReportTypeHelper.Disaridakiler));
            BindClickRecursive(pnlKart5, (s, e) => RaiseReport(DashboardReportTypeHelper.Devamsizlar));
            BindClickRecursive(pnlKart6, (s, e) => RaiseReport(DashboardReportTypeHelper.Izinliler));
            BindClickRecursive(pnlKart7, (s, e) => RaiseReport(DashboardReportTypeHelper.IseBaslayanlar));
            BindClickRecursive(pnlKart8, (s, e) => RaiseReport(DashboardReportTypeHelper.IstenAyrilanlar));
        }
        private void BindClickRecursive(Control root, EventHandler handler)
        {
            if (root == null) return;

            root.Cursor = Cursors.Hand;

            root.Click -= handler;
            root.Click += handler;

            foreach (Control child in root.Controls)
                BindClickRecursive(child, handler);
        }
    }
}