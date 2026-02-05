using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CeyPASS.WFA.UserControls
{
    public partial class ucPozisyonTanimla : UserControl
    {
        private readonly ISessionContext _session;
        private readonly IAuthorizationService _auth;
        private readonly IPozisyonService _psvc;
        private enum ScreenMode { List, Add, Edit }
        private ScreenMode _mode = ScreenMode.List;
        AuthorizationHelper authHelp;
        private bool _wired;
        private const string PageName = "Pozisyonlar";
        private const string PageNameUI = "Pozisyonlar";

        public ucPozisyonTanimla(ISessionContext session, IPozisyonService psvc, IAuthorizationService auth)
        {
            InitializeComponent();
            _session = session;
            _auth = auth;
            _psvc = psvc;
            authHelp = new AuthorizationHelper(_session, _auth);
            var cid = Guid.NewGuid().ToString("N");

            LogHelper.Info(PageName, "Init", "ucPozisyonTanimla açıldı", $"{{\"kullaniciId\":{_session.AktifKullaniciId}}}", cid);

            if (!_auth.ViewAbility(PageName))
            {
                LogHelper.Warn(PageName, "View", "Görüntüleme yetkisi yok", null, cid);
                MessageBox.Show("Pozisyonlar ekranını görüntüleme yetkiniz yok");
                this.Visible = false;
                return;
            }

            btnPozisyonEkle.Tag = YetkiTipleri.Create;
            btnPozisyonGuncelle.Tag = YetkiTipleri.Update;
            btnPozisyonSil.Tag = YetkiTipleri.Delete;
            btnKaydet.Tag = YetkiTipleri.Update;

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);

            BeautifyList(chkPozisyonlar);
            WireEventsOnce();
            LoadList();
            EnterListMode();
            AppTheme.ApplyToControl(this);
            LogHelper.Info(PageName, "Init", "Ekran hazır", null, cid);
        }

        private void WireEventsOnce()
        {
            var cid = Guid.NewGuid().ToString("N");
            if (_wired) return;

            btnPozisyonEkle.Click += (s, e) => { LogHelper.Info(PageName, "Click", "PozisyonEkle", null, cid); EnterAddMode(); };
            btnPozisyonGuncelle.Click += (s, e) => { LogHelper.Info(PageName, "Click", "PozisyonGuncelle", null, cid); EnterEditModeFromSelection(); };
            btnPozisyonSil.Click += (s, e) => { LogHelper.Info(PageName, "Click", "PozisyonSil", null, cid); DeleteSelected(); };

            btnKaydet.Click += (s, e) => { LogHelper.Info(PageName, "Click", "Kaydet", $"{{\"mode\":\"{_mode}\"}}", cid); Save(); };
            btnVazgec.Click += (s, e) => { LogHelper.Info(PageName, "Click", "Vazgec", $"{{\"mode\":\"{_mode}\"}}", cid); EnterListMode(); };

            chkPozisyonlar.SelectedIndexChanged += (s, e) =>
            {
                if (_mode == ScreenMode.List) FillInputsFromSelection();
            };

            _wired = true;
        }
        private void BeautifyList(CheckedListBox list)
        {
            list.BorderStyle = BorderStyle.None;
            list.Font = new Font("Segoe UI", 10f);
            list.BackColor = Color.White;
            list.CheckOnClick = false;
        }
        private void LoadList()
        {
            var cid = Guid.NewGuid().ToString("N");
            var list = _psvc.GetAll() ?? new List<LookupItem>();
            chkPozisyonlar.BeginUpdate();
            try
            {
                chkPozisyonlar.Items.Clear();
                foreach (var it in list) chkPozisyonlar.Items.Add(it);
                chkPozisyonlar.DisplayMember = nameof(LookupItem.Ad);
                chkPozisyonlar.ValueMember = nameof(LookupItem.Id);
                if (chkPozisyonlar.Items.Count > 0) chkPozisyonlar.SelectedIndex = 0;
                LogHelper.Info(PageName, "ListeYukle", "Tamamlandı", $"{{\"adet\":{list.Count}}}", cid);
            }
            finally { chkPozisyonlar.EndUpdate(); }

            FillInputsFromSelection();

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }
        private void EnterListMode()
        {
            _mode = ScreenMode.List;

            btnKaydet.Visible = false;
            btnVazgec.Visible = false;

            btnPozisyonEkle.Enabled = true;
            bool hasSel = chkPozisyonlar.SelectedItem != null;
            btnPozisyonGuncelle.Enabled = hasSel;
            btnPozisyonSil.Enabled = hasSel;

            txtPozisyonId.ReadOnly = true;
            txtPozisyonAdi.ReadOnly = true;
            txtPozisyonAciklama.ReadOnly = true;

            FillInputsFromSelection();
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }
        private void EnterAddMode()
        {
            if (!_auth.Can(PageName, YetkiTipleri.Create)) { System.Media.SystemSounds.Beep.Play(); return; }

            _mode = ScreenMode.Add;

            btnKaydet.Visible = true;
            btnVazgec.Visible = true;

            btnPozisyonEkle.Enabled = false;
            btnPozisyonGuncelle.Enabled = false;
            btnPozisyonSil.Enabled = false;

            txtPozisyonId.Text = "";
            txtPozisyonAdi.Text = "";
            txtPozisyonAciklama.Text = "";

            txtPozisyonAdi.ReadOnly = false;
            txtPozisyonAciklama.ReadOnly = false;
            txtPozisyonAdi.Focus();

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }
        private void EnterEditModeFromSelection()
        {
            var cid = Guid.NewGuid().ToString("N");
            if (!_auth.Can(PageName, YetkiTipleri.Update)) { System.Media.SystemSounds.Beep.Play(); return; }

            var it = chkPozisyonlar.SelectedItem as LookupItem;
            if (it == null) return;

            _mode = ScreenMode.Edit;

            btnKaydet.Visible = true;
            btnVazgec.Visible = true;

            btnPozisyonEkle.Enabled = false;
            btnPozisyonGuncelle.Enabled = false;
            btnPozisyonSil.Enabled = false;

            var data = _psvc.GetForEdit(it.Id);
            if (data.HasValue)
            {
                txtPozisyonId.Text = data.Value.id.ToString();
                txtPozisyonAdi.Text = data.Value.ad;
                txtPozisyonAciklama.Text = data.Value.ack;
                LogHelper.Info(PageName, "EditDataYukle", "Veri yüklendi",
                               $"{{\"id\":{data.Value.id},\"ad\":\"{data.Value.ad}\"}}", cid);
            }

            txtPozisyonAdi.ReadOnly = false;
            txtPozisyonAciklama.ReadOnly = false;
            txtPozisyonAdi.Focus();
            txtPozisyonAdi.SelectAll();

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }
        private void FillInputsFromSelection()
        {
            var cid = Guid.NewGuid().ToString("N");
            var it = chkPozisyonlar.SelectedItem as LookupItem;
            if (it == null)
            {
                txtPozisyonId.Clear();
                txtPozisyonAdi.Clear();
                txtPozisyonAciklama.Clear();
                return;
            }

            txtPozisyonId.Text = it.Id.ToString();

            var data = _psvc.GetForEdit(it.Id);
            txtPozisyonAdi.Text = data?.ad ?? it.Ad;
            txtPozisyonAciklama.Text = data?.ack ?? "";
            LogHelper.Info(PageName, "FillInputs", "Alanlar dolduruldu",
                           $"{{\"id\":{it.Id},\"ad\":\"{txtPozisyonAdi.Text}\"}}", cid);
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }
        private void Save()
        {
            var cid = Guid.NewGuid().ToString("N");
            string ad = (txtPozisyonAdi.Text ?? "").Trim();
            string ack = (txtPozisyonAciklama.Text ?? "").Trim();

            if (_mode == ScreenMode.Add && !_auth.Can(PageName, YetkiTipleri.Create))
            { System.Media.SystemSounds.Beep.Play(); return; }

            if (_mode == ScreenMode.Edit && !_auth.Can(PageName, YetkiTipleri.Update))
            { System.Media.SystemSounds.Beep.Play(); return; }

            if (string.IsNullOrWhiteSpace(ad))
            {
                MessageBox.Show("Pozisyon adı boş olamaz.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPozisyonAdi.Focus();
                return;
            }

            bool ok;
            if (_mode == ScreenMode.Add)
            {
                ok = _psvc.Add(ad, ack);
            }
            else if (_mode == ScreenMode.Edit)
            {
                int id;
                if (!int.TryParse(txtPozisyonId.Text, out id))
                {
                    LogHelper.Warn(PageName, "Kaydet", "Geçersiz ID", $"{{\"text\":\"{txtPozisyonId.Text}\"}}", cid);
                    MessageBox.Show("Geçersiz ID.", "Uyarı");
                    return;
                }
                LogHelper.Info(PageName, "Kaydet", "Güncelle başlıyor",
                              $"{{\"id\":{id},\"ad\":\"{ad}\"}}", cid);
                ok = _psvc.Update(id, ad, ack);
            }
            else return;

            if (!ok)
            {
                MessageBox.Show("İşlem başarısız.", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LogHelper.Info(PageName, "Kaydet", "Tamamlandı",
                           $"{{\"mode\":\"{_mode}\",\"ad\":\"{ad}\"}}", cid);

            MessageBox.Show("Kayıt tamamlandı.", "Bilgi",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            LoadList();
            EnterListMode();
        }
        private void DeleteSelected()
        {
            var cid = Guid.NewGuid().ToString("N");
            if (!_auth.Can(PageName, YetkiTipleri.Delete)) { System.Media.SystemSounds.Beep.Play(); return; }

            var it = chkPozisyonlar.SelectedItem as LookupItem;
            if (it == null) return;

            if (MessageBox.Show($"“{it.Ad}” silinsin mi?",
                "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                LogHelper.Info(PageName, "Sil", "Kullanıcı iptal etti",
                               $"{{\"id\":{it.Id}}}", cid);
                return;
            }


            if (!_psvc.Delete(it.Id))
            {
                MessageBox.Show("Silme başarısız. Kayıt başka tablolar tarafından kullanılıyor olabilir.",
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            LogHelper.Info(PageName, "Sil", "Silindi",
                          $"{{\"id\":{it.Id}}}", cid);
            LoadList();
            EnterListMode();
        }
    }
}

