using System.Collections.ObjectModel;
using System.Data;
using System.Windows.Input;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using CeyPASS.WPF.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CeyPASS.WPF.ViewModels;

public sealed class IzinlerViewModel : ObservableObject
{
    private enum ScreenMode { List, Add, Edit }

    private const string PageName = "Izinler";
    private const int TumuInt = 0;
    private const string TumuStr = "ALL";

    private readonly IServiceScopeFactory _scopes;
    private readonly ISessionContext _session;
    private readonly IAuthorizationService _auth;

    private ScreenMode _mode = ScreenMode.List;
    private int? _editingIzinId;
    private bool _saving;
    private bool _suppressChecks;
    private bool _kisilerLoaded;
    private bool _izinTipleriLoaded;
    private List<FirmaIsyeriYetkiDTO> _yetkiler = new();

    private Firma? _selectedFirma;
    private KisiListItem? _selectedKisi;
    private IzinTip? _selectedIzinTip;
    private DataRowView? _selectedRow;
    private DataView? _grid;

    private DateTime _baslangicTarihi = DateTime.Today;
    private DateTime _bitisTarihi = DateTime.Today;
    private TimeSpan _baslangicSaati = TimeSpan.FromHours(9);
    private TimeSpan _bitisSaati = TimeSpan.FromHours(18);
    private string _aciklama = "";
    private bool _saatlikIzinMi;
    private bool _yarimGunYillikIzin;
    private int _yarimGunDilimIndex;
    private string? _error;
    private bool _formEnabled;
    private bool _showSaveCancel;
    private bool _saatEnabled;
    private bool _izinTipEnabled = true;
    private bool _yarimGunDilimEnabled;
    private bool _aciklamaReadOnly;
    private bool _saatlikCheckEnabled = true;
    private bool _canAdd;
    private bool _canEdit;
    private bool _canDelete;
    private bool _canList = true;
    private bool _filtersEnabled = true;

    public IzinlerViewModel(IServiceProvider root)
    {
        _scopes = root.GetRequiredService<IServiceScopeFactory>();
        _session = root.GetRequiredService<ISessionContext>();
        _auth = root.GetRequiredService<IAuthorizationService>();

        Firmalar = new ObservableCollection<Firma>();
        Kisiler = new ObservableCollection<KisiListItem>();
        IzinTipleri = new ObservableCollection<IzinTip>();
        YarimGunDilimler = new ObservableCollection<string>
        {
            YarimGunYillikIzinHelper.DilimComboText(YarimGunYillikIzinHelper.Dilim.Sabah),
            YarimGunYillikIzinHelper.DilimComboText(YarimGunYillikIzinHelper.Dilim.OgledenSonra)
        };

        ListeleCommand = new RelayCommand(Listele, () => CanList && !_saving);
        AddCommand = new RelayCommand(EnterAddMode, () => CanAdd);
        EditCommand = new RelayCommand(EnterEditMode, () => CanEdit);
        DeleteCommand = new RelayCommand(DeleteSelected, () => CanDelete);
        SaveCommand = new RelayCommand(Save, () => ShowSaveCancel && !_saving);
        CancelCommand = new RelayCommand(ExitEditMode, () => ShowSaveCancel && !_saving);
        LoadKisilerCommand = new RelayCommand(EnsureKisilerLoaded);
        LoadIzinTipleriCommand = new RelayCommand(EnsureIzinTipleriLoaded);

        if (!_auth.ViewAbility(PageName))
        {
            Error = "İzinler sayfasını görüntüleme yetkiniz yok.";
            return;
        }

        LoadFirms();
        ExitEditMode();
        RefreshToolbar();
    }

    public ObservableCollection<Firma> Firmalar { get; }
    public ObservableCollection<KisiListItem> Kisiler { get; }
    public ObservableCollection<IzinTip> IzinTipleri { get; }
    public ObservableCollection<string> YarimGunDilimler { get; }

    public Firma? SelectedFirma
    {
        get => _selectedFirma;
        set
        {
            if (Equals(_selectedFirma, value)) return;
            SetProperty(ref _selectedFirma, value);
            Kisiler.Clear();
            IzinTipleri.Clear();
            _kisilerLoaded = false;
            _izinTipleriLoaded = false;
            _selectedKisi = null;
            _selectedIzinTip = null;
            RaisePropertyChanged(nameof(SelectedKisi));
            RaisePropertyChanged(nameof(SelectedIzinTip));
        }
    }

