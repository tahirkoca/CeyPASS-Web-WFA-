using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;

namespace CeyPASS.WPF.Views;

/// <summary>
/// Seçili günlere aynı Giriş/Çıkış (cihaz + saat) değerlerini uygular; tip/saat her gün ayrı hesaplanır.
/// </summary>
public static class PuantajTopluDuzenleDialog
{
    private const string KartBasimiYok = "Kart basımı yok";
    private const string ElleMudahaleAd = "Elle Müdahale";
    private static readonly CultureInfo Tr = new("tr-TR");
    private static readonly Color AccentBlue = Color.FromRgb(0x25, 0x63, 0xEB);
    private static readonly Color TrackOff = Color.FromRgb(0xCB, 0xD5, 0xE1);

    private static CihazListDTO CreateElleMudahale(int cihazTipi) => new()
    {
        CihazId = 0,
        CihazTipi = cihazTipi,
        AnaGirisCikisMi = true,
        CihazAdi = ElleMudahaleAd,
        Text = ElleMudahaleAd,
        AktifMi = true
    };

    public static bool Show(
        IReadOnlyList<PuantajGunSatirDTO> gunler,
        int personelId,
        int firmaId,
        IPuantajService psvc,
        IKisiHareketService hareketSvc,
        ICihazService cihazSvc,
        ISessionContext session,
        out int updated,
        out int skippedFail)
    {
        updated = 0;
        skippedFail = 0;

        if (gunler == null || gunler.Count == 0)
            return false;

        var ordered = gunler.OrderBy(g => g.Tarih).ToList();
        var ornek = ordered[0];
        var ornekGun = ornek.Tarih.Date;

        var tipler = psvc.GetPuantajTipleri() ?? new List<PuantajTipDTO>();
        var tumCihazlar = (cihazSvc.GetListe(sadeceAktif: true, firmaId: firmaId) ?? new List<CihazListDTO>())
            .Where(c => c.AnaGirisCikisMi)
            .ToList();

        foreach (var c in tumCihazlar)
            c.Text = string.IsNullOrWhiteSpace(c.CihazAdi) ? $"#{c.CihazId}" : c.CihazAdi.Trim();

        var girisCihazlar = tumCihazlar.Where(c => c.CihazTipi == 1).ToList();
        var cikisCihazlar = tumCihazlar.Where(c => c.CihazTipi == 0).ToList();
        // Elle Müdahale yalnızca firmada ana giriş/çıkış cihazı yoksa
        if (tumCihazlar.Count == 0)
        {
            girisCihazlar.Insert(0, CreateElleMudahale(cihazTipi: 1));
            cikisCihazlar.Insert(0, CreateElleMudahale(cihazTipi: 0));
        }

        var uctan = hareketSvc.GetGunUctanUca(personelId, ornekGun);

        var root = new StackPanel();

        root.Children.Add(UiFormDialog.CreateLabel("Seçili günler"));
        var ozet = BuildTarihOzet(ordered);
        root.Children.Add(new TextBlock
        {
            Text = ozet,
            FontSize = 15,
            FontWeight = FontWeights.SemiBold,
            Foreground = ThemeBrushes.Get("Brush.TextPrimary", Color.FromRgb(0x0F, 0x17, 0x2A)),
            Margin = new Thickness(0, 0, 0, 4),
            TextWrapping = TextWrapping.Wrap
        });
        root.Children.Add(new TextBlock
        {
            Text = "Önizleme ilk seçili güne göredir; Onayla’da her gün kendi saatine göre ayrı hesaplanır.",
            FontSize = 12,
            FontStyle = FontStyles.Italic,
            Foreground = ThemeBrushes.Get("Brush.TextMuted", Color.FromRgb(0x64, 0x74, 0x8B)),
            Margin = new Thickness(0, 0, 0, 14),
            TextWrapping = TextWrapping.Wrap
        });

        var columns = new Grid { Margin = new Thickness(0, 0, 0, 12) };
        columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
        columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var girisPanel = BuildSidePanel(
            "Giriş",
            girisCihazlar,
            ornek.IlkGiris,
            ResolveCihazId(uctan.GirisTarih, ornek.IlkGiris, uctan.GirisCihazId),
            out var tgGiris,
            out var cmbGirisCihaz,
            out var txtGirisSaat);
        Grid.SetColumn(girisPanel, 0);
        columns.Children.Add(girisPanel);

        var divider = new Border
        {
            Width = 1,
            Background = ThemeBrushes.Get("Brush.FieldBorder", Color.FromRgb(0xD0, 0xD7, 0xE2)),
            Margin = new Thickness(0, 4, 0, 4),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetColumn(divider, 1);
        columns.Children.Add(divider);

        var cikisPanel = BuildSidePanel(
            "Çıkış",
            cikisCihazlar,
            ornek.SonCikis,
            ResolveCihazId(uctan.CikisTarih, ornek.SonCikis, uctan.CikisCihazId),
            out var tgCikis,
            out var cmbCikisCihaz,
            out var txtCikisSaat);
        Grid.SetColumn(cikisPanel, 2);
        columns.Children.Add(cikisPanel);

        root.Children.Add(columns);

        var tipSaatGrid = new Grid { Margin = new Thickness(0, 4, 0, 0) };
        tipSaatGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        tipSaatGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
        tipSaatGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var mevcutPanel = new StackPanel();
        mevcutPanel.Children.Add(UiFormDialog.CreateLabel("Mevcut Çalışma Tipi"));
        mevcutPanel.Children.Add(new TextBlock
        {
            Text = FormatTipAd(tipler, ornek.CalismaTipi),
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Foreground = ThemeBrushes.Get("Brush.Text", Color.FromRgb(0x1E, 0x29, 0x3B)),
            Margin = new Thickness(0, 0, 0, 10)
        });
        mevcutPanel.Children.Add(UiFormDialog.CreateLabel("Mevcut Çalışma Saati"));
        mevcutPanel.Children.Add(new TextBlock
        {
            Text = ornek.Saat > 0
                ? ornek.Saat.ToString("0.##", CultureInfo.InvariantCulture)
                : "—",
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Foreground = ThemeBrushes.Get("Brush.Text", Color.FromRgb(0x1E, 0x29, 0x3B))
        });
        Grid.SetColumn(mevcutPanel, 0);
        tipSaatGrid.Children.Add(mevcutPanel);

        var tipDivider = new Border
        {
            Width = 1,
            Background = ThemeBrushes.Get("Brush.FieldBorder", Color.FromRgb(0xD0, 0xD7, 0xE2)),
            Margin = new Thickness(0, 4, 0, 4),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetColumn(tipDivider, 1);
        tipSaatGrid.Children.Add(tipDivider);

        var hesapPanel = new StackPanel();
        hesapPanel.Children.Add(UiFormDialog.CreateLabel("Hesaplanan Çalışma Tipi"));
        var tipText = new TextBlock
        {
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Foreground = ThemeBrushes.Get("Brush.Text", Color.FromRgb(0x1E, 0x29, 0x3B)),
            Margin = new Thickness(0, 0, 0, 10)
        };
        hesapPanel.Children.Add(tipText);
        hesapPanel.Children.Add(UiFormDialog.CreateLabel("Hesaplanan Çalışma Saati"));
        var txtHesaplananSaat = UiFormDialog.CreateTextBox("");
        txtHesaplananSaat.Margin = new Thickness(0, 0, 0, 0);
        txtHesaplananSaat.MaxLength = 12;
        hesapPanel.Children.Add(txtHesaplananSaat);
        Grid.SetColumn(hesapPanel, 2);
        tipSaatGrid.Children.Add(hesapPanel);

        root.Children.Add(tipSaatGrid);

        root.Children.Add(UiFormDialog.CreateLabel("Açıklama"));
        const string aciklamaPlaceholder =
            "Hesaplanan çalışma saatini değiştiriyorsanız nedenini yazmanız önerilir.";
        var txtAciklama = UiFormDialog.CreateTextBoxWithPlaceholder(ornek.Aciklama ?? "", aciklamaPlaceholder);
        txtAciklama.MaxLength = 400;
        txtAciklama.AcceptsReturn = true;
        txtAciklama.TextWrapping = TextWrapping.Wrap;
        txtAciklama.Height = 64;
        txtAciklama.VerticalContentAlignment = VerticalAlignment.Top;
        txtAciklama.Margin = new Thickness(0, 0, 0, 4);
        root.Children.Add(txtAciklama);

        var mevcutTipKod = ornek.CalismaTipi ?? "";
        var lastSpTipKod = mevcutTipKod;
        var manuelSaatModu = false;
        var suppressSaatChanged = false;
        Action? applyHareketEnabled = null;

        void SetHesaplananSaatFromSp(decimal? saat)
        {
            suppressSaatChanged = true;
            try
            {
                txtHesaplananSaat.Text = saat.HasValue
                    ? saat.Value.ToString("0.##", CultureInfo.InvariantCulture)
                    : "";
            }
            finally
            {
                suppressSaatChanged = false;
            }
        }

        void Recalc()
        {
            if (manuelSaatModu) return;

            string computedTip;
            decimal? computedSaat;
            try
            {
                DateTime? gOv = null;
                DateTime? cOv = null;

                if (tgGiris.IsChecked == true && TryParseTime(GetSaatText(txtGirisSaat), out var gTs))
                    gOv = ornekGun.Add(gTs);
                if (tgCikis.IsChecked == true && TryParseTime(GetSaatText(txtCikisSaat), out var cTs))
                {
                    cOv = ornekGun.Add(cTs);
                    if (gOv.HasValue && cOv <= gOv)
                        cOv = cOv.Value.AddDays(1);
                }

                var tipSaat = psvc.GetGunTipSaat(personelId, ornekGun, gOv, cOv, girisAcik: true, cikisAcik: true);
                computedTip = tipSaat.CalismaTipi ?? "";
                computedSaat = tipSaat.Saat;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("PuantajTopluDuzenleDialog.Recalc: " + ex);
                computedTip = "";
                computedSaat = null;
            }

            lastSpTipKod = computedTip;
            tipText.Text = FormatTipAd(tipler, computedTip);
            SetHesaplananSaatFromSp(computedSaat);
        }

        void ApplyManuelSaatTipi()
        {
            if (!TryParseDecimalSaat(txtHesaplananSaat.Text, out var saat))
            {
                tipText.Text = "—";
                lastSpTipKod = "";
                return;
            }

            // Mevcut (Aylık SP / Final) ailesi + katalog eşikleri
            lastSpTipKod = psvc.ResolveManuelCalismaTipi(mevcutTipKod, saat);
            tipText.Text = FormatTipAd(tipler, lastSpTipKod);
        }

        void ClearEksikVeriAciklamaIfNeeded()
        {
            var val = UiFormDialog.GetTextBoxValue(txtAciklama);
            if (!string.Equals(val, "EKSİK VERİ", StringComparison.OrdinalIgnoreCase))
                return;
            UiFormDialog.ResetTextBoxToPlaceholder(txtAciklama);
        }

        void EnterManuelSaatModu()
        {
            if (manuelSaatModu) return;
            manuelSaatModu = true;
            tgGiris.IsChecked = false;
            tgCikis.IsChecked = false;
            tgGiris.IsEnabled = false;
            tgCikis.IsEnabled = false;
            applyHareketEnabled?.Invoke();
            ClearEksikVeriAciklamaIfNeeded();
        }

        void ExitManuelSaatModuIfNeeded()
        {
            if (!manuelSaatModu) return;
            manuelSaatModu = false;
            tgGiris.IsEnabled = true;
            tgCikis.IsEnabled = true;
            applyHareketEnabled?.Invoke();
            Recalc();
        }

        void WireSide(ToggleButton tg, ComboBox cmb, TextBox txt)
        {
            string? lastValidSaat = IsValidTimeText(txt.Text) ? txt.Text.Trim() : null;

            void ApplyEnabled()
            {
                var locked = manuelSaatModu;
                var on = !locked && tg.IsChecked == true;
                tg.IsEnabled = !locked;
                cmb.IsEnabled = on;
                cmb.Opacity = on ? 1 : 0.55;

                if (on)
                {
                    if (string.Equals(txt.Text, KartBasimiYok, StringComparison.Ordinal)
                        || string.IsNullOrWhiteSpace(txt.Text))
                        txt.Text = lastValidSaat ?? "";
                    txt.IsEnabled = true;
                    txt.Opacity = 1;
                    txt.FontStyle = FontStyles.Normal;
                    txt.Foreground = ThemeBrushes.Get("Brush.TextPrimary", Color.FromRgb(0x0F, 0x17, 0x2A));
                }
                else
                {
                    if (IsValidTimeText(txt.Text))
                        lastValidSaat = txt.Text.Trim();

                    if (!string.IsNullOrEmpty(lastValidSaat))
                        SetSaltOkunurSaat(txt, lastValidSaat);
                    else
                        SetKartBasimiYok(txt);
                }
            }

            applyHareketEnabled += ApplyEnabled;

            tg.Checked += (_, _) =>
            {
                if (manuelSaatModu)
                {
                    ExitManuelSaatModuIfNeeded();
                    ClearEksikVeriAciklamaIfNeeded();
                    return;
                }
                ClearEksikVeriAciklamaIfNeeded();
                ApplyEnabled();
                Recalc();
            };
            tg.Unchecked += (_, _) => { ApplyEnabled(); Recalc(); };
            txt.TextChanged += (_, _) =>
            {
                if (tg.IsChecked == true && IsValidTimeText(txt.Text))
                    lastValidSaat = txt.Text.Trim();
                if (tg.IsChecked == true && !string.Equals(txt.Text, KartBasimiYok, StringComparison.Ordinal))
                {
                    ClearEksikVeriAciklamaIfNeeded();
                    Recalc();
                }
            };
            ApplyEnabled();
        }

        WireSide(tgGiris, cmbGirisCihaz, txtGirisSaat);
        WireSide(tgCikis, cmbCikisCihaz, txtCikisSaat);

        txtHesaplananSaat.TextChanged += (_, _) =>
        {
            if (suppressSaatChanged) return;
            EnterManuelSaatModu();
            ApplyManuelSaatTipi();
        };

        Recalc();

        var localUpdated = 0;
        var localSkipped = 0;

        var ok = UiFormDialog.Show(
            title: "Toplu Puantaj Düzenle",
            subtitle: "Seçtiğiniz giriş/çıkış veya hesaplanan saat tüm günlere uygulanır. Kayıt Onayla ile kaydedilir.",
            body: root,
            primaryText: "Onayla",
            secondaryText: "İptal",
            width: 560,
            validateOnPrimary: () =>
            {
                TimeSpan? girisTs = null;
                TimeSpan? cikisTs = null;
                int? girisCihazId = null;
                int? cikisCihazId = null;
                decimal? saatOverride = null;
                string? tipOverride = null;

                if (manuelSaatModu)
                {
                    if (!TryParseDecimalSaat(txtHesaplananSaat.Text, out var manuelSaat))
                    {
                        UiDialog.Warning("Hesaplanan çalışma saati sayısal olmalıdır (örn. 7.5).", "Uyarı");
                        return false;
                    }
                    saatOverride = manuelSaat;
                    tipOverride = lastSpTipKod;
                }
                else
                {
                    if (tgGiris.IsChecked == true)
                    {
                        if (cmbGirisCihaz.SelectedItem is not CihazListDTO gCihaz)
                        {
                            UiDialog.Warning("Giriş için cihaz seçin.", "Uyarı");
                            return false;
                        }
                        if (!TryParseTime(GetSaatText(txtGirisSaat), out var gTs))
                        {
                            UiDialog.Warning("Giriş saati HH:mm veya HH:mm:ss olmalıdır.", "Uyarı");
                            return false;
                        }
                        girisTs = gTs;
                        girisCihazId = gCihaz.CihazId;
                    }

                    if (tgCikis.IsChecked == true)
                    {
                        if (cmbCikisCihaz.SelectedItem is not CihazListDTO cCihaz)
                        {
                            UiDialog.Warning("Çıkış için cihaz seçin.", "Uyarı");
                            return false;
                        }
                        if (!TryParseTime(GetSaatText(txtCikisSaat), out var cTs))
                        {
                            UiDialog.Warning("Çıkış saati HH:mm veya HH:mm:ss olmalıdır.", "Uyarı");
                            return false;
                        }
                        cikisTs = cTs;
                        cikisCihazId = cCihaz.CihazId;
                    }
                }

                localUpdated = 0;
                localSkipped = 0;

                var aciklamaText = UiFormDialog.GetTextBoxValue(txtAciklama);
                var aciklamaOverride = string.IsNullOrWhiteSpace(aciklamaText)
                    ? null
                    : aciklamaText;

                try
                {
                    foreach (var row in ordered)
                    {
                        var gun = row.Tarih.Date;
                        try
                        {
                            PuantajGunHareketUcu? girisUc = null;
                            PuantajGunHareketUcu? cikisUc = null;

                            if (!manuelSaatModu)
                            {
                                if (girisTs.HasValue && girisCihazId.HasValue)
                                {
                                    girisUc = new PuantajGunHareketUcu
                                    {
                                        CihazId = girisCihazId.Value,
                                        TarihSaat = gun.Add(girisTs.Value)
                                    };
                                }

                                if (cikisTs.HasValue && cikisCihazId.HasValue)
                                {
                                    var cikisDt = gun.Add(cikisTs.Value);
                                    if (girisUc != null && cikisDt <= girisUc.TarihSaat)
                                        cikisDt = cikisDt.AddDays(1);

                                    cikisUc = new PuantajGunHareketUcu
                                    {
                                        CihazId = cikisCihazId.Value,
                                        TarihSaat = cikisDt
                                    };
                                }
                            }

                            // Doluysa şablon; boşsa satırın mevcut açıklaması korunur
                            var aciklama = aciklamaOverride ?? row.Aciklama;

                            psvc.DuzenleGun(
                                firmaId,
                                personelId,
                                gun,
                                girisUc,
                                cikisUc,
                                aciklama,
                                session.AktifKullaniciId,
                                saatOverride,
                                tipOverride);
                            localUpdated++;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"PuantajTopluDuzenleDialog day {gun:yyyy-MM-dd}: {ex}");
                            localSkipped++;
                        }
                    }

                    if (localUpdated == 0)
                    {
                        UiDialog.Error(
                            localSkipped > 0
                                ? "Hiçbir gün güncellenemedi."
                                : "Güncelleme yapılmadı.",
                            "Hata");
                        return false;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    UiDialog.Error("Toplu onaylama başarısız:\n" + ex.Message, "Hata");
                    return false;
                }
            });

        updated = localUpdated;
        skippedFail = localSkipped;
        return ok;
    }

    private static string FormatTipAd(List<PuantajTipDTO> tipler, string? kod)
    {
        if (string.IsNullOrWhiteSpace(kod)) return "—";
        var tipAd = tipler.FirstOrDefault(t =>
            string.Equals(t.Kod, kod, StringComparison.OrdinalIgnoreCase));
        return tipAd != null ? tipAd.AdKod : kod;
    }

    private static bool TryParseDecimalSaat(string? text, out decimal saat)
    {
        saat = 0;
        if (string.IsNullOrWhiteSpace(text) || text.Trim() == "—") return false;
        var t = text.Trim().Replace(',', '.');
        return decimal.TryParse(t, NumberStyles.Number, CultureInfo.InvariantCulture, out saat)
               && saat >= 0 && saat <= 24;
    }

    private static string BuildTarihOzet(IReadOnlyList<PuantajGunSatirDTO> gunler)
    {
        var n = gunler.Count;
        if (n == 1)
            return $"1 gün seçildi — {gunler[0].Tarih.ToString("d MMMM yyyy dddd", Tr)}";

        var ilk = gunler[0].Tarih;
        var son = gunler[n - 1].Tarih;
        if (n <= 5)
        {
            var liste = string.Join(", ", gunler.Select(g => g.Tarih.ToString("d MMM", Tr)));
            return $"{n} gün seçildi — {liste}";
        }

        return $"{n} gün seçildi — {ilk.ToString("d MMM", Tr)} … {son.ToString("d MMM yyyy", Tr)}";
    }

    private static StackPanel BuildSidePanel(
        string title,
        List<CihazListDTO> cihazlar,
        TimeSpan? gridSaat,
        int cihazId,
        out ToggleButton toggle,
        out ComboBox cmbCihaz,
        out TextBox txtSaat)
    {
        var panel = new StackPanel();
        var hasData = gridSaat.HasValue;

        toggle = CreateSwitchToggle(hasData);
        panel.Children.Add(toggle);

        panel.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 15,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 8)
        });

