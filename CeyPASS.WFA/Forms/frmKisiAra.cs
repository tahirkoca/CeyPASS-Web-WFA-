using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CeyPASS.WFA.Forms
{
    public partial class frmKisiAra : Form
    {
        private const int PageSize = 25;
        private const int BtnW = 120;
        private const int BtnH = 36;

        private readonly IKisiQueryService _kisiQuery;
        private readonly IKisiEkraniLookUpService _lookup;
        private readonly IFirmaService _firmaSvc;
        private readonly IKullaniciFirmaIsyeriYetkiService _yetkiSvc;
        private readonly ISessionContext _session;

        private List<FirmaIsyeriYetkiDTO> _yetkiler = new();
        private bool _isAdmin;
        private bool _suppressEvents;
        private int _page = 1;
        private int _totalCount;

        private ComboBox cmbFirma;
        private ComboBox cmbIsyeri;
        private ComboBox cmbCalismaDurumu;
        private ComboBox cmbPuantaj;
        private TextBox txtAdSoyadKart;
        private TextBox txtSicil;
        private TextBox txtTc;
        private TextBox txtEmail;
        private ComboBox cmbDepartman;
        private ComboBox cmbPozisyon;
        private ComboBox cmbStatu;
        private DataGridView dgvSonuc;
        private Label lblSayfalama;
        private Button btnOnceki;
        private Button btnSonraki;
        private Button btnTemizle;
        private Button btnAra;
        private PictureBox picOnizleme;
        private Label lblOnizlemeAd;
        private Label lblOnizlemeTc;
        private Label lblOnizlemeDepartman;
        private Label lblOnizlemePozisyon;
        private Label lblOnizlemeStatu;
        private Label lblOnizlemeIsyeri;
        private Button btnKisiSec;

        public string SelectedPersonelId { get; private set; }
        /// <summary>Seçim sonrası ana ekran filtrelerinin senkronu için.</summary>
        public KisiAraContext AppliedContext { get; private set; }

        public frmKisiAra(
            IKisiQueryService kisiQuery,
            IKisiEkraniLookUpService lookup,
            IFirmaService firmaSvc,
            IKullaniciFirmaIsyeriYetkiService yetkiSvc,
            ISessionContext session)
        {
            _kisiQuery = kisiQuery;
            _lookup = lookup;
            _firmaSvc = firmaSvc;
            _yetkiSvc = yetkiSvc;
            _session = session;
            InitializeComponent();
            BuildUi();
        }

        public void SetContext(KisiAraContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            _isAdmin = FirmaIsyeriYetkiHelper.IsAdmin(_session.RolId);
            _yetkiler = _yetkiSvc.GetYetkiler((int)_session.AktifKullaniciId) ?? new List<FirmaIsyeriYetkiDTO>();

            _suppressEvents = true;
            try
            {
                LoadFirmaCombo(context.FirmaId);
                LoadIsyeriCombo(GetSeciliFirmaId(), context.IsyeriId);
                SelectByText(cmbCalismaDurumu, context.SadeceIstenCikanlar ? "İşten Çıkanlar" : "Aktif Çalışanlar");
                if (context.SadeceIstenCikanlar)
                    cmbPuantaj.SelectedIndex = 0;
                else if (context.PuantajYapilirMi == false)
                    SelectByText(cmbPuantaj, "Puantaj Yapılmayanlar");
                else
                    SelectByText(cmbPuantaj, "Puantaj Yapılanlar");

                UpdatePuantajEnabled();
                LoadDetailLookups();
                ResetDetailFilters();
            }
            finally
            {
                _suppressEvents = false;
            }

            RunSearch(resetPage: true);
        }

        private void BuildUi()
        {
            Text = "Personel Ara";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1140, 680);
            MinimumSize = new Size(960, 560);
            Font = new Font(AppTheme.FontFamily, 10F);
            BackColor = AppTheme.ContentBackground;

            var pnlRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(12)
            };
            pnlRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
            pnlRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            pnlRoot.Controls.Add(BuildContextBar(), 0, 0);

            var pnlBody = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3
            };
            pnlBody.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 270F));
            pnlBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            pnlBody.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250F));
            pnlBody.Controls.Add(BuildFilterPanel(), 0, 0);
            pnlBody.Controls.Add(BuildGridPanel(), 1, 0);
            pnlBody.Controls.Add(BuildPreviewPanel(), 2, 0);
            pnlRoot.Controls.Add(pnlBody, 0, 1);

            Controls.Add(pnlRoot);
        }

        private Panel BuildContextBar()
        {
            var card = CreateCard();
            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 8,
                RowCount = 1,
                Padding = new Padding(8, 4, 8, 4)
            };
            for (int i = 0; i < 4; i++)
            {
                tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));
                tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            }

            cmbFirma = NewContextCombo();
            cmbIsyeri = NewContextCombo();
            cmbCalismaDurumu = NewContextCombo();
            cmbPuantaj = NewContextCombo();

            cmbCalismaDurumu.DataSource = new[] { "Aktif Çalışanlar", "İşten Çıkanlar" };
            cmbPuantaj.DataSource = new[] { "Puantaj Yapılanlar", "Puantaj Yapılmayanlar" };

            cmbFirma.SelectedIndexChanged += (_, __) =>
            {
                if (_suppressEvents) return;
                LoadIsyeriCombo(GetSeciliFirmaId(), preferredIsyeriId: null);
                LoadDetailLookups();
                ResetDetailFilters();
                RunSearch(resetPage: true);
            };
            cmbIsyeri.SelectedIndexChanged += (_, __) =>
            {
                if (_suppressEvents) return;
                RunSearch(resetPage: true);
            };
            cmbCalismaDurumu.SelectedIndexChanged += (_, __) =>
            {
                if (_suppressEvents) return;
                UpdatePuantajEnabled();
                RunSearch(resetPage: true);
            };
            cmbPuantaj.SelectedIndexChanged += (_, __) =>
            {
                if (_suppressEvents) return;
                RunSearch(resetPage: true);
            };

            AddContextCell(tlp, 0, "Firma", cmbFirma);
            AddContextCell(tlp, 2, "İşyeri", cmbIsyeri);
            AddContextCell(tlp, 4, "Durum", cmbCalismaDurumu);
            AddContextCell(tlp, 6, "Puantaj", cmbPuantaj);

            card.Controls.Add(tlp);
            return card;
        }

        private static void AddContextCell(TableLayoutPanel tlp, int col, string caption, ComboBox cmb)
        {
            var lbl = new Label
            {
                Text = caption,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = AppTheme.TextSecondary,
                Padding = new Padding(0, 0, 6, 0)
            };
            tlp.Controls.Add(lbl, col, 0);
            tlp.Controls.Add(cmb, col + 1, 0);
        }

        private static ComboBox NewContextCombo()
        {
            return new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 8, 8, 8)
            };
        }

        private Panel BuildFilterPanel()
        {
            var card = CreateCard();
            const int fieldRows = 14;
            const int btnRowH = BtnH + 12;

            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = fieldRows + 2,
                Padding = new Padding(8, 8, 8, 8),
                AutoScroll = false,
                AutoSize = false
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            for (int i = 0; i < fieldRows; i++)
                tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, i % 2 == 0 ? 22F : 30F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, btnRowH));

            int row = 0;
            txtAdSoyadKart = AddFilterField(tlp, ref row, "Ad Soyad / Kart No");
            txtSicil = AddFilterField(tlp, ref row, "Sicil No");
            txtTc = AddFilterField(tlp, ref row, "TC Kimlik No");
            txtEmail = AddFilterField(tlp, ref row, "E-posta");
            cmbDepartman = AddFilterFieldCombo(tlp, ref row, "Departman");
            cmbPozisyon = AddFilterFieldCombo(tlp, ref row, "Pozisyon");
            cmbStatu = AddFilterFieldCombo(tlp, ref row, "Statü");

            var pnlBtns = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(0, 4, 0, 0)
            };
            pnlBtns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pnlBtns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pnlBtns.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            btnTemizle = CreateActionButton("Tümünü Sıfırla", secondary: true);
            btnTemizle.Dock = DockStyle.Fill;
            btnTemizle.Margin = new Padding(0, 0, 4, 0);
            btnTemizle.Click += (_, __) => { ResetDetailFilters(); RunSearch(resetPage: true); };

            btnAra = CreateActionButton("Ara", secondary: false);
            btnAra.Dock = DockStyle.Fill;
            btnAra.Margin = new Padding(4, 0, 0, 0);
            btnAra.Click += (_, __) => RunSearch(resetPage: true);

            pnlBtns.Controls.Add(btnTemizle, 0, 0);
            pnlBtns.Controls.Add(btnAra, 1, 0);
            tlp.Controls.Add(pnlBtns, 0, fieldRows + 1);

            card.Controls.Add(tlp);
            return card;
        }

        private Panel BuildGridPanel()
        {
            var card = CreateCard();
            card.Padding = new Padding(8);

            dgvSonuc = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            dgvSonuc.SelectionChanged += DgvSonuc_SelectionChanged;
            dgvSonuc.CellDoubleClick += (_, __) => TrySelectAndClose();

            var pnlPager = new Panel { Dock = DockStyle.Bottom, Height = 48 };
            btnOnceki = CreateActionButton("◀ Önceki", secondary: true);
            btnOnceki.Location = new Point(0, 6);
            btnSonraki = CreateActionButton("Sonraki ▶", secondary: true);
            btnSonraki.Location = new Point(BtnW + 8, 6);
            lblSayfalama = new Label
            {
                Location = new Point((BtnW + 8) * 2, 12),
                AutoSize = true,
                ForeColor = AppTheme.TextSecondary
            };
            btnOnceki.Click += (_, __) => { if (_page > 1) { _page--; RunSearch(resetPage: false); } };
            btnSonraki.Click += (_, __) =>
            {
                int maxPage = Math.Max(1, (int)Math.Ceiling(_totalCount / (double)PageSize));
                if (_page < maxPage) { _page++; RunSearch(resetPage: false); }
            };
            pnlPager.Controls.Add(btnOnceki);
            pnlPager.Controls.Add(btnSonraki);
            pnlPager.Controls.Add(lblSayfalama);

            card.Controls.Add(dgvSonuc);
            card.Controls.Add(pnlPager);
            return card;
        }

        private Panel BuildPreviewPanel()
        {
            var card = CreateCard();
            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 9,
                Padding = new Padding(12)
            };
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 140F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, BtnH + 8));

            picOnizleme = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.WhiteSmoke
            };
            lblOnizlemeAd = CreatePreviewLabel(true);
            lblOnizlemeTc = CreatePreviewLabel(false);
            lblOnizlemeDepartman = CreatePreviewLabel(false);
            lblOnizlemePozisyon = CreatePreviewLabel(false);
            lblOnizlemeStatu = CreatePreviewLabel(false);
            lblOnizlemeIsyeri = CreatePreviewLabel(false);

            btnKisiSec = CreateActionButton("Personel Seç", secondary: false);
            btnKisiSec.Dock = DockStyle.Fill;
            btnKisiSec.Width = 0;
            btnKisiSec.Click += (_, __) => TrySelectAndClose();

            tlp.Controls.Add(picOnizleme, 0, 0);
            tlp.Controls.Add(lblOnizlemeAd, 0, 1);
            tlp.Controls.Add(lblOnizlemeTc, 0, 2);
            tlp.Controls.Add(lblOnizlemeDepartman, 0, 3);
            tlp.Controls.Add(lblOnizlemePozisyon, 0, 4);
            tlp.Controls.Add(lblOnizlemeStatu, 0, 5);
            tlp.Controls.Add(lblOnizlemeIsyeri, 0, 6);
            tlp.Controls.Add(btnKisiSec, 0, 8);

            card.Controls.Add(tlp);
            return card;
        }

        private static Button CreateActionButton(string text, bool secondary)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(BtnW, BtnH),
                FlatStyle = FlatStyle.Flat,
                Font = new Font(AppTheme.FontFamily, 9.5F, FontStyle.Bold),
                Margin = new Padding(0, 0, 8, 0),
                Cursor = Cursors.Hand
            };
            if (secondary)
            {
                btn.BackColor = Color.White;
                btn.ForeColor = AppTheme.TextPrimary;
                btn.FlatAppearance.BorderColor = AppTheme.Border;
                btn.FlatAppearance.BorderSize = 1;
            }
            else
            {
                btn.BackColor = AppTheme.Primary;
                btn.ForeColor = Color.White;
                btn.FlatAppearance.BorderSize = 0;
            }
            return btn;
        }

        private static Panel CreateCard()
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.CardBackground,
                Padding = new Padding(4),
                Margin = new Padding(4)
            };
        }

        private static TextBox AddFilterField(TableLayoutPanel tlp, ref int row, string caption)
        {
            var lbl = new Label
            {
                Text = caption,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                ForeColor = AppTheme.TextSecondary,
                Margin = new Padding(0, row == 0 ? 0 : 2, 0, 0)
            };
            tlp.Controls.Add(lbl, 0, row++);

            var txt = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 2)
            };
            tlp.Controls.Add(txt, 0, row++);
            return txt;
        }

        private static ComboBox AddFilterFieldCombo(TableLayoutPanel tlp, ref int row, string caption)
        {
            var lbl = new Label
            {
                Text = caption,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                ForeColor = AppTheme.TextSecondary,
                Margin = new Padding(0, row == 0 ? 0 : 2, 0, 0)
            };
            tlp.Controls.Add(lbl, 0, row++);

            var cmb = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 2)
            };
            tlp.Controls.Add(cmb, 0, row++);
            return cmb;
        }

        private static Label CreatePreviewLabel(bool bold)
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font(AppTheme.FontFamily, bold ? 10.5F : 9F, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = bold ? AppTheme.TextPrimary : AppTheme.TextSecondary
            };
        }

        private void LoadFirmaCombo(int preferredFirmaId)
        {
            var tum = _firmaSvc.GetAll() ?? new List<Firma>();
            var liste = FirmaIsyeriYetkiHelper.FilterFirmalar(tum, _yetkiler, _isAdmin)
                .OrderBy(f => f.FirmaAdi)
                .ToList();

            cmbFirma.DataSource = null;
            cmbFirma.DisplayMember = nameof(Firma.FirmaAdi);
            cmbFirma.ValueMember = nameof(Firma.FirmaId);
            cmbFirma.DataSource = liste;
            cmbFirma.Enabled = _isAdmin || liste.Count > 1;

            if (liste.Any(x => x.FirmaId == preferredFirmaId))
                cmbFirma.SelectedValue = preferredFirmaId;
            else if (liste.Count > 0)
                cmbFirma.SelectedIndex = 0;
        }

        private void LoadIsyeriCombo(int firmaId, int? preferredIsyeriId)
        {
            var list = _lookup.GetIsyerleri(firmaId) ?? new List<LookupItem>();
            list = FirmaIsyeriYetkiHelper.FilterIsyeriLookup(list, firmaId, _yetkiler, _isAdmin);
            var data = new List<LookupItem> { new LookupItem { Id = 0, Ad = "Tümü" } };
            data.AddRange(list);

            cmbIsyeri.DataSource = null;
            cmbIsyeri.DisplayMember = nameof(LookupItem.Ad);
            cmbIsyeri.ValueMember = nameof(LookupItem.Id);
            cmbIsyeri.DataSource = data;

            if (preferredIsyeriId.HasValue && preferredIsyeriId.Value > 0
                && data.Any(x => x.Id == preferredIsyeriId.Value))
                cmbIsyeri.SelectedValue = preferredIsyeriId.Value;
            else
                cmbIsyeri.SelectedValue = 0;
        }

        private void LoadDetailLookups()
        {
            int firmaId = GetSeciliFirmaId();
            BindLookup(cmbDepartman, _lookup.GetDepartmanlar(firmaId));
            BindLookup(cmbPozisyon, _lookup.GetPozisyonlar(firmaId));
            BindLookup(cmbStatu, _lookup.GetCalismaStatuleri(firmaId));
        }

        private static void BindLookup(ComboBox cmb, List<LookupItem> list)
        {
            var data = new List<LookupItem> { new LookupItem { Id = 0, Ad = "Tümü" } };
            if (list != null)
                data.AddRange(list);

            cmb.DataSource = null;
            cmb.DisplayMember = nameof(LookupItem.Ad);
            cmb.ValueMember = nameof(LookupItem.Id);
            cmb.DataSource = data;
            cmb.SelectedIndex = 0;
        }

        private void ResetDetailFilters()
        {
            txtAdSoyadKart.Clear();
            txtSicil.Clear();
            txtTc.Clear();
            txtEmail.Clear();
            if (cmbDepartman.Items.Count > 0) cmbDepartman.SelectedIndex = 0;
            if (cmbPozisyon.Items.Count > 0) cmbPozisyon.SelectedIndex = 0;
            if (cmbStatu.Items.Count > 0) cmbStatu.SelectedIndex = 0;
            ClearPreview();
        }

        private void UpdatePuantajEnabled()
        {
            bool cikan = cmbCalismaDurumu.SelectedIndex == 1;
            cmbPuantaj.Enabled = !cikan;
        }

        private int GetSeciliFirmaId()
        {
            if (cmbFirma?.SelectedValue is int id)
                return id;
            if (cmbFirma?.SelectedValue != null && int.TryParse(cmbFirma.SelectedValue.ToString(), out var parsed))
                return parsed;
            return _session.AktifFirmaId ?? 0;
        }

        private int? GetSeciliIsyeriIdRaw()
        {
            if (cmbIsyeri?.SelectedValue is int id)
                return id <= 0 ? null : id;
            if (cmbIsyeri?.SelectedValue != null && int.TryParse(cmbIsyeri.SelectedValue.ToString(), out var parsed))
                return parsed <= 0 ? null : parsed;
            return null;
        }

        private bool GetSadeceIstenCikanMi() => cmbCalismaDurumu.SelectedIndex == 1;

        private bool? GetPuantajYapilanSecili()
        {
            if (GetSadeceIstenCikanMi()) return null;
            if (cmbPuantaj.SelectedIndex == 1) return false;
            return true;
        }

        private KisiSearchFilter BuildFilterFromUi()
        {
            int firmaId = GetSeciliFirmaId();
            bool sadeceIstenCikanlar = GetSadeceIstenCikanMi();
            bool? puantaj = GetPuantajYapilanSecili();
            var (isyeriId, isyeriIdIn) = FirmaIsyeriYetkiHelper.ResolveKisiQueryIsyeriFilter(
                firmaId, GetSeciliIsyeriIdRaw(), _yetkiler, _isAdmin);

            return new KisiSearchFilter
            {
                FirmaId = firmaId,
                PuantajYapilirMi = puantaj,
                IsyeriId = isyeriId,
                IsyeriIdIn = isyeriIdIn,
                SadeceIstenCikanlar = sadeceIstenCikanlar,
                AdSoyadKart = NullIfWhite(txtAdSoyadKart.Text),
                Sicil = NullIfWhite(txtSicil.Text),
                TcKimlikNo = NullIfWhite(txtTc.Text),
                Email = NullIfWhite(txtEmail.Text),
                DepartmanId = GetComboId(cmbDepartman),
                PozisyonId = GetComboId(cmbPozisyon),
                CalismaStatuId = GetComboId(cmbStatu)
            };
        }

        private KisiAraContext CaptureAppliedContext()
        {
            int firmaId = GetSeciliFirmaId();
            bool sadeceIstenCikanlar = GetSadeceIstenCikanMi();
            var (isyeriId, isyeriIdIn) = FirmaIsyeriYetkiHelper.ResolveKisiQueryIsyeriFilter(
                firmaId, GetSeciliIsyeriIdRaw(), _yetkiler, _isAdmin);

            return new KisiAraContext
            {
                FirmaId = firmaId,
                FirmaAdi = cmbFirma.Text?.Trim() ?? "",
                IsyeriId = isyeriId,
                IsyeriIdIn = isyeriIdIn,
                IsyeriAdi = cmbIsyeri.Text?.Trim() ?? "Tümü",
                SadeceIstenCikanlar = sadeceIstenCikanlar,
                PuantajYapilirMi = GetPuantajYapilanSecili(),
                CalismaDurumuMetni = cmbCalismaDurumu.Text?.Trim() ?? "",
                PuantajMetni = cmbPuantaj.Text?.Trim() ?? ""
            };
        }

        private static string NullIfWhite(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        private static int? GetComboId(ComboBox cmb)
        {
            if (cmb?.SelectedValue == null)
                return null;
            if (cmb.SelectedValue is int id)
                return id > 0 ? id : null;
            return int.TryParse(cmb.SelectedValue.ToString(), out var parsed) && parsed > 0 ? parsed : null;
        }

        private static void SelectByText(ComboBox cmb, string text)
        {
            if (cmb == null || string.IsNullOrWhiteSpace(text))
                return;
            for (int i = 0; i < cmb.Items.Count; i++)
            {
                if (string.Equals(cmb.Items[i]?.ToString(), text, StringComparison.OrdinalIgnoreCase))
                {
                    cmb.SelectedIndex = i;
                    return;
                }
            }
        }

        private void RunSearch(bool resetPage)
        {
            if (cmbFirma == null || cmbFirma.DataSource == null)
                return;

            if (resetPage)
                _page = 1;

            var filter = BuildFilterFromUi();
            var rows = _kisiQuery.SearchKisilerPaged(filter, _page, PageSize, out _totalCount)
                       ?? new List<KisiSearchResultItem>();

            dgvSonuc.DataSource = null;
            dgvSonuc.AutoGenerateColumns = true;
            dgvSonuc.DataSource = rows;
            ApplyGridHeaders();

            int maxPage = Math.Max(1, (int)Math.Ceiling(_totalCount / (double)PageSize));
            lblSayfalama.Text = $"Sayfa {_page} / {maxPage}  ·  Toplam {_totalCount}";
            btnOnceki.Enabled = _page > 1;
            btnSonraki.Enabled = _page < maxPage;

            if (rows.Count > 0)
                dgvSonuc.Rows[0].Selected = true;
            else
                ClearPreview();
        }

        private void ApplyGridHeaders()
        {
            void H(string col, string header)
            {
                if (dgvSonuc.Columns[col] != null)
                    dgvSonuc.Columns[col].HeaderText = header;
            }

            H("PersonelId", "Sicil No");
            H("AdSoyad", "Ad Soyad");
            H("KartNo", "Kart No");
            H("TcKimlikNo", "TC Kimlik No");
            H("IsyeriAdi", "İşyeri");
            H("DepartmanAdi", "Departman");
            H("PozisyonAdi", "Pozisyon");
        }

        private void DgvSonuc_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvSonuc.CurrentRow?.DataBoundItem is not KisiSearchResultItem row)
            {
                ClearPreview();
                return;
            }

            LoadPreview(row);
        }

        private void LoadPreview(KisiSearchResultItem row)
        {
            var (detay, _) = _kisiQuery.GetDetayOrPuantajsizKart(row.PersonelId);
            if (detay == null)
            {
                ClearPreview();
                return;
            }

            picOnizleme.Image = DbHelpers.BytesToImage(detay.Fotograf);

            var adSoyad = $"{detay.Ad} {detay.Soyad}".Trim();
            if (string.IsNullOrWhiteSpace(adSoyad))
                adSoyad = row.AdSoyad;
            lblOnizlemeAd.Text = string.IsNullOrWhiteSpace(adSoyad) ? "—" : adSoyad;
            lblOnizlemeTc.Text = string.IsNullOrWhiteSpace(detay.TcKimlikNo)
                ? "TC: —"
                : $"TC: {detay.TcKimlikNo}";

            int firmaId = GetSeciliFirmaId();
            var dept = _lookup.GetDepartmanlar(firmaId)?.FirstOrDefault(x => x.Id == detay.DepartmanId)?.Ad
                       ?? row.DepartmanAdi;
            var poz = _lookup.GetPozisyonlar(firmaId)?.FirstOrDefault(x => x.Id == detay.PozisyonId)?.Ad
                      ?? row.PozisyonAdi;
            var isy = _lookup.GetIsyerleri(firmaId)?.FirstOrDefault(x => x.Id == detay.IsyeriId)?.Ad
                      ?? row.IsyeriAdi;

            lblOnizlemeDepartman.Text = "Departman: " + (string.IsNullOrWhiteSpace(dept) ? "—" : dept);
            lblOnizlemePozisyon.Text = "Pozisyon: " + (string.IsNullOrWhiteSpace(poz) ? "—" : poz);
            lblOnizlemeStatu.Text = "Statü: " + (detay.CalismaStatusuText ?? "—");
            lblOnizlemeIsyeri.Text = "İşyeri: " + (string.IsNullOrWhiteSpace(isy) ? "—" : isy);
        }

        private void ClearPreview()
        {
            picOnizleme.Image = null;
            lblOnizlemeAd.Text = "Ad Soyad: —";
            lblOnizlemeTc.Text = "TC: —";
            lblOnizlemeDepartman.Text = "Departman: —";
            lblOnizlemePozisyon.Text = "Pozisyon: —";
            lblOnizlemeStatu.Text = "Statü: —";
            lblOnizlemeIsyeri.Text = "İşyeri: —";
        }

        private void TrySelectAndClose()
        {
            if (dgvSonuc.CurrentRow?.DataBoundItem is not KisiSearchResultItem row)
            {
                MessageBox.Show("Lütfen listeden bir personel seçin.", "Personel Ara", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SelectedPersonelId = row.PersonelId;
            AppliedContext = CaptureAppliedContext();
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
