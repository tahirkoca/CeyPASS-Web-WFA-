using System.Collections.ObjectModel;
using System.Data;
using System.Windows.Input;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CeyPASS.WPF.ViewModels;

public sealed class IsyeriViewModel : ObservableObject
{
    private enum ScreenMode { List, Add, Edit }

    private const string PageName = "Isyerler";

    private readonly IServiceScopeFactory _scopes;
    private readonly ISessionContext _session;
    private ScreenMode _mode = ScreenMode.List;
    private IsyeriItem? _selected;
    private bool _suppressSelection;
    private string _firmaIdText = "";
    private string _isyeriIdText = "";
    private string _ad = "";
    private string? _error;
    private bool _fieldsReadOnly = true;
    private bool _firmaIdReadOnly = true;
    private bool _isyeriIdReadOnly = true;
    private bool _showSaveCancel;
    private bool _canAdd;
    private bool _canEdit;
    private bool _canDelete;
    private bool _listEnabled = true;
    private bool _saving;

    public IsyeriViewModel(IServiceProvider root)
    {
        _scopes = root.GetRequiredService<IServiceScopeFactory>();
        _session = root.GetRequiredService<ISessionContext>();
        Items = new ObservableCollection<IsyeriItem>();

        AddCommand = new RelayCommand(EnterAddMode, () => CanAdd);
        EditCommand = new RelayCommand(EnterEditMode, () => CanEdit);
        DeleteCommand = new RelayCommand(DeleteSelected, () => CanDelete);
        SaveCommand = new RelayCommand(Save, () => ShowSaveCancel && !_saving);
        CancelCommand = new RelayCommand(EnterListMode, () => ShowSaveCancel && !_saving);
        RefreshCommand = new RelayCommand(() => LoadList());

        LoadList();
    }

    public ObservableCollection<IsyeriItem> Items { get; }

