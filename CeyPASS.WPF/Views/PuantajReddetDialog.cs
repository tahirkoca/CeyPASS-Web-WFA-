using System.Windows;
using System.Windows.Controls;
using CeyPASS.Entities.Concrete;

namespace CeyPASS.WPF.Views;

/// <summary>WFA reddetmeEkrani karşılığı — ret sebebi girişi.</summary>
public static class PuantajReddetDialog
{
    public static bool Show(PuantajGunSatirDTO model, out string sebep)
    {
        var fields = new StackPanel();
        fields.Children.Add(UiFormDialog.CreateLabel("Ret nedeni"));
        var txt = UiFormDialog.CreateTextBox(model.Aciklama ?? "");
        fields.Children.Add(txt);

        string result = "";
        bool ok = UiFormDialog.Show(
            title: "Reddet",
            subtitle: model.Tarih.ToString("d MMM yyyy dddd", new System.Globalization.CultureInfo("tr-TR")),
            body: fields,
            primaryText: "Kaydet",
            secondaryText: "İptal",
            width: 420,
            validateOnPrimary: () =>
            {
                result = txt.Text?.Trim() ?? "";
                model.Aciklama = result;
                return true;
            });

        sebep = result;
        return ok;
    }
}
