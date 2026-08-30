using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

public partial class DepartmanView : System.Windows.Controls.UserControl
{
    public DepartmanView()
    {
        InitializeComponent();
        DataContext = new DepartmanViewModel(App.Services);
    }
}
