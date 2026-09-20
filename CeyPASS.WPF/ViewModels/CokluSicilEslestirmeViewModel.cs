using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CeyPASS.Business.Abstractions;
using CeyPASS.Infrastructure.Helpers;
using CeyPASS.Entities.Concrete;
using Microsoft.Extensions.DependencyInjection;

namespace CeyPASS.WPF.ViewModels;

/// <summary>Çoklu sicil hedef bağlantı grid satırı.</summary>
public sealed class CokluSicilGridRow : ObservableObject
{
    public int HedefPersonelId { get; set; }
    public string HedefAdSoyad { get; set; } = "";
    public string FirmaIsyeri { get; set; } = "";
    public string BolumAdi { get; set; } = "";
    public int AktarimGunSayisi { get; set; }
    public string GirisCikis { get; set; } = "";
    public string Aciklama { get; set; } = "";
    public string AktifText { get; set; } = "";
    public bool AktifMi { get; set; }
    public CokluSicilBaglantiDTO Source { get; set; } = null!;
}

/// <summary>Hedef sicil combobox öğesi (aday veya mevcut bağlantı).</summary>
public sealed class CokluSicilHedefComboItem
{
    public int PersonelId { get; set; }
    public string Display { get; set; } = "";
    public CokluSicilHedefAdayDTO? Source { get; set; }
    public CokluSicilBaglantiDTO? Baglanti { get; set; }
}

/// <summary>
/// Ana sicilden hedef sicillere aktarım günü tanımı; puantaj ekranındaki aylık aktarımın ön koşulu.
/// </summary>
public sealed class CokluSicilEslestirmeViewModel : ObservableObject
{
    private readonly IServiceScopeFactory _scopes;
    private readonly ISessionContext _session;
    private readonly IAuthorizationService _auth;
    private readonly int _anaPersonelId;
    private readonly string _tcKimlikNo;
    private readonly string _adSoyad;

    private CokluSicilGridRow? _selectedRow;
    private CokluSicilHedefComboItem? _selectedHedef;
    private int _aktarimGun = 1;
    private string? _aciklama;
    private bool _aktifMi = true;
    private string? _error;
    private bool _canSave;
    private bool _canPasiflestir;
    private bool _canAktiflestir;
    private bool _canPasiflestirTumunu;
    private string _summaryText = "";
    private string _editorModeText = "Yeni eşleştirme";
    private string _hedefOrgText = "Hedef sicil seçildiğinde firma / işyeri / bölüm bilgisi burada görünür.";
    private DateTime? _iseGirisTarihi;
    private bool _showAktifCalisiyorText = true;
    private string _istenCikisDisplayText = "";
    private bool _showEmptyState = true;
    private bool _suppressSelectionSync;

    public CokluSicilEslestirmeViewModel(IServiceProvider root, int anaPersonelId, string tcKimlikNo, string adSoyad)
    {
        _scopes = root.GetRequiredService<IServiceScopeFactory>();
        _session = root.GetRequiredService<ISessionContext>();
        _auth = root.GetRequiredService<IAuthorizationService>();
        _anaPersonelId = anaPersonelId;
        _tcKimlikNo = tcKimlikNo?.Trim() ?? "";
        _adSoyad = adSoyad?.Trim() ?? "";

        Rows = new ObservableCollection<CokluSicilGridRow>();
        Hedefler = new ObservableCollection<CokluSicilHedefComboItem>();

        SaveCommand = new RelayCommand(Save, () => CanSave);
        PasiflestirCommand = new RelayCommand(Pasiflestir, () => CanPasiflestir);
        AktiflestirCommand = new RelayCommand(Aktiflestir, () => CanAktiflestir);
        PasiflestirTumunuCommand = new RelayCommand(PasiflestirTumunu, () => CanPasiflestirTumunu);
        ClearCommand = new RelayCommand(ClearEditor, () => true);

        Title = $"Çoklu Sicil Eşleştirmeleri — {_adSoyad}";
        Subtitle = $"Ana sicil: {_anaPersonelId}  •  TC: {MaskTc(_tcKimlikNo)}";
        RefreshAuth();
    }

    public void Load()
    {
        LoadData();
        ClearEditor();
    }

    public string Title { get; }
    public string Subtitle { get; }
    public ObservableCollection<CokluSicilGridRow> Rows { get; }
    public ObservableCollection<CokluSicilHedefComboItem> Hedefler { get; }

