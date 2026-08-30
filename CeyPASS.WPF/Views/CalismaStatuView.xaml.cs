using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

public partial class CalismaStatuView : System.Windows.Controls.UserControl
{
    public CalismaStatuView()
    {
        InitializeComponent();
        DataContext = new CalismaStatuViewModel(App.Services);
    }
}
