using System.Windows;
using System.Windows.Controls;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Entities.Helpers;

namespace CeyPASS.WPF.Views;

/// <summary>WFA aracKartiAtama karşılığı; plaka zorunlu, T.C./pasaport en az biri.</summary>
public static class AracKartiAtamaDialog
{
    public static void ShowYeni(Window owner, ISessionContext session, IAracKartiService svc, int firmaId, string? preselectPersonelId = null)
    {
        session.AktifFirmaId = firmaId;
        var cards = svc.GetCardsForNew(firmaId) ?? new List<KisiListItem>();

        var root = new Grid { MinWidth = 640 };
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(320) });

        var fields = BuildFormFields(cards, isGuncelle: false, svc);

        var history = new GecmisZiyaretciPanel
        {
            Margin = new Thickness(12, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Stretch
        };
        history.SetSearchPlaceholder("İsim veya plaka ara...");
        history.LoadListe(ad => svc.SearchGecmisZiyaretciler(firmaId, ad));
        // Geçmiş ziyaretçi/araç kaydı forma aktarılır.
        history.ZiyaretciSecildi += item =>
        {
            if (item == null) return;
            fields.TxtAd.Text = item.AdSoyad ?? "";
            fields.TcField.ShowMasked(item.TCKimlikNo);
            fields.TxtPasaport.Text = item.PasaportNo ?? "";
            fields.TxtPlaka.Text = item.Plaka ?? "";
            fields.TxtKime.Text = item.ZiyaretEdilenKisi ?? "";
            fields.TxtAciklama.Text = item.Notlar ?? "";
            fields.DtpGiris.EditValue = DateTime.Now;
        };

        Grid.SetColumn(fields.Panel, 0);
        Grid.SetColumn(history, 1);
        root.Children.Add(fields.Panel);
        root.Children.Add(history);

        SelectKartByPersonelId(fields.CmbKart, cards, preselectPersonelId);

        UiFormDialog.Show(
            title: "Araç Kartı Ver",
            subtitle: "Plaka zorunludur. T.C. veya Pasaport'tan en az biri girilmelidir.",
            body: root,
            owner: owner,
            primaryText: "Kaydet",
            secondaryText: "İptal",
            width: 920,
            validateOnPrimary: () => SaveYeni(session, svc, fields, owner),
            maxBodyHeight: 520);
    }

    public static void ShowGuncelle(Window owner, ISessionContext session, IAracKartiService svc, int firmaId, int? preselectAtamaId = null)
    {
        session.AktifFirmaId = firmaId;
        var aktifler = svc.GetOpenActiveAssignments(firmaId) ?? new List<PuantajsizKartAtama>();

        if (aktifler.Count == 0)
        {
            UiDialog.Info("Güncellenecek aktif atama bulunamadı.", "Araç Kartı Güncelle", owner);
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
                fields.TxtPasaport.Text = a.PasaportNo ?? "";
                fields.TxtPlaka.Text = a.Plaka ?? "";
                fields.TxtAciklama.Text = a.Notlar ?? "";
                fields.DtpGiris.EditValue = a.Baslangic;
                fields.DtpCikis.EditValue = DateTime.Now;
            }
        };
        SelectAtamaById(fields.CmbKart, aktifler, preselectAtamaId);

        UiFormDialog.Show(
            title: "Verilen Araç Kartını Güncelle",
            subtitle: "Aktif araç kartı atamasını güncelleyin.",
            body: fields.Panel,
            owner: owner,
            primaryText: "Kaydet",
            secondaryText: "İptal",
            width: 520,
            validateOnPrimary: () => SaveGuncelle(session, svc, fields, owner));
    }

    private static void SelectKartByPersonelId(ComboBox cmb, List<KisiListItem> cards, string? personelId)
    {
        if (cards.Count == 0) return;
        if (!string.IsNullOrWhiteSpace(personelId))
        {
            var match = cards.FirstOrDefault(c =>
                string.Equals(c.PersonelId?.Trim(), personelId.Trim(), StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                cmb.SelectedItem = match;
                return;
            }
        }
        cmb.SelectedIndex = 0;
    }

    private static void SelectAtamaById(ComboBox cmb, List<PuantajsizKartAtama> aktifler, int? atamaId)
    {
        if (aktifler.Count == 0) return;
        if (atamaId.HasValue)
        {
            var match = aktifler.FirstOrDefault(a => a.AtamaId == atamaId.Value);
            if (match != null)
            {
                cmb.SelectedItem = match;
                return;
            }
        }
        cmb.SelectedIndex = 0;
    }

    private sealed class AracFormFields
    {
        public Grid Panel { get; init; } = null!;
        public ComboBox CmbKart { get; init; } = null!;
        public TextBox TxtAd { get; init; } = null!;
        public CanliIzlemeKartFormHelper.TcField TcField { get; init; } = null!;
        public TextBox TxtPasaport { get; init; } = null!;
        public TextBox TxtPlaka { get; init; } = null!;
        public TextBox TxtKime { get; init; } = null!;
        public DevExpress.Xpf.Editors.DateEdit DtpGiris { get; init; } = null!;
        public DevExpress.Xpf.Editors.DateEdit DtpCikis { get; init; } = null!;
        public TextBox TxtAciklama { get; init; } = null!;
    }

    private static AracFormFields BuildFormFields(
        System.Collections.IEnumerable items,
        bool isGuncelle,
        IAracKartiService svc)
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
        row = CanliIzlemeKartFormHelper.AddFullRow(grid, row, "Ad Soyad", txtAd);

        var tcBox = CanliIzlemeKartFormHelper.CreateField();
        var tcField = new CanliIzlemeKartFormHelper.TcField(tcBox);
        var txtPasaport = CanliIzlemeKartFormHelper.CreateField();
        txtPasaport.MaxLength = 50;
        var txtPlaka = CanliIzlemeKartFormHelper.CreateField();
        var txtKime = CanliIzlemeKartFormHelper.CreateField();
        var txtAciklama = CanliIzlemeKartFormHelper.CreateAciklama();
        tcField.OnLeave(() => TryFillFromTc(svc, tcField, txtAd, txtKime, txtPlaka, txtPasaport, txtAciklama));
        row = CanliIzlemeKartFormHelper.AddSplitRow(grid, row, "T.C. Kimlik No", tcBox, "Pasaport No", txtPasaport);
        row = CanliIzlemeKartFormHelper.AddSplitRow(grid, row, "Plaka *", txtPlaka, "Kime Geldiği", txtKime);

        var dtpGiris = CanliIzlemeKartFormHelper.CreateDateTimeEdit(DateTime.Now);
        var dtpCikis = CanliIzlemeKartFormHelper.CreateDateTimeEdit(DateTime.Now, isGuncelle);
        row = CanliIzlemeKartFormHelper.AddSplitRow(grid, row, "Giriş Saati", dtpGiris, "Çıkış Saati", dtpCikis);

        CanliIzlemeKartFormHelper.AddFullRow(grid, row, "Açıklama", txtAciklama);

        return new AracFormFields
        {
            Panel = grid,
            CmbKart = cmbKart,
            TxtAd = txtAd,
            TcField = tcField,
            TxtPasaport = txtPasaport,
            TxtPlaka = txtPlaka,
            TxtKime = txtKime,
            DtpGiris = dtpGiris,
            DtpCikis = dtpCikis,
            TxtAciklama = txtAciklama
        };
    }

    private static void TryFillFromTc(
        IAracKartiService svc,
        CanliIzlemeKartFormHelper.TcField tcField,
        TextBox txtAd,
        TextBox txtKime,
        TextBox txtPlaka,
        TextBox txtPasaport,
        TextBox txtAciklama)
    {
        var tc = tcField.Box.Text?.Trim();
        if (string.IsNullOrEmpty(tc) || TcKimlikHelper.LooksMasked(tc)) return;

        try
        {
            var rec = svc.GetBilgisiByTc(tc);
            if (rec == null) return;
            if (!string.IsNullOrEmpty(rec.MisafirAdSoyad) && string.IsNullOrWhiteSpace(txtAd.Text))
                txtAd.Text = rec.MisafirAdSoyad;
            if (!string.IsNullOrEmpty(rec.ZiyaretEdilenKisi) && string.IsNullOrWhiteSpace(txtKime.Text))
                txtKime.Text = rec.ZiyaretEdilenKisi;
            if (!string.IsNullOrEmpty(rec.Plaka) && string.IsNullOrWhiteSpace(txtPlaka.Text))
                txtPlaka.Text = rec.Plaka;
            if (!string.IsNullOrEmpty(rec.PasaportNo) && string.IsNullOrWhiteSpace(txtPasaport.Text))
                txtPasaport.Text = rec.PasaportNo;
            if (!string.IsNullOrEmpty(rec.Notlar) && string.IsNullOrWhiteSpace(txtAciklama.Text))
                txtAciklama.Text = rec.Notlar;
        }
        catch
        {
        }
    }

    private static bool SaveYeni(
        ISessionContext session,
        IAracKartiService svc,
        AracFormFields f,
        Window owner)
    {
        try
        {
            var plaka = string.IsNullOrWhiteSpace(f.TxtPlaka.Text) ? null : f.TxtPlaka.Text.Trim();
            if (string.IsNullOrWhiteSpace(plaka))
                throw new InvalidOperationException("Plaka giriniz.");

            if (f.CmbKart.SelectedValue == null && f.CmbKart.SelectedItem is not KisiListItem)
                throw new InvalidOperationException("Kart seçiniz.");

            var kartId = f.CmbKart.SelectedValue?.ToString()
                         ?? (f.CmbKart.SelectedItem as KisiListItem)?.PersonelId
                         ?? throw new InvalidOperationException("Kart seçiniz.");

            var tc = f.TcField.ResolveOptionalForSave();
            var pasaport = string.IsNullOrWhiteSpace(f.TxtPasaport.Text) ? null : f.TxtPasaport.Text.Trim();
            var kime = string.IsNullOrWhiteSpace(f.TxtKime.Text) ? null : f.TxtKime.Text.Trim();
            var giris = (DateTime)(f.DtpGiris.EditValue ?? DateTime.Now);

            svc.CreateAssignment(
                (int)session.AktifFirmaId!,
                kartId,
                f.TxtAd.Text,
                giris,
                f.TxtAciklama.Text,
                tc,
                kime ?? "",
                plaka,
                pasaport);

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
        ISessionContext session,
        IAracKartiService svc,
        AracFormFields f,
        Window owner)
    {
        try
        {
            if (f.CmbKart.SelectedItem is not PuantajsizKartAtama a)
                throw new InvalidOperationException("Güncellenecek atamayı seçiniz.");

            var plaka = string.IsNullOrWhiteSpace(f.TxtPlaka.Text) ? null : f.TxtPlaka.Text.Trim();
            if (string.IsNullOrWhiteSpace(plaka))
                throw new InvalidOperationException("Plaka giriniz.");

            var tc = f.TcField.ResolveOptionalForSave();
            var pasaport = string.IsNullOrWhiteSpace(f.TxtPasaport.Text) ? null : f.TxtPasaport.Text.Trim();
            var kime = string.IsNullOrWhiteSpace(f.TxtKime.Text) ? null : f.TxtKime.Text.Trim();
            var giris = (DateTime)(f.DtpGiris.EditValue ?? DateTime.Now);
            DateTime? cikis = f.DtpCikis.IsEnabled ? (DateTime?)(f.DtpCikis.EditValue ?? DateTime.Now) : null;

            var kisitKaldirildi = svc.UpdateAssignment(a.AtamaId, f.TxtAd.Text, giris, cikis, f.TxtAciklama.Text, tc, kime ?? "", plaka, pasaport, session.AktifKullaniciId);
            UiDialog.Success(KartAtamaMesajlari.Guncellendi(kisitKaldirildi), "Bilgi", owner);
            return true;
        }
        catch (Exception ex)
        {
            UiDialog.Error(ex.Message, "Hata", owner);
            return false;
        }
    }
}