    public CokluSicilGridRow? SelectedRow
    {
        get => _selectedRow;
        set
        {
            if (_suppressSelectionSync || Equals(_selectedRow, value)) return;
            SetProperty(ref _selectedRow, value);
            if (value is null)
            {
                RefreshToggleCommands();
                return;
            }

            EditorModeText = $"Düzenle — hedef sicil {value.HedefPersonelId}";
            _suppressSelectionSync = true;
            try
            {
                SelectedHedef = Hedefler.FirstOrDefault(h => h.PersonelId == value.HedefPersonelId)
                    ?? CreateComboFromBaglanti(value.Source);
            }
            finally { _suppressSelectionSync = false; }

            AktarimGun = value.AktarimGunSayisi;
            Aciklama = value.Source.Aciklama;
            AktifMi = value.AktifMi;
            IseGirisTarihi = value.Source.IseGirisTarihi;
            RefreshIstenCikisDisplay();
            UpdateHedefOrgText();
            RefreshAuth();
        }
    }

    public CokluSicilHedefComboItem? SelectedHedef
    {
        get => _selectedHedef;
        set
        {
            if (Equals(_selectedHedef, value)) return;
            SetProperty(ref _selectedHedef, value);
            if (_suppressSelectionSync) return;

            if (value?.Baglanti != null)
            {
                EditorModeText = $"Düzenle — hedef sicil {value.PersonelId}";
                AktarimGun = value.Baglanti.AktarimGunSayisi;
                Aciklama = value.Baglanti.Aciklama;
                AktifMi = value.Baglanti.AktifMi;
                IseGirisTarihi = value.Baglanti.IseGirisTarihi ?? value.Source?.IseGirisTarihi;
            }
            else if (value != null)
            {
                EditorModeText = "Yeni eşleştirme";
                AktarimGun = 1;
                Aciklama = null;
                AktifMi = true;
                IseGirisTarihi = value.Source?.IseGirisTarihi;
            }

            RefreshIstenCikisDisplay();
            UpdateHedefOrgText();
        }
    }

    public int AktarimGun { get => _aktarimGun; set => SetProperty(ref _aktarimGun, value); }
    public string? Aciklama { get => _aciklama; set => SetProperty(ref _aciklama, value); }
    public bool AktifMi { get => _aktifMi; set => SetProperty(ref _aktifMi, value); }
    public DateTime? IseGirisTarihi { get => _iseGirisTarihi; set => SetProperty(ref _iseGirisTarihi, value); }

    public bool ShowAktifCalisiyorText { get => _showAktifCalisiyorText; private set => SetProperty(ref _showAktifCalisiyorText, value); }
    public bool ShowIstenCikisDateText => !ShowAktifCalisiyorText;
    public string IstenCikisDisplayText { get => _istenCikisDisplayText; private set => SetProperty(ref _istenCikisDisplayText, value); }
    public string AktifCalisiyorPlaceholder => "Aktif Çalışıyor";

    public string? Error { get => _error; private set => SetProperty(ref _error, value); }
    public string SummaryText { get => _summaryText; private set => SetProperty(ref _summaryText, value); }
    public string EditorModeText { get => _editorModeText; private set => SetProperty(ref _editorModeText, value); }
    public string HedefOrgText { get => _hedefOrgText; private set => SetProperty(ref _hedefOrgText, value); }
    public bool ShowEmptyState { get => _showEmptyState; private set => SetProperty(ref _showEmptyState, value); }
    public bool CanSave { get => _canSave; private set => SetProperty(ref _canSave, value); }
    public bool CanPasiflestir { get => _canPasiflestir; private set => SetProperty(ref _canPasiflestir, value); }
    public bool CanAktiflestir { get => _canAktiflestir; private set => SetProperty(ref _canAktiflestir, value); }
    public bool CanPasiflestirTumunu { get => _canPasiflestirTumunu; private set => SetProperty(ref _canPasiflestirTumunu, value); }

    public ICommand SaveCommand { get; }
    public ICommand PasiflestirCommand { get; }
    public ICommand AktiflestirCommand { get; }
    public ICommand PasiflestirTumunuCommand { get; }
    public ICommand ClearCommand { get; }

    private void RefreshAuth()
    {
        CanSave = _auth.Can("Personeller", YetkiTipleri.Update);
        RefreshToggleCommands();
        CommandManager.InvalidateRequerySuggested();
    }

    private void RefreshToggleCommands()
    {
        CanPasiflestir = CanSave && SelectedRow is { AktifMi: true };
        CanAktiflestir = CanSave && SelectedRow is { AktifMi: false };
        CanPasiflestirTumunu = CanSave && Rows.Any(r => r.AktifMi);
    }

