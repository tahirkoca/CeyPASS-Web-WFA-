using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

/// <summary>Turnike/cihaz tanımları; admin panelde firma filtresi kapalı mod.</summary>
public partial class CihazView : System.Windows.Controls.UserControl
{
    public CihazView() : this(adminPanelMode: false)
    {
    }

    public CihazView(bool adminPanelMode)
    {
        InitializeComponent();
        DataContext = new CihazViewModel(App.Services, adminPanelMode);
    }
}
