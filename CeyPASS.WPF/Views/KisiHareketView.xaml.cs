using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

/// <summary>Geçiş hareketleri listesi ve manuel hareket girişi.</summary>
public partial class KisiHareketView : System.Windows.Controls.UserControl
{
    public KisiHareketView()
    {
        InitializeComponent();
        DataContext = new KisiHareketViewModel(App.Services);
    }
}
