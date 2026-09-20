using CeyPASS.Business.Abstractions;
using CeyPASS.Business.Services;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using CeyPASS.WFA.UserControls.Canlı_İzleme;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CeyPASS.WFA.Forms
{
    /// <summary>Canlı İzleme ana ekranı — son geçiş kartları, hareket listesi ve kart atama yönetimi.</summary>
    public partial class canliIzlemeVeriEkrani : Form
    {
        private readonly ISessionContext _session;
        private readonly ICanliIzlemeService _svc;
        private readonly IKisiHareketService _khsvc;
        private readonly IMisafirKartService _mSvc;
        private readonly IAracKartiService _aracSvc;
        private readonly ICanliIzlemeKartKomutService _kartKomutSvc;
        private ComboBox cmbAtamaTip;
        private DataGridView dgAtama;
        private System.Windows.Forms.Timer anlikVeriTrafigi;
        private List<KisiKartKontrolu> kartlar = new List<KisiKartKontrolu>();
        private int? seciliKisiIdManuel = null;

        /// <summary>Rol bazlı layout ve 1 sn periyotlu yenileme timer'ını hazırlar.</summary>
        public canliIzlemeVeriEkrani(ISessionContext session, ICanliIzlemeService svc, IKisiHareketService khsvc, IKisiDetayService kisiDetaysvc, IMisafirKartService msvc, IAracKartiService aracSvc, ICanliIzlemeKartKomutService kartKomutSvc)
        {
            InitializeComponent();
            _session = session;
            _svc = svc;
            _khsvc = khsvc;
            _ = kisiDetaysvc; // Seçili kişi paneli kaldırıldı; DI imzası korundu
            _mSvc = msvc;
            _aracSvc = aracSvc;
            _kartKomutSvc = kartKomutSvc;
        }
        private void canliIzlemeVeriEkrani_Load(object sender, EventArgs e)
        {
            ApplyTheme();
            ApplyRoleLayout();
            SonGecenKartlariHazirla();
            SonGecenVerileriYukle();
            if (CanliIzlemeRoleHelper.ShowHareketListesi(_session?.RolAdi))
            {
                DataGridViewAyarla();
                SonHareketleriYukle();
            }
            if (!CanliIzlemeRoleHelper.HideKartAtama(_session?.RolAdi))
                YukleAtamaListe();
            // İş kuralı: son geçiş ve atama listesi ~1 sn'de bir yenilenir.
            anlikVeriTrafigi = new System.Windows.Forms.Timer();
            anlikVeriTrafigi.Interval = 1000;
            anlikVeriTrafigi.Tick += (s, ev) => SonGecenVerileriYukle();
            anlikVeriTrafigi.Tick += (s, ev) =>
            {
                if (CanliIzlemeRoleHelper.ShowHareketListesi(_session?.RolAdi))
                    SonHareketleriYukle();
                if (!CanliIzlemeRoleHelper.HideKartAtama(_session?.RolAdi))
                    YukleAtamaListe();
            };
            anlikVeriTrafigi.Start();
        }

        private void ApplyRoleLayout()
        {
            var showList = CanliIzlemeRoleHelper.ShowHareketListesi(_session?.RolAdi);
            var hideKart = CanliIzlemeRoleHelper.HideKartAtama(_session?.RolAdi);

            tableLayoutPanel2.Visible = showList;
            tableLayoutPanel4.Visible = !hideKart;
            kisiyeKartiAta.Visible = false;
            atananKartiGuncelle.Visible = false;
            aracKartiVer.Visible = false;
            aracKartiGuncelle.Visible = false;
            if (!hideKart)
                EnsureAtamaGrid();

            if (!showList && hideKart)
            {
                tableLayoutPanel1.RowStyles[0].SizeType = SizeType.Percent;
                tableLayoutPanel1.RowStyles[0].Height = 100F;
                tableLayoutPanel1.RowStyles[1].SizeType = SizeType.Absolute;
                tableLayoutPanel1.RowStyles[1].Height = 0F;
                tableLayoutPanel1.RowStyles[2].SizeType = SizeType.Absolute;
                tableLayoutPanel1.RowStyles[2].Height = 0F;
            }
            else if (showList && !hideKart)
            {
                tableLayoutPanel1.RowStyles[0].SizeType = SizeType.Percent;
                tableLayoutPanel1.RowStyles[0].Height = 42F;
                tableLayoutPanel1.RowStyles[1].SizeType = SizeType.Percent;
                tableLayoutPanel1.RowStyles[1].Height = 28F;
                tableLayoutPanel1.RowStyles[2].SizeType = SizeType.Percent;
                tableLayoutPanel1.RowStyles[2].Height = 30F;
            }
            else if (!showList && !hideKart)
            {
                tableLayoutPanel1.RowStyles[2].SizeType = SizeType.Percent;
                tableLayoutPanel1.RowStyles[2].Height = 40F;
            }
        }

        private void EnsureAtamaGrid()
        {
            if (dgAtama != null) return;
            flpKartButonlari.Visible = false;
            tableLayoutPanel4.Controls.Clear();
            tableLayoutPanel4.ColumnCount = 1;
            tableLayoutPanel4.RowCount = 2;
            tableLayoutPanel4.RowStyles.Clear();
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var bar = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Padding = new Padding(4, 4, 4, 0) };
            bar.Controls.Add(new Label { Text = "Kart Atamaları", AutoSize = true, Font = new Font(Font, FontStyle.Bold), Padding = new Padding(0, 6, 4, 0) });
            var btnYardim = new Button { Text = "?", Width = 28, Height = 26, Margin = new Padding(0, 2, 8, 0) };
            // İş kuralı: ? butonu HAZIR/ATANMIŞ/GİRİŞ/ÇIKIŞ ve KartKomut (kısıt) ayrımını açıklar.
            btnYardim.Click += (_, __) => MessageBox.Show(KartAtamaYardimMetni, "Kart Atamaları — durumlar", MessageBoxButtons.OK, MessageBoxIcon.Information);
            bar.Controls.Add(btnYardim);
            cmbAtamaTip = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160 };
            cmbAtamaTip.Items.AddRange(new object[] { "Misafir / Ziyaretçi", "Araç" });
            cmbAtamaTip.SelectedIndex = 0;
            cmbAtamaTip.SelectedIndexChanged += (_, __) => YukleAtamaListe();
            bar.Controls.Add(cmbAtamaTip);
            var btnDurum = new Button { Text = "Kart durumları", AutoSize = true, Margin = new Padding(8, 2, 0, 0) };
            btnDurum.Click += (_, __) =>
            {
                if (KartDurumTopluForm.Show(this, _session, _mSvc, _aracSvc, _kartKomutSvc))
                    YukleAtamaListe();
            };
            bar.Controls.Add(btnDurum);

            dgAtama = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = false,
                RowHeadersVisible = false,
                MultiSelect = false
            };
            dgAtama.Columns.Add(new DataGridViewTextBoxColumn { Name = "Kart", HeaderText = "Kart", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgAtama.Columns.Add(new DataGridViewTextBoxColumn { Name = "Kisi", HeaderText = "Kişi / Plaka", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgAtama.Columns.Add(new DataGridViewTextBoxColumn { Name = "Durum", HeaderText = "Durum", Width = 90 });
            var colKisit = new DataGridViewButtonColumn { Name = "Kisit", HeaderText = "", Text = "Kartı Kısıtla", UseColumnTextForButtonValue = true, Width = 110 };
            var colSerbest = new DataGridViewButtonColumn { Name = "Serbest", HeaderText = "", Text = "Kart Kısıtı Kaldır", UseColumnTextForButtonValue = true, Width = 130 };
            dgAtama.Columns.Add(colKisit);
            dgAtama.Columns.Add(colSerbest);
            dgAtama.Columns.Add(new DataGridViewTextBoxColumn { Name = "Pid", Visible = false });
            dgAtama.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tip", Visible = false });
            dgAtama.Columns.Add(new DataGridViewTextBoxColumn { Name = "AtamaId", Visible = false });
            dgAtama.Columns.Add(new DataGridViewTextBoxColumn { Name = "DurumKey", Visible = false });
            dgAtama.CellContentClick += DgAtama_CellContentClick;
            dgAtama.CellDoubleClick += DgAtama_CellDoubleClick;
            dgAtama.CellFormatting += DgAtama_CellFormatting;

            tableLayoutPanel4.Controls.Add(bar, 0, 0);
            tableLayoutPanel4.Controls.Add(dgAtama, 0, 1);
        }

        private const string KartAtamaYardimMetni =
            "HAZIR: Kart boşta; kimseye verilmemiş. Çift tıklayınca yeni atama açılır.\n" +
            "ATANMIŞ: Kart birine verilmiş; henüz turnikede giriş/çıkış yok.\n" +
            "GİRİŞ: Atama sonrası son hareket giriş (içeride).\n" +
            "ÇIKIŞ: Atama sonrası son hareket çıkış.\n\n" +
            "ATANMIŞ / GİRİŞ / ÇIKIŞ satırına çift tık → atamayı güncelleyin (Kartı Kısıtla’dan bağımsızdır).\n" +
            "Satırın sağındaki Kartı Kısıtla / Kart Kısıtı Kaldır: cihaz kuyruğuna komut yazar; atamayı değiştirmez.\n" +
            "Kısıt kanıtı yoksa kart serbest sayılır. Atanmış+serbest → Kısıtla; kısıtlı → Kısıtı Kaldır; HAZIR+serbest → ikisi kapalı.\n" +
            "Kart durumları: misafir+araç anlık serbest/kısıtlı listesi (Tümü/Misafir/Araç); seçerek veya topluca yönetin.\n" +
            "Üstteki Misafir / Araç seçimi liste tipini değiştirir.";

        /// <summary>dgAtama Durum sütunu: HAZIR / ATANMIŞ / GİRİŞ / ÇIKIŞ renk kodları.</summary>
        private void DgAtama_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || dgAtama.Columns[e.ColumnIndex].Name != "Durum")
                return;
            var text = Convert.ToString(e.Value);
            var color = text switch
            {
                "HAZIR" => Color.FromArgb(0xE6, 0x7E, 0x22),
                "ATANMIŞ" => Color.FromArgb(0x47, 0x69, 0x8A),
                "GİRİŞ" => Color.FromArgb(0x2E, 0x8B, 0x57),
                "ÇIKIŞ" => Color.FromArgb(0xB2, 0x22, 0x22),
                _ => dgAtama.DefaultCellStyle.ForeColor
            };
            e.CellStyle.ForeColor = color;
            e.CellStyle.SelectionForeColor = color;
        }

        private void YukleAtamaListe()
        {
            if (dgAtama == null || !_session.AktifFirmaId.HasValue) return;
            var tip = cmbAtamaTip != null && cmbAtamaTip.SelectedIndex == 1 ? "arac" : "misafir";
            var list = KartAtamaListePresenter.Load(_mSvc, _aracSvc, _kartKomutSvc, _session.AktifFirmaId.Value, tip);
            var selected = dgAtama.CurrentRow?.Cells["Pid"].Value?.ToString();
            dgAtama.Rows.Clear();
            foreach (var r in list)
            {
                var i = dgAtama.Rows.Add(r.KartAdi, r.KisiPlaka, r.DurumText, r.CanKisitla ? "Kartı Kısıtla" : "", r.CanSerbestBirak ? "Kart Kısıtı Kaldır" : "", r.PersonelId, r.Tip, r.AtamaId?.ToString() ?? "", r.Durum);
                dgAtama.Rows[i].Tag = r;
            }
            if (!string.IsNullOrEmpty(selected))
            {
                foreach (DataGridViewRow row in dgAtama.Rows)
                {
                    if (string.Equals(Convert.ToString(row.Cells["Pid"].Value), selected, StringComparison.OrdinalIgnoreCase))
                    {
                        row.Selected = true;
                        break;
                    }
                }
            }
        }

        private void DgAtama_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgAtama.Rows[e.RowIndex].Tag is not KartAtamaListeSatir row) return;
            var name = dgAtama.Columns[e.ColumnIndex].Name;
            if (name == "Kisit" && row.CanKisitla)
                EnqueueKomut(row, true);
            else if (name == "Serbest" && row.CanSerbestBirak)
                EnqueueKomut(row, false);
        }

        private void DgAtama_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgAtama.Rows[e.RowIndex].Tag is not KartAtamaListeSatir row) return;
            if (dgAtama.Columns[e.ColumnIndex].Name == "Kisit" || dgAtama.Columns[e.ColumnIndex].Name == "Serbest")
                return;
            var firmaId = (int)_session.AktifFirmaId;
            if (row.Durum == nameof(KartAtamaListeDurum.Hazir))
            {
                if (row.Tip == "arac")
                {
                    var uc = new aracKartiAtama(_session, _aracSvc);
                    uc.InitYeni(firmaId, row.PersonelId);
                    ShowUserControlInDialog(uc, "Araç Kartı Ver", 1080, 600, this);
                }
                else
                {
                    var uc = new misafirKartAtama(_session, _mSvc);
                    uc.InitYeni(firmaId, row.PersonelId);
                    ShowUserControlInDialog(uc, "Misafir Kart Atama - Yeni", 1080, 600, this);
                }
            }
            else
            {
                if (row.Tip == "arac")
                {
                    var uc = new aracKartiAtama(_session, _aracSvc);
                    uc.InitGuncelleme(firmaId, DateTime.Now, row.AtamaId);
                    ShowUserControlInDialog(uc, "Verilen Araç Kartını Güncelle", 720, 600, this);
                }
                else
                {
                    var uc = new misafirKartAtama(_session, _mSvc);
                    uc.InitGuncelleme(firmaId, DateTime.Now, row.AtamaId);
                    ShowUserControlInDialog(uc, "Misafir Kart Atama - Güncelleme", 720, 600, this);
                }
            }
            YukleAtamaListe();
        }

        private void EnqueueKomut(KartAtamaListeSatir row, bool pasif)
        {
            if (!_session.AktifFirmaId.HasValue) return;
            var baslik = pasif ? "Kartı Kısıtla" : "Kart Kısıtı Kaldır";
            var onay = pasif
                ? "“" + row.KartAdi + "” kartı cihazlarda kısıtlansın mı?"
                : "“" + row.KartAdi + "” kartındaki kısıt kaldırılsın mı?";
            if (MessageBox.Show(this, onay, baslik, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            try
            {
                if (pasif)
                    _kartKomutSvc.EnqueuePasif(_session.AktifFirmaId.Value, row.PersonelId, _session.AktifKullaniciId);
                else
                    _kartKomutSvc.EnqueueAktif(_session.AktifFirmaId.Value, row.PersonelId, _session.AktifKullaniciId);
                YukleAtamaListe();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, baslik, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ApplyTheme()
        {
            BackColor = AppTheme.ContentBackground;
            flpSonGecenler.BackColor = AppTheme.CardBackground;
            StyleKartButton(kisiyeKartiAta, AppTheme.Primary);
            StyleKartButton(atananKartiGuncelle, AppTheme.Primary);
            StyleKartButton(aracKartiVer, AppTheme.VehicleAction);
            StyleKartButton(aracKartiGuncelle, AppTheme.VehicleAction);
        }

        private static void StyleKartButton(Button btn, Color backColor)
        {
            if (btn == null) return;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.UseVisualStyleBackColor = false;
            btn.BackColor = backColor;
            btn.ForeColor = Color.White;
            btn.Font = new Font(AppTheme.FontFamily, 10F, FontStyle.Regular);
        }
        private void canliIzlemeVeriEkrani_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
        private void SonGecenKartlariHazirla()
        {
            kartlar.Clear();
            flpSonGecenler.Controls.Clear();
            flpSonGecenler.WrapContents = false;
            flpSonGecenler.Resize -= FlpSonGecenler_Resize;
            flpSonGecenler.Resize += FlpSonGecenler_Resize;
            for (int i = 0; i < 4; i++)
            {
                KisiKartKontrolu kart = new KisiKartKontrolu();
                kart.Margin = new Padding(8);
                flpSonGecenler.Controls.Add(kart);
                kartlar.Add(kart);
            }
            ApplyCardSizes();
        }

        private void FlpSonGecenler_Resize(object sender, EventArgs e) => ApplyCardSizes();

        private void ApplyCardSizes()
        {
            if (kartlar.Count == 0 || flpSonGecenler.ClientSize.Width < 40 || flpSonGecenler.ClientSize.Height < 40)
                return;

            int margin = 8;
            int availableW = flpSonGecenler.ClientSize.Width - margin * 2;
            int availableH = flpSonGecenler.ClientSize.Height - margin * 2;
            int cardW = Math.Max(120, (availableW / 4) - margin * 2);
            int cardH = Math.Max(180, availableH - margin);

            foreach (var kart in kartlar)
            {
                kart.Margin = new Padding(margin);
                kart.Size = new Size(cardW, cardH);
            }
        }
        private void SonGecenVerileriYukle()
        {
            try
            {
                var rol = _session?.RolAdi;
                List<LastPassDTO> lastPasses;
                if (CanliIzlemeRoleHelper.IsArac(rol))
                    lastPasses = _svc.GetLastPassesArac((int)_session.AktifFirmaId, 4);
                else if (CanliIzlemeRoleHelper.IsYemekhane(rol))
                    lastPasses = _svc.GetLastPassesYemekhane((int)_session.AktifFirmaId, 4);
                else
                    lastPasses = _svc.GetLastPasses((int)_session.AktifFirmaId, 4);

                for (int i = 0; i < lastPasses.Count && i < kartlar.Count; i++)
                {
                    LastPassDTO p = lastPasses[i];

                    Image foto = Properties.Resources.Unknown_person;
                    if (p.Foto != null && p.Foto.Length > 0)
                    {
                        using (var ms = new MemoryStream(p.Foto))
                        using (var tmp = Image.FromStream(ms))
                        {
                            foto = (Image)tmp.Clone();
                        }
                    }

                    kartlar[i].Ayarla(
                        foto,
                        p.AdSoyad,
                        p.IsyeriAdi,
                        p.Unvan,
                        p.Zaman,
                        p.TerminalAdi,
                        p.GirisMi
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Son geçiş bilgileri alınamadı: " + ex.Message);
            }
        }
        private void SonHareketleriYukle()
        {
            if (!CanliIzlemeRoleHelper.ShowHareketListesi(_session?.RolAdi))
                return;

            if (!_session.AktifFirmaId.HasValue)
            {
                MessageBox.Show("Aktif firma bilgisi bulunamadı. Lütfen tekrar giriş yapın.");
                return;
            }

            int? seciliKisiId = null;
            if (dgSonHareketler.CurrentRow != null &&
                dgSonHareketler.CurrentRow.Cells["KisiId"] != null &&
                dgSonHareketler.CurrentRow.Cells["KisiId"].Value != null &&
                dgSonHareketler.CurrentRow.Cells["KisiId"].Value != DBNull.Value)
            {
                seciliKisiId = Convert.ToInt32(dgSonHareketler.CurrentRow.Cells["KisiId"].Value);
            }

            var rol = _session?.RolAdi;
            List<KisiHareketDTO> list;
            if (CanliIzlemeRoleHelper.IsArac(rol) && !CanliIzlemeRoleHelper.IsDanisma(rol))
                list = _khsvc.GetLastMovesByFirmaArac(15, _session.AktifFirmaId.Value);
            else if (CanliIzlemeRoleHelper.IsYemekhane(rol) && !CanliIzlemeRoleHelper.IsDanisma(rol))
                list = _khsvc.GetLastMovesByFirmaYemekhane(15, _session.AktifFirmaId.Value);
            else
                list = _khsvc.GetLastMovesByFirma(15, _session.AktifFirmaId.Value);


            var table = new DataTable();
            table.Columns.Add("Tarih", typeof(DateTime));
            table.Columns.Add("Ad Soyad", typeof(string));
            table.Columns.Add("Turnike", typeof(string));
            table.Columns.Add("KisiId", typeof(int));

            foreach (var x in list)
            {
                table.Rows.Add(x.Tarih, x.AdSoyad, x.CihazAdi, x.PersonelId);
            }

            dgSonHareketler.AutoGenerateColumns = true;
            dgSonHareketler.DataSource = table;

            if (dgSonHareketler.Columns.Contains("KisiId"))
                dgSonHareketler.Columns["KisiId"].Visible = false;

            dgSonHareketler.CurrentCell = null;

            int? hedefId = seciliKisiIdManuel ?? seciliKisiId;
            if (!hedefId.HasValue)
            {
                return;
            }

            foreach (DataGridViewRow row in dgSonHareketler.Rows)
            {
                var cell = row.Cells["KisiId"];

                if (cell == null || cell.Value == null || cell.Value == DBNull.Value)
                    continue;

                if (!int.TryParse(cell.Value.ToString(), out int kisiId))
                    continue;

                if (kisiId == hedefId.Value)
                {
                    row.Selected = true;
                    dgSonHareketler.CurrentCell = row.Cells[0];
                    break;
                }
            }
        }
        private void DataGridViewAyarla()
        {
            var dgv = dgSonHareketler;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkSlateGray;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgv.DefaultCellStyle.SelectionBackColor = Color.LightSteelBlue;
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.RowHeadersVisible = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.ReadOnly = true;
        }
        private void dgSonHareketler_SelectionChanged(object sender, EventArgs e)
        {
            // Seçili kişi detay paneli kaldırıldı; ileride başka içerik eklenecek.
        }
        private void dgSonHareketler_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                seciliKisiIdManuel = Convert.ToInt32(dgSonHareketler.Rows[e.RowIndex].Cells["KisiId"].Value);
            }
        }
        private void kisiyeKartiAta_Click(object sender, EventArgs e)
        {
            var uc = new misafirKartAtama(_session, _mSvc);
            uc.InitYeni((int)_session.AktifFirmaId);
            ShowUserControlInDialog(uc, "Misafir Kart Atama - Yeni", 1080, 600, this);
        }
        private void atananKartiGuncelle_Click(object sender, EventArgs e)
        {
            var uc = new misafirKartAtama(_session, _mSvc);
            uc.InitGuncelleme((int)_session.AktifFirmaId, DateTime.Now);
            ShowUserControlInDialog(uc, "Misafir Kart Atama - Güncelleme", 720, 600, this);
        }
        private void aracKartiVer_Click(object sender, EventArgs e)
        {
            var uc = new aracKartiAtama(_session, _aracSvc);
            uc.InitYeni((int)_session.AktifFirmaId);
            ShowUserControlInDialog(uc, "Araç Kartı Ver", 1080, 600, this);
        }
        private void aracKartiGuncelle_Click(object sender, EventArgs e)
        {
            var uc = new aracKartiAtama(_session, _aracSvc);
            uc.InitGuncelleme((int)_session.AktifFirmaId, DateTime.Now);
            ShowUserControlInDialog(uc, "Verilen Araç Kartını Güncelle", 720, 600, this);
        }
        private void ShowUserControlInDialog(UserControl uc, string baslik, int width, int height, IWin32Window owner = null)
        {
            using (var host = new Form())
            {
                host.Text = baslik;
                host.StartPosition = FormStartPosition.CenterParent;
                host.FormBorderStyle = FormBorderStyle.FixedDialog;
                host.MaximizeBox = false;
                host.MinimizeBox = false;
                host.ClientSize = new System.Drawing.Size(width, height);

                uc.Dock = DockStyle.Fill;
                host.Controls.Add(uc);
                host.ShowDialog(owner);
            }
        }
    }
}
