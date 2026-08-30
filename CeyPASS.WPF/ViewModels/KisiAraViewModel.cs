using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CeyPASS.WPF.ViewModels;

public sealed class KisiAraViewModel : ObservableObject
{
    private const int PageSize = 25;

    private readonly IServiceScopeFactory _scopes;
    private readonly ISessionContext _session;
    private List<FirmaIsyeriYetkiDTO> _yetkiler = new();
    private bool _isAdmin;
    private bool _suppress;
    private int _page = 1;
    private int _totalCount;

    private Firma? _selectedFirma;
    private LookupItem? _selectedIsyeri;
    private bool _istenCikanlar;
    private bool _puantajYapilan = true;
    private string _adSoyadKart = "";
    private string _sicil = "";
    private string _tc = "";
    private string _email = "";
    private LookupItem? _selectedDepartman;
    private LookupItem? _selectedPozisyon;
    private LookupItem? _selectedStatu;
    private KisiSearchResultItem? _selectedRow;
    private string _pageInfo = "";
    private ImageSource? _previewFoto;
    private string _previewAd = "Ad Soyad: —";
    private string _previewTc = "TC: —";
    private string _previewDepartman = "Departman: —";
    private string _previewPozisyon = "Pozisyon: —";
    private string _previewStatu = "Statü: —";
    private string _previewIsyeri = "İşyeri: —";

    public KisiAraViewModel(IServiceProvider root, KisiAraContext context)
    {
        _scopes = root.GetRequiredService<IServiceScopeFactory>();
        _session = root.GetRequiredService<ISessionContext>();

        Firmalar = new ObservableCollection<Firma>();
        Isyerler = new ObservableCollection<LookupItem>();
        Departmanlar = new ObservableCollection<LookupItem>();
        Pozisyonlar = new ObservableCollection<LookupItem>();
        Statuler = new ObservableCollection<LookupItem>();
        Results = new ObservableCollection<KisiSearchResultItem>();

        SearchCommand = new RelayCommand(() => RunSearch(true));
        ClearCommand = new RelayCommand(ClearFilters);
        PrevPageCommand = new RelayCommand(() => { if (_page > 1) { _page--; RunSearch(false); } }, () => _page > 1);
        NextPageCommand = new RelayCommand(() =>
        {
            int max = Math.Max(1, (int)Math.Ceiling(_totalCount / (double)PageSize));
            if (_page < max) { _page++; RunSearch(false); }
        }, () => _page < Math.Max(1, (int)Math.Ceiling(_totalCount / (double)PageSize)));
        SelectCommand = new RelayCommand(SelectCurrent, () => SelectedRow != null);

        Init(context);
    }

    public ObservableCollection<Firma> Firmalar { get; }
    public ObservableCollection<LookupItem> Isyerler { get; }
    public ObservableCollection<LookupItem> Departmanlar { get; }
    public ObservableCollection<LookupItem> Pozisyonlar { get; }
    public ObservableCollection<LookupItem> Statuler { get; }
    public ObservableCollection<KisiSearchResultItem> Results { get; }

    public string? SelectedPersonelId { get; private set; }
    public KisiAraContext? AppliedContext { get; private set; }
    public Action? CloseOk { get; set; }

    public Firma? SelectedFirma
    {
        get => _selectedFirma;
        set
        {
            if (Equals(_selectedFirma, value)) return;
            SetProperty(ref _selectedFirma, value);
            if (_suppress || value is null) return;
            LoadIsyerler();
            LoadLookups();
            RunSearch(true);
        }
    }

    public LookupItem? SelectedIsyeri
    {
        get => _selectedIsyeri;
        set
        {
            if (Equals(_selectedIsyeri, value)) return;
            SetProperty(ref _selectedIsyeri, value);
            if (_suppress) return;
            RunSearch(true);
        }
    }

    public bool IstenCikanlar
    {
        get => _istenCikanlar;
        set
        {
            if (_istenCikanlar == value) return;
            SetProperty(ref _istenCikanlar, value);
            RaisePropertyChanged(nameof(KartTipiEnabled));
            if (_suppress) return;
            RunSearch(true);
        }
    }

