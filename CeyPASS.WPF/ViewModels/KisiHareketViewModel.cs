using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows.Input;

namespace CeyPASS.WPF.ViewModels;

public sealed class PersonCheckItem : ObservableObject
{
    private bool _isChecked;
    public int Id { get; init; }
    public string Ad { get; init; } = "";
    public bool IsChecked
    {
        get => _isChecked;
        set => SetProperty(ref _isChecked, value);
    }
}

public sealed class KisiHareketViewModel : ObservableObject
{
    private const string PageName = "KisiHareketler";

    private readonly IServiceScopeFactory _scopes;
    private readonly ISessionContext _session;
    private List<FirmaIsyeriYetkiDTO> _yetkiler = new();
    private bool _isAdmin;
    private bool _suppressFilter;

    private Firma? _selectedFirma;
    private LookupItem? _selectedIsyeri;
    private string _kartTipi = "Puantaj Yapılanlar";
    private DateTime _baslangic = DateTime.Today;
    private DateTime _bitis = DateTime.Today.AddDays(1).AddMinutes(-1);
    private bool _aktif;
    private bool _pasif;
    private bool _yemekhane;
    private DataTable? _rows;
    private DataRowView? _selectedRow;
    private string? _status;
    private bool _busy;
    private bool _showFirmaCombo;
    private bool _canAdd;
    private bool _canEdit;
    private bool _canDelete;
    private bool _canLoadGrid;

    public KisiHareketViewModel(IServiceProvider root)
    {
        _scopes = root.GetRequiredService<IServiceScopeFactory>();
        _session = root.GetRequiredService<ISessionContext>();
        Persons = new ObservableCollection<PersonCheckItem>();
        Firmalar = new ObservableCollection<Firma>();
        Isyerleri = new ObservableCollection<LookupItem>();
        KartTipleri = new ObservableCollection<string>
        {
            "Puantaj Yapılanlar",
            "Puantaj Yapılmayanlar"
        };

        LoadPersonsCommand = new RelayCommand(LoadPersons, () => !Busy);
        LoadGridCommand = new RelayCommand(LoadGrid, () => CanLoadGrid && !Busy);
        AddCommand = new RelayCommand(AddForCheckedPersons, () => CanAdd && !Busy);
        EditCommand = new RelayCommand(UpdateSelected, () => CanEdit && SelectedRow != null && !Busy);
        DeleteCommand = new RelayCommand(DeleteOrActivateSelected, () => CanDelete && SelectedRow != null && !Busy);

        Init();
    }

    public ObservableCollection<PersonCheckItem> Persons { get; }
    public ObservableCollection<Firma> Firmalar { get; }
    public ObservableCollection<LookupItem> Isyerleri { get; }
    public ObservableCollection<string> KartTipleri { get; }
    public BindableFieldErrors Errors { get; } = new();

    public Firma? SelectedFirma
    {
        get => _selectedFirma;
        set
        {
            if (ReferenceEquals(_selectedFirma, value)) return;
            SetProperty(ref _selectedFirma, value);
            if (_suppressFilter || value is null) return;
            PersistFilters();
            LoadIsyerleri(value.FirmaId);
            ClearPersonChecks();
            LoadPersons();
        }
    }

    public LookupItem? SelectedIsyeri
    {
        get => _selectedIsyeri;
        set
        {
            if (ReferenceEquals(_selectedIsyeri, value)) return;
            SetProperty(ref _selectedIsyeri, value);
            if (_suppressFilter) return;
            PersistFilters();
            ClearPersonChecks();
            LoadPersons();
        }
    }

    public string KartTipi
    {
        get => _kartTipi;
        set
        {
            if (_kartTipi == value) return;
            SetProperty(ref _kartTipi, value);
            if (_suppressFilter) return;
            PersistFilters();
            ClearPersonChecks();
            LoadPersons();
        }
    }

    public DateTime Baslangic
    {
        get => _baslangic;
        set
        {
            if (_baslangic == value) return;
            SetProperty(ref _baslangic, value);
            if (_suppressFilter) return;
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
            if (_suppressFilter) return;
            PersistFilters();
        }
    }

