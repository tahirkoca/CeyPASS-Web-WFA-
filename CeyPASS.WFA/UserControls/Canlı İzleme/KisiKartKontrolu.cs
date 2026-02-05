using System;
using System.Drawing;
using System.Windows.Forms;

namespace CeyPASS.WFA.UserControls.Canlı_İzleme
{
    public partial class KisiKartKontrolu : UserControl
    {
        public KisiKartKontrolu()
        {
            InitializeComponent();
        }

        public void Ayarla(Image foto, string adSoyad, string departman, string unvan, DateTime zaman, string turnikeAdi, bool girisMi)
        {
            var old = pbFoto.Image;
            pbFoto.Image = foto;
            if (!ReferenceEquals(old, foto) && old != null && !ReferenceEquals(old, Properties.Resources.Unknown_person))
            {
                old.Dispose();
            }
            labelAdSoyad.Text = adSoyad;
            labelFirmaUnvan.Text = departman;
            labelUnvan.Text = unvan;
            labelGirisZamani.Text = zaman.ToString("dd.MM.yyyy HH:mm:ss");
            labelTurnike.Text = turnikeAdi;
            pnlAltBilgi.BackColor = girisMi ? Color.SeaGreen : Color.Firebrick;
            labelGirisZamani.ForeColor = labelTurnike.ForeColor = Color.White;
        }
    }
}