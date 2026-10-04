using CeyPASS.Business.Abstractions;
using CeyPASS.Business.Services;
using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CeyPASS.WFA.Forms
{
    /// <summary>Canlı İzleme — anlık serbest/kısıtlı listesi ve toplu komut.</summary>
    public static class KartDurumTopluForm
    {
        /// <summary>Modal liste; değişiklik yapıldıysa true döner.</summary>
        public static bool Show(
            IWin32Window owner,
            ISessionContext session,
            IMisafirKartService misafir,
            IAracKartiService arac,
            ICanliIzlemeKartKomutService komut)
        {
            if (!session.AktifFirmaId.HasValue)
            {
                MessageBox.Show(owner, "Aktif firma bilgisi bulunamadı.", "Kart durumları", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            var firmaId = session.AktifFirmaId.Value;
            var all = KartAtamaListePresenter.Load(misafir, arac, komut, firmaId, "tumu");
            var changed = false;

            using (var f = new Form())
            {
                f.Text = "Kart durumları";
                f.StartPosition = FormStartPosition.CenterParent;
                f.Size = new Size(860, 560);
                f.MinimizeBox = false;
                f.MaximizeBox = false;

                var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 78, Padding = new Padding(8), WrapContents = true };
                var cmb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
                cmb.Items.AddRange(new object[] { "Tümü", "Misafir", "Araç" });
                cmb.SelectedIndex = 0;
                var grid = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = false,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    AutoGenerateColumns = false,
                    RowHeadersVisible = false
                };
                grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Sec", HeaderText = "", Width = 36 });
                grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tip", HeaderText = "Tip", ReadOnly = true, Width = 70 });
                grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Kart", HeaderText = "Kart", ReadOnly = true, Width = 140 });
                grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Kisi", HeaderText = "Kişi / Plaka", ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
                grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Atama", HeaderText = "Atama", ReadOnly = true, Width = 80 });
                grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cihaz", HeaderText = "Cihaz", ReadOnly = true, Width = 80 });
                grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Pid", Visible = false });

                void Fill()
                {
                    var tip = cmb.SelectedIndex == 1 ? "misafir" : cmb.SelectedIndex == 2 ? "arac" : "tumu";
                    var rows = tip == "tumu" ? all : all.Where(x => x.Tip == tip).ToList();
                    grid.Rows.Clear();
                    foreach (var r in rows)
                    {
                        grid.Rows.Add(false, r.TipLabel, r.KartAdi, r.KisiPlaka, r.DurumText, r.CihazdaAktif ? "Serbest" : "Kısıtlı", r.PersonelId);
                    }
                }

                List<string> SelectedIds()
                {
                    var ids = new List<string>();
                    foreach (DataGridViewRow row in grid.Rows)
                    {
                        if (row.Cells["Sec"].Value is bool b && b)
                            ids.Add(Convert.ToString(row.Cells["Pid"].Value) ?? "");
                    }
                    return ids.Where(x => x.Length > 0).ToList();
                }

                void Enqueue(IReadOnlyList<string> ids, bool pasif, string empty)
                {
                    if (ids.Count == 0)
                    {
                        MessageBox.Show(f, empty, "Kart durumları", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    var hazirAtlanan = 0;
                    if (pasif)
                    {
                        var hazirIds = new HashSet<string>(
                            all.Where(x => x.Durum == nameof(KartAtamaListeDurum.Hazir)).Select(x => x.PersonelId),
                            StringComparer.OrdinalIgnoreCase);
                        hazirAtlanan = ids.Count(hazirIds.Contains);
                        ids = ids.Where(x => !hazirIds.Contains(x)).ToList();
                        if (ids.Count == 0)
                        {
                            MessageBox.Show(f, "HAZIR durumdaki kartlar kısıtlanamaz.", "Kart durumları", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }
                    var atlananNotu = hazirAtlanan > 0 ? "\n" + hazirAtlanan + " HAZIR kart atlandı." : "";
                    var onay = pasif
                        ? ids.Count + " kart cihazlarda kısıtlansın mı?" + atlananNotu
                        : ids.Count + " kartın kısıtı kaldırılsın mı?";
                    if (MessageBox.Show(f, onay, pasif ? "Kartı Kısıtla" : "Kart Kısıtı Kaldır", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return;
                    var ok = 0;
                    try
                    {
                        foreach (var id in ids)
                        {
                            if (pasif) komut.EnqueuePasif(firmaId, id, session.AktifKullaniciId);
                            else komut.EnqueueAktif(firmaId, id, session.AktifKullaniciId);
                            var row = all.FirstOrDefault(x => string.Equals(x.PersonelId, id, StringComparison.OrdinalIgnoreCase));
                            if (row != null) row.CihazdaAktif = !pasif;
                            ok++;
                        }
                        changed = true;
                        MessageBox.Show(f, (pasif ? "Kısıtlama" : "Kısıt kaldırma") + " komutu kuyruğa alındı: " + ok + atlananNotu, "Kart durumları");
                    }
                    catch (Exception ex)
                    {
                        if (ok > 0) changed = true;
                        MessageBox.Show(f, ok > 0 ? ok + " kart yazıldı, sonra hata:\n" + ex.Message : ex.Message, "Kart durumları", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    Fill();
                }

                Button Btn(string text, Color back, EventHandler click)
                {
                    var b = new Button { Text = text, AutoSize = true, BackColor = back, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Margin = new Padding(4) };
                    b.FlatAppearance.BorderSize = 0;
                    b.Click += click;
                    return b;
                }

                top.Controls.Add(new Label { Text = "Kart tipi", AutoSize = true, Padding = new Padding(0, 8, 6, 0) });
                top.Controls.Add(cmb);
                top.Controls.Add(Btn("Tümünü seç", Color.FromArgb(0x3B, 0x82, 0xF6), (_, __) =>
                {
                    foreach (DataGridViewRow row in grid.Rows) row.Cells["Sec"].Value = true;
                }));
                top.Controls.Add(Btn("Seçimi kaldır", Color.FromArgb(0x64, 0x74, 0x8B), (_, __) =>
                {
                    foreach (DataGridViewRow row in grid.Rows) row.Cells["Sec"].Value = false;
                }));
                top.Controls.Add(Btn("Seçilenleri kısıtla", Color.FromArgb(0xEA, 0x58, 0x0C), (_, __) =>
                    Enqueue(SelectedIds(), true, "Önce kart seçin.")));
                top.Controls.Add(Btn("Seçilenleri serbest bırak", Color.FromArgb(0x0D, 0x94, 0x88), (_, __) =>
                    Enqueue(SelectedIds(), false, "Önce kart seçin.")));
                top.Controls.Add(Btn("Tüm serbestleri kısıtla", Color.FromArgb(0xDC, 0x26, 0x26), (_, __) =>
                    Enqueue(VisibleByAktif(grid, true), true, "Serbest kart yok.")));
                top.Controls.Add(Btn("Tüm kısıtlıları serbest bırak", Color.FromArgb(0x16, 0xA3, 0x4A), (_, __) =>
                    Enqueue(VisibleByAktif(grid, false), false, "Kısıtlı kart yok.")));

                cmb.SelectedIndexChanged += (_, __) => Fill();
                f.Controls.Add(grid);
                f.Controls.Add(top);
                Fill();
                f.ShowDialog(owner);
            }

            return changed;
        }

        private static List<string> VisibleByAktif(DataGridView grid, bool serbest)
        {
            var ids = new List<string>();
            foreach (DataGridViewRow row in grid.Rows)
            {
                var cihaz = Convert.ToString(row.Cells["Cihaz"].Value);
                var isSerbest = string.Equals(cihaz, "Serbest", StringComparison.Ordinal);
                if (isSerbest == serbest)
                    ids.Add(Convert.ToString(row.Cells["Pid"].Value) ?? "");
            }
            return ids.Where(x => x.Length > 0).ToList();
        }
    }
}
