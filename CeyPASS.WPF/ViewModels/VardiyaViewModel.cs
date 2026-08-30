using System.Collections.ObjectModel;
using System.Windows.Input;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CeyPASS.WPF.ViewModels;

public sealed class VardiyaViewModel : ObservableObject
{
    private enum ScreenMode { List, Add, Edit }

    private const string PageName = "Vardiyalar";

    private readonly IServiceScopeFactory _scopes;
    private readonly ISessionContext _session;
    private readonly bool _adminPanelMode;
    private ScreenMode _mode = ScreenMode.List;
    private CalismaSekli? _selected;
    private bool _suppressSelection;
    private string _ad = "";
    private string _baslangicText = "07:00";
    private string _bitisText = "15:00";
    private string _basTolText = "00:15";
    private string _bitTolText = "00:15";
    private string _yemekAktifText = "17:00";
    private string? _error;
    private bool _fieldsReadOnly = true;
    private bool _showSaveCancel;
    private bool _canAdd;
    private bool _canEdit;
    private bool _canDelete;
    private bool _listEnabled = true;
    private bool _canOpenYemekSaatleri;

    public VardiyaViewModel(IServiceProvider root, bool adminPanelMode = false)
    {
        _scopes = root.GetRequiredService<IServiceScopeFactory>();
        _session = root.GetRequiredService<ISessionContext>();
        _adminPanelMode = adminPanelMode;
        Items = new ObservableCollection<CalismaSekli>();

        AddCommand = new RelayCommand(EnterAddMode, () => CanAdd);
        EditCommand = new RelayCommand(EnterEditMode, () => CanEdit);
        DeleteCommand = new RelayCommand(DeleteSelected, () => CanDelete);
        SaveCommand = new RelayCommand(Save, () => ShowSaveCancel);
        CancelCommand = new RelayCommand(EnterListMode, () => ShowSaveCancel);
        RefreshCommand = new RelayCommand(LoadList);
        YemekSaatleriCommand = new RelayCommand(OpenYemekSaatleri, () => CanOpenYemekSaatleri);

        LoadList();
    }

    public ObservableCollection<CalismaSekli> Items { get; }

    public CalismaSekli? SelectedItem
    {
        get => _selected;
        set
        {
            if (ReferenceEquals(_selected, value)) return;
            if (_selected is not null && value is not null && _selected.Id == value.Id)
            {
                SetProperty(ref _selected, value);
                return;
            }

            SetProperty(ref _selected, value);
            if (_suppressSelection) return;
            if (_mode == ScreenMode.List)
            {
                FillFromSelection();
                RefreshToolbar();
            }
        }
    }

    public string Ad
    {
        get => _ad;
        set => SetProperty(ref _ad, value);
    }

    public string BaslangicText
    {
        get => _baslangicText;
        set => SetProperty(ref _baslangicText, value);
    }

    public string BitisText
    {
        get => _bitisText;
        set => SetProperty(ref _bitisText, value);
    }

    public string BaslangicToleransText
    {
        get => _basTolText;
        set => SetProperty(ref _basTolText, value);
    }

    public string BitisToleransText
    {
        get => _bitTolText;
        set => SetProperty(ref _bitTolText, value);
    }

    public string YemekAktiflestirmeText
    {
        get => _yemekAktifText;
        set => SetProperty(ref _yemekAktifText, value);
    }

    public string? Error
    {
        get => _error;
        private set => SetProperty(ref _error, value);
    }

    public BindableFieldErrors Errors { get; } = new();

