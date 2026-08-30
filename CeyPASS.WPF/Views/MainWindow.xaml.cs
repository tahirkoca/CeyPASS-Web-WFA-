using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CeyPASS.WPF.Views;

public partial class MainWindow : CeypassThemedWindow
{
    private const int SidebarWidthExpanded = 294;
    private const int SidebarWidthCollapsed = 72;
    private const string SidebarCollapsedStorageKey = "ceypass-sidebar-collapsed";

    private readonly ISessionContext _session;
    private Button? _activeMenuButton;
    private bool _sidebarCollapsed;
    private DashboardViewModel? _dashboardVm;
    private string _currentPageKey = "Dashboard";

    public MainWindow()
    {
        InitializeComponent();
        _session = App.Services.GetRequiredService<ISessionContext>();

        ApplyUserHeader();
        using (var scope = App.Services.CreateScope())
        {
            var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
            ApplyMenuVisibility(auth);
        }

        LoadSidebarCollapsedState();
        ApplySidebarState();
        UiToast.RegisterHost(ToastHost);
        UiStatus.Register(LblStatusMessage, LblStatusCount);
        PreviewKeyDown += MainWindow_OnPreviewKeyDown;
        NavigateToDashboard();
    }

    protected override void OnClosed(EventArgs e)
    {
        UiToast.UnregisterHost(ToastHost);
        UiStatus.Unregister();
        base.OnClosed(e);
    }

    private void ApplyUserHeader()
    {
        var name = string.IsNullOrWhiteSpace(_session.AdSoyad) ? "Kullanıcı" : _session.AdSoyad!;
        var role = string.IsNullOrWhiteSpace(_session.RolAdi) ? "Kullanıcı" : _session.RolAdi!;
        LblUserName.Text = name;
        LblUserRole.Text = role;
        var initial = name[..1].ToUpperInvariant();
        LblAvatar.Text = initial;
        LblAvatarCollapsed.Text = initial;
    }

    private void ApplyMenuVisibility(IAuthorizationService auth)
    {
        SetVisible(BtnDashboard, auth.ViewAbility("Dashboard"));
        SetVisible(BtnDepartmanlar, auth.ViewAbility("Departmanlar"));
        SetVisible(BtnPersoneller, auth.ViewAbility("Personeller"));
        SetVisible(BtnPozisyonlar, auth.ViewAbility("Pozisyonlar"));
        SetVisible(BtnFirmalar, auth.ViewAbility("Firmalar"));
        SetVisible(BtnIsyerler, auth.ViewAbility("Isyerler"));
        SetVisible(BtnIzinler, auth.ViewAbility("Izinler"));
        SetVisible(BtnKisiHareket, auth.ViewAbility("KisiHareketler"));
        SetVisible(BtnAylikPuantaj, auth.ViewAbility("AylikPuantaj"));
        SetVisible(BtnRaporlar, auth.ViewAbility("Raporlar"));
        SetVisible(BtnCalismaStatuleri, auth.ViewAbility("CalismaStatuleri"));
        SetVisible(BtnVardiyalar, auth.ViewAbility("Vardiyalar"));
        SetVisible(BtnCihazlar, auth.ViewAbility("Cihazlar"));
        SetVisible(BtnResmiTatiller, auth.ViewAbility("ResmiTatiller"));

        bool admin = _session.RolId == 1;
        ExpAdmin.Visibility = admin ? Visibility.Visible : Visibility.Collapsed;
        BtnAdmin.Visibility = admin ? Visibility.Visible : Visibility.Collapsed;

        ExpPOY.Visibility = AnyVisible(BtnDepartmanlar, BtnPersoneller, BtnPozisyonlar, BtnFirmalar, BtnIsyerler, BtnIzinler)
            ? Visibility.Visible : Visibility.Collapsed;
        ExpEO.Visibility = AnyVisible(BtnKisiHareket, BtnAylikPuantaj, BtnRaporlar)
            ? Visibility.Visible : Visibility.Collapsed;
        ExpVMY.Visibility = AnyVisible(BtnCalismaStatuleri, BtnVardiyalar)
            ? Visibility.Visible : Visibility.Collapsed;
        ExpAyarlar.Visibility = AnyVisible(BtnCihazlar, BtnResmiTatiller)
            ? Visibility.Visible : Visibility.Collapsed;
    }

