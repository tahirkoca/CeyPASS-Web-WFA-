using System.Collections.ObjectModel;
using System.Windows.Input;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.Extensions.DependencyInjection;

namespace CeyPASS.WPF.ViewModels;

public sealed class PozisyonViewModel : ObservableObject
{
    private enum ScreenMode { List, Add, Edit }

    private const string PageName = "Pozisyonlar";

    private readonly IServiceScopeFactory _scopes;
    private ScreenMode _mode = ScreenMode.List;
    private LookupItem? _selected;
    private bool _suppressSelection;
    private string _idText = "";
    private string _ad = "";
    private string _aciklama = "";
    private string? _error;
    private bool _fieldsReadOnly = true;
    private bool _showSaveCancel;
    private bool _canAdd;
    private bool _canEdit;
    private bool _canDelete;
    private bool _listEnabled = true;

    public PozisyonViewModel(IServiceProvider root)
    {
        _scopes = root.GetRequiredService<IServiceScopeFactory>();
        Items = new ObservableCollection<LookupItem>();

        AddCommand = new RelayCommand(EnterAddMode, () => CanAdd);
        EditCommand = new RelayCommand(EnterEditMode, () => CanEdit);
        DeleteCommand = new RelayCommand(DeleteSelected, () => CanDelete);
        SaveCommand = new RelayCommand(Save, () => ShowSaveCancel);
        CancelCommand = new RelayCommand(EnterListMode, () => ShowSaveCancel);
        RefreshCommand = new RelayCommand(LoadList);

        LoadList();
    }

    public ObservableCollection<LookupItem> Items { get; }

    public LookupItem? SelectedItem
    {
        get => _selected;
        set
        {
            if (Equals(_selected, value)) return;
            SetProperty(ref _selected, value);
            if (_suppressSelection) return;
            if (_mode == ScreenMode.List)
            {
                FillFromSelection();
                RefreshToolbar();
            }
        }
    }

    public string IdText
    {
        get => _idText;
        set => SetProperty(ref _idText, value);
    }

    public string Ad
    {
        get => _ad;
        set => SetProperty(ref _ad, value);
    }

    public string Aciklama
    {
        get => _aciklama;
        set => SetProperty(ref _aciklama, value);
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

    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand RefreshCommand { get; }

    private void LoadList()
    {
        Error = null;
        try
        {
            using var scope = _scopes.CreateScope();
            var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
            if (!auth.ViewAbility(PageName))
            {
                Error = "Pozisyonlar ekranını görüntüleme yetkiniz yok.";
                Items.Clear();
                ClearFields();
                RefreshToolbar(auth);
                return;
            }

            var svc = scope.ServiceProvider.GetRequiredService<IPozisyonService>();
            var list = svc.GetAll() ?? new List<LookupItem>();
            var keepId = SelectedItem?.Id;

            _suppressSelection = true;
            Items.Clear();
            foreach (var it in list)
                Items.Add(it);

            LookupItem? next = null;
            if (keepId.HasValue)
                next = Items.FirstOrDefault(x => x.Id == keepId.Value);
            next ??= Items.FirstOrDefault();
            _selected = next;
            RaisePropertyChanged(nameof(SelectedItem));
            _suppressSelection = false;

            FillFromSelection(svc);
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
                Error = "Pozisyon ekleme yetkiniz yok.";
                UiDialog.Warning(Error, PageName);
                return;
            }

            _mode = ScreenMode.Add;
            IdText = "";
            Ad = "";
            Aciklama = "";
            Error = null;

            FieldsReadOnly = false;
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
                Error = "Pozisyon güncelleme yetkiniz yok.";
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
            var svc = scope.ServiceProvider.GetRequiredService<IPozisyonService>();

            if (_mode == ScreenMode.Add && !auth.Can(PageName, YetkiTipleri.Create))
            {
                Error = "Pozisyon ekleme yetkiniz yok.";
                return;
            }
            if (_mode == ScreenMode.Edit && !auth.Can(PageName, YetkiTipleri.Update))
            {
                Error = "Pozisyon güncelleme yetkiniz yok.";
                return;
            }

            var ad = (Ad ?? "").Trim();
            var ack = (Aciklama ?? "").Trim();
            if (!Errors.Require("Ad", ad, "Pozisyon adı zorunludur."))
            {
                Error = Errors.FirstMessage;
                return;
            }

            bool ok;
            int? keepId = null;
            if (_mode == ScreenMode.Add)
            {
                ok = svc.Add(ad, ack);
            }
            else if (_mode == ScreenMode.Edit)
            {
                if (!int.TryParse(IdText, out var id))
                {
                    Errors.Set("Id", "Geçersiz ID.");
                    Error = Errors.FirstMessage;
                    return;
                }
                keepId = id;
                ok = svc.Update(id, ad, ack);
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
            var svc = scope.ServiceProvider.GetRequiredService<IPozisyonService>();
            var list = svc.GetAll() ?? new List<LookupItem>();

            _suppressSelection = true;
            Items.Clear();
            foreach (var it in list)
                Items.Add(it);

            _selected = Items.FirstOrDefault(x => x.Id == id) ?? Items.FirstOrDefault();
            RaisePropertyChanged(nameof(SelectedItem));
            _suppressSelection = false;

            FillFromSelection(svc);
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
            Error = "Pozisyon silme yetkiniz yok.";
            UiDialog.Warning(Error, PageName);
            return;
        }

        var it = SelectedItem;
        if (it is null) return;

        if (!UiDialog.Confirm($"“{it.Ad}” silinsin mi?", "Onay", yesText: "Sil", noText: "Vazgeç"))
            return;

        try
        {
            var svc = scope.ServiceProvider.GetRequiredService<IPozisyonService>();
            if (!svc.Delete(it.Id))
            {
                Error = "Silme başarısız. Kayıt başka tablolar tarafından kullanılıyor olabilir.";
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

    private void FillFromSelection(IPozisyonService? svc = null)
    {
        var it = SelectedItem;
        if (it is null)
        {
            ClearFields();
            return;
        }

        IdText = it.Id.ToString();

        (int id, string ad, string ack)? data;
        if (svc is null)
        {
            using var scope = _scopes.CreateScope();
            var local = scope.ServiceProvider.GetRequiredService<IPozisyonService>();
            data = local.GetForEdit(it.Id);
        }
        else
        {
            data = svc.GetForEdit(it.Id);
        }

        Ad = data?.ad ?? it.Ad ?? "";
        Aciklama = data?.ack ?? "";
    }

    private void ClearFields()
    {
        IdText = "";
        Ad = "";
        Aciklama = "";
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
}