    public KisiListItem? SelectedKisi
    {
        get => _selectedKisi;
        set => SetProperty(ref _selectedKisi, value);
    }

    public IzinTip? SelectedIzinTip
    {
        get => _selectedIzinTip;
        set => SetProperty(ref _selectedIzinTip, value);
    }

    public DataView? Grid
    {
        get => _grid;
        private set => SetProperty(ref _grid, value);
    }

    public DataRowView? SelectedRow
    {
        get => _selectedRow;
        set
        {
            if (Equals(_selectedRow, value)) return;
            SetProperty(ref _selectedRow, value);
            if (_mode == ScreenMode.List)
                RefreshToolbar();
        }
    }

    public DateTime BaslangicTarihi
    {
        get => _baslangicTarihi;
        set
        {
            var d = value.Date;
            if (_baslangicTarihi == d) return;
            SetProperty(ref _baslangicTarihi, d);
            if (YarimGunYillikIzin)
            {
                BitisTarihi = d;
                UygulaYarimGunSaatleri();
            }
        }
    }

    public DateTime BitisTarihi
    {
        get => _bitisTarihi;
        set => SetProperty(ref _bitisTarihi, value.Date);
    }

    public TimeSpan BaslangicSaati
    {
        get => _baslangicSaati;
        set
        {
            if (_baslangicSaati == value) return;
            SetProperty(ref _baslangicSaati, value);
            RaisePropertyChanged(nameof(BaslangicSaatText));
        }
    }

    public TimeSpan BitisSaati
    {
        get => _bitisSaati;
        set
        {
            if (_bitisSaati == value) return;
            SetProperty(ref _bitisSaati, value);
            RaisePropertyChanged(nameof(BitisSaatText));
        }
    }

    public string BaslangicSaatText
    {
        get => BaslangicSaati.ToString(@"hh\:mm");
        set
        {
            if (TimeSpan.TryParse(value, out var t))
                BaslangicSaati = t;
        }
    }

    public string BitisSaatText
    {
        get => BitisSaati.ToString(@"hh\:mm");
        set
        {
            if (TimeSpan.TryParse(value, out var t))
                BitisSaati = t;
        }
    }

    public string Aciklama
    {
        get => _aciklama;
        set => SetProperty(ref _aciklama, value ?? "");
    }

    public bool SaatlikIzinMi
    {
        get => _saatlikIzinMi;
        set
        {
            if (_suppressChecks) { SetProperty(ref _saatlikIzinMi, value); return; }
            if (_saatlikIzinMi == value) return;
            SetProperty(ref _saatlikIzinMi, value);
            if (value)
            {
                _suppressChecks = true;
                YarimGunYillikIzin = false;
                _suppressChecks = false;
            }
            ApplySaatlikRule();
            ApplyYarimGunRule();
        }
    }

    public bool YarimGunYillikIzin
    {
        get => _yarimGunYillikIzin;
        set
        {
            if (_suppressChecks) { SetProperty(ref _yarimGunYillikIzin, value); return; }
            if (_yarimGunYillikIzin == value) return;
            SetProperty(ref _yarimGunYillikIzin, value);
            if (value)
            {
                _suppressChecks = true;
                SaatlikIzinMi = false;
                _suppressChecks = false;

                if (_mode == ScreenMode.List)
                {
                    if (!_auth.Can(PageName, YetkiTipleri.Create))
                    {
                        _suppressChecks = true;
                        SetProperty(ref _yarimGunYillikIzin, false);
                        _suppressChecks = false;
                        return;
                    }
                    EnsureKisilerLoaded();
                    EnsureIzinTipleriLoaded();
                    EnterAddMode(fromYarimGun: true);
                    return;
                }
            }
            ApplyYarimGunRule();
            ApplySaatlikRule();
        }
    }

    public int YarimGunDilimIndex
    {
        get => _yarimGunDilimIndex;
        set
        {
            if (_yarimGunDilimIndex == value) return;
            SetProperty(ref _yarimGunDilimIndex, value);
            if (YarimGunYillikIzin)
                UygulaYarimGunSaatleri();
        }
    }

    public string? Error
    {
        get => _error;
        private set => SetProperty(ref _error, value);
    }

