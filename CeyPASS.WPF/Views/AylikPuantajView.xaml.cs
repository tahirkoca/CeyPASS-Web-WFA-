using System.Windows.Controls;
using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

/// <summary>Aylık puantaj onay, red ve satır düzenleme.</summary>
public partial class AylikPuantajView : UserControl
{
    public AylikPuantajView()
    {
        InitializeComponent();
        DataContext = new AylikPuantajViewModel(App.Services);
    }
}
