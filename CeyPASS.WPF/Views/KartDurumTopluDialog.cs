using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;

namespace CeyPASS.WPF.Views;

/// <summary>Kart Atamaları — anlık serbest/kısıtlı durumu; toplu Kısıtla / Kısıtı Kaldır.</summary>
public static class KartDurumTopluDialog
{
    private const string DialogTitle = "Kart durumları";
    private const string TipAll = "all";
    private const string TipMisafir = "misafir";
    private const string TipArac = "arac";

    /// <returns>En az bir komut kuyruğa yazıldıysa true.</returns>
    public static bool Show(
        Window owner,
        ISessionContext session,
        IMisafirKartService misafirSvc,
        IAracKartiService aracSvc,
        ICanliIzlemeKartKomutService kartKomutSvc)
    {
        if (!session.AktifFirmaId.HasValue)
        {
            UiDialog.Warning("Aktif firma bilgisi bulunamadı.", DialogTitle, owner);
            return false;
        }

        var firmaId = session.AktifFirmaId.Value;
        var misafirList = misafirSvc.GetAtamaListe(firmaId) ?? new List<KartAtamaListeItem>();
        var aracList = aracSvc.GetAtamaListe(firmaId) ?? new List<KartAtamaListeItem>();

        var allSource = new List<KartDurumRow>(misafirList.Count + aracList.Count);
        void Append(IEnumerable<KartAtamaListeItem> items, string tipKey, string tipLabel)
        {
            foreach (var x in items)
            {
                var pid = x.PersonelId ?? "";
                if (string.IsNullOrWhiteSpace(pid)) continue;
                allSource.Add(new KartDurumRow
                {
                    PersonelId = pid.Trim(),
                    KartAdi = x.KartAdi,
                    KisiPlaka = FormatKisiPlaka(x.MisafirAdSoyad, x.Plaka),
                    AtamaDurumText = x.DurumText,
                    TipKey = tipKey,
                    TipLabel = tipLabel,
                    CihazdaAktif = true // map below
                });
            }
        }
        Append(misafirList, TipMisafir, "Misafir");
        Append(aracList, TipArac, "Araç");

        var aktifMap = kartKomutSvc.GetCihazdaAktifMap(firmaId, allSource.Select(x => x.PersonelId));
        foreach (var r in allSource)
            r.CihazdaAktif = !aktifMap.TryGetValue(r.PersonelId, out var a) || a;

        var rows = new ObservableCollection<KartDurumRow>();
        var changed = false;
        var accent = Color.FromRgb(0x25, 0x63, 0xEB);
        var soft = ThemeBrushes.ColorOf("Brush.DialogSoft", Color.FromRgb(0xEF, 0xF6, 0xFF));

        var subtitle = new TextBlock
        {
            FontSize = 13,
            Foreground = ThemeBrushes.Get("Brush.TextMuted", Color.FromRgb(0x64, 0x74, 0x8B)),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 0)
        };

        void ApplyFilter(string tipKey)
        {
            rows.Clear();
            IEnumerable<KartDurumRow> src = tipKey switch
            {
                TipMisafir => allSource.Where(r => r.TipKey == TipMisafir),
                TipArac => allSource.Where(r => r.TipKey == TipArac),
                _ => allSource
            };
            foreach (var r in src.OrderBy(x => x.TipLabel).ThenBy(x => x.KartAdi, StringComparer.CurrentCultureIgnoreCase))
                rows.Add(r);

            var tipText = tipKey switch
            {
                TipMisafir => "Misafir",
                TipArac => "Araç",
                _ => "Tümü"
            };
            subtitle.Text = $"{tipText} — butona basıldığı andaki durum ({rows.Count} kart).";
        }

        var tipItems = new[]
        {
            new TipFilterItem(TipAll, "Tümü"),
            new TipFilterItem(TipMisafir, "Misafir"),
            new TipFilterItem(TipArac, "Araç")
        };
        var cmbTip = new ComboBox
        {
            Width = 140,
            FontSize = 13,
            ItemsSource = tipItems,
            DisplayMemberPath = nameof(TipFilterItem.Label),
            SelectedIndex = 0,
            VerticalAlignment = VerticalAlignment.Center,
            Padding = new Thickness(8, 4, 8, 4)
        };

