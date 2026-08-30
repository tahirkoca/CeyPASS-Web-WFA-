using System.Collections.ObjectModel;
using System.Windows.Input;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CeyPASS.WPF.ViewModels;

public sealed class YemekPencereRow : ObservableObject
{
    public int Id { get; set; }
    public string Isyeri { get; set; } = "";
    public string Cihaz { get; set; } = "";
    public string Baslangic { get; set; } = "";
    public string Bitis { get; set; } = "";
    public string Aktif { get; set; } = "";
    public int IsyeriId { get; set; }
    public int CihazId { get; set; }
    public TimeSpan YemekBaslangicSaati { get; set; }
    public TimeSpan YemekBitisSaati { get; set; }
    public bool AktifMi { get; set; }
}

public sealed class YemekSaatleriDetailViewModel : ObservableObject
{
    private const string PageName = "Vardiyalar";

    private readonly IServiceScopeFactory _scopes;
    private readonly ISessionContext _session;
    private readonly IAuthorizationService _auth;
    private readonly int _calismaSekliId;
    private readonly string _vardiyaAd;

    private YemekPencereRow? _selected;
    private LookupItem? _selectedIsyeri;
    private CihazListDTO? _selectedCihaz;
    private string _baslangicText = "11:30";
    private string _bitisText = "12:30";
    private bool _aktifMi = true;
    private int? _editingId;
    private string? _error;
    private bool _canAdd;
    private bool _canUpdate;
    private bool _canDelete;
    private bool _editorEnabled;

    public YemekSaatleriDetailViewModel(
        IServiceProvider root,
        int calismaSekliId,
        string vardiyaAd)
    {
        _scopes = root.GetRequiredService<IServiceScopeFactory>();
        _session = root.GetRequiredService<ISessionContext>();
        _auth = root.GetRequiredService<IAuthorizationService>();
        _calismaSekliId = calismaSekliId;
        _vardiyaAd = vardiyaAd ?? "";

        Rows = new ObservableCollection<YemekPencereRow>();
        Isyerleri = new ObservableCollection<LookupItem>();
        Cihazlar = new ObservableCollection<CihazListDTO>();

        AddCommand = new RelayCommand(Add, () => CanAdd && EditorEnabled);
        UpdateCommand = new RelayCommand(Update, () => CanUpdate && EditorEnabled);
        DeleteCommand = new RelayCommand(Delete, () => CanDelete && EditorEnabled);
        ClearCommand = new RelayCommand(ClearEditor, () => EditorEnabled);

        Title = $"Yemekhane Saatleri Detayı — {_vardiyaAd}";
        RefreshAuthFlags();
        LoadLookups();
        LoadRows();
        ClearEditor();
    }

    public string Title { get; }

    public ObservableCollection<YemekPencereRow> Rows { get; }
    public ObservableCollection<LookupItem> Isyerleri { get; }
    public ObservableCollection<CihazListDTO> Cihazlar { get; }

    public YemekPencereRow? SelectedRow
    {
        get => _selected;
        set
        {
            if (Equals(_selected, value)) return;
            SetProperty(ref _selected, value);
            if (value is null) return;

            _editingId = value.Id;
            SelectedIsyeri = Isyerleri.FirstOrDefault(x => x.Id == value.IsyeriId);
            SelectedCihaz = Cihazlar.FirstOrDefault(x => x.CihazId == value.CihazId);
            BaslangicText = value.YemekBaslangicSaati.ToString(@"hh\:mm");
            BitisText = value.YemekBitisSaati.ToString(@"hh\:mm");
            AktifMi = value.AktifMi;
            RefreshAuthFlags();
        }
    }

    public LookupItem? SelectedIsyeri
    {
        get => _selectedIsyeri;
        set => SetProperty(ref _selectedIsyeri, value);
    }

