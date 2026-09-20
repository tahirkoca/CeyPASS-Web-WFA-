using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

/// <summary>İşyeri tanımlama (firmaya bağlı).</summary>
public partial class IsyeriView : System.Windows.Controls.UserControl
{
    public IsyeriView()
    {
        InitializeComponent();
        DataContext = new IsyeriViewModel(App.Services);
    }
}
