using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

/// <summary>Resmi tatil günleri tanımlama.</summary>
public partial class ResmiTatilView : System.Windows.Controls.UserControl
{
    public ResmiTatilView()
    {
        InitializeComponent();
        DataContext = new ResmiTatilViewModel(App.Services);
    }
}
