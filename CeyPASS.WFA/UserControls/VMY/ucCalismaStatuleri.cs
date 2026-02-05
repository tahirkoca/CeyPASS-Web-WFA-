using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CeyPASS.WFA.UserControls.VMY
{
    public partial class ucCalismaStatuleri : UserControl
    {
        private enum ScreenMode { List, Add, Edit }
        private readonly ISessionContext _session;
        private readonly ICalismaStatuService _csvc;
        private readonly IAuthorizationService _auth;
        private ScreenMode _mode = ScreenMode.List;
        private bool _wired;
        AuthorizationHelper authHelp;
        private const string PageName = "CalismaStatuleri";
        private const string PageNameUI = "Çalışma Statüleri";

        public ucCalismaStatuleri(ISessionContext session,ICalismaStatuService csvc,IAuthorizationService auth)
        {
            InitializeComponent();
            var cid = Guid.NewGuid().ToString("N");
           _session= session;
            _csvc = csvc;
            _auth = auth;
            authHelp = new AuthorizationHelper(_session, _auth);
            if (!_auth.ViewAbility(PageName))
            {
                LogHelper.Info(PageName, "View", "Görüntüleme yetkisi yok", _session.AktifKullaniciId.ToString(), cid);
                MessageBox.Show("Çalışma Statüleri ekranını görüntüleme yetkiniz yok");
                this.Visible = false;
                return;
            }
            AppTheme.ApplyToControl(this);
            LogHelper.Info(PageName, "Open", $"Başlangıç Kullanıcı Id={_session.AktifKullaniciId}", _session.AktifKullaniciId.ToString(), cid);
            this.Load += UcCalismaStatuleri_Load;
        }

        private void UcCalismaStatuleri_Load(object sender, EventArgs e)
        {
            var cid = Guid.NewGuid().ToString("N");
           

            btnCalismaStatuEkle.Tag = YetkiTipleri.Create;
            btnCalismaStatuGuncelle.Tag = YetkiTipleri.Update;
            btnCalismaStatuSil.Tag = YetkiTipleri.Delete;

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);

            if (!_wired) { WireAll(); _wired = true; }

            BeautifyList(chkCalismaStatuleri);
            LoadList();
            EnterListMode();
            LogHelper.Info(PageName, "Load", "Tamamlandı", _session.AktifKullaniciId.ToString(), cid);
        }
        private void WireAll()
        {
            btnCalismaStatuEkle.Click += (s, e) => EnterAddMode();
            btnCalismaStatuGuncelle.Click += (s, e) => EnterEditModeFromSelection();
            btnCalismaStatuSil.Click += (s, e) => DeleteSelected();
            btnKaydet.Click += (s, e) => Save();
            btnVazgec.Click += (s, e) => EnterListMode();

            chkCalismaStatuleri.SelectedIndexChanged += chkCalismaStatuleri_SelectedIndexChanged;
            chkCalismaStatuleri.ItemCheck += chkCalismaStatuleri_ItemCheck;
        }
        private void EnterListMode()
        {
            _mode = ScreenMode.List;

            btnKaydet.Visible = false;
            btnVazgec.Visible = false;

            btnCalismaStatuEkle.Enabled = true;
            btnCalismaStatuGuncelle.Enabled =
                (chkCalismaStatuleri.Items.Count > 0 && chkCalismaStatuleri.SelectedItem != null);
            btnCalismaStatuSil.Enabled = btnCalismaStatuGuncelle.Enabled;

            txtCalismaStatuId.ReadOnly = true;
            txtCalismaStatuAdi.ReadOnly = true;

            FillInputsFromSelection();
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }
        private void EnterAddMode()
        {
            if (!_auth.Can(PageName, YetkiTipleri.Create)) { System.Media.SystemSounds.Beep.Play(); return; }

            _mode = ScreenMode.Add;

            btnKaydet.Visible = true;
            btnVazgec.Visible = true;

            btnCalismaStatuEkle.Enabled = false;
            btnCalismaStatuGuncelle.Enabled = false;
            btnCalismaStatuSil.Enabled = false;

            int nextId = _csvc.GetNextId();
            txtCalismaStatuId.Text = nextId.ToString();

            txtCalismaStatuId.ReadOnly = true;
            txtCalismaStatuAdi.ReadOnly = false;

            txtCalismaStatuAdi.Clear();
            txtCalismaStatuAdi.Focus();

            btnKaydet.Tag = YetkiTipleri.Create;
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }
        private void EnterEditModeFromSelection()
        {
            if (!_auth.Can(PageName, YetkiTipleri.Update)) { System.Media.SystemSounds.Beep.Play(); return; }

            if (chkCalismaStatuleri.SelectedItem == null) return;

            _mode = ScreenMode.Edit;

            btnKaydet.Visible = true;
            btnVazgec.Visible = true;

            btnCalismaStatuEkle.Enabled = false;
            btnCalismaStatuGuncelle.Enabled = false;
            btnCalismaStatuSil.Enabled = false;

            txtCalismaStatuAdi.ReadOnly = false;
            FillInputsFromSelection();
            txtCalismaStatuAdi.Focus();
            txtCalismaStatuAdi.SelectAll();

            btnKaydet.Tag = YetkiTipleri.Update;
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }
        private void LoadList()
        {
            var list = _csvc.GetAll() ?? new List<LookupItem>();

            chkCalismaStatuleri.BeginUpdate();
            try
            {
                chkCalismaStatuleri.Items.Clear();

                foreach (var it in list)
                    chkCalismaStatuleri.Items.Add(it);

                chkCalismaStatuleri.DisplayMember = nameof(LookupItem.Ad);
                chkCalismaStatuleri.ValueMember = nameof(LookupItem.Id);

                if (chkCalismaStatuleri.Items.Count > 0)
                    chkCalismaStatuleri.SelectedIndex = 0;
            }
            finally { chkCalismaStatuleri.EndUpdate(); }

            FillInputsFromSelection();
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }
        private void chkCalismaStatuleri_SelectedIndexChanged(object sender, EventArgs e) => FillInputsFromSelection();
        private void chkCalismaStatuleri_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            BeginInvoke(new Action(FillInputsFromSelection));
        }
        private void FillInputsFromSelection()
        {
            var item = chkCalismaStatuleri.SelectedItem as LookupItem;
            if (item == null)
            {
                txtCalismaStatuId.Clear();
                txtCalismaStatuAdi.Clear();
                return;
            }

            txtCalismaStatuId.Text = item.Id.ToString();
            txtCalismaStatuAdi.Text = item.Ad;

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }
        private void Save()
        {
            var cid = Guid.NewGuid().ToString("N");
            if (_mode == ScreenMode.Add && !_auth.Can(PageName, YetkiTipleri.Create)) { System.Media.SystemSounds.Beep.Play(); return; }
            if (_mode == ScreenMode.Edit && !_auth.Can(PageName, YetkiTipleri.Update)) { System.Media.SystemSounds.Beep.Play(); return; }

            string ad = (txtCalismaStatuAdi.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(ad))
            {
                MessageBox.Show("Çalışma statü adı boş bırakılamaz.", "Uyarı",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCalismaStatuAdi.Focus();
                return;
            }

            bool ok;
            if (_mode == ScreenMode.Add)
            {
                int id = int.Parse(txtCalismaStatuId.Text);
                ok = _csvc.Add(id, ad);
                LogHelper.Info(PageName, "Save.Add", $"Id={id}, Ok={ok}", _session.AktifKullaniciId.ToString(), cid);
            }
            else if (_mode == ScreenMode.Edit)
            {
                if (!int.TryParse(txtCalismaStatuId.Text, out var id))
                {
                    MessageBox.Show("Geçersiz ID.");
                    return;
                }
                ok = _csvc.Update(id, ad);
                LogHelper.Info(PageName, "Save.Edit", $"Id={id}, Ok={ok}", _session.AktifKullaniciId.ToString(), cid);
            }
            else return;

            if (!ok)
            {
                MessageBox.Show("İşlem başarısız.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("Kayıt tamamlandı.");
            LoadList();
            EnterListMode();
        }
        private void DeleteSelected()
        {
            var cid = Guid.NewGuid().ToString("N");
            if (!_auth.Can(PageName, YetkiTipleri.Delete)) { System.Media.SystemSounds.Beep.Play(); return; }
            var item = chkCalismaStatuleri.SelectedItem as LookupItem;
            if (item == null) return;

            if (MessageBox.Show($"“{item.Ad}” statüsünü silmek istiyor musunuz?","Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            bool ok = _csvc.Delete(item.Id);
            LogHelper.Info(PageName, "DeleteSelected", $"Deleting Id={item.Id}, Ok={ok}", _session.AktifKullaniciId.ToString(), cid);
            if (!ok)
            {
                MessageBox.Show("Silme işlemi başarısız. Bu statü başka kayıtlarca kullanılıyor olabilir.","Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LoadList();
            EnterListMode();
        }
        private void BeautifyList(CheckedListBox list)
        {
            list.BorderStyle = BorderStyle.None;
            list.Font = new Font("Segoe UI", 10f);
            list.BackColor = Color.White;
            list.CheckOnClick = false;
        }
    }
}