    public BindableFieldErrors Errors { get; } = new();

    public bool FormEnabled
    {
        get => _formEnabled;
        private set => SetProperty(ref _formEnabled, value);
    }

    public bool ShowSaveCancel
    {
        get => _showSaveCancel;
        private set
        {
            if (_showSaveCancel == value) return;
            SetProperty(ref _showSaveCancel, value);
            RaiseCanExecutes();
        }
    }

    public bool SaatEnabled
    {
        get => _saatEnabled;
        private set => SetProperty(ref _saatEnabled, value);
    }

    public bool IzinTipEnabled
    {
        get => _izinTipEnabled;
        private set => SetProperty(ref _izinTipEnabled, value);
    }

    public bool YarimGunDilimEnabled
    {
        get => _yarimGunDilimEnabled;
        private set => SetProperty(ref _yarimGunDilimEnabled, value);
    }

    public bool AciklamaReadOnly
    {
        get => _aciklamaReadOnly;
        private set => SetProperty(ref _aciklamaReadOnly, value);
    }

    public bool SaatlikCheckEnabled
    {
        get => _saatlikCheckEnabled;
        private set => SetProperty(ref _saatlikCheckEnabled, value);
    }

    public bool CanAdd
    {
        get => _canAdd;
        private set
        {
            if (_canAdd == value) return;
            SetProperty(ref _canAdd, value);
            RaiseCanExecutes();
        }
    }

    public bool CanEdit
    {
        get => _canEdit;
        private set
        {
            if (_canEdit == value) return;
            SetProperty(ref _canEdit, value);
            RaiseCanExecutes();
        }
    }

    public bool CanDelete
    {
        get => _canDelete;
        private set
        {
            if (_canDelete == value) return;
            SetProperty(ref _canDelete, value);
            RaiseCanExecutes();
        }
    }

    public bool CanList
    {
        get => _canList;
        private set
        {
            if (_canList == value) return;
            SetProperty(ref _canList, value);
            RaiseCanExecutes();
        }
    }

    public bool FiltersEnabled
    {
        get => _filtersEnabled;
        private set => SetProperty(ref _filtersEnabled, value);
    }

    public ICommand ListeleCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand LoadKisilerCommand { get; }
    public ICommand LoadIzinTipleriCommand { get; }

    private static void RaiseCanExecutes() =>
        System.Windows.Input.CommandManager.InvalidateRequerySuggested();

    private void LoadFirms()
    {
        try
        {
            using var scope = _scopes.CreateScope();
            var yetkiSvc = scope.ServiceProvider.GetRequiredService<IKullaniciFirmaIsyeriYetkiService>();
            var fsvc = scope.ServiceProvider.GetRequiredService<IFirmaService>();
            _yetkiler = yetkiSvc.GetYetkiler((int)_session.AktifKullaniciId!) ?? new List<FirmaIsyeriYetkiDTO>();
            bool isAdmin = FirmaIsyeriYetkiHelper.IsAdmin(_session.RolId);
            var list = FirmaIsyeriYetkiHelper.FilterFirmalar(fsvc.GetPuantajFirmalar(), _yetkiler, isAdmin);

            var firmaIds = _yetkiler.Select(y => y.FirmaId).Distinct().ToHashSet();
            if (firmaIds.Count == 0)
                list.Insert(0, new Firma { FirmaId = TumuInt, FirmaAdi = "— TÜMÜ —" });

            Firmalar.Clear();
            foreach (var f in list)
                Firmalar.Add(f);

            SelectedFirma = Firmalar.FirstOrDefault();
        }
        catch (Exception ex)
        {
            Error = "Firmalar yüklenemedi: " + ex.Message;
        }
    }

