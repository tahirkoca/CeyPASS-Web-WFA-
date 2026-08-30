using System.Collections.ObjectModel;
using System.Windows.Input;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CeyPASS.WPF.ViewModels;

public sealed class CihazViewModel : ObservableObject
{
    private enum ScreenMode { List, Add, Edit }

    private const string PageName = "Cihazlar";

    private readonly IServiceScopeFactory _scopes;
    private readonly ISessionContext _session;
    private readonly bool _adminPanelMode;
    private ScreenMode _mode = ScreenMode.List;
    private CihazListDTO? _selected;
    private bool _suppressSelection;
    private string _cihazIdText = "";
    private string _firmaIdText = "";
    private string _cihazAdi = "";
    private string _ipAdres = "";
    private string _portText = "4370";
    private string _aciklama = "";
    private CihazTip? _selectedTip;
    private bool _saatPenceresiAktifMi;
    private bool _anaGirisCikisMi;
    private bool _aracGirisCikisMi;
    private string? _error;
    private bool _fieldsReadOnly = true;
    private bool _firmaIdReadOnly = true;
    private bool _tipEnabled;
    private bool _saatPenceresiEnabled;
    private bool _showSaveCancel;
    private bool _canAdd;
    private bool _canEdit;
    private bool _canDelete;
    private bool _listEnabled = true;

    public CihazViewModel(IServiceProvider root, bool adminPanelMode = false)
    {
        _scopes = root.GetRequiredService<IServiceScopeFactory>();
        _session = root.GetRequiredService<ISessionContext>();
        _adminPanelMode = adminPanelMode;
        Items = new ObservableCollection<CihazListDTO>();
        Tipler = new ObservableCollection<CihazTip>();

        AddCommand = new RelayCommand(EnterAddMode, () => CanAdd);
        EditCommand = new RelayCommand(EnterEditMode, () => CanEdit);
        DeleteCommand = new RelayCommand(DeleteSelected, () => CanDelete);
        SaveCommand = new RelayCommand(Save, () => ShowSaveCancel);
        CancelCommand = new RelayCommand(EnterListMode, () => ShowSaveCancel);
        RefreshCommand = new RelayCommand(LoadList);

        LoadTypes();
        LoadList();
    }

    public ObservableCollection<CihazListDTO> Items { get; }
    public ObservableCollection<CihazTip> Tipler { get; }