    public bool Aktif
    {
        get => _aktif;
        set
        {
            if (_aktif == value) return;
            SetProperty(ref _aktif, value);
            RaisePropertyChanged(nameof(ShowActivateMode));
            if (_suppressFilter) return;
            PersistFilters();
        }
    }

    public bool Pasif
    {
        get => _pasif;
        set
        {
            if (_pasif == value) return;
            SetProperty(ref _pasif, value);
            RaisePropertyChanged(nameof(ShowActivateMode));
            if (_suppressFilter) return;
            PersistFilters();
        }
    }

    public bool Yemekhane
    {
        get => _yemekhane;
        set
        {
            if (_yemekhane == value) return;
            SetProperty(ref _yemekhane, value);
            if (_suppressFilter) return;
            PersistFilters();
        }
    }

    public DataTable? Rows
    {
        get => _rows;
        private set => SetProperty(ref _rows, value);
    }

    public DataRowView? SelectedRow
    {
        get => _selectedRow;
        set
        {
            if (ReferenceEquals(_selectedRow, value)) return;
            SetProperty(ref _selectedRow, value);
            RaisePropertyChanged(nameof(ShowActivateMode));
            CommandManager.InvalidateRequerySuggested();
        }
    }

    /// <summary>
    /// Seçili satır pasifse veya yalnızca pasif filtre açıksa Sil yerine Aktif Et gösterilir.
    /// </summary>
    public bool ShowActivateMode
    {
        get
        {
            if (TryGetSelectedAktifMi(out var aktifMi))
                return !aktifMi;
            return Pasif && !Aktif;
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

    public bool ShowFirmaCombo
    {
        get => _showFirmaCombo;
        private set => SetProperty(ref _showFirmaCombo, value);
    }

    public bool CanAdd
    {
        get => _canAdd;
        private set => SetProperty(ref _canAdd, value);
    }

    public bool CanEdit
    {
        get => _canEdit;
        private set => SetProperty(ref _canEdit, value);
    }

    public bool CanDelete
    {
        get => _canDelete;
        private set => SetProperty(ref _canDelete, value);
    }

    public bool CanLoadGrid
    {
        get => _canLoadGrid;
        private set => SetProperty(ref _canLoadGrid, value);
    }

    public ICommand LoadPersonsCommand { get; }
    public ICommand LoadGridCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    private int SelectedFirmaId =>
        SelectedFirma?.FirmaId ?? _session.AktifFirmaId ?? 0;

    private bool PuantajYapilanlarSecili => KartTipi != "Puantaj Yapılmayanlar";

    private void Init()
    {
        using var scope = _scopes.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        if (!auth.ViewAbility(PageName))
        {
            Status = "Kişi Hareketler ekranını görüntüleme yetkiniz yok.";
            UiDialog.Warning(Status, PageName);
            return;
        }

        _isAdmin = FirmaIsyeriYetkiHelper.IsAdmin(_session.RolId);
        var yetkiSvc = scope.ServiceProvider.GetRequiredService<IKullaniciFirmaIsyeriYetkiService>();
        _yetkiler = _session.AktifKullaniciId.HasValue
            ? yetkiSvc.GetYetkiler((int)_session.AktifKullaniciId.Value) ?? new List<FirmaIsyeriYetkiDTO>()
            : new List<FirmaIsyeriYetkiDTO>();

        RefreshAuth(auth);
        LoadFirmalar(scope.ServiceProvider.GetRequiredService<IFirmaService>());
        LoadIsyerleri(SelectedFirmaId);
        LoadPersons();
    }

    private void RefreshAuth(IAuthorizationService auth)
    {
        CanLoadGrid = auth.Can(PageName, YetkiTipleri.View) || auth.ViewAbility(PageName);
        CanAdd = auth.Can(PageName, YetkiTipleri.Create);
        CanEdit = auth.Can(PageName, YetkiTipleri.Update);
        CanDelete = auth.Can(PageName, YetkiTipleri.Delete);
        CommandManager.InvalidateRequerySuggested();
    }

    private void PersistFilters()
    {
        var kart = KartTipi ?? "";
        if (Yemekhane)
            kart += "|Y";
        PageFilterPrefsStore.Save(PageName, new PageFilterPrefs
        {
            FirmaId = SelectedFirmaId > 0 ? SelectedFirmaId : null,
            IsyeriId = GetSeciliIsyeriFilterId(),
            DateA = Baslangic,
            DateB = Bitis,
            BoolA = Aktif,
            BoolB = Pasif,
            Extra = kart
        });
    }

    private void LoadFirmalar(IFirmaService firmaSvc)
    {
        var firmalar = FirmaIsyeriYetkiHelper.FilterFirmalar(firmaSvc.GetAll(), _yetkiler, _isAdmin)
            .OrderBy(f => f.FirmaAdi)
            .ToList();

        ShowFirmaCombo = (_isAdmin || firmalar.Count > 1) && firmalar.Count > 0;

        Firmalar.Clear();
        foreach (var f in firmalar)
            Firmalar.Add(f);

        var prefs = PageFilterPrefsStore.Load(PageName);
        Firma? sel = null;
        if (prefs?.FirmaId is int pfid)
            sel = Firmalar.FirstOrDefault(f => f.FirmaId == pfid);
        if (sel == null && _session.AktifFirmaId.HasValue)
            sel = Firmalar.FirstOrDefault(f => f.FirmaId == _session.AktifFirmaId.Value);
        sel ??= Firmalar.FirstOrDefault();

        _suppressFilter = true;
        try
        {
            _selectedFirma = sel;
            RaisePropertyChanged(nameof(SelectedFirma));

            if (prefs != null)
            {
                if (prefs.DateA.HasValue)
                    _baslangic = prefs.DateA.Value;
                if (prefs.DateB.HasValue)
                    _bitis = prefs.DateB.Value;
                if (prefs.BoolA.HasValue)
                    _aktif = prefs.BoolA.Value;
                if (prefs.BoolB.HasValue)
                    _pasif = prefs.BoolB.Value;

                if (!string.IsNullOrWhiteSpace(prefs.Extra))
                {
                    var extra = prefs.Extra;
                    _yemekhane = extra.EndsWith("|Y", StringComparison.Ordinal);
                    var kart = _yemekhane ? extra[..^2] : extra;
                    if (KartTipleri.Contains(kart))
                        _kartTipi = kart;
                }

                RaisePropertyChanged(nameof(Baslangic));
                RaisePropertyChanged(nameof(Bitis));
                RaisePropertyChanged(nameof(Aktif));
                RaisePropertyChanged(nameof(Pasif));
                RaisePropertyChanged(nameof(Yemekhane));
                RaisePropertyChanged(nameof(KartTipi));
                RaisePropertyChanged(nameof(ShowActivateMode));
            }
        }
        finally
        {
            _suppressFilter = false;
        }
    }

    private void LoadIsyerleri(int firmaId)
    {
        try
        {
            using var scope = _scopes.CreateScope();
            var ikl = scope.ServiceProvider.GetRequiredService<IKisiEkraniLookUpService>();
            var list = ikl.GetIsyerleri(firmaId) ?? new List<LookupItem>();
            list = FirmaIsyeriYetkiHelper.FilterIsyeriLookup(list, firmaId, _yetkiler, _isAdmin);

            Isyerleri.Clear();
            Isyerleri.Add(new LookupItem { Id = 0, Ad = "Tümü" });
            foreach (var i in list)
                Isyerleri.Add(i);

            var prefs = PageFilterPrefsStore.Load(PageName);
            var preferredIsyeri = prefs?.IsyeriId;
            _suppressFilter = true;
            try
            {
                _selectedIsyeri = (preferredIsyeri.HasValue
                        ? Isyerleri.FirstOrDefault(x => x.Id == preferredIsyeri.Value)
                        : null)
                    ?? Isyerleri.FirstOrDefault();
                RaisePropertyChanged(nameof(SelectedIsyeri));
            }
            finally
            {
                _suppressFilter = false;
            }
        }
        catch (Exception ex)
        {
            Status = "İşyeri listesi hatası: " + ex.Message;
        }
    }

    private int? GetSeciliIsyeriFilterId()
    {
        var id = SelectedIsyeri?.Id ?? 0;
        return id <= 0 ? null : id;
    }

    private void ClearPersonChecks()
    {
        foreach (var p in Persons)
            p.IsChecked = false;
    }

    private string BosListeUyariMesaji(int? seciliIsyeriId)
    {
        if (seciliIsyeriId.HasValue && seciliIsyeriId.Value > 0)
        {
            var ad = SelectedIsyeri?.Ad?.Trim();
            return string.IsNullOrEmpty(ad)
                ? "Seçili işyerde personel bulunamadı."
                : $"\"{ad}\" işyerinde personel bulunamadı.";
        }
        return "Seçili filtreye uygun personel bulunamadı.";
    }

    private void LoadPersons()
    {
        try
        {
            Busy = true;
            using var scope = _scopes.CreateScope();
            var kq = scope.ServiceProvider.GetRequiredService<IKisiQueryService>();

            int firmaId = SelectedFirmaId;
            if (firmaId <= 0)
            {
                Persons.Clear();
                Status = "Aktif firma seçili değil.";
                return;
            }

            var seciliIsyeri = GetSeciliIsyeriFilterId();
            var (isyeriId, isyeriIdIn) = FirmaIsyeriYetkiHelper.ResolveKisiQueryIsyeriFilter(
                firmaId, seciliIsyeri, _yetkiler, _isAdmin);

            var data = kq.GetAktifKisilerByFirma(firmaId, null, PuantajYapilanlarSecili, isyeriId, isyeriIdIn)
                       ?? new List<KisiListItem>();

            Persons.Clear();
            foreach (var k in data)
            {
                if (string.IsNullOrWhiteSpace(k.PersonelId) || string.IsNullOrWhiteSpace(k.AdSoyad))
                    continue;
                if (!int.TryParse(k.PersonelId, out var id) || id <= 0)
                    continue;
                Persons.Add(new PersonCheckItem { Id = id, Ad = k.AdSoyad });
            }

            if (Persons.Count == 0)
                Status = BosListeUyariMesaji(seciliIsyeri);
            else
                Status = $"{Persons.Count} personel yüklendi.";
        }
        catch (Exception ex)
        {
            Status = "Personel listesi hatası: " + ex.Message;
            UiDialog.Error(ex.Message, PageName);
        }
        finally
        {
            Busy = false;
        }
    }

    private void LoadGrid()
    {
        try
        {
            Busy = true;
            using var scope = _scopes.CreateScope();
            var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
            if (!auth.Can(PageName, YetkiTipleri.View) && !auth.ViewAbility(PageName))
            {
                UiDialog.Warning("Grid görüntüleme yetkiniz yok.", PageName);
                return;
            }

            var kh = scope.ServiceProvider.GetRequiredService<IKisiHareketService>();
            var ids = Persons.Where(p => p.IsChecked).Select(p => p.Id).ToList();
            var dt = kh.GetByPersons(ids, Baslangic, Bitis, Aktif, Pasif, Yemekhane, SelectedFirmaId);
            Rows = dt;
            SelectedRow = null;
            Status = $"{dt?.Rows.Count ?? 0} hareket";
            PersistFilters();
        }
        catch (Exception ex)
        {
            Status = "Hareket yükleme hatası: " + ex.Message;
            UiDialog.Error(ex.Message, PageName);
        }
        finally
        {
            Busy = false;
        }
    }

    private void AddForCheckedPersons()
    {
        Errors.Clear();
        using var scope = _scopes.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        if (!auth.Can(PageName, YetkiTipleri.Create))
        {
            UiDialog.Warning("Manuel ekleme yetkiniz yok.", PageName);
            return;
        }

        var ids = Persons.Where(p => p.IsChecked).Select(p => p.Id).ToList();
        if (ids.Count == 0)
        {
            Errors.Set("Persons", "Kişi seçiniz.");
            Status = Errors.FirstMessage;
            return;
        }

        if (!Views.HareketInputWindow.Show(null, DateTime.Now, "Giriş", out var tarih, out var tip))
            return;

        var kh = scope.ServiceProvider.GetRequiredService<IKisiHareketService>();
        int ok = 0, fail = 0;
        foreach (var pid in ids)
        {
            try
            {
                if (kh.InsertManual(SelectedFirmaId, pid, tarih, tip)) ok++;
                else fail++;
            }
            catch { fail++; }
        }

        UiDialog.Success($"Başarılı Ekleme: {ok}, Hata: {fail}", PageName);
        LoadGrid();
    }

    private void UpdateSelected()
    {
        using var scope = _scopes.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        if (!auth.Can(PageName, YetkiTipleri.Update))
        {
            UiDialog.Warning("Güncelleme yetkiniz yok.", PageName);
            return;
        }

        var row = SelectedRow?.Row;
        if (row is null) return;

        int id = Convert.ToInt32(row["Id"]);
        DateTime tarih = Convert.ToDateTime(row["Tarih"]);
        string tip = Convert.ToString(row["Tip"]) ?? "Giriş";

        if (!Views.HareketInputWindow.Show(null, tarih, tip, out tarih, out tip))
            return;

        var kh = scope.ServiceProvider.GetRequiredService<IKisiHareketService>();
        if (kh.UpdateManual(id, tarih, tip))
        {
            UiDialog.Success("Hareket güncellendi.", PageName);
            LoadGrid();
        }
        else
            UiDialog.Warning("Güncelleme başarısız.", PageName);
    }

    private void DeleteOrActivateSelected()
    {
        using var scope = _scopes.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        if (!auth.Can(PageName, YetkiTipleri.Delete))
        {
            UiDialog.Warning(ShowActivateMode ? "Aktif etme yetkiniz yok." : "Silme yetkiniz yok.", PageName);
            return;
        }

        var row = SelectedRow?.Row;
        if (row is null) return;

        int id = Convert.ToInt32(row["Id"]);
        var kh = scope.ServiceProvider.GetRequiredService<IKisiHareketService>();

        if (ShowActivateMode)
        {
            if (!UiDialog.Confirm("Kayıt tekrar aktif edilsin mi?", "Onay", yesText: "Aktif et", noText: "Vazgeç"))
                return;

            if (kh.AktifYap(id))
            {
                UiDialog.Success("Hareket tekrar aktif edildi.", PageName);
                LoadGrid();
            }
            else
                UiDialog.Warning("İşlem başarısız.", PageName);
            return;
        }

        if (!UiDialog.Confirm("Kayıt pasif edilsin mi?", "Onay", yesText: "Pasife al", noText: "Vazgeç"))
            return;

        if (kh.PasifYap(id))
        {
            UiDialog.SuccessWithUndo("Hareket pasife alındı.", () =>
            {
                using var s2 = _scopes.CreateScope();
                if (s2.ServiceProvider.GetRequiredService<IKisiHareketService>().AktifYap(id))
                {
                    LoadGrid();
                    UiDialog.Success("Geri alındı.", PageName);
                }
                else
                    UiDialog.Warning("Geri alma başarısız.", PageName);
            }, PageName);
            LoadGrid();
        }
        else
            UiDialog.Warning("İşlem başarısız.", PageName);
    }

    private static bool TryGetSelectedAktifMi(DataRowView? selected, out bool aktifMi)
    {
        aktifMi = true;
        var row = selected?.Row;
        if (row?.Table.Columns.Contains("AktifMi") != true)
            return false;
        var v = row["AktifMi"];
        if (v is null || v == DBNull.Value)
            return false;
        aktifMi = Convert.ToBoolean(v);
        return true;
    }

    private bool TryGetSelectedAktifMi(out bool aktifMi)
        => TryGetSelectedAktifMi(SelectedRow, out aktifMi);
}
