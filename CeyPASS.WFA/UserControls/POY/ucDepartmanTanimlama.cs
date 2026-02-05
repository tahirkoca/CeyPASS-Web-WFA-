using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CeyPASS.WFA.UserControls
{
    public partial class ucDepartmanTanimlama : UserControl
    {
        private enum ScreenMode { List, Add, Edit }
        private readonly ISessionContext _session;
        private readonly IDepartmanService _dsvc;
        private readonly IAuthorizationService _auth;
        private ScreenMode _mode = ScreenMode.List;    
        AuthorizationHelper authHelp;
        private bool _wired;
        private const string PageName = "Departmanlar";
        private const string PageNameUI = "Departmanlar";

        public ucDepartmanTanimlama(ISessionContext session,IDepartmanService dsvc,IAuthorizationService auth)
        {
            InitializeComponent();
            _session = session;
            _dsvc = dsvc;
            _auth = auth;
            authHelp = new AuthorizationHelper(_session, _auth);
            if (!_auth.ViewAbility(PageName))
            {
                LogHelper.Warn(PageName, "View", "Departmanlar ekranını görüntüleme yetkisi yok");
                MessageBox.Show("Departmanlar ekranını görüntüleme yetkiniz yok");
                this.Visible = false;
                return;
            }

            btnDepartmanEkle.Tag = YetkiTipleri.Create;
            btnDepartmanGuncelle.Tag = YetkiTipleri.Update;
            btnDepartmanSil.Tag = YetkiTipleri.Delete;
            btnKaydet.Tag = YetkiTipleri.Create;
            btnVazgec.Tag = 0;

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);

            WireEventsOnce();
            BeautifyList(chkDepartmanlar);
            LoadList();
            EnterListMode();
            AppTheme.ApplyToControl(this);
            LogHelper.Info(PageName, "Open", "Departmanlar ekranı açıldı",detayJson: $"{{\"KullaniciId\":{_session.AktifKullaniciId}}}");
        }

        private void WireEventsOnce()
        {
            if (_wired) return;
            btnDepartmanEkle.Click += (s, e) => EnterAddMode();
            btnDepartmanGuncelle.Click += (s, e) => EnterEditModeFromSelection();
            btnDepartmanSil.Click += (s, e) => DeleteSelected();

            btnKaydet.Click += (s, e) => Save();
            btnVazgec.Click += (s, e) => EnterListMode();

            chkDepartmanlar.SelectedIndexChanged += (s, e) =>
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
            var list = _dsvc.GetAll() ?? new List<LookupItem>();
            chkDepartmanlar.BeginUpdate();
            try
            {
                chkDepartmanlar.Items.Clear();
                foreach (var it in list)
                    chkDepartmanlar.Items.Add(it);

                chkDepartmanlar.DisplayMember = nameof(LookupItem.Ad);
                chkDepartmanlar.ValueMember = nameof(LookupItem.Id);

                if (chkDepartmanlar.Items.Count > 0)
                    chkDepartmanlar.SelectedIndex = 0;
            }
            finally { chkDepartmanlar.EndUpdate(); }

            FillInputsFromSelection();

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
            UpdateToolbarState();
            LogHelper.Info(PageName, "Load", "Departman listesi yüklendi",detayJson: $"{{\"KayitSayisi\":{chkDepartmanlar.Items.Count}}}");
        }
        private void UpdateToolbarState()
        {
            bool canEdit = (_mode == ScreenMode.List) &&
                           (chkDepartmanlar.Items.Count > 0) &&
                           (chkDepartmanlar.SelectedItem != null);

            btnDepartmanGuncelle.Enabled = canEdit && _auth.Can(PageName, YetkiTipleri.Update);
            btnDepartmanSil.Enabled = canEdit && _auth.Can(PageName, YetkiTipleri.Delete);
            btnDepartmanEkle.Enabled = _mode == ScreenMode.List && _auth.Can(PageName, YetkiTipleri.Create);
        }
        private void EnterListMode()
        {
            _mode = ScreenMode.List;

            btnKaydet.Visible = false;
            btnVazgec.Visible = false;

            btnDepartmanEkle.Enabled = true;
            btnDepartmanGuncelle.Enabled = (chkDepartmanlar.SelectedItem != null);
            btnDepartmanSil.Enabled = btnDepartmanGuncelle.Enabled;

            txtDepartmanId.ReadOnly = true;
            txtDepartmanAdi.ReadOnly = true;
            txtDepartmanAciklama.ReadOnly = true;

            FillInputsFromSelection();
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
            UpdateToolbarState();
        }
        private void EnterAddMode()
        {
            if (!_auth.Can(PageName, YetkiTipleri.Create)) 
            {
                LogHelper.Warn(PageName, "ActionDenied", "Departman ekleme yetkisi yok",detayJson: "{\"Yetki\":\"Create\"}");
                System.Media.SystemSounds.Beep.Play(); 
                return; 
            }
            LogHelper.Info(PageName, "Create", "Yeni departman ekleme modu açıldı");
            _mode = ScreenMode.Add;

            btnKaydet.Visible = true;
            btnVazgec.Visible = true;

            btnDepartmanEkle.Enabled = false;
            btnDepartmanGuncelle.Enabled = false;
            btnDepartmanSil.Enabled = false;

            txtDepartmanId.ReadOnly = false;
            txtDepartmanAdi.ReadOnly = false;
            txtDepartmanAciklama.ReadOnly = false;

            txtDepartmanId.Text = _dsvc.GetNextId().ToString();
            txtDepartmanAdi.Clear();
            txtDepartmanAciklama.Clear();

            txtDepartmanAdi.Focus();

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
            UpdateToolbarState();
        }
        private void EnterEditModeFromSelection()
        {
            if (!_auth.Can(PageName, YetkiTipleri.Update)) 
            {
                LogHelper.Warn(PageName, "ActionDenied", "Departman güncelleme yetkisi yok",detayJson: "{\"Yetki\":\"Update\"}");
                System.Media.SystemSounds.Beep.Play();
                return; 
            }

            var sel = chkDepartmanlar.SelectedItem as LookupItem;
            if (sel == null) 
            {
                LogHelper.Warn(PageName, "Edit", "Seçim bulunamadı");
                return;
            } 
            LogHelper.Info(PageName, "Edit", "Departman düzenleme modu açıldı",detayJson: $"{{\"DepartmanId\":{sel.Id}}}");
            _mode = ScreenMode.Edit;

            btnKaydet.Visible = true;
            btnVazgec.Visible = true;

            btnDepartmanEkle.Enabled = false;
            btnDepartmanGuncelle.Enabled = false;
            btnDepartmanSil.Enabled = false;

            txtDepartmanId.ReadOnly = true;
            txtDepartmanAdi.ReadOnly = false;
            txtDepartmanAciklama.ReadOnly = false;

            FillInputsFromSelection();

            txtDepartmanAdi.Focus();
            txtDepartmanAdi.SelectAll();

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
            UpdateToolbarState();
        }
        private void FillInputsFromSelection()
        {
            var it = chkDepartmanlar.SelectedItem as LookupItem;
            if (it == null)
            {
                txtDepartmanId.Clear();
                txtDepartmanAdi.Clear();
                txtDepartmanAciklama.Clear();
                return;
            }
            txtDepartmanId.Text = it.Id.ToString();
            var row = _dsvc.GetRowById(it.Id);
            txtDepartmanAdi.Text = row?["DepartmanAdi"]?.ToString() ?? "";
            txtDepartmanAciklama.Text = row?["Aciklama"]?.ToString() ?? "";
        }
        private void Save()
        {
            try
            {
                if (_mode == ScreenMode.Add)
                {
                    if (!_auth.Can(PageName, YetkiTipleri.Create)) 
                    {
                        LogHelper.Warn(PageName, "ActionDenied", "Departman ekleme yetkisi yok",detayJson: "{\"Yetki\":\"Create\"}");
                        System.Media.SystemSounds.Beep.Play(); 
                        return; 
                    }
                }
                else if (_mode == ScreenMode.Edit)
                {
                    if (!_auth.Can(PageName, YetkiTipleri.Update)) 
                    {
                        LogHelper.Warn(PageName, "ActionDenied", "Departman güncelleme yetkisi yok",detayJson: "{\"Yetki\":\"Update\"}");
                        System.Media.SystemSounds.Beep.Play(); 
                        return; 
                    }
                }

                string ad = (txtDepartmanAdi.Text ?? "").Trim();
                string ack = (txtDepartmanAciklama.Text ?? "").Trim();

                if (string.IsNullOrWhiteSpace(ad))
                {
                    LogHelper.Warn(PageName, "Validate", "Departman adı boş");
                    MessageBox.Show("Departman adı boş olamaz.", "Uyarı",MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtDepartmanAdi.Focus();
                    return;
                }

                bool ok;
                if (_mode == ScreenMode.Add)
                {
                    if (!int.TryParse(txtDepartmanId.Text, out var id))
                        id = _dsvc.GetNextId();

                    LogHelper.Info(PageName, "Create", "Departman ekleniyor",detayJson: $"{{\"DepartmanId\":{id},\"Ad\":\"{ad}\"}}");

                    ok = _dsvc.Add(id, ad, ack);
                    if (ok)
                        LogHelper.Info(PageName, "Create", "Departman eklendi",detayJson: $"{{\"DepartmanId\":{id}}}");
                    else
                        LogHelper.Warn(PageName, "Create", "Departman ekleme başarısız",detayJson: $"{{\"Ad\":\"{ad}\"}}");
                }
                else if (_mode == ScreenMode.Edit)
                {
                    if (!int.TryParse(txtDepartmanId.Text, out var id))
                    {
                        LogHelper.Warn(PageName, "Validate", "Geçersiz ID");
                        MessageBox.Show("Geçersiz ID.", "Uyarı");
                        return;
                    }
                    ok = _dsvc.Update(id, ad, ack);
                    if (ok)
                        LogHelper.Info(PageName, "Update", "Departman güncellendi",detayJson: $"{{\"DepartmanId\":{id}}}");
                    else
                        LogHelper.Warn(PageName, "Update", "Departman güncelleme başarısız",detayJson: $"{{\"DepartmanId\":{id}}}");
                }
                else return;

                if (!ok)
                {
                    MessageBox.Show("İşlem başarısız.", "Hata",MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show("Kayıt tamamlandı.", "Bilgi",MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadList();
                EnterListMode();
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "Save", "Departman kaydedilirken hata", ex);
                throw;
            }     
        }
        private void DeleteSelected()
        {
            if (!_auth.Can(PageName, YetkiTipleri.Delete)) 
            {
                LogHelper.Warn(PageName, "ActionDenied", "Departman silme yetkisi yok",detayJson: "{\"Yetki\":\"Delete\"}");
                System.Media.SystemSounds.Beep.Play(); 
                return; 
            }

            var it = chkDepartmanlar.SelectedItem as LookupItem;
            if (it == null) 
            {
                LogHelper.Warn(PageName, "Delete", "Silinecek seçim yok");
                return;
            }

            if (MessageBox.Show($"“{it.Ad}” departmanı silinsin mi?","Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) 
            {
                LogHelper.Info(PageName, "Delete", "Kullanıcı silme işlemini iptal etti",detayJson: $"{{\"DepartmanId\":{it.Id}}}");
                return;
            }

            try
            {
                LogHelper.Info(PageName, "Delete", "Departman silme deneniyor",detayJson: $"{{\"DepartmanId\":{it.Id}}}");
                if (!_dsvc.Delete(it.Id))
                {
                    LogHelper.Warn(PageName, "Delete", "Departman silme başarısız — muhtemelen kullanımda",detayJson: $"{{\"DepartmanId\":{it.Id}}}");
                    MessageBox.Show("Silme işlemi başarısız. Bu departman başka kayıtlar tarafından kullanılıyor olabilir.","Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                LogHelper.Info(PageName, "Delete", "Departman silindi",detayJson: $"{{\"DepartmanId\":{it.Id}}}");
                LoadList();
                EnterListMode();
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "Delete", "Departman silme sırasında hata", ex);
                throw;
            }          
        }
    }
}
