using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CeyPASS.WPF.ViewModels;

public sealed class DashboardViewModel : ObservableObject
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ISessionContext _session;
    private bool _loadingFirma;
    private Firma? _selectedFirma;
    private Visibility _firmaFilterVisibility = Visibility.Collapsed;
    private string? _error;

    private string _girisYapan = "0";
    private string _iceridekiler = "0";
    private string _gecKalanlar = "0";
    private string _disaridakiler = "0";
    private string _devamsizlar = "0";
    private string _izinliler = "0";
    private string _iseBaslayan = "0";
    private string _istenAyrilan = "0";

    private string _hdrGec = "En Geç Gelen Personeller";
    private string _hdrDogum = "Bu Ay Doğum Günü Olanlar";
    private string _hdrIseBaslayan = "Bu Ay İşe Başlayanlar";
    private string _hdrIstenAyrilan = "Bu Ay İşten Ayrılanlar";

    public DashboardViewModel(IServiceProvider root)
    {
        _scopeFactory = root.GetRequiredService<IServiceScopeFactory>();
        _session = root.GetRequiredService<ISessionContext>();

        Firmalar = new ObservableCollection<Firma>();
        LateList = new ObservableCollection<GecKalanlarDashboard>();
        Birthdays = new ObservableCollection<BirthdayRow>();
        NewHires = new ObservableCollection<IseBaslayanlarDashboard>();
        Resignations = new ObservableCollection<IstenAyrilanlarDashboard>();

        RefreshCommand = new RelayCommand(LoadDashboard);
        OpenReportCommand = new RelayCommand(p =>
        {
            if (p is DashboardReportTypeHelper t)
                RaiseReport(t);
            else if (p is string s && Enum.TryParse(s, out DashboardReportTypeHelper parsed))
                RaiseReport(parsed);
        });
        LoadFirmalar();
        LoadDashboard();
    }

    public event EventHandler<ReportRequest>? ReportRequested;

    public ObservableCollection<Firma> Firmalar { get; }
    public ObservableCollection<GecKalanlarDashboard> LateList { get; }
    public ObservableCollection<BirthdayRow> Birthdays { get; }
    public ObservableCollection<IseBaslayanlarDashboard> NewHires { get; }
    public ObservableCollection<IstenAyrilanlarDashboard> Resignations { get; }

    public ICommand RefreshCommand { get; }
    public ICommand OpenReportCommand { get; }

    public void RaiseReport(DashboardReportTypeHelper type)
    {
        var today = DateTime.Today;
        DateTime baslangic = today;
        DateTime bitis = today;
        if (type is DashboardReportTypeHelper.IseBaslayanlar or DashboardReportTypeHelper.IstenAyrilanlar)
            baslangic = new DateTime(today.Year, today.Month, 1);

        int? firmaId = SelectedFirma?.FirmaId ?? _session.AktifFirmaId;

        ReportRequested?.Invoke(this, new ReportRequest
        {
            Type = type,
            Baslangic = baslangic,
            Bitis = bitis,
            FirmaId = firmaId
        });
    }

    public Visibility FirmaFilterVisibility
    {
        get => _firmaFilterVisibility;
        set => SetProperty(ref _firmaFilterVisibility, value);
    }

    public Firma? SelectedFirma
    {
        get => _selectedFirma;
        set
        {
            if (Equals(_selectedFirma, value)) return;
            SetProperty(ref _selectedFirma, value);
            if (_loadingFirma || value is null)
                return;
            if (_session.AktifFirmaId != value.FirmaId)
            {
                _session.AktifFirmaId = value.FirmaId;
                LoadDashboard();
            }
        }
    }

    public string? Error
    {
        get => _error;
        set => SetProperty(ref _error, value);
    }

    public string GirisYapan { get => _girisYapan; set => SetProperty(ref _girisYapan, value); }
    public string Iceridekiler { get => _iceridekiler; set => SetProperty(ref _iceridekiler, value); }
    public string GecKalanlar { get => _gecKalanlar; set => SetProperty(ref _gecKalanlar, value); }
    public string Disaridakiler { get => _disaridakiler; set => SetProperty(ref _disaridakiler, value); }
    public string Devamsizlar { get => _devamsizlar; set => SetProperty(ref _devamsizlar, value); }
    public string Izinliler { get => _izinliler; set => SetProperty(ref _izinliler, value); }
    public string IseBaslayan { get => _iseBaslayan; set => SetProperty(ref _iseBaslayan, value); }
    public string IstenAyrilan { get => _istenAyrilan; set => SetProperty(ref _istenAyrilan, value); }

    public string HdrGec { get => _hdrGec; set => SetProperty(ref _hdrGec, value); }
    public string HdrDogum { get => _hdrDogum; set => SetProperty(ref _hdrDogum, value); }
    public string HdrIseBaslayan { get => _hdrIseBaslayan; set => SetProperty(ref _hdrIseBaslayan, value); }
    public string HdrIstenAyrilan { get => _hdrIstenAyrilan; set => SetProperty(ref _hdrIstenAyrilan, value); }

    private void LoadFirmalar()
    {
        Error = null;
        _loadingFirma = true;
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
            if (!auth.ViewAbility("Dashboard"))
            {
                Error = "Ana sayfa ekranını görüntüleme yetkiniz yok.";
                FirmaFilterVisibility = Visibility.Collapsed;
                return;
            }

            var firmaSvc = scope.ServiceProvider.GetRequiredService<IFirmaService>();
            var yetkiSvc = scope.ServiceProvider.GetRequiredService<IKullaniciFirmaIsyeriYetkiService>();
            bool isAdmin = FirmaIsyeriYetkiHelper.IsAdmin(_session.RolId);
            var yetkiler = yetkiSvc.GetYetkiler(_session.AktifKullaniciId ?? 0);
            var firmalar = FirmaIsyeriYetkiHelper.FilterFirmalar(firmaSvc.GetAll(), yetkiler, isAdmin)
                .OrderBy(f => f.FirmaAdi)
                .ToList();

            Firmalar.Clear();
            foreach (var f in firmalar)
                Firmalar.Add(f);

            if (isAdmin || firmalar.Count > 1)
            {
                FirmaFilterVisibility = firmalar.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                var current = firmalar.FirstOrDefault(f => f.FirmaId == _session.AktifFirmaId)
                              ?? firmalar.FirstOrDefault();
                _selectedFirma = current;
                RaisePropertyChanged(nameof(SelectedFirma));
                if (current != null)
                    _session.AktifFirmaId = current.FirmaId;
            }
            else
            {
                FirmaFilterVisibility = Visibility.Collapsed;
                var only = firmalar.FirstOrDefault();
                if (only != null)
                    _session.AktifFirmaId = only.FirmaId;
            }
        }
        catch (Exception ex)
        {
            Error = "Firma listesi yüklenemedi: " + ex.Message;
            FirmaFilterVisibility = Visibility.Collapsed;
        }
        finally
        {
            _loadingFirma = false;
        }
    }

    private void LoadDashboard()
    {
        Error = null;
        if (!_session.AktifFirmaId.HasValue)
        {
            Error = "Aktif firma seçili değil.";
            return;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
            if (!auth.ViewAbility("Dashboard"))
            {
                Error = "Ana sayfa ekranını görüntüleme yetkiniz yok.";
                return;
            }

            var svc = scope.ServiceProvider.GetRequiredService<IDashboardService>();
            var ds = svc.GetDashboardForToday(_session.AktifFirmaId.Value);
            FillCards(ds.Cards);
            FillGrids(ds);
        }
        catch (Exception ex)
        {
            Error = "Dashboard verileri yüklenirken hata: " + ex.Message;
        }
    }

    private void FillCards(AnaEkranKartlariDashboard cards)
    {
        GirisYapan = cards.GirisYapan.ToString("N0");
        Iceridekiler = cards.Iceridekiler.ToString("N0");
        GecKalanlar = cards.GecKalanlar.ToString("N0");
        Disaridakiler = cards.Disaridakiler.ToString("N0");
        Devamsizlar = cards.Devamsizlar.ToString("N0");
        Izinliler = cards.Izinli.ToString("N0");
        IseBaslayan = cards.IseBaslayan.ToString("N0");
        IstenAyrilan = cards.IstenAyrilan.ToString("N0");
    }

    private void FillGrids(DashboardResult ds)
    {
        LateList.Clear();
        foreach (var x in ds.LateList)
            LateList.Add(x);

        Birthdays.Clear();
        var tr = CultureInfo.GetCultureInfo("tr-TR");
        foreach (var x in ds.Birthdays)
        {
            Birthdays.Add(new BirthdayRow
            {
                Ad = x.Ad,
                Soyad = x.Soyad,
                DogumGunu = x.BuYilDogumGunu.ToString("dd MMMM", tr)
            });
        }

        NewHires.Clear();
        foreach (var x in ds.NewHires)
            NewHires.Add(x);

        Resignations.Clear();
        foreach (var x in ds.Resignations)
            Resignations.Add(x);

        HdrGec = $"En Geç Gelen Personeller ({ds.LateList.Count} Kayıt)";
        HdrDogum = $"Bu Ay Doğum Günü Olanlar ({ds.Birthdays.Count} Kişi)";
        HdrIseBaslayan = $"Bu Ay İşe Başlayanlar ({ds.NewHires.Count} Kişi)";
        HdrIstenAyrilan = $"Bu Ay İşten Ayrılanlar ({ds.Resignations.Count} Kişi)";
    }

    public sealed class BirthdayRow
    {
        public string Ad { get; set; } = "";
        public string Soyad { get; set; } = "";
        public string DogumGunu { get; set; } = "";
    }
}
