using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CeyPASS.Entities.Concrete;
using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

public partial class RaporlarView : System.Windows.Controls.UserControl
{
    public RaporlarView()
    {
        InitializeComponent();
        DataContext = new RaporlarViewModel(App.Services);
    }

    public RaporlarViewModel ViewModel => (RaporlarViewModel)DataContext!;

    public void OpenFromDashboard(ReportRequest req) => ViewModel.OpenFromDashboard(req);

    /// <summary>Açılan listede uzun seçenek metinlerinin kesilmemesi için popup genişliğini içeriğe göre ayarlar.</summary>
    private void FilterCombo_DropDownOpened(object sender, EventArgs e)
    {
        if (sender is not ComboBox combo)
            return;

        combo.Dispatcher.BeginInvoke(() =>
        {
            if (combo.Template?.FindName("PART_Popup", combo) is not Popup popup)
                return;

            double maxNeeded = combo.ActualWidth;
            foreach (var item in combo.Items)
            {
                var text = item switch
                {
                    null => "",
                    _ when !string.IsNullOrEmpty(combo.DisplayMemberPath)
                        => item.GetType().GetProperty(combo.DisplayMemberPath)?.GetValue(item)?.ToString() ?? item.ToString() ?? "",
                    _ => item.ToString() ?? ""
                };

                var formatted = new FormattedText(
                    text,
                    System.Globalization.CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    new Typeface(combo.FontFamily, combo.FontStyle, combo.FontWeight, combo.FontStretch),
                    combo.FontSize,
                    Brushes.Black,
                    VisualTreeHelper.GetDpi(combo).PixelsPerDip);

                maxNeeded = Math.Max(maxNeeded, formatted.Width + 36);
            }

            popup.MinWidth = maxNeeded;
            popup.Width = maxNeeded;
        }, System.Windows.Threading.DispatcherPriority.Loaded);
    }
}
