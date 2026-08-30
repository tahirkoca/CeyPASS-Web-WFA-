using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using CeyPASS.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;

namespace CeyPASS.WPF.ViewModels;

public sealed class AySecimItem
{
    public int Yil { get; init; }
    public int Ay { get; init; }
    public string Display { get; init; } = "";
}

public sealed class PuantajGunRowItem : ObservableObject
{
    private static readonly CultureInfo Tr = new("tr-TR");
    private readonly PuantajGunSatirDTO _dto;
    private bool _isLocked;

    public PuantajGunRowItem(PuantajGunSatirDTO dto, bool isLocked)
    {
        _dto = dto ?? throw new ArgumentNullException(nameof(dto));
        _isLocked = isLocked;
    }

    public PuantajGunSatirDTO Dto => _dto;

    public DateTime Tarih => _dto.Tarih;
    public string TarihText => _dto.Tarih.ToString("d MMM yyyy dddd", Tr);
    public string? VardiyaTuru => _dto.VardiyaTuru;
    public string IlkGirisText => FormatTs(_dto.IlkGiris);
    public string SonCikisText => FormatTs(_dto.SonCikis);
    public string VardiyaBaslangicText => FormatTs(_dto.VardiyaBaslangic);
    public string VardiyaBitisText => FormatTs(_dto.VardiyaBitis);
    public int SaatlikIzinDakika => _dto.SaatlikIzinDakika;
    public int ErkenGirisDakika => _dto.ErkenGirisDakika;
    public int GecCikisDakika => _dto.GecCikisDakika;
    public int SistemFMDakika => _dto.SistemFMDakika;

    public OnayDurumu OnayDurumu
    {
        get => _dto.OnayDurumu;
        set
        {
            if (_dto.OnayDurumu == value) return;
            _dto.OnayDurumu = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(OnayDurumuText));
        }
    }

    public string OnayDurumuText => _dto.OnayDurumu.ToString();

    public int DuzenlenenFMDakika
    {
        get => _dto.DuzenlenenFMDakika;
        set
        {
            if (_dto.DuzenlenenFMDakika == value) return;
            _dto.DuzenlenenFMDakika = value;
            RaisePropertyChanged();
        }
    }

    public string? Aciklama
    {
        get => _dto.Aciklama;
        set
        {
            if (_dto.Aciklama == value) return;
            _dto.Aciklama = value;
            RaisePropertyChanged();
        }
    }

    public string? CalismaTipi
    {
        get => _dto.CalismaTipi;
        set
        {
            if (_dto.CalismaTipi == value) return;
            _dto.CalismaTipi = value;
            RaisePropertyChanged();
        }
    }

    public decimal Saat
    {
        get => _dto.Saat;
        set
        {
            if (_dto.Saat == value) return;
            _dto.Saat = value;
            RaisePropertyChanged();
        }
    }

    public bool IsLocked
    {
        get => _isLocked;
        set => SetProperty(ref _isLocked, value);
    }

    public void RefreshFromDto()
    {
        RaisePropertyChanged(nameof(DuzenlenenFMDakika));
        RaisePropertyChanged(nameof(Aciklama));
        RaisePropertyChanged(nameof(CalismaTipi));
        RaisePropertyChanged(nameof(Saat));
        RaisePropertyChanged(nameof(OnayDurumu));
        RaisePropertyChanged(nameof(OnayDurumuText));
    }

    private static string FormatTs(TimeSpan? ts) =>
        ts.HasValue ? ts.Value.ToString(@"hh\:mm") : "";
}

public sealed class AylikPuantajViewModel : ObservableObject
{
    private const string PageName = "AylikPuantaj";
    private static readonly CultureInfo Tr = new("tr-TR");

    private readonly IServiceScopeFactory _scopes;
    private readonly ISessionContext _session;
    private List<FirmaIsyeriYetkiDTO> _yetkiler = new();
    private int _ekKayitGun;
    private bool _suppressCascade;
    private bool _viewAllowed;