        var grid = new DataGrid
        {
            ItemsSource = rows,
            AutoGenerateColumns = false,
            CanUserAddRows = false,
            CanUserDeleteRows = false,
            CanUserResizeRows = false,
            IsReadOnly = false,
            SelectionMode = DataGridSelectionMode.Extended,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            RowHeaderWidth = 0,
            MinHeight = 280,
            MaxHeight = 420,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            FontSize = 13
        };
        grid.Columns.Add(new DataGridCheckBoxColumn
        {
            Header = "",
            Binding = new System.Windows.Data.Binding(nameof(KartDurumRow.IsSelected))
            {
                UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
            },
            Width = new DataGridLength(36)
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Tip",
            Binding = new System.Windows.Data.Binding(nameof(KartDurumRow.TipLabel)),
            IsReadOnly = true,
            Width = new DataGridLength(70)
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Kart",
            Binding = new System.Windows.Data.Binding(nameof(KartDurumRow.KartAdi)),
            IsReadOnly = true,
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Kişi / Plaka",
            Binding = new System.Windows.Data.Binding(nameof(KartDurumRow.KisiPlaka)),
            IsReadOnly = true,
            Width = new DataGridLength(1.1, DataGridLengthUnitType.Star)
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Atama",
            Binding = new System.Windows.Data.Binding(nameof(KartDurumRow.AtamaDurumText)),
            IsReadOnly = true,
            Width = new DataGridLength(80)
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Cihaz",
            Binding = new System.Windows.Data.Binding(nameof(KartDurumRow.KisitDurumText)),
            IsReadOnly = true,
            Width = new DataGridLength(80)
        });

        void SetAll(bool selected)
        {
            foreach (var r in rows)
                r.IsSelected = selected;
        }

        void EnqueueTargets(IReadOnlyList<KartDurumRow> targets, bool pasif, string emptyMsg)
        {
            if (targets.Count == 0)
            {
                UiDialog.Warning(emptyMsg, DialogTitle, owner);
                return;
            }

            var baslik = pasif ? "Kartı Kısıtla" : "Kart Kısıtı Kaldır";
            var onay = pasif
                ? $"{targets.Count} kart cihazlarda kısıtlansın mı?"
                : $"{targets.Count} kartın kısıtı kaldırılsın mı?";
            if (!UiDialog.Confirm(onay, baslik, owner, yesText: pasif ? "Kısıtla" : "Kısıtı kaldır", noText: "Vazgeç"))
                return;

            var ok = 0;
            try
            {
                foreach (var r in targets)
                {
                    if (pasif)
                        kartKomutSvc.EnqueuePasif(firmaId, r.PersonelId, session.AktifKullaniciId);
                    else
                        kartKomutSvc.EnqueueAktif(firmaId, r.PersonelId, session.AktifKullaniciId);
                    r.CihazdaAktif = !pasif;
                    ok++;
                }

                changed = true;
                UiDialog.Success(
                    pasif
                        ? $"{ok} kısıtlama komutu kuyruğa alındı."
                        : $"{ok} kısıt kaldırma komutu kuyruğa alındı.",
                    baslik,
                    owner);
            }
            catch (Exception ex)
            {
                if (ok > 0)
                    changed = true;
                UiDialog.Error(
                    ok > 0
                        ? $"{ok} kart yazıldı, sonra hata:\n{ex.Message}"
                        : ex.Message,
                    baslik,
                    owner);
            }
        }

        void EnqueueSelected(bool pasif)
            => EnqueueTargets(
                rows.Where(r => r.IsSelected).ToList(),
                pasif,
                "Önce kart seçin.");

        void EnqueueByDurum(bool currentlyAktif, bool pasif, string emptyMsg)
            => EnqueueTargets(
                rows.Where(r => r.CihazdaAktif == currentlyAktif).ToList(),
                pasif,
                emptyMsg);

