using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.Extensions.DependencyInjection;

namespace CeyPASS.WPF.ViewModels;

public sealed class GuncellemeBildirimViewModel : ObservableObject
{
    private const string PageName = "Guncelleme";

    private readonly IServiceScopeFactory _scopes;
    private string _versiyon;
    private DateTime _yayinTarihi = DateTime.Now;
    private string _guncellemeTipi = "Minor";
    private string _ekNotlar = "";
    private string _yeniOzellikText = "";
    private string _iyilestirmeText = "";
    private string _hataDuzeltmeText = "";
    private string _kritikDegisiklikText = "";
    private string? _selectedYeni;
    private string? _selectedIyilestirme;
    private string? _selectedHata;
    private string? _selectedKritik;
    private string? _error;
    private bool _busy;
    private bool _canSend;
    private bool _canPreview;

    public GuncellemeBildirimViewModel(IServiceProvider root)
    {
        _scopes = root.GetRequiredService<IServiceScopeFactory>();
        _versiyon = AppVersion.ProductVersion;

        TipSecenekleri = new ObservableCollection<string> { "Major", "Minor", "Bugfix" };
        YeniOzellikler = new ObservableCollection<string>();
        Iyilestirmeler = new ObservableCollection<string>();
        HataDuzeltmeleri = new ObservableCollection<string>();
        KritikDegisiklikler = new ObservableCollection<string>();

        AddYeniCommand = new RelayCommand(AddYeni);
        RemoveYeniCommand = new RelayCommand(RemoveYeni, () => SelectedYeni is not null);
        AddIyilestirmeCommand = new RelayCommand(AddIyilestirme);
        RemoveIyilestirmeCommand = new RelayCommand(RemoveIyilestirme, () => SelectedIyilestirme is not null);
        AddHataCommand = new RelayCommand(AddHata);
        RemoveHataCommand = new RelayCommand(RemoveHata, () => SelectedHata is not null);
        AddKritikCommand = new RelayCommand(AddKritik);
        RemoveKritikCommand = new RelayCommand(RemoveKritik, () => SelectedKritik is not null);
        PreviewCommand = new RelayCommand(Preview, () => CanPreview && !Busy);
        SendCommand = new RelayCommand(async () => await SendAsync(), () => CanSend && !Busy);

        RefreshAuth();
    }

    public ObservableCollection<string> TipSecenekleri { get; }
    public ObservableCollection<string> YeniOzellikler { get; }
    public ObservableCollection<string> Iyilestirmeler { get; }
    public ObservableCollection<string> HataDuzeltmeleri { get; }
    public ObservableCollection<string> KritikDegisiklikler { get; }

    public string Versiyon
    {
        get => _versiyon;
        set => SetProperty(ref _versiyon, value);
    }

    public DateTime YayinTarihi
    {
        get => _yayinTarihi;
        set => SetProperty(ref _yayinTarihi, value);
    }

    public string GuncellemeTipi
    {
        get => _guncellemeTipi;
        set => SetProperty(ref _guncellemeTipi, value);
    }

    public string EkNotlar
    {
        get => _ekNotlar;
        set => SetProperty(ref _ekNotlar, value);
    }

    public string YeniOzellikText
    {
        get => _yeniOzellikText;
        set => SetProperty(ref _yeniOzellikText, value);
    }

    public string IyilestirmeText
    {
        get => _iyilestirmeText;
        set => SetProperty(ref _iyilestirmeText, value);
    }

    public string HataDuzeltmeText
    {
        get => _hataDuzeltmeText;
        set => SetProperty(ref _hataDuzeltmeText, value);
    }

    public string KritikDegisiklikText
    {
        get => _kritikDegisiklikText;
        set => SetProperty(ref _kritikDegisiklikText, value);
    }