    public void EnsureKisilerLoaded()
    {
        if (_kisilerLoaded) return;
        var firmaId = SelectedFirma?.FirmaId ?? 0;
        if (firmaId == TumuInt && _session.AktifFirmaId.HasValue)
            firmaId = (int)_session.AktifFirmaId.Value;
        if (firmaId <= 0) return;

        try
        {
            using var scope = _scopes.CreateScope();
            var ksvc = scope.ServiceProvider.GetRequiredService<IKisiQueryService>();
            bool isAdmin = FirmaIsyeriYetkiHelper.IsAdmin(_session.RolId);
            var (isyeriId, isyeriIdIn) = FirmaIsyeriYetkiHelper.ResolveKisiQueryIsyeriFilter(
                firmaId, null, _yetkiler, isAdmin);
            var kisiler = ksvc.GetAktifKisilerByFirma(firmaId, isyeriId: isyeriId, isyeriIdIn: isyeriIdIn);
            kisiler.Insert(0, new KisiListItem { PersonelId = TumuStr, AdSoyad = "— TÜMÜ —" });

            Kisiler.Clear();
            foreach (var k in kisiler)
                Kisiler.Add(k);
            SelectedKisi = Kisiler.FirstOrDefault();
            _kisilerLoaded = true;
        }
        catch (Exception ex)
        {
            Error = "Kişi listesi yüklenemedi: " + ex.Message;
        }
    }

    public void EnsureIzinTipleriLoaded()
    {
        if (_izinTipleriLoaded) return;
        if (SelectedFirma is null) return;

        try
        {
            using var scope = _scopes.CreateScope();
            var isvc = scope.ServiceProvider.GetRequiredService<IIzinTipService>();
            var tipler = isvc.GetAktif();
            tipler.Insert(0, new IzinTip { IzinTipId = TumuInt, Ad = "— TÜMÜ —" });

            IzinTipleri.Clear();
            foreach (var t in tipler)
                IzinTipleri.Add(t);
            SelectedIzinTip = IzinTipleri.FirstOrDefault();
            _izinTipleriLoaded = true;
        }
        catch (Exception ex)
        {
            Error = "İzin tipleri yüklenemedi: " + ex.Message;
        }
    }

    private void Listele()
    {
        if (!_auth.Can(PageName, YetkiTipleri.View))
        {
            Error = "Görüntüleme yetkiniz yok.";
            return;
        }

        Error = null;
        EnsureKisilerLoaded();
        EnsureIzinTipleriLoaded();
        ReloadGrid();
        RefreshToolbar();
    }

    private void ReloadGrid()
    {
        try
        {
            using var scope = _scopes.CreateScope();
            var kisvc = scope.ServiceProvider.GetRequiredService<IKisiIzinService>();

            int? firmaId = null;
            if (SelectedFirma is not null && SelectedFirma.FirmaId != TumuInt)
                firmaId = SelectedFirma.FirmaId;

            string personelId = "";
            if (SelectedKisi is not null
                && !string.Equals(SelectedKisi.PersonelId, TumuStr, StringComparison.OrdinalIgnoreCase))
                personelId = SelectedKisi.PersonelId ?? "";

            int? izinTipId = null;
            if (SelectedIzinTip is not null && SelectedIzinTip.IzinTipId != TumuInt)
                izinTipId = SelectedIzinTip.IzinTipId;

            DateTime bas = BaslangicTarihi.Date;
            DateTime bit = BitisTarihi.Date.AddDays(1).AddSeconds(-1);

            var dt = kisvc.GetTumIzinler(firmaId, personelId, izinTipId, bas, bit);
            Grid = dt?.DefaultView;
            SelectedRow = null;
        }
        catch (Exception ex)
        {
            Error = "Liste hatası: " + ex.Message;
            Grid = null;
        }
    }

    private void EnterAddMode() => EnterAddMode(fromYarimGun: false);

    private void EnterAddMode(bool fromYarimGun)
    {
        if (!_auth.Can(PageName, YetkiTipleri.Create))
        {
            Error = "Ekleme yetkiniz yok.";
            return;
        }

        Error = null;
        Aciklama = "";
        BaslangicTarihi = DateTime.Today;
        BitisTarihi = DateTime.Today;
        BaslangicSaati = TimeSpan.FromHours(9);
        BitisSaati = TimeSpan.FromHours(18);
        if (!fromYarimGun)
            ResetYarimGunUi(clearChecks: true);

        if (SelectedFirma?.FirmaId == TumuInt && _session.AktifFirmaId.HasValue)
        {
            var aktif = Firmalar.FirstOrDefault(f => f.FirmaId == (int)_session.AktifFirmaId.Value);
            if (aktif is not null)
                SelectedFirma = aktif;
        }

        EnsureKisilerLoaded();
        EnsureIzinTipleriLoaded();
        _mode = ScreenMode.Add;
        _editingIzinId = null;
        FormEnabled = true;
        FiltersEnabled = true;
        ShowSaveCancel = true;
        CanList = false;
        if (fromYarimGun)
        {
            _suppressChecks = true;
            SetProperty(ref _yarimGunYillikIzin, true);
            _suppressChecks = false;
            ApplyYarimGunRule();
            ApplySaatlikRule();
        }
        RefreshToolbar();
    }

