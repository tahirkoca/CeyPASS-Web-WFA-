using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

public partial class FirmaView : System.Windows.Controls.UserControl
{
    public FirmaView()
    {
        InitializeComponent();
        DataContext = new FirmaViewModel(App.Services);
    }
}