    private void LoadData()
    {
        Error = null;
        try
        {
            using var scope = _scopes.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<ICokluSicilService>();

            var baglantilar = svc.GetByAnaPersonelId(_anaPersonelId);
            var adaylar = svc.GetHedefAdaylari(_anaPersonelId, _tcKimlikNo);

            Rows.Clear();
            foreach (var b in baglantilar)
            {
                Rows.Add(new CokluSicilGridRow
                {
                    HedefPersonelId = b.HedefPersonelId,
                    HedefAdSoyad = b.HedefAdSoyad ?? "",
                    FirmaIsyeri = $"{b.FirmaAdi ?? "-"} / {b.IsyeriAdi ?? "-"}",
                    BolumAdi = b.BolumAdi ?? "-",
                    AktarimGunSayisi = b.AktarimGunSayisi,
                    GirisCikis = FormatGirisCikis(b.IseGirisTarihi, b.IstenCikisTarihi),
                    Aciklama = string.IsNullOrWhiteSpace(b.Aciklama) ? "-" : b.Aciklama!,
                    AktifText = b.AktifMi ? "Aktif" : "Pasif",
                    AktifMi = b.AktifMi,
                    Source = b
                });
            }

            var bagMap = baglantilar.ToDictionary(b => b.HedefPersonelId);
            Hedefler.Clear();
            foreach (var a in adaylar.OrderBy(x => x.AdSoyad))
            {
                bagMap.TryGetValue(a.PersonelId, out var bag);
                Hedefler.Add(CreateComboItem(a, bag));
            }
            foreach (var b in baglantilar.Where(b => adaylar.All(a => a.PersonelId != b.HedefPersonelId)))
                Hedefler.Add(CreateComboFromBaglanti(b));

            var aktif = Rows.Count(r => r.AktifMi);
            var pasif = Rows.Count - aktif;
            SummaryText = Rows.Count == 0
                ? "Henüz hedef sicil bağlantısı yok."
                : $"{aktif} aktif, {pasif} pasif hedef sicil";
            ShowEmptyState = Rows.Count == 0;
            RefreshToggleCommands();
        }
        catch (Exception ex)
        {
            LogHelper.Error("Personeller", "CokluSicilLoadData", $"AnaPersonelId={_anaPersonelId}", ex);
            Error = ex.Message;
        }
    }

    private static CokluSicilHedefComboItem CreateComboItem(CokluSicilHedefAdayDTO a, CokluSicilBaglantiDTO? bag)
    {
        var bagli = bag != null ? (bag.AktifMi ? " — bağlı (aktif)" : " — bağlı (pasif)") : "";
        var engel = !a.SecilebilirMi && string.IsNullOrWhiteSpace(bagli) ? $" — {a.EngelMesaji}" : "";
        return new CokluSicilHedefComboItem
        {
            PersonelId = a.PersonelId,
            Display = $"{a.PersonelId} — {a.AdSoyad}  |  {a.FirmaAdi ?? "-"} / {a.IsyeriAdi ?? "-"}{bagli}{engel}",
            Source = a,
            Baglanti = bag
        };
    }

    private static CokluSicilHedefComboItem CreateComboFromBaglanti(CokluSicilBaglantiDTO b)
        => new()
        {
            PersonelId = b.HedefPersonelId,
            Display = $"{b.HedefPersonelId} — {b.HedefAdSoyad ?? "?"}  |  {b.FirmaAdi ?? "-"} / {b.IsyeriAdi ?? "-"} — bağlı",
            Baglanti = b
        };

    private void UpdateHedefOrgText()
    {
        if (SelectedHedef?.Source != null)
        {
            var s = SelectedHedef.Source;
            HedefOrgText = $"Firma: {s.FirmaAdi ?? "-"}  •  İşyeri: {s.IsyeriAdi ?? "-"}  •  Bölüm: {s.BolumAdi ?? "-"}";
            return;
        }
        if (SelectedHedef?.Baglanti != null)
        {
            var b = SelectedHedef.Baglanti;
            HedefOrgText = $"Firma: {b.FirmaAdi ?? "-"}  •  İşyeri: {b.IsyeriAdi ?? "-"}  •  Bölüm: {b.BolumAdi ?? "-"}";
            return;
        }
        HedefOrgText = "Hedef sicil seçildiğinde firma / işyeri / bölüm bilgisi burada görünür.";
    }