    public IsyeriItem? SelectedItem
    {
        get => _selected;
        set
        {
            if (ReferenceEquals(_selected, value)) return;
            if (_selected is not null && value is not null
                && _selected.FirmaId == value.FirmaId && _selected.IsyeriId == value.IsyeriId)
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

    public string FirmaIdText
    {
        get => _firmaIdText;
        set => SetProperty(ref _firmaIdText, value);
    }

    public string IsyeriIdText
    {
        get => _isyeriIdText;
        set => SetProperty(ref _isyeriIdText, value);
    }

    public string Ad
    {
        get => _ad;
        set => SetProperty(ref _ad, value);
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

    public bool IsyeriIdReadOnly
    {
        get => _isyeriIdReadOnly;
        private set => SetProperty(ref _isyeriIdReadOnly, value);
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

    private void LoadList(int? keepFirmaId = null, int? keepIsyeriId = null)
    {
        Error = null;
        try
        {
            using var scope = _scopes.CreateScope();
            var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
            if (!auth.ViewAbility(PageName))
            {
                Error = "İşyerleri ekranını görüntüleme yetkiniz yok.";
                Items.Clear();
                ClearFields();
                RefreshToolbar(auth);
                return;
            }

            var svc = scope.ServiceProvider.GetRequiredService<IIsyeriService>();
            var yetkiSvc = scope.ServiceProvider.GetRequiredService<IKullaniciFirmaIsyeriYetkiService>();
            bool isAdmin = FirmaIsyeriYetkiHelper.IsAdmin(_session.RolId);
            var yetkiler = _session.AktifKullaniciId.HasValue
                ? yetkiSvc.GetYetkiler((int)_session.AktifKullaniciId.Value) ?? new List<FirmaIsyeriYetkiDTO>()
                : new List<FirmaIsyeriYetkiDTO>();

            var dt = svc.GetAll() ?? new DataTable();
            var keepF = keepFirmaId ?? SelectedItem?.FirmaId;
            var keepI = keepIsyeriId ?? SelectedItem?.IsyeriId;

            _suppressSelection = true;
            Items.Clear();
            foreach (DataRow r in dt.Rows)
            {
                int fId = ToInt(r["FirmaId"]);
                int iId = ToInt(r["IsyeriId"]);
                string ad = ToStr(r["IsyeriAdi"]);
                if (iId < 0) continue;
                if (!FirmaIsyeriYetkiHelper.IsFirmaAuthorized(fId, yetkiler, isAdmin)) continue;
                if (!FirmaIsyeriYetkiHelper.IsIsyeriAuthorized(fId, iId, yetkiler, isAdmin)) continue;
                Items.Add(new IsyeriItem(fId, iId, ad));
            }

            IsyeriItem? next = null;
            if (keepF.HasValue && keepI.HasValue)
                next = Items.FirstOrDefault(x => x.FirmaId == keepF.Value && x.IsyeriId == keepI.Value);
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
        IsyeriIdReadOnly = true;
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
                Error = "İşyeri ekleme yetkiniz yok.";
                UiDialog.Warning(Error, PageName);
                return;
            }

            var prevFirma = FirmaIdText;
            _mode = ScreenMode.Add;
            FirmaIdText = prevFirma;
            IsyeriIdText = "";
            Ad = "";
            Error = null;

            FieldsReadOnly = false;
            FirmaIdReadOnly = false;
            IsyeriIdReadOnly = false;
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
                Error = "İşyeri güncelleme yetkiniz yok.";
                UiDialog.Warning(Error, PageName);
                return;
            }

            if (SelectedItem is null)
                return;

            _mode = ScreenMode.Edit;
            FillFromSelection();
            Error = null;

            FieldsReadOnly = false;
            FirmaIdReadOnly = true;
            IsyeriIdReadOnly = true;
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
        if (_saving) return;
        Errors.Clear();
        Error = null;

        try
        {
            using var scope = _scopes.CreateScope();
            var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
            var svc = scope.ServiceProvider.GetRequiredService<IIsyeriService>();

            if (_mode == ScreenMode.Add && !auth.Can(PageName, YetkiTipleri.Create))
            {
                Error = "İşyeri ekleme yetkiniz yok.";
                UiDialog.Warning(Error, PageName);
                return;
            }
            if (_mode == ScreenMode.Edit && !auth.Can(PageName, YetkiTipleri.Update))
            {
                Error = "İşyeri güncelleme yetkiniz yok.";
                UiDialog.Warning(Error, PageName);
                return;
            }

            if (!TryReadInputs(out int firmaId, out int isyeriId, out string ad))
            {
                Error = Errors.FirstMessage;
                return;
            }

            _saving = true;
            bool ok;
            if (_mode == ScreenMode.Add)
                ok = svc.AddManual(firmaId, isyeriId, ad);
            else if (_mode == ScreenMode.Edit)
                ok = svc.Update(firmaId, isyeriId, ad);
            else return;

            if (!ok)
            {
                Error = "İşlem tamamlanamadı.";
                UiDialog.Error(Error, PageName);
                return;
            }

            UiDialog.Success("Kayıt tamamlandı.", PageName);
            _mode = ScreenMode.List;
            FieldsReadOnly = true;
            FirmaIdReadOnly = true;
            IsyeriIdReadOnly = true;
            ShowSaveCancel = false;
            ListEnabled = true;
            LoadList(firmaId, isyeriId);
        }
        catch (Exception ex)
        {
            Error = "İşlem başarısız: " + ex.Message;
            UiDialog.Error(Error, PageName);
        }
        finally
        {
            _saving = false;
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private void DeleteSelected()
    {
        Error = null;
        using var scope = _scopes.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        if (!auth.Can(PageName, YetkiTipleri.Delete))
        {
            Error = "İşyeri silme yetkiniz yok.";
            UiDialog.Warning(Error, PageName);
            return;
        }

        var it = SelectedItem;
        if (it is null) return;

        if (!UiDialog.Confirm($"“{it.Ad}” kaydını silmek istiyor musunuz?", "Onay", yesText: "Sil", noText: "Vazgeç"))
            return;

        try
        {
            var svc = scope.ServiceProvider.GetRequiredService<IIsyeriService>();
            if (!svc.Delete(it.FirmaId, it.IsyeriId))
            {
                Error = "Silme işlemi tamamlanamadı. Kayıt başka tablolarca kullanılıyor olabilir.";
                UiDialog.Error(Error, PageName);
                return;
            }

            UiDialog.Success("Kayıt silindi.", PageName);
            LoadList();
            EnterListMode();
        }
        catch (Exception ex)
        {
            Error = "Silme başarısız: " + ex.Message;
            UiDialog.Error(Error, PageName);
        }
    }

    private bool TryReadInputs(out int firmaId, out int isyeriId, out string ad)
    {
        ad = (Ad ?? "").Trim();
        firmaId = 0;
        isyeriId = 0;

        if (!int.TryParse(FirmaIdText, out firmaId) || firmaId <= 0)
        {
            Errors.Set("FirmaId", "Geçerli bir Firma Id giriniz.");
            return false;
        }

        if (!int.TryParse(IsyeriIdText, out isyeriId) || isyeriId <= 0)
        {
            Errors.Set("IsyeriId", "Geçerli bir İşyeri Id giriniz.");
            return false;
        }

        if (!Errors.Require("Ad", ad, "İşyeri adı zorunludur."))
            return false;

        return true;
    }

    private void FillFromSelection()
    {
        var it = SelectedItem;
        if (it is null)
        {
            ClearFields();
            return;
        }

        FirmaIdText = it.FirmaId.ToString();
        IsyeriIdText = it.IsyeriId.ToString();
        Ad = it.Ad ?? "";
    }

    private void ClearFields()
    {
        FirmaIdText = "";
        IsyeriIdText = "";
        Ad = "";
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
        CommandManager.InvalidateRequerySuggested();
    }

    private static int ToInt(object? v, int def = 0)
        => v is null or DBNull ? def : Convert.ToInt32(v);

    private static string ToStr(object? v)
        => v is null or DBNull ? "" : v.ToString() ?? "";
}