        // Sabit genişlikte 2×3; uzun metinler hücre içinde ortalanır (kayma yok)
        var toolbar = new UniformGrid
        {
            Columns = 3,
            Rows = 2,
            Margin = new Thickness(0, 0, 0, 10)
        };
        void AddTb(Button b)
        {
            b.Margin = new Thickness(0, 0, 6, 6);
            b.HorizontalAlignment = HorizontalAlignment.Stretch;
            b.VerticalAlignment = VerticalAlignment.Stretch;
            b.MinHeight = 40;
            toolbar.Children.Add(b);
        }

        AddTb(CreateCompactButton("Tümünü seç", Color.FromRgb(0x3B, 0x82, 0xF6), () => SetAll(true)));
        AddTb(CreateCompactButton("Seçimi kaldır", Color.FromRgb(0x64, 0x74, 0x8B), () => SetAll(false)));
        AddTb(CreateCompactButton("Seçilenleri kısıtla", Color.FromRgb(0xEA, 0x58, 0x0C), () => EnqueueSelected(pasif: true)));
        AddTb(CreateCompactButton("Seçilenleri serbest bırak", Color.FromRgb(0x0D, 0x94, 0x88), () => EnqueueSelected(pasif: false)));
        AddTb(CreateCompactButton("Tüm serbestleri kısıtla", Color.FromRgb(0xDC, 0x26, 0x26),
            () => EnqueueByDurum(currentlyAktif: true, pasif: true, "Serbest kart yok.")));
        AddTb(CreateCompactButton("Tüm kısıtlıları serbest bırak", Color.FromRgb(0x16, 0xA3, 0x4A),
            () => EnqueueByDurum(currentlyAktif: false, pasif: false, "Kısıtlı kart yok.")));