    private void RefreshIstenCikisDisplay()
    {
        var effective = GetEffectiveIstenCikis(SelectedHedef);
        ShowAktifCalisiyorText = SelectedHedef == null || !effective.HasValue;
        IstenCikisDisplayText = effective?.ToString("dd.MM.yyyy") ?? "";
        RaisePropertyChanged(nameof(ShowIstenCikisDateText));
    }

    private static DateTime? GetEffectiveIstenCikis(CokluSicilHedefComboItem? hedef)
    {
        if (hedef == null) return null;
        if (hedef.Baglanti?.IstenCikisTarihi is { } bagDate) return bagDate;
        return hedef.Source?.IstenCikisTarihi;
    }

    private void Save()
    {
        Error = null;
        if (SelectedHedef == null) { Error = "Hedef sicil seçiniz."; return; }
        if (SelectedHedef.Source is { SecilebilirMi: false } aday && SelectedHedef.Baglanti == null)
        {
            Error = aday.EngelMesaji ?? "Bu hedef sicil seçilemez.";
            return;
        }
        if (AktarimGun < 1) { Error = "Aktarım gün sayısı en az 1 olmalıdır."; return; }

        try
        {
            using var scope = _scopes.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<ICokluSicilService>();
            var src = SelectedHedef.Source;
            svc.Upsert(_anaPersonelId, _tcKimlikNo, new CokluSicilUpsertRequest
            {
                HedefPersonelId = SelectedHedef.PersonelId,
                FirmaId = src?.FirmaId ?? SelectedHedef.Baglanti?.FirmaId,
                SirketId = src?.IsyeriId ?? SelectedHedef.Baglanti?.SirketId,
                BolumId = src?.BolumId ?? SelectedHedef.Baglanti?.BolumId,
                IseGirisTarihi = IseGirisTarihi,
                IstenCikisTarihi = null,
                AktarimGunSayisi = AktarimGun,
                Aciklama = Aciklama,
                AktifMi = AktifMi
            }, _session.AktifKullaniciId);

            LoadData();
            ClearEditor();
        }
        catch (Exception ex) { Error = ex.Message; }
    }

    private void Pasiflestir() => SetSelectedAktif(false);
    private void Aktiflestir() => SetSelectedAktif(true);

    private void PasiflestirTumunu()
    {
        var aktifSayisi = Rows.Count(r => r.AktifMi);
        if (aktifSayisi <= 0) return;
        if (!UiDialog.Confirm(
                $"{aktifSayisi} aktif hedef bağlantı pasifleştirilecek. Devam edilsin mi?",
                "Çoklu Sicil",
                yesText: "Pasifleştir",
                noText: "Vazgeç"))
            return;

        Error = null;
        try
        {
            using var scope = _scopes.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<ICokluSicilService>();
            svc.PasifleştirTümünü(_anaPersonelId, _session.AktifKullaniciId);
            LoadData();
            ClearEditor();
        }
        catch (Exception ex) { Error = ex.Message; }
    }

    private void SetSelectedAktif(bool aktif)
    {
        if (SelectedRow == null) return;
        Error = null;
        try
        {
            using var scope = _scopes.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<ICokluSicilService>();
            svc.SetAktif(_anaPersonelId, SelectedRow.HedefPersonelId, aktif, _session.AktifKullaniciId);
            LoadData();
            ClearEditor();
        }
        catch (Exception ex) { Error = ex.Message; }
    }

    public void ClearEditor()
    {
        EditorModeText = "Yeni eşleştirme";
        _suppressSelectionSync = true;
        try
        {
            _selectedRow = null;
            RaisePropertyChanged(nameof(SelectedRow));
            SelectedHedef = null;
        }
        finally { _suppressSelectionSync = false; }

        AktarimGun = 1;
        Aciklama = null;
        AktifMi = true;
        IseGirisTarihi = null;
        RefreshIstenCikisDisplay();
        Error = null;
        UpdateHedefOrgText();
        RefreshAuth();
    }

    private static string FormatGirisCikis(DateTime? giris, DateTime? cikis)
    {
        var g = giris?.ToString("dd.MM.yyyy") ?? "-";
        var c = cikis?.ToString("dd.MM.yyyy") ?? "Aktif";
        return $"{g} — {c}";
    }

    private static string MaskTc(string tc)
    {
        if (string.IsNullOrWhiteSpace(tc)) return "-";
        var t = tc.Trim();
        if (t.Length <= 4) return t;
        return new string('*', t.Length - 4) + t[^4..];
    }
}
