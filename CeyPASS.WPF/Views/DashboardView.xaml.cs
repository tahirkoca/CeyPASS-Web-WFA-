using System.Windows;
using System.Windows.Input;
using CeyPASS.Entities.Concrete;
using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

/// <summary>Ana sayfa KPI kartları; tıklanınca ilgili rapor türü tetiklenir.</summary>
public partial class DashboardView : System.Windows.Controls.UserControl
{
    public DashboardView()
    {
        InitializeComponent();
        DataContext = new DashboardViewModel(App.Services);
    }

    public DashboardViewModel ViewModel => (DashboardViewModel)DataContext!;

    /// <summary>KPI Tag değeri <see cref="DashboardReportTypeHelper"/> ile rapor isteğine dönüştürülür.</summary>
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
