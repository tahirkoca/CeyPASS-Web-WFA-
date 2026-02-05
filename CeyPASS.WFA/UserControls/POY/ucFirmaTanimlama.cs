using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using System;
using System.Linq;
using System.Windows.Forms;

namespace CeyPASS.WFA.UserControls
{
    public partial class ucFirmaTanimlama : UserControl
    {
        private enum ScreenMode { List, Add, Edit }
        private readonly ISessionContext _session;
        private readonly IFirmaService _fsvc;
        private readonly IAuthorizationService _auth;
        private ScreenMode _mode = ScreenMode.List;
        private bool _wired = false;  
        AuthorizationHelper authHelp;
        private const string PageName = "Firmalar";
        private const string PageNameUI = "Firmalar";

        public ucFirmaTanimlama(ISessionContext session,IFirmaService fsvc,IAuthorizationService auth)
        {
            InitializeComponent();
            _session = session;
            _fsvc = fsvc;
            _auth = auth;
            authHelp = new AuthorizationHelper(_session, _auth);
            if (!_auth.ViewAbility(PageName))
            {
                LogHelper.Warn(PageName, "View", "Firmalar ekranını görüntüleme yetkisi yok");
                MessageBox.Show("Firmalar ekranını görüntüleme yetkiniz yok");
                this.Visible = false;
                return;
            }
            AppTheme.ApplyToControl(this);
            LogHelper.Info(PageName, "Open", "Firmalar ekranı açıldı",
                           detayJson: $"{{\"KullaniciId\":{_session.AktifKullaniciId}}}");

            btnFirmaEkle.Tag = YetkiTipleri.Create;
            btnFirmaGuncelle.Tag = YetkiTipleri.Update;
            btnFirmaSil.Tag = YetkiTipleri.Delete;
            btnKaydet.Tag = YetkiTipleri.Create;
            btnVazgec.Tag = 0;

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);

            WireEventsOnce();
            EnterListMode();
            LoadList();
            BeautifyList(chkFirmalar);
        }

