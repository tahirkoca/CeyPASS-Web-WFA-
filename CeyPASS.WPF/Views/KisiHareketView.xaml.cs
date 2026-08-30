using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

public partial class KisiHareketView : System.Windows.Controls.UserControl
{
    public KisiHareketView()
    {
        InitializeComponent();
        DataContext = new KisiHareketViewModel(App.Services);
    }
}
