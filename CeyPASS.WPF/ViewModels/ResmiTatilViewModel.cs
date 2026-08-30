using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CeyPASS.WPF.ViewModels;

public sealed class ResmiTatilViewModel : ObservableObject
{
    private const string PageName = "ResmiTatiller";

    private readonly IServiceScopeFactory _scopes;
    private string _baslangicYili;
    private string _bitisYili;
    private DateTime? _eklenecekTarih = DateTime.Today;
    private string _eklenecekAd = "";
    private string _calismaSaatiText = "0";
    private string? _error;
    private bool _canSabitAktar;
    private bool _canTekilEkle;

    public ResmiTatilViewModel(IServiceProvider root)
    {
        _scopes = root.GetRequiredService<IServiceScopeFactory>();
        Items = new ObservableCollection<ResmiTatilDTO>();

        var y = DateTime.Today.Year.ToString(CultureInfo.InvariantCulture);
        _baslangicYili = y;
        _bitisYili = y;

        SabitAktarCommand = new RelayCommand(SabitAktar, () => CanSabitAktar);
        TekilEkleCommand = new RelayCommand(TekilEkle, () => CanTekilEkle);
        RefreshCommand = new RelayCommand(LoadList);

        RefreshAuth();
        LoadList();
    }

    public ObservableCollection<ResmiTatilDTO> Items { get; }

    public string BaslangicYili
    {
        get => _baslangicYili;
        set => SetProperty(ref _baslangicYili, value);
    }

    public string BitisYili
    {
        get => _bitisYili;
        set => SetProperty(ref _bitisYili, value);
    }

    public DateTime? EklenecekTarih
    {
        get => _eklenecekTarih;
        set => SetProperty(ref _eklenecekTarih, value);
    }

    public string EklenecekAd
    {
        get => _eklenecekAd;
        set => SetProperty(ref _eklenecekAd, value);
    }

    public string CalismaSaatiText
    {
        get => _calismaSaatiText;
        set => SetProperty(ref _calismaSaatiText, value);
    }

    public string? Error
    {
        get => _error;
        private set => SetProperty(ref _error, value);
    }

    public BindableFieldErrors Errors { get; } = new();

    public bool CanSabitAktar
    {
        get => _canSabitAktar;
        private set => SetProperty(ref _canSabitAktar, value);
    }

    public bool CanTekilEkle
    {
        get => _canTekilEkle;
        private set => SetProperty(ref _canTekilEkle, value);
    }

    public ICommand SabitAktarCommand { get; }
    public ICommand TekilEkleCommand { get; }
    public ICommand RefreshCommand { get; }

    private void RefreshAuth()
    {
        using var scope = _scopes.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        CanSabitAktar = auth.Can(PageName, YetkiTipleri.Approve);
        CanTekilEkle = auth.Can(PageName, YetkiTipleri.Create);
        CommandManager.InvalidateRequerySuggested();
    }

    private void LoadList()
    {
        Error = null;
        try
        {
            using var scope = _scopes.CreateScope();
            var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
            if (!auth.ViewAbility(PageName))
            {
                Error = "Resmî Tatiller ekranını görüntüleme yetkiniz yok.";
                Items.Clear();
                return;
            }

            var svc = scope.ServiceProvider.GetRequiredService<IResmiTatilService>();
            var list = svc.GetList() ?? new List<ResmiTatilDTO>();
            Items.Clear();
            foreach (var x in list)
                Items.Add(x);
        }
        catch (Exception ex)
        {
            Error = "Liste yüklenemedi: " + ex.Message;
        }
    }

    private void SabitAktar()
    {
        Errors.Clear();
        Error = null;
        try
        {
            using var scope = _scopes.CreateScope();
            var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
            if (!auth.Can(PageName, YetkiTipleri.Approve))
            {
                Error = "Sabit resmî tatilleri aktarma yetkiniz yok.";
                UiDialog.Warning(Error, PageName);
                return;
            }

            if (!InputHelper.TryParseYear(BaslangicYili, out var basYil))
            {
                Errors.Set("BaslangicYili", "Başlangıç yılı geçerli değil.");
                Error = Errors.FirstMessage;
                return;
            }
            if (!InputHelper.TryParseYear(BitisYili, out var bitYil))
            {
                Errors.Set("BitisYili", "Bitiş yılı geçerli değil.");
                Error = Errors.FirstMessage;
                return;
            }

            var svc = scope.ServiceProvider.GetRequiredService<IResmiTatilService>();
            svc.DoldurSabit(basYil, bitYil);
            UiDialog.Success($"Sabit resmi tatiller {basYil}-{bitYil} aralığı için işlendi.", PageName);
            LoadList();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            UiDialog.Error(Error, PageName);
        }
    }

    private void TekilEkle()
    {
        Errors.Clear();
        Error = null;
        try
        {
            using var scope = _scopes.CreateScope();
            var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
            if (!auth.Can(PageName, YetkiTipleri.Create))
            {
                Error = "Resmî tatil ekleme yetkiniz yok.";
                UiDialog.Warning(Error, PageName);
                return;
            }

            if (!EklenecekTarih.HasValue)
            {
                Errors.Set("EklenecekTarih", "Tarih seçiniz.");
                Error = Errors.FirstMessage;
                return;
            }

            decimal? calismaSaati = null;
            if (!string.IsNullOrWhiteSpace(CalismaSaatiText))
            {
                if (!decimal.TryParse(CalismaSaatiText.Replace(',', '.'),
                        NumberStyles.Number, CultureInfo.InvariantCulture, out var cs)
                    && !decimal.TryParse(CalismaSaatiText, NumberStyles.Number,
                        CultureInfo.CurrentCulture, out cs))
                {
                    Errors.Set("CalismaSaati", "Çalışma saati geçerli değil.");
                    Error = Errors.FirstMessage;
                    return;
                }
                if (cs < 0 || cs > 24)
                {
                    Errors.Set("CalismaSaati", "Çalışma saati 0–24 arasında olmalıdır.");
                    Error = Errors.FirstMessage;
                    return;
                }
                calismaSaati = cs;
            }

            var svc = scope.ServiceProvider.GetRequiredService<IResmiTatilService>();
            svc.KaydetTekil(EklenecekTarih.Value.Date, EklenecekAd, calismaSaati);
            UiDialog.Success("Resmi tatil kaydı işlendi.", PageName);
            LoadList();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            UiDialog.Error(Error, PageName);
        }
    }
}