        private void WireEventsOnce()
        {
            if (_wired) return;

            btnFirmaEkle.Click += (s, e) => EnterAddMode();
            btnFirmaGuncelle.Click += (s, e) => EnterEditModeFromSelection();
            btnFirmaSil.Click += (s, e) => DeleteSelected();

            btnKaydet.Click += (s, e) => Save();
            btnVazgec.Click += (s, e) => EnterListMode();

            chkFirmalar.SelectedIndexChanged += (s, e) =>
            {
                if (_mode == ScreenMode.List) FillInputsFromSelection();
            };

            _wired = true;
        }
        private void LoadList()
        {
            var list = _fsvc.GetLookup();

            chkFirmalar.BeginUpdate();
            try
            {
                chkFirmalar.Items.Clear();
                foreach (var it in list)
                    chkFirmalar.Items.Add(it);
                if (chkFirmalar.Items.Count > 0)
                    chkFirmalar.SelectedIndex = 0;
            }
            finally { chkFirmalar.EndUpdate(); }

            FillInputsFromSelection();

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
            UpdateToolbar();
            LogHelper.Info(PageName, "Load", "Firma listesi yüklendi",
                   detayJson: $"{{\"KayitSayisi\":{chkFirmalar.Items.Count}}}");
        }
        private void EnterListMode()
        {
            _mode = ScreenMode.List;
            LogHelper.Info(PageName, "Mode", "Liste moduna geçildi");
            btnKaydet.Visible = false;
            btnVazgec.Visible = false;

            btnFirmaEkle.Enabled = true;
            btnFirmaGuncelle.Enabled = (chkFirmalar.SelectedItem != null);
            btnFirmaSil.Enabled = btnFirmaGuncelle.Enabled;

            txtFirmaId.ReadOnly = true;
            txtFirmaAdi.ReadOnly = true;
            txtITMail.ReadOnly = true;

            FillInputsFromSelection();

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
            UpdateToolbar();
        }
        private void EnterAddMode()
        {
            if (!_auth.Can(PageName, YetkiTipleri.Create)) 
            {
                LogHelper.Warn(PageName, "ActionDenied", "Firma ekleme yetkisi yok",
                       detayJson: "{\"Yetki\":\"Create\"}");
                System.Media.SystemSounds.Beep.Play(); 
                return; 
            }

            LogHelper.Info(PageName, "Create", "Yeni firma ekleme modu açıldı");

            _mode = ScreenMode.Add;

            btnKaydet.Visible = true;
            btnVazgec.Visible = true;

            btnFirmaEkle.Enabled = false;
            btnFirmaGuncelle.Enabled = false;
            btnFirmaSil.Enabled = false;

            txtFirmaId.ReadOnly = false;
            txtFirmaAdi.ReadOnly = false;
            txtITMail.ReadOnly = false;

            txtFirmaId.Text = _fsvc.SuggestNextId().ToString();
            txtFirmaAdi.Clear();
            txtITMail.Clear();

            txtFirmaAdi.Focus();
            txtFirmaAdi.SelectAll();

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
            UpdateToolbar();
        }
        private void EnterEditModeFromSelection()
        {
            if (!_auth.Can(PageName, YetkiTipleri.Update)) 
            {
                LogHelper.Warn(PageName, "ActionDenied", "Firma güncelleme yetkisi yok",
                       detayJson: "{\"Yetki\":\"Update\"}");
                System.Media.SystemSounds.Beep.Play();
                return; 
            }

            var sel = chkFirmalar.SelectedItem as LookupItem;
            if (sel == null) 
            {
                LogHelper.Warn(PageName, "Edit", "Düzenlenecek firma seçilmedi");
                return;
            }

            LogHelper.Info(PageName, "Edit", "Firma düzenleme modu açıldı",
                 detayJson: $"{{\"FirmaId\":{sel.Id}}}");

            _mode = ScreenMode.Edit;

            btnKaydet.Visible = true;
            btnVazgec.Visible = true;

            btnFirmaEkle.Enabled = false;
            btnFirmaGuncelle.Enabled = false;
            btnFirmaSil.Enabled = false;

            txtFirmaId.ReadOnly = true;
            txtFirmaAdi.ReadOnly = false;
            txtITMail.ReadOnly = false;

            FillInputsFromSelection();
            txtFirmaAdi.Focus();
            txtFirmaAdi.SelectAll();

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
            UpdateToolbar();
        }
        private void FillInputsFromSelection()
        {
            var it = chkFirmalar.SelectedItem as LookupItem;
            if (it == null)
            {
                txtFirmaId.Clear();
                txtFirmaAdi.Clear();
                txtITMail.Clear();
                return;
            }

            var full = _fsvc.GetAll().FirstOrDefault(x => x.FirmaId == it.Id);
            if (full == null) return;

            txtFirmaId.Text = full.FirmaId.ToString();
            txtFirmaAdi.Text = full.FirmaAdi;
            txtITMail.Text = full.ITBirimMail;
        }
        private void Save()
        {
            try
            {
                if (_mode == ScreenMode.Add)
                {
                    if (!_auth.Can(PageName, YetkiTipleri.Create))
                    {
                        LogHelper.Warn(PageName, "ActionDenied", "Firma ekleme yetkisi yok");
                        System.Media.SystemSounds.Beep.Play();
                        return;
                    }
                }
                else if (_mode == ScreenMode.Edit)
                {
                    if (!_auth.Can(PageName, YetkiTipleri.Update))
                    {
                        LogHelper.Warn(PageName, "ActionDenied", "Firma güncelleme yetkisi yok");
                        System.Media.SystemSounds.Beep.Play();
                        return;
                    }
                }

                int id;
                if (!int.TryParse(txtFirmaId.Text, out id))
                {
                    LogHelper.Warn(PageName, "Validate", "Geçersiz FirmaId");
                    MessageBox.Show("Geçerli bir Firma Id girin.");
                    txtFirmaId.Focus();
                    return;
                }

                string ad = txtFirmaAdi.Text?.Trim();
                string mail = txtITMail.Text?.Trim();

                if (string.IsNullOrWhiteSpace(ad))
                {
                    LogHelper.Warn(PageName, "Validate", "Firma adı boş");
                    MessageBox.Show("Firma adı boş olamaz.");
                    return;
                }

                LogHelper.Info(PageName, _mode.ToString(), "Firma kaydediliyor",
                               detayJson: $"{{\"FirmaId\":{id},\"Ad\":\"{ad}\",\"Mail\":\"{mail}\"}}");

                bool ok;
                string msg;
                if (_mode == ScreenMode.Add)
                    ok = _fsvc.Add(id, ad, mail, out msg);
                else if (_mode == ScreenMode.Edit)
                    ok = _fsvc.Update(id, ad, mail, out msg);
                else return;

                if (ok)
                {
                    LogHelper.Info(PageName, _mode.ToString(), "Firma kaydı tamamlandı",
                                   detayJson: $"{{\"FirmaId\":{id}}}");
                    MessageBox.Show("Kayıt tamamlandı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadList();
                    EnterListMode();
                }
                else
                {
                    LogHelper.Warn(PageName, _mode.ToString(), "Firma kaydı başarısız",
                                   detayJson: $"{{\"FirmaId\":{id},\"Mesaj\":\"{msg}\"}}");
                    MessageBox.Show(msg ?? "İşlem başarısız.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                LoadList();
                EnterListMode();
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "Save", "Firma kaydedilirken hata", ex);
                throw;
            }
        }
        private void DeleteSelected()
        {
            if (!_auth.Can(PageName, YetkiTipleri.Delete)) 
            {
                LogHelper.Warn(PageName, "ActionDenied", "Firma silme yetkisi yok",
                       detayJson: "{\"Yetki\":\"Delete\"}");
                System.Media.SystemSounds.Beep.Play(); 
                return; 
            }

            var it = chkFirmalar.SelectedItem as LookupItem;
            if (it == null) 
            {
                LogHelper.Warn(PageName, "Delete", "Silinecek firma seçilmedi");
                return;
            }

            if (MessageBox.Show($"“{it.Ad}” firmasını silmek istiyor musunuz?",
                "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) 
            {
                LogHelper.Info(PageName, "Delete", "Kullanıcı silmeyi iptal etti",
                       detayJson: $"{{\"FirmaId\":{it.Id}}}");
                return;
            }

            try
            {
                LogHelper.Info(PageName, "Delete", "Firma silme işlemi başlatıldı",
                       detayJson: $"{{\"FirmaId\":{it.Id}}}");
                if (!_fsvc.Delete(it.Id))
                {
                    LogHelper.Warn(PageName, "Delete", "Firma silme başarısız (ilişkili kayıt)",
                           detayJson: $"{{\"FirmaId\":{it.Id}}}");
                    MessageBox.Show("Silme işlemi başarısız. Kayıt ilişkili olabilir.",
                                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                LogHelper.Info(PageName, "Delete", "Firma silindi",
                       detayJson: $"{{\"FirmaId\":{it.Id}}}");

                LoadList();
                EnterListMode();
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "Delete", "Firma silme sırasında hata", ex);
                throw;
            }     
        }
        private void BeautifyList(CheckedListBox list)
        {
            list.BorderStyle = BorderStyle.None;
            list.Font = new System.Drawing.Font("Segoe UI", 10f);
            list.BackColor = System.Drawing.Color.White;
            list.CheckOnClick = false;
        }
        private void UpdateToolbar()
        {
            bool hasSel = chkFirmalar.SelectedItem != null;
            if (_mode == ScreenMode.List)
            {
                btnFirmaEkle.Enabled = _auth.Can(PageName, YetkiTipleri.Create);
                btnFirmaGuncelle.Enabled = hasSel && _auth.Can(PageName, YetkiTipleri.Update);
                btnFirmaSil.Enabled = hasSel && _auth.Can(PageName, YetkiTipleri.Delete);
            }

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }
    }
}