    private static void SetVisible(Button btn, bool visible)
        => btn.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;

    private static bool AnyVisible(params Button[] buttons)
        => buttons.Any(b => b.Visibility == Visibility.Visible);

    private void Menu_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        var tag = btn.Tag as string ?? "";

        if (tag == "Dashboard")
        {
            NavigateToDashboard();
            return;
        }

        if (tag == "KisiHareketler")
        {
            NavigateToKisiHareket();
            return;
        }

        if (tag == "Departmanlar")
        {
            NavigateToDepartman();
            return;
        }

        if (tag == "Personeller")
        {
            NavigateToPersonel();
            return;
        }

        if (tag == "Pozisyonlar")
        {
            NavigateToPozisyon();
            return;
        }

        if (tag == "Firmalar")
        {
            NavigateToFirma();
            return;
        }

        if (tag == "Isyerler")
        {
            NavigateToIsyeri();
            return;
        }

        if (tag == "Izinler")
        {
            NavigateToIzinler();
            return;
        }

        if (tag == "CalismaStatuleri")
        {
            NavigateToCalismaStatu();
            return;
        }

        if (tag == "Vardiyalar")
        {
            NavigateToVardiya();
            return;
        }

        if (tag == "Cihazlar")
        {
            NavigateToCihaz();
            return;
        }

        if (tag == "ResmiTatiller")
        {
            NavigateToResmiTatil();
            return;
        }

        if (tag == "Admin")
        {
            NavigateToAdminPanel();
            return;
        }

        if (tag == "Raporlar")
        {
            NavigateToRaporlar();
            return;
        }