    private AySecimItem? _selectedAy;
    private Firma? _selectedFirma;
    private IsyeriItem? _selectedIsyeri;
    private Kisi? _selectedPersonel;
    private PuantajGunRowItem? _selectedRow;
    private string _ekKayitGunText = "0";
    private string? _status;
    private bool _busy;
    private bool _canApprove;
    private bool _canDelete;
    private bool _canUpdate;
    private bool _canExport;
    private bool _canEditEkKayit;

    public AylikPuantajViewModel(IServiceProvider root)
    {
        _scopes = root.GetRequiredService<IServiceScopeFactory>();
        _session = root.GetRequiredService<ISessionContext>();

        Aylar = new ObservableCollection<AySecimItem>();
        Firmalar = new ObservableCollection<Firma>();
        Isyerleri = new ObservableCollection<IsyeriItem>();
        Personeller = new ObservableCollection<Kisi>();
        Rows = new ObservableCollection<PuantajGunRowItem>();

        OnayCommand = new RelayCommand(OnaylaSelected, () => CanApprove && SelectedRow is { IsLocked: false } && !Busy);
        RetCommand = new RelayCommand(ReddetSelected, () => CanDelete && SelectedRow is { IsLocked: false } && !Busy);
        DuzenleCommand = new RelayCommand(DuzenleSelected, () => CanUpdate && SelectedRow is { IsLocked: false } && !Busy);
        BuguneKadarOnaylaCommand = new RelayCommand(BuguneKadarOnayla, () => CanApprove && !Busy);
        CokluSicileAktarCommand = new RelayCommand(CokluSicileAktar, () => !Busy);
        PuantajYapCommand = new RelayCommand(PuantajYap, () => CanExport && !Busy);
        EkKayitAyarlaCommand = new RelayCommand(EkKayitAyarla, () => CanEditEkKayit && !Busy);

        Init();
    }

    public ObservableCollection<AySecimItem> Aylar { get; }
    public ObservableCollection<Firma> Firmalar { get; }
    public ObservableCollection<IsyeriItem> Isyerleri { get; }
    public ObservableCollection<Kisi> Personeller { get; }
    public ObservableCollection<PuantajGunRowItem> Rows { get; }

    public BindableFieldErrors Errors { get; } = new();

    public AySecimItem? SelectedAy
    {
        get => _selectedAy;
        set
        {
            if (ReferenceEquals(_selectedAy, value)) return;
            SetProperty(ref _selectedAy, value);
            if (_suppressCascade || value is null) return;
            PersistFilters();
            ReloadFirmalarCascade();
        }
    }

    public Firma? SelectedFirma
    {
        get => _selectedFirma;
        set
        {
            if (ReferenceEquals(_selectedFirma, value)) return;
            SetProperty(ref _selectedFirma, value);
            if (_suppressCascade) return;
            PersistFilters();
            ReloadIsyerileriCascade();
        }
    }

    public IsyeriItem? SelectedIsyeri
    {
        get => _selectedIsyeri;
        set
        {
            if (ReferenceEquals(_selectedIsyeri, value)) return;
            SetProperty(ref _selectedIsyeri, value);
            if (_suppressCascade) return;
            PersistFilters();
            ReloadPersonellerCascade();
        }
    }

    public Kisi? SelectedPersonel
    {
        get => _selectedPersonel;
        set
        {
            if (ReferenceEquals(_selectedPersonel, value)) return;
            SetProperty(ref _selectedPersonel, value);
            if (_suppressCascade) return;
            LoadGrid();
        }
    }

