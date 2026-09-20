using System.Windows;
using System.Windows.Controls;
using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

/// <summary>İzin talep/onay ekranı; ağır listeler combo açılınca yüklenir.</summary>
public partial class IzinlerView : UserControl
{
    public IzinlerView()
    {
        InitializeComponent();
        DataContext = new IzinlerViewModel(App.Services);
    }

    private void Firma_DropDownOpened(object sender, EventArgs e) { }

    /// <summary>Performans: kişi listesi ilk açılışta çekilir.</summary>
    private void Kisi_DropDownOpened(object sender, EventArgs e)
    {
        if (DataContext is IzinlerViewModel vm)
            vm.EnsureKisilerLoaded();
    }

    /// <summary>Performans: izin tipleri ilk açılışta çekilir.</summary>
    private void IzinTip_DropDownOpened(object sender, EventArgs e)
    {
        if (DataContext is IzinlerViewModel vm)
            vm.EnsureIzinTipleriLoaded();
    }
}