    private void EnterEditMode()
    {
        if (!_auth.Can(PageName, YetkiTipleri.Update))
        {
            Error = "Güncelleme yetkiniz yok.";
            return;
        }
        if (SelectedRow is null)
        {
            Error = "Güncellemek için satır seçin.";
            return;
        }

        var id = TryGetKisiIzinId(SelectedRow);
        if (id is null)
        {
            Error = "KisiIzinId bulunamadı.";
            return;
        }

        Error = null;
        EnsureKisilerLoaded();
        EnsureIzinTipleriLoaded();
        _mode = ScreenMode.Edit;
        _editingIzinId = id;
        LoadRowToInputs(id.Value);
        FormEnabled = true;
        ShowSaveCancel = true;
        CanList = false;
        RefreshToolbar();
    }

    private void LoadRowToInputs(int kisiIzinId)
    {
        using var scope = _scopes.CreateScope();
        var kisvc = scope.ServiceProvider.GetRequiredService<IKisiIzinService>();
        var kayit = kisvc.GetById(kisiIzinId);
        if (kayit is null) return;

        BaslangicTarihi = kayit.Baslangic.Date;
        BaslangicSaati = kayit.Baslangic.TimeOfDay;
        BitisTarihi = kayit.Bitis.Date;
        BitisSaati = kayit.Bitis.TimeOfDay;
        Aciklama = kayit.Aciklama ?? "";

        bool yarimGun = YarimGunYillikIzinHelper.KayitYarimGunYillikIzinMi(
            kayit.IzinId, kayit.SaatlikIzinMi, kayit.SureDakika, kayit.Aciklama);

        _suppressChecks = true;
        YarimGunYillikIzin = yarimGun;
        SaatlikIzinMi = kayit.SaatlikIzinMi && !yarimGun;
        if (yarimGun)
        {
            if (YarimGunYillikIzinHelper.TryDilimFromAciklama(kayit.Aciklama, out var dilim))
                YarimGunDilimIndex = (int)dilim;
            else if (kayit.Baslangic.TimeOfDay.Hours >= 12)
                YarimGunDilimIndex = (int)YarimGunYillikIzinHelper.Dilim.OgledenSonra;
            else
                YarimGunDilimIndex = (int)YarimGunYillikIzinHelper.Dilim.Sabah;
        }
        _suppressChecks = false;

        SelectedIzinTip = IzinTipleri.FirstOrDefault(t => t.IzinTipId == kayit.IzinId);
        SelectedKisi = Kisiler.FirstOrDefault(k =>
            string.Equals(k.PersonelId, kayit.PersonelId, StringComparison.OrdinalIgnoreCase));

        ApplyYarimGunRule();
        ApplySaatlikRule();
    }

    private void ExitEditMode()
    {
        _mode = ScreenMode.List;
        _editingIzinId = null;
        FormEnabled = false;
        ShowSaveCancel = false;
        CanList = true;
        FiltersEnabled = true;
        Aciklama = "";
        ResetYarimGunUi(clearChecks: true);
        ApplySaatlikRule();
        ApplyYarimGunRule();
        RefreshToolbar();
    }

    private void DeleteSelected()
    {
        if (!_auth.Can(PageName, YetkiTipleri.Delete))
        {
            Error = "Silme yetkiniz yok.";
            return;
        }
        if (SelectedRow is null)
        {
            Error = "Silmek için satır seçin.";
            return;
        }

        var id = TryGetKisiIzinId(SelectedRow);
        if (id is null)
        {
            Error = "KisiIzinId bulunamadı.";
            return;
        }

        if (!UiDialog.Confirm("Seçili izin silinsin mi?", "Onay", yesText: "Sil", noText: "Vazgeç"))
            return;

        try
        {
            using var scope = _scopes.CreateScope();
            var kisvc = scope.ServiceProvider.GetRequiredService<IKisiIzinService>();
            if (!kisvc.PasifYap(id.Value))
            {
                Error = "İşlem başarısız.";
                return;
            }
            var izinId = id.Value;
            Error = null;
            UiDialog.SuccessWithUndo("İzin silindi.", () =>
            {
                using var s2 = _scopes.CreateScope();
                if (s2.ServiceProvider.GetRequiredService<IKisiIzinService>().AktifYap(izinId))
                {
                    ReloadGrid();
                    RefreshToolbar();
                    UiDialog.Success("Geri alındı.", "Bilgi");
                }
                else
                    UiDialog.Warning("Geri alma başarısız.", "Bilgi");
            }, "Bilgi");
            ReloadGrid();
            RefreshToolbar();
        }
        catch (Exception ex)
        {
            Error = "Silme hatası: " + ex.Message;
        }
    }

