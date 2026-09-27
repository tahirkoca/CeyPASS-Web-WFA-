using System.Windows.Controls;
using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

public partial class CihazView : UserControl
{
    public CihazView()
    {
        InitializeComponent();
        DataContext = new CihazViewModel(App.Services);
    }
}