    public bool FieldsReadOnly
    {
        get => _fieldsReadOnly;
        private set => SetProperty(ref _fieldsReadOnly, value);
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

    public bool ListEnabled
    {
        get => _listEnabled;
        private set => SetProperty(ref _listEnabled, value);
    }

    public bool CanOpenYemekSaatleri
    {
        get => _canOpenYemekSaatleri;
        private set => SetProperty(ref _canOpenYemekSaatleri, value);
    }

    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand YemekSaatleriCommand { get; }

    private void LoadList()
    {
        Error = null;
        try
        {
            using var scope = _scopes.CreateScope();
            var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
            if (!auth.ViewAbility(PageName))
            {
                Error = "Vardiyalar ekranını görüntüleme yetkiniz yok.";
                Items.Clear();
                ClearFields();
                RefreshToolbar(auth);
                return;
            }

            if (!_adminPanelMode && !_session.AktifFirmaId.HasValue)
            {
                Error = "Aktif firma seçili değil.";
                Items.Clear();
                ClearFields();
                RefreshToolbar(auth);
                return;
            }

            var svc = scope.ServiceProvider.GetRequiredService<ICalismaSekliService>();
            var list = _adminPanelMode
                ? (svc.GetAllForAdmin() ?? new List<CalismaSekli>())
                : (svc.GetAll((int)_session.AktifFirmaId!.Value) ?? new List<CalismaSekli>());
            var keepId = SelectedItem?.Id;

            _suppressSelection = true;
            Items.Clear();
            foreach (var it in list)
                Items.Add(it);

            CalismaSekli? next = null;
            if (keepId.HasValue)
                next = Items.FirstOrDefault(x => x.Id == keepId.Value);
            next ??= Items.FirstOrDefault();
            _selected = next;
            RaisePropertyChanged(nameof(SelectedItem));
            _suppressSelection = false;

            FillFromSelection();
            RefreshToolbar(auth);
        }
        catch (Exception ex)
        {
            Error = "Liste yüklenemedi: " + ex.Message;
        }
    }

    private void EnterListMode()
    {
        _mode = ScreenMode.List;
        FieldsReadOnly = true;
        ShowSaveCancel = false;
        ListEnabled = true;
        FillFromSelection();
        RefreshToolbar();
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
                Error = "Vardiya ekleme yetkiniz yok.";
                UiDialog.Warning(Error, PageName);
                return;
            }

            _mode = ScreenMode.Add;
            Ad = "";
            BaslangicText = "07:00";
            BitisText = "15:00";
            BaslangicToleransText = "00:15";
            BitisToleransText = "00:15";
            YemekAktiflestirmeText = "17:00";
            Error = null;

            FieldsReadOnly = false;
            ListEnabled = false;
            CanAdd = false;
            CanEdit = false;
            CanDelete = false;
            CanOpenYemekSaatleri = false;
            ShowSaveCancel = true;
            CommandManager.InvalidateRequerySuggested();
        }
        catch (Exception ex)
        {
            Error = "Ekleme modu açılamadı: " + ex.Message;
            EnterListMode();
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
                Error = "Vardiya güncelleme yetkiniz yok.";
                UiDialog.Warning(Error, PageName);
                return;
            }

            if (SelectedItem is null)
                return;

            _mode = ScreenMode.Edit;
            FillFromSelection();
            Error = null;

