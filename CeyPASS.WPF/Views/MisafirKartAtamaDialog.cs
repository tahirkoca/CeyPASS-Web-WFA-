using System.Windows;
using System.Windows.Controls;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Entities.Helpers;

namespace CeyPASS.WPF.Views;

/// <summary>WFA misafirKartAtama karşılığı.</summary>
public static class MisafirKartAtamaDialog
{
    public static void ShowYeni(Window owner, ISessionContext session, IMisafirKartService svc, int firmaId)
    {
        session.AktifFirmaId = firmaId;
        var cards = svc.GetCardsForNew(firmaId) ?? new List<KisiListItem>();

        var root = new Grid { MinWidth = 640 };
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(320) });

        var fields = BuildFormFields(
            cards,
            isGuncelle: false,
            svc);

        var history = new GecmisZiyaretciPanel { Margin = new Thickness(12, 0, 0, 0) };
        history.SetSearchPlaceholder("İsim ara...");
        history.LoadListe(ad => svc.SearchGecmisZiyaretciler(firmaId, ad));
        history.ZiyaretciSecildi += item =>
        {
            if (item == null) return;
            fields.TxtAd.Text = item.AdSoyad ?? "";
            fields.TcField.ShowMasked(item.TCKimlikNo);
            fields.TxtKime.Text = item.ZiyaretEdilenKisi ?? "";
            fields.DtpGiris.EditValue = DateTime.Now;
        };

        Grid.SetColumn(fields.Panel, 0);
        Grid.SetColumn(history, 1);
        root.Children.Add(fields.Panel);
        root.Children.Add(history);

        if (cards.Count > 0)
            fields.CmbKart.SelectedIndex = 0;

        UiFormDialog.Show(
            title: "Misafir Kart Atama - Yeni",
            subtitle: "T.C. Kimlik No zorunludur (11 hane). Geçmiş ziyaretçiden seçim veya arama ile formu doldurabilirsiniz.",
            body: root,
            owner: owner,
            primaryText: "Kaydet",
            secondaryText: "İptal",
            width: 920,
            validateOnPrimary: () => SaveYeni(session, svc, fields, owner));
    }

    public static void ShowGuncelle(Window owner, ISessionContext session, IMisafirKartService svc, int firmaId)
    {
        session.AktifFirmaId = firmaId;
        var now = DateTime.Now;
        var aktifler = svc.GetTodayActiveAssignments(now, firmaId) ?? new List<PuantajsizKartAtama>();

        if (aktifler.Count == 0)
        {
            UiDialog.Info("Bugün için güncellenecek aktif atama bulunamadı.", "Atanan Kartı Güncelle", owner);
            return;
        }

        var fields = BuildFormFields(aktifler, isGuncelle: true, svc);
        fields.CmbKart.SelectionChanged += (_, _) =>
        {
            if (fields.CmbKart.SelectedItem is PuantajsizKartAtama a)
            {
                fields.TxtAd.Text = a.MisafirAdSoyad ?? "";
                fields.TxtKime.Text = a.ZiyaretEdilenKisi ?? "";
                fields.TcField.ShowMasked(a.TCKimlikNo);
                fields.TxtAciklama.Text = a.Notlar ?? "";
                fields.DtpGiris.EditValue = a.Baslangic;
                fields.DtpCikis.EditValue = DateTime.Now;
            }
        };
        fields.CmbKart.SelectedIndex = 0;

        UiFormDialog.Show(
            title: "Misafir Kart Atama - Güncelleme",
            subtitle: "Bugünkü aktif misafir atamasını güncelleyin.",
            body: fields.Panel,
            owner: owner,
            primaryText: "Kaydet",
            secondaryText: "İptal",
            width: 520,
            validateOnPrimary: () => SaveGuncelle(svc, fields, owner));
    }

    private sealed class MisafirFormFields
    {
        public Grid Panel { get; init; } = null!;
        public ComboBox CmbKart { get; init; } = null!;
        public TextBox TxtAd { get; init; } = null!;
        public CanliIzlemeKartFormHelper.TcField TcField { get; init; } = null!;
        public TextBox TxtKime { get; init; } = null!;
        public DevExpress.Xpf.Editors.DateEdit DtpGiris { get; init; } = null!;
        public DevExpress.Xpf.Editors.DateEdit DtpCikis { get; init; } = null!;
        public TextBox TxtAciklama { get; init; } = null!;
    }

    private static MisafirFormFields BuildFormFields(
        System.Collections.IEnumerable items,
        bool isGuncelle,
        IMisafirKartService svc)
    {
        var grid = CanliIzlemeKartFormHelper.CreateFormGrid();
        var row = 0;

        var cmbKart = CanliIzlemeKartFormHelper.CreateCombo();
        cmbKart.ItemsSource = items;
        if (isGuncelle)
            cmbKart.DisplayMemberPath = nameof(PuantajsizKartAtama.KartAdi);
        else
        {
            cmbKart.DisplayMemberPath = nameof(KisiListItem.AdSoyad);
            cmbKart.SelectedValuePath = nameof(KisiListItem.PersonelId);
        }
        row = CanliIzlemeKartFormHelper.AddFullRow(grid, row, "Atanacak Kart", cmbKart);

        var txtAd = CanliIzlemeKartFormHelper.CreateField();
        row = CanliIzlemeKartFormHelper.AddFullRow(grid, row, "Misafir Adı Soyadı", txtAd);

        var tcBox = CanliIzlemeKartFormHelper.CreateField();
        var tcField = new CanliIzlemeKartFormHelper.TcField(tcBox);
        var txtKime = CanliIzlemeKartFormHelper.CreateField();
        tcField.OnLeave(() => TryFillFromTc(svc, tcField, txtAd, txtKime));
        row = CanliIzlemeKartFormHelper.AddSplitRow(grid, row, "T.C. Kimlik No *", tcBox, "Kime Geldiği", txtKime);

        var dtpGiris = CanliIzlemeKartFormHelper.CreateDateTimeEdit(DateTime.Now);
        var dtpCikis = CanliIzlemeKartFormHelper.CreateDateTimeEdit(DateTime.Now, isGuncelle);
        row = CanliIzlemeKartFormHelper.AddSplitRow(grid, row, "Giriş Saati", dtpGiris, "Çıkış Saati", dtpCikis);

        var txtAciklama = CanliIzlemeKartFormHelper.CreateAciklama();
        CanliIzlemeKartFormHelper.AddFullRow(grid, row, "Açıklama", txtAciklama);

        return new MisafirFormFields
        {
            Panel = grid,
            CmbKart = cmbKart,
            TxtAd = txtAd,
            TcField = tcField,
            TxtKime = txtKime,
            DtpGiris = dtpGiris,
            DtpCikis = dtpCikis,
            TxtAciklama = txtAciklama
        };
    }

    private static void TryFillFromTc(
        IMisafirKartService svc,
        CanliIzlemeKartFormHelper.TcField tcField,
        TextBox txtAd,
        TextBox txtKime)
    {
        var tc = tcField.Box.Text?.Trim();
        if (string.IsNullOrEmpty(tc) || TcKimlikHelper.LooksMasked(tc)) return;

        try
        {
            var rec = svc.GetMisafirBilgisiByTc(tc);
            if (rec == null) return;
            if (!string.IsNullOrEmpty(rec.MisafirAdSoyad) && string.IsNullOrWhiteSpace(txtAd.Text))
                txtAd.Text = rec.MisafirAdSoyad;
            if (!string.IsNullOrEmpty(rec.ZiyaretEdilenKisi) && string.IsNullOrWhiteSpace(txtKime.Text))
                txtKime.Text = rec.ZiyaretEdilenKisi;
        }
        catch
        {
            // WFA ile aynı: sessiz
        }
    }

    private static bool SaveYeni(
        ISessionContext session,
        IMisafirKartService svc,
        MisafirFormFields f,
        Window owner)
    {
        try
        {
            if (f.CmbKart.SelectedValue == null && f.CmbKart.SelectedItem is not KisiListItem)
                throw new InvalidOperationException("Kart seçiniz.");

            var kartId = f.CmbKart.SelectedValue?.ToString()
                         ?? (f.CmbKart.SelectedItem as KisiListItem)?.PersonelId
                         ?? throw new InvalidOperationException("Kart seçiniz.");

            var tc = f.TcField.ResolveForSave();
            var kime = string.IsNullOrWhiteSpace(f.TxtKime.Text) ? null : f.TxtKime.Text.Trim();
            var giris = (DateTime)(f.DtpGiris.EditValue ?? DateTime.Now);

            svc.CreateAssignment(
                (int)session.AktifFirmaId!,
                kartId,
                f.TxtAd.Text,
                giris,
                f.TxtAciklama.Text,
                tc,
                kime ?? "");

            UiDialog.Success("Kayıt başarıyla oluşturuldu.", "Bilgi", owner);
            return true;
        }
        catch (Exception ex)
        {
            UiDialog.Error(ex.Message, "Hata", owner);
            return false;
        }
    }

    private static bool SaveGuncelle(
        IMisafirKartService svc,
        MisafirFormFields f,
        Window owner)
    {
        try
        {
            if (f.CmbKart.SelectedItem is not PuantajsizKartAtama a)
                throw new InvalidOperationException("Güncellenecek atamayı seçiniz.");

            var tc = f.TcField.ResolveForSave();
            var kime = string.IsNullOrWhiteSpace(f.TxtKime.Text) ? null : f.TxtKime.Text.Trim();
            var giris = (DateTime)(f.DtpGiris.EditValue ?? DateTime.Now);
            DateTime? cikis = f.DtpCikis.IsEnabled ? (DateTime?)(f.DtpCikis.EditValue ?? DateTime.Now) : null;

            svc.UpdateAssignment(a.AtamaId, f.TxtAd.Text, giris, cikis, f.TxtAciklama.Text, tc, kime ?? "");
            UiDialog.Success("Kayıt güncellendi.", "Bilgi", owner);
            return true;
        }
        catch (Exception ex)
        {
            UiDialog.Error(ex.Message, "Hata", owner);
            return false;
        }
    }
}
