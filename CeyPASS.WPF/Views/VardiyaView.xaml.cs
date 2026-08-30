using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

public partial class VardiyaView : System.Windows.Controls.UserControl
{
    public VardiyaView() : this(adminPanelMode: false)
    {
    }

    public VardiyaView(bool adminPanelMode)
    {
        InitializeComponent();
        DataContext = new VardiyaViewModel(App.Services, adminPanelMode);
    }
}