        panel.Children.Add(UiFormDialog.CreateLabel("Cihaz"));
        cmbCihaz = new ComboBox
        {
            ItemsSource = cihazlar,
            DisplayMemberPath = nameof(CihazListDTO.Text),
            FontSize = 14,
            Padding = new Thickness(8, 6, 8, 6),
            Margin = new Thickness(0, 0, 0, 10),
            BorderBrush = ThemeBrushes.Get("Brush.FieldBorder", Color.FromRgb(0xD0, 0xD7, 0xE2))
        };
        if (cihazId >= 0)
            cmbCihaz.SelectedItem = cihazlar.FirstOrDefault(c => c.CihazId == cihazId);
        if (cmbCihaz.SelectedItem == null && cihazlar.Count > 0)
            cmbCihaz.SelectedIndex = 0;
        panel.Children.Add(cmbCihaz);

        panel.Children.Add(UiFormDialog.CreateLabel("Saat"));
        if (hasData)
        {
            var saat = gridSaat!.Value;
            var saatStr = saat.Seconds == 0
                ? saat.ToString(@"hh\:mm", CultureInfo.InvariantCulture)
                : saat.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
            txtSaat = UiFormDialog.CreateTextBox(saatStr);
        }
        else
        {
            txtSaat = UiFormDialog.CreateTextBox(KartBasimiYok);
            SetKartBasimiYok(txtSaat);
        }
        txtSaat.Margin = new Thickness(0, 0, 0, 0);
        txtSaat.MaxLength = 24;
        txtSaat.TextWrapping = TextWrapping.NoWrap;
        panel.Children.Add(txtSaat);

