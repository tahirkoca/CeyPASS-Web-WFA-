using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

public partial class ResmiTatilView : System.Windows.Controls.UserControl
{
    public ResmiTatilView()
    {
        InitializeComponent();
        DataContext = new ResmiTatilViewModel(App.Services);
    }
}