    public PuantajGunRowItem? SelectedRow
    {
        get => _selectedRow;
        set
        {
            SetProperty(ref _selectedRow, value);
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string EkKayitGunText
    {
        get => _ekKayitGunText;
        set => SetProperty(ref _ekKayitGunText, value);
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

    public bool CanApprove
    {
        get => _canApprove;
        private set => SetProperty(ref _canApprove, value);
    }

    public bool CanDelete
    {
        get => _canDelete;
        private set => SetProperty(ref _canDelete, value);
    }

    public bool CanUpdate
    {
        get => _canUpdate;
        private set => SetProperty(ref _canUpdate, value);
    }

    public bool CanExport
    {
        get => _canExport;
        private set => SetProperty(ref _canExport, value);
    }

    public bool CanEditEkKayit
    {
        get => _canEditEkKayit;
        private set => SetProperty(ref _canEditEkKayit, value);
    }

    public ICommand OnayCommand { get; }
    public ICommand RetCommand { get; }
    public ICommand DuzenleCommand { get; }
    public ICommand BuguneKadarOnaylaCommand { get; }
    public ICommand CokluSicileAktarCommand { get; }
    public ICommand PuantajYapCommand { get; }
    public ICommand EkKayitAyarlaCommand { get; }

    private int SeciliYil => SelectedAy?.Yil ?? 0;
    private int SeciliAyNum => SelectedAy?.Ay ?? 0;

    private int SeciliPersonelId
    {
        get
        {
            if (SelectedPersonel?.PersonelId is null) return 0;
            return int.TryParse(SelectedPersonel.PersonelId, out var pid) ? pid : 0;
        }
    }

    private void Init()
    {
        using var scope = _scopes.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        if (!auth.ViewAbility(PageName))
        {
            _viewAllowed = false;
            Status = "Aylık Puantaj ekranını görüntüleme yetkiniz yok.";
            UiDialog.Warning(Status, PageName);
            return;
        }

        _viewAllowed = true;
        RefreshAuth(auth);
        CanEditEkKayit = _session.RolId == 1 || _session.RolId == 2;

        var yetkiSvc = scope.ServiceProvider.GetRequiredService<IKullaniciFirmaIsyeriYetkiService>();
        _yetkiler = _session.AktifKullaniciId.HasValue
            ? yetkiSvc.GetYetkiler((int)_session.AktifKullaniciId.Value) ?? new List<FirmaIsyeriYetkiDTO>()
            : new List<FirmaIsyeriYetkiDTO>();

        var psvc = scope.ServiceProvider.GetRequiredService<IPuantajService>();
        _ekKayitGun = psvc.GetEkKayitGun();
        EkKayitGunText = _ekKayitGun.ToString();

        LoadAylar();
        ReloadFirmalarCascade();
        Status = "Hazır.";
    }

    private void RefreshAuth(IAuthorizationService auth)
    {
        CanApprove = auth.Can(PageName, YetkiTipleri.Approve);
        CanDelete = auth.Can(PageName, YetkiTipleri.Delete);
        CanUpdate = auth.Can(PageName, YetkiTipleri.Update);
        CanExport = auth.Can(PageName, YetkiTipleri.Export);
        CommandManager.InvalidateRequerySuggested();
    }

    private void PersistFilters()
    {
        PageFilterPrefsStore.Save(PageName, new PageFilterPrefs
        {
            FirmaId = SelectedFirma?.FirmaId,
            IsyeriId = SelectedIsyeri?.IsyeriId,
            DateA = SelectedAy is null ? null : new DateTime(SelectedAy.Yil, SelectedAy.Ay, 1)
        });
    }

    private void LoadAylar()
    {
        Aylar.Clear();
        var baslangic = new DateTime(2025, 1, 1);
        var bugun = DateTime.Now;
        AySecimItem? current = null;

        while (baslangic <= bugun)
        {
            var item = new AySecimItem
            {
                Yil = baslangic.Year,
                Ay = baslangic.Month,
                Display = baslangic.ToString("MMMM yyyy", Tr)
            };
            Aylar.Add(item);
            if (baslangic.Year == bugun.Year && baslangic.Month == bugun.Month)
                current = item;
            baslangic = baslangic.AddMonths(1);
        }

        var prefs = PageFilterPrefsStore.Load(PageName);
        if (prefs?.DateA is DateTime da)
        {
            var preferred = Aylar.FirstOrDefault(a => a.Yil == da.Year && a.Ay == da.Month);
            if (preferred != null)
                current = preferred;
        }

        _suppressCascade = true;
        try
        {
            SelectedAy = current ?? Aylar.LastOrDefault();
        }
        finally
        {
            _suppressCascade = false;
        }
    }

    private void ReloadFirmalarCascade()
    {
        if (!_viewAllowed) return;

        _suppressCascade = true;
        try
        {
            LoadFirmalarCore();
            LoadIsyerileriCore();
            LoadPersonellerCore();
        }
        finally
        {
            _suppressCascade = false;
        }

        LoadGrid();
    }

    private void ReloadIsyerileriCascade()
    {
        if (!_viewAllowed) return;

        _suppressCascade = true;
        try
        {
            LoadIsyerileriCore();
            LoadPersonellerCore();
        }
        finally
        {
            _suppressCascade = false;
        }

        LoadGrid();
    }

    private void ReloadPersonellerCascade()
    {
        if (!_viewAllowed) return;

        _suppressCascade = true;
        try
        {
            LoadPersonellerCore();
        }
        finally
        {
            _suppressCascade = false;
        }

        LoadGrid();
    }

    private void LoadFirmalarCore()
    {
        using var scope = _scopes.CreateScope();
        var fsvc = scope.ServiceProvider.GetRequiredService<IFirmaService>();
        bool isAdmin = FirmaIsyeriYetkiHelper.IsAdmin(_session.RolId);
        var list = FirmaIsyeriYetkiHelper.FilterFirmalar(fsvc.GetPuantajFirmalar(), _yetkiler, isAdmin);

        Firmalar.Clear();
        foreach (var f in list)
            Firmalar.Add(f);

        var prefs = PageFilterPrefsStore.Load(PageName);
        var preferredId = prefs?.FirmaId;
        SelectedFirma = (preferredId.HasValue
                            ? Firmalar.FirstOrDefault(f => f.FirmaId == preferredId.Value)
                            : null)
                        ?? Firmalar.FirstOrDefault(f => f.FirmaId == _session.AktifFirmaId)
                        ?? Firmalar.FirstOrDefault();
    }

    private void LoadIsyerileriCore()
    {
        Isyerleri.Clear();
        SelectedIsyeri = null;

        if (SelectedFirma is null) return;

        using var scope = _scopes.CreateScope();
        var isvc = scope.ServiceProvider.GetRequiredService<IIsyeriService>();
        bool isAdmin = FirmaIsyeriYetkiHelper.IsAdmin(_session.RolId);
        var list = FirmaIsyeriYetkiHelper.FilterIsyeriler(
            isvc.GetIsyerleriByFirma(SelectedFirma.FirmaId),
            SelectedFirma.FirmaId,
            _yetkiler,
            isAdmin);

        foreach (var i in list)
            Isyerleri.Add(i);

        var prefs = PageFilterPrefsStore.Load(PageName);
        var preferredIsyeri = prefs?.IsyeriId;
        SelectedIsyeri = (preferredIsyeri.HasValue
                ? Isyerleri.FirstOrDefault(x => x.IsyeriId == preferredIsyeri.Value)
                : null)
            ?? Isyerleri.FirstOrDefault();
    }

    private void LoadPersonellerCore()
    {
        Personeller.Clear();
        SelectedPersonel = null;

        if (SelectedFirma is null || SelectedIsyeri is null || SeciliYil <= 0 || SeciliAyNum <= 0)
            return;

        using var scope = _scopes.CreateScope();
        var ksvc = scope.ServiceProvider.GetRequiredService<IKisiService>();
        var kisiler = ksvc.GetKisilerForPuantaj(
            SelectedFirma.FirmaId,
            SelectedIsyeri.IsyeriId,
            SeciliYil,
            SeciliAyNum) ?? new List<Kisi>();

        foreach (var k in kisiler)
            Personeller.Add(k);

        SelectedPersonel = Personeller.FirstOrDefault();
    }

    private void LoadGrid()
    {
        Rows.Clear();
        SelectedRow = null;

        if (!_viewAllowed) return;

        var pidStr = SelectedPersonel?.PersonelId;
        if (string.IsNullOrEmpty(pidStr))
        {
            Status = "Personel seçiniz.";
            return;
        }

        if (!int.TryParse(pidStr, out var personelId))
        {
            UiDialog.Warning("Seçili personelin ID’si sayısal değil.", PageName);
            return;
        }

        if (SeciliYil <= 0 || SeciliAyNum <= 0) return;

        try
        {
            Busy = true;
            using var scope = _scopes.CreateScope();
            var psvc = scope.ServiceProvider.GetRequiredService<IPuantajService>();
            var liste = psvc.GetAy(personelId, SeciliYil, SeciliAyNum) ?? new List<PuantajGunSatirDTO>();

            foreach (var dto in liste)
            {
                bool locked = IsLockedRow(dto) || !psvc.IsRowEditable(dto.Tarih, _ekKayitGun);
                Rows.Add(new PuantajGunRowItem(dto, locked));
            }

            Status = $"{Rows.Count} gün yüklendi.";
        }
        catch (Exception ex)
        {
            Status = "Yükleme hatası.";
            UiDialog.Error(ex.Message, PageName);
        }
        finally
        {
            Busy = false;
        }
    }

    private static bool IsMultiLockNote(string? aciklama) =>
        !string.IsNullOrWhiteSpace(aciklama)
        && aciklama.StartsWith("Çoklu Sicil Aktarım", StringComparison.OrdinalIgnoreCase);

    private static bool IsLockedRow(PuantajGunSatirDTO dto)
    {
        if (dto.Tarih.Date >= DateTime.Today) return true;
        if (dto.OnayDurumu == OnayDurumu.Düzeltildi && IsMultiLockNote(dto.Aciklama))
            return true;
        return false;
    }

    private bool EnsureAuth(string yetki)
    {
        using var scope = _scopes.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        if (auth.Can(PageName, yetki)) return true;
        UiDialog.Warning("Bu işlem için yetkiniz yok.", PageName);
        return false;
    }

    private void OnaylaSelected()
    {
        if (SelectedRow is null || SelectedRow.IsLocked) return;
        if (!EnsureAuth(YetkiTipleri.Approve)) return;

        var pid = SeciliPersonelId;
        if (pid <= 0) return;

        var satir = SelectedRow.Dto;
        if (string.Equals(satir.CalismaTipi, "EKSİK VERİ", StringComparison.OrdinalIgnoreCase))
        {
            if (!UiDialog.Confirm("Bu satır 'EKSİK VERİ'. Yine de onaylansın mı?", "Onay", yesText: "Onayla", noText: "Vazgeç"))
                return;
        }

        int fmToSave = satir.DuzenlenenFMDakika > 0 ? satir.DuzenlenenFMDakika : satir.SistemFMDakika;

        try
        {
            using var scope = _scopes.CreateScope();
            var psvc = scope.ServiceProvider.GetRequiredService<IPuantajService>();
            psvc.Onayla(pid, satir.Tarih, satir.DuzenlenenFMDakika, satir.Aciklama, satir.CalismaTipi, satir.Saat,
                (int)_session.AktifKullaniciId!);

            SelectedRow.DuzenlenenFMDakika = fmToSave;
            SelectedRow.OnayDurumu = OnayDurumu.Onaylandı;

            try
            {
                if (string.Equals(satir.CalismaTipi, "FM1", StringComparison.OrdinalIgnoreCase))
                    SelectedRow.Saat = psvc.HesaplaFM1CalismaSaati(fmToSave);
            }
            catch
            {
                // WFA ile aynı: FM1 hesap hatası yutulur
            }

            SelectedRow.IsLocked = IsLockedRow(satir) || !psvc.IsRowEditable(satir.Tarih, _ekKayitGun);
            SelectedRow.RefreshFromDto();
            Status = "Satır onaylandı.";
            UiDialog.Success("Satır onaylandı.", PageName);
            CommandManager.InvalidateRequerySuggested();
        }
        catch (Exception ex)
        {
            UiDialog.Error("Onay sırasında hata:\n" + ex.Message, PageName);
        }
    }

    private void ReddetSelected()
    {
        if (SelectedRow is null || SelectedRow.IsLocked) return;
        if (!EnsureAuth(YetkiTipleri.Delete)) return;

        var pid = SeciliPersonelId;
        if (pid <= 0) return;

        if (!PuantajReddetDialog.Show(SelectedRow.Dto, out var sebep))
            return;

        try
        {
            using var scope = _scopes.CreateScope();
            var psvc = scope.ServiceProvider.GetRequiredService<IPuantajService>();
            psvc.Reddet(pid, SelectedRow.Dto.Tarih, sebep, (int)_session.AktifKullaniciId!);
            SelectedRow.OnayDurumu = OnayDurumu.Reddedildi;
            SelectedRow.Aciklama = sebep;
            SelectedRow.IsLocked = IsLockedRow(SelectedRow.Dto) || !psvc.IsRowEditable(SelectedRow.Dto.Tarih, _ekKayitGun);
            SelectedRow.RefreshFromDto();
            Status = "Satır reddedildi.";
            UiDialog.Success("Satır reddedildi.", PageName);
            CommandManager.InvalidateRequerySuggested();
        }
        catch (Exception ex)
        {
            UiDialog.Error("Ret sırasında hata:\n" + ex.Message, PageName);
        }
    }

    private void DuzenleSelected()
    {
        if (SelectedRow is null || SelectedRow.IsLocked) return;
        if (!EnsureAuth(YetkiTipleri.Update)) return;

        var pid = SeciliPersonelId;
        if (pid <= 0) return;

        using var scope = _scopes.CreateScope();
        var psvc = scope.ServiceProvider.GetRequiredService<IPuantajService>();

        if (!PuantajSatirDuzenleDialog.Show(SelectedRow.Dto, pid, psvc, _session))
            return;

        SelectedRow.OnayDurumu = OnayDurumu.Düzeltildi;
        SelectedRow.IsLocked = IsLockedRow(SelectedRow.Dto) || !psvc.IsRowEditable(SelectedRow.Dto.Tarih, _ekKayitGun);
        SelectedRow.RefreshFromDto();
        Status = "Satır güncellendi.";
        UiDialog.Success("Satır güncellendi.", PageName);
        CommandManager.InvalidateRequerySuggested();
    }

    private void BuguneKadarOnayla()
    {
        if (!EnsureAuth(YetkiTipleri.Approve)) return;

        if (SeciliPersonelId <= 0)
        {
            UiDialog.Warning("Lütfen personel seçiniz.", "Uyarı");
            return;
        }

        if (SeciliYil <= 0 || SeciliAyNum <= 0) return;

        var today = DateTime.Today;
        bool isSameMonth = SeciliYil == today.Year && SeciliAyNum == today.Month;
        bool isFutureMonth = SeciliYil > today.Year || (SeciliYil == today.Year && SeciliAyNum > today.Month);

        if (isFutureMonth)
        {
            UiDialog.Info("Seçili ay henüz gelmedi. Onaylama yapılamaz.", "Bilgi");
            return;
        }

        var ayBas = new DateTime(SeciliYil, SeciliAyNum, 1);
        var aySon = new DateTime(SeciliYil, SeciliAyNum, DateTime.DaysInMonth(SeciliYil, SeciliAyNum));
        var hedefGun = isSameMonth ? today.AddDays(-1) : aySon;

        if (hedefGun < ayBas)
        {
            UiDialog.Info("Onaylanacak geçmiş gün yok.", "Bilgi");
            return;
        }

        try
        {
            Busy = true;
            using var scope = _scopes.CreateScope();
            var psvc = scope.ServiceProvider.GetRequiredService<IPuantajService>();
            var gunler = psvc.GetAy(SeciliPersonelId, SeciliYil, SeciliAyNum);

            var eligibleRows = gunler.Where(g =>
                    g.Tarih.Date <= hedefGun.Date &&
                    g.OnayDurumu == OnayDurumu.Bekliyor &&
                    psvc.IsRowEditable(g.Tarih, _ekKayitGun))
                .ToList();

            if (eligibleRows.Count <= 0)
            {
                UiDialog.Info("Onaylanacak bekleyen puantaj kaydı yok.", "Bilgi");
                return;
            }

            int startDay = eligibleRows.Min(g => g.Tarih.Day);
            string ayAdi = Tr.DateTimeFormat.GetMonthName(SeciliAyNum);
            string confirmMsg = isSameMonth
                ? $"{SeciliYil} {ayAdi} ayının {startDay}-{hedefGun:dd} günleri arası bekleyen puantaj kayıtları toplu onaylansın mı?"
                : $"{SeciliYil} {ayAdi} ayının {startDay}-{hedefGun:dd} günleri arası (bekleyen ve düzenlenebilir) puantaj kayıtları toplu onaylansın mı?";

            if (!UiDialog.Confirm(confirmMsg, "Onay", yesText: "Onayla", noText: "Vazgeç"))
                return;

            psvc.TopluOnaylaKadar(SeciliPersonelId, SeciliYil, SeciliAyNum, hedefGun, (int)_session.AktifKullaniciId!);
            LoadGrid();
            Status = "Toplu onay tamamlandı.";
            UiDialog.Success("Toplu onay tamamlandı.", PageName);
        }
        catch (Exception ex)
        {
            UiDialog.Error("Toplu onay sırasında hata:\n" + ex.Message, "Hata");
        }
        finally
        {
            Busy = false;
        }
    }

    private void CokluSicileAktar()
    {
        if (SeciliPersonelId <= 0)
        {
            UiDialog.Warning("Lütfen kişi seçiniz.", "Uyarı");
            return;
        }

        int yil = SeciliYil;
        int ay = SeciliAyNum;
        if (!UiDialog.Confirm(
                $"Seçili kişinin bağlı tüm sicillerine {yil}-{ay:D2} ayının SON GÜNÜNE 'NG 7,5' yazılacak.\n" +
                "Ayrıca ana personelin ayın SON GÜNÜNDEKİ kayıtları kaldırılacaktır. Onaylıyor musunuz?",
                "Onay",
                yesText: "Aktar",
                noText: "Vazgeç"))
            return;

        try
        {
            Busy = true;
            using var scope = _scopes.CreateScope();
            var psvc = scope.ServiceProvider.GetRequiredService<IPuantajService>();
            psvc.CokluSicileAktar(SeciliPersonelId, yil, ay, _session.AktifKullaniciId);
            UiDialog.Success("Aktarım tamamlandı.", "Bilgi");
            LoadGrid();
        }
        catch (Exception ex)
        {
            UiDialog.Error("Aktarım sırasında hata oluştu:\n" + ex.Message, "Hata");
        }
        finally
        {
            Busy = false;
        }
    }

    private void PuantajYap()
    {
        if (!EnsureAuth(YetkiTipleri.Export)) return;
        if (SelectedAy is null) return;

        try
        {
            Busy = true;
            using var scope = _scopes.CreateScope();
            var psvc = scope.ServiceProvider.GetRequiredService<IPuantajService>();
            var request = new PuantajExportRequest
            {
                Yil = SelectedAy.Yil,
                Ay = SelectedAy.Ay,
                Yetkiler = _yetkiler
            };

            var exportData = psvc.PrepareMonthlyExport(request);
            var dlg = new SaveFileDialog
            {
                Filter = "Excel Dosyası | *.xlsx",
                Title = "Excel Dosyasını Kaydet",
                FileName = $"{SelectedAy.Yil} {new DateTime(SelectedAy.Yil, SelectedAy.Ay, 1):MMMM} Ayı Puantaj Exceli.xlsx"
            };

            if (dlg.ShowDialog() == true)
            {
                ExcelHelper.ExceleDonustur(exportData, dlg.FileName);
                UiDialog.Success("Excel kaydedildi.", "Bilgi");
                Status = "Export tamamlandı.";
            }
        }
        catch (Exception ex)
        {
            UiDialog.Error("Export sırasında hata oluştu:\n" + ex.Message, "Hata");
        }
        finally
        {
            Busy = false;
        }
    }

    private void EkKayitAyarla()
    {
        if (!CanEditEkKayit) return;
        if (!EnsureAuth(YetkiTipleri.Update)) return;

        Errors.Clear();
        Status = null;
        if (!int.TryParse(EkKayitGunText, out var gun) || gun < 0 || gun > 31)
        {
            Errors.Set("EkKayitGun", "Lütfen 0-31 arasında bir tam sayı girin.");
            Status = Errors.FirstMessage;
            return;
        }

        try
        {
            using var scope = _scopes.CreateScope();
            var psvc = scope.ServiceProvider.GetRequiredService<IPuantajService>();
            psvc.SetEkKayitGun(gun, (int)_session.AktifKullaniciId!);
            _ekKayitGun = gun;
            UiDialog.Success($"Ek Kayıt Süresi güncellendi.\nSüre: {_ekKayitGun} gün.", "Bilgi");
            LoadGrid();
        }
        catch (Exception ex)
        {
            UiDialog.Error("Güncelleme başarısız:\n" + ex.Message, "Hata");
        }
    }
}
