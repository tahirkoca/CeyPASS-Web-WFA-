using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CeyPASS.WFA.Forms
{
    /// <summary>Aynı TC altında çoklu sicil bağlantıları ve puantaj aktarımı yönetimi.</summary>
    public sealed class frmCokluSicilEslestirme : Form
    {
        private readonly ICokluSicilService _svc;
        private readonly ISessionContext _session;
        private readonly IAuthorizationService _auth;
        private readonly int _anaPersonelId;
        private readonly string _tcKimlikNo;
        private readonly string _adSoyad;

        private DataGridView _grid;
        private ComboBox _cmbHedef;
        private NumericUpDown _nudAktarimGun;
        private TextBox _txtAciklama;
        private CheckBox _chkAktif;
        private Label _lblError;
        private List<CokluSicilBaglantiDTO> _baglantilar = new();
        private List<CokluSicilHedefAdayDTO> _adaylar = new();

        /// <summary>Ana personel kimliği ile modal formu oluşturur ve listeleri yükler.</summary>
        public frmCokluSicilEslestirme(
            ICokluSicilService svc,
            ISessionContext session,
            IAuthorizationService auth,
            int anaPersonelId,
            string tcKimlikNo,
            string adSoyad)
        {
            _svc = svc;
            _session = session;
            _auth = auth;
            _anaPersonelId = anaPersonelId;
            _tcKimlikNo = tcKimlikNo?.Trim() ?? "";
            _adSoyad = adSoyad?.Trim() ?? "";

            Text = $"Çoklu Sicil Eşleştirmeleri — {_adSoyad} (Sicil {_anaPersonelId})";
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 10F);
            Size = new Size(920, 620);
            MinimizeBox = false;
            MaximizeBox = true;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.Sizable;

            BuildUi();
            LoadData();
        }

        private void BuildUi()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(12)
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var lblInfo = new Label
            {
                AutoSize = true,
                Text = "Ana sicile bağlı hedef sicilleri tek tek ekleyin, güncelleyin veya pasifleştirin.",
                ForeColor = Color.Gray,
                Margin = new Padding(0, 0, 0, 8)
            };
            root.Controls.Add(lblInfo, 0, 0);

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            _grid.Columns.Add("HedefPersonelId", "Hedef Sicil");
            _grid.Columns.Add("HedefAdSoyad", "Ad Soyad");
            _grid.Columns.Add("FirmaIsyeri", "Firma / İşyeri");
            _grid.Columns.Add("AktarimGunSayisi", "Aktarım Gün");
            _grid.Columns.Add("GirisCikis", "Giriş — Çıkış");
            _grid.Columns.Add("AktifText", "Aktif");
            _grid.SelectionChanged += Grid_SelectionChanged;
            root.Controls.Add(_grid, 0, 1);

            var editor = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                AutoSize = true,
                Padding = new Padding(0, 10, 0, 0)
            };
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            _cmbHedef = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _nudAktarimGun = new NumericUpDown { Minimum = 1, Maximum = 31, Value = 1, Dock = DockStyle.Fill };
            _txtAciklama = new TextBox { Dock = DockStyle.Fill };
            _chkAktif = new CheckBox { Text = "Aktif", Checked = true, AutoSize = true, Anchor = AnchorStyles.Left };

            editor.Controls.Add(Wrap("Hedef Sicil", _cmbHedef), 0, 0);
            editor.Controls.Add(Wrap("Aktarım Gün", _nudAktarimGun), 1, 0);
            editor.Controls.Add(Wrap("Açıklama", _txtAciklama), 2, 0);
            editor.SetColumnSpan(editor.Controls[editor.Controls.Count - 1], 2);
            editor.Controls.Add(_chkAktif, 4, 0);

            var btnPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Dock = DockStyle.Fill,
                WrapContents = false
            };
            var btnKaydet = new Button { Text = "Kaydet", AutoSize = true, Padding = new Padding(12, 6, 12, 6) };
            var btnPasif = new Button { Text = "Pasifleştir", AutoSize = true, Padding = new Padding(12, 6, 12, 6) };
            var btnAktif = new Button { Text = "Aktifleştir", AutoSize = true, Padding = new Padding(12, 6, 12, 6) };
            var btnTemizle = new Button { Text = "Temizle", AutoSize = true, Padding = new Padding(12, 6, 12, 6) };
            var btnKapat = new Button { Text = "Kapat", AutoSize = true, Padding = new Padding(12, 6, 12, 6), DialogResult = DialogResult.Cancel };
            btnKaydet.Click += (_, __) => Save();
            btnPasif.Click += (_, __) => SetAktif(false);
            btnAktif.Click += (_, __) => SetAktif(true);
            btnTemizle.Click += (_, __) => ClearEditor();
            var btnPasifTumunu = new Button { Text = "Tümünü pasifleştir", AutoSize = true, Padding = new Padding(12, 6, 12, 6), ForeColor = Color.Firebrick };
            btnPasifTumunu.Click += (_, __) => PasiflestirTumunu();
            btnPanel.Controls.AddRange(new Control[] { btnKaydet, btnPasif, btnAktif, btnTemizle, btnPasifTumunu, btnKapat });
            editor.Controls.Add(btnPanel, 0, 1);
            editor.SetColumnSpan(btnPanel, 5);

            _lblError = new Label { AutoSize = true, ForeColor = Color.Firebrick, Dock = DockStyle.Top, MaximumSize = new Size(860, 0) };
            var bottom = new Panel { Dock = DockStyle.Fill, AutoSize = true };
            bottom.Controls.Add(_lblError);
            bottom.Controls.Add(editor);
            editor.Top = _lblError.Bottom + 4;
            root.Controls.Add(bottom, 0, 2);

            Controls.Add(root);

            var canEdit = _auth.Can("Personeller", YetkiTipleri.Update);
            btnKaydet.Enabled = canEdit;
            btnPasif.Enabled = canEdit;
            btnAktif.Enabled = canEdit;
            btnPasifTumunu.Enabled = canEdit;
        }

        private void PasiflestirTumunu()
        {
            _lblError.Text = "";
            var aktifSayisi = _baglantilar.Count(b => b.AktifMi);
            if (aktifSayisi <= 0) return;
            if (MessageBox.Show(
                    $"{aktifSayisi} aktif hedef bağlantı pasifleştirilecek. Devam edilsin mi?",
                    "Çoklu Sicil",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                _svc.PasifleştirTümünü(_anaPersonelId, _session.AktifKullaniciId);
                LoadData();
                ClearEditor();
            }
            catch (Exception ex)
            {
                _lblError.Text = ex.Message;
            }
        }

        private static Panel Wrap(string label, Control child)
        {
            var p = new Panel { Dock = DockStyle.Fill, Height = 56, Margin = new Padding(0, 0, 8, 0) };
            var lbl = new Label { Text = label, Dock = DockStyle.Top, Height = 20, ForeColor = Color.Gray };
            child.Top = 22;
            child.Height = 28;
            p.Controls.Add(child);
            p.Controls.Add(lbl);
            return p;
        }

        private void LoadData()
        {
            _baglantilar = _svc.GetByAnaPersonelId(_anaPersonelId);
            _adaylar = _svc.GetHedefAdaylari(_anaPersonelId, _tcKimlikNo);

            _grid.Rows.Clear();
            foreach (var b in _baglantilar)
            {
                _grid.Rows.Add(
                    b.HedefPersonelId,
                    b.HedefAdSoyad ?? "",
                    $"{b.FirmaAdi ?? "-"} / {b.IsyeriAdi ?? "-"}",
                    b.AktarimGunSayisi,
                    FormatGirisCikis(b.IseGirisTarihi, b.IstenCikisTarihi),
                    b.AktifMi ? "Evet" : "Hayır");
            }

            _cmbHedef.DataSource = null;
            _cmbHedef.DisplayMember = nameof(HedefComboItem.Display);
            _cmbHedef.ValueMember = nameof(HedefComboItem.PersonelId);
            _cmbHedef.DataSource = _adaylar.Select(a =>
            {
                var engel = !a.SecilebilirMi && !a.ZatenBagli ? $" [{a.EngelMesaji}]" : "";
                return new HedefComboItem
                {
                    PersonelId = a.PersonelId,
                    Display = $"{a.PersonelId} — {a.AdSoyad} ({a.FirmaAdi ?? "-"}){engel}",
                    Source = a
                };
            }).ToList();
        }

        private void Grid_SelectionChanged(object? sender, EventArgs e)
        {
            if (_grid.CurrentRow == null) return;
            var hedefId = Convert.ToInt32(_grid.CurrentRow.Cells[0].Value);
            var bag = _baglantilar.FirstOrDefault(x => x.HedefPersonelId == hedefId);
            if (bag == null) return;

            SelectHedef(hedefId);
            _nudAktarimGun.Value = Math.Max(1, bag.AktarimGunSayisi);
            _txtAciklama.Text = bag.Aciklama ?? "";
            _chkAktif.Checked = bag.AktifMi;
        }

        private void SelectHedef(int hedefId)
        {
            for (int i = 0; i < _cmbHedef.Items.Count; i++)
            {
                if (_cmbHedef.Items[i] is HedefComboItem item && item.PersonelId == hedefId)
                {
                    _cmbHedef.SelectedIndex = i;
                    return;
                }
            }
        }

        private void Save()
        {
            _lblError.Text = "";
            if (_cmbHedef.SelectedItem is not HedefComboItem sel)
            {
                _lblError.Text = "Hedef sicil seçiniz.";
                return;
            }
            if (!sel.Source.SecilebilirMi && _baglantilar.All(b => b.HedefPersonelId != sel.PersonelId))
            {
                _lblError.Text = sel.Source.EngelMesaji ?? "Bu hedef sicil seçilemez.";
                return;
            }

            try
            {
                var aday = sel.Source;
                _svc.Upsert(_anaPersonelId, _tcKimlikNo, new CokluSicilUpsertRequest
                {
                    HedefPersonelId = sel.PersonelId,
                    FirmaId = aday.FirmaId,
                    SirketId = aday.IsyeriId,
                    BolumId = aday.BolumId,
                    AktarimGunSayisi = (int)_nudAktarimGun.Value,
                    Aciklama = _txtAciklama.Text,
                    AktifMi = _chkAktif.Checked
                }, _session.AktifKullaniciId);

                LoadData();
                ClearEditor();
            }
            catch (Exception ex)
            {
                _lblError.Text = ex.Message;
            }
        }

        private void SetAktif(bool aktif)
        {
            _lblError.Text = "";
            if (_grid.CurrentRow == null)
            {
                _lblError.Text = "Listeden bir satır seçiniz.";
                return;
            }

            try
            {
                var hedefId = Convert.ToInt32(_grid.CurrentRow.Cells[0].Value);
                _svc.SetAktif(_anaPersonelId, hedefId, aktif, _session.AktifKullaniciId);
                LoadData();
                ClearEditor();
            }
            catch (Exception ex)
            {
                _lblError.Text = ex.Message;
            }
        }

        private void ClearEditor()
        {
            _grid.ClearSelection();
            _cmbHedef.SelectedIndex = -1;
            _nudAktarimGun.Value = 1;
            _txtAciklama.Clear();
            _chkAktif.Checked = true;
            _lblError.Text = "";
        }

        private static string FormatGirisCikis(DateTime? giris, DateTime? cikis)
        {
            var g = giris?.ToString("dd.MM.yyyy") ?? "-";
            var c = cikis?.ToString("dd.MM.yyyy") ?? "-";
            return $"{g} — {c}";
        }

        private sealed class HedefComboItem
        {
            public int PersonelId { get; set; }
            public string Display { get; set; } = "";
            public CokluSicilHedefAdayDTO Source { get; set; } = null!;
        }
    }
}