        if (tag == "AylikPuantaj")
        {
            NavigateToAylikPuantaj();
        }
    }

    private void NavigateToDashboard()
    {
        DetachDashboardReportHandler();

        SetPageHeader("Ana Sayfa", BtnDashboard, "Dashboard");
        var view = App.Services.GetRequiredService<DashboardView>();
        _dashboardVm = view.ViewModel;
        _dashboardVm.ReportRequested += OnDashboardReportRequested;
        ContentHost.Content = view;
        SetActive(BtnDashboard);
        AfterNavigate();
    }

    private void DetachDashboardReportHandler()
    {
        if (_dashboardVm == null) return;
        _dashboardVm.ReportRequested -= OnDashboardReportRequested;
        _dashboardVm = null;
    }

    private void OnDashboardReportRequested(object? sender, ReportRequest req)
        => NavigateToRaporlar(req);

    private void NavigateToRaporlar(ReportRequest? fromDashboard = null)
    {
        DetachDashboardReportHandler();
        SetPageHeader("Raporlar", BtnRaporlar, "Raporlar");
        var view = App.Services.GetRequiredService<RaporlarView>();
        ContentHost.Content = view;
        SetActive(BtnRaporlar);
        if (fromDashboard != null)
            view.OpenFromDashboard(fromDashboard);
        AfterNavigate();
    }

    private void NavigateToKisiHareket()
    {
        SetPageHeader("Kişi Hareketleri", BtnKisiHareket, "KisiHareketler");
        ContentHost.Content = App.Services.GetRequiredService<KisiHareketView>();
        SetActive(BtnKisiHareket);
        AfterNavigate();
    }

    private void NavigateToDepartman()
    {
        SetPageHeader("Departman Tanımlama", BtnDepartmanlar, "Departmanlar");
        ContentHost.Content = App.Services.GetRequiredService<DepartmanView>();
        SetActive(BtnDepartmanlar);
        AfterNavigate();
    }

    private void NavigateToPersonel()
    {
        SetPageHeader("Personel Tanımlama", BtnPersoneller, "Personeller");
        ContentHost.Content = App.Services.GetRequiredService<PersonelView>();
        SetActive(BtnPersoneller);
        AfterNavigate();
    }

    private void NavigateToPozisyon()
    {
        SetPageHeader("Pozisyon Tanımlama", BtnPozisyonlar, "Pozisyonlar");
        ContentHost.Content = App.Services.GetRequiredService<PozisyonView>();
        SetActive(BtnPozisyonlar);
        AfterNavigate();
    }

    private void NavigateToFirma()
    {
        SetPageHeader("Firma Tanımlama", BtnFirmalar, "Firmalar");
        ContentHost.Content = App.Services.GetRequiredService<FirmaView>();
        SetActive(BtnFirmalar);
        AfterNavigate();
    }

    private void NavigateToIsyeri()
    {
        SetPageHeader("İşyeri Tanımlama", BtnIsyerler, "Isyerler");
        ContentHost.Content = App.Services.GetRequiredService<IsyeriView>();
        SetActive(BtnIsyerler);
        AfterNavigate();
    }

    private void NavigateToIzinler()
    {
        SetPageHeader("İzinler", BtnIzinler, "Izinler");
        ContentHost.Content = App.Services.GetRequiredService<IzinlerView>();
        SetActive(BtnIzinler);
        AfterNavigate();
    }

    private void NavigateToCalismaStatu()
    {
        SetPageHeader("Çalışma Statüleri", BtnCalismaStatuleri, "CalismaStatuleri");
        ContentHost.Content = App.Services.GetRequiredService<CalismaStatuView>();
        SetActive(BtnCalismaStatuleri);
        AfterNavigate();
    }

    private void NavigateToVardiya()
    {
        SetPageHeader("Vardiyalar", BtnVardiyalar, "Vardiyalar");
        ContentHost.Content = App.Services.GetRequiredService<VardiyaView>();
        SetActive(BtnVardiyalar);
        AfterNavigate();
    }

    private void NavigateToCihaz()
    {
        SetPageHeader("Cihazlar", BtnCihazlar, "Cihazlar");
        ContentHost.Content = App.Services.GetRequiredService<CihazView>();
        SetActive(BtnCihazlar);
        AfterNavigate();
    }

    private void NavigateToResmiTatil()
    {
        SetPageHeader("Resmi Tatiller", BtnResmiTatiller, "ResmiTatiller");
        ContentHost.Content = App.Services.GetRequiredService<ResmiTatilView>();
        SetActive(BtnResmiTatiller);
        AfterNavigate();
    }

    private void NavigateToAdminPanel()
    {
        if (_session.RolId != 1)
        {
            UiDialog.Warning("Admin Panel yalnızca süper yönetici için açıktır.", "Admin Panel");
            return;
        }

        SetPageHeader("Admin Panel", BtnAdmin, "Admin");
        ContentHost.Content = App.Services.GetRequiredService<AdminPanelView>();
        SetActive(BtnAdmin);
        AfterNavigate();
    }

    private void NavigateToAylikPuantaj()
    {
        SetPageHeader("Aylık Puantaj", BtnAylikPuantaj, "AylikPuantaj");
        ContentHost.Content = App.Services.GetRequiredService<AylikPuantajView>();
        SetActive(BtnAylikPuantaj);
        AfterNavigate();
    }

    private void AfterNavigate()
    {
        UiStatus.Set($"{LblPageTitle.Text} açık");
    }

    private void SetPageHeader(string title, Button? menuBtn, string pageKey)
    {
        _currentPageKey = pageKey;
        LblPageTitle.Text = title;
        var icon = ExtractButtonIcon(menuBtn);
        if (icon != null)
        {
            ImgPageTitle.Source = icon;
            ImgPageTitle.Visibility = Visibility.Visible;
        }
        else
        {
            ImgPageTitle.Visibility = Visibility.Collapsed;
        }
    }

    private static System.Windows.Media.ImageSource? ExtractButtonIcon(Button? btn)
    {
        if (btn?.Content is not StackPanel sp)
            return null;
        foreach (var child in sp.Children)
        {
            if (child is System.Windows.Controls.Image img && img.Source != null)
                return img.Source;
        }
        return null;
    }

    private void SetActive(Button btn)
    {
        if (_activeMenuButton != null)
        {
            var prevTall = IsTallMenuButton(_activeMenuButton);
            _activeMenuButton.Style = (Style)FindResource(prevTall ? "SidebarButtonTall" : "SidebarButton");
            _activeMenuButton.Height = prevTall ? 55 : 45;
        }

        _activeMenuButton = btn;
        var tall = IsTallMenuButton(btn);
        btn.Style = (Style)FindResource("SidebarButtonActive");
        btn.Height = tall ? 55 : 45;
    }

    private static bool IsTallMenuButton(Button btn)
    {
        var tag = btn.Tag as string;
        return tag is "Izinler" or "Raporlar";
    }

    private void BtnLogout_OnClick(object sender, RoutedEventArgs e)
    {
        var login = App.Services.GetRequiredService<LoginWindow>();
        login.Show();
        Close();
    }

    private void BtnSidebarToggle_OnClick(object sender, RoutedEventArgs e)
    {
        _sidebarCollapsed = !_sidebarCollapsed;
        ApplySidebarState();
        SaveSidebarCollapsedState();
    }

    private void ApplySidebarState()
    {
        if (_sidebarCollapsed)
        {
            SidebarColumn.Width = new GridLength(SidebarWidthCollapsed);
            SidebarExpandedContent.Visibility = Visibility.Collapsed;
            SidebarCollapsedContent.Visibility = Visibility.Visible;
            ToggleEdge.Width = 36;
            BtnSidebarToggle.Content = "▶";
        }
        else
        {
            SidebarColumn.Width = new GridLength(SidebarWidthExpanded);
            SidebarExpandedContent.Visibility = Visibility.Visible;
            SidebarCollapsedContent.Visibility = Visibility.Collapsed;
            ToggleEdge.Width = 28;
            BtnSidebarToggle.Content = "◀";
        }
    }

    private void LoadSidebarCollapsedState()
    {
        try
        {
            var path = GetSidebarStatePath();
            if (File.Exists(path))
            {
                var v = File.ReadAllText(path).Trim();
                _sidebarCollapsed = v == "1";
            }
        }
        catch
        {
            _sidebarCollapsed = false;
        }
    }

    private void SaveSidebarCollapsedState()
    {
        try
        {
            var path = GetSidebarStatePath();
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(path, _sidebarCollapsed ? "1" : "0");
        }
        catch
        {
            // ignore persistence errors
        }
    }

    private static string GetSidebarStatePath()
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(root, "CeyPASS", SidebarCollapsedStorageKey + ".txt");
    }

    private void BtnShortcuts_OnClick(object sender, RoutedEventArgs e)
        => ShowShortcutPanel();

    private void BtnCloseShortcuts_OnClick(object sender, RoutedEventArgs e)
        => ShortcutPanel.Visibility = Visibility.Collapsed;

    private void ShortcutPanel_OnBackdropClick(object sender, MouseButtonEventArgs e)
    {
        ShortcutPanel.Visibility = Visibility.Collapsed;
        e.Handled = true;
    }

    private void ShortcutPanel_OnInnerClick(object sender, MouseButtonEventArgs e)
        => e.Handled = true;

    private void ShowShortcutPanel()
    {
        LstShortcuts.ItemsSource = ShortcutCatalog.ForPage(_currentPageKey);
        ShortcutPanel.Visibility = Visibility.Visible;
    }

    private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && ShortcutPanel.Visibility == Visibility.Visible)
        {
            ShortcutPanel.Visibility = Visibility.Collapsed;
            e.Handled = true;
            return;
        }

        if (e.Key == Key.F1)
        {
            ShowShortcutPanel();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.OemQuestion && Keyboard.Modifiers == ModifierKeys.Control)
        {
            ShowShortcutPanel();
            e.Handled = true;
            return;
        }

        // Ctrl+/ (NumPadDivide or Oem2 depending on layout)
        if (Keyboard.Modifiers == ModifierKeys.Control
            && (e.Key == Key.Divide || e.Key == Key.Oem2))
        {
            ShowShortcutPanel();
            e.Handled = true;
        }
    }
}