            FieldsReadOnly = false;
            ListEnabled = false;
            CanAdd = false;
            CanEdit = false;
            CanDelete = false;
            CanOpenYemekSaatleri = false;
            ShowSaveCancel = true;
            CommandManager.InvalidateRequerySuggested();
        }
        catch (Exception ex)
        {
            Error = "Düzenleme modu açılamadı: " + ex.Message;
            EnterListMode();
        }
    }

    private void Save()
    {
        Errors.Clear();
        Error = null;
        try
        {
            using var scope = _scopes.CreateScope();
            var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
            var svc = scope.ServiceProvider.GetRequiredService<ICalismaSekliService>();
            var yetkiSvc = scope.ServiceProvider.GetRequiredService<IKullaniciFirmaIsyeriYetkiService>();

            if (!_session.AktifFirmaId.HasValue || !_session.AktifKullaniciId.HasValue)
            {
                Error = "Oturum bilgisi eksik.";
                UiDialog.Warning(Error, PageName);
                return;
            }

            int firmaId = (int)_session.AktifFirmaId.Value;
            bool isAdmin = FirmaIsyeriYetkiHelper.IsAdmin(_session.RolId);
            var yetkiler = yetkiSvc.GetYetkiler((int)_session.AktifKullaniciId.Value);
            if (!FirmaIsyeriYetkiHelper.IsFirmaAuthorized(firmaId, yetkiler, isAdmin))
            {
                Error = "Bu firma için işlem yetkiniz yok.";
                UiDialog.Warning(Error, PageName);
                return;
            }

            if (_mode == ScreenMode.Add && !auth.Can(PageName, YetkiTipleri.Create))
            {
                Error = "Vardiya ekleme yetkiniz yok.";
                return;
            }
            if (_mode == ScreenMode.Edit && !auth.Can(PageName, YetkiTipleri.Update))
            {
                Error = "Vardiya güncelleme yetkiniz yok.";
                return;
            }

            var ad = (Ad ?? "").Trim();
            if (!Errors.Require("Ad", ad, "Vardiya adı zorunludur."))
            {
                Error = Errors.FirstMessage;
                return;
            }

            if (!TryParseTime(BaslangicText, out var bas)
                || !TryParseTime(BitisText, out var bit)
                || !TryParseTime(BaslangicToleransText, out var basTol)
                || !TryParseTime(BitisToleransText, out var bitTol)
                || !TryParseTime(YemekAktiflestirmeText, out var yemek))
            {
                Errors.Set("Baslangic", "Saat alanları HH:mm formatında olmalıdır.");
                Error = Errors.FirstMessage;
                return;
            }

            var x = new CalismaSekli
            {
                FirmaId = firmaId,
                Ad = ad,
                Baslangic = bas,
                Bitis = bit,
                BaslangicTolerans = basTol,
                BitisTolerans = bitTol,
                YemekAktiflestirme = yemek
            };

            bool ok;
            int? keepId = null;
            if (_mode == ScreenMode.Add)
            {
                var newId = svc.Add(x);
                ok = newId > 0;
                if (ok) keepId = newId;
            }
            else if (_mode == ScreenMode.Edit && SelectedItem is not null)
            {
                x.Id = SelectedItem.Id;
                keepId = x.Id;
                ok = svc.Update(x);
            }
            else return;

            if (!ok)
            {
                Error = "İşlem başarısız.";
                UiDialog.Error(Error, PageName);
                return;
            }

            UiDialog.Success("Kayıt tamamlandı.", PageName);
            _mode = ScreenMode.List;
            FieldsReadOnly = true;
            ShowSaveCancel = false;
            ListEnabled = true;

            if (keepId.HasValue)
                LoadListKeepSelection(keepId.Value);
            else
                LoadList();
        }
        catch (Exception ex)
        {
            Error = "Kayıt sırasında hata: " + ex.Message;
            UiDialog.Error(Error, PageName);
        }
    }

    private void LoadListKeepSelection(int id)
    {
        Error = null;
        try
        {
            using var scope = _scopes.CreateScope();
            var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
            var svc = scope.ServiceProvider.GetRequiredService<ICalismaSekliService>();
            List<CalismaSekli> list;
            if (_adminPanelMode)
            {
                list = svc.GetAllForAdmin() ?? new List<CalismaSekli>();
            }
            else
            {
                if (!_session.AktifFirmaId.HasValue) return;
                list = svc.GetAll((int)_session.AktifFirmaId.Value) ?? new List<CalismaSekli>();
            }

            _suppressSelection = true;
            Items.Clear();
            foreach (var it in list)
                Items.Add(it);

            _selected = Items.FirstOrDefault(x => x.Id == id) ?? Items.FirstOrDefault();
            RaisePropertyChanged(nameof(SelectedItem));
            _suppressSelection = false;

            FillFromSelection();
            RefreshToolbar(auth);
            CommandManager.InvalidateRequerySuggested();
        }
        catch (Exception ex)
        {
            Error = "Liste yüklenemedi: " + ex.Message;
        }
    }

    private void DeleteSelected()
    {
        Error = null;
        using var scope = _scopes.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        if (!auth.Can(PageName, YetkiTipleri.Delete))
        {
            Error = "Vardiya silme yetkiniz yok.";
            UiDialog.Warning(Error, PageName);
            return;
        }

        var it = SelectedItem;
        if (it is null) return;
        if (!_adminPanelMode && !_session.AktifFirmaId.HasValue) return;

        if (!UiDialog.Confirm($"“{it.Ad}” silinsin mi?", "Onay", yesText: "Sil", noText: "Vazgeç"))
            return;

        try
        {
            var svc = scope.ServiceProvider.GetRequiredService<ICalismaSekliService>();
            int firmaId = _adminPanelMode
                ? it.FirmaId
                : (int)_session.AktifFirmaId!.Value;
            if (!svc.Delete(it.Id, firmaId))
            {
                Error = "Silme işlemi başarısız. Kayıt başka tablolarca kullanılıyor olabilir.";
                UiDialog.Error(Error, PageName);
                return;
            }

            UiDialog.Success("Kayıt silindi.", PageName);
            LoadList();
            EnterListMode();
        }
        catch (Exception ex)
        {
            Error = "Silme sırasında hata: " + ex.Message;
            UiDialog.Error(Error, PageName);
        }
    }

    private void FillFromSelection()
    {
        var it = SelectedItem;
        if (it is null)
        {
            ClearFields();
            return;
        }

        Ad = it.Ad ?? "";
        BaslangicText = FormatTs(it.Baslangic);
        BitisText = FormatTs(it.Bitis);
        BaslangicToleransText = FormatTs(it.BaslangicTolerans);
        BitisToleransText = FormatTs(it.BitisTolerans);
        YemekAktiflestirmeText = FormatTs(it.YemekAktiflestirme);
    }

    private void ClearFields()
    {
        Ad = "";
        BaslangicText = "07:00";
        BitisText = "15:00";
        BaslangicToleransText = "00:15";
        BitisToleransText = "00:15";
        YemekAktiflestirmeText = "17:00";
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
        bool canSelect = _mode == ScreenMode.List && SelectedItem != null && Items.Count > 0;
        CanAdd = _mode == ScreenMode.List && auth.Can(PageName, YetkiTipleri.Create);
        CanEdit = canSelect && auth.Can(PageName, YetkiTipleri.Update);
        CanDelete = canSelect && auth.Can(PageName, YetkiTipleri.Delete);
        CanOpenYemekSaatleri = canSelect && auth.ViewAbility(PageName);
        CommandManager.InvalidateRequerySuggested();
    }

    private void OpenYemekSaatleri()
    {
        if (SelectedItem is null || _mode == ScreenMode.Add)
        {
            UiDialog.Warning("Önce listeden bir vardiya seçin.", "Yemekhane Saatleri Detayı");
            return;
        }

        int firmaId = _session.AktifFirmaId ?? 0;
        try
        {
            using var scope = _scopes.CreateScope();
            var yemekSvc = scope.ServiceProvider.GetRequiredService<IPersonelVardiyaYemekYetkiService>();
            bool aktif = firmaId > 0 && yemekSvc.FirmaHasSaatPenceresiAktif(firmaId);
            if (!aktif)
            {
                UiDialog.Info(
                    $"Aktif firmada (FirmaId={firmaId}) 'Yemek saat penceresi aktif' işaretli cihaz bulunamadı.\n\n" +
                    "Cihazlar ekranında ilgili cihazı seçip kutuyu işaretleyin, Kaydet'e basın.\n" +
                    "Cihaz başka firmadaysa önce o firmaya geçin.",
                    "Yemekhane Saatleri Detayı");
                return;
            }
        }
        catch (Exception ex)
        {
            UiDialog.Warning($"Kontrol hatası (FirmaId={firmaId}):\n{ex.Message}", "Yemekhane Saatleri Detayı");
            return;
        }

        var owner = System.Windows.Application.Current?.Windows
            .OfType<System.Windows.Window>()
            .FirstOrDefault(w => w.IsActive)
            ?? System.Windows.Application.Current?.MainWindow;

        var win = new Views.YemekSaatleriDetailWindow(SelectedItem.Id, SelectedItem.Ad ?? "")
        {
            Owner = owner
        };
        win.ShowDialog();
    }

    private static string FormatTs(TimeSpan ts) => ts.ToString(@"hh\:mm");

    private static bool TryParseTime(string? text, out TimeSpan ts)
    {
        ts = default;
        if (string.IsNullOrWhiteSpace(text)) return false;
        return TimeSpan.TryParse(text.Trim(), out ts);
    }
}
