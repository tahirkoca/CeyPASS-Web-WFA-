using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CeyPASS.WFA.UserControls.Canlı_İzleme
{
    public partial class gecmisZiyaretciPanel : UserControl
    {
        private Func<string, List<GecmisZiyaretciItem>> _search;
        private bool _suppressSearch;

        public event Action<GecmisZiyaretciItem> ZiyaretciSecildi;

        public gecmisZiyaretciPanel()
        {
            InitializeComponent();
            txtAra.TextChanged += TxtAra_TextChanged;
            lstGecmis.SelectedIndexChanged += LstGecmis_SelectedIndexChanged;
        }

        public void LoadListe(Func<string, List<GecmisZiyaretciItem>> search)
        {
            _search = search;
            YenileListe();
        }

        public void SetSearchPlaceholder(string placeholder)
        {
            if (txtAra != null)
                txtAra.PlaceholderText = placeholder ?? "";
        }

        private void TxtAra_TextChanged(object sender, EventArgs e)
        {
            if (_suppressSearch) return;
            YenileListe();
        }

        private void LstGecmis_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstGecmis.SelectedItem is GecmisZiyaretciItem item)
                ZiyaretciSecildi?.Invoke(item);
        }

        private void YenileListe()
        {
            if (_search == null) return;

            var filter = txtAra.Text?.Trim() ?? "";
            List<GecmisZiyaretciItem> items;
            try
            {
                items = _search(filter) ?? new List<GecmisZiyaretciItem>();
            }
            catch
            {
                items = new List<GecmisZiyaretciItem>();
            }

            _suppressSearch = true;
            lstGecmis.BeginUpdate();
            lstGecmis.DataSource = null;
            lstGecmis.DisplayMember = nameof(GecmisZiyaretciItem.Gosterim);
            lstGecmis.DataSource = items;
            lstGecmis.EndUpdate();
            _suppressSearch = false;
        }
    }
}
