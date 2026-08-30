using System.Windows;
using System.Windows.Controls;
using CeyPASS.Entities.Concrete;

namespace CeyPASS.WPF.Views;

public partial class GecmisZiyaretciPanel : UserControl
{
    private Func<string, List<GecmisZiyaretciItem>>? _search;
    private bool _suppressSearch;

    public event Action<GecmisZiyaretciItem>? ZiyaretciSecildi;

    public GecmisZiyaretciPanel()
    {
        InitializeComponent();
        TxtAra.TextChanged += (_, _) =>
        {
            if (!_suppressSearch)
                YenileListe();
        };
        LstGecmis.SelectionChanged += (_, _) =>
        {
            if (LstGecmis.SelectedItem is GecmisZiyaretciItem item)
                ZiyaretciSecildi?.Invoke(item);
        };
    }

    public void LoadListe(Func<string, List<GecmisZiyaretciItem>> search)
    {
        _search = search;
        YenileListe();
    }

    public void SetSearchPlaceholder(string placeholder)
    {
        TxtAra.Tag = placeholder;
        ToolTipService.SetToolTip(TxtAra, placeholder);
    }

    private void YenileListe()
    {
        if (_search == null) return;

        var filter = TxtAra.Text?.Trim() ?? "";
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
        LstGecmis.ItemsSource = items;
        _suppressSearch = false;
    }
}
