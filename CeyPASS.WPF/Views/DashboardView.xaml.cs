using System.Windows;
using System.Windows.Input;
using CeyPASS.Entities.Concrete;
using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

public partial class DashboardView : System.Windows.Controls.UserControl
{
    public DashboardView()
    {
        InitializeComponent();
        DataContext = new DashboardViewModel(App.Services);
    }

    public DashboardViewModel ViewModel => (DashboardViewModel)DataContext!;

    private void KpiCard_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: string tag })
            return;
        if (!Enum.TryParse(tag, out DashboardReportTypeHelper type))
            return;

        ViewModel.RaiseReport(type);
        e.Handled = true;
    }
}
