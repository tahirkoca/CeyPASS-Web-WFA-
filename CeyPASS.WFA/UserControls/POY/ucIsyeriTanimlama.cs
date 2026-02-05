using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace CeyPASS.WFA.UserControls
{
    public partial class ucIsyeriTanimlama : UserControl
    {
        private enum ScreenMode { List, Add, Edit }
        private readonly ISessionContext _session;
        private readonly IIsyeriService _isvc;
        private readonly IAuthorizationService _auth;
        private ScreenMode _mode = ScreenMode.List;     
        AuthorizationHelper authHelp;
        private bool _eventsWired;
        private bool _isLoadingList = false;
        private bool _saving;
        private const string PageName = "Isyerler";
        private const string PageNameUI = "İşyerleri";

        public ucIsyeriTanimlama(ISessionContext session,IIsyeriService isvc,IAuthorizationService auth)
        {
            InitializeComponent();
            _session = session;
            _isvc = isvc;
            _auth = auth;

            authHelp = new AuthorizationHelper(_session, _auth);
            if (!_auth.ViewAbility(PageName))
            {
                LogHelper.Warn(PageName, "View", "İşyerleri ekranını görüntüleme yetkisi yok");
                MessageBox.Show("İşyerleri ekranını görüntüleme yetkiniz yok");
                this.Visible = false;
                return;
            }
            AppTheme.ApplyToControl(this);
            LogHelper.Info(PageName, "Open", "İşyerleri ekranı açıldı",
                   detayJson: $"{{\"KullaniciId\":{_session.AktifKullaniciId}}}");
            btnIsyeriEkle.Tag = YetkiTipleri.Create;
            btnIsyeriGuncelle.Tag = YetkiTipleri.Update;
            btnIsyeriSil.Tag = YetkiTipleri.Delete;
            btnKaydet.Tag = YetkiTipleri.Create;
            btnVazgec.Tag = 0;

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);

            WireEventsOnce();
            Beautify(chkIsyerleri);
            EnterListMode();
            LoadList();
        }

        private void WireEventsOnce()
        {
            if (_eventsWired) return;

            btnIsyeriEkle.Click -= btnIsyeriEkle_Click;
            btnIsyeriGuncelle.Click -= btnIsyeriGuncelle_Click;
            btnIsyeriSil.Click -= btnIsyeriSil_Click;
            btnKaydet.Click -= btnKaydet_Click;
            btnVazgec.Click -= btnVazgec_Click;
            chkIsyerleri.SelectedIndexChanged -= chkIsyerleri_SelectedIndexChanged;

            btnIsyeriEkle.Click += btnIsyeriEkle_Click;
            btnIsyeriGuncelle.Click += btnIsyeriGuncelle_Click;
            btnIsyeriSil.Click += btnIsyeriSil_Click;
            btnKaydet.Click += btnKaydet_Click;
            btnVazgec.Click += btnVazgec_Click;
            chkIsyerleri.SelectedIndexChanged += chkIsyerleri_SelectedIndexChanged;

            _eventsWired = true;
        }
        private void chkIsyerleri_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoadingList || _saving) return;

            if (_mode == ScreenMode.List)
                FillInputsFromSelection();

            UpdateButtons();
        }
        private void EnterListMode()
        {
            _mode = ScreenMode.List;

            btnKaydet.Visible = false;
            btnVazgec.Visible = false;

            btnIsyeriEkle.Enabled = true;
            btnIsyeriGuncelle.Enabled = (chkIsyerleri.SelectedItem != null);
            btnIsyeriSil.Enabled = btnIsyeriGuncelle.Enabled;

            txtFirmaId.ReadOnly = false;
            txtIsyeriId.ReadOnly = true;
            txtIsyeriAdi.ReadOnly = true;

            FillInputsFromSelection();
            UpdateButtons();

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
            UpdateButtons();
        }
        private void EnterAddMode()
        {
            if (!_auth.Can(PageName, YetkiTipleri.Create)) 
            {
                LogHelper.Warn(PageName, "ActionDenied", "İşyeri ekleme yetkisi yok");
                System.Media.SystemSounds.Beep.Play(); 
                return; 
            }
            LogHelper.Info(PageName, "Create", "Yeni işyeri ekleme modu");
            _mode = ScreenMode.Add;

            btnKaydet.Visible = true;
            btnVazgec.Visible = true;

            btnIsyeriEkle.Enabled = false;
            btnIsyeriGuncelle.Enabled = false;
            btnIsyeriSil.Enabled = false;

            txtFirmaId.Enabled = true;
            txtIsyeriId.Enabled = true;

            txtIsyeriId.ReadOnly = false;
            txtIsyeriAdi.ReadOnly = false;
            txtFirmaId.ReadOnly = false;

            txtIsyeriId.Text = "";
            txtIsyeriAdi.Text = "";

            txtIsyeriId.Focus();
            txtIsyeriId.SelectAll();

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
            UpdateButtons();
        }
        private void EnterEditModeFromSelection()
        {
            if (!_auth.Can(PageName, YetkiTipleri.Update)) 
            {
                LogHelper.Warn(PageName, "ActionDenied", "İşyeri güncelleme yetkisi yok");
                System.Media.SystemSounds.Beep.Play(); 
                return; 
            }
            IsyeriItem it = chkIsyerleri.SelectedItem as IsyeriItem;
            if (it == null) 
            {
                LogHelper.Warn(PageName, "Edit", "Düzenlenecek işyeri seçilmedi");
                return; 
            }

            LogHelper.Info(PageName, "Edit", "İşyeri düzenleme modu",
                   detayJson: $"{{\"FirmaId\":{it.FirmaId},\"IsyeriId\":{it.IsyeriId}}}");
            _mode = ScreenMode.Edit;

            btnKaydet.Visible = true;
            btnVazgec.Visible = true;

            btnIsyeriEkle.Enabled = false;
            btnIsyeriGuncelle.Enabled = false;
            btnIsyeriSil.Enabled = false;

            txtIsyeriId.ReadOnly = true;
            txtIsyeriAdi.ReadOnly = false;

            FillInputsFromSelection();
            txtIsyeriAdi.Focus();
            txtIsyeriAdi.SelectAll();

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
            UpdateButtons();
        }
        private void UpdateButtons()
        {
            bool hasSel = (chkIsyerleri.SelectedItem != null) && _mode == ScreenMode.List;

            btnIsyeriEkle.Enabled = _auth.Can(PageName, YetkiTipleri.Create) && _mode == ScreenMode.List;
            btnIsyeriGuncelle.Enabled = hasSel && _auth.Can(PageName, YetkiTipleri.Update);
            btnIsyeriSil.Enabled = hasSel && _auth.Can(PageName, YetkiTipleri.Delete);

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
        }
        private void Beautify(CheckedListBox list)
        {
            list.BorderStyle = BorderStyle.None;
            list.Font = new Font("Segoe UI", 10f);
            list.CheckOnClick = false;
        }
        private static int ToInt(object v, int def = 0)
        {
            return (v == null || v == DBNull.Value) ? def : Convert.ToInt32(v);
        }
        private static string ToStr(object v)
        {
            return (v == null || v == DBNull.Value) ? string.Empty : v.ToString();
        }
        private void LoadList()
        {
            var dt = _isvc.GetAll();
            if (dt == null) dt = new DataTable();

            try
            {
                _isLoadingList = true;
                chkIsyerleri.BeginUpdate();
                chkIsyerleri.Items.Clear();

                foreach (DataRow r in dt.Rows)
                {
                    int fId = ToInt(r["FirmaId"]);
                    int iId = ToInt(r["IsyeriId"]);
                    string ad = ToStr(r["IsyeriAdi"]);
                    if (iId < 0) continue;
                    chkIsyerleri.Items.Add(new IsyeriItem(fId, iId, ad));
                }

                if (chkIsyerleri.Items.Count > 0)
                    chkIsyerleri.SelectedIndex = 0;

                chkIsyerleri.EndUpdate();
                WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
                UpdateButtons();

                LogHelper.Info(PageName, "Load", "İşyeri listesi yüklendi",
                           detayJson: $"{{\"KayitSayisi\":{chkIsyerleri.Items.Count}}}");
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "Load", "İşyeri listesi yüklenirken hata", ex);
                throw;
            }
            finally
            {
                _isLoadingList = false;
            }
        }
        private void FillInputsFromSelection()
        {
            IsyeriItem it = chkIsyerleri.SelectedItem as IsyeriItem;
            if (it == null)
            {
                txtIsyeriId.Text = "";
                txtIsyeriAdi.Text = "";
                return;
            }

            txtFirmaId.Text = it.FirmaId.ToString();
            txtIsyeriId.Text = it.IsyeriId.ToString();
            txtIsyeriAdi.Text = it.Ad ?? string.Empty;
        }
        private bool TryReadInputs(out int firmaId, out int isyeriId, out string ad, out string msg)
        {
            msg = string.Empty;
            ad = (txtIsyeriAdi.Text ?? string.Empty).Trim();

            if (!int.TryParse(txtFirmaId.Text, out firmaId) || firmaId <= 0)
            {
                msg = "Geçerli bir Firma Id giriniz.";
                txtFirmaId.Focus();
                isyeriId = 0;
                return false;
            }

            if (!int.TryParse(txtIsyeriId.Text, out isyeriId) || isyeriId <= 0)
            {
                msg = "Geçerli bir İşyeri Id giriniz.";
                txtIsyeriId.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(ad))
            {
                msg = "İşyeri adı boş bırakılamaz.";
                txtIsyeriAdi.Focus();
                return false;
            }

            return true;
        }
        private void btnIsyeriEkle_Click(object sender, EventArgs e)
        {
            if (!_auth.Can(PageName, YetkiTipleri.Create)) { System.Media.SystemSounds.Beep.Play(); return; }

            EnterAddMode();
        }
        private void btnIsyeriGuncelle_Click(object sender, EventArgs e)
        {
            if (!_auth.Can(PageName, YetkiTipleri.Update)) { System.Media.SystemSounds.Beep.Play(); return; }

            EnterEditModeFromSelection();
        }
        private void btnIsyeriSil_Click(object sender, EventArgs e)
        {
            if (!_auth.Can(PageName, YetkiTipleri.Delete)) { System.Media.SystemSounds.Beep.Play(); return; }

            DeleteSelected();
        }
        private void btnVazgec_Click(object sender, EventArgs e)
        {
            EnterListMode();
        }
        private void Save()
        {
            if (_saving) return;

            ScreenMode originalMode = _mode;

            // Yetki kontrolü
            if (originalMode == ScreenMode.Add && !_auth.Can(PageName, YetkiTipleri.Create))
            {
                LogHelper.Warn(PageName, "ActionDenied", "İşyeri ekleme yetkisi yok");
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            if (originalMode == ScreenMode.Edit && !_auth.Can(PageName, YetkiTipleri.Update))
            {
                LogHelper.Warn(PageName, "ActionDenied", "İşyeri güncelleme yetkisi yok");
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            int firmaId, isyeriId;
            string ad, error;

            if (!TryReadInputs(out firmaId, out isyeriId, out ad, out error))
            {
                LogHelper.Warn(PageName, "Validate", "Girdi doğrulaması başarısız",
                           detayJson: $"{{\"Hata\":\"{error}\"}}");
                MessageBox.Show(error, "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _saving = true;
            bool ok = false;

            try
            {
                LogHelper.Info(PageName, originalMode.ToString(), "İşyeri kaydediliyor",
                           detayJson: $"{{\"FirmaId\":{firmaId},\"IsyeriId\":{isyeriId},\"Ad\":\"{ad}\"}}");

                if (originalMode == ScreenMode.Add)
                {
                    ok = _isvc.AddManual(firmaId, isyeriId, ad);
                }
                else if (originalMode == ScreenMode.Edit)
                {
                    ok = _isvc.Update(firmaId, isyeriId, ad);
                }

                if (!ok)
                {
                    MessageBox.Show("İşlem tamamlanamadı.", "Hata",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                LogHelper.Info(PageName, originalMode.ToString(), "İşyeri kaydı tamamlandı",
                           detayJson: $"{{\"FirmaId\":{firmaId},\"IsyeriId\":{isyeriId}}}");

                MessageBox.Show("Kayıt tamamlandı.", "Bilgi",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "Save", "İşyeri kaydetme sırasında hata", ex);
                MessageBox.Show("İşlem başarısız: " + ex.Message, "Hata",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                _saving = false;
            }

            if (ok)
            {
                EnterListMode();
                LoadList();
                WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
                UpdateButtons();
            }
        }
        private void btnKaydet_Click(object sender, EventArgs e)
        {
            if (_saving) return;
            Save();
        }
        private void DeleteSelected()
        {
            if (!_auth.Can(PageName, YetkiTipleri.Delete)) 
            {
                LogHelper.Warn(PageName, "ActionDenied", "İşyeri silme yetkisi yok");
                System.Media.SystemSounds.Beep.Play(); 
                return; 
            }

            IsyeriItem it = chkIsyerleri.SelectedItem as IsyeriItem;
            if (it == null) 
            {
                LogHelper.Warn(PageName, "Delete", "Silinecek işyeri seçilmedi");
                return;
            }

            if (MessageBox.Show(
                    string.Format("“{0}” kaydını silmek istiyor musunuz?", it.Ad),
                    "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                LogHelper.Info(PageName, "Delete", "Kullanıcı silmeyi iptal etti",
                       detayJson: $"{{\"FirmaId\":{it.FirmaId},\"IsyeriId\":{it.IsyeriId}}}");
                return;
            }

            bool ok = false;
            try
            {
                ok = _isvc.Delete(it.FirmaId, it.IsyeriId);
            }
            catch (Exception ex)
            {
                LogHelper.Warn(PageName, "Delete", "İşyeri silme başarısız (ilişkili kayıt olabilir)",
                           detayJson: $"{{\"FirmaId\":{it.FirmaId},\"IsyeriId\":{it.IsyeriId}}}");
                MessageBox.Show("Silme başarısız: " + ex.Message, "Hata",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!ok)
            {
                MessageBox.Show("Silme işlemi tamamlanamadı. Kayıt başka tablolarca kullanılıyor olabilir.",
                                "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LogHelper.Info(PageName, "Delete", "İşyeri silindi",
                       detayJson: $"{{\"FirmaId\":{it.FirmaId},\"IsyeriId\":{it.IsyeriId}}}");
            LoadList();
            EnterListMode();
        }
    }
}