        var filterBar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 8),
            HorizontalAlignment = HorizontalAlignment.Left
        };
        var tipLabel = new TextBlock
        {
            Text = "Kart tipi",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0),
            FontSize = 13,
            Foreground = ThemeBrushes.Get("Brush.TextMuted", Color.FromRgb(0x64, 0x74, 0x8B))
        };
        filterBar.Children.Add(tipLabel);
        filterBar.Children.Add(cmbTip);

        var body = new DockPanel();
        DockPanel.SetDock(filterBar, Dock.Top);
        DockPanel.SetDock(toolbar, Dock.Top);
        body.Children.Add(filterBar);
        body.Children.Add(toolbar);
        body.Children.Add(grid);

        cmbTip.SelectionChanged += (_, _) =>
        {
            if (cmbTip.SelectedItem is TipFilterItem t)
                ApplyFilter(t.Key);
        };
        ApplyFilter(TipAll);

        var dlg = new Window
        {
            Title = DialogTitle,
            Width = 680,
            SizeToContent = SizeToContent.Height,
            MinWidth = 680,
            MaxWidth = 680,
            MinHeight = 360,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.None,
            AllowsTransparency = true,
            Background = Brushes.Transparent,
            ShowInTaskbar = false,
            FontFamily = new FontFamily("Segoe UI"),
            SnapsToDevicePixels = true,
            Owner = owner,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var card = new Border
        {
            Background = ThemeBrushes.Get("Brush.Card", Colors.White),
            CornerRadius = new CornerRadius(16),
            Margin = new Thickness(12),
            Effect = new DropShadowEffect
            {
                BlurRadius = 28,
                ShadowDepth = 4,
                Opacity = 0.22,
                Color = Color.FromRgb(0x0F, 0x17, 0x2A)
            }
        };

        var root = new DockPanel();
        var topStripe = new Border
        {
            Height = 5,
            Background = new SolidColorBrush(accent),
            CornerRadius = new CornerRadius(16, 16, 0, 0)
        };
        DockPanel.SetDock(topStripe, Dock.Top);
        root.Children.Add(topStripe);

        var footer = new Border { Padding = new Thickness(22, 0, 22, 20) };
        DockPanel.SetDock(footer, Dock.Bottom);
        var closeRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        closeRow.Children.Add(UiDialogChrome.CreateButton("Kapat", isPrimary: false, accent, () =>
        {
            dlg.DialogResult = changed;
        }));
        footer.Child = closeRow;
        root.Children.Add(footer);

        var header = new Grid { Margin = new Thickness(22, 18, 22, 8) };
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var titlePanel = new StackPanel();
        titlePanel.Children.Add(new TextBlock
        {
            Text = DialogTitle,
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Foreground = ThemeBrushes.Get("Brush.TextPrimary", Color.FromRgb(0x0F, 0x17, 0x2A))
        });
        titlePanel.Children.Add(subtitle);
        Grid.SetColumn(titlePanel, 0);
        header.Children.Add(titlePanel);
        var xBtn = UiDialogChrome.CreateCloseButton(() => dlg.DialogResult = changed);
        Grid.SetColumn(xBtn, 1);
        header.Children.Add(xBtn);
        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        var bodyHost = new Border
        {
            Child = body,
            Margin = new Thickness(22, 4, 22, 12),
            Background = new SolidColorBrush(soft),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(14)
        };
        root.Children.Add(bodyHost);
        card.Child = root;
        dlg.Content = card;

        dlg.PreviewKeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                dlg.DialogResult = changed;
                e.Handled = true;
            }
        };
        dlg.MouseLeftButtonDown += (_, e) =>
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                dlg.DragMove();
        };

        dlg.ShowDialog();
        return changed;
    }

    private static string FormatKisiPlaka(string? misafirAdSoyad, string? plaka)
    {
        var ad = (misafirAdSoyad ?? "").Trim();
        var pl = (plaka ?? "").Trim();
        if (ad.Length == 0)
            return pl;
        if (pl.Length == 0)
            return ad;
        return $"{ad} ({pl})";
    }

    private static Button CreateCompactButton(string text, Color bg, Action onClick)
    {
        byte Scale(byte v, double amount) => (byte)Math.Max(0, (int)(v * (1 - amount)));
        var hover = Color.FromRgb(Scale(bg.R, 0.12), Scale(bg.G, 0.12), Scale(bg.B, 0.12));
        var label = new TextBlock
        {
            Text = text,
            FontWeight = FontWeights.SemiBold,
            FontSize = 11.5,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            TextTrimming = TextTrimming.None,
            TextAlignment = TextAlignment.Center
        };
        var bd = new Border
        {
            Background = new SolidColorBrush(bg),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(6, 8, 6, 8),
            ClipToBounds = false,
            SnapsToDevicePixels = true,
            Child = label
        };
        var factory = new FrameworkElementFactory(typeof(ContentPresenter));
        factory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
        factory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Stretch);
        var btn = new Button
        {
            Content = bd,
            Cursor = Cursors.Hand,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch,
            MinWidth = 0,
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent,
            Padding = new Thickness(0),
            ToolTip = text,
            ClipToBounds = false,
            Template = new ControlTemplate(typeof(Button)) { VisualTree = factory }
        };
        btn.MouseEnter += (_, _) => bd.Background = new SolidColorBrush(hover);
        btn.MouseLeave += (_, _) => bd.Background = new SolidColorBrush(bg);
        btn.Click += (_, _) => onClick();
        return btn;
    }

    private sealed record TipFilterItem(string Key, string Label);

    private sealed class KartDurumRow : INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _cihazdaAktif;

        public string PersonelId { get; init; } = "";
        public string KartAdi { get; init; } = "";
        public string KisiPlaka { get; init; } = "";
        public string AtamaDurumText { get; init; } = "";
        public string TipKey { get; init; } = "";
        public string TipLabel { get; init; } = "";

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public bool CihazdaAktif
        {
            get => _cihazdaAktif;
            set
            {
                if (_cihazdaAktif == value) return;
                _cihazdaAktif = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(KisitDurumText));
            }
        }

        public string KisitDurumText => CihazdaAktif ? "Serbest" : "Kısıtlı";

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