    public bool PuantajYapilan
    {
        get => _puantajYapilan;
        set
        {
            if (_puantajYapilan == value) return;
            SetProperty(ref _puantajYapilan, value);
            if (_suppress || IstenCikanlar) return;
            RunSearch(true);
        }
    }

    public bool KartTipiEnabled => !IstenCikanlar;

    public string AdSoyadKart { get => _adSoyadKart; set => SetProperty(ref _adSoyadKart, value ?? ""); }
    public string Sicil { get => _sicil; set => SetProperty(ref _sicil, value ?? ""); }
    public string Tc { get => _tc; set => SetProperty(ref _tc, value ?? ""); }
    public string Email { get => _email; set => SetProperty(ref _email, value ?? ""); }

    public LookupItem? SelectedDepartman { get => _selectedDepartman; set => SetProperty(ref _selectedDepartman, value); }
    public LookupItem? SelectedPozisyon { get => _selectedPozisyon; set => SetProperty(ref _selectedPozisyon, value); }
    public LookupItem? SelectedStatu { get => _selectedStatu; set => SetProperty(ref _selectedStatu, value); }

    public KisiSearchResultItem? SelectedRow
    {
        get => _selectedRow;
        set
        {
            if (Equals(_selectedRow, value)) return;
            SetProperty(ref _selectedRow, value);
            CommandManager.InvalidateRequerySuggested();
            if (value != null) LoadPreview(value);
            else ClearPreview();
        }
    }

    public string PageInfo { get => _pageInfo; private set => SetProperty(ref _pageInfo, value); }
    public ImageSource? PreviewFoto { get => _previewFoto; private set => SetProperty(ref _previewFoto, value); }
    public string PreviewAd { get => _previewAd; private set => SetProperty(ref _previewAd, value); }
    public string PreviewTc { get => _previewTc; private set => SetProperty(ref _previewTc, value); }
    public string PreviewDepartman { get => _previewDepartman; private set => SetProperty(ref _previewDepartman, value); }
    public string PreviewPozisyon { get => _previewPozisyon; private set => SetProperty(ref _previewPozisyon, value); }
    public string PreviewStatu { get => _previewStatu; private set => SetProperty(ref _previewStatu, value); }
    public string PreviewIsyeri { get => _previewIsyeri; private set => SetProperty(ref _previewIsyeri, value); }

    public ICommand SearchCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand PrevPageCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand SelectCommand { get; }

    private void Init(KisiAraContext context)
    {
        using var scope = _scopes.CreateScope();
        var firmaSvc = scope.ServiceProvider.GetRequiredService<IFirmaService>();
        var yetkiSvc = scope.ServiceProvider.GetRequiredService<IKullaniciFirmaIsyeriYetkiService>();
        _isAdmin = FirmaIsyeriYetkiHelper.IsAdmin(_session.RolId);
        _yetkiler = _session.AktifKullaniciId.HasValue
            ? yetkiSvc.GetYetkiler(_session.AktifKullaniciId.Value) ?? new List<FirmaIsyeriYetkiDTO>()
            : new List<FirmaIsyeriYetkiDTO>();

        var firmalar = FirmaIsyeriYetkiHelper.FilterFirmalar(firmaSvc.GetAll(), _yetkiler, _isAdmin)
            .OrderBy(f => f.FirmaAdi)
            .ToList();

        _suppress = true;
        Firmalar.Clear();
        foreach (var f in firmalar)
            Firmalar.Add(f);

        _selectedFirma = firmalar.FirstOrDefault(f => f.FirmaId == context.FirmaId) ?? firmalar.FirstOrDefault();
        RaisePropertyChanged(nameof(SelectedFirma));
        LoadIsyerler(context.IsyeriId);
        IstenCikanlar = context.SadeceIstenCikanlar;
        PuantajYapilan = context.PuantajYapilirMi != false;
        LoadLookups();
        ClearDetailFiltersOnly();
        _suppress = false;
        RunSearch(true);
    }

