using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using DevExpress.Xpf.Grid;
using Microsoft.Extensions.DependencyInjection;

namespace CeyPASS.WPF.Views;

/// <summary>Canlı geçiş kartları, hareket listesi ve misafir/araç kart atama (role göre görünürlük).</summary>
public partial class CanliIzlemeWindow : CeypassThemedWindow
{
    private sealed class AtamaTipItem
    {
        public string Key { get; init; } = "";
        public string Label { get; init; } = "";
        public override string ToString() => Label;
    }

    private readonly ISessionContext _session;
    private readonly ICanliIzlemeService _svc;
    private readonly IKisiHareketService _khsvc;
    private readonly IMisafirKartService _misafirSvc;
    private readonly IAracKartiService _aracSvc;
    private readonly ICanliIzlemeKartKomutService _kartKomutSvc;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DispatcherTimer _timer;
    private readonly ObservableCollection<PassCardVm> _cards = new();
    private readonly ObservableCollection<AtamaRowVm> _atamaRows = new();
    private int? _seciliKisiId;
    private string? _seciliRowKey;
    private string[] _lastHareketKeys = Array.Empty<string>();
    private string[] _lastAtamaKeys = Array.Empty<string>();
    private bool _refreshingGrid;
    private bool _atamaTipReady;
    private bool _atamaRefreshInFlight;
    private bool _pendingAtamaRefresh;
    private bool _pendingAtamaForce;
    private DateTime _lastAtamaRefreshUtc = DateTime.MinValue;
    private List<KisiListItem>? _misafirKartCache;
    private DateTime _misafirKartCacheUtc = DateTime.MinValue;
    private List<KisiListItem>? _aracKartCache;
    private DateTime _aracKartCacheUtc = DateTime.MinValue;
    private const int AtamaRefreshIntervalSeconds = 1;
    private const int AtamaKartCacheSeconds = 5;

    public CanliIzlemeWindow(
        ISessionContext session,
        ICanliIzlemeService svc,
        IKisiHareketService khsvc,
        IKisiDetayService kisiDetaySvc,
        IMisafirKartService misafirSvc,
        IAracKartiService aracSvc,
        ICanliIzlemeKartKomutService kartKomutSvc,
        IServiceScopeFactory scopeFactory)
    {
        InitializeComponent();
        _session = session;
        _svc = svc;
        _khsvc = khsvc;
        _ = kisiDetaySvc;
        _misafirSvc = misafirSvc;
        _aracSvc = aracSvc;
        _kartKomutSvc = kartKomutSvc;
        _scopeFactory = scopeFactory;

        for (var i = 0; i < 4; i++)
            _cards.Add(PassCardVm.Empty());
        LastPassCards.ItemsSource = _cards;
        AtamaGrid.ItemsSource = _atamaRows;

        CmbAtamaTip.ItemsSource = new[]
        {
            new AtamaTipItem { Key = "misafir", Label = "Misafir / Ziyaretçi" },
            new AtamaTipItem { Key = "arac", Label = "Araç" }
        };
        CmbAtamaTip.DisplayMemberPath = nameof(AtamaTipItem.Label);
        CmbAtamaTip.SelectedIndex = 0;
        _atamaTipReady = true;

        Loaded += (_, _) =>
        {
            ApplyRoleVisibility();
            RefreshAll();
        };

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (_, _) => RefreshAll();
        _timer.Start();
    }

    private void ApplyRoleVisibility()
    {
        var showList = CanliIzlemeRoleHelper.ShowHareketListesi(_session.RolAdi);

        HareketPanel.Visibility = showList ? Visibility.Visible : Visibility.Collapsed;

        if (!showList)
        {
            CardsRow.Height = new GridLength(1, GridUnitType.Star);
            MovesRowDef.Height = new GridLength(0);
        }
        else
        {
            CardsRow.Height = new GridLength(1.2, GridUnitType.Star);
            MovesRowDef.Height = new GridLength(1, GridUnitType.Star);
        }
    }

    private void RefreshAll()
    {
        if (!_session.AktifFirmaId.HasValue) return;
        RefreshLastPasses();
        if (CanliIzlemeRoleHelper.ShowHareketListesi(_session.RolAdi))
        {
            RefreshHareketler();
            var due = (DateTime.UtcNow - _lastAtamaRefreshUtc).TotalSeconds >= AtamaRefreshIntervalSeconds;
            if (due)
                RefreshAtamaListe(force: false);
        }
    }

