using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

public partial class GuncellemeBildirimView : System.Windows.Controls.UserControl
{
    public GuncellemeBildirimView()
    {
        InitializeComponent();
        DataContext = new GuncellemeBildirimViewModel(App.Services);
    }
}
