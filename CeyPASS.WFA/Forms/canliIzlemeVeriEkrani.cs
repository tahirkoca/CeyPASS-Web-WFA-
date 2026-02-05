using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.WFA.UserControls.Canlı_İzleme;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CeyPASS.WFA.Forms
{
    public partial class canliIzlemeVeriEkrani : Form
    {
        private readonly ISessionContext _session;
        private readonly ICanliIzlemeService _svc;
        private readonly IKisiHareketService _khsvc;
        private readonly IKisiDetayService _kisiDetaySvc;
        private readonly IMisafirKartService _mSvc;
        private System.Windows.Forms.Timer anlikVeriTrafigi;
        private List<KisiKartKontrolu> kartlar = new List<KisiKartKontrolu>();
        private int? seciliKisiIdManuel = null;

        public canliIzlemeVeriEkrani(ISessionContext session, ICanliIzlemeService svc, IKisiHareketService khsvc, IKisiDetayService kisiDetaysvc, IMisafirKartService msvc)
        {
            InitializeComponent();
            _session = session;
            _svc = svc;
            _khsvc = khsvc;
            _kisiDetaySvc = kisiDetaysvc;
            _mSvc = msvc;
        }
        private void canliIzlemeVeriEkrani_Load(object sender, EventArgs e)
        {
            ApplyTheme();
            if (IsYemekhaneRole() && !IsDanismaRole())
            {
                kisiyeKartiAta.Visible = false;
                atananKartiGuncelle.Visible = false;
            }
            SonGecenKartlariHazirla();
            SonGecenVerileriYukle();
            DataGridViewAyarla();
            SonHareketleriYukle();
            anlikVeriTrafigi = new System.Windows.Forms.Timer();
            anlikVeriTrafigi.Interval = 1000;
            anlikVeriTrafigi.Tick += (s, ev) => SonGecenVerileriYukle();
            anlikVeriTrafigi.Tick += (s, ev) => SonHareketleriYukle();
            anlikVeriTrafigi.Start();
        }
        private void ApplyTheme()
        {
            BackColor = AppTheme.ContentBackground;
            flpSonGecenler.BackColor = AppTheme.CardBackground;
            if (kisiyeKartiAta != null) { kisiyeKartiAta.BackColor = AppTheme.Primary; kisiyeKartiAta.ForeColor = Color.White; }
            if (atananKartiGuncelle != null) { atananKartiGuncelle.BackColor = AppTheme.Primary; atananKartiGuncelle.ForeColor = Color.White; }
        }
        private void canliIzlemeVeriEkrani_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
        private void SonGecenKartlariHazirla()
        {
            kartlar.Clear();
            flpSonGecenler.Controls.Clear();
            for (int i = 0; i < 4; i++)
            {
                KisiKartKontrolu kart = new KisiKartKontrolu();
                kart.Margin = new Padding(25);
                flpSonGecenler.Controls.Add(kart);
                kartlar.Add(kart);
            }
        }
        private void SonGecenVerileriYukle()
        {
            try
            {
                bool yemekhane = string.Equals(_session?.RolAdi, "YEMEKHANE", StringComparison.OrdinalIgnoreCase);
                var lastPasses = yemekhane
                    ? _svc.GetLastPassesYemekhane((int)_session.AktifFirmaId, 4)
                    : _svc.GetLastPasses((int)_session.AktifFirmaId, 4);

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
                        p.DepartmanAdi,
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

            var list = (IsYemekhaneRole() && !IsDanismaRole())
    ? _khsvc.GetLastMovesByFirmaYemekhane(15, _session.AktifFirmaId.Value)
    : _khsvc.GetLastMovesByFirma(15, _session.AktifFirmaId.Value);


            var table = new DataTable();
            table.Columns.Add("Tarih", typeof(DateTime));
            table.Columns.Add("Adı Soyadı", typeof(string));
            table.Columns.Add("Departmanı", typeof(string));
            table.Columns.Add("Unvanı", typeof(string));
            table.Columns.Add("Turnike Bilgisi", typeof(string));
            table.Columns.Add("KisiId", typeof(int));

            foreach (var x in list)
            {
                table.Rows.Add(x.Tarih, x.AdSoyad, x.Departman, x.Unvan, x.CihazAdi, x.PersonelId);
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
            if (dgSonHareketler.SelectedRows.Count == 0)
                return;

            dgSonHareketler.SelectionChanged -= dgSonHareketler_SelectionChanged;

            int kisiId = Convert.ToInt32(dgSonHareketler.SelectedRows[0].Cells["KisiId"].Value);
            KisiDetaylariniGetir(kisiId);

            dgSonHareketler.SelectionChanged += dgSonHareketler_SelectionChanged;
        }
        private void dgSonHareketler_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                seciliKisiIdManuel = Convert.ToInt32(dgSonHareketler.Rows[e.RowIndex].Cells["KisiId"].Value);
                KisiDetaylariniGetir(seciliKisiIdManuel.Value);
            }
        }
        private void kisiyeKartiAta_Click(object sender, EventArgs e)
        {
            var uc = new misafirKartAtama(_session, _mSvc);
            uc.InitYeni((int)_session.AktifFirmaId);
            ShowUserControlInDialog(uc, "Misafir Kart Atama - Yeni", 520, 360, this);
        }
        private void atananKartiGuncelle_Click(object sender, EventArgs e)
        {
            var uc = new misafirKartAtama(_session, _mSvc);
            uc.InitGuncelleme((int)_session.AktifFirmaId, DateTime.Now);
            ShowUserControlInDialog(uc, "Misafir Kart Atama - Güncelleme", 520, 360, this);
        }
        private void KisiDetaylariniGetir(int kisiId)
        {
            try
            {
                var dto = _kisiDetaySvc.GetDetay(kisiId);
                if (dto == null)
                {
                    lblSeciliAdSoyad.Text = "-";
                    lblSeciliUnvan.Text = "-";
                    lblSeciliDepartman.Text = "-";
                    pbSeciliFoto.Image = Properties.Resources.Unknown_person;
                    return;
                }

                lblSeciliAdSoyad.Text = dto.AdSoyad ?? "-";
                lblSeciliUnvan.Text = dto.Unvan ?? "-";
                lblSeciliDepartman.Text = dto.Departman ?? "-";

                if (dto.Foto != null && dto.Foto.Length > 0)
                {
                    using (var ms = new MemoryStream(dto.Foto))
                    {
                        pbSeciliFoto.Image = Image.FromStream(ms);
                    }
                }
                else
                {
                    pbSeciliFoto.Image = Properties.Resources.Unknown_person;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kişi detayları alınamadı: " + ex.Message);
            }
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
        private bool IsYemekhaneRole() => string.Equals(_session?.RolAdi, "YEMEKHANE", StringComparison.OrdinalIgnoreCase);
        private bool IsDanismaRole()
        {
            var r = _session?.RolAdi ?? "";
            return r.IndexOf("DANIŞMA", StringComparison.OrdinalIgnoreCase) >= 0
                || r.IndexOf("DANISMA", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