    public string? SelectedYeni
    {
        get => _selectedYeni;
        set
        {
            SetProperty(ref _selectedYeni, value);
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string? SelectedIyilestirme
    {
        get => _selectedIyilestirme;
        set
        {
            SetProperty(ref _selectedIyilestirme, value);
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string? SelectedHata
    {
        get => _selectedHata;
        set
        {
            SetProperty(ref _selectedHata, value);
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string? SelectedKritik
    {
        get => _selectedKritik;
        set
        {
            SetProperty(ref _selectedKritik, value);
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string? Error
    {
        get => _error;
        private set => SetProperty(ref _error, value);
    }

    public BindableFieldErrors Errors { get; } = new();

    public bool Busy
    {
        get => _busy;
        private set
        {
            SetProperty(ref _busy, value);
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public bool CanSend
    {
        get => _canSend;
        private set => SetProperty(ref _canSend, value);
    }

    public bool CanPreview
    {
        get => _canPreview;
        private set => SetProperty(ref _canPreview, value);
    }

    public ICommand AddYeniCommand { get; }
    public ICommand RemoveYeniCommand { get; }
    public ICommand AddIyilestirmeCommand { get; }
    public ICommand RemoveIyilestirmeCommand { get; }
    public ICommand AddHataCommand { get; }
    public ICommand RemoveHataCommand { get; }
    public ICommand AddKritikCommand { get; }
    public ICommand RemoveKritikCommand { get; }
    public ICommand PreviewCommand { get; }
    public ICommand SendCommand { get; }

    private void RefreshAuth()
    {
        using var scope = _scopes.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        if (!auth.ViewAbility(PageName))
        {
            Error = "Güncelleme Bildirimi ekranını görüntüleme yetkiniz yok.";
            CanSend = false;
            CanPreview = false;
            return;
        }

        CanSend = auth.Can(PageName, YetkiTipleri.Create);
        CanPreview = auth.Can(PageName, YetkiTipleri.View) || auth.ViewAbility(PageName);
        CommandManager.InvalidateRequerySuggested();
    }

    private void AddYeni()
    {
        Errors.Clear();
        Error = null;
        if (!Errors.Require("YeniOzellikText", YeniOzellikText, "Lütfen bir özellik açıklaması girin."))
        {
            Error = Errors.FirstMessage;
            return;
        }
        YeniOzellikler.Add(YeniOzellikText.Trim());
        YeniOzellikText = "";
    }

    private void RemoveYeni()
    {
        if (SelectedYeni is null) { UiDialog.Warning("Lütfen silmek için bir öğe seçin.", PageName); return; }
        YeniOzellikler.Remove(SelectedYeni);
        SelectedYeni = null;
    }

    private void AddIyilestirme()
    {
        Errors.Clear();
        Error = null;
        if (!Errors.Require("IyilestirmeText", IyilestirmeText, "Lütfen bir iyileştirme açıklaması girin."))
        {
            Error = Errors.FirstMessage;
            return;
        }
        Iyilestirmeler.Add(IyilestirmeText.Trim());
        IyilestirmeText = "";
    }

    private void RemoveIyilestirme()
    {
        if (SelectedIyilestirme is null) { UiDialog.Warning("Lütfen silmek için bir öğe seçin.", PageName); return; }
        Iyilestirmeler.Remove(SelectedIyilestirme);
        SelectedIyilestirme = null;
    }

    private void AddHata()
    {
        Errors.Clear();
        Error = null;
        if (!Errors.Require("HataDuzeltmeText", HataDuzeltmeText, "Lütfen bir hata düzeltmesi açıklaması girin."))
        {
            Error = Errors.FirstMessage;
            return;
        }
        HataDuzeltmeleri.Add(HataDuzeltmeText.Trim());
        HataDuzeltmeText = "";
    }

    private void RemoveHata()
    {
        if (SelectedHata is null) { UiDialog.Warning("Lütfen silmek için bir öğe seçin.", PageName); return; }
        HataDuzeltmeleri.Remove(SelectedHata);
        SelectedHata = null;
    }

    private void AddKritik()
    {
        Errors.Clear();
        Error = null;
        if (!Errors.Require("KritikDegisiklikText", KritikDegisiklikText, "Lütfen bir kritik değişiklik açıklaması girin."))
        {
            Error = Errors.FirstMessage;
            return;
        }
        KritikDegisiklikler.Add(KritikDegisiklikText.Trim());
        KritikDegisiklikText = "";
    }

    private void RemoveKritik()
    {
        if (SelectedKritik is null) { UiDialog.Warning("Lütfen silmek için bir öğe seçin.", PageName); return; }
        KritikDegisiklikler.Remove(SelectedKritik);
        SelectedKritik = null;
    }

    private GuncellemeNotifikasyonDTO BuildDto() => new()
    {
        VersiyonNumarasi = (Versiyon ?? "").Trim(),
        YayinTarihi = YayinTarihi,
        GuncellemeTipi = GuncellemeTipi ?? "",
        EkNotlar = (EkNotlar ?? "").Trim(),
        YeniOzellikler = YeniOzellikler.ToList(),
        Iyilestirmeler = Iyilestirmeler.ToList(),
        HataDuzeltmeleri = HataDuzeltmeleri.ToList(),
        KritikDegisiklikler = KritikDegisiklikler.ToList()
    };

    private bool Validate(GuncellemeNotifikasyonDTO dto)
    {
        Errors.Clear();
        Error = null;
        Errors.Require("Versiyon", dto.VersiyonNumarasi, "Lütfen versiyon numarası girin.");
        Errors.Require("GuncellemeTipi", dto.GuncellemeTipi, "Lütfen güncelleme tipini seçin.");
        if (dto.YeniOzellikler.Count == 0 && dto.Iyilestirmeler.Count == 0
            && dto.HataDuzeltmeleri.Count == 0 && dto.KritikDegisiklikler.Count == 0)
        {
            Errors.Set("YeniOzellikText", "En az bir kategoriye madde eklemelisiniz.");
        }
        if (Errors.HasErrors)
        {
            Error = Errors.FirstMessage;
            return false;
        }
        return true;
    }

    private void Preview()
    {
        Error = null;
        try
        {
            var dto = BuildDto();
            if (!Validate(dto)) return;

            using var scope = _scopes.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<INotificationService>();
            string html = svc.OnizlemeHtmlOlustur(dto);
            string tempPath = Path.Combine(Path.GetTempPath(), "ceypass_email_onizleme.html");
            File.WriteAllText(tempPath, html, System.Text.Encoding.UTF8);
            Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
            UiDialog.Success("Email önizlemesi tarayıcınızda açıldı.", PageName);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            UiDialog.Error($"Önizleme oluşturulurken hata: {ex.Message}", PageName);
        }
    }

    private async Task SendAsync()
    {
        Error = null;
        try
        {
            var dto = BuildDto();
            if (!Validate(dto)) return;

            if (!UiDialog.Confirm(
                    "Güncelleme bildirimi tüm kullanıcılara gönderilecek. Emin misiniz?",
                    "Onay",
                    yesText: "Gönder",
                    noText: "Vazgeç"))
                return;

            Busy = true;
            using var scope = _scopes.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<INotificationService>();
            bool ok = await svc.GuncellemeNotifikasyonuGonderAsync(dto);
            if (ok)
                UiDialog.Success("Güncelleme bildirimi başarıyla gönderildi!", PageName);
            else
                UiDialog.Error("Mail gönderilirken bir hata oluştu. Lütfen ayarları kontrol edin.", PageName);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            UiDialog.Error(ex.Message, PageName);
        }
        finally
        {
            Busy = false;
        }
    }
}
