using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CeyPASS.Business.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CeyPASS.WPF.Views;

public partial class CanliIzlemeLoginWindow : Window
{
    private readonly Window _loginOwner;
    private readonly ICanliIzlemeService _svc;
    private readonly ISessionContext _session;
    private readonly IServiceScopeFactory _scopeFactory;
    private bool _navigatingToMonitor;
    private bool _returningToLogin;
    private bool _loadingBolge;

    /// <summary>True when the user chose Back/Esc to return to the main login screen.</summary>
    public bool ReturnedToLogin => _returningToLogin;

    /// <summary>True when login succeeded and the live monitor window is opening.</summary>
    public bool NavigatedToMonitor => _navigatingToMonitor;
    private readonly ObservableCollection<FirmaItem> _firmalar = new();
    private readonly ObservableCollection<string> _kullanicilar = new();

    public CanliIzlemeLoginWindow(
        LoginWindow loginOwner,
        ICanliIzlemeService svc,
        ISessionContext session,
        IServiceScopeFactory scopeFactory)
    {
        InitializeComponent();
        _loginOwner = loginOwner;
        _svc = svc;
        _session = session;
        _scopeFactory = scopeFactory;

        CmbBolge.ItemsSource = _firmalar;
        CmbKullanici.ItemsSource = _kullanicilar;

        Loaded += (_, _) => LoadFirmalar();
    }

    private void LoadFirmalar()
    {
        LblError.Text = "";
        _firmalar.Clear();
        _kullanicilar.Clear();

        try
        {
            var dt = _svc.GetFirmalar();
            if (dt == null || dt.Rows.Count == 0)
            {
                LblError.Text = "Bölge listesi boş. Firma kaydı bulunamadı.";
                return;
            }

            foreach (DataRow row in dt.Rows)
            {
                _firmalar.Add(new FirmaItem
                {
                    FirmaId = Convert.ToInt32(row["FirmaId"]),
                    FirmaAdi = row["FirmaAdi"]?.ToString() ?? ""
                });
            }

            _loadingBolge = true;
            CmbBolge.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            LblError.Text = "Bölgeler yüklenemedi: " + ex.Message;
            return;
        }
        finally
        {
            _loadingBolge = false;
        }

        FillKullanicilar();
    }

    private void CmbBolge_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loadingBolge) return;
        FillKullanicilar();
    }

    private void FillKullanicilar()
    {
        _kullanicilar.Clear();
        CmbKullanici.SelectedIndex = -1;

        if (CmbBolge.SelectedItem is not FirmaItem firma)
            return;

        try
        {
            var adlar = _svc.GetKullaniciAdlariByFirma(firma.FirmaId) ?? new List<string>();
            foreach (var a in adlar.Where(x => !string.IsNullOrWhiteSpace(x)))
                _kullanicilar.Add(a!);

            if (_kullanicilar.Count > 0)
            {
                CmbKullanici.SelectedIndex = 0;
                if (LblError.Text.StartsWith("Bu bölge için", StringComparison.Ordinal) ||
                    LblError.Text.StartsWith("Lütfen", StringComparison.Ordinal))
                    LblError.Text = "";
            }
            else
            {
                LblError.Text = "Bu bölge için aktif canlı izleme kullanıcısı yok.";
            }
        }
        catch (Exception ex)
        {
            LblError.Text = "Kullanıcılar yüklenemedi: " + ex.Message;
        }
    }

    private void BtnGiris_OnClick(object sender, RoutedEventArgs e)
    {
        LblError.Text = "";

        if (CmbBolge.SelectedItem is not FirmaItem firma)
        {
            LblError.Text = "Lütfen bölge seçin.";
            return;
        }

        var user = (CmbKullanici.SelectedItem as string)?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(user))
        {
            LblError.Text = "Lütfen kullanıcı seçin.";
            return;
        }

        if (string.IsNullOrEmpty(PwdBox.Password))
        {
            LblError.Text = "Lütfen şifrenizi girin.";
            return;
        }

        AuthUserResult? auth;
        try
        {
            auth = LoginSafe(firma.FirmaId, user, PwdBox.Password);
        }
        catch (Exception ex)
        {
            LblError.Text = "Giriş hatası: " + ex.Message;
            return;
        }

        if (auth is null)
        {
            LblError.Text = "Hatalı kullanıcı adı/şifre veya bu bölge için yetki yok.";
            return;
        }

        _session.AktifFirmaId = firma.FirmaId;
        _session.AktifKullaniciId = auth.KullaniciId;
        _session.AdSoyad = auth.KullaniciAdi ?? "";
        _session.RolAdi = auth.Rol ?? "";

        var monitorScope = _scopeFactory.CreateScope();
        var monitor = ActivatorUtilities.CreateInstance<CanliIzlemeWindow>(monitorScope.ServiceProvider);
        monitor.Closed += (_, _) =>
        {
            monitorScope.Dispose();
            System.Windows.Application.Current.Shutdown();
        };

        _navigatingToMonitor = true;
        _loginOwner.Hide();
        monitor.Show();
        Close();
    }

    private AuthUserResult? LoginSafe(int firmaId, string user, string password)
    {
        var auth = _svc.Login(firmaId, user, password);
        if (auth is null) return null;
        return new AuthUserResult(auth.KullaniciId, auth.KullaniciAdi, auth.Rol);
    }

    private void BtnGeri_OnClick(object sender, RoutedEventArgs e) => ReturnToLogin();

    private void ReturnToLogin()
    {
        _returningToLogin = true;
        Close();
    }

    private void Window_OnClosing(object? sender, CancelEventArgs e)
    {
        if (_navigatingToMonitor || _returningToLogin)
            return;
        System.Windows.Application.Current.Shutdown();
    }

    private void Window_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            ReturnToLogin();
            e.Handled = true;
        }
    }

    private void Window_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is DependencyObject d &&
            FindVisualParent<System.Windows.Controls.Primitives.ButtonBase>(d) != null)
            return;

        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }

    private static T? FindVisualParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = System.Windows.Media.VisualTreeHelper.GetParent(child);
        while (parent is not null)
        {
            if (parent is T match) return match;
            parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
        }
        return null;
    }

    private sealed record AuthUserResult(int KullaniciId, string? KullaniciAdi, string? Rol);

    public sealed class FirmaItem
    {
        public int FirmaId { get; set; }
        public string FirmaAdi { get; set; } = "";
        public override string ToString() => FirmaAdi;
    }
}
