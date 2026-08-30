using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

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
