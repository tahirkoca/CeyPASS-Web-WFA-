using System.Windows;
using System.Windows.Input;
using CeyPASS.Entities.Concrete;
using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

public partial class KisiAraWindow : Window
{
    private readonly KisiAraViewModel _vm;

    public string? SelectedPersonelId => _vm.SelectedPersonelId;
    public KisiAraContext? AppliedContext => _vm.AppliedContext;

    public KisiAraWindow(KisiAraContext context)
    {
        InitializeComponent();
        _vm = new KisiAraViewModel(App.Services, context);
        _vm.CloseOk = () =>
        {
            DialogResult = true;
            Close();
        };
        DataContext = _vm;
    }

    private void BtnVazgec_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Window_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            DialogResult = false;
            Close();
            e.Handled = true;
        }
    }

    private void Window_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }
}
