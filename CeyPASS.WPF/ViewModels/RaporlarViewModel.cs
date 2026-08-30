using System.Collections.ObjectModel;
using System.Data;
using System.Windows.Input;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CeyPASS.WPF.ViewModels;

public sealed class RaporCheckItem : ObservableObject
{
    private bool _isChecked;
    public int Id { get; init; }
    public string Ad { get; init; } = "";
    public object? Tag { get; init; }
    public bool IsChecked
    {
        get => _isChecked;
        set => SetProperty(ref _isChecked, value);
    }
}

public sealed class RaporlarViewModel : ObservableObject
{
    private const string PageName = "Raporlar";

    private readonly IServiceScopeFactory _scopes;
    private readonly ISessionContext _session;
    private DataTable? _report;
    private IReadOnlyList<string> _aktifParametreler = Array.Empty<string>();
    private int? _loadedMultiFirmaId;
    private RaporParametreHelper.MultiSelectKind _loadedMultiKind;
    private bool _isAdmin;

    private RaporTanimi? _selectedRapor;
    private LookupItem? _selectedFirma;
    private DateTime _baslangic = DateTime.Today;
    private DateTime _bitis = DateTime.Today;
    private DataView? _rows;
    private string? _status;
    private bool _busy;
    private bool _showMultiSelect;
    private string _multiSelectTitle = "İşyerleri";
    private bool _canGetir;

    public RaporlarViewModel(IServiceProvider root)
    {
        _scopes = root.GetRequiredService<IServiceScopeFactory>();
        _session = root.GetRequiredService<ISessionContext>();
        Raporlar = new ObservableCollection<RaporTanimi>();
        Firmalar = new ObservableCollection<LookupItem>();
        MultiSelectItems = new ObservableCollection<RaporCheckItem>();

        GetirCommand = new RelayCommand(async () => await GetirAsync(), () => CanGetir && !Busy);

        Init();
    }

    public ObservableCollection<RaporTanimi> Raporlar { get; }
    public ObservableCollection<LookupItem> Firmalar { get; }
    public ObservableCollection<RaporCheckItem> MultiSelectItems { get; }

    public RaporTanimi? SelectedRapor
    {
        get => _selectedRapor;
        set
        {
            if (ReferenceEquals(_selectedRapor, value)) return;
            SetProperty(ref _selectedRapor, value);
            ApplyRaporParametreUi();
            PersistFilters();
        }
    }

    public LookupItem? SelectedFirma
    {
        get => _selectedFirma;
        set
        {
            if (ReferenceEquals(_selectedFirma, value)) return;
            SetProperty(ref _selectedFirma, value);
            _loadedMultiFirmaId = null;
            EnsureMultiSelectGuncel();
            PersistFilters();
        }
    }

    private int SelectedFirmaId => SelectedFirma?.Id ?? 0;

    public DateTime Baslangic
    {
        get => _baslangic;
        set
        {
            if (_baslangic == value) return;
            SetProperty(ref _baslangic, value);
            PersistFilters();
        }
    }

    public DateTime Bitis
    {
        get => _bitis;
        set
        {
            if (_bitis == value) return;
            SetProperty(ref _bitis, value);
            PersistFilters();
        }
    }