    private void LoadIsyerler(int? preferId = null)
    {
        int firmaId = SelectedFirma?.FirmaId ?? 0;
        using var scope = _scopes.CreateScope();
        var lookup = scope.ServiceProvider.GetRequiredService<IKisiEkraniLookUpService>();
        var list = new List<LookupItem> { new() { Id = 0, Ad = "Tümü" } };
        if (firmaId > 0)
            list.AddRange(lookup.GetIsyerleri(firmaId) ?? new List<LookupItem>());

        Isyerler.Clear();
        foreach (var x in list)
            Isyerler.Add(x);

        int want = preferId ?? SelectedIsyeri?.Id ?? 0;
        _selectedIsyeri = Isyerler.FirstOrDefault(x => x.Id == want) ?? Isyerler.FirstOrDefault();
        RaisePropertyChanged(nameof(SelectedIsyeri));
    }

    private void LoadLookups()
    {
        int firmaId = SelectedFirma?.FirmaId ?? 0;
        using var scope = _scopes.CreateScope();
        var lookup = scope.ServiceProvider.GetRequiredService<IKisiEkraniLookUpService>();

        void Fill(ObservableCollection<LookupItem> target, IEnumerable<LookupItem>? src)
        {
            target.Clear();
            target.Add(new LookupItem { Id = 0, Ad = "Tümü" });
            foreach (var x in src ?? Enumerable.Empty<LookupItem>())
                target.Add(x);
        }

        Fill(Departmanlar, lookup.GetDepartmanlar(firmaId));
        Fill(Pozisyonlar, lookup.GetPozisyonlar(firmaId));
        Fill(Statuler, lookup.GetCalismaStatuleri(firmaId));
        SelectedDepartman = Departmanlar.FirstOrDefault();
        SelectedPozisyon = Pozisyonlar.FirstOrDefault();
        SelectedStatu = Statuler.FirstOrDefault();
    }

    private void ClearFilters()
    {
        ClearDetailFiltersOnly();
        RunSearch(true);
    }

    private void ClearDetailFiltersOnly()
    {
        AdSoyadKart = "";
        Sicil = "";
        Tc = "";
        Email = "";
        SelectedDepartman = Departmanlar.FirstOrDefault();
        SelectedPozisyon = Pozisyonlar.FirstOrDefault();
        SelectedStatu = Statuler.FirstOrDefault();
    }

    private void RunSearch(bool resetPage)
    {
        if (SelectedFirma is null) return;
        if (resetPage) _page = 1;

        try
        {
            using var scope = _scopes.CreateScope();
            var kq = scope.ServiceProvider.GetRequiredService<IKisiQueryService>();
            var filter = BuildFilter();
            var rows = kq.SearchKisilerPaged(filter, _page, PageSize, out _totalCount)
                       ?? new List<KisiSearchResultItem>();

            Results.Clear();
            foreach (var r in rows)
                Results.Add(r);

            int maxPage = Math.Max(1, (int)Math.Ceiling(_totalCount / (double)PageSize));
            PageInfo = $"Sayfa {_page} / {maxPage}  ·  Toplam {_totalCount}";
            CommandManager.InvalidateRequerySuggested();

            SelectedRow = Results.FirstOrDefault();
        }
        catch (Exception ex)
        {
            UiDialog.Error("Arama başarısız: " + ex.Message, "Personel Ara");
        }
    }

    private KisiSearchFilter BuildFilter()
    {
        int firmaId = SelectedFirma?.FirmaId ?? 0;
        bool sadeceIstenCikanlar = IstenCikanlar;
        int? isyeriRaw = SelectedIsyeri?.Id is > 0 ? SelectedIsyeri.Id : null;
        var (isyeriId, isyeriIdIn) = FirmaIsyeriYetkiHelper.ResolveKisiQueryIsyeriFilter(
            firmaId, isyeriRaw, _yetkiler, _isAdmin);

        return new KisiSearchFilter
        {
            FirmaId = firmaId,
            PuantajYapilirMi = sadeceIstenCikanlar ? null : PuantajYapilan,
            IsyeriId = isyeriId,
            IsyeriIdIn = isyeriIdIn,
            SadeceIstenCikanlar = sadeceIstenCikanlar,
            AdSoyadKart = NullIfWhite(AdSoyadKart),
            Sicil = NullIfWhite(Sicil),
            TcKimlikNo = NullIfWhite(Tc),
            Email = NullIfWhite(Email),
            DepartmanId = SelectedDepartman?.Id is > 0 ? SelectedDepartman.Id : null,
            PozisyonId = SelectedPozisyon?.Id is > 0 ? SelectedPozisyon.Id : null,
            CalismaStatuId = SelectedStatu?.Id is > 0 ? SelectedStatu.Id : null
        };
    }