    private void Save()
    {
        if (_saving) return;
        _saving = true;
        RaiseCanExecutes();

        try
        {
            Errors.Clear();
            Error = null;

            if (_mode == ScreenMode.List)
            {
                UiDialog.Info("Kayıt için önce «İzin Ekle» veya «Güncelle» kullanın.", "Bilgi");
                return;
            }

            if (_mode == ScreenMode.Add && !_auth.Can(PageName, YetkiTipleri.Create))
            {
                Error = "Ekleme yetkiniz yok.";
                return;
            }
            if (_mode == ScreenMode.Edit && !_auth.Can(PageName, YetkiTipleri.Update))
            {
                Error = "Güncelleme yetkiniz yok.";
                return;
            }

            if (YarimGunYillikIzin)
                UygulaYarimGunSaatleri();

            if (!ValidateInputs(out var msg))
            {
                Error = Errors.FirstMessage ?? msg;
                return;
            }

            int selectedFirmaId = (int)(_session.AktifFirmaId ?? 0);
            if (SelectedFirma is not null && SelectedFirma.FirmaId != TumuInt)
                selectedFirmaId = SelectedFirma.FirmaId;

            bool yarimGun = YarimGunYillikIzin;
            bool saatlik = yarimGun || SaatlikIzinMi;

            string? personelId = null;
            if (SelectedKisi is not null
                && !string.Equals(SelectedKisi.PersonelId, TumuStr, StringComparison.OrdinalIgnoreCase))
                personelId = SelectedKisi.PersonelId;
            else if (_mode == ScreenMode.Edit && SelectedRow is not null)
                personelId = TryGetPersonelId(SelectedRow);

            if (string.IsNullOrWhiteSpace(personelId))
            {
                Errors.Set("Kisi", "Kayıt için «— TÜMÜ —» dışında bir personel seçiniz.");
                Error = Errors.FirstMessage;
                return;
            }

            int izinTipId;
            if (yarimGun)
                izinTipId = YarimGunYillikIzinHelper.YillikIzinTipId;
            else if (SelectedIzinTip is null || SelectedIzinTip.IzinTipId == TumuInt)
            {
                Errors.Set("IzinTip", "Lütfen bir izin tipi seçiniz.");
                Error = Errors.FirstMessage;
                return;
            }
            else
                izinTipId = SelectedIzinTip.IzinTipId;

            string aciklama = Aciklama?.Trim() ?? "";
            DateTime bas, bit;

            if (yarimGun)
            {
                if (YarimGunDilimIndex < 0)
                {
                    Errors.Set("YarimGunDilim", "Yarım gün için sabah veya öğleden sonra dilimini seçiniz.");
                    Error = Errors.FirstMessage;
                    return;
                }
                var dilim = (YarimGunYillikIzinHelper.Dilim)YarimGunDilimIndex;
                YarimGunYillikIzinHelper.KayitZamanlari(dilim, BaslangicTarihi.Date, out bas, out bit);
                aciklama = YarimGunYillikIzinHelper.AciklamaMetni(dilim);
                saatlik = true;
            }
            else if (saatlik)
            {
                bas = BaslangicTarihi.Date + BaslangicSaati;
                bit = BitisTarihi.Date + BitisSaati;
            }
            else
            {
                bas = BaslangicTarihi.Date;
                bit = BitisTarihi.Date;
            }

            var izin = new KisiIzin
            {
                KisiIzinId = _mode == ScreenMode.Edit ? _editingIzinId : null,
                FirmaId = selectedFirmaId,
                PersonelId = personelId,
                IzinId = izinTipId,
                Baslangic = bas,
                Bitis = bit,
                Aciklama = aciklama,
                SaatlikIzinMi = saatlik,
                OlusturanKullaniciId = (int)_session.AktifKullaniciId!
            };

            using var scope = _scopes.CreateScope();
            var kisvc = scope.ServiceProvider.GetRequiredService<IKisiIzinService>();
            bool ok = _mode == ScreenMode.Add ? kisvc.Ekle(izin) : kisvc.Guncelle(izin);
            if (!ok)
            {
                UiDialog.Error("İşlem başarısız. Veritabanı bağlantısı veya yetkileri kontrol edin.", "Hata");
                return;
            }

            UiDialog.Success("Kayıt tamamlandı.", "Bilgi");
            if (_izinTipleriLoaded)
                SelectedIzinTip = IzinTipleri.FirstOrDefault(t => t.IzinTipId == TumuInt);
            ExitEditMode();
            ReloadGrid();
            RefreshToolbar();
            Error = null;
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException?.Message;
            Error = string.IsNullOrWhiteSpace(inner) ? ex.Message : ex.Message + " | " + inner;
        }
        finally
        {
            _saving = false;
            RaiseCanExecutes();
        }
    }