    public CihazListDTO? SelectedCihaz
    {
        get => _selectedCihaz;
        set => SetProperty(ref _selectedCihaz, value);
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

    public bool AktifMi
    {
        get => _aktifMi;
        set => SetProperty(ref _aktifMi, value);
    }

    public string? Error
    {
        get => _error;
        private set => SetProperty(ref _error, value);
    }

    public BindableFieldErrors Errors { get; } = new();

    public bool CanAdd
    {
        get => _canAdd;
        private set { SetProperty(ref _canAdd, value); CommandManager.InvalidateRequerySuggested(); }
    }

    public bool CanUpdate
    {
        get => _canUpdate;
        private set { SetProperty(ref _canUpdate, value); CommandManager.InvalidateRequerySuggested(); }
    }

    public bool CanDelete
    {
        get => _canDelete;
        private set { SetProperty(ref _canDelete, value); CommandManager.InvalidateRequerySuggested(); }
    }

    public bool EditorEnabled
    {
        get => _editorEnabled;
        private set { SetProperty(ref _editorEnabled, value); CommandManager.InvalidateRequerySuggested(); }
    }

    public ICommand AddCommand { get; }
    public ICommand UpdateCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ClearCommand { get; }

    private void RefreshAuthFlags()
    {
        EditorEnabled = true;
        CanAdd = _auth.Can(PageName, YetkiTipleri.Create);
        CanUpdate = _auth.Can(PageName, YetkiTipleri.Update) && _editingId.HasValue;
        CanDelete = _auth.Can(PageName, YetkiTipleri.Delete) && _editingId.HasValue;
    }

    private void LoadLookups()
    {
        try
        {
            using var scope = _scopes.CreateScope();
            var lookup = scope.ServiceProvider.GetRequiredService<IKisiEkraniLookUpService>();
            var yetkiSvc = scope.ServiceProvider.GetRequiredService<IKullaniciFirmaIsyeriYetkiService>();
            var cihazSvc = scope.ServiceProvider.GetRequiredService<ICihazService>();

            int firmaId = _session.AktifFirmaId ?? 0;
            bool isAdmin = FirmaIsyeriYetkiHelper.IsAdmin(_session.RolId);
            var yetkiler = yetkiSvc.GetYetkiler((int)_session.AktifKullaniciId!) ?? new List<FirmaIsyeriYetkiDTO>();

            var isyerleri = FirmaIsyeriYetkiHelper.FilterIsyeriLookup(
                    lookup.GetIsyerleri(firmaId) ?? new List<LookupItem>(),
                    firmaId, yetkiler, isAdmin)
                .Where(x => x.Id > 0)
                .ToList();

            Isyerleri.Clear();
            foreach (var i in isyerleri)
                Isyerleri.Add(i);

            var cihazlar = cihazSvc.GetListe(sadeceAktif: true, firmaId) ?? new List<CihazListDTO>();
            Cihazlar.Clear();
            foreach (var c in cihazlar)
                Cihazlar.Add(c);
        }
        catch (Exception ex)
        {
            Error = "Listeler yüklenemedi: " + ex.Message;
        }
    }

    private void LoadRows()
    {
        try
        {
            using var scope = _scopes.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<IPersonelVardiyaYemekYetkiService>();
            var data = svc.GetByCalismaSekliId(_calismaSekliId) ?? new List<PersonelVardiyaYemekYetki>();

            Rows.Clear();
            foreach (var x in data)
            {
                Rows.Add(new YemekPencereRow
                {
                    Id = x.Id,
                    Isyeri = x.IsyeriAdi ?? x.IsyeriId.ToString(),
                    Cihaz = string.IsNullOrWhiteSpace(x.CihazAdi)
                        ? (x.CihazId > 0 ? x.CihazId.ToString() : "")
                        : x.CihazAdi,
                    Baslangic = x.YemekBaslangicSaati.ToString(@"hh\:mm"),
                    Bitis = x.YemekBitisSaati.ToString(@"hh\:mm"),
                    Aktif = x.AktifMi ? "Evet" : "Hayır",
                    IsyeriId = x.IsyeriId,
                    CihazId = x.CihazId,
                    YemekBaslangicSaati = x.YemekBaslangicSaati,
                    YemekBitisSaati = x.YemekBitisSaati,
                    AktifMi = x.AktifMi
                });
            }

            SelectedRow = null;
            _editingId = null;
            RefreshAuthFlags();
        }
        catch (Exception ex)
        {
            Error = "Kayıtlar yüklenemedi: " + ex.Message;
        }
    }

    private void ClearEditor()
    {
        _editingId = null;
        _selected = null;
        RaisePropertyChanged(nameof(SelectedRow));
        SelectedIsyeri = Isyerleri.FirstOrDefault();
        SelectedCihaz = Cihazlar.FirstOrDefault();
        BaslangicText = "11:30";
        BitisText = "12:30";
        AktifMi = true;
        Errors.Clear();
        Error = null;
        RefreshAuthFlags();
    }

    private PersonelVardiyaYemekYetki? BuildItem()
    {
        Errors.Clear();
        Error = null;
        bool okBas = TimeSpan.TryParse(BaslangicText?.Trim(), out var bas);
        bool okBit = TimeSpan.TryParse(BitisText?.Trim(), out var bit);
        if (!okBas)
            Errors.Set("Baslangic", "Saat alanları HH:mm formatında olmalıdır.");
        if (!okBit)
            Errors.Set("Bitis", "Saat alanları HH:mm formatında olmalıdır.");
        if (Errors.HasErrors)
        {
            Error = Errors.FirstMessage;
            return null;
        }

        int isyeriId = SelectedIsyeri?.Id ?? 0;
        int cihazId = SelectedCihaz?.CihazId ?? 0;

        return new PersonelVardiyaYemekYetki
        {
            Id = _editingId ?? 0,
            CalismaSekliId = _calismaSekliId,
            IsyeriId = isyeriId,
            CihazId = cihazId,
            YemekBaslangicSaati = bas,
            YemekBitisSaati = bit,
            AktifMi = AktifMi
        };
    }

    private void Add()
    {
        if (!_auth.Can(PageName, YetkiTipleri.Create))
        {
            UiDialog.Warning("Ekleme yetkiniz yok.", PageName);
            return;
        }

        var item = BuildItem();
        if (item is null) return;
        item.Id = 0;

        using var scope = _scopes.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IPersonelVardiyaYemekYetkiService>();
        var (ok, error) = svc.Add(item);
        if (!ok)
        {
            UiDialog.Warning(error ?? "İşlem başarısız.", "Uyarı");
            return;
        }

        UiDialog.Success("Yemek saat penceresi eklendi.", "Bilgi");
        LoadRows();
        ClearEditor();
    }

    private void Update()
    {
        if (!_auth.Can(PageName, YetkiTipleri.Update))
        {
            UiDialog.Warning("Güncelleme yetkiniz yok.", PageName);
            return;
        }

        if (!_editingId.HasValue)
        {
            UiDialog.Warning("Güncellemek için listeden bir kayıt seçin.", "Uyarı");
            return;
        }

        var item = BuildItem();
        if (item is null) return;

        using var scope = _scopes.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IPersonelVardiyaYemekYetkiService>();
        var (ok, error) = svc.Update(item);
        if (!ok)
        {
            UiDialog.Warning(error ?? "İşlem başarısız.", "Uyarı");
            return;
        }

        UiDialog.Success("Yemek saat penceresi güncellendi.", "Bilgi");
        LoadRows();
        ClearEditor();
    }

    private void Delete()
    {
        if (!_auth.Can(PageName, YetkiTipleri.Delete))
        {
            UiDialog.Warning("Silme yetkiniz yok.", PageName);
            return;
        }

        if (!_editingId.HasValue)
        {
            UiDialog.Warning("Silmek için listeden bir kayıt seçin.", "Uyarı");
            return;
        }

        if (!UiDialog.Confirm("Seçili yemek saat penceresi silinsin mi?", "Onay", yesText: "Sil", noText: "Vazgeç"))
            return;

        using var scope = _scopes.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IPersonelVardiyaYemekYetkiService>();
        if (!svc.Delete(_editingId.Value))
        {
            UiDialog.Error("Silme işlemi başarısız.", "Hata");
            return;
        }

        UiDialog.Success("Kayıt silindi.", PageName);
        LoadRows();
        ClearEditor();
    }
}
