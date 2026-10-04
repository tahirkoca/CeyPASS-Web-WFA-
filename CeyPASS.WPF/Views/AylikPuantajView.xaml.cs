using System.Linq;
using System.Windows.Controls;
using CeyPASS.WPF.ViewModels;
using DevExpress.Xpf.Grid;

namespace CeyPASS.WPF.Views;

/// <summary>Aylık puantaj onay, red ve satır düzenleme.</summary>
public partial class AylikPuantajView : UserControl
{
    public AylikPuantajView()
    {
        InitializeComponent();
        DataContext = new AylikPuantajViewModel(App.Services);
    }

    private void GridPuantaj_OnSelectionChanged(object sender, GridSelectionChangedEventArgs e)
    {
        if (DataContext is not AylikPuantajViewModel vm) return;
        var selected = GridPuantaj.SelectedItems
            .OfType<PuantajGunRowItem>()
            .ToList();
        vm.SetGridSelection(selected);
    }
}
