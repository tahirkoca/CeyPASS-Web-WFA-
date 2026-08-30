using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using CeyPASS.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace CeyPASS.WPF.ViewModels;

public sealed class VardiyaCheckItem : ObservableObject
{
    private int _id;
    private string _ad = "";
    private bool _secili;

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string Ad
    {
        get => _ad;
        set => SetProperty(ref _ad, value ?? "");
    }

    /// <summary>Çoklu vardiya seçimi — CheckBox.IsChecked DP adı ile çakışmasın diye Secili.</summary>
    public bool Secili
    {
        get => _secili;
        set => SetProperty(ref _secili, value);
    }
}

public sealed class PersonelViewModel : ObservableObject
{
    private enum ScreenMode { View, Add, Edit, Exit }

    private const string PageName = "Personeller";

    private readonly IServiceScopeFactory _scopes;
    private readonly ISessionContext _session;

    private ScreenMode _mode = ScreenMode.View;
    private bool _suppressSelection;
    private bool _suppressFilter;
    private string? _originalPersonelId;

    private Firma? _selectedFirma;
    private LookupItem? _selectedIsyeriFilter;
    private bool _istenCikanlar;
    private bool _puantajYapilan = true;

    private KisiListItem? _selectedKisi;

    private string _adSoyad = "";
    private string _sicilNo = "";
    private string _kartNo = "";
    private string _tcKimlikNo = "";
    private string _firmaDisiKartNo = "";
    private string _email = "";
    private string _cepTel = "";
    private DateTime _iseGiris = DateTime.Today;
    private DateTime? _istenCikis;
    private DateTime? _dogumTarihi;
    private bool _hasDogum;
    private bool _hasIstenCikis;

    private LookupItem? _selectedDepartman;
    private LookupItem? _selectedBolum;
    private LookupItem? _selectedPozisyon;
    private LookupItem? _selectedIsyeri;
    private LookupItem? _selectedFirmaDetail;
    private LookupItem? _selectedCalismaStatu;

    private bool _firmaPersoneli = true;
    private bool _puantajYapilir = true;
    private bool _yemekHakki;
    private bool _ziyaretci;
    private bool _aracKarti;
    private bool _taseron;
    private int _yemekAdedi;

    private bool _fieldsReadOnly = true;
    private bool _listEnabled = true;
    private bool _showSaveCancel;
    private bool _canAdd;
    private bool _canEdit;
    private bool _canDelete;
    private bool _canPhoto;
    private bool _fotoDirty;
    private byte[]? _fotografBytes;
    private ImageSource? _fotoImage;

    private string? _status;
    private string? _error;

    public PersonelViewModel(IServiceProvider root)
    {
        _scopes = root.GetRequiredService<IServiceScopeFactory>();
        _session = root.GetRequiredService<ISessionContext>();

        Firmalar = new ObservableCollection<Firma>();
        Isyerler = new ObservableCollection<LookupItem>();
        Kisiler = new ObservableCollection<KisiListItem>();
        Departmanlar = new ObservableCollection<LookupItem>();
        Bolumler = new ObservableCollection<LookupItem>();
        Pozisyonlar = new ObservableCollection<LookupItem>();
        IsyerleriForm = new ObservableCollection<LookupItem>();
        FirmalarForm = new ObservableCollection<LookupItem>();
        CalismaStatuleri = new ObservableCollection<LookupItem>();
        Vardiyalar = new ObservableCollection<VardiyaCheckItem>();

        AddCommand = new RelayCommand(EnterAddMode, () => CanAdd);
        EditCommand = new RelayCommand(EnterEditMode, () => CanEdit);
        DeleteOrActivateCommand = new RelayCommand(DeleteOrActivate, () => CanDelete);
        SaveCommand = new RelayCommand(Save, () => ShowSaveCancel);
        CancelCommand = new RelayCommand(EnterViewMode, () => ShowSaveCancel);
        RefreshCommand = new RelayCommand(Refresh);
        FilterChangedCommand = new RelayCommand(() => LoadList(), () => ListEnabled && _mode == ScreenMode.View);
        AddPhotoCommand = new RelayCommand(AddPhoto, () => CanPhoto);
        RemovePhotoCommand = new RelayCommand(RemovePhoto, () => CanPhoto);
        SearchPersonCommand = new RelayCommand(OpenKisiAra, () => ListEnabled && _mode == ScreenMode.View);

        Refresh();
    }

    public ObservableCollection<Firma> Firmalar { get; }
    public ObservableCollection<LookupItem> Isyerler { get; }
    public ObservableCollection<KisiListItem> Kisiler { get; }
    public ObservableCollection<LookupItem> Departmanlar { get; }
    public ObservableCollection<LookupItem> Bolumler { get; }
    public ObservableCollection<LookupItem> Pozisyonlar { get; }
    public ObservableCollection<LookupItem> IsyerleriForm { get; }
    public ObservableCollection<LookupItem> FirmalarForm { get; }
    public ObservableCollection<LookupItem> CalismaStatuleri { get; }
    public ObservableCollection<VardiyaCheckItem> Vardiyalar { get; }

    public Firma? SelectedFirma
    {
        get => _selectedFirma;
        set
        {
            if (Equals(_selectedFirma, value)) return;
            SetProperty(ref _selectedFirma, value);
            if (_suppressFilter || value is null) return;
            OnFirmaFilterChanged();
            PersistFilters();
        }
    }

    public LookupItem? SelectedIsyeriFilter
    {
        get => _selectedIsyeriFilter;
        set
        {
            if (Equals(_selectedIsyeriFilter, value)) return;
            SetProperty(ref _selectedIsyeriFilter, value);
            if (_suppressFilter || _mode != ScreenMode.View) return;
            _originalPersonelId = null;
            LoadList(preserveSelection: false);
            PersistFilters();
        }
    }

    public bool IstenCikanlar
    {
        get => _istenCikanlar;
        set
        {
            if (_istenCikanlar == value) return;
            SetProperty(ref _istenCikanlar, value);
            RaisePropertyChanged(nameof(DurumIndex));
            RaisePropertyChanged(nameof(KartTipiEnabled));
            RaisePropertyChanged(nameof(PuantajFilterEnabled));
            RaisePropertyChanged(nameof(DeleteButtonText));
            if (_suppressFilter || _mode != ScreenMode.View) return;
            _originalPersonelId = null;
            LoadList(preserveSelection: false);
            RefreshToolbar();
            PersistFilters();
        }
    }