    public CihazListDTO? SelectedItem
    {
        get => _selected;
        set
        {
            if (ReferenceEquals(_selected, value)) return;
            if (_selected is not null && value is not null && _selected.CihazId == value.CihazId)
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

    public string CihazIdText
    {
        get => _cihazIdText;
        private set => SetProperty(ref _cihazIdText, value);
    }

    public string FirmaIdText
    {
        get => _firmaIdText;
        set => SetProperty(ref _firmaIdText, value);
    }

    public string CihazAdi
    {
        get => _cihazAdi;
        set => SetProperty(ref _cihazAdi, value);
    }

    public string IpAdres
    {
        get => _ipAdres;
        set => SetProperty(ref _ipAdres, value);
    }

    public string PortText
    {
        get => _portText;
        set => SetProperty(ref _portText, value);
    }

    public string Aciklama
    {
        get => _aciklama;
        set => SetProperty(ref _aciklama, value);
    }

    public CihazTip? SelectedTip
    {
        get => _selectedTip;
        set => SetProperty(ref _selectedTip, value);
    }

    public bool SaatPenceresiAktifMi
    {
        get => _saatPenceresiAktifMi;
        set => SetProperty(ref _saatPenceresiAktifMi, value);
    }

    public bool AnaGirisCikisMi
    {
        get => _anaGirisCikisMi;
        set => SetProperty(ref _anaGirisCikisMi, value);
    }

    public bool AracGirisCikisMi
    {
        get => _aracGirisCikisMi;
        set => SetProperty(ref _aracGirisCikisMi, value);
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

    public bool FirmaIdReadOnly
    {
        get => _firmaIdReadOnly;
        private set => SetProperty(ref _firmaIdReadOnly, value);
    }

    public bool TipEnabled
    {
        get => _tipEnabled;
        private set => SetProperty(ref _tipEnabled, value);
    }

    public bool SaatPenceresiEnabled
    {
        get => _saatPenceresiEnabled;
        private set => SetProperty(ref _saatPenceresiEnabled, value);
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

    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand RefreshCommand { get; }

    private void LoadTypes()
    {
        try
        {
            using var scope = _scopes.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<ICihazService>();
            var tips = svc.GetCihazTipleri() ?? new List<CihazTip>();
            Tipler.Clear();
            foreach (var t in tips)
                Tipler.Add(t);
            SelectedTip = Tipler.FirstOrDefault();
        }
        catch (Exception ex)
        {
            Error = "Cihaz tipleri yüklenemedi: " + ex.Message;
        }
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
                Error = "Cihazlar ekranı için görüntüleme yetkiniz yok.";
                Items.Clear();
                ClearInputs();
                RefreshToolbar(auth);
                return;
            }

            var svc = scope.ServiceProvider.GetRequiredService<ICihazService>();
            List<CihazListDTO> list;
            if (_adminPanelMode)
            {
                list = svc.GetListe(sadeceAktif: false, firmaId: null) ?? new List<CihazListDTO>();
            }
            else
            {
                if (!_session.AktifFirmaId.HasValue || !_session.AktifKullaniciId.HasValue)
                {
                    Error = "Aktif firma seçili değil.";
                    Items.Clear();
                    ClearInputs();
                    RefreshToolbar(auth);
                    return;
                }

                int firmaId = (int)_session.AktifFirmaId.Value;
                bool isAdmin = FirmaIsyeriYetkiHelper.IsAdmin(_session.RolId);
                var yetkiSvc = scope.ServiceProvider.GetRequiredService<IKullaniciFirmaIsyeriYetkiService>();
                var yetkiler = yetkiSvc.GetYetkiler((int)_session.AktifKullaniciId.Value);
                if (!FirmaIsyeriYetkiHelper.IsFirmaAuthorized(firmaId, yetkiler, isAdmin))
                {
                    Items.Clear();
                    ClearInputs();
                    RefreshToolbar(auth);
                    return;
                }

                list = svc.GetListe(sadeceAktif: true, firmaId: firmaId) ?? new List<CihazListDTO>();
            }
            var keepId = SelectedItem?.CihazId;

            _suppressSelection = true;
            Items.Clear();
            foreach (var it in list)
                Items.Add(it);

            CihazListDTO? next = null;
            if (keepId.HasValue)
                next = Items.FirstOrDefault(x => x.CihazId == keepId.Value);
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
        FirmaIdReadOnly = true;
        TipEnabled = false;
        SaatPenceresiEnabled = false;
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
                Error = "Yeni cihaz ekleme yetkiniz yok.";
                UiDialog.Warning(Error, PageName);
                return;
            }

            _mode = ScreenMode.Add;
            ClearInputs();
            if (_session.AktifFirmaId.HasValue)
                FirmaIdText = ((int)_session.AktifFirmaId.Value).ToString();
            Error = null;

            FieldsReadOnly = false;
            FirmaIdReadOnly = false;
            TipEnabled = true;
            SaatPenceresiEnabled = true;
            ListEnabled = false;
            CanAdd = false;
            CanEdit = false;
            CanDelete = false;
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
                Error = "Güncelleme yetkiniz yok.";
                UiDialog.Warning(Error, PageName);
                return;
            }

            if (SelectedItem is null) return;

            _mode = ScreenMode.Edit;
            FillFromSelection();
            Error = null;

            FieldsReadOnly = false;
            FirmaIdReadOnly = false;
            TipEnabled = true;
            SaatPenceresiEnabled = true;
            ListEnabled = false;
            CanAdd = false;
            CanEdit = false;
            CanDelete = false;
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
            var svc = scope.ServiceProvider.GetRequiredService<ICihazService>();

            string need = _mode == ScreenMode.Edit ? YetkiTipleri.Update : YetkiTipleri.Create;
            if (!auth.Can(PageName, need))
            {
                Error = $"Bu işlem için yetkiniz yok: {need}";
                UiDialog.Warning(Error, PageName);
                return;
            }

            if (!TryCollect(out var c))
            {
                Error = Errors.FirstMessage;
                return;
            }

            bool ok;
            int? keepId = null;
            if (_mode == ScreenMode.Add)
            {
                var newId = svc.Ekle(c);
                ok = newId > 0;
                if (ok)
                {
                    CihazIdText = newId.ToString();
                    keepId = newId;
                }
            }
            else if (_mode == ScreenMode.Edit)
            {
                if (c.CihazId <= 0)
                {
                    Errors.Set("CihazId", "Geçersiz CihazId.");
                    Error = Errors.FirstMessage;
                    return;
                }
                svc.Guncelle(c);
                ok = true;
                keepId = c.CihazId;
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
            FirmaIdReadOnly = true;
            TipEnabled = false;
            SaatPenceresiEnabled = false;
            ShowSaveCancel = false;
            ListEnabled = true;

            if (keepId.HasValue)
                LoadListKeepSelection(keepId.Value);
            else
                LoadList();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
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
            var svc = scope.ServiceProvider.GetRequiredService<ICihazService>();
            List<CihazListDTO> list;
            if (_adminPanelMode)
            {
                list = svc.GetListe(sadeceAktif: false, firmaId: null) ?? new List<CihazListDTO>();
            }
            else
            {
                if (!_session.AktifFirmaId.HasValue) return;
                list = svc.GetListe(sadeceAktif: true, firmaId: (int)_session.AktifFirmaId.Value)
                       ?? new List<CihazListDTO>();
            }

            _suppressSelection = true;
            Items.Clear();
            foreach (var it in list)
                Items.Add(it);

            _selected = Items.FirstOrDefault(x => x.CihazId == id) ?? Items.FirstOrDefault();
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
        if (!auth.DeleteAbility(PageName))
        {
            Error = "Silme (pasife çekme) yetkiniz yok.";
            UiDialog.Warning(Error, PageName);
            return;
        }

        var it = SelectedItem;
        if (it is null) return;

        if (!UiDialog.Confirm($"“{it.CihazAdi}” pasife çekilsin mi?", "Onay", yesText: "Pasife al", noText: "Vazgeç"))
            return;

        try
        {
            var svc = scope.ServiceProvider.GetRequiredService<ICihazService>();
            var cihazId = it.CihazId;
            svc.PasifYap(cihazId);
            UiDialog.SuccessWithUndo("Cihaz pasife çekildi.", () =>
            {
                using var s2 = _scopes.CreateScope();
                s2.ServiceProvider.GetRequiredService<ICihazService>().AktifYap(cihazId);
                LoadList();
                EnterListMode();
                UiDialog.Success("Geri alındı.", PageName);
            }, PageName);
            LoadList();
            EnterListMode();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            UiDialog.Error(Error, PageName);
        }
    }

    private bool TryCollect(out Cihaz c)
    {
        c = new Cihaz();

        if (!Errors.Require("CihazAdi", CihazAdi, "Cihaz Adı zorunludur."))
            return false;
        if (!Errors.Require("IpAdres", IpAdres, "IP Adres zorunludur."))
            return false;
        if (!Errors.Require("SelectedTip", SelectedTip, "Cihaz tipi seçiniz."))
            return false;

        int.TryParse(PortText, out var port);
        if (port <= 0) port = 4370;
        int.TryParse(FirmaIdText, out var firmaId);

        c = new Cihaz
        {
            CihazId = int.TryParse(CihazIdText, out var id) ? id : 0,
            FirmaId = firmaId,
            CihazAdi = CihazAdi.Trim(),
            IPAdres = IpAdres.Trim(),
            Port = port,
            Notlar = string.IsNullOrWhiteSpace(Aciklama) ? null! : Aciklama.Trim(),
            CihazTipi = SelectedTip!.TipId,
            AktifMi = true,
            SaatPenceresiAktifMi = SaatPenceresiAktifMi,
            AnaGirisCikisMi = AnaGirisCikisMi,
            AracGirisCikisMi = AracGirisCikisMi
        };
        return true;
    }

    private void FillFromSelection()
    {
        var it = SelectedItem;
        if (it is null)
        {
            ClearInputs();
            return;
        }

        try
        {
            using var scope = _scopes.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<ICihazService>();
            var c = svc.Get(it.CihazId);
            if (c is null)
            {
                ClearInputs();
                return;
            }

            CihazIdText = c.CihazId.ToString();
            FirmaIdText = c.FirmaId.ToString();
            CihazAdi = c.CihazAdi ?? "";
            IpAdres = c.IPAdres ?? "";
            PortText = (c.Port > 0 ? c.Port : 4370).ToString();
            Aciklama = c.Notlar ?? "";
            SelectedTip = Tipler.FirstOrDefault(t => t.TipId == c.CihazTipi) ?? Tipler.FirstOrDefault();
            SaatPenceresiAktifMi = c.SaatPenceresiAktifMi;
            AnaGirisCikisMi = c.AnaGirisCikisMi;
            AracGirisCikisMi = c.AracGirisCikisMi;
        }
        catch (Exception ex)
        {
            Error = "Detay yüklenemedi: " + ex.Message;
        }
    }

    private void ClearInputs()
    {
        CihazIdText = "";
        FirmaIdText = "";
        CihazAdi = "";
        IpAdres = "";
        PortText = "4370";
        Aciklama = "";
        SelectedTip = Tipler.FirstOrDefault();
        SaatPenceresiAktifMi = false;
        AnaGirisCikisMi = false;
        AracGirisCikisMi = false;
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
        CanDelete = canSelect && auth.DeleteAbility(PageName);
        CommandManager.InvalidateRequerySuggested();
    }
}