    private bool ValidateInputs(out string msg)
    {
        msg = "";
        string? personelId = null;
        if (SelectedKisi is not null
            && !string.Equals(SelectedKisi.PersonelId, TumuStr, StringComparison.OrdinalIgnoreCase))
            personelId = SelectedKisi.PersonelId;
        else if (_mode == ScreenMode.Edit && SelectedRow is not null)
            personelId = TryGetPersonelId(SelectedRow);

        int? izinTipId = null;
        if (SelectedIzinTip is not null && SelectedIzinTip.IzinTipId != TumuInt)
            izinTipId = SelectedIzinTip.IzinTipId;

        bool yarimGun = YarimGunYillikIzin;
        bool saatlik = yarimGun || SaatlikIzinMi;
        if (yarimGun)
            izinTipId = YarimGunYillikIzinHelper.YillikIzinTipId;

        if (yarimGun && YarimGunDilimIndex < 0)
        {
            msg = "Yarım gün için sabah veya öğleden sonra dilimini seçiniz.";
            Errors.Set("YarimGunDilim", msg);
            return false;
        }

        var dto = new IzinKayitValidasyonDTO
        {
            SaatlikIzinMi = saatlik,
            YarimGunYillikIzinMi = yarimGun,
            PersonelId = personelId,
            IzinTipId = izinTipId,
            BaslangicTarihi = BaslangicTarihi,
            BitisTarihi = yarimGun ? BaslangicTarihi : BitisTarihi,
            BaslangicSaati = saatlik ? BaslangicSaati : null,
            BitisSaati = saatlik ? BitisSaati : null
        };

        using var scope = _scopes.CreateScope();
        var kisvc = scope.ServiceProvider.GetRequiredService<IKisiIzinService>();
        var result = kisvc.ValidateKayit(dto);
        msg = result.Message ?? "";
        if (result.IsValid) return true;

        if (msg.Contains("kişi", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("personel", StringComparison.OrdinalIgnoreCase))
            Errors.Set("Kisi", msg);
        else if (msg.Contains("izin tipi", StringComparison.OrdinalIgnoreCase)
                 || msg.Contains("yıllık izin", StringComparison.OrdinalIgnoreCase))
            Errors.Set("IzinTip", msg);
        else if (msg.Contains("başlangıç ve bitiş tarihi", StringComparison.OrdinalIgnoreCase)
                 || msg.Contains("Bitiş tarihi", StringComparison.OrdinalIgnoreCase))
            Errors.Set("BitisTarihi", msg);
        else if (msg.Contains("Bitiş, başlangıçtan", StringComparison.OrdinalIgnoreCase))
            Errors.Set("BitisSaat", msg);
        else
            Errors.Set("Kisi", msg);

        return false;
    }

    private void ApplySaatlikRule()
    {
        if (YarimGunYillikIzin) return;

        SaatEnabled = SaatlikIzinMi && FormEnabled;
        if (!YarimGunYillikIzin)
            AciklamaReadOnly = false;

        if (SaatlikIzinMi && FormEnabled)
        {
            using var scope = _scopes.CreateScope();
            var tipId = scope.ServiceProvider.GetRequiredService<IIzinTipService>().GetSaatlikIzinTipId();
            if (tipId.HasValue)
            {
                EnsureIzinTipleriLoaded();
                SelectedIzinTip = IzinTipleri.FirstOrDefault(t => t.IzinTipId == tipId.Value);
            }
        }

        SyncIzinTipEnabled();
    }

    private void ApplyYarimGunRule()
    {
        bool yarimGun = YarimGunYillikIzin;
        YarimGunDilimEnabled = yarimGun && FormEnabled;
        SaatlikCheckEnabled = !yarimGun && FormEnabled;
        SaatEnabled = !yarimGun && SaatlikIzinMi && FormEnabled;
        AciklamaReadOnly = yarimGun;

        if (yarimGun && FormEnabled)
        {
            if (YarimGunDilimIndex < 0)
                YarimGunDilimIndex = 0;
            EnsureIzinTipleriLoaded();
            SelectedIzinTip = IzinTipleri.FirstOrDefault(t => t.IzinTipId == YarimGunYillikIzinHelper.YillikIzinTipId);
            BitisTarihi = BaslangicTarihi.Date;
            UygulaYarimGunSaatleri();
        }

        SyncIzinTipEnabled();
    }

    /// <summary>
    /// Liste modunda izin tipi filtresi açık kalır; ekle/güncellede saatlik veya yarım günde kilitlenir.
    /// </summary>
    private void SyncIzinTipEnabled()
    {
        IzinTipEnabled = !FormEnabled || !(SaatlikIzinMi || YarimGunYillikIzin);
    }

    private void UygulaYarimGunSaatleri()
    {
        if (!YarimGunYillikIzin || YarimGunDilimIndex < 0) return;
        var dilim = (YarimGunYillikIzinHelper.Dilim)YarimGunDilimIndex;
        YarimGunYillikIzinHelper.KayitZamanlari(dilim, BaslangicTarihi.Date, out var bas, out var bit);
        BaslangicSaati = bas.TimeOfDay;
        BitisSaati = bit.TimeOfDay;
        BitisTarihi = BaslangicTarihi.Date;
        Aciklama = YarimGunYillikIzinHelper.AciklamaMetni(dilim);
    }

    private void ResetYarimGunUi(bool clearChecks)
    {
        _suppressChecks = true;
        try
        {
                if (clearChecks)
                {
                    SetProperty(ref _yarimGunYillikIzin, false);
                    SetProperty(ref _saatlikIzinMi, false);
                    RaisePropertyChanged(nameof(YarimGunYillikIzin));
                    RaisePropertyChanged(nameof(SaatlikIzinMi));
                }
            YarimGunDilimIndex = 0;
        }
        finally
        {
            _suppressChecks = false;
        }
        YarimGunDilimEnabled = false;
        AciklamaReadOnly = false;
        SaatlikCheckEnabled = FormEnabled;
    }

    private void RefreshToolbar()
    {
        bool listMode = _mode == ScreenMode.List;
        bool hasRow = SelectedRow is not null;
        CanAdd = listMode && _auth.Can(PageName, YetkiTipleri.Create);
        CanEdit = listMode && hasRow && _auth.Can(PageName, YetkiTipleri.Update);
        CanDelete = listMode && hasRow && _auth.Can(PageName, YetkiTipleri.Delete);
        CanList = listMode && _auth.Can(PageName, YetkiTipleri.View);
        FormEnabled = !listMode;
        SaatlikCheckEnabled = FormEnabled && !YarimGunYillikIzin;
        if (FormEnabled)
        {
            ApplyYarimGunRule();
            ApplySaatlikRule();
        }
        else
        {
            SaatEnabled = false;
            YarimGunDilimEnabled = false;
            SyncIzinTipEnabled();
        }
        RaiseCanExecutes();
    }

    private static int? TryGetKisiIzinId(DataRowView row)
    {
        if (row.Row.Table.Columns.Contains("KisiIzinId") && row["KisiIzinId"] is not DBNull)
            return Convert.ToInt32(row["KisiIzinId"]);
        return null;
    }

    private static string? TryGetPersonelId(DataRowView row)
    {
        foreach (var col in new[] { "PersonelId", "SicilNo", "Sicil No" })
        {
            if (row.Row.Table.Columns.Contains(col) && row[col] is not DBNull and not null)
                return row[col]?.ToString();
        }
        return null;
    }
}