        return panel;
    }

    private static void SetSaltOkunurSaat(TextBox txt, string saat)
    {
        txt.Text = saat;
        txt.FontStyle = FontStyles.Normal;
        txt.Foreground = ThemeBrushes.Get("Brush.TextMuted", Color.FromRgb(0x64, 0x74, 0x8B));
        txt.IsEnabled = false;
        txt.Opacity = 0.9;
    }

    private static void SetKartBasimiYok(TextBox txt)
    {
        txt.Text = KartBasimiYok;
        txt.FontStyle = FontStyles.Italic;
        txt.Foreground = ThemeBrushes.Get("Brush.TextMuted", Color.FromRgb(0x64, 0x74, 0x8B));
        txt.IsEnabled = false;
        txt.Opacity = 0.9;
    }

    private static string GetSaatText(TextBox txt)
    {
        var t = txt.Text?.Trim() ?? "";
        return string.Equals(t, KartBasimiYok, StringComparison.Ordinal) ? "" : t;
    }

    private static bool IsValidTimeText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        if (string.Equals(text.Trim(), KartBasimiYok, StringComparison.Ordinal)) return false;
        return TryParseTime(text, out _);
    }

    private static ToggleButton CreateSwitchToggle(bool isOn)
    {
        const double w = 44, h = 24, knob = 18, pad = 3;

        var track = new Border
        {
            Width = w,
            Height = h,
            CornerRadius = new CornerRadius(h / 2),
            Background = new SolidColorBrush(isOn ? AccentBlue : TrackOff)
        };

        var thumb = new Ellipse
        {
            Width = knob,
            Height = knob,
            Fill = Brushes.White,
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 4,
                ShadowDepth = 1,
                Opacity = 0.25
            },
            HorizontalAlignment = isOn ? HorizontalAlignment.Right : HorizontalAlignment.Left,
            Margin = new Thickness(pad)
        };

        var host = new Grid { Width = w, Height = h };
        host.Children.Add(track);
        host.Children.Add(thumb);

        var tg = new ToggleButton
        {
            Content = host,
            IsChecked = isOn,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
            Margin = new Thickness(0, 0, 0, 8),
            HorizontalAlignment = HorizontalAlignment.Left,
            Cursor = System.Windows.Input.Cursors.Hand,
            Focusable = true,
            ToolTip = isOn ? "Açık" : "Kapalı"
        };

        tg.Template = CreateBareToggleTemplate();

        void SyncVisual()
        {
            var on = tg.IsChecked == true;
            track.Background = new SolidColorBrush(on ? AccentBlue : TrackOff);
            thumb.HorizontalAlignment = on ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            tg.ToolTip = on ? "Açık" : "Kapalı";
        }

        tg.Checked += (_, _) => SyncVisual();
        tg.Unchecked += (_, _) => SyncVisual();
        return tg;
    }

    private static ControlTemplate CreateBareToggleTemplate()
    {
        var template = new ControlTemplate(typeof(ToggleButton));
        var factory = new FrameworkElementFactory(typeof(ContentPresenter));
        factory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        factory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        template.VisualTree = factory;
        return template;
    }

    private static int ResolveCihazId(DateTime? hareketTarih, TimeSpan? gridSaat, int? cihazId)
    {
        if (!TimesMatch(hareketTarih, gridSaat))
            return 0;
        return cihazId ?? 0;
    }

    private static bool TimesMatch(DateTime? hareketTarih, TimeSpan? gridSaat)
    {
        if (!hareketTarih.HasValue || !gridSaat.HasValue) return false;
        return Math.Abs((hareketTarih.Value.TimeOfDay - gridSaat.Value).TotalSeconds) < 2;
    }

    private static bool TryParseTime(string? text, out TimeSpan ts)
    {
        ts = default;
        if (string.IsNullOrWhiteSpace(text)) return false;
        var t = text.Trim();
        if (string.Equals(t, KartBasimiYok, StringComparison.Ordinal)) return false;
        if (TimeSpan.TryParseExact(t, new[] { @"h\:mm", @"hh\:mm", @"h\:mm\:ss", @"hh\:mm\:ss" },
                CultureInfo.InvariantCulture, out ts))
            return true;
        return TimeSpan.TryParse(t, CultureInfo.InvariantCulture, out ts);
    }
}
