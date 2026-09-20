using System.Windows;
using System.Windows.Input;
using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

/// <summary>Aynı T.C. ile birden fazla sicilin ana personele eşleştirilmesi.</summary>
public partial class CokluSicilEslestirmeWindow : Window
{
    public CokluSicilEslestirmeWindow(int anaPersonelId, string tcKimlikNo, string adSoyad)
    {
        InitializeComponent();
        DataContext = new CokluSicilEslestirmeViewModel(App.Services, anaPersonelId, tcKimlikNo, adSoyad);
        Loaded += (_, _) => ((CokluSicilEslestirmeViewModel)DataContext).Load();
    }

    private void Window_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            DialogResult = false;
            Close();
        }
    }

    private void Window_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }

    private void BtnKapat_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
