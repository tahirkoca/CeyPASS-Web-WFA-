using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

public partial class IsyeriView : System.Windows.Controls.UserControl
{
    public IsyeriView()
    {
        InitializeComponent();
        DataContext = new IsyeriViewModel(App.Services);
    }
}
