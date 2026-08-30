using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace CeyPASS.WFA.UserControls.VMY
{
    public partial class ucCalismaSekilleri : UserControl
    {
        private enum ScreenMode { List, Add, Edit }
        private readonly ISessionContext _session;
        private readonly ICalismaSekliService _vsvc;
        private readonly IAuthorizationService _auth;
        private readonly IKullaniciFirmaIsyeriYetkiService _yetkiSvc;
        private readonly IPersonelVardiyaYemekYetkiService _yemekYetkiSvc;
        private readonly IKisiEkraniLookUpService _lookupSvc;
        private readonly ICihazService _cihazSvc;
        private ScreenMode _mode = ScreenMode.List;
        private bool _wired = false;
        private bool _saatPenceresiAktif;
        private int? _seciliYemekYetkiId;
        AuthorizationHelper authHelp;
        private const string PageName = "Vardiyalar";
        private const string PageNameUI = "Vardiyalar";
        private readonly WinFormsFieldErrors _fieldErrors;

        private Panel pnlYemekPencereleri;
        private Button btnYemekSaatleri;
        private Label lblYemekPencereBaslik;
        private Label lblYemekTalimat;
        private DataGridView dgvYemekPencereleri;
        private ComboBox cmbYemekIsyeri;
        private ComboBox cmbYemekCihaz;
        private DateTimePicker dtpYemekBas;
        private DateTimePicker dtpYemekBit;
        private CheckBox chkYemekAktif;
        private Button btnYemekEkle;
        private Button btnYemekGuncelle;
        private Button btnYemekSil;
        private Button btnYemekTemizle;

        /// <summary>
        /// Admin Panel'den açıldığında true; tüm firmaların tüm vardiyaları listelenir.
        /// </summary>
        public bool AdminPanelMode { get; set; }

        public ucCalismaSekilleri(
            ISessionContext session,
            ICalismaSekliService vsvc,
            IAuthorizationService auth,
            IKullaniciFirmaIsyeriYetkiService yetkiSvc,
            IPersonelVardiyaYemekYetkiService yemekYetkiSvc,
            IKisiEkraniLookUpService lookupSvc,
            ICihazService cihazSvc)
        {
            InitializeComponent();
            _fieldErrors = new WinFormsFieldErrors(this);
            _session = session;
            _vsvc = vsvc;
            _auth = auth;
            _yetkiSvc = yetkiSvc;
            _yemekYetkiSvc = yemekYetkiSvc;
            _lookupSvc = lookupSvc;
            _cihazSvc = cihazSvc;
            authHelp = new AuthorizationHelper(_session, _auth);
            var cid = Guid.NewGuid().ToString("N");
            AppTheme.ApplyToControl(this);
            LogHelper.Info(PageName, "Open", $"Açılış Kullanıcı Id={_session.AktifKullaniciId}, FirmaId={_session.AktifFirmaId}", cid);
            if (!_auth.ViewAbility(PageName))
            {
                LogHelper.Info(PageName, "View", "Görüntüleme yetkisi yok", cid);
                MessageBox.Show("Vardiyalar ekranını görüntüleme yetkiniz yok");
                this.Visible = false;
                return;
            }

            btnVardiyaEkle.Tag = YetkiTipleri.Create;
            btnVardiyaGuncelle.Tag = YetkiTipleri.Update;
            btnVardiyaSil.Tag = YetkiTipleri.Delete;
            btnKaydet.Tag = YetkiTipleri.Create;

            BuildYemekPencerePanel();
            InitTimePickers();
            WireEventsOnce();
            EnterListMode();
            LoadList();
            InitSaatPenceresiPanel();
            BeautifyList(chkVardiyalar);

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }

        private void BuildYemekPencerePanel()
        {
            // Vardiya formuna dokunma — yemek CRUD ayrı pencerede; araç çubuğunda buton.
            btnYemekSaatleri = new Button
            {
                Name = "btnYemekSaatleri",
                Text = "Yemekhane Saatleri\nDetayı",
                Size = new Size(210, btnVardiyaEkle.Height),
                Margin = btnVardiyaEkle.Margin,
                Padding = btnVardiyaEkle.Padding,
                BackColor = Color.FromArgb(255, 140, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = btnVardiyaEkle.Font,
                Image = CreateClockIcon(48),
                TextAlign = ContentAlignment.MiddleRight,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                UseVisualStyleBackColor = false,
                Visible = false
            };
            btnYemekSaatleri.FlatAppearance.BorderSize = 0;
            var tip = new ToolTip();
            tip.SetToolTip(btnYemekSaatleri, "Seçili vardiya için işyeri bazlı yemekhane saat aralıklarını yönetir");
            pnlToolbar.WrapContents = false;
            pnlToolbar.Controls.Add(btnYemekSaatleri);

            // Not: tlpForm içinde (dock yok) — pnlCard dock sırası bozulmasın.
            lblYemekTalimat = new Label
            {
                Name = "lblYemekTalimat",
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 4, 0, 8),
                Padding = new Padding(12, 8, 12, 8),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(73, 80, 87),
                BackColor = Color.FromArgb(255, 243, 224),
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "Not: İşyeri bazlı yemekhane saat aralığı eklemek için sol listeden vardiyayı seçin, " +
                       "turuncu «Yemekhane Saatleri Detayı» butonuna tıklayın; açılan pencerede işyeri, " +
                       "yemek başlangıç/bitiş saatlerini girip Ekle’ye basın. Güncellemek veya silmek için satırı seçin."
            };
            InsertYemekTalimatIntoForm();

            pnlYemekPencereleri = new Panel
            {
                Name = "pnlYemekPencereleri",
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                BackColor = Color.White
            };

            lblYemekPencereBaslik = new Label
            {
                Text = "Seçili vardiya için işyeri bazlı yemek saat aralıkları",
                Dock = DockStyle.Top,
                Height = 36,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var pnlYemekEditor = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 200,
                Padding = new Padding(0, 8, 0, 0)
            };

            dgvYemekPencereleri = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                BackgroundColor = Color.White
            };

            var lblIsyeri = new Label { Text = "İşyeri", Location = new Point(0, 4), AutoSize = true };
            cmbYemekIsyeri = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(0, 24),
                Width = 280,
                Font = new Font("Segoe UI", 10F)
            };

            var lblCihaz = new Label { Text = "Cihaz", Location = new Point(300, 4), AutoSize = true };
            cmbYemekCihaz = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(300, 24),
                Width = 280,
                Font = new Font("Segoe UI", 10F)
            };

            var lblBas = new Label { Text = "Yemek Başlangıç", Location = new Point(0, 60), AutoSize = true };
            dtpYemekBas = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "HH:mm",
                ShowUpDown = true,
                Location = new Point(0, 80),
                Width = 100
            };

            var lblBit = new Label { Text = "Yemek Bitiş", Location = new Point(130, 60), AutoSize = true };
            dtpYemekBit = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "HH:mm",
                ShowUpDown = true,
                Location = new Point(130, 80),
                Width = 100
            };

            chkYemekAktif = new CheckBox
            {
                Text = "Aktif",
                Checked = true,
                Location = new Point(250, 82),
                AutoSize = true
            };

            btnYemekEkle = new Button
            {
                Text = "Ekle",
                Location = new Point(0, 125),
                Size = new Size(80, 36),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Tag = YetkiTipleri.Create
            };
            btnYemekEkle.FlatAppearance.BorderSize = 0;

            btnYemekGuncelle = new Button
            {
                Text = "Güncelle",
                Location = new Point(90, 125),
                Size = new Size(90, 36),
                BackColor = Color.FromArgb(23, 162, 184),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Tag = YetkiTipleri.Update
            };
            btnYemekGuncelle.FlatAppearance.BorderSize = 0;

            btnYemekSil = new Button
            {
                Text = "Sil",
                Location = new Point(190, 125),
                Size = new Size(70, 36),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Tag = YetkiTipleri.Delete
            };
            btnYemekSil.FlatAppearance.BorderSize = 0;

            btnYemekTemizle = new Button
            {
                Text = "Temizle",
                Location = new Point(270, 125),
                Size = new Size(80, 36),
                FlatStyle = FlatStyle.Flat
            };

            pnlYemekEditor.Controls.Add(lblIsyeri);
            pnlYemekEditor.Controls.Add(cmbYemekIsyeri);
            pnlYemekEditor.Controls.Add(lblCihaz);
            pnlYemekEditor.Controls.Add(cmbYemekCihaz);
            pnlYemekEditor.Controls.Add(lblBas);
            pnlYemekEditor.Controls.Add(dtpYemekBas);
            pnlYemekEditor.Controls.Add(lblBit);
            pnlYemekEditor.Controls.Add(dtpYemekBit);
            pnlYemekEditor.Controls.Add(chkYemekAktif);
            pnlYemekEditor.Controls.Add(btnYemekEkle);
            pnlYemekEditor.Controls.Add(btnYemekGuncelle);
            pnlYemekEditor.Controls.Add(btnYemekSil);
            pnlYemekEditor.Controls.Add(btnYemekTemizle);

            pnlYemekPencereleri.Controls.Add(dgvYemekPencereleri);
            pnlYemekPencereleri.Controls.Add(pnlYemekEditor);
            pnlYemekPencereleri.Controls.Add(lblYemekPencereBaslik);
        }

        /// <summary>
        /// Diğer ekranlarla paylaşılmayan, yalnızca bu buton için çizilen saat ikonu.
        /// </summary>
        private static Image CreateClockIcon(int size)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            int m = 3;
            using (var pen = new Pen(Color.Black, 2.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                var rect = new Rectangle(m, m, size - 2 * m, size - 2 * m);
                g.DrawEllipse(pen, rect);

                int cx = size / 2;
                int cy = size / 2;
                g.DrawLine(pen, cx, cy, cx, cy - size / 4);
                g.DrawLine(pen, cx, cy, cx + size / 5, cy + size / 10);
                g.FillEllipse(Brushes.Black, cx - 2, cy - 2, 4, 4);
            }

            return bmp;
        }

        /// <summary>
        /// Talimat etiketini tlpForm'un en üst satırına ekler; mevcut satırları bir alta kaydırır.
        /// </summary>
        private void InsertYemekTalimatIntoForm()
        {
            if (tlpForm == null || lblYemekTalimat == null)
                return;

            tlpForm.SuspendLayout();
            try
            {
                // Mevcut kontrollerin satırını +1
                foreach (Control c in tlpForm.Controls.Cast<Control>().ToList())
                {
                    int row = tlpForm.GetRow(c);
                    if (row >= 0)
                        tlpForm.SetRow(c, row + 1);
                }

                tlpForm.RowCount += 1;
                tlpForm.RowStyles.Insert(0, new RowStyle(SizeType.Absolute, 58F));
                tlpForm.Controls.Add(lblYemekTalimat, 0, 0);
                tlpForm.SetColumnSpan(lblYemekTalimat, 2);
            }
            finally
            {
                tlpForm.ResumeLayout(true);
            }
        }

        private void SetYemekPanelVisible(bool visible)
        {
            if (btnYemekSaatleri != null)
                btnYemekSaatleri.Visible = visible;
            if (lblYemekTalimat != null)
            {
                lblYemekTalimat.Visible = visible;
                int row = tlpForm != null ? tlpForm.GetRow(lblYemekTalimat) : -1;
                if (row >= 0 && row < tlpForm.RowStyles.Count)
                    tlpForm.RowStyles[row].Height = visible ? 58F : 0F;
            }
        }

        private void InitSaatPenceresiPanel()
        {
            int firmaId = _session.AktifFirmaId ?? 0;
            bool hasFlag = false;
            try
            {
                hasFlag = firmaId > 0 && _yemekYetkiSvc.FirmaHasSaatPenceresiAktif(firmaId);
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "InitSaatPenceresi", "FirmaHasSaatPenceresiAktif hata", ex);
                MessageBox.Show(
                    $"Yemek saat penceresi kontrolü başarısız (FirmaId={firmaId}):\n{ex.Message}",
                    "Yemekhane Saatleri",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                hasFlag = false;
            }

            _saatPenceresiAktif = hasFlag;
            LogHelper.Info(PageName, "InitSaatPenceresi",
                $"FirmaId={firmaId}, SaatPenceresiAktif={_saatPenceresiAktif}");

            SetYemekPanelVisible(true);

            if (!_saatPenceresiAktif)
                return;

            LoadYemekIsyerleri();
            LoadYemekCihazlar();
            ClearYemekEditor();
            LoadYemekPencereleri();
            UpdateYemekCrudEnabled();
        }

        /// <summary>
        /// Scoped UC tekrar host'a eklenince gate/listeyi yeniler (Cihazlar'da flag değişmiş olabilir).
        /// </summary>
        public void OnHostedAgain()
        {
            if (IsDisposed)
                return;
            InitSaatPenceresiPanel();
            if (_mode == ScreenMode.List)
                FillInputsFromSelection();
        }

        private void BtnYemekSaatleri_Click(object sender, EventArgs e)
        {
            int firmaId = _session.AktifFirmaId ?? 0;
            try
            {
                _saatPenceresiAktif = firmaId > 0 && _yemekYetkiSvc.FirmaHasSaatPenceresiAktif(firmaId);
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "YemekSaatleriClick", "FirmaHasSaatPenceresiAktif hata", ex);
                MessageBox.Show(
                    $"Kontrol hatası (FirmaId={firmaId}):\n{ex.Message}",
                    "Yemekhane Saatleri Detayı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!_saatPenceresiAktif)
            {
                MessageBox.Show(
                    $"Aktif firmada (FirmaId={firmaId}) 'Yemek saat penceresi aktif' işaretli cihaz bulunamadı.\n\n" +
                    "Cihazlar ekranında ilgili cihazı seçip kutuyu işaretleyin, Kaydet'e basın.\n" +
                    "Cihaz başka firmadaysa önce o firmaya geçin.",
                    "Yemekhane Saatleri Detayı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            var vardiya = chkVardiyalar.SelectedItem as CalismaSekli;
            if (vardiya == null || _mode == ScreenMode.Add)
            {
                MessageBox.Show("Önce listeden bir vardiya seçin.", "Yemekhane Saatleri Detayı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LoadYemekIsyerleri();
            LoadYemekCihazlar();
            ClearYemekEditor();
            LoadYemekPencereleri();
            UpdateYemekCrudEnabled();

            using var dlg = new Form
            {
                Text = $"Yemekhane Saatleri Detayı — {vardiya.Ad}",
                Size = new Size(780, 560),
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = true,
                ShowInTaskbar = false,
                Font = new Font("Segoe UI", 10F)
            };

            dlg.FormClosing += (_, __) =>
            {
                if (pnlYemekPencereleri.Parent == dlg)
                    dlg.Controls.Remove(pnlYemekPencereleri);
            };

            if (pnlYemekPencereleri.Parent != null)
                pnlYemekPencereleri.Parent.Controls.Remove(pnlYemekPencereleri);

            pnlYemekPencereleri.Dock = DockStyle.Fill;
            dlg.Controls.Add(pnlYemekPencereleri);
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, dlg);
            dlg.ShowDialog(FindForm());
        }

        private static void SafeSelectFirst(ComboBox cmb)
        {
            if (cmb == null || cmb.IsDisposed)
                return;
            try
            {
                if (cmb.Items.Count > 0)
                    cmb.SelectedIndex = 0;
                else
                    cmb.SelectedIndex = -1;
            }
            catch (ArgumentException)
            {
                // Boş / bağlanmamış ComboBox'ta SelectedIndex=0 ArgumentException fırlatabilir
            }
        }

        private void LoadYemekIsyerleri()
        {
            int firmaId = _session.AktifFirmaId ?? 0;
            var list = _lookupSvc.GetIsyerleri(firmaId) ?? new List<LookupItem>();
            bool isAdmin = FirmaIsyeriYetkiHelper.IsAdmin(_session.RolId);
            var yetkiler = _yetkiSvc.GetYetkiler((int)_session.AktifKullaniciId);
            list = FirmaIsyeriYetkiHelper.FilterIsyeriLookup(list, firmaId, yetkiler, isAdmin)
                .Where(x => x.Id > 0)
                .ToList();

            cmbYemekIsyeri.DataSource = null;
            cmbYemekIsyeri.DisplayMember = nameof(LookupItem.Ad);
            cmbYemekIsyeri.ValueMember = nameof(LookupItem.Id);
            cmbYemekIsyeri.DataSource = list;
            SafeSelectFirst(cmbYemekIsyeri);
        }

        private void LoadYemekCihazlar()
        {
            int firmaId = _session.AktifFirmaId ?? 0;
            var list = _cihazSvc.GetListe(sadeceAktif: true, firmaId) ?? new List<CihazListDTO>();

            cmbYemekCihaz.DataSource = null;
            cmbYemekCihaz.DisplayMember = nameof(CihazListDTO.CihazAdi);
            cmbYemekCihaz.ValueMember = nameof(CihazListDTO.CihazId);
            cmbYemekCihaz.DataSource = list;
            SafeSelectFirst(cmbYemekCihaz);
        }

        private void LoadYemekPencereleri()
        {
            if (!_saatPenceresiAktif)
                return;

            var vardiya = chkVardiyalar.SelectedItem as CalismaSekli;
            if (vardiya == null || _mode == ScreenMode.Add)
            {
                BindYemekGrid(new List<YemekPencereGridRow>());
                return;
            }

            var data = _yemekYetkiSvc.GetByCalismaSekliId(vardiya.Id) ?? new List<PersonelVardiyaYemekYetki>();
            BindYemekGrid(data.Select(x => new YemekPencereGridRow
            {
                Id = x.Id,
                Isyeri = x.IsyeriAdi ?? x.IsyeriId.ToString(),
                Cihaz = string.IsNullOrWhiteSpace(x.CihazAdi)
                    ? (x.CihazId > 0 ? x.CihazId.ToString() : "")
                    : x.CihazAdi,
                Baslangic = x.YemekBaslangicSaati.ToString(@"hh\:mm"),
                Bitis = x.YemekBitisSaati.ToString(@"hh\:mm"),
                Aktif = x.AktifMi ? "Evet" : "Hayır",
                IsyeriId = x.IsyeriId,
                CihazId = x.CihazId,
                YemekBaslangicSaati = x.YemekBaslangicSaati,
                YemekBitisSaati = x.YemekBitisSaati,
                AktifMi = x.AktifMi
            }).ToList());
        }

        private void BindYemekGrid(List<YemekPencereGridRow> rows)
        {
            dgvYemekPencereleri.DataSource = null;
            dgvYemekPencereleri.AutoGenerateColumns = true;
            dgvYemekPencereleri.DataSource = rows ?? new List<YemekPencereGridRow>();
            ApplyYemekGridColumnHeaders();
        }

        private void ApplyYemekGridColumnHeaders()
        {
            // Browsable(false) teknik kolonları gizler; görünenlere Türkçe başlık ver.
            void H(string name, string header)
            {
                if (dgvYemekPencereleri.Columns[name] != null)
                    dgvYemekPencereleri.Columns[name].HeaderText = header;
            }

            H(nameof(YemekPencereGridRow.Isyeri), "İşyeri");
            H(nameof(YemekPencereGridRow.Cihaz), "Cihaz");
            H(nameof(YemekPencereGridRow.Baslangic), "Yemek Başlangıç");
            H(nameof(YemekPencereGridRow.Bitis), "Yemek Bitiş");
            H(nameof(YemekPencereGridRow.Aktif), "Aktif");
        }

        private sealed class YemekPencereGridRow
        {
            [Browsable(false)]
            public int Id { get; set; }

            [DisplayName("İşyeri")]
            public string Isyeri { get; set; }

            [DisplayName("Cihaz")]
            public string Cihaz { get; set; }

            [DisplayName("Yemek Başlangıç")]
            public string Baslangic { get; set; }

            [DisplayName("Yemek Bitiş")]
            public string Bitis { get; set; }

            [DisplayName("Aktif")]
            public string Aktif { get; set; }

            [Browsable(false)]
            public int IsyeriId { get; set; }

            [Browsable(false)]
            public int CihazId { get; set; }

            [Browsable(false)]
            public TimeSpan YemekBaslangicSaati { get; set; }

            [Browsable(false)]
            public TimeSpan YemekBitisSaati { get; set; }

            [Browsable(false)]
            public bool AktifMi { get; set; }
        }

        private void UpdateYemekCrudEnabled()
        {
            if (!_saatPenceresiAktif)
                return;

            bool canEdit = _mode == ScreenMode.List && chkVardiyalar.SelectedItem is CalismaSekli;
            cmbYemekIsyeri.Enabled = canEdit;
            cmbYemekCihaz.Enabled = canEdit;
            dtpYemekBas.Enabled = canEdit;
            dtpYemekBit.Enabled = canEdit;
            chkYemekAktif.Enabled = canEdit;
            btnYemekEkle.Enabled = canEdit;
            btnYemekGuncelle.Enabled = canEdit && _seciliYemekYetkiId.HasValue;
            btnYemekSil.Enabled = canEdit && _seciliYemekYetkiId.HasValue;
            btnYemekTemizle.Enabled = canEdit;
            dgvYemekPencereleri.Enabled = canEdit;
        }

        private void ClearYemekEditor()
        {
            _seciliYemekYetkiId = null;
            SafeSelectFirst(cmbYemekIsyeri);
            SafeSelectFirst(cmbYemekCihaz);
            SetTS(dtpYemekBas, new TimeSpan(11, 30, 0));
            SetTS(dtpYemekBit, new TimeSpan(12, 30, 0));
            chkYemekAktif.Checked = true;
            if (dgvYemekPencereleri != null && !dgvYemekPencereleri.IsDisposed)
                dgvYemekPencereleri.ClearSelection();
            UpdateYemekCrudEnabled();
        }

        private PersonelVardiyaYemekYetki BuildYemekYetkiFromEditor()
        {
            var vardiya = chkVardiyalar.SelectedItem as CalismaSekli;
            if (vardiya == null)
                return null;

            int isyeriId = ReadComboInt(cmbYemekIsyeri);
            int cihazId = ReadComboInt(cmbYemekCihaz);

            return new PersonelVardiyaYemekYetki
            {
                Id = _seciliYemekYetkiId ?? 0,
                CalismaSekliId = vardiya.Id,
                IsyeriId = isyeriId,
                CihazId = cihazId,
                YemekBaslangicSaati = TS(dtpYemekBas),
                YemekBitisSaati = TS(dtpYemekBit),
                AktifMi = chkYemekAktif.Checked
            };
        }

        private static int ReadComboInt(ComboBox cmb)
        {
            if (cmb?.SelectedValue is int iv)
                return iv;
            if (cmb?.SelectedValue != null && int.TryParse(cmb.SelectedValue.ToString(), out int parsed))
                return parsed;
            return 0;
        }

        private void DgvYemekPencereleri_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvYemekPencereleri.CurrentRow?.DataBoundItem is not YemekPencereGridRow item)
                return;

            _seciliYemekYetkiId = item.Id;
            cmbYemekIsyeri.SelectedValue = item.IsyeriId;
            cmbYemekCihaz.SelectedValue = item.CihazId;
            SetTS(dtpYemekBas, item.YemekBaslangicSaati);
            SetTS(dtpYemekBit, item.YemekBitisSaati);
            chkYemekAktif.Checked = item.AktifMi;
            UpdateYemekCrudEnabled();
        }

        private void BtnYemekEkle_Click(object sender, EventArgs e)
        {
            if (!_auth.Can(PageName, YetkiTipleri.Create)) { System.Media.SystemSounds.Beep.Play(); return; }
            var item = BuildYemekYetkiFromEditor();
            if (item == null)
            {
                MessageBox.Show("Önce bir vardiya seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            item.Id = 0;
            var (ok, error) = _yemekYetkiSvc.Add(item);
            if (!ok)
            {
                MessageBox.Show(error, "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ClearYemekEditor();
            LoadYemekPencereleri();
            MessageBox.Show("Yemek saat penceresi eklendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }

        private void BtnYemekGuncelle_Click(object sender, EventArgs e)
        {
            if (!_auth.Can(PageName, YetkiTipleri.Update)) { System.Media.SystemSounds.Beep.Play(); return; }
            if (!_seciliYemekYetkiId.HasValue)
            {
                MessageBox.Show("Güncellemek için listeden bir kayıt seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var item = BuildYemekYetkiFromEditor();
            if (item == null)
                return;

            var (ok, error) = _yemekYetkiSvc.Update(item);
            if (!ok)
            {
                MessageBox.Show(error, "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ClearYemekEditor();
            LoadYemekPencereleri();
            MessageBox.Show("Yemek saat penceresi güncellendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }

        private void BtnYemekSil_Click(object sender, EventArgs e)
        {
            if (!_auth.Can(PageName, YetkiTipleri.Delete)) { System.Media.SystemSounds.Beep.Play(); return; }
            if (!_seciliYemekYetkiId.HasValue)
            {
                MessageBox.Show("Silmek için listeden bir kayıt seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!UiConfirm.Confirm(this, "Seçili yemek saat penceresi silinsin mi?", "Onay", "Sil", "Vazgeç"))
                return;

            if (!_yemekYetkiSvc.Delete(_seciliYemekYetkiId.Value))
            {
                MessageBox.Show("Silme işlemi başarısız.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ClearYemekEditor();
            LoadYemekPencereleri();
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }

        private void InitTimePickers()
        {
            var pickers = new[]
            {
            dtpVardiyaBaslangicSaati,
            dtpVardiyaBitisSaati,
            dtpVardiyaBaslangicToleransSaati,
            dtpVardiyaBitisToleransSaati,
            dtpYemekAktiflemeSaati
        };

            foreach (var p in pickers)
            {
                p.Format = DateTimePickerFormat.Custom;
                p.CustomFormat = "HH:mm";
                p.ShowUpDown = true;
                p.Value = new DateTime(1900, 1, 1, 9, 0, 0);
            }
        }
        private static TimeSpan TS(DateTimePicker p) => p.Value.TimeOfDay;
        private static void SetTS(DateTimePicker p, TimeSpan ts) => p.Value = new DateTime(1900, 1, 1).Add(ts);
        private void BeautifyList(CheckedListBox list)
        {
            list.BorderStyle = BorderStyle.None;
            list.Font = new Font("Segoe UI", 10f);
            list.BackColor = Color.White;
            list.CheckOnClick = false;
        }
        private void WireEventsOnce()
        {
            if (_wired) return;

            btnVardiyaEkle.Click += (s, e) => EnterAddMode();
            btnVardiyaGuncelle.Click += (s, e) => EnterEditModeFromSelection();
            btnVardiyaSil.Click += (s, e) => DeleteSelected();

            btnKaydet.Click += (s, e) => Save();
            btnVazgec.Click += (s, e) => EnterListMode();

            chkVardiyalar.SelectedIndexChanged += (s, e) =>
            {
                if (_mode != ScreenMode.List) return;
                FillInputsFromSelection();
            };

            btnYemekSaatleri.Click += BtnYemekSaatleri_Click;
            btnYemekEkle.Click += BtnYemekEkle_Click;
            btnYemekGuncelle.Click += BtnYemekGuncelle_Click;
            btnYemekSil.Click += BtnYemekSil_Click;
            btnYemekTemizle.Click += (s, e) => ClearYemekEditor();
            dgvYemekPencereleri.SelectionChanged += DgvYemekPencereleri_SelectionChanged;

            _wired = true;
        }
        private void EnterListMode()
        {
            _mode = ScreenMode.List;

            btnKaydet.Visible = false;
            btnVazgec.Visible = false;

            btnVardiyaEkle.Enabled = true;
            btnVardiyaGuncelle.Enabled = chkVardiyalar.SelectedItem != null;
            btnVardiyaSil.Enabled = btnVardiyaGuncelle.Enabled;

            txtVardiyaAdi.ReadOnly = true;
            FillInputsFromSelection();
            UpdateYemekCrudEnabled();
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }
        private void EnterAddMode()
        {
            if (!_auth.Can(PageName, YetkiTipleri.Create)) { System.Media.SystemSounds.Beep.Play(); return; }

            _mode = ScreenMode.Add;

            btnKaydet.Visible = true;
            btnVazgec.Visible = true;

            btnVardiyaEkle.Enabled = false;
            btnVardiyaGuncelle.Enabled = false;
            btnVardiyaSil.Enabled = false;

            txtVardiyaAdi.ReadOnly = false;
            txtVardiyaAdi.Clear();

            SetTS(dtpVardiyaBaslangicSaati, new TimeSpan(7, 0, 0));
            SetTS(dtpVardiyaBitisSaati, new TimeSpan(15, 0, 0));
            SetTS(dtpVardiyaBaslangicToleransSaati, new TimeSpan(0, 15, 0));
            SetTS(dtpVardiyaBitisToleransSaati, new TimeSpan(0, 15, 0));
            SetTS(dtpYemekAktiflemeSaati, new TimeSpan(17, 0, 0));

            txtVardiyaAdi.Focus();
            ClearYemekEditor();
            BindYemekGrid(new List<YemekPencereGridRow>());
            UpdateYemekCrudEnabled();

            btnKaydet.Tag = YetkiTipleri.Create;
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }
        private void EnterEditModeFromSelection()
        {
            if (!_auth.Can(PageName, YetkiTipleri.Update)) { System.Media.SystemSounds.Beep.Play(); return; }
            if (chkVardiyalar.SelectedItem as CalismaSekli == null) return;

            _mode = ScreenMode.Edit;

            btnKaydet.Visible = true;
            btnVazgec.Visible = true;

            btnVardiyaEkle.Enabled = false;
            btnVardiyaGuncelle.Enabled = false;
            btnVardiyaSil.Enabled = false;

            txtVardiyaAdi.ReadOnly = false;
            FillInputsFromSelection();
            txtVardiyaAdi.Focus();
            txtVardiyaAdi.SelectAll();
            UpdateYemekCrudEnabled();

            btnKaydet.Tag = YetkiTipleri.Update;
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }
        private void LoadList()
        {
            var list = AdminPanelMode
                ? _vsvc.GetAllForAdmin()
                : _vsvc.GetAll((int)_session.AktifFirmaId);

            chkVardiyalar.BeginUpdate();
            try
            {
                chkVardiyalar.Items.Clear();
                foreach (var it in list) chkVardiyalar.Items.Add(it);
                chkVardiyalar.DisplayMember = nameof(CalismaSekli.Ad);
                chkVardiyalar.ValueMember = nameof(CalismaSekli.Id);
                if (chkVardiyalar.Items.Count > 0) chkVardiyalar.SelectedIndex = 0;
            }
            finally { chkVardiyalar.EndUpdate(); }

            FillInputsFromSelection();
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }

        /// <summary>
        /// Admin Panel'de AdminPanelMode set edildikten sonra listeyi filtresiz yeniden yüklemek için.
        /// </summary>
        public void RefreshList()
        {
            LoadList();
            InitSaatPenceresiPanel();
        }

        private void FillInputsFromSelection()
        {
            var it = chkVardiyalar.SelectedItem as CalismaSekli;
            if (it == null)
            {
                txtVardiyaAdi.Clear();
                if (_saatPenceresiAktif)
                {
                    BindYemekGrid(new List<YemekPencereGridRow>());
                    ClearYemekEditor();
                }
                return;
            }

            txtVardiyaAdi.Text = it.Ad;

            SetTS(dtpVardiyaBaslangicSaati, it.Baslangic);
            SetTS(dtpVardiyaBitisSaati, it.Bitis);
            SetTS(dtpVardiyaBaslangicToleransSaati, it.BaslangicTolerans);
            SetTS(dtpVardiyaBitisToleransSaati, it.BitisTolerans);
            SetTS(dtpYemekAktiflemeSaati, it.YemekAktiflestirme);

            if (_saatPenceresiAktif && _mode == ScreenMode.List)
            {
                ClearYemekEditor();
                LoadYemekPencereleri();
            }

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }
        private void Save()
        {
            var cid = Guid.NewGuid().ToString("N");
            int firmaId = (int)_session.AktifFirmaId;
            bool isAdmin = FirmaIsyeriYetkiHelper.IsAdmin(_session.RolId);
            var yetkiler = _yetkiSvc.GetYetkiler((int)_session.AktifKullaniciId);
            if (!FirmaIsyeriYetkiHelper.IsFirmaAuthorized(firmaId, yetkiler, isAdmin))
            {
                MessageBox.Show("Bu firma için işlem yetkiniz yok.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_mode == ScreenMode.Add && !_auth.Can(PageName, YetkiTipleri.Create))
            { System.Media.SystemSounds.Beep.Play(); return; }

            if (_mode == ScreenMode.Edit && !_auth.Can(PageName, YetkiTipleri.Update))
            { System.Media.SystemSounds.Beep.Play(); return; }

            var ad = (txtVardiyaAdi.Text ?? "").Trim();
            _fieldErrors.Clear();
            if (!_fieldErrors.Require(txtVardiyaAdi, ad, "Vardiya adı boş bırakılamaz."))
            {
                txtVardiyaAdi.Focus();
                return;
            }

            var x = new CalismaSekli
            {
                FirmaId = (int)_session.AktifFirmaId,
                Ad = ad,
                Baslangic = TS(dtpVardiyaBaslangicSaati),
                Bitis = TS(dtpVardiyaBitisSaati),
                BaslangicTolerans = TS(dtpVardiyaBaslangicToleransSaati),
                BitisTolerans = TS(dtpVardiyaBitisToleransSaati),
                YemekAktiflestirme = TS(dtpYemekAktiflemeSaati)
            };

            bool ok;
            if (_mode == ScreenMode.Add)
            {
                ok = _vsvc.Add(x) > 0;
                LogHelper.Info(PageName, "Save.Add", $"Result={ok}", _session.AktifKullaniciId.ToString(), cid);
            }
            else if (_mode == ScreenMode.Edit && chkVardiyalar.SelectedItem is CalismaSekli cur)
            {
                x.Id = cur.Id;
                ok = _vsvc.Update(x);
                LogHelper.Info(PageName, "Save.Edit", $"Id={x.Id}, Result={ok}", _session.AktifKullaniciId.ToString(), cid);
            }
            else return;

            if (!ok)
            {
                LogHelper.Info(PageName, "Save.Exception", "Kaydetme hatası", _session.AktifKullaniciId.ToString(), cid);
                MessageBox.Show("İşlem başarısız.", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("Kayıt tamamlandı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadList();
            EnterListMode();
        }
        private void DeleteSelected()
        {
            var cid = Guid.NewGuid().ToString("N");
            if (!_auth.Can(PageName, YetkiTipleri.Delete)) { System.Media.SystemSounds.Beep.Play(); return; }
            if (!(chkVardiyalar.SelectedItem is CalismaSekli it)) return;

            if (!UiConfirm.Confirm(this, $"“{it.Ad}” silinsin mi?", "Onay", "Sil", "Vazgeç"))
                return;

            try
            {
                var ok = _vsvc.Delete(it.Id, (int)_session.AktifFirmaId);
                LogHelper.Info(PageName, "DeleteSelected", $"Deleting Id={it.Id}, FirmaId={_session.AktifFirmaId}", _session.AktifKullaniciId.ToString(), cid);
                if (!ok)
                {
                    LogHelper.Info(PageName, "DeleteSelected", "Manager returned false.", _session.AktifKullaniciId.ToString(), cid);
                    MessageBox.Show("Silme işlemi başarısız. Kayıt başka tablolarca kullanılıyor olabilir.",
                        "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "DeleteSelected.Exception", "Seçilen vardiya silinememe hatası", ex, _session.AktifKullaniciId.ToString(), cid);
                MessageBox.Show("Silme işlemi başarısız: " + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LoadList();
            EnterListMode();
        }
    }
}
