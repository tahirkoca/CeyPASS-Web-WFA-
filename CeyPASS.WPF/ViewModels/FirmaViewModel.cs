using System.Collections.ObjectModel;
using System.Windows.Input;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.Extensions.DependencyInjection;

namespace CeyPASS.WPF.ViewModels;

public sealed class FirmaViewModel : ObservableObject
{
    private enum ScreenMode { List, Add, Edit }

    private const string PageName = "Firmalar";

    private readonly IServiceScopeFactory _scopes;
    private ScreenMode _mode = ScreenMode.List;
    private LookupItem? _selected;
    private bool _suppressSelection;
    private string _idText = "";
    private string _ad = "";
    private string _itMail = "";
    private string? _error;
    private bool _fieldsReadOnly = true;
    private bool _idReadOnly = true;
    private bool _showSaveCancel;
    private bool _canAdd;
    private bool _canEdit;
    private bool _canDelete;
    private bool _listEnabled = true;

    public FirmaViewModel(IServiceProvider root)
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

    public string ItMail
    {
        get => _itMail;
        set => SetProperty(ref _itMail, value);
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

    public bool IdReadOnly
    {
        get => _idReadOnly;
        private set => SetProperty(ref _idReadOnly, value);
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
                Error = "Firmalar ekranını görüntüleme yetkiniz yok.";
                Items.Clear();
                ClearFields();
                RefreshToolbar(auth);
                return;
            }

            var svc = scope.ServiceProvider.GetRequiredService<IFirmaService>();
            var list = svc.GetLookup() ?? new List<LookupItem>();
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
        IdReadOnly = true;
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
                Error = "Firma ekleme yetkiniz yok.";
                UiDialog.Warning(Error, PageName);
                return;
            }

            var svc = scope.ServiceProvider.GetRequiredService<IFirmaService>();
            _mode = ScreenMode.Add;
            IdText = svc.SuggestNextId().ToString();
            Ad = "";
            ItMail = "";
            Error = null;

            FieldsReadOnly = false;
            IdReadOnly = false;
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
                Error = "Firma güncelleme yetkiniz yok.";
                UiDialog.Warning(Error, PageName);
                return;
            }

            if (SelectedItem is null)
                return;

            _mode = ScreenMode.Edit;
            FillFromSelection();
            Error = null;

            FieldsReadOnly = false;
            IdReadOnly = true;
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
            var svc = scope.ServiceProvider.GetRequiredService<IFirmaService>();

            if (_mode == ScreenMode.Add && !auth.Can(PageName, YetkiTipleri.Create))
            {
                Error = "Firma ekleme yetkiniz yok.";
                UiDialog.Warning(Error, PageName);
                return;
            }
            if (_mode == ScreenMode.Edit && !auth.Can(PageName, YetkiTipleri.Update))
            {
                Error = "Firma güncelleme yetkiniz yok.";
                UiDialog.Warning(Error, PageName);
                return;
            }

            if (!int.TryParse(IdText, out var id))
            {
                Errors.Set("Id", "Geçerli bir Firma Id girin.");
                Error = Errors.FirstMessage;
                return;
            }

            var ad = (Ad ?? "").Trim();
            var mail = (ItMail ?? "").Trim();
            if (!Errors.Require("Ad", ad, "Firma adı zorunludur."))
            {
                Error = Errors.FirstMessage;
                return;
            }

            bool ok;
            string msg;
            if (_mode == ScreenMode.Add)
                ok = svc.Add(id, ad, mail, out msg);
            else if (_mode == ScreenMode.Edit)
                ok = svc.Update(id, ad, mail, out msg);
            else return;

            if (!ok)
            {
                Error = string.IsNullOrWhiteSpace(msg) ? "İşlem başarısız." : msg;
                UiDialog.Error(Error, PageName);
                return;
            }

            UiDialog.Success("Kayıt tamamlandı.", PageName);
            _mode = ScreenMode.List;
            FieldsReadOnly = true;
            IdReadOnly = true;
            ShowSaveCancel = false;
            ListEnabled = true;
            LoadListKeepSelection(id);
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
            var svc = scope.ServiceProvider.GetRequiredService<IFirmaService>();
            var list = svc.GetLookup() ?? new List<LookupItem>();

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
            Error = "Firma silme yetkiniz yok.";
            UiDialog.Warning(Error, PageName);
            return;
        }

        var it = SelectedItem;
        if (it is null) return;

        if (!UiDialog.Confirm($"“{it.Ad}” firmasını silmek istiyor musunuz?", "Onay", yesText: "Sil", noText: "Vazgeç"))
            return;

        try
        {
            var svc = scope.ServiceProvider.GetRequiredService<IFirmaService>();
            if (!svc.Delete(it.Id))
            {
                Error = "Silme işlemi başarısız. Kayıt ilişkili olabilir.";
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

    private void FillFromSelection(IFirmaService? svc = null)
    {
        var it = SelectedItem;
        if (it is null)
        {
            ClearFields();
            return;
        }

        Firma? full;
        if (svc is null)
        {
            using var scope = _scopes.CreateScope();
            var local = scope.ServiceProvider.GetRequiredService<IFirmaService>();
            full = local.GetAll()?.FirstOrDefault(x => x.FirmaId == it.Id);
        }
        else
        {
            full = svc.GetAll()?.FirstOrDefault(x => x.FirmaId == it.Id);
        }

        if (full is null)
        {
            IdText = it.Id.ToString();
            Ad = it.Ad ?? "";
            ItMail = "";
            return;
        }

        IdText = full.FirmaId.ToString();
        Ad = full.FirmaAdi ?? "";
        ItMail = full.ITBirimMail ?? "";
    }

    private void ClearFields()
    {
        IdText = "";
        Ad = "";
        ItMail = "";
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
