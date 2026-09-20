using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

/// <summary>Pozisyon / unvan tanımlama.</summary>
public partial class PozisyonView : System.Windows.Controls.UserControl
{
    public PozisyonView()
    {
        InitializeComponent();
        DataContext = new PozisyonViewModel(App.Services);
    }
}
