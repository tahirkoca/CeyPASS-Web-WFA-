using System.Windows.Controls;
using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

public partial class VardiyaView : UserControl
{
    public VardiyaView()
    {
        InitializeComponent();
        DataContext = new VardiyaViewModel(App.Services);
    }
}