    public int DurumIndex
    {
        get => IstenCikanlar ? 1 : 0;
        set => IstenCikanlar = value == 1;
    }

    public bool PuantajYapilan
    {
        get => _puantajYapilan;
        set
        {
            if (_puantajYapilan == value) return;
            SetProperty(ref _puantajYapilan, value);
            if (_suppressFilter || _mode != ScreenMode.View || IstenCikanlar) return;
            _originalPersonelId = null;
            LoadList(preserveSelection: false);
            PersistFilters();
        }
    }

    public KisiListItem? SelectedKisi
    {
        get => _selectedKisi;
        set
        {
            if (Equals(_selectedKisi, value)) return;
            SetProperty(ref _selectedKisi, value);
            if (_suppressSelection) return;
            if (_mode == ScreenMode.View)
            {
                if (value is null || string.IsNullOrWhiteSpace(value.PersonelId))
                {
                    _originalPersonelId = null;
                    ClearFields();
                }
                else
                {
                    _originalPersonelId = value.PersonelId.Trim();
                    LoadDetail(value.PersonelId);
                }
                RefreshToolbar();
            }
        }
    }

    public string AdSoyad { get => _adSoyad; set => SetProperty(ref _adSoyad, value ?? ""); }
    public string SicilNo { get => _sicilNo; set => SetProperty(ref _sicilNo, value ?? ""); }
    public string KartNo { get => _kartNo; set => SetProperty(ref _kartNo, value ?? ""); }
    public string TcKimlikNo { get => _tcKimlikNo; set => SetProperty(ref _tcKimlikNo, value ?? ""); }
    public string FirmaDisiKartNo { get => _firmaDisiKartNo; set => SetProperty(ref _firmaDisiKartNo, value ?? ""); }
    public string Email { get => _email; set => SetProperty(ref _email, value ?? ""); }
    public string CepTel { get => _cepTel; set => SetProperty(ref _cepTel, value ?? ""); }

    public DateTime IseGiris
    {
        get => _iseGiris;
        set => SetProperty(ref _iseGiris, value);
    }

    public DateTime? IstenCikis
    {
        get => _istenCikis;
        set => SetProperty(ref _istenCikis, value);
    }

    public DateTime? DogumTarihi
    {
        get => _dogumTarihi;
        set => SetProperty(ref _dogumTarihi, value);
    }

    public bool HasDogum
    {
        get => _hasDogum;
        set => SetProperty(ref _hasDogum, value);
    }

    public bool HasIstenCikis
    {
        get => _hasIstenCikis;
        set
        {
            if (_hasIstenCikis == value) return;
            SetProperty(ref _hasIstenCikis, value);
            RaiseDependentUi();
        }
    }

    public LookupItem? SelectedDepartman { get => _selectedDepartman; set => SetProperty(ref _selectedDepartman, value); }
    public LookupItem? SelectedBolum { get => _selectedBolum; set => SetProperty(ref _selectedBolum, value); }
    public LookupItem? SelectedPozisyon { get => _selectedPozisyon; set => SetProperty(ref _selectedPozisyon, value); }
    public LookupItem? SelectedIsyeri { get => _selectedIsyeri; set => SetProperty(ref _selectedIsyeri, value); }
    public LookupItem? SelectedFirmaDetail { get => _selectedFirmaDetail; set => SetProperty(ref _selectedFirmaDetail, value); }
    public LookupItem? SelectedCalismaStatu { get => _selectedCalismaStatu; set => SetProperty(ref _selectedCalismaStatu, value); }

    public bool FirmaPersoneli
    {
        get => _firmaPersoneli;
        set
        {
            if (_firmaPersoneli == value) return;
            SetProperty(ref _firmaPersoneli, value);
            RaiseDependentUi();
        }
    }

    public bool PuantajYapilir
    {
        get => _puantajYapilir;
        set
        {
            if (_puantajYapilir == value) return;
            SetProperty(ref _puantajYapilir, value);
            RaiseDependentUi();
        }
    }

    public bool YemekHakki
    {
        get => _yemekHakki;
        set
        {
            if (_yemekHakki == value) return;
            SetProperty(ref _yemekHakki, value);
            if (!value && YemekAdedi != 0)
                YemekAdedi = 0;
            RaiseDependentUi();
        }
    }

    public bool Ziyaretci { get => _ziyaretci; set => SetProperty(ref _ziyaretci, value); }
    public bool AracKarti { get => _aracKarti; set => SetProperty(ref _aracKarti, value); }
    public bool Taseron { get => _taseron; set => SetProperty(ref _taseron, value); }

    public int YemekAdedi
    {
        get => _yemekAdedi;
        set => SetProperty(ref _yemekAdedi, value);
    }

    public bool FieldsReadOnly
    {
        get => _fieldsReadOnly;
        private set
        {
            if (_fieldsReadOnly == value) return;
            SetProperty(ref _fieldsReadOnly, value);
            RaisePropertyChanged(nameof(YemekAdediEnabled));
            RaisePropertyChanged(nameof(VardiyaEditable));
        }
    }

    public bool ListEnabled
    {
        get => _listEnabled;
        private set
        {
            SetProperty(ref _listEnabled, value);
            RaisePropertyChanged(nameof(PuantajFilterEnabled));
        }
    }

