using CeyPASS.Business.Abstractions;
using CeyPASS.Infrastructure.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace CeyPASS.WPF.ViewModels;

public sealed class LoginViewModel : ObservableObject
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ISessionContext _session;
    private string _password = "";
    private string? _selectedUser;
    private string? _error;
    private bool _showPassword;

    public LoginViewModel(IServiceProvider root)
    {
        _scopeFactory = root.GetRequiredService<IServiceScopeFactory>();
        _session = root.GetRequiredService<ISessionContext>();

        var ver = AppVersion.ProductVersion;
        VersionText = $"Ver {ver}";
        WindowTitle = $"CeyPASS v{ver}";

        using var scope = _scopeFactory.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<IKullaniciService>().GetTumKullaniciAdlari()
                    ?? new List<string>();
        Users = new ObservableCollection<string>(users);
        if (Users.Count > 0)
            SelectedUser = Users[0];

        LoginCommand = new RelayCommand(Login);
        ForgotPasswordCommand = new RelayCommand(ForgotPassword);
        LiveMonitorCommand = new RelayCommand(OpenLiveMonitor);
    }

    public ObservableCollection<string> Users { get; }
    public string VersionText { get; }
    public string WindowTitle { get; }
    public BindableFieldErrors Errors { get; } = new();

    public string? SelectedUser
    {
        get => _selectedUser;
        set => SetProperty(ref _selectedUser, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string? Error
    {
        get => _error;
        set => SetProperty(ref _error, value);
    }

    public bool ShowPassword
    {
        get => _showPassword;
        set => SetProperty(ref _showPassword, value);
    }

    public ICommand LoginCommand { get; }
    public ICommand ForgotPasswordCommand { get; }
    public ICommand LiveMonitorCommand { get; }

    public event Action? LoginSucceeded;
    public event Action? OpenLiveMonitorRequested;

    private void Login()
    {
        Errors.Clear();
        Error = null;

        Errors.Require("SelectedUser", SelectedUser, "Kullanıcı adı zorunludur.");
        Errors.Require("Password", Password, "Şifre zorunludur.");
        if (Errors.HasErrors)
        {
            Error = Errors.FirstMessage;
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var ksvc = scope.ServiceProvider.GetRequiredService<IKullaniciService>();
        var yetkiSvc = scope.ServiceProvider.GetRequiredService<IKullaniciFirmaIsyeriYetkiService>();

        var kullanici = ksvc.GirisYap(SelectedUser?.Trim() ?? "", Password.Trim());
        if (kullanici is null)
        {
            Error = "Hatalı kullanıcı adı veya şifre.";
            return;
        }

        var yetkiler = yetkiSvc.GetYetkiler(kullanici.KullaniciId);
        bool isAdmin = FirmaIsyeriYetkiHelper.IsAdmin(kullanici.RolId);
        var aktifFirmaId = FirmaIsyeriYetkiHelper.ResolveAktifFirmaId(kullanici.FirmaId, yetkiler, isAdmin);
        if (!aktifFirmaId.HasValue)
        {
            Error = "Bu kullanıcıya tanımlı firma veya firma yetkisi bulunamadı.";
            return;
        }

        _session.AktifKullaniciId = kullanici.KullaniciId;
        _session.AktifFirmaId = aktifFirmaId;
        _session.AdSoyad = kullanici.AdSoyad;
        _session.RolAdi = kullanici.RolTanimi;
        _session.RolId = kullanici.RolId;

        LoginSucceeded?.Invoke();
    }

    private void ForgotPassword()
    {
        Errors.Clear();
        Error = null;
        if (!Errors.Require("SelectedUser", SelectedUser, "Lütfen kullanıcı adınızı seçin."))
        {
            Error = Errors.FirstMessage;
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var dlg = ActivatorUtilities.CreateInstance<Views.ForgotPasswordWindow>(
            scope.ServiceProvider, SelectedUser!.Trim());
        dlg.Owner = System.Windows.Application.Current.Windows.OfType<Views.LoginWindow>().FirstOrDefault();
        dlg.ShowDialog();
    }

    private void OpenLiveMonitor() => OpenLiveMonitorRequested?.Invoke();
}