    private void LoadPreview(KisiSearchResultItem row)
    {
        try
        {
            using var scope = _scopes.CreateScope();
            var kq = scope.ServiceProvider.GetRequiredService<IKisiQueryService>();
            var (detay, _) = kq.GetDetayOrPuantajsizKart(row.PersonelId);
            if (detay == null)
            {
                ClearPreview();
                return;
            }

            PreviewFoto = BytesToImage(detay.Fotograf);
            var adSoyad = ((detay.Ad ?? "") + " " + (detay.Soyad ?? "")).Trim();
            PreviewAd = string.IsNullOrWhiteSpace(adSoyad) ? "Ad Soyad: —" : adSoyad;
            PreviewTc = "TC: " + (string.IsNullOrWhiteSpace(detay.TcKimlikNo) ? "—" : detay.TcKimlikNo);
            PreviewDepartman = "Departman: " + (row.DepartmanAdi is { Length: > 0 } ? row.DepartmanAdi : "—");
            PreviewPozisyon = "Pozisyon: " + (row.PozisyonAdi is { Length: > 0 } ? row.PozisyonAdi : "—");
            PreviewStatu = "Statü: " + (detay.CalismaStatusuText ?? "—");
            PreviewIsyeri = "İşyeri: " + (row.IsyeriAdi is { Length: > 0 } ? row.IsyeriAdi : "—");
        }
        catch
        {
            ClearPreview();
        }
    }

    private void ClearPreview()
    {
        PreviewFoto = null;
        PreviewAd = "Ad Soyad: —";
        PreviewTc = "TC: —";
        PreviewDepartman = "Departman: —";
        PreviewPozisyon = "Pozisyon: —";
        PreviewStatu = "Statü: —";
        PreviewIsyeri = "İşyeri: —";
    }

    private void SelectCurrent()
    {
        if (SelectedRow == null) return;
        SelectedPersonelId = SelectedRow.PersonelId?.Trim();
        AppliedContext = CaptureContext();
        CloseOk?.Invoke();
    }

    private KisiAraContext CaptureContext()
    {
        int firmaId = SelectedFirma?.FirmaId ?? 0;
        int? isyeriRaw = SelectedIsyeri?.Id is > 0 ? SelectedIsyeri.Id : null;
        var (isyeriId, isyeriIdIn) = FirmaIsyeriYetkiHelper.ResolveKisiQueryIsyeriFilter(
            firmaId, isyeriRaw, _yetkiler, _isAdmin);

        return new KisiAraContext
        {
            FirmaId = firmaId,
            FirmaAdi = SelectedFirma?.FirmaAdi ?? "",
            IsyeriId = isyeriId,
            IsyeriIdIn = isyeriIdIn,
            IsyeriAdi = SelectedIsyeri?.Ad ?? "Tümü",
            SadeceIstenCikanlar = IstenCikanlar,
            PuantajYapilirMi = IstenCikanlar ? null : PuantajYapilan,
            CalismaDurumuMetni = IstenCikanlar ? "İşten Çıkanlar" : "Aktif Çalışanlar",
            PuantajMetni = PuantajYapilan ? "Puantaj Yapılanlar" : "Puantaj Yapılmayanlar"
        };
    }

    private static string? NullIfWhite(string s)
        => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static ImageSource? BytesToImage(byte[]? bytes)
    {
        if (bytes == null || bytes.Length == 0) return null;
        try
        {
            var img = new BitmapImage();
            using var ms = new MemoryStream(bytes);
            img.BeginInit();
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.StreamSource = ms;
            img.EndInit();
            img.Freeze();
            return img;
        }
        catch { return null; }
    }
}