    public bool ShowSaveCancel
    {
        get => _showSaveCancel;
        private set => SetProperty(ref _showSaveCancel, value);
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

    public bool CanPhoto
    {
        get => _canPhoto;
        private set
        {
            SetProperty(ref _canPhoto, value);
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public ImageSource? FotoImage
    {
        get => _fotoImage;
        private set => SetProperty(ref _fotoImage, value);
    }

    public string DeleteButtonText => IstenCikanlar ? "Aktif Et" : "İşten Çıkar";
    public bool KartTipiEnabled => !IstenCikanlar;
    public bool PuantajFilterEnabled => ListEnabled && !IstenCikanlar;

    /// <summary>İşten çıkış tarihi yoksa WFA'daki gibi "Aktif Çalışıyor..." gösterilir.</summary>
    public bool ShowAktifCalisiyorText => !HasIstenCikis && _mode != ScreenMode.Exit;
    public bool ShowIstenCikisDatePicker => HasIstenCikis || _mode == ScreenMode.Exit;
    public string CalismaDurumuText => HasIstenCikis ? "İşten çıkmış" : "Aktif çalışıyor";
    public string AktifCalisiyorPlaceholder => "Aktif Çalışıyor...";

    public bool FirmaDisiEnabled
    {
        get
        {
            bool kuralIzinVeriyor = !FirmaPersoneli || (FirmaPersoneli && !PuantajYapilir);
            return kuralIzinVeriyor || _mode == ScreenMode.Edit;
        }
    }

    public bool YemekAdediEnabled => YemekHakki && !FieldsReadOnly;
    public bool IstenCikisEnabled => _mode == ScreenMode.Exit;
    public bool VardiyaEditable => !FieldsReadOnly;

    public string? Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public string? Error
    {
        get => _error;
        private set => SetProperty(ref _error, value);
    }

    public BindableFieldErrors Errors { get; } = new();

    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteOrActivateCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand FilterChangedCommand { get; }
    public ICommand AddPhotoCommand { get; }
    public ICommand RemovePhotoCommand { get; }
    public ICommand SearchPersonCommand { get; }

    private int GetSeciliFirmaId()
        => SelectedFirma?.FirmaId ?? _session.AktifFirmaId ?? 0;

    private int? GetSeciliIsyeriFilterId()
    {
        var id = SelectedIsyeriFilter?.Id ?? 0;
        return id <= 0 ? null : id;
    }

    private void Refresh()
    {
        LoadFirmalar();
        LoadLookups();
        LoadList();
    }

    private void PersistFilters()
    {
        PageFilterPrefsStore.Save(PageName, new PageFilterPrefs
        {
            FirmaId = GetSeciliFirmaId() > 0 ? GetSeciliFirmaId() : null,
            IsyeriId = GetSeciliIsyeriFilterId(),
            BoolA = IstenCikanlar,
            BoolB = PuantajYapilan
        });
    }

    private void OnFirmaFilterChanged()
    {
        if (_mode != ScreenMode.View) return;
        _originalPersonelId = null;
        ClearFields();
        LoadLookups();
        LoadList();
    }

    private void LoadFirmalar()
    {
        Error = null;
        _suppressFilter = true;
        try
        {
            using var scope = _scopes.CreateScope();
            var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
            if (!auth.ViewAbility(PageName))
            {
                Error = "Personeller ekranını görüntüleme yetkiniz yok.";
                Firmalar.Clear();
                Kisiler.Clear();
                ClearFields();
                RefreshToolbar(auth);
                return;
            }

            var firmaSvc = scope.ServiceProvider.GetRequiredService<IFirmaService>();
            var yetkiSvc = scope.ServiceProvider.GetRequiredService<IKullaniciFirmaIsyeriYetkiService>();
            bool isAdmin = FirmaIsyeriYetkiHelper.IsAdmin(_session.RolId);
            var yetkiler = _session.AktifKullaniciId.HasValue
                ? yetkiSvc.GetYetkiler(_session.AktifKullaniciId.Value) ?? new List<FirmaIsyeriYetkiDTO>()
                : new List<FirmaIsyeriYetkiDTO>();

            var liste = FirmaIsyeriYetkiHelper.FilterFirmalar(firmaSvc.GetAll(), yetkiler, isAdmin)
                .OrderBy(f => f.FirmaAdi)
                .ToList();

            Firmalar.Clear();
            foreach (var f in liste)
                Firmalar.Add(f);

            var prefs = PageFilterPrefsStore.Load(PageName);
            var preferredId = prefs?.FirmaId;
            var current = (preferredId.HasValue
                              ? liste.FirstOrDefault(f => f.FirmaId == preferredId.Value)
                              : null)
                          ?? liste.FirstOrDefault(f => f.FirmaId == _session.AktifFirmaId)
                          ?? liste.FirstOrDefault();
            _selectedFirma = current;
            RaisePropertyChanged(nameof(SelectedFirma));

            if (prefs != null)
            {
                if (prefs.BoolA.HasValue && prefs.BoolA.Value != _istenCikanlar)
                {
                    _istenCikanlar = prefs.BoolA.Value;
                    RaisePropertyChanged(nameof(IstenCikanlar));
                    RaisePropertyChanged(nameof(DurumIndex));
                }
                if (prefs.BoolB.HasValue && prefs.BoolB.Value != _puantajYapilan)
                {
                    _puantajYapilan = prefs.BoolB.Value;
                    RaisePropertyChanged(nameof(PuantajYapilan));
                }
            }
        }
        catch (Exception ex)
        {
            Error = "Firma listesi yüklenemedi: " + ex.Message;
        }
        finally
        {
            _suppressFilter = false;
        }
    }

    private void LoadLookups()
    {
        int firmaId = GetSeciliFirmaId();
        if (firmaId <= 0) return;

        try
        {
            using var scope = _scopes.CreateScope();
            var lookup = scope.ServiceProvider.GetRequiredService<IKisiEkraniLookUpService>();
            var calisma = scope.ServiceProvider.GetRequiredService<ICalismaSekliService>();
            var yetkiSvc = scope.ServiceProvider.GetRequiredService<IKullaniciFirmaIsyeriYetkiService>();
            bool isAdmin = FirmaIsyeriYetkiHelper.IsAdmin(_session.RolId);
            var yetkiler = _session.AktifKullaniciId.HasValue
                ? yetkiSvc.GetYetkiler(_session.AktifKullaniciId.Value) ?? new List<FirmaIsyeriYetkiDTO>()
                : new List<FirmaIsyeriYetkiDTO>();

            Replace(Departmanlar, lookup.GetDepartmanlar(firmaId) ?? new List<LookupItem>());
            Replace(Bolumler, lookup.GetBolumler(firmaId) ?? new List<LookupItem>());
            Replace(Pozisyonlar, lookup.GetPozisyonlar(firmaId) ?? new List<LookupItem>());
            Replace(IsyerleriForm, lookup.GetIsyerleri(firmaId) ?? new List<LookupItem>());
            Replace(FirmalarForm, lookup.GetFirma(firmaId) ?? new List<LookupItem>());
            Replace(CalismaStatuleri, lookup.GetCalismaStatuleri(firmaId) ?? new List<LookupItem>());

            var isyeriFilter = lookup.GetIsyerleri(firmaId) ?? new List<LookupItem>();
            isyeriFilter = FirmaIsyeriYetkiHelper.FilterIsyeriLookup(isyeriFilter, firmaId, yetkiler, isAdmin);
            var filterData = new List<LookupItem> { new LookupItem { Id = 0, Ad = "Tümü" } };
            filterData.AddRange(isyeriFilter);

            _suppressFilter = true;
            Isyerler.Clear();
            foreach (var it in filterData)
                Isyerler.Add(it);
            var prefs = PageFilterPrefsStore.Load(PageName);
            var preferredIsyeri = prefs?.IsyeriId;
            _selectedIsyeriFilter = (preferredIsyeri.HasValue
                    ? Isyerler.FirstOrDefault(x => x.Id == preferredIsyeri.Value)
                    : null)
                ?? Isyerler.FirstOrDefault();
            RaisePropertyChanged(nameof(SelectedIsyeriFilter));
            _suppressFilter = false;

            Vardiyalar.Clear();
            foreach (var v in calisma.GetAll(firmaId, includeGlobal: true) ?? new List<CalismaSekli>())
            {
                Vardiyalar.Add(new VardiyaCheckItem
                {
                    Id = v.Id,
                    Ad = v.Ad ?? "",
                    Secili = false
                });
            }

            SelectedDepartman = Departmanlar.FirstOrDefault();
            SelectedBolum = Bolumler.FirstOrDefault();
            SelectedPozisyon = Pozisyonlar.FirstOrDefault();
            SelectedIsyeri = IsyerleriForm.FirstOrDefault();
            SelectedCalismaStatu = CalismaStatuleri.FirstOrDefault();
            SelectedFirmaDetail = FirmalarForm.FirstOrDefault(x => x.Id == firmaId) ?? FirmalarForm.FirstOrDefault();
        }
        catch (Exception ex)
        {
            Error = "Lookup listeleri yüklenemedi: " + ex.Message;
        }
    }

    private void LoadList(bool preserveSelection = true)
    {
        Error = null;
        int firmaId = GetSeciliFirmaId();
        try
        {
            using var scope = _scopes.CreateScope();
            var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
            if (!auth.ViewAbility(PageName))
            {
                Error = "Personeller ekranını görüntüleme yetkiniz yok.";
                Kisiler.Clear();
                ClearFields();
                RefreshToolbar(auth);
                return;
            }

            if (firmaId <= 0)
            {
                Kisiler.Clear();
                Status = "Firma seçiniz.";
                RefreshToolbar(auth);
                return;
            }

            var kq = scope.ServiceProvider.GetRequiredService<IKisiQueryService>();
            var yetkiSvc = scope.ServiceProvider.GetRequiredService<IKullaniciFirmaIsyeriYetkiService>();
            bool isAdmin = FirmaIsyeriYetkiHelper.IsAdmin(_session.RolId);
            var yetkiler = _session.AktifKullaniciId.HasValue
                ? yetkiSvc.GetYetkiler(_session.AktifKullaniciId.Value) ?? new List<FirmaIsyeriYetkiDTO>()
                : new List<FirmaIsyeriYetkiDTO>();

            bool sadeceIstenCikanlar = IstenCikanlar;
            bool? puantajYapilirMi = sadeceIstenCikanlar ? null : PuantajYapilan;
            var (isyeriId, isyeriIdIn) = FirmaIsyeriYetkiHelper.ResolveKisiQueryIsyeriFilter(
                firmaId, GetSeciliIsyeriFilterId(), yetkiler, isAdmin);

            var data = kq.GetAktifKisilerByFirma(
                firmaId, null, puantajYapilirMi, isyeriId, isyeriIdIn, sadeceIstenCikanlar)
                       ?? new List<KisiListItem>();

            var keepId = preserveSelection
                ? (_originalPersonelId ?? SelectedKisi?.PersonelId)
                : null;

            _suppressSelection = true;
            Kisiler.Clear();
            foreach (var k in data)
                Kisiler.Add(k);

            KisiListItem? next = null;
            if (!string.IsNullOrWhiteSpace(keepId))
                next = Kisiler.FirstOrDefault(x => x.PersonelId == keepId);
            next ??= Kisiler.FirstOrDefault();
            _selectedKisi = next;
            RaisePropertyChanged(nameof(SelectedKisi));
            _suppressSelection = false;

            if (next is null)
            {
                _originalPersonelId = null;
                ClearFields();
                Status = BosListeUyariMesaji(GetSeciliIsyeriFilterId());
            }
            else
            {
                _originalPersonelId = next.PersonelId.Trim();
                LoadDetail(next.PersonelId, kq);
                Status = $"{Kisiler.Count} personel yüklendi.";
            }

            RefreshToolbar(auth);
        }
        catch (Exception ex)
        {
            Error = "Liste yüklenemedi: " + ex.Message;
            UiDialog.Error(Error, PageName);
        }
    }

    private string BosListeUyariMesaji(int? seciliIsyeriId)
    {
        bool isyeriVar = seciliIsyeriId.HasValue && seciliIsyeriId.Value > 0;
        string? isyeriAd = isyeriVar ? SelectedIsyeriFilter?.Ad?.Trim() : null;
        if (isyeriVar)
        {
            return string.IsNullOrEmpty(isyeriAd)
                ? "Seçili işyerde personel bulunamadı."
                : $"\"{isyeriAd}\" işyerinde personel bulunamadı.";
        }

        return "Seçili filtreye uygun personel bulunamadı.";
    }

    private void LoadDetail(string kisiId, IKisiQueryService? kq = null)
    {
        try
        {
            KisiDetay? d;
            if (kq is null)
            {
                using var scope = _scopes.CreateScope();
                var local = scope.ServiceProvider.GetRequiredService<IKisiQueryService>();
                (d, _) = local.GetDetayOrPuantajsizKart(kisiId);
            }
            else
            {
                (d, _) = kq.GetDetayOrPuantajsizKart(kisiId);
            }

            if (d is null)
            {
                UiDialog.Warning("Kişi bulunamadı.", PageName);
                ClearFields();
                return;
            }

            int filterFirmaId = GetSeciliFirmaId();
            if (d.FirmaId > 0 && filterFirmaId > 0 && d.FirmaId != filterFirmaId)
            {
                UiDialog.Warning("Seçilen personel bu firmaya ait değil.", PageName);
                ClearFields();
                return;
            }

            ApplyDetay(d);
        }
        catch (Exception ex)
        {
            Error = "Kişi detayı yüklenemedi: " + ex.Message;
            UiDialog.Error(Error, PageName);
        }
    }

    private void ApplyDetay(KisiDetay d)
    {
        AdSoyad = ((d.Ad ?? "") + " " + (d.Soyad ?? "")).Trim();
        SicilNo = d.PersonelId ?? "";
        KartNo = d.KartNo ?? "";
        TcKimlikNo = d.TcKimlikNo ?? "";
        CepTel = d.CepTel ?? "";
        Email = d.Email ?? "";
        FirmaDisiKartNo = d.TaseronKartNo ?? "";

        IseGiris = d.IseGirisTarihi ?? DateTime.Today;
        HasIstenCikis = d.IstenCikisTarihi.HasValue;
        IstenCikis = d.IstenCikisTarihi;
        HasDogum = d.DogumTarihi.HasValue;
        DogumTarihi = d.DogumTarihi;

        SelectedPozisyon = FindLookup(Pozisyonlar, d.PozisyonId);
        SelectedDepartman = FindLookup(Departmanlar, d.DepartmanId);
        SelectedIsyeri = FindLookup(IsyerleriForm, d.IsyeriId, allowMissingZero: true);
        SelectedFirmaDetail = FindLookup(FirmalarForm, d.FirmaId);
        SelectedBolum = FindLookup(Bolumler, d.BolumId);
        SelectedCalismaStatu = d.CalismaStatusuId.HasValue
            ? FindLookup(CalismaStatuleri, d.CalismaStatusuId)
            : CalismaStatuleri.FirstOrDefault();

        VardiyalariIsaretle(d.CalismaSekliCsv ?? "");

        YemekHakki = d.YemekHakkiVar;
        YemekAdedi = d.GunlukYemekAdedi ?? 0;
        FirmaPersoneli = d.FirmaPersoneli;
        PuantajYapilir = d.PuantajYapilabilir;
        Ziyaretci = d.ZiyaretciMi;
        AracKarti = d.AracKartiMi;
        Taseron = d.TaseronCalisanMi;

        SetFoto(d.Fotograf, dirty: false);
        RaiseDependentUi();
    }

    private void VardiyalariIsaretle(string csvIds)
    {
        var hedef = new HashSet<int>(
            (csvIds ?? "")
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => x.Length > 0)
                .Select(x => int.TryParse(x, out var n) ? n : (int?)null)
                .Where(n => n.HasValue)
                .Select(n => n!.Value));

        foreach (var v in Vardiyalar)
            v.Secili = hedef.Contains(v.Id);
    }

    private string SecilenVardiyaIds()
        => string.Join(",", Vardiyalar.Where(v => v.Secili).Select(v => v.Id));

    private static void AdSoyadAyir(string tamAd, out string ad, out string soyad)
    {
        ad = tamAd?.Trim() ?? "";
        soyad = "";
        if (string.IsNullOrEmpty(ad)) return;

        var parts = ad.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            soyad = "";
            return;
        }

        soyad = parts[parts.Length - 1];
        ad = string.Join(" ", parts, 0, parts.Length - 1);
    }

    private static int? GetNullableId(LookupItem? item, bool allowZero = false)
    {
        if (item is null) return null;
        if (item.Id < 0) return null;
        if (!allowZero && item.Id == 0) return null;
        return item.Id;
    }

    private static LookupItem? FindLookup(ObservableCollection<LookupItem> items, int? id, bool allowMissingZero = false)
    {
        if (!id.HasValue)
            return items.FirstOrDefault();
        var found = items.FirstOrDefault(x => x.Id == id.Value);
        if (found != null) return found;
        if (allowMissingZero && id.Value == 0)
            return items.FirstOrDefault(x => x.Id == 0) ?? items.FirstOrDefault();
        return items.FirstOrDefault();
    }

    private void EnterViewMode()
    {
        _mode = ScreenMode.View;
        FieldsReadOnly = true;
        ShowSaveCancel = false;
        ListEnabled = true;
        if (SelectedKisi != null && !string.IsNullOrWhiteSpace(SelectedKisi.PersonelId))
            LoadDetail(SelectedKisi.PersonelId);
        else
            ClearFields();
        RefreshToolbar();
        RaiseDependentUi();
        CommandManager.InvalidateRequerySuggested();
    }

    private void EnterAddMode()
    {
        try
        {
            using var scope = _scopes.CreateScope();
            var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
            if (!auth.Can(PageName, YetkiTipleri.Create))
            {
                Error = "Personel ekleme yetkiniz yok.";
                UiDialog.Warning(Error, PageName);
                return;
            }

            ClearFields();
            _mode = ScreenMode.Add;
            _originalPersonelId = null;
            Error = null;

            FirmaPersoneli = true;
            PuantajYapilir = PuantajYapilan;
            if (!PuantajYapilan)
            {
                FirmaPersoneli = false;
                Ziyaretci = false;
                AracKarti = false;
                Taseron = false;
            }

            IseGiris = DateTime.Today;
            SelectedFirmaDetail = FirmalarForm.FirstOrDefault(x => x.Id == GetSeciliFirmaId())
                                  ?? FirmalarForm.FirstOrDefault();

            FieldsReadOnly = false;
            ListEnabled = false;
            ShowSaveCancel = true;
            RefreshToolbar();
            RaiseDependentUi();
            CommandManager.InvalidateRequerySuggested();
        }
        catch (Exception ex)
        {
            Error = "Ekleme modu açılamadı: " + ex.Message;
            EnterViewMode();
        }
    }

    private void EnterEditMode()
    {
        try
        {
            using var scope = _scopes.CreateScope();
            var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
            if (!auth.Can(PageName, YetkiTipleri.Update))
            {
                Error = "Personel güncelleme yetkiniz yok.";
                UiDialog.Warning(Error, PageName);
                return;
            }

            if (SelectedKisi is null)
            {
                UiDialog.Warning("Güncellemek için listeden bir kişi seçiniz.", PageName);
                return;
            }

            _mode = ScreenMode.Edit;
            _originalPersonelId = (SicilNo ?? "").Trim();
            Error = null;
            FieldsReadOnly = false;
            ListEnabled = false;
            ShowSaveCancel = true;
            RefreshToolbar();
            RaiseDependentUi();
            UiDialog.Info("Bilgileri düzenleyin ve 'Kaydet' butonuna basın.", "Güncelleme Modu");
            CommandManager.InvalidateRequerySuggested();
        }
        catch (Exception ex)
        {
            Error = "Düzenleme modu açılamadı: " + ex.Message;
            EnterViewMode();
        }
    }

    private void DeleteOrActivate()
    {
        Error = null;
        if (IstenCikanlar)
        {
            ActivateSelected();
            return;
        }

        try
        {
            using var scope = _scopes.CreateScope();
            var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
            if (!auth.Can(PageName, YetkiTipleri.Delete))
            {
                Error = "İşten çıkış yetkiniz yok.";
                UiDialog.Warning(Error, PageName);
                return;
            }

            if (SelectedKisi is null)
            {
                UiDialog.Warning("Lütfen listeden bir kişi seçiniz.", PageName);
                return;
            }

            _mode = ScreenMode.Exit;
            HasIstenCikis = true;
            if (!IstenCikis.HasValue || IstenCikis.Value.Year < 2000)
                IstenCikis = DateTime.Today;

            FieldsReadOnly = true;
            ListEnabled = false;
            ShowSaveCancel = true;
            RefreshToolbar();
            RaiseDependentUi();
            UiDialog.Info("Lütfen işten çıkış tarihini seçin ve 'Kaydet' butonuna basın.", "Tarih Seçimi");
            CommandManager.InvalidateRequerySuggested();
        }
        catch (Exception ex)
        {
            Error = "İşten çıkış moduna geçilemedi: " + ex.Message;
            EnterViewMode();
        }
    }

    private void ActivateSelected()
    {
        using var scope = _scopes.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        if (!auth.Can(PageName, YetkiTipleri.Update))
        {
            Error = "Personel aktif etme yetkiniz yok.";
            UiDialog.Warning(Error, PageName);
            return;
        }

        var personelId = SelectedKisi?.PersonelId;
        if (string.IsNullOrWhiteSpace(personelId))
        {
            UiDialog.Warning("Lütfen listeden bir kişi seçiniz.", PageName);
            return;
        }

        if (!UiDialog.Confirm("Seçili personeli tekrar aktif etmek istiyor musunuz?", "Onay", yesText: "Aktif et", noText: "Vazgeç"))
            return;

        bool puantajYapilirMi = UiDialog.Confirm("Puantaj yapılan bir kart mı?", "Puantaj");

        try
        {
            var kisiSvc = scope.ServiceProvider.GetRequiredService<IKisiService>();
            var sonuc = kisiSvc.KisiTekrarAktifEt(personelId.Trim(), puantajYapilirMi);
            if (!sonuc.Success)
            {
                var err = string.IsNullOrWhiteSpace(sonuc.ErrorMessage)
                    ? "Personel tekrar aktif edilemedi."
                    : ("Personel tekrar aktif edilemedi. " + sonuc.ErrorMessage.Trim());
                Error = err;
                UiDialog.Error(err, PageName);
                return;
            }

            UiDialog.Success(PersonelMesajlari.TekrarAktifBasariMesaji(
                sonuc.YenidenAktifYemekLimiti, sonuc.CihazUyarisiGoster, sonuc.WarningMessage), PageName);

            _originalPersonelId = personelId.Trim();
            _suppressFilter = true;
            IstenCikanlar = false;
            _suppressFilter = false;
            LoadList();
            EnterViewMode();
        }
        catch (Exception ex)
        {
            Error = "Aktif etme sırasında hata: " + ex.Message;
            UiDialog.Error(Error, PageName);
        }
    }

    private void Save()
    {
        Error = null;
        try
        {
            using var scope = _scopes.CreateScope();
            var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
            var kisiSvc = scope.ServiceProvider.GetRequiredService<IKisiService>();

            if (_mode == ScreenMode.Exit)
            {
                if (!auth.Can(PageName, YetkiTipleri.Delete))
                {
                    Error = "İşten çıkış yetkiniz yok.";
                    return;
                }
                SaveExit(kisiSvc);
                return;
            }

            if (_mode == ScreenMode.Edit)
            {
                if (!auth.Can(PageName, YetkiTipleri.Update))
                {
                    Error = "Personel güncelleme yetkiniz yok.";
                    return;
                }
                SaveEdit(kisiSvc);
                return;
            }

            if (_mode == ScreenMode.Add)
            {
                if (!auth.Can(PageName, YetkiTipleri.Create))
                {
                    Error = "Personel ekleme yetkiniz yok.";
                    return;
                }
                SaveAdd(kisiSvc);
            }
        }
        catch (Exception ex)
        {
            Error = "Kayıt sırasında hata: " + ex.Message;
            UiDialog.Error(Error, PageName);
        }
    }

    private void SaveExit(IKisiService kisiSvc)
    {
        var personelId = SelectedKisi?.PersonelId;
        var adSoyad = SelectedKisi?.AdSoyad ?? AdSoyad;
        if (string.IsNullOrWhiteSpace(personelId))
        {
            UiDialog.Warning("Seçili kaydın PersonelId bilgisi yok.", PageName);
            return;
        }

        if (!HasIstenCikis || !IstenCikis.HasValue)
        {
            UiDialog.Warning("İşten çıkış tarihini seçiniz.", PageName);
            return;
        }

        var cikis = IstenCikis.Value.Date;
        if (!UiDialog.Confirm(
                $"{adSoyad} için işten çıkış tarihi {cikis:dd.MM.yyyy} olarak işlenecek. Onaylıyor musunuz?",
                "Onay",
                yesText: "İşten çıkar",
                noText: "Vazgeç"))
            return;

        var puantajForUndo = PuantajYapilir;
        if (!kisiSvc.KisiIstenCikar(personelId, cikis, (FirmaDisiKartNo ?? "").Trim()))
        {
            Error = "İşten çıkış işlemi tamamlanamadı.";
            UiDialog.Error(Error, PageName);
            return;
        }

        var pid = personelId.Trim();
        UiDialog.SuccessWithUndo("İşten çıkış başarıyla işlendi.", () =>
        {
            using var s2 = _scopes.CreateScope();
            var sonuc = s2.ServiceProvider.GetRequiredService<IKisiService>().KisiTekrarAktifEt(pid, puantajForUndo);
            if (sonuc.Success)
            {
                LoadList();
                EnterViewMode();
                UiDialog.Success("Geri alındı.", PageName);
            }
            else
                UiDialog.Warning(string.IsNullOrWhiteSpace(sonuc.ErrorMessage) ? "Geri alma başarısız." : sonuc.ErrorMessage, PageName);
        }, PageName);
        LoadList();
        EnterViewMode();
    }

    private void SaveEdit(IKisiService kisiSvc)
    {
        Errors.Clear();
        Error = null;
        Errors.Require("SicilNo", SicilNo, "Sicil No zorunludur.");
        Errors.Require("AdSoyad", AdSoyad, "Ad Soyad zorunludur.");
        if (Errors.HasErrors)
        {
            Error = Errors.FirstMessage;
            return;
        }

        AdSoyadAyir(AdSoyad, out string ad, out string soyad);
        var kisi = BuildKisi(ad, soyad);

        var ok = kisiSvc.KisiGuncelle(
            kisi,
            originalPersonelId: _originalPersonelId ?? kisi.PersonelId,
            FirmaPersoneli,
            PuantajYapilir,
            YemekHakki,
            YemekAdedi,
            (FirmaDisiKartNo ?? "").Trim(),
            fotoDegisti: _fotoDirty);

        if (!ok)
        {
            UiDialog.Warning("Kayıt güncellenemedi!.", PageName);
            return;
        }

        _originalPersonelId = kisi.PersonelId.Trim();
        UiDialog.Success("Kayıt güncellendi.", PageName);
        LoadList();
        EnterViewMode();
    }

    private void SaveAdd(IKisiService kisiSvc)
    {
        Errors.Clear();
        Error = null;
        Errors.Require("SicilNo", SicilNo, "Sicil No zorunludur.");
        Errors.Require("AdSoyad", AdSoyad, "Ad Soyad zorunludur.");
        if (Errors.HasErrors)
        {
            Error = Errors.FirstMessage;
            return;
        }

        var validasyonDto = new KisiKayitValidasyonDTO
        {
            PersonelId = (SicilNo ?? "").Trim(),
            FirmaPersoneli = FirmaPersoneli,
            PuantajYapilir = PuantajYapilir,
            YemekHakkiVar = YemekHakki,
            YemekAdedi = YemekAdedi,
            FirmaDisiKartNo = (FirmaDisiKartNo ?? "").Trim(),
            TcKimlikNo = (TcKimlikNo ?? "").Trim(),
            KartNo = (KartNo ?? "").Trim(),
            TaseronCalisanMi = Taseron,
            ZiyaretciMi = Ziyaretci,
            AracKartiMi = AracKarti
        };

        var validasyonSonuc = kisiSvc.ValidateKisiKayit(validasyonDto);
        if (!validasyonSonuc.IsValid)
        {
            Errors.Set("SicilNo", validasyonSonuc.Message ?? "Doğrulama başarısız.");
            Error = Errors.FirstMessage;
            return;
        }

        AdSoyadAyir(AdSoyad, out string adYeni, out string soyadYeni);
        var yeniKisi = BuildKisi(adYeni, soyadYeni);

        string kartId = (SicilNo ?? "").Trim();
        string kartNo = (FirmaDisiKartNo ?? "").Trim();
        string kartAdi = string.IsNullOrWhiteSpace(AdSoyad)
            ? (yeniKisi.Ad + " " + yeniKisi.Soyad).Trim()
            : AdSoyad.Trim();

        kisiSvc.YeniKisiEkle(
            yeniKisi,
            FirmaPersoneli,
            PuantajYapilir,
            YemekHakki,
            YemekAdedi,
            kartId,
            kartNo,
            kartAdi);

        _originalPersonelId = yeniKisi.PersonelId.Trim();
        UiDialog.Success("Kayıt tamamlandı.", PageName);
        LoadList();
        EnterViewMode();
    }

    private Kisi BuildKisi(string ad, string soyad)
    {
        return new Kisi
        {
            PersonelId = (SicilNo ?? "").Trim(),
            Ad = ad,
            Soyad = soyad,
            KartNo = (KartNo ?? "").Trim(),
            TcKimlikNo = (TcKimlikNo ?? "").Trim(),
            PozisyonId = GetNullableId(SelectedPozisyon),
            DepartmanId = GetNullableId(SelectedDepartman),
            IsyeriId = GetNullableId(SelectedIsyeri, allowZero: true),
            BolumId = GetNullableId(SelectedBolum),
            FirmaId = GetNullableId(SelectedFirmaDetail) ?? GetSeciliFirmaId(),
            IseGirisTarihi = IseGiris.Date,
            IstenCikisTarihi = HasIstenCikis ? IstenCikis?.Date : null,
            DogumTarihi = HasDogum ? DogumTarihi?.Date : null,
            CalismaStatusu = SelectedCalismaStatu?.Id.ToString() ?? "",
            CalismaSekli = SecilenVardiyaIds(),
            CepTel = (CepTel ?? "").Trim(),
            Email = (Email ?? "").Trim(),
            Fotograf = (_mode == ScreenMode.Add || _fotoDirty) ? (_fotografBytes ?? Array.Empty<byte>()) : null!,
            PuantajYapilirMi = PuantajYapilir,
            ZiyaretciMi = Ziyaretci,
            AracKartiMi = AracKarti,
            TaseronCalisanMi = Taseron
        };
    }

    private void ClearFields()
    {
        AdSoyad = "";
        SicilNo = "";
        KartNo = "";
        TcKimlikNo = "";
        CepTel = "";
        Email = "";
        FirmaDisiKartNo = "";
        IseGiris = DateTime.Today;
        HasDogum = false;
        DogumTarihi = null;
        HasIstenCikis = false;
        IstenCikis = null;

        SelectedDepartman = Departmanlar.FirstOrDefault();
        SelectedPozisyon = Pozisyonlar.FirstOrDefault();
        SelectedIsyeri = IsyerleriForm.FirstOrDefault();
        SelectedBolum = Bolumler.FirstOrDefault();
        SelectedCalismaStatu = CalismaStatuleri.FirstOrDefault();
        int filterFirmaId = GetSeciliFirmaId();
        SelectedFirmaDetail = filterFirmaId > 0
            ? FirmalarForm.FirstOrDefault(x => x.Id == filterFirmaId) ?? FirmalarForm.FirstOrDefault()
            : FirmalarForm.FirstOrDefault();

        foreach (var v in Vardiyalar)
            v.Secili = false;

        FirmaPersoneli = false;
        PuantajYapilir = false;
        YemekHakki = false;
        Ziyaretci = false;
        AracKarti = false;
        Taseron = false;
        YemekAdedi = 0;
        SetFoto(null, dirty: false);
        RaiseDependentUi();
    }

    private void RefreshToolbar(IAuthorizationService? auth = null)
    {
        if (auth is null)
        {
            using var scope = _scopes.CreateScope();
            ApplyToolbar(scope.ServiceProvider.GetRequiredService<IAuthorizationService>());
            return;
        }

        ApplyToolbar(auth);
    }

    private void ApplyToolbar(IAuthorizationService auth)
    {
        bool listMode = _mode == ScreenMode.View;
        bool canSelect = listMode && SelectedKisi != null && Kisiler.Count > 0;
        CanAdd = listMode && !IstenCikanlar && auth.Can(PageName, YetkiTipleri.Create);
        CanEdit = canSelect && !IstenCikanlar && auth.Can(PageName, YetkiTipleri.Update);
        CanDelete = canSelect && (IstenCikanlar
            ? auth.Can(PageName, YetkiTipleri.Update)
            : auth.Can(PageName, YetkiTipleri.Delete));

        bool editMode = _mode is ScreenMode.Add or ScreenMode.Edit;
        CanPhoto = editMode && !IstenCikanlar && (
            auth.Can(PageName, YetkiTipleri.Update) || auth.Can(PageName, YetkiTipleri.Create));

        RaisePropertyChanged(nameof(DeleteButtonText));
        CommandManager.InvalidateRequerySuggested();
    }

    private void RaiseDependentUi()
    {
        RaisePropertyChanged(nameof(FirmaDisiEnabled));
        RaisePropertyChanged(nameof(YemekAdediEnabled));
        RaisePropertyChanged(nameof(IstenCikisEnabled));
        RaisePropertyChanged(nameof(KartTipiEnabled));
        RaisePropertyChanged(nameof(VardiyaEditable));
        RaisePropertyChanged(nameof(ShowAktifCalisiyorText));
        RaisePropertyChanged(nameof(ShowIstenCikisDatePicker));
        RaisePropertyChanged(nameof(CalismaDurumuText));
    }

    private void SetFoto(byte[]? bytes, bool dirty)
    {
        _fotografBytes = bytes is { Length: > 0 } ? bytes : null;
        _fotoDirty = dirty;
        FotoImage = BytesToImageSource(_fotografBytes);
    }

    private void AddPhoto()
    {
        if (!CanPhoto) return;
        var dlg = new OpenFileDialog
        {
            Title = "Fotoğraf Seç",
            Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp"
        };
        if (dlg.ShowDialog() != true)
            return;

        try
        {
            var bytes = File.ReadAllBytes(dlg.FileName);
            SetFoto(bytes, dirty: true);
        }
        catch (Exception ex)
        {
            UiDialog.Error("Fotoğraf yüklenemedi: " + ex.Message, PageName);
        }
    }

    private void RemovePhoto()
    {
        if (!CanPhoto) return;
        if (_fotografBytes == null || _fotografBytes.Length == 0)
        {
            UiDialog.InfoToast("Silinecek bir fotoğraf yok.", PageName);
            return;
        }

        if (!UiDialog.Confirm("Fotoğrafı silmek istediğinize emin misiniz?", "Onay", yesText: "Sil", noText: "Vazgeç"))
            return;

        SetFoto(null, dirty: true);
    }

    private void OpenKisiAra()
    {
        if (_mode != ScreenMode.View) return;

        try
        {
            var ctx = BuildKisiAraContext();
            var win = new KisiAraWindow(ctx)
            {
                Owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                        ?? Application.Current?.MainWindow
            };

            if (win.ShowDialog() != true || string.IsNullOrWhiteSpace(win.SelectedPersonelId))
                return;

            ApplyKisiAraContext(win.AppliedContext);
            _suppressSelection = true;
            LoadList();
            var found = Kisiler.FirstOrDefault(k =>
                string.Equals(k.PersonelId, win.SelectedPersonelId.Trim(), StringComparison.OrdinalIgnoreCase));
            _suppressSelection = false;
            if (found != null)
                SelectedKisi = found;
            else
            {
                _originalPersonelId = win.SelectedPersonelId.Trim();
                LoadDetail(win.SelectedPersonelId.Trim());
            }
        }
        catch (Exception ex)
        {
            UiDialog.Error("Personel arama açılamadı: " + ex.Message, PageName);
        }
    }

    private KisiAraContext BuildKisiAraContext()
    {
        using var scope = _scopes.CreateScope();
        var yetkiSvc = scope.ServiceProvider.GetRequiredService<IKullaniciFirmaIsyeriYetkiService>();
        bool isAdmin = FirmaIsyeriYetkiHelper.IsAdmin(_session.RolId);
        var yetkiler = _session.AktifKullaniciId.HasValue
            ? yetkiSvc.GetYetkiler(_session.AktifKullaniciId.Value) ?? new List<FirmaIsyeriYetkiDTO>()
            : new List<FirmaIsyeriYetkiDTO>();

        int firmaId = GetSeciliFirmaId();
        bool sadeceIstenCikanlar = IstenCikanlar;
        bool? puantajYapilirMi = sadeceIstenCikanlar ? null : PuantajYapilan;
        int? isyeriFilterId = GetSeciliIsyeriFilterId();
        var (isyeriId, isyeriIdIn) = FirmaIsyeriYetkiHelper.ResolveKisiQueryIsyeriFilter(
            firmaId, isyeriFilterId, yetkiler, isAdmin);

        return new KisiAraContext
        {
            FirmaId = firmaId,
            FirmaAdi = SelectedFirma?.FirmaAdi ?? "",
            IsyeriId = isyeriFilterId ?? isyeriId,
            IsyeriIdIn = isyeriIdIn,
            IsyeriAdi = SelectedIsyeriFilter?.Ad ?? "Tümü",
            SadeceIstenCikanlar = sadeceIstenCikanlar,
            PuantajYapilirMi = puantajYapilirMi,
            CalismaDurumuMetni = sadeceIstenCikanlar ? "İşten Çıkanlar" : "Aktif Çalışanlar",
            PuantajMetni = puantajYapilirMi == false ? "Puantaj Yapılmayanlar" : "Puantaj Yapılanlar"
        };
    }

    private void ApplyKisiAraContext(KisiAraContext? ctx)
    {
        if (ctx == null) return;
        _suppressFilter = true;
        try
        {
            var firma = Firmalar.FirstOrDefault(f => f.FirmaId == ctx.FirmaId);
            if (firma != null)
            {
                _selectedFirma = firma;
                RaisePropertyChanged(nameof(SelectedFirma));
                LoadLookups();
            }

            if (ctx.IsyeriId.HasValue && ctx.IsyeriId.Value > 0)
                SelectedIsyeriFilter = Isyerler.FirstOrDefault(x => x.Id == ctx.IsyeriId.Value)
                                       ?? Isyerler.FirstOrDefault();
            else
                SelectedIsyeriFilter = Isyerler.FirstOrDefault(x => x.Id == 0) ?? Isyerler.FirstOrDefault();

            IstenCikanlar = ctx.SadeceIstenCikanlar;
            if (!ctx.SadeceIstenCikanlar)
                PuantajYapilan = ctx.PuantajYapilirMi != false;
        }
        finally
        {
            _suppressFilter = false;
        }
    }

    private static ImageSource? BytesToImageSource(byte[]? bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return null;
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
        catch
        {
            return null;
        }
    }

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source)
            target.Add(item);
    }
}
