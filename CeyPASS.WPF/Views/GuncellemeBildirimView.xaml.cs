using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

/// <summary>Admin: uygulama güncelleme bildirimi yönetimi.</summary>
public partial class GuncellemeBildirimView : System.Windows.Controls.UserControl
{
    public GuncellemeBildirimView()
    {
        InitializeComponent();
        DataContext = new GuncellemeBildirimViewModel(App.Services);
    }
}
