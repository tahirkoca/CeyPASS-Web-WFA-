using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CeyPASS.Business.Abstractions;
using CeyPASS.Business.Services;
using CeyPASS.Entities.Concrete;
using CeyPASS.Entities.Helpers;
using CeyPASS.Infrastructure.Helpers;
using CeyPASS.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace CeyPASS.WPF.ViewModels;

/// <summary>Personel formunda çoklu vardiya seçim satırı.</summary>
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

/// <summary>
/// Personel tanım, işten çıkış/aktif etme, çoklu sicil ve gelişmiş arama — firma/işyeri yetkilerine göre filtrelenir.
/// </summary>
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
    private DateTime? _iseGiris = DateTime.Today;
    private DateTime? _istenCikis;
    private DateTime? _dogumTarihi;

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
    private bool _isAnaSicil;
    private int _cokluSicilHedefSayisi;
    private string? _hedefSicilBilgi;
    private bool _isHedefSicilOnly;
    private bool _isApplyingDetay;
    private int? _yemekAdedi;
    private bool _loadedYemekHakki;

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

    /// <param name="root">DI kökü; scope ile servis çözümlemesi.</param>
    public PersonelViewModel(IServiceProvider root)
    {
        _scopes = root.GetRequiredService<IServiceScopeFactory>();
        _session = root.GetRequiredService<ISessionContext>();

        Firmalar = new ObservableCollection<Firma>();
        Isyerler = new ObservableCollection<LookupItem>();
        Kisiler = new ObservableCollection<KisiListItem>();
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
        OpenCokluSicilCommand = new RelayCommand(OpenCokluSicil, () => CanOpenCokluSicil);
        ClearDogumCommand = new RelayCommand(ClearDogum, () => !FieldsReadOnly);

        Refresh();
    }

    public ObservableCollection<Firma> Firmalar { get; }
    public ObservableCollection<LookupItem> Isyerler { get; }
    public ObservableCollection<KisiListItem> Kisiler { get; }
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

    /// <summary>Listeyi aktif veya işten çıkan personelle sınırlar.</summary>
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

    /// <summary>Durum combo indeksi (0=aktif, 1=işten çıkan).</summary>
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
    public string KartNo
    {
        get => FieldsReadOnly ? KisiDisplayHelper.TextOrMissing(_kartNo) : _kartNo;
        set => SetProperty(ref _kartNo, value ?? "");
    }
    public string TcKimlikNo
    {
        get => FieldsReadOnly ? KisiDisplayHelper.TextOrMissing(_tcKimlikNo) : _tcKimlikNo;
        set
        {
            var v = value ?? "";
            if (_tcKimlikNo == v) return;
            SetProperty(ref _tcKimlikNo, v);
            RaiseCokluSicilUi();
        }
    }
    public string FirmaDisiKartNo
    {
        get => FieldsReadOnly ? KisiDisplayHelper.TextOrMissing(_firmaDisiKartNo) : _firmaDisiKartNo;
        set => SetProperty(ref _firmaDisiKartNo, value ?? "");
    }
    public string Email
    {
        get => FieldsReadOnly ? KisiDisplayHelper.TextOrMissing(_email) : _email;
        set => SetProperty(ref _email, value ?? "");
    }
    public string CepTel
    {
        get => FieldsReadOnly ? KisiDisplayHelper.TextOrMissing(_cepTel) : _cepTel;
        set => SetProperty(ref _cepTel, value ?? "");
    }

    public DateTime? IseGiris
    {
        get => _iseGiris;
        set
        {
            if (_iseGiris == value) return;
            SetProperty(ref _iseGiris, value);
            RaiseDependentUi();
        }
    }

    public DateTime? IstenCikis
    {
        get => _istenCikis;
        set
        {
            if (_istenCikis == value) return;
            SetProperty(ref _istenCikis, value);
            RaiseDependentUi();
        }
    }

    public DateTime? DogumTarihi
    {
        get => _dogumTarihi;
        set => SetProperty(ref _dogumTarihi, value);
    }

    public bool ShowIseGirisMissingText => FieldsReadOnly && !IseGiris.HasValue;
    public string IseGirisMissingText => KisiDisplayHelper.Missing;
    public bool IsIstenCikmis => IstenCikis.HasValue;

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
            if (!value && _yemekAdedi.HasValue)
                _yemekAdedi = null;
            RaisePropertyChanged(nameof(YemekAdediText));
            RaiseDependentUi();
        }
    }

    public bool Ziyaretci
    {
        get => _ziyaretci;
        set
        {
            if (_ziyaretci == value) return;
            SetProperty(ref _ziyaretci, value);
            RaiseDependentUi();
        }
    }

    public bool AracKarti
    {
        get => _aracKarti;
        set
        {
            if (_aracKarti == value) return;
            SetProperty(ref _aracKarti, value);
            RaiseDependentUi();
        }
    }

    public bool Taseron
    {
        get => _taseron;
        set
        {
            if (_taseron == value) return;
            SetProperty(ref _taseron, value);
            RaiseDependentUi();
        }
    }

    public bool IsAnaSicil => _isAnaSicil;

    public int CokluSicilHedefSayisi
    {
        get => _cokluSicilHedefSayisi;
        private set
        {
            if (_cokluSicilHedefSayisi == value) return;
            SetProperty(ref _cokluSicilHedefSayisi, value);
            RaisePropertyChanged(nameof(CokluSicilDurumText));
            RaisePropertyChanged(nameof(ShowCokluSicilDurum));
            RaiseCokluSicilUi();
        }
    }

    public string? HedefSicilBilgi
    {
        get => _hedefSicilBilgi;
        private set
        {
            if (_hedefSicilBilgi == value) return;
            SetProperty(ref _hedefSicilBilgi, value);
            RaisePropertyChanged(nameof(CokluSicilDurumText));
            RaisePropertyChanged(nameof(ShowCokluSicilDurum));
        }
    }

    public bool IsHedefSicilOnly
    {
        get => _isHedefSicilOnly;
        private set
        {
            if (_isHedefSicilOnly == value) return;
            SetProperty(ref _isHedefSicilOnly, value);
            RaisePropertyChanged(nameof(CokluSicilDurumText));
            RaisePropertyChanged(nameof(ShowCokluSicilDurum));
            RaiseCokluSicilUi();
        }
    }

    public bool ShowCokluSicilDurum => IsHedefSicilOnly || _isAnaSicil;

    public string CokluSicilDurumText
    {
        get
        {
            if (IsHedefSicilOnly && !string.IsNullOrWhiteSpace(HedefSicilBilgi))
                return HedefSicilBilgi!;
            if (_isAnaSicil)
                return CokluSicilHedefSayisi > 0
                    ? $"Ana sicil · {CokluSicilHedefSayisi} hedef bağlı"
                    : "Ana sicil · bağlantı yok";
            return "";
        }
    }

    public bool CanOpenCokluSicil => _mode != ScreenMode.Add
        && !IstenCikanlar
        && PuantajYapilir
        && !IsHedefSicilOnly
        && int.TryParse(SicilNo, out var id) && id > 0
        && !string.IsNullOrWhiteSpace(TcKimlikNo);
    public string CokluSicilOpenTooltip
    {
        get
        {
            if (_mode == ScreenMode.Add) return "Önce personeli kaydedin.";
            if (IstenCikanlar) return "İşten çıkan personelde kullanılamaz.";
            if (!PuantajYapilir) return "Puantaj Yapılır işaretlenmelidir.";
            if (IsHedefSicilOnly) return "Bu sicil yalnızca hedef sicildir.";
            if (string.IsNullOrWhiteSpace(TcKimlikNo)) return "TC kimlik numarası girilmelidir.";
            if (!int.TryParse(SicilNo, out var pid) || pid <= 0) return "Kayıtlı personel seçin.";
            return "Hedef sicil eşleştirmeleri";
        }
    }

    public ICommand OpenCokluSicilCommand { get; }
    public ICommand ClearDogumCommand { get; }

    public string YemekAdediText
    {
        get
        {
            if (FieldsReadOnly)
                return !YemekHakki ? KisiDisplayHelper.Missing : KisiDisplayHelper.NumberOrMissing(_yemekAdedi);
            return _yemekAdedi.HasValue ? _yemekAdedi.Value.ToString() : "";
        }
        set
        {
            int? next = string.IsNullOrWhiteSpace(value)
                ? null
                : int.TryParse(value.Trim(), out var n) ? n : _yemekAdedi;
            if (_yemekAdedi == next) return;
            _yemekAdedi = next;
            RaisePropertyChanged(nameof(YemekAdediText));
        }
    }

    private int YemekAdediForSave => _yemekAdedi ?? 0;

    public bool FieldsReadOnly
    {
        get => _fieldsReadOnly;
        private set
        {
            if (_fieldsReadOnly == value) return;
            SetProperty(ref _fieldsReadOnly, value);
            RefreshReadOnlyFieldDisplays();
            RaisePropertyChanged(nameof(YemekAdediEnabled));
            RaisePropertyChanged(nameof(VardiyaEditable));
            RaiseCokluSicilUi();
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
    /// <summary>İşten çıkanlar listesinde kart tipi filtresini kilitler.</summary>
    public bool KartTipiEnabled => !IstenCikanlar;
    /// <summary>İşten çıkanlar modunda puantaj filtresini kapatır.</summary>
    public bool PuantajFilterEnabled => ListEnabled && !IstenCikanlar;

    /// <summary>İşten çıkış tarihi yoksa WFA'daki gibi "Aktif Çalışıyor..." gösterilir.</summary>
    public bool ShowAktifCalisiyorText => !IstenCikis.HasValue && _mode != ScreenMode.Exit;
    public bool ShowIstenCikisDatePicker => IstenCikis.HasValue || _mode == ScreenMode.Exit;
    public string CalismaDurumuText => IstenCikis.HasValue ? "İşten çıkmış" : "Aktif çalışıyor";
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

    public bool EditingRequired => _mode is ScreenMode.Add or ScreenMode.Edit;
    public bool SicilNoRequired => EditingRequired;
    public bool AdSoyadRequired => EditingRequired;
    public bool IseGirisRequired => EditingRequired;
    public bool IsyeriRequired => EditingRequired;
    public bool TcKimlikRequired => EditingRequired && (FirmaPersoneli || Taseron);
    public bool KartNoRequired => EditingRequired && (Ziyaretci || AracKarti);
    public bool YemekAdediRequired => EditingRequired && YemekHakki;

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
        => FirmaIsyeriYetkiHelper.ToIsyeriQueryFilterId(SelectedIsyeriFilter?.Id);

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

            Replace(Bolumler, WithComboPlaceholder(lookup.GetBolumler(firmaId)));
            Replace(Pozisyonlar, WithComboPlaceholder(lookup.GetPozisyonlar(firmaId)));
            Replace(IsyerleriForm, WithComboPlaceholder(lookup.GetIsyerleri(firmaId)));
            Replace(FirmalarForm, lookup.GetFirma(firmaId) ?? new List<LookupItem>());
            Replace(CalismaStatuleri, WithComboPlaceholder(lookup.GetCalismaStatuleri(firmaId)));

            var isyeriFilter = lookup.GetIsyerleri(firmaId) ?? new List<LookupItem>();
            isyeriFilter = FirmaIsyeriYetkiHelper.FilterIsyeriLookup(isyeriFilter, firmaId, yetkiler, isAdmin);
            var filterData = new List<LookupItem> { FirmaIsyeriYetkiHelper.CreateIsyeriFilterTumuItem() };
            filterData.AddRange(isyeriFilter);

            _suppressFilter = true;
            Isyerler.Clear();
            foreach (var it in filterData)
                Isyerler.Add(it);
            var prefs = PageFilterPrefsStore.Load(PageName);
            var preferredIsyeri = prefs?.IsyeriId;
            _selectedIsyeriFilter = (preferredIsyeri.HasValue && preferredIsyeri.Value >= 0
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

            SelectedBolum = ComboPlaceholderItem(Bolumler);
            SelectedPozisyon = ComboPlaceholderItem(Pozisyonlar);
            SelectedIsyeri = ComboPlaceholderItem(IsyerleriForm);
            SelectedCalismaStatu = ComboPlaceholderItem(CalismaStatuleri);
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
        bool isyeriVar = seciliIsyeriId.HasValue;
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
        _isApplyingDetay = true;
        try
        {
            AdSoyad = ((d.Ad ?? "") + " " + (d.Soyad ?? "")).Trim();
            SicilNo = d.PersonelId ?? "";
            KartNo = d.KartNo ?? "";
            TcKimlikNo = d.TcKimlikNo ?? "";
            CepTel = d.CepTel ?? "";
            Email = d.Email ?? "";
            FirmaDisiKartNo = d.TaseronKartNo ?? "";

            IseGiris = d.IseGirisTarihi;
            IstenCikis = d.IstenCikisTarihi;
            DogumTarihi = d.DogumTarihi;

            SelectedPozisyon = FindLookup(Pozisyonlar, d.PozisyonId);
            SelectedIsyeri = FindLookup(IsyerleriForm, d.IsyeriId, allowMissingZero: true);
            SelectedFirmaDetail = FindLookup(FirmalarForm, d.FirmaId);
            SelectedBolum = FindLookup(Bolumler, d.BolumId);
            SelectedCalismaStatu = FindLookup(CalismaStatuleri, d.CalismaStatusuId);

            VardiyalariIsaretle(d.CalismaSekliCsv ?? "");

            YemekHakki = d.YemekHakkiVar;
            _loadedYemekHakki = d.YemekHakkiVar;
            _yemekAdedi = d.GunlukYemekAdedi;
            RaisePropertyChanged(nameof(YemekAdediText));
            FirmaPersoneli = d.FirmaPersoneli;
            Ziyaretci = d.ZiyaretciMi;
            AracKarti = d.AracKartiMi;
            Taseron = d.TaseronCalisanMi;

            _isAnaSicil = false;
            CokluSicilHedefSayisi = 0;
            HedefSicilBilgi = null;
            IsHedefSicilOnly = false;

            PuantajYapilir = d.PuantajYapilabilir;

            SetFoto(d.Fotograf, dirty: false);
            RefreshCokluSicilState(d.PersonelId, d.TcKimlikNo);
        }
        finally
        {
            _isApplyingDetay = false;
        }

        RaiseDependentUi();
    }

    private void RefreshCokluSicilState(string personelId, string? tcKimlikNo)
    {
        HedefSicilBilgi = null;
        IsHedefSicilOnly = false;
        SetProperty(ref _isAnaSicil, false);
        RaisePropertyChanged(nameof(IsAnaSicil));
        RaisePropertyChanged(nameof(CokluSicilDurumText));
        RaisePropertyChanged(nameof(ShowCokluSicilDurum));

        var pid = ResolvePersonelId(personelId);
        if (pid <= 0)
            pid = ResolvePersonelId(SicilNo);
        if (pid <= 0)
        {
            CokluSicilHedefSayisi = 0;
            RaiseCokluSicilUi();
            return;
        }

        try
        {
            using var scope = _scopes.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<ICokluSicilService>();
            var ozet = svc.GetOzet(pid);

            CokluSicilHedefSayisi = ozet.AktifHedefSayisi;
            IsHedefSicilOnly = ozet.IsHedefSicil && !ozet.IsAnaSicil;
            if (IsHedefSicilOnly && ozet.AnaPersonelId.HasValue)
                HedefSicilBilgi = $"Hedef sicil — ana: {ozet.AnaPersonelId.Value}";

            SetProperty(ref _isAnaSicil, ozet.IsAnaSicil);
            RaisePropertyChanged(nameof(IsAnaSicil));
            RaisePropertyChanged(nameof(CokluSicilDurumText));
            RaisePropertyChanged(nameof(ShowCokluSicilDurum));
        }
        catch (Exception ex)
        {
            LogHelper.Error(PageName, "RefreshCokluSicilState", $"Çoklu sicil durumu okunamadı (PersonelId={pid})", ex);
            CokluSicilHedefSayisi = 0;
            SetProperty(ref _isAnaSicil, false);
            RaisePropertyChanged(nameof(IsAnaSicil));
        }

        RaiseCokluSicilUi();
    }

    private static int ResolvePersonelId(string? personelId)
    {
        if (string.IsNullOrWhiteSpace(personelId)) return 0;
        return int.TryParse(personelId.Trim(), out var pid) ? pid : 0;
    }

    private void OpenCokluSicil()
    {
        if (!CanOpenCokluSicil) return;
        if (!int.TryParse(SicilNo, out var pid) || pid <= 0)
        {
            UiDialog.Warning("Önce personeli kaydedin.", PageName);
            return;
        }

        using var scope = _scopes.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        if (!auth.Can(PageName, YetkiTipleri.Update))
        {
            UiDialog.Warning("Bu işlem için yetkiniz yok.", PageName);
            return;
        }

        try
        {
            var dlg = new CokluSicilEslestirmeWindow(pid, TcKimlikNo, AdSoyad)
            {
                Owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                        ?? Application.Current?.MainWindow
            };
            dlg.ShowDialog();
            RefreshCokluSicilState(SicilNo, TcKimlikNo);
        }
        catch (Exception ex)
        {
            LogHelper.Error(PageName, "OpenCokluSicil", $"PersonelId={pid}", ex);
            UiDialog.Error("Çoklu sicil ekranı açılamadı:\n" + ex.Message, PageName);
        }
    }

    private void RaiseCokluSicilUi()
    {
        RaisePropertyChanged(nameof(CanOpenCokluSicil));
        RaisePropertyChanged(nameof(CokluSicilOpenTooltip));
        RaisePropertyChanged(nameof(CokluSicilDurumText));
        RaisePropertyChanged(nameof(ShowCokluSicilDurum));
        CommandManager.InvalidateRequerySuggested();
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
            return ComboPlaceholderItem(items);
        var found = items.FirstOrDefault(x => x.Id == id.Value);
        if (found != null) return found;
        if (allowMissingZero && id.Value == 0)
            return items.FirstOrDefault(x => x.Id == 0) ?? ComboPlaceholderItem(items);
        return ComboPlaceholderItem(items);
    }

    private static LookupItem? ComboPlaceholderItem(ObservableCollection<LookupItem> items) =>
        items.FirstOrDefault(x => x.Id < 0) ?? items.FirstOrDefault();

    private static List<LookupItem> WithComboPlaceholder(IEnumerable<LookupItem>? items)
    {
        var list = new List<LookupItem>
        {
            new LookupItem { Id = -1, Ad = KisiDisplayHelper.ComboPlaceholder }
        };
        if (items != null)
            list.AddRange(items);
        return list;
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

        if (!IstenCikis.HasValue)
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
        if (!IseGiris.HasValue)
            Errors.Set("IseGiris", "İşe giriş tarihi zorunludur.");
        if (Errors.HasErrors)
        {
            Error = Errors.FirstMessage;
            return;
        }

        AdSoyadAyir(AdSoyad, out string ad, out string soyad);
        var kisi = BuildKisi(ad, soyad);

        bool oncekiYemekHakki = _loadedYemekHakki;
        bool yeniYemekHakki = YemekHakki && YemekAdediForSave > 0;
        if (YemekhaneEtkiMesaji.KaldirmaOnayiGerekir(oncekiYemekHakki, yeniYemekHakki)
            && !UiDialog.Confirm(YemekhaneEtkiMesaji.KaldirmaOnayi, "Yemek hakkı", yesText: "Kaldır", noText: "Vazgeç"))
            return;

        var ok = kisiSvc.KisiGuncelle(
            kisi,
            originalPersonelId: _originalPersonelId ?? kisi.PersonelId,
            FirmaPersoneli,
            PuantajYapilir,
            YemekHakki,
            YemekAdediForSave,
            (FirmaDisiKartNo ?? "").Trim(),
            fotoDegisti: _fotoDirty);

        if (!ok)
        {
            UiDialog.Warning("Kayıt güncellenemedi!.", PageName);
            return;
        }

        _originalPersonelId = kisi.PersonelId.Trim();
        _loadedYemekHakki = yeniYemekHakki;
        UiDialog.Success(
            YemekhaneEtkiMesaji.Ekle("Kayıt güncellendi.", YemekhaneEtkiMesaji.Guncelleme(oncekiYemekHakki, yeniYemekHakki)),
            PageName);
        LoadList();
        EnterViewMode();
    }

    private void SaveAdd(IKisiService kisiSvc)
    {
        Errors.Clear();
        Error = null;
        Errors.Require("SicilNo", SicilNo, "Sicil No zorunludur.");
        Errors.Require("AdSoyad", AdSoyad, "Ad Soyad zorunludur.");
        if (!IseGiris.HasValue)
            Errors.Set("IseGiris", "İşe giriş tarihi zorunludur.");
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
            YemekAdedi = YemekAdediForSave,
            FirmaDisiKartNo = (FirmaDisiKartNo ?? "").Trim(),
            TcKimlikNo = (TcKimlikNo ?? "").Trim(),
            KartNo = (KartNo ?? "").Trim(),
            TaseronCalisanMi = Taseron,
            ZiyaretciMi = Ziyaretci,
            AracKartiMi = AracKarti,
            IsyeriId = GetNullableId(SelectedIsyeri, allowZero: true)
        };

        var validasyonSonuc = kisiSvc.ValidateKisiKayit(validasyonDto);
        if (!validasyonSonuc.IsValid)
        {
            var msg = validasyonSonuc.Message ?? "Doğrulama başarısız.";
            MapValidasyonError(msg);
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
            YemekAdediForSave,
            kartId,
            kartNo,
            kartAdi);

        _originalPersonelId = yeniKisi.PersonelId.Trim();
        _loadedYemekHakki = YemekHakki;
        UiDialog.Success(YemekhaneEtkiMesaji.Ekle("Kayıt tamamlandı.", YemekhaneEtkiMesaji.YeniKayit(YemekHakki)), PageName);
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
            IsyeriId = GetNullableId(SelectedIsyeri, allowZero: true),
            BolumId = GetNullableId(SelectedBolum),
            FirmaId = GetNullableId(SelectedFirmaDetail) ?? GetSeciliFirmaId(),
            IseGirisTarihi = IseGiris?.Date,
            IstenCikisTarihi = IstenCikis?.Date,
            DogumTarihi = DogumTarihi?.Date,
            CalismaStatusu = GetNullableId(SelectedCalismaStatu)?.ToString() ?? "",
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
        DogumTarihi = null;
        IstenCikis = null;

        SelectedPozisyon = ComboPlaceholderItem(Pozisyonlar);
        SelectedIsyeri = ComboPlaceholderItem(IsyerleriForm);
        SelectedBolum = ComboPlaceholderItem(Bolumler);
        SelectedCalismaStatu = ComboPlaceholderItem(CalismaStatuleri);
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
        _yemekAdedi = null;
        _loadedYemekHakki = false;
        RaisePropertyChanged(nameof(YemekAdediText));
        SetFoto(null, dirty: false);
        _isAnaSicil = false;
        CokluSicilHedefSayisi = 0;
        HedefSicilBilgi = null;
        IsHedefSicilOnly = false;
        RaisePropertyChanged(nameof(IsAnaSicil));
        RaisePropertyChanged(nameof(CokluSicilDurumText));
        RaisePropertyChanged(nameof(ShowCokluSicilDurum));
        RaiseCokluSicilUi();
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
        RaisePropertyChanged(nameof(ShowIseGirisMissingText));
        RaisePropertyChanged(nameof(IseGirisMissingText));
        RaisePropertyChanged(nameof(CalismaDurumuText));
        RaisePropertyChanged(nameof(IsIstenCikmis));
        RaisePropertyChanged(nameof(EditingRequired));
        RaisePropertyChanged(nameof(SicilNoRequired));
        RaisePropertyChanged(nameof(AdSoyadRequired));
        RaisePropertyChanged(nameof(IseGirisRequired));
        RaisePropertyChanged(nameof(IsyeriRequired));
        RaisePropertyChanged(nameof(TcKimlikRequired));
        RaisePropertyChanged(nameof(KartNoRequired));
        RaisePropertyChanged(nameof(YemekAdediRequired));
        RaiseCokluSicilUi();
    }

    private void MapValidasyonError(string msg)
    {
        if (ContainsIgnoreCase(msg, "İşyeri"))
            Errors.Set("Isyeri", msg);
        else if (ContainsIgnoreCase(msg, "T.C.") || ContainsIgnoreCase(msg, "TC Kimlik") || ContainsIgnoreCase(msg, "Kimlik No"))
            Errors.Set("TcKimlikNo", msg);
        else if (ContainsIgnoreCase(msg, "Kart No"))
            Errors.Set("KartNo", msg);
        else if (ContainsIgnoreCase(msg, "Yemek"))
            Errors.Set("YemekAdedi", msg);
        else
            Errors.Set("SicilNo", msg);
    }

    private static bool ContainsIgnoreCase(string haystack, string needle)
        => haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;

    private void RefreshReadOnlyFieldDisplays()
    {
        RaisePropertyChanged(nameof(KartNo));
        RaisePropertyChanged(nameof(TcKimlikNo));
        RaisePropertyChanged(nameof(FirmaDisiKartNo));
        RaisePropertyChanged(nameof(Email));
        RaisePropertyChanged(nameof(CepTel));
        RaisePropertyChanged(nameof(YemekAdediText));
        RaisePropertyChanged(nameof(ShowIseGirisMissingText));
    }

    private void ClearDogum()
    {
        if (FieldsReadOnly) return;
        DogumTarihi = null;
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

            if (ctx.IsyeriId.HasValue && ctx.IsyeriId.Value >= 0)
                SelectedIsyeriFilter = Isyerler.FirstOrDefault(x => x.Id == ctx.IsyeriId.Value)
                                       ?? Isyerler.FirstOrDefault();
            else
                SelectedIsyeriFilter = Isyerler.FirstOrDefault(x => x.Id == FirmaIsyeriYetkiHelper.IsyeriFilterTumuId)
                                       ?? Isyerler.FirstOrDefault();

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
