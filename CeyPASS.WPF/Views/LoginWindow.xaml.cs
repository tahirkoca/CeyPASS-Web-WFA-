using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CeyPASS.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CeyPASS.WPF.Views;

/// <summary>Ana uygulama giriş penceresi; başarılı oturumda <see cref="MainWindow"/> açılır.</summary>
public partial class LoginWindow : Window
{
    private readonly LoginViewModel _vm;
    private bool _syncingPassword;

    public LoginWindow()
    {
        InitializeComponent();
        _vm = new LoginViewModel(App.Services);
        DataContext = _vm;
        _vm.LoginSucceeded += OnLoginSucceeded;
        _vm.OpenLiveMonitorRequested += OnOpenLiveMonitor;
        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(LoginViewModel.ShowPassword) && !_vm.ShowPassword)
            {
                _syncingPassword = true;
                try { PwdBox.Password = _vm.Password; }
                finally { _syncingPassword = false; }
            }
        };
    }

    /// <summary>PasswordBox ↔ ViewModel; ShowPassword kapatılınca senkronizasyon döngüsünü keser.</summary>
    private void PwdBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (_syncingPassword) return;
        if (sender is PasswordBox pb)
            _vm.Password = pb.Password;
    }

    private void Window_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }

    /// <summary>Esc pencereyi kapatır; Enter giriş komutunu çalıştırır (komut müsaitse).</summary>
    private void Window_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Enter && _vm.LoginCommand.CanExecute(null))
        {
            _vm.LoginCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void OnLoginSucceeded()
    {
        var main = App.Services.GetRequiredService<MainWindow>();
        Application.Current.MainWindow = main;
        main.Show();
        Close();
    }

    /// <summary>Canlı izleme girişine geçer; DI scope pencere kapanınca dispose edilir.</summary>
    private void OnOpenLiveMonitor()
    {
        var scope = App.Services.CreateScope();
        var win = ActivatorUtilities.CreateInstance<CanliIzlemeLoginWindow>(scope.ServiceProvider, this);
        win.Closed += (_, _) =>
        {
            scope.Dispose();
            // Geri/Esc: login'e dön. Monitöre geçişte gizli kalsın.
            if (win.ReturnedToLogin && IsLoaded && !IsVisible)
                Show();
        };
        Hide();
        win.Show();
    }
}
