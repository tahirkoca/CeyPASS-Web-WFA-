using System;
using System.Globalization;
using System.Windows.Data;
using CeyPASS.WPF.ViewModels;

namespace CeyPASS.WPF.Views;

public partial class PersonelView : System.Windows.Controls.UserControl
{
    public PersonelView()
    {
        InitializeComponent();
        DataContext = new PersonelViewModel(App.Services);
    }
}

public sealed class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && !b;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && !b;
}

public sealed class InverseBoolToVisConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && !b
            ? System.Windows.Visibility.Visible
            : System.Windows.Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
