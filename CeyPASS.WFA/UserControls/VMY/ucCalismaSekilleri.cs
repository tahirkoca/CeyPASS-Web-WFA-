using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CeyPASS.WFA.UserControls.VMY
{
    public partial class ucCalismaSekilleri : UserControl
    {
        private enum ScreenMode { List, Add, Edit }
        private readonly ISessionContext _session;
        private readonly ICalismaSekliService _vsvc;
        private readonly IAuthorizationService _auth;
        private ScreenMode _mode = ScreenMode.List;
        private bool _wired = false;
        AuthorizationHelper authHelp;
        private const string PageName = "Vardiyalar";
        private const string PageNameUI = "Vardiyalar";

        /// <summary>
        /// Admin Panel'den açıldığında true; tüm firmaların tüm vardiyaları listelenir.
        /// </summary>
        public bool AdminPanelMode { get; set; }

        public ucCalismaSekilleri(ISessionContext session, ICalismaSekliService vsvc, IAuthorizationService auth)
        {
            InitializeComponent();
            _session = session;
            _vsvc = vsvc;
            _auth = auth;
            authHelp = new AuthorizationHelper(_session, _auth);
            var cid = Guid.NewGuid().ToString("N");
            AppTheme.ApplyToControl(this);
            LogHelper.Info(PageName, "Open", $"Açılış Kullanıcı Id={_session.AktifKullaniciId}, FirmaId={_session.AktifFirmaId}", cid);
            if (!_auth.ViewAbility(PageName))
            {
                LogHelper.Info(PageName, "View", "Görüntüleme yetkisi yok", cid);
                MessageBox.Show("Vardiyalar ekranını görüntüleme yetkiniz yok");
                this.Visible = false;
                return;
            }

            btnVardiyaEkle.Tag = YetkiTipleri.Create;
            btnVardiyaGuncelle.Tag = YetkiTipleri.Update;
            btnVardiyaSil.Tag = YetkiTipleri.Delete;
            btnKaydet.Tag = YetkiTipleri.Create;

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);

            InitTimePickers();
            WireEventsOnce();
            EnterListMode();
            LoadList();
            BeautifyList(chkVardiyalar);
        }

        private void InitTimePickers()
        {
            var pickers = new[]
            {
            dtpVardiyaBaslangicSaati,
            dtpVardiyaBitisSaati,
            dtpVardiyaBaslangicToleransSaati,
            dtpVardiyaBitisToleransSaati,
            dtpYemekAktiflemeSaati
        };

            foreach (var p in pickers)
            {
                p.Format = DateTimePickerFormat.Custom;
                p.CustomFormat = "HH:mm";
                p.ShowUpDown = true;
                p.Value = new DateTime(1900, 1, 1, 9, 0, 0);
            }
        }
        private static TimeSpan TS(DateTimePicker p) => p.Value.TimeOfDay;
        private static void SetTS(DateTimePicker p, TimeSpan ts) => p.Value = new DateTime(1900, 1, 1).Add(ts);
        private void BeautifyList(CheckedListBox list)
        {
            list.BorderStyle = BorderStyle.None;
            list.Font = new Font("Segoe UI", 10f);
            list.BackColor = Color.White;
            list.CheckOnClick = false;
        }
        private void WireEventsOnce()
        {
            if (_wired) return;

            btnVardiyaEkle.Click += (s, e) => EnterAddMode();
            btnVardiyaGuncelle.Click += (s, e) => EnterEditModeFromSelection();
            btnVardiyaSil.Click += (s, e) => DeleteSelected();

            btnKaydet.Click += (s, e) => Save();
            btnVazgec.Click += (s, e) => EnterListMode();

            chkVardiyalar.SelectedIndexChanged += (s, e) =>
            {
                if (_mode != ScreenMode.List) return;
                FillInputsFromSelection();
            };

            _wired = true;
        }
        private void EnterListMode()
        {
            _mode = ScreenMode.List;

            btnKaydet.Visible = false;
            btnVazgec.Visible = false;

            btnVardiyaEkle.Enabled = true;
            btnVardiyaGuncelle.Enabled = chkVardiyalar.SelectedItem != null;
            btnVardiyaSil.Enabled = btnVardiyaGuncelle.Enabled;

            txtVardiyaAdi.ReadOnly = true;
            FillInputsFromSelection();
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }
        private void EnterAddMode()
        {
            if (!_auth.Can(PageName, YetkiTipleri.Create)) { System.Media.SystemSounds.Beep.Play(); return; }

            _mode = ScreenMode.Add;

            btnKaydet.Visible = true;
            btnVazgec.Visible = true;

            btnVardiyaEkle.Enabled = false;
            btnVardiyaGuncelle.Enabled = false;
            btnVardiyaSil.Enabled = false;

            txtVardiyaAdi.ReadOnly = false;
            txtVardiyaAdi.Clear();

            SetTS(dtpVardiyaBaslangicSaati, new TimeSpan(7, 0, 0));
            SetTS(dtpVardiyaBitisSaati, new TimeSpan(15, 0, 0));
            SetTS(dtpVardiyaBaslangicToleransSaati, new TimeSpan(0, 15, 0));
            SetTS(dtpVardiyaBitisToleransSaati, new TimeSpan(0, 15, 0));
            SetTS(dtpYemekAktiflemeSaati, new TimeSpan(17, 0, 0));

            txtVardiyaAdi.Focus();

            btnKaydet.Tag = YetkiTipleri.Create;
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }
        private void EnterEditModeFromSelection()
        {
            if (!_auth.Can(PageName, YetkiTipleri.Update)) { System.Media.SystemSounds.Beep.Play(); return; }
            if (chkVardiyalar.SelectedItem as CalismaSekli == null) return;

            _mode = ScreenMode.Edit;

            btnKaydet.Visible = true;
            btnVazgec.Visible = true;

            btnVardiyaEkle.Enabled = false;
            btnVardiyaGuncelle.Enabled = false;
            btnVardiyaSil.Enabled = false;

            txtVardiyaAdi.ReadOnly = false;
            FillInputsFromSelection();
            txtVardiyaAdi.Focus();
            txtVardiyaAdi.SelectAll();

            btnKaydet.Tag = YetkiTipleri.Update;
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }
        private void LoadList()
        {
            var list = AdminPanelMode
                ? _vsvc.GetAllForAdmin()
                : _vsvc.GetAll((int)_session.AktifFirmaId);

            chkVardiyalar.BeginUpdate();
            try
            {
                chkVardiyalar.Items.Clear();
                foreach (var it in list) chkVardiyalar.Items.Add(it);
                chkVardiyalar.DisplayMember = nameof(CalismaSekli.Ad);
                chkVardiyalar.ValueMember = nameof(CalismaSekli.Id);
                if (chkVardiyalar.Items.Count > 0) chkVardiyalar.SelectedIndex = 0;
            }
            finally { chkVardiyalar.EndUpdate(); }

            FillInputsFromSelection();
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }

        /// <summary>
        /// Admin Panel'de AdminPanelMode set edildikten sonra listeyi filtresiz yeniden yüklemek için.
        /// </summary>
        public void RefreshList()
        {
            LoadList();
        }

        private void FillInputsFromSelection()
        {
            var it = chkVardiyalar.SelectedItem as CalismaSekli;
            if (it == null)
            {
                txtVardiyaAdi.Clear();
                return;
            }

            txtVardiyaAdi.Text = it.Ad;

            SetTS(dtpVardiyaBaslangicSaati, it.Baslangic);
            SetTS(dtpVardiyaBitisSaati, it.Bitis);
            SetTS(dtpVardiyaBaslangicToleransSaati, it.BaslangicTolerans);
            SetTS(dtpVardiyaBitisToleransSaati, it.BitisTolerans);
            SetTS(dtpYemekAktiflemeSaati, it.YemekAktiflestirme);

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }
        private void Save()
        {
            var cid = Guid.NewGuid().ToString("N");
            if (_mode == ScreenMode.Add && !_auth.Can(PageName, YetkiTipleri.Create))
            { System.Media.SystemSounds.Beep.Play(); return; }

            if (_mode == ScreenMode.Edit && !_auth.Can(PageName, YetkiTipleri.Update))
            { System.Media.SystemSounds.Beep.Play(); return; }

            var ad = (txtVardiyaAdi.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(ad))
            {
                MessageBox.Show("Vardiya adı boş bırakılamaz.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtVardiyaAdi.Focus();
                return;
            }

            var x = new CalismaSekli
            {
                FirmaId = (int)_session.AktifFirmaId,
                Ad = ad,
                Baslangic = TS(dtpVardiyaBaslangicSaati),
                Bitis = TS(dtpVardiyaBitisSaati),
                BaslangicTolerans = TS(dtpVardiyaBaslangicToleransSaati),
                BitisTolerans = TS(dtpVardiyaBitisToleransSaati),
                YemekAktiflestirme = TS(dtpYemekAktiflemeSaati)
            };

            bool ok;
            if (_mode == ScreenMode.Add)
            {
                ok = _vsvc.Add(x) > 0;
                LogHelper.Info(PageName, "Save.Add", $"Result={ok}", _session.AktifKullaniciId.ToString(), cid);
            }
            else if (_mode == ScreenMode.Edit && chkVardiyalar.SelectedItem is CalismaSekli cur)
            {
                x.Id = cur.Id;
                ok = _vsvc.Update(x);
                LogHelper.Info(PageName, "Save.Edit", $"Id={x.Id}, Result={ok}", _session.AktifKullaniciId.ToString(), cid);
            }
            else return;

            if (!ok)
            {
                LogHelper.Info(PageName, "Save.Exception", "Kaydetme hatası", _session.AktifKullaniciId.ToString(), cid);
                MessageBox.Show("İşlem başarısız.", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("Kayıt tamamlandı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadList();
            EnterListMode();
        }
        private void DeleteSelected()
        {
            var cid = Guid.NewGuid().ToString("N");
            if (!_auth.Can(PageName, YetkiTipleri.Delete)) { System.Media.SystemSounds.Beep.Play(); return; }
            if (!(chkVardiyalar.SelectedItem is CalismaSekli it)) return;

            if (MessageBox.Show($"“{it.Ad}” silinsin mi?",
                "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                var ok = _vsvc.Delete(it.Id, (int)_session.AktifFirmaId);
                LogHelper.Info(PageName, "DeleteSelected", $"Deleting Id={it.Id}, FirmaId={_session.AktifFirmaId}", _session.AktifKullaniciId.ToString(), cid);
                if (!ok)
                {
                    LogHelper.Info(PageName, "DeleteSelected", "Manager returned false.", _session.AktifKullaniciId.ToString(), cid);
                    MessageBox.Show("Silme işlemi başarısız. Kayıt başka tablolarca kullanılıyor olabilir.",
                        "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "DeleteSelected.Exception", "Seçilen vardiya silinememe hatası", ex, _session.AktifKullaniciId.ToString(), cid);
                MessageBox.Show("Silme işlemi başarısız: " + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LoadList();
            EnterListMode();
        }
    }
}
