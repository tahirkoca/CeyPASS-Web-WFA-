using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

/// <summary>Vardiya ve çalışma şekli tanımları; admin panelde genişletilmiş liste.</summary>
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
