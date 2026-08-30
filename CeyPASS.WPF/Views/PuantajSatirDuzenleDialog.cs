using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;

namespace CeyPASS.WPF.Views;

/// <summary>WFA puantajSatirDuzenlemeEkrani karşılığı — DuzenleOnayla çağırır.</summary>
public static class PuantajSatirDuzenleDialog
{
    public static bool Show(
        PuantajGunSatirDTO model,
        int personelId,
        IPuantajService svc,
        ISessionContext session)
    {
        var tipler = svc.GetPuantajTipleri() ?? new List<PuantajTipDTO>();
        var fields = new StackPanel();

        fields.Children.Add(UiFormDialog.CreateLabel("Tarih"));
        fields.Children.Add(new TextBlock
        {
            Text = model.Tarih.ToString("d MMM yyyy dddd", new CultureInfo("tr-TR")),
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 14)
        });

        fields.Children.Add(UiFormDialog.CreateLabel("Çalışma tipi"));
        var cmb = new ComboBox
        {
            ItemsSource = tipler,
            DisplayMemberPath = nameof(PuantajTipDTO.AdKod),
            SelectedValuePath = nameof(PuantajTipDTO.Kod),
            FontSize = 14,
            Padding = new Thickness(8, 6, 8, 6),
            Margin = new Thickness(0, 0, 0, 14)
        };
        if (!string.IsNullOrWhiteSpace(model.CalismaTipi))
            cmb.SelectedValue = model.CalismaTipi;
        else if (tipler.Count > 0)
            cmb.SelectedIndex = 0;
        fields.Children.Add(cmb);

        fields.Children.Add(UiFormDialog.CreateLabel("Çalışma saati"));
        var saatBox = UiFormDialog.CreateTextBox(
            (model.Saat > 0 ? model.Saat : 0M).ToString("0.##", CultureInfo.InvariantCulture));
        fields.Children.Add(saatBox);

        cmb.SelectionChanged += (_, _) =>
        {
            if (cmb.SelectedItem is PuantajTipDTO tip && tip.VarsayilanSaat.HasValue)
                saatBox.Text = tip.VarsayilanSaat.Value.ToString("0.##", CultureInfo.InvariantCulture);
        };

        fields.Children.Add(UiFormDialog.CreateLabel("Açıklama"));
        var aciklamaBox = UiFormDialog.CreateTextBox(model.Aciklama ?? "");
        fields.Children.Add(aciklamaBox);

        return UiFormDialog.Show(
            title: "Puantaj Satır Düzenle",
            subtitle: "Çalışma tipi, saat ve açıklamayı güncelleyin.",
            body: fields,
            primaryText: "Kaydet",
            secondaryText: "İptal",
            width: 440,
            validateOnPrimary: () =>
            {
                if (cmb.SelectedItem is not PuantajTipDTO tip)
                {
                    UiDialog.Warning("Çalışma tipini seçin.", "Uyarı");
                    return false;
                }

                if (!decimal.TryParse(saatBox.Text.Trim().Replace(',', '.'),
                        NumberStyles.Number, CultureInfo.InvariantCulture, out var saat)
                    || saat < 0 || saat > 24)
                {
                    UiDialog.Warning("Çalışma saati 0–24 arasında olmalıdır.", "Uyarı");
                    return false;
                }

                model.CalismaTipi = tip.Kod;
                model.Saat = saat;
                model.Aciklama = aciklamaBox.Text?.Trim() ?? "";
                model.DuzenlenenFMDakika = svc.HesaplaFazlaMesaiDakika(model.CalismaTipi, model.Saat);

                try
                {
                    svc.DuzenleOnayla(
                        personelId,
                        model.Tarih,
                        model.DuzenlenenFMDakika,
                        model.Aciklama,
                        model.CalismaTipi,
                        model.Saat,
                        session.AktifKullaniciId);
                    return true;
                }
                catch (Exception ex)
                {
                    UiDialog.Error("Kaydetme başarısız:\n" + ex.Message, "Hata");
                    return false;
                }
            });
    }
}
