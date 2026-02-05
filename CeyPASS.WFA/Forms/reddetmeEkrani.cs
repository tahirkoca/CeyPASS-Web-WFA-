using CeyPASS.Entities.Concrete;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CeyPASS.WFA.Forms
{
    public partial class reddetmeEkrani : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;
        private readonly PuantajGunSatirDTO _model;
        public string RetSebebi { get; private set; }

        public reddetmeEkrani(PuantajGunSatirDTO model)
        {
            InitializeComponent();
            SendMessage(txtSebep.Handle, EM_SETCUEBANNER, 0, "Ret nedeni giriniz");
            _model = model;
        }

        private void puantajSatirDuzenlemeEkrani_Load(object sender, EventArgs e) { }
        private void btnKaydet_Click(object sender, EventArgs e)
        {
            RetSebebi = txtSebep.Text?.Trim();
            _model.Aciklama = RetSebebi;
            DialogResult = DialogResult.OK;
            Close();
        }
        private void btnIptal_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