    public DataView? Rows
    {
        get => _rows;
        private set
        {
            SetProperty(ref _rows, value);
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string? Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public bool Busy
    {
        get => _busy;
        private set
        {
            SetProperty(ref _busy, value);
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public bool ShowMultiSelect
    {
        get => _showMultiSelect;
        private set => SetProperty(ref _showMultiSelect, value);
    }

    public string MultiSelectTitle
    {
        get => _multiSelectTitle;
        private set => SetProperty(ref _multiSelectTitle, value);
    }

    public bool CanGetir
    {
        get => _canGetir;
        private set => SetProperty(ref _canGetir, value);
    }

    public ICommand GetirCommand { get; }

    public void OpenFromDashboard(ReportRequest req)
    {
        if (!CanGetir)
            return;

        if (req.FirmaId is > 0)
        {
            var firma = Firmalar.FirstOrDefault(f => f.Id == req.FirmaId.Value);
            if (firma != null)
                SelectedFirma = firma;
        }

        Baslangic = req.Baslangic.Date;
        Bitis = req.Bitis.Date;

        string? proc = GetProcName(req.Type);
        if (!string.IsNullOrEmpty(proc))
        {
            var rapor = Raporlar.FirstOrDefault(r =>
                string.Equals(r.ProcedureAdi, proc, StringComparison.OrdinalIgnoreCase));
            if (rapor != null)
                SelectedRapor = rapor;
        }

        _ = GetirAsync();
    }

    private static string? GetProcName(DashboardReportTypeHelper type) => type switch
    {
        DashboardReportTypeHelper.Izinliler => "sp_GunlukIzinlilerRaporu",
        DashboardReportTypeHelper.Disaridakiler => "sp_AnlikDisaridakilerRaporu",
        DashboardReportTypeHelper.HareketiBulunanlar => "sp_GunlukHareketiBulunanlarRaporu",
        DashboardReportTypeHelper.Iceridekiler => "sp_AnlikIceridekilerRaporu",
        DashboardReportTypeHelper.GecKalanlar => "sp_GunlukGecKalanlarRaporu",
        DashboardReportTypeHelper.Devamsizlar => "sp_DevamsizlarRaporu",
        DashboardReportTypeHelper.IseBaslayanlar => "sp_IseBaslayanlarRaporu",
        DashboardReportTypeHelper.IstenAyrilanlar => "sp_IstenAyrilanlarRaporu",
        _ => null
    };

    private void Init()
    {
        using var scope = _scopes.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        if (!auth.ViewAbility(PageName))
        {
            Status = "Raporlar ekranını görüntüleme yetkiniz yok.";
            UiDialog.Warning(Status, PageName);
            return;
        }

        CanGetir = auth.Can(PageName, YetkiTipleri.View) || auth.ViewAbility(PageName);
        CommandManager.InvalidateRequerySuggested();

        _isAdmin = FirmaIsyeriYetkiHelper.IsAdmin(_session.RolId);
        LoadFirmalar(scope.ServiceProvider);

        var svc = scope.ServiceProvider.GetRequiredService<IRaporService>();
        var list = svc.GetirRaporlar() ?? new List<RaporTanimi>();
        Raporlar.Clear();
        foreach (var r in list)
            Raporlar.Add(r);

        RestoreRaporPrefs();
        if (_selectedRapor == null)
            _selectedRapor = Raporlar.FirstOrDefault();
        RaisePropertyChanged(nameof(SelectedRapor));
        ApplyRaporParametreUi();
        Status = $"{Raporlar.Count} rapor tanımı yüklendi.";
    }

    private void LoadFirmalar(IServiceProvider sp)
    {
        var yetkiSvc = sp.GetRequiredService<IKullaniciFirmaIsyeriYetkiService>();
        var yetkiler = _session.AktifKullaniciId.HasValue
            ? yetkiSvc.GetYetkiler((int)_session.AktifKullaniciId.Value) ?? new List<FirmaIsyeriYetkiDTO>()
            : new List<FirmaIsyeriYetkiDTO>();

        var firmaSvc = sp.GetRequiredService<IFirmaService>();
        var filtered = FirmaIsyeriYetkiHelper.FilterFirmalar(firmaSvc.GetAll(), yetkiler, _isAdmin)
            .OrderBy(f => f.FirmaAdi)
            .Select(f => new LookupItem { Id = f.FirmaId, Ad = f.FirmaAdi ?? $"Firma {f.FirmaId}" })
            .ToList();

        Firmalar.Clear();
        if (_isAdmin)
            Firmalar.Add(new LookupItem { Id = 0, Ad = "TÜMÜ" });
        foreach (var f in filtered)
            Firmalar.Add(f);

        var prefs = PageFilterPrefsStore.Load(PageName);
        LookupItem? sel = null;
        if (prefs?.FirmaId is int pfid)
            sel = Firmalar.FirstOrDefault(f => f.Id == pfid);
        if (sel == null && _session.AktifFirmaId.HasValue && _session.AktifFirmaId.Value > 0)
            sel = Firmalar.FirstOrDefault(f => f.Id == _session.AktifFirmaId.Value);
        sel ??= Firmalar.FirstOrDefault(f => f.Id > 0);
        sel ??= Firmalar.FirstOrDefault();

        _selectedFirma = sel;
        RaisePropertyChanged(nameof(SelectedFirma));

        if (prefs?.DateA.HasValue == true)
            Baslangic = prefs.DateA.Value.Date;
        if (prefs?.DateB.HasValue == true)
            Bitis = prefs.DateB.Value.Date;
    }

    private void RestoreRaporPrefs()
    {
        var prefs = PageFilterPrefsStore.Load(PageName);
        if (string.IsNullOrWhiteSpace(prefs?.Extra)) return;
        var rapor = Raporlar.FirstOrDefault(r =>
            string.Equals(r.ProcedureAdi, prefs.Extra, StringComparison.OrdinalIgnoreCase)
            || string.Equals(r.RaporAdi, prefs.Extra, StringComparison.OrdinalIgnoreCase));
        if (rapor == null) return;
        _selectedRapor = rapor;
        RaisePropertyChanged(nameof(SelectedRapor));
    }

    private void PersistFilters()
    {
        PageFilterPrefsStore.Save(PageName, new PageFilterPrefs
        {
            FirmaId = SelectedFirma?.Id,
            DateA = Baslangic.Date,
            DateB = Bitis.Date,
            Extra = SelectedRapor?.ProcedureAdi ?? SelectedRapor?.RaporAdi
        });
    }

    private void ApplyRaporParametreUi()
    {
        using var scope = _scopes.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IRaporService>();
        string? proc = SelectedRapor?.ProcedureAdi;
        _aktifParametreler = string.IsNullOrEmpty(proc)
            ? Array.Empty<string>()
            : svc.GetProcedureParameterNames(proc);
        _loadedMultiFirmaId = null;
        EnsureMultiSelectGuncel();
    }

    private void EnsureMultiSelectGuncel()
    {
        int firmaId = SelectedFirmaId;
        if (firmaId <= 0)
        {
            MultiSelectItems.Clear();
            ShowMultiSelect = false;
            _loadedMultiFirmaId = firmaId;
            _loadedMultiKind = RaporParametreHelper.MultiSelectKind.None;
            return;
        }

        var kind = RaporParametreHelper.GetMultiSelect(_aktifParametreler);
        if (_loadedMultiFirmaId == firmaId && _loadedMultiKind == kind)
            return;

        if (kind == RaporParametreHelper.MultiSelectKind.Cihaz)
            LoadCihazlar(firmaId);
        else if (kind == RaporParametreHelper.MultiSelectKind.Isyeri)
            LoadIsyerleri(firmaId);
        else
        {
            MultiSelectItems.Clear();
            ShowMultiSelect = false;
            _loadedMultiFirmaId = firmaId;
            _loadedMultiKind = kind;
        }
    }

    private void LoadCihazlar(int firmaId)
    {
        try
        {
            using var scope = _scopes.CreateScope();
            var cihazSvc = scope.ServiceProvider.GetRequiredService<ICihazService>();
            var list = cihazSvc.GetListe(true, firmaId) ?? new List<CihazListDTO>();
            MultiSelectItems.Clear();
            foreach (var c in list)
            {
                MultiSelectItems.Add(new RaporCheckItem
                {
                    Id = c.CihazId,
                    Ad = c.CihazAdi ?? $"Cihaz {c.CihazId}",
                    Tag = c
                });
            }
            MultiSelectTitle = "Cihazlar (işaretlenmezse firmanın tüm aktif cihazları)";
            ShowMultiSelect = MultiSelectItems.Count > 0;
            _loadedMultiFirmaId = firmaId;
            _loadedMultiKind = RaporParametreHelper.MultiSelectKind.Cihaz;
        }
        catch (Exception ex)
        {
            Status = "Cihaz listesi hatası: " + ex.Message;
        }
    }

    private void LoadIsyerleri(int firmaId)
    {
        try
        {
            using var scope = _scopes.CreateScope();
            var yetkiSvc = scope.ServiceProvider.GetRequiredService<IKullaniciFirmaIsyeriYetkiService>();
            var yetkiler = _session.AktifKullaniciId.HasValue
                ? yetkiSvc.GetYetkiler((int)_session.AktifKullaniciId.Value) ?? new List<FirmaIsyeriYetkiDTO>()
                : new List<FirmaIsyeriYetkiDTO>();
            var lookup = scope.ServiceProvider.GetRequiredService<IKisiEkraniLookUpService>();
            var list = lookup.GetIsyerleri(firmaId) ?? new List<LookupItem>();
            list = FirmaIsyeriYetkiHelper.FilterIsyeriLookup(list, firmaId, yetkiler, _isAdmin);

            MultiSelectItems.Clear();
            foreach (var iy in list.Where(x => x.Id > 0))
            {
                MultiSelectItems.Add(new RaporCheckItem
                {
                    Id = iy.Id,
                    Ad = iy.Ad ?? $"İşyeri {iy.Id}",
                    Tag = iy
                });
            }
            MultiSelectTitle = "İşyerleri (işaretlenmezse yetkili tüm işyerler)";
            ShowMultiSelect = MultiSelectItems.Count > 0;
            _loadedMultiFirmaId = firmaId;
            _loadedMultiKind = RaporParametreHelper.MultiSelectKind.Isyeri;
        }
        catch (Exception ex)
        {
            Status = "İşyeri listesi hatası: " + ex.Message;
        }
    }

    private async Task GetirAsync()
    {
        using var scope = _scopes.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        if (!auth.Can(PageName, YetkiTipleri.View) && !auth.ViewAbility(PageName))
        {
            UiDialog.Warning("Rapor görüntüleme yetkiniz yok.", PageName);
            return;
        }

        string? procedureAdi = SelectedRapor?.ProcedureAdi;
        if (string.IsNullOrEmpty(procedureAdi))
        {
            UiDialog.Warning("Lütfen bir rapor seçin.", PageName);
            return;
        }

        int firmaId = SelectedFirmaId;
        if (firmaId < 0 || (firmaId == 0 && !_isAdmin))
        {
            UiDialog.Warning("Firma bilgisi bulunamadı.", PageName);
            return;
        }

        var yetkiSvc = scope.ServiceProvider.GetRequiredService<IKullaniciFirmaIsyeriYetkiService>();
        var yetkiler = _session.AktifKullaniciId.HasValue
            ? yetkiSvc.GetYetkiler((int)_session.AktifKullaniciId.Value) ?? new List<FirmaIsyeriYetkiDTO>()
            : new List<FirmaIsyeriYetkiDTO>();

        if (firmaId > 0 && !FirmaIsyeriYetkiHelper.IsFirmaAuthorized(firmaId, yetkiler, _isAdmin))
        {
            UiDialog.Warning("Seçili firma için rapor görüntüleme yetkiniz yok.", PageName);
            return;
        }

        if (Baslangic.Date > Bitis.Date)
        {
            UiDialog.Warning("Başlangıç tarihi, bitiş tarihinden büyük olamaz.", PageName);
            return;
        }

        var kind = RaporParametreHelper.GetMultiSelect(_aktifParametreler);
        string isyeriIdCsv = "";
        string cihazIdCsv = "";

        if (firmaId > 0)
        {
            if (kind == RaporParametreHelper.MultiSelectKind.Isyeri)
            {
                if (!TryBuildIsyeriCsv(scope.ServiceProvider, firmaId, yetkiler, _isAdmin, out isyeriIdCsv))
                    return;
            }
            else if (kind == RaporParametreHelper.MultiSelectKind.Cihaz)
            {
                cihazIdCsv = string.Join(",", MultiSelectItems.Where(x => x.IsChecked).Select(x => x.Id));
            }
        }

        var parametreler = new Dictionary<string, object>
        {
            { RaporParametreHelper.FirmaIdList, firmaId > 0 ? firmaId.ToString() : "" },
            { RaporParametreHelper.IsyeriIdList, isyeriIdCsv },
            { RaporParametreHelper.CihazIdList, cihazIdCsv },
            { RaporParametreHelper.TarihBaslangic, RaporTarihHelper.ToReportRangeStart(Baslangic) },
            { RaporParametreHelper.TarihBitis, RaporTarihHelper.ToReportRangeEnd(Bitis) }
        };

        var raporAdi = SelectedRapor?.RaporAdi;

        try
        {
            Busy = true;
            Status = "Rapor yükleniyor...";
            await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);

            var report = await Task.Run(() =>
            {
                using var bgScope = _scopes.CreateScope();
                var svc = bgScope.ServiceProvider.GetRequiredService<IRaporService>();
                return svc.CalistirRapor(procedureAdi, parametreler);
            });

            _report = report;
            Rows = _report?.DefaultView;
            Status = $"{_report?.Rows.Count ?? 0} satır — {raporAdi}";
        }
        catch (Exception ex)
        {
            Status = "Hata: " + ex.Message;
            UiDialog.Error("Hata oluştu: " + ex.Message, PageName);
        }
        finally
        {
            Busy = false;
        }
    }

    private bool TryBuildIsyeriCsv(
        IServiceProvider sp,
        int firmaId,
        List<FirmaIsyeriYetkiDTO> yetkiler,
        bool isAdmin,
        out string isyeriIdCsv)
    {
        isyeriIdCsv = "";
        var kq = sp.GetRequiredService<IKullaniciQueryService>();
        var yetkiSvc = sp.GetRequiredService<IKullaniciFirmaIsyeriYetkiService>();
        var firmaIsyeriIds = kq.GetFirmayaAitIsyeriIdleri(firmaId) ?? new List<int>();
        var maxCsv = yetkiSvc.BuildIsyeriIdListCsv(firmaId, yetkiler, isAdmin, firmaIsyeriIds);
        var selectedIds = MultiSelectItems.Where(x => x.IsChecked).Select(x => x.Id).Where(id => id > 0).Distinct().ToList();
        var (csv, status) = FirmaIsyeriYetkiHelper.ResolveRaporIsyeriIdListCsv(
            firmaId, selectedIds, maxCsv, yetkiler, isAdmin);

        if (status == FirmaIsyeriYetkiHelper.RaporIsyeriListStatus.UnauthorizedSelection)
        {
            UiDialog.Warning("Seçilen işyerlerden bazıları için yetkiniz yok.", PageName);
            return false;
        }
        if (status == FirmaIsyeriYetkiHelper.RaporIsyeriListStatus.NoAccess)
        {
            UiDialog.Warning("Seçili firma için rapor görüntüleme yetkiniz yok.", PageName);
            return false;
        }

        isyeriIdCsv = csv ?? "";
        return true;
    }
}

