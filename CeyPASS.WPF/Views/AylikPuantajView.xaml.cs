using System.Windows.Controls;
using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

public partial class AylikPuantajView : UserControl
{
    public AylikPuantajView()
    {
        InitializeComponent();
        DataContext = new AylikPuantajViewModel(App.Services);
    }
}
