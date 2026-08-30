using System.Windows;
using System.Windows.Input;
using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

public partial class YemekSaatleriDetailWindow : Window
{
    public YemekSaatleriDetailWindow(int calismaSekliId, string vardiyaAd)
    {
        InitializeComponent();
        DataContext = new YemekSaatleriDetailViewModel(App.Services, calismaSekliId, vardiyaAd);
    }

    private void BtnKapat_OnClick(object sender, RoutedEventArgs e) => Close();

    private void Window_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
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
