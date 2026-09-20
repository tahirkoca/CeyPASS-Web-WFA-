using System;
using System.Drawing;
using System.Windows.Forms;

namespace CeyPASS.WFA.UserControls.Canlı_İzleme
{
    /// <summary>Canlı izlemede tek geçiş kartı — foto, giriş/çıkış rengi.</summary>
    public partial class KisiKartKontrolu : UserControl
    {
        private const int DesignWidth = 420;
        private const int DesignHeight = 483;

        /// <summary>Responsive layout için boyutlandırma olayını bağlar.</summary>
        public KisiKartKontrolu()
        {
            InitializeComponent();
            labelFirmaUnvan.Visible = false;
            labelUnvan.Visible = false;
            Resize += (_, _) => LayoutContents();
            LayoutContents();
        }

        /// <summary>Son geçiş verisini karta yansıtır; alt şerit giriş/çıkış rengine göre boyanır.</summary>
        public void Ayarla(Image foto, string adSoyad, string departman, string unvan, DateTime zaman, string turnikeAdi, bool girisMi)
        {
            var old = pbFoto.Image;
            pbFoto.Image = foto;
            if (!ReferenceEquals(old, foto) && old != null && !ReferenceEquals(old, Properties.Resources.Unknown_person))
            {
                old.Dispose();
            }
            labelAdSoyad.Text = adSoyad;
            labelGirisZamani.Text = zaman.ToString("dd.MM.yyyy HH:mm:ss");
            labelTurnike.Text = turnikeAdi;
            pnlAltBilgi.BackColor = girisMi ? Color.SeaGreen : Color.Firebrick;
            labelGirisZamani.ForeColor = labelTurnike.ForeColor = Color.White;
        }

        private void LayoutContents()
        {
            if (Width < 50 || Height < 50) return;

            float sx = Width / (float)DesignWidth;
            float sy = Height / (float)DesignHeight;

            // Fotoğraf alanı: üstte, isim+alt şerit için yer bırak
            int footerH = Math.Max(90, (int)(110 * sy));
            int nameH = Math.Max(28, (int)(32 * sy));
            int pad = Math.Max(8, (int)(14 * Math.Min(sx, sy)));
            int gap = Math.Max(4, (int)(8 * sy));

            int fotoTop = pad;
            int fotoBottom = Height - footerH - nameH - gap * 2 - pad;
            int fotoH = Math.Max(80, fotoBottom - fotoTop);
            int fotoW = Math.Max(60, Width - pad * 2);
            // Oranı bozmadan ortala (StretchImage kullanıyoruz ama çerçeve kareye yakın)
            int side = Math.Min(fotoW, fotoH);
            pbFoto.Size = new Size(side, side);
            pbFoto.Location = new Point((Width - side) / 2, fotoTop + (fotoH - side) / 2);
            pbFoto.SizeMode = PictureBoxSizeMode.Zoom;

            labelAdSoyad.Size = new Size(Width - pad * 2, nameH);
            labelAdSoyad.Location = new Point(pad, pbFoto.Bottom + gap);
            labelAdSoyad.Font = new Font("Segoe UI", Math.Max(10f, 10f * Math.Min(sx, sy) + 2f), FontStyle.Bold);

            pnlAltBilgi.Size = new Size(Width - pad * 2, footerH);
            pnlAltBilgi.Location = new Point(pad, Height - footerH - pad / 2);
            float footerFont = Math.Max(10f, 11f * Math.Min(sx, sy) + 1f);
            labelGirisZamani.Font = new Font("Segoe UI", footerFont);
            labelTurnike.Font = new Font("Segoe UI", footerFont);
        }
    }
}
