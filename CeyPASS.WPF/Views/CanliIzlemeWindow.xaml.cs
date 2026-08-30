using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using DevExpress.Xpf.Grid;

namespace CeyPASS.WPF.Views;

public partial class CanliIzlemeWindow : CeypassThemedWindow
{
    private readonly ISessionContext _session;
    private readonly ICanliIzlemeService _svc;
    private readonly IKisiHareketService _khsvc;
    private readonly IKisiDetayService _kisiDetaySvc;
    private readonly IMisafirKartService _misafirSvc;
    private readonly IAracKartiService _aracSvc;
    private readonly DispatcherTimer _timer;
    private readonly ObservableCollection<PassCardVm> _cards = new();
    private int? _seciliKisiId;
    private string? _seciliRowKey;
    private string[] _lastHareketKeys = Array.Empty<string>();
    private bool _refreshingGrid;

    public CanliIzlemeWindow(
        ISessionContext session,
        ICanliIzlemeService svc,
        IKisiHareketService khsvc,
        IKisiDetayService kisiDetaySvc,
        IMisafirKartService misafirSvc,
        IAracKartiService aracSvc)
    {
        InitializeComponent();
        _session = session;
        _svc = svc;
        _khsvc = khsvc;
        _kisiDetaySvc = kisiDetaySvc;
        _misafirSvc = misafirSvc;
        _aracSvc = aracSvc;

        for (var i = 0; i < 4; i++)
            _cards.Add(PassCardVm.Empty());
        LastPassCards.ItemsSource = _cards;

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
        if (CanliIzlemeRoleHelper.HideKartAtama(_session.RolAdi))
            KartButonlariPanel.Visibility = Visibility.Collapsed;
    }

    private void RefreshAll()
    {
        if (!_session.AktifFirmaId.HasValue) return;
        RefreshLastPasses();
        RefreshHareketler();
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
            // Canlı ekranda timer hatalarını sessizce yut; bir sonraki tick dener
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
                    Departman = x.Departman,
                    Unvan = x.Unvan,
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

                // Aynı kişi birden fazla satırdaysa KisiId ile FirstOrDefault kullanma —
                // seçilen hareketin RowKey'i ile geri yükle.
                if (!string.IsNullOrEmpty(_seciliRowKey))
                {
                    var match = rows.FirstOrDefault(r => r.RowKey == _seciliRowKey);
                    if (match != null)
                        HareketGrid.SelectedItem = match;
                    else
                        HareketGrid.SelectedItem = null; // hareket listeden düştü; ilk KisiId satırına sıçrama
                }
            }
            finally
            {
                _refreshingGrid = false;
            }
        }
        catch
        {
            // timer tick
        }
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
        LoadKisiDetay(row.KisiId);
    }

    private void LoadKisiDetay(int kisiId)
    {
        try
        {
            var dto = _kisiDetaySvc.GetDetay(kisiId);
            if (dto == null)
            {
                LblSeciliAd.Text = "-";
                LblSeciliUnvan.Text = "-";
                LblSeciliDepartman.Text = "-";
                ImgSecili.Source = LoadUnknown();
                return;
            }

            LblSeciliAd.Text = dto.AdSoyad ?? "-";
            LblSeciliUnvan.Text = dto.Unvan ?? "-";
            LblSeciliDepartman.Text = dto.Departman ?? "-";
            ImgSecili.Source = BytesToImage(dto.Foto) ?? LoadUnknown();
        }
        catch (Exception ex)
        {
            UiDialog.Error("Kişi detayları alınamadı: " + ex.Message, "Hata", this);
        }
    }

    private void BtnKisiyeKartAta_OnClick(object sender, RoutedEventArgs e)
        => OpenMisafirYeni();

    private void BtnAtananKartGuncelle_OnClick(object sender, RoutedEventArgs e)
        => OpenMisafirGuncelle();

    private void BtnAracKartiVer_OnClick(object sender, RoutedEventArgs e)
        => OpenAracYeni();

    private void BtnAracKartiGuncelle_OnClick(object sender, RoutedEventArgs e)
        => OpenAracGuncelle();

    private int? RequireFirmaId()
    {
        if (!_session.AktifFirmaId.HasValue)
        {
            UiDialog.Warning("Aktif firma bilgisi bulunamadı.", "Canlı İzleme", this);
            return null;
        }
        return _session.AktifFirmaId.Value;
    }

    private void OpenMisafirYeni()
    {
        var firmaId = RequireFirmaId();
        if (firmaId == null) return;
        MisafirKartAtamaDialog.ShowYeni(this, _session, _misafirSvc, firmaId.Value);
    }

    private void OpenMisafirGuncelle()
    {
        var firmaId = RequireFirmaId();
        if (firmaId == null) return;
        MisafirKartAtamaDialog.ShowGuncelle(this, _session, _misafirSvc, firmaId.Value);
    }

    private void OpenAracYeni()
    {
        var firmaId = RequireFirmaId();
        if (firmaId == null) return;
        AracKartiAtamaDialog.ShowYeni(this, _session, _aracSvc, firmaId.Value);
    }

    private void OpenAracGuncelle()
    {
        var firmaId = RequireFirmaId();
        if (firmaId == null) return;
        AracKartiAtamaDialog.ShowGuncelle(this, _session, _aracSvc, firmaId.Value);
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
        public string? Departman { get; set; }
        public string? Unvan { get; set; }
        public string? Turnike { get; set; }
        public int KisiId { get; set; }
        public string RowKey { get; set; } = "";

        public static string BuildBaseKey(DateTime tarih, int kisiId, string? turnike)
            => $"{tarih:O}|{kisiId}|{turnike ?? ""}";
    }

    private sealed class PassCardVm : ObservableObject
    {
        private string _adSoyad = "-";
        private string _departman = "";
        private string _unvan = "";
        private string _zamanText = "";
        private string _terminal = "";
        private string _yonText = "";
        private Brush _yonBg = Brushes.Gray;
        private ImageSource? _photo;

        public string AdSoyad { get => _adSoyad; set => SetProperty(ref _adSoyad, value); }
        public string Departman { get => _departman; set => SetProperty(ref _departman, value); }
        public string Unvan { get => _unvan; set => SetProperty(ref _unvan, value); }
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
            Departman = "";
            Unvan = "";
            ZamanText = "";
            Terminal = "";
            YonText = "";
            YonBg = new SolidColorBrush(Color.FromRgb(0x6C, 0x75, 0x7D));
            Photo = LoadUnknown();
        }

        public void Apply(LastPassDTO p)
        {
            AdSoyad = p.AdSoyad ?? "-";
            Departman = p.DepartmanAdi ?? "";
            Unvan = p.Unvan ?? "";
            ZamanText = p.Zaman.ToString("dd.MM.yyyy HH:mm:ss");
            Terminal = p.TerminalAdi ?? "";
            // WFA: SeaGreen / Firebrick
            YonText = p.GirisMi ? "GİRİŞ" : "ÇIKIŞ";
            YonBg = new SolidColorBrush(p.GirisMi
                ? Color.FromRgb(0x2E, 0x8B, 0x57)   // SeaGreen
                : Color.FromRgb(0xB2, 0x22, 0x22)); // Firebrick
            Photo = BytesToImage(p.Foto) ?? LoadUnknown();
        }
    }
}
