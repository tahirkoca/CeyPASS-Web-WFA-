using System.Windows;
using System.Windows.Controls;
using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

public partial class IzinlerView : UserControl
{
    public IzinlerView()
    {
        InitializeComponent();
        DataContext = new IzinlerViewModel(App.Services);
    }

    private void Firma_DropDownOpened(object sender, EventArgs e) { }

    private void Kisi_DropDownOpened(object sender, EventArgs e)
    {
        if (DataContext is IzinlerViewModel vm)
            vm.EnsureKisilerLoaded();
    }

    private void IzinTip_DropDownOpened(object sender, EventArgs e)
    {
        if (DataContext is IzinlerViewModel vm)
            vm.EnsureIzinTipleriLoaded();
    }
}