    private void RefreshLastPasses()
    {
        try
        {
            var firmaId = _session.AktifFirmaId!.Value;
            var rol = _session.RolAdi;
            List<LastPassDTO> passes;
            if (CanliIzlemeRoleHelper.IsArac(rol))
                passes = _svc.GetLastPassesArac(firmaId, 4);
            else if (CanliIzlemeRoleHelper.IsYemekhane(rol))
                passes = _svc.GetLastPassesYemekhane(firmaId, 4);
            else
                passes = _svc.GetLastPasses(firmaId, 4);

            for (var i = 0; i < 4; i++)
            {
                if (i < passes.Count)
                    _cards[i].Apply(passes[i]);
                else
                    _cards[i].Clear();
            }
        }
        catch
        {
        }
    }

    private void RefreshHareketler()
    {
        try
        {
            var firmaId = _session.AktifFirmaId!.Value;
            var rol = _session.RolAdi;
            List<KisiHareketDTO> list;
            if (CanliIzlemeRoleHelper.IsArac(rol) && !CanliIzlemeRoleHelper.IsDanisma(rol))
                list = _khsvc.GetLastMovesByFirmaArac(15, firmaId);
            else if (CanliIzlemeRoleHelper.IsYemekhane(rol) && !CanliIzlemeRoleHelper.IsDanisma(rol))
                list = _khsvc.GetLastMovesByFirmaYemekhane(15, firmaId);
            else
                list = _khsvc.GetLastMovesByFirma(15, firmaId);

            var rows = new List<HareketRow>(list.Count);
            var keyCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var x in list)
            {
                var baseKey = HareketRow.BuildBaseKey(x.Tarih, x.PersonelId, x.CihazAdi);
                keyCounts.TryGetValue(baseKey, out var n);
                keyCounts[baseKey] = n + 1;
                var rowKey = n == 0 ? baseKey : $"{baseKey}#{n}";

                rows.Add(new HareketRow
                {
                    Tarih = x.Tarih,
                    AdSoyad = x.AdSoyad,
                    Turnike = x.CihazAdi,
                    KisiId = x.PersonelId,
                    RowKey = rowKey
                });
            }

            var newKeys = rows.Select(r => r.RowKey).ToArray();
            var unchanged = newKeys.Length == _lastHareketKeys.Length
                            && newKeys.SequenceEqual(_lastHareketKeys);

            if (unchanged)
                return;

            _lastHareketKeys = newKeys;

            _refreshingGrid = true;
            try
            {
                HareketGrid.ItemsSource = rows;
                HideInternalColumns();

                if (!string.IsNullOrEmpty(_seciliRowKey))
                {
                    var match = rows.FirstOrDefault(r => r.RowKey == _seciliRowKey);
                    HareketGrid.SelectedItem = match;
                }
            }
            finally
            {
                _refreshingGrid = false;
            }
        }
        catch
        {
        }
    }

    private bool IsAracAtamaTip()
        => CmbAtamaTip.SelectedItem is AtamaTipItem t
           && string.Equals(t.Key, "arac", StringComparison.OrdinalIgnoreCase);

    private string CurrentAtamaTipKey()
        => IsAracAtamaTip() ? "arac" : "misafir";

    private void InvalidateAtamaKartCacheForCurrentTip()
    {
        if (IsAracAtamaTip())
        {
            _aracKartCache = null;
            _aracKartCacheUtc = DateTime.MinValue;
        }
        else
        {
            _misafirKartCache = null;
            _misafirKartCacheUtc = DateTime.MinValue;
        }
    }

    private IReadOnlyList<KisiListItem>? TryGetTipKartCache(string tipKey)
    {
        if (string.Equals(tipKey, "arac", StringComparison.Ordinal))
        {
            if (_aracKartCache != null
                && (DateTime.UtcNow - _aracKartCacheUtc).TotalSeconds < AtamaKartCacheSeconds)
                return _aracKartCache;
            return null;
        }

        if (_misafirKartCache != null
            && (DateTime.UtcNow - _misafirKartCacheUtc).TotalSeconds < AtamaKartCacheSeconds)
            return _misafirKartCache;
        return null;
    }

    private void StoreTipKartCache(string tipKey, List<KisiListItem> kartlar)
    {
        if (string.Equals(tipKey, "arac", StringComparison.Ordinal))
        {
            _aracKartCache = kartlar;
            _aracKartCacheUtc = DateTime.UtcNow;
        }
        else
        {
            _misafirKartCache = kartlar;
            _misafirKartCacheUtc = DateTime.UtcNow;
        }
    }

    private void DrainPendingAtamaRefresh()
    {
        if (!_pendingAtamaRefresh) return;
        var force = _pendingAtamaForce;
        _pendingAtamaRefresh = false;
        _pendingAtamaForce = false;
        RefreshAtamaListe(force: force, invalidateKartCache: false);
    }

    private void RefreshAtamaListe(bool force = true, bool invalidateKartCache = false)
    {
        if (!_session.AktifFirmaId.HasValue) return;

        if (invalidateKartCache)
            InvalidateAtamaKartCacheForCurrentTip();

        if (_atamaRefreshInFlight)
        {
            _pendingAtamaRefresh = true;
            _pendingAtamaForce |= force;
            return;
        }

        var firmaId = _session.AktifFirmaId.Value;
        var isArac = IsAracAtamaTip();
        var tipKey = isArac ? "arac" : "misafir";
        var cachedKartlar = TryGetTipKartCache(tipKey);

        _atamaRefreshInFlight = true;
        _lastAtamaRefreshUtc = DateTime.UtcNow;

        _ = Task.Run(() =>
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var misafirSvc = scope.ServiceProvider.GetRequiredService<IMisafirKartService>();
                var aracSvc = scope.ServiceProvider.GetRequiredService<IAracKartiService>();
                var kartKomutSvc = scope.ServiceProvider.GetRequiredService<ICanliIzlemeKartKomutService>();

                List<KisiListItem>? kartlarForCache = null;
                IReadOnlyList<KisiListItem>? kartlarArg = cachedKartlar;
                if (kartlarArg == null)
                {
                    kartlarForCache = isArac
                        ? aracSvc.GetAktifKartlar(firmaId)
                        : misafirSvc.GetAktifKartlar(firmaId);
                    kartlarArg = kartlarForCache;
                }

                var list = isArac
                    ? aracSvc.GetAtamaListe(firmaId, kartlarArg)
                    : misafirSvc.GetAtamaListe(firmaId, kartlarArg);

                var aktifMap = kartKomutSvc.GetCihazdaAktifMap(
                    firmaId,
                    list.Select(x => x.PersonelId));

                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        if (!string.Equals(CurrentAtamaTipKey(), tipKey, StringComparison.Ordinal))
                        {
                            _pendingAtamaRefresh = true;
                            _pendingAtamaForce = true;
                            return;
                        }

                        if (kartlarForCache != null)
                            StoreTipKartCache(tipKey, kartlarForCache);

                        var keys = list.Select(x =>
                        {
                            var pid = x.PersonelId ?? "";
                            var aktif = !aktifMap.TryGetValue(pid, out var a) || a;
                            return $"{tipKey}|{pid}|{x.Durum}|{x.AtamaId}|{x.MisafirAdSoyad}|{x.KartAdi}|{x.Plaka}|{aktif}";
                        }).ToArray();
                        if (!force && keys.Length == _lastAtamaKeys.Length && keys.SequenceEqual(_lastAtamaKeys))
                            return;
                        _lastAtamaKeys = keys;

                        _atamaRows.Clear();
                        foreach (var x in list)
                        {
                            var pid = x.PersonelId ?? "";
                            var cihazdaAktif = !aktifMap.TryGetValue(pid, out var a) || a;
                            _atamaRows.Add(AtamaRowVm.From(x, cihazdaAktif));
                        }
                    }
                    finally
                    {
                        _atamaRefreshInFlight = false;
                        DrainPendingAtamaRefresh();
                    }
                });
            }
            catch
            {
                Dispatcher.Invoke(() =>
                {
                    _atamaRefreshInFlight = false;
                    DrainPendingAtamaRefresh();
                });
            }
        });
    }

    private void CmbAtamaTip_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_atamaTipReady) return;
        _lastAtamaKeys = Array.Empty<string>();
        if (_session.AktifFirmaId.HasValue && CanliIzlemeRoleHelper.ShowHareketListesi(_session.RolAdi))
            RefreshAtamaListe(force: true, invalidateKartCache: false);
    }

    private void HideInternalColumns()
    {
        if (HareketGrid.Columns["KisiId"] != null)
            HareketGrid.Columns["KisiId"].Visible = false;
        if (HareketGrid.Columns["RowKey"] != null)
            HareketGrid.Columns["RowKey"].Visible = false;
    }

    private void HareketGrid_OnSelectedItemChanged(object sender, SelectedItemChangedEventArgs e)
    {
        if (_refreshingGrid) return;
        if (e.NewItem is not HareketRow row) return;
        _seciliKisiId = row.KisiId;
        _seciliRowKey = row.RowKey;
    }

    /// <summary>Çift tık: Hazır satırda yeni atama, diğer durumlarda güncelleme diyaloğu.</summary>
    private void AtamaGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        => OpenAtamaFromSelection();

    private void AtamaTableView_OnRowDoubleClick(object sender, RowDoubleClickEventArgs e)
        => OpenAtamaFromSelection();

    private void BtnSatirKartPasif_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: AtamaRowVm row })
            EnqueueKartKomut(row, pasif: true);
    }

    private void BtnSatirKartAktif_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: AtamaRowVm row })
            EnqueueKartKomut(row, pasif: false);
    }

    private void BtnKartDurumToplu_OnClick(object sender, RoutedEventArgs e)
    {
        var firmaId = RequireFirmaId();
        if (firmaId == null) return;

        var changed = KartDurumTopluDialog.Show(
            this,
            _session,
            _misafirSvc,
            _aracSvc,
            _kartKomutSvc);

        if (!changed) return;
        _lastAtamaKeys = Array.Empty<string>();
        RefreshAtamaListe(force: true, invalidateKartCache: true);
    }

    private void EnqueueKartKomut(AtamaRowVm row, bool pasif)
    {
        var firmaId = RequireFirmaId();
        if (firmaId == null) return;

        var baslik = pasif ? "Kartı Kısıtla" : "Kart Kısıtı Kaldır";
        var onay = pasif
            ? $"“{row.KartAdi}” kartı cihazlarda kısıtlansın mı?"
            : $"“{row.KartAdi}” kartındaki kısıt kaldırılsın mı?";
        if (!UiDialog.Confirm(onay, baslik, this, yesText: pasif ? "Kısıtla" : "Kısıtı kaldır", noText: "Vazgeç"))
            return;

        try
        {
            if (pasif)
                _kartKomutSvc.EnqueuePasif(firmaId.Value, row.PersonelId, _session.AktifKullaniciId);
            else
                _kartKomutSvc.EnqueueAktif(firmaId.Value, row.PersonelId, _session.AktifKullaniciId);

            UiDialog.Success(
                pasif ? "Kısıtlama komutu kuyruğa alındı." : "Kısıt kaldırma komutu kuyruğa alındı.",
                baslik,
                this);
            _lastAtamaKeys = Array.Empty<string>();
            RefreshAtamaListe(force: true, invalidateKartCache: true);
        }
        catch (Exception ex)
        {
            UiDialog.Error(ex.Message, baslik, this);
        }
    }

    private void OpenAtamaFromSelection()
    {
        if (AtamaGrid.SelectedItem is not AtamaRowVm row) return;
        var firmaId = RequireFirmaId();
        if (firmaId == null) return;

        var isArac = IsAracAtamaTip();
        if (row.Durum == KartAtamaListeDurum.Hazir)
        {
            if (isArac)
                AracKartiAtamaDialog.ShowYeni(this, _session, _aracSvc, firmaId.Value, row.PersonelId);
            else
                MisafirKartAtamaDialog.ShowYeni(this, _session, _misafirSvc, firmaId.Value, row.PersonelId);
        }
        else
        {
            if (!row.AtamaId.HasValue)
            {
                UiDialog.Warning("Atama kaydı bulunamadı.", "Canlı İzleme", this);
                return;
            }
            if (isArac)
                AracKartiAtamaDialog.ShowGuncelle(this, _session, _aracSvc, firmaId.Value, row.AtamaId);
            else
                MisafirKartAtamaDialog.ShowGuncelle(this, _session, _misafirSvc, firmaId.Value, row.AtamaId);
        }

        _lastAtamaKeys = Array.Empty<string>();
        RefreshAtamaListe(force: true, invalidateKartCache: true);
    }

    private int? RequireFirmaId()
    {
        if (!_session.AktifFirmaId.HasValue)
        {
            UiDialog.Warning("Aktif firma bilgisi bulunamadı.", "Canlı İzleme", this);
            return null;
        }
        return _session.AktifFirmaId.Value;
    }

    private void Window_OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _timer.Stop();
        System.Windows.Application.Current.Shutdown();
    }

    private static BitmapImage? BytesToImage(byte[]? bytes)
    {
        if (bytes == null || bytes.Length == 0) return null;
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

    private static ImageSource LoadUnknown()
        => new BitmapImage(new Uri("pack://application:,,,/Assets/Unknown_person.jpg"));

    private sealed class HareketRow
    {
        public DateTime Tarih { get; set; }
        public string? AdSoyad { get; set; }
        public string? Turnike { get; set; }
        public int KisiId { get; set; }
        public string RowKey { get; set; } = "";

        public static string BuildBaseKey(DateTime tarih, int kisiId, string? turnike)
            => $"{tarih:O}|{kisiId}|{turnike ?? ""}";
    }

    private sealed class AtamaRowVm
    {
        public string PersonelId { get; init; } = "";
        public string KartAdi { get; init; } = "";
        public string? MisafirAdSoyad { get; init; }
        public int? AtamaId { get; init; }
        public KartAtamaListeDurum Durum { get; init; }
        public string DurumText { get; init; } = "";
        public Brush DurumBg { get; init; } = Brushes.Gray;
        public bool CanPasifEt { get; init; }
        public bool CanAktifEt { get; init; }

        public static AtamaRowVm From(KartAtamaListeItem x, bool cihazdaAktif)
        {
            string? kisi = x.MisafirAdSoyad;
            if (!string.IsNullOrWhiteSpace(x.Plaka))
            {
                kisi = string.IsNullOrWhiteSpace(kisi)
                    ? x.Plaka.Trim()
                    : $"{kisi.Trim()} ({x.Plaka.Trim()})";
            }

            bool atanmis = x.Durum != KartAtamaListeDurum.Hazir;
            return new AtamaRowVm
            {
                PersonelId = x.PersonelId,
                KartAdi = x.KartAdi,
                MisafirAdSoyad = kisi,
                AtamaId = x.AtamaId,
                Durum = x.Durum,
                DurumText = x.DurumText,
                DurumBg = x.Durum switch
                {
                    KartAtamaListeDurum.Hazir => new SolidColorBrush(Color.FromRgb(0xE6, 0x7E, 0x22)),
                    KartAtamaListeDurum.Atanmis => new SolidColorBrush(Color.FromRgb(0x47, 0x69, 0x8A)),
                    KartAtamaListeDurum.Giris => new SolidColorBrush(Color.FromRgb(0x2E, 0x8B, 0x57)),
                    KartAtamaListeDurum.Cikis => new SolidColorBrush(Color.FromRgb(0xB2, 0x22, 0x22)),
                    _ => Brushes.Gray
                },
                CanPasifEt = atanmis && cihazdaAktif,
                CanAktifEt = !cihazdaAktif
            };
        }
    }

    private sealed class PassCardVm : ObservableObject
    {
        private string _adSoyad = "-";
        private string _zamanText = "";
        private string _terminal = "";
        private string _yonText = "";
        private Brush _yonBg = Brushes.Gray;
        private ImageSource? _photo;

        public string AdSoyad { get => _adSoyad; set => SetProperty(ref _adSoyad, value); }
        public string ZamanText { get => _zamanText; set => SetProperty(ref _zamanText, value); }
        public string Terminal { get => _terminal; set => SetProperty(ref _terminal, value); }
        public string YonText { get => _yonText; set => SetProperty(ref _yonText, value); }
        public Brush YonBg { get => _yonBg; set => SetProperty(ref _yonBg, value); }
        public ImageSource? Photo { get => _photo; set => SetProperty(ref _photo, value); }

        public static PassCardVm Empty()
        {
            var c = new PassCardVm();
            c.Clear();
            return c;
        }

        public void Clear()
        {
            AdSoyad = "-";
            ZamanText = "";
            Terminal = "";
            YonText = "";
            YonBg = new SolidColorBrush(Color.FromRgb(0x6C, 0x75, 0x7D));
            Photo = LoadUnknown();
        }

        public void Apply(LastPassDTO p)
        {
            AdSoyad = p.AdSoyad ?? "-";
            ZamanText = p.Zaman.ToString("dd.MM.yyyy HH:mm:ss");
            Terminal = p.TerminalAdi ?? "";
            YonText = p.GirisMi ? "GİRİŞ" : "ÇIKIŞ";
            YonBg = new SolidColorBrush(p.GirisMi
                ? Color.FromRgb(0x2E, 0x8B, 0x57)
                : Color.FromRgb(0xB2, 0x22, 0x22));
            Photo = BytesToImage(p.Foto) ?? LoadUnknown();
        }
    }
}
