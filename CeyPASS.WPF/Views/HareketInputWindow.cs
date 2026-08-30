using System.Windows;
using System.Windows.Controls;

namespace CeyPASS.WPF.Views;

/// <summary>
/// Kişi Hareketleri Ekle/Güncelle: UiFormDialog stilinde tarih + tip girişi.
/// </summary>
public static class HareketInputWindow
{
    public static bool Show(Window? owner, DateTime defTarih, string defTip, out DateTime tarih, out string tip)
    {
        var tips = new[] { "Giriş", "Çıkış", "Yemekhane" };
        var fields = new StackPanel();

        fields.Children.Add(UiFormDialog.CreateLabel("Tarih"));
        var dp = UiFormDialog.CreateDatePicker(defTarih.Date);
        fields.Children.Add(dp);

        fields.Children.Add(UiFormDialog.CreateLabel("Saat (HH:mm:ss)"));
        var timeBox = UiFormDialog.CreateTextBox(defTarih.ToString("HH:mm:ss"));
        fields.Children.Add(timeBox);

        fields.Children.Add(UiFormDialog.CreateLabel("Hareket Tipi"));
        var cb = UiFormDialog.CreateComboBox(tips, defTip);
        fields.Children.Add(cb);

        DateTime selectedTarih = defTarih;
        string selectedTip = tips.Contains(defTip) ? defTip : "Giriş";

        bool ok = UiFormDialog.Show(
            title: "Hareket Bilgisi",
            subtitle: "Manuel hareket için tarih ve tipi seçin.",
            body: fields,
            owner: owner,
            primaryText: "Tamam",
            secondaryText: "Vazgeç",
            width: 420,
            validateOnPrimary: () =>
            {
                FieldValidation.SetError(timeBox, null);
                var date = dp.SelectedDate ?? DateTime.Today;
                if (!TimeSpan.TryParse(timeBox.Text.Trim(), out var ts))
                {
                    FieldValidation.SetError(timeBox, "Saat HH:mm:ss formatında olmalıdır.");
                    return false;
                }

                selectedTarih = date.Date.Add(ts);
                selectedTip = cb.SelectedItem as string ?? "Giriş";
                return true;
            });

        if (ok)
        {
            tarih = selectedTarih;
            tip = selectedTip;
            return true;
        }

        tarih = defTarih;
        tip = defTip;
        return false;
    }
}
