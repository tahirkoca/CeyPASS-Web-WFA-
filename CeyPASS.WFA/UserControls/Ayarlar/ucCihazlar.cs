using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CeyPASS.WFA.UserControls.Ayarlar
{
    public partial class ucCihazlar : UserControl
    {
        private enum ScreenMode { List, Add, Edit }
        private ScreenMode _mode = ScreenMode.List;
        private readonly ISessionContext _session;
        private readonly ICihazService _svc;
        private readonly IAuthorizationService _auth;
        AuthorizationHelper authHelp;
        private bool _wired;
        private const string PageName = "Cihazlar";
        private const string PageNameUI = "Cihazlar";

        /// <summary>
        /// Admin Panel'den açıldığında true; tüm firmaların tüm cihazları (aktif/pasif) listelenir.
        /// </summary>
        public bool AdminPanelMode { get; set; }

        public ucCihazlar(ISessionContext session, ICihazService svc, IAuthorizationService auth)
        {
            InitializeComponent();
            _session = session;
            _svc = svc;
            _auth = auth;
            authHelp = new AuthorizationHelper(_session, _auth);
            BeautifyList(chkCihazlar);
            WireEventsOnce();
            SetupAuthTags();

            if (!_auth.ViewAbility(PageName))
            {
                MessageBox.Show("Cihazlar ekranı için görüntüleme yetkiniz yok");
                LogHelper.Warn(PageName, "View", "Ekrana erişim yetkisi yok");
                this.Visible = false;
                return;
            }
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
            LoadTypes();
            LoadList();
            EnterListMode();
            LogHelper.Info(PageName, "Open", $"Ekran açıldı (FirmaId={(int)_session.AktifFirmaId}).");
        }

        private void ucCihazlar_Load(object sender, EventArgs e)
        {
            AppTheme.ApplyToControl(this);
        }
        private void SetupAuthTags()
        {
            btnCihazEkle.Tag = YetkiTipleri.Create;
            btnCihazGuncelle.Tag = YetkiTipleri.Update;
            btnCihazSil.Tag = YetkiTipleri.Delete;
            btnKaydet.Tag = YetkiTipleri.Create;
        }
        private void WireEventsOnce()
        {
            if (_wired) return;

            btnCihazEkle.Click += (s, e) => EnterAddMode();
            btnCihazGuncelle.Click += (s, e) => EnterEditModeFromSelection();
            btnCihazSil.Click += (s, e) => DeleteSelected();

            btnKaydet.Click += (s, e) => Save();
            btnVazgec.Click += (s, e) => EnterListMode();

            chkCihazlar.SelectedIndexChanged += (s, e) =>
            {
                if (_mode == ScreenMode.List) FillInputsFromSelection();
            };
            chkCihazlar.ItemCheck += (s, e) =>
            {
                chkCihazlar.SelectedIndex = e.Index;
                BeginInvoke(new Action(() =>
                {
                    if (_mode == ScreenMode.List) FillInputsFromSelection();
                }));
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
        private void LoadTypes()
        {
            var tips = _svc.GetCihazTipleri() ?? new List<CihazTip>();
            cmbCihazTipleri.DisplayMember = nameof(CihazTip.TipAdi);
            cmbCihazTipleri.ValueMember = nameof(CihazTip.TipId);
            cmbCihazTipleri.DataSource = tips;
        }
        private void LoadList()
        {
            try
            {
                var raw = AdminPanelMode
                    ? _svc.GetListe(sadeceAktif: false, firmaId: null)
                    : _svc.GetListe(sadeceAktif: true, firmaId: (int)_session.AktifFirmaId);

                chkCihazlar.BeginUpdate();
                try
                {
                    chkCihazlar.Items.Clear();
                    foreach (var x in raw) chkCihazlar.Items.Add(x);
                    if (chkCihazlar.Items.Count > 0) chkCihazlar.SelectedIndex = 0;
                }
                finally { chkCihazlar.EndUpdate(); }

                FillInputsFromSelection();
                LogHelper.Info(PageName, "ListLoad", $"Cihaz listesi yüklendi. Adet={raw?.Count ?? 0}", detayJson: AdminPanelMode ? "{\"AdminPanel\":true}" : $"{{\"FirmaId\":{(int)_session.AktifFirmaId}}}");
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "ListLoad", "Cihaz listesi yüklenirken hata", ex, detayJson: AdminPanelMode ? "{\"AdminPanel\":true}" : $"{{\"FirmaId\":{(int)_session.AktifFirmaId}}}");
                throw;
            }
        }

        /// <summary>
        /// Admin Panel'de AdminPanelMode set edildikten sonra listeyi filtresiz yeniden yüklemek için.
        /// </summary>
        public void RefreshList()
        {
            LoadList();
        }

        private void EnterListMode()
        {
            _mode = ScreenMode.List;

            btnKaydet.Visible = false;
            btnVazgec.Visible = false;

            btnCihazEkle.Enabled = true;
            bool hasSel = chkCihazlar.SelectedItem != null;
            btnCihazGuncelle.Enabled = hasSel;
            btnCihazSil.Enabled = hasSel;

            txtCihazId.ReadOnly = true;
            txtFirmaId.ReadOnly = true;
            txtCihazAdi.ReadOnly = true;
            txtIpAdres.ReadOnly = true;
            txtPort.ReadOnly = true;
            txtAciklama.ReadOnly = true;
            cmbCihazTipleri.Enabled = false;

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);
            FillInputsFromSelection();
            LogHelper.Info(PageName, "EnterMode", "Liste modu");
        }
        private void EnterAddMode()
        {
            if (!_auth.Can(PageName, YetkiTipleri.Create))
            {
                MessageBox.Show("Yeni cihaz ekleme yetkiniz yok.");
                LogHelper.Warn(PageName, "EnterMode", "Add moduna geçilemedi: yetki yok");
                return;
            }

            _mode = ScreenMode.Add;

            btnKaydet.Visible = true;
            btnVazgec.Visible = true;

            btnCihazEkle.Enabled = false;
            btnCihazGuncelle.Enabled = false;
            btnCihazSil.Enabled = false;

            ClearInputs();

            txtFirmaId.ReadOnly = false;
            txtCihazAdi.ReadOnly = false;
            txtIpAdres.ReadOnly = false;
            txtPort.ReadOnly = false;
            txtAciklama.ReadOnly = false;
            cmbCihazTipleri.Enabled = true;

            btnKaydet.Tag = YetkiTipleri.Create;
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);

            txtCihazAdi.Focus();
            LogHelper.Info(PageName, "EnterMode", "Add modu");
        }
        private void EnterEditModeFromSelection()
        {
            var it = chkCihazlar.SelectedItem as CihazListDTO;
            if (it == null) return;

            if (!_auth.Can(PageName, YetkiTipleri.Update))
            {
                MessageBox.Show("Güncelleme yetkiniz yok.");
                LogHelper.Warn(PageName, "EnterMode", "Edit moduna geçilemedi: yetki yok");
                return;
            }

            _mode = ScreenMode.Edit;

            btnKaydet.Visible = true;
            btnVazgec.Visible = true;

            btnCihazEkle.Enabled = false;
            btnCihazGuncelle.Enabled = false;
            btnCihazSil.Enabled = false;

            var c = _svc.Get(it.CihazId);
            if (c != null) FillInputs(c);

            txtFirmaId.ReadOnly = false;
            txtCihazAdi.ReadOnly = false;
            txtIpAdres.ReadOnly = false;
            txtPort.ReadOnly = false;
            txtAciklama.ReadOnly = false;
            cmbCihazTipleri.Enabled = true;

            btnKaydet.Tag = YetkiTipleri.Update;
            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);

            txtCihazAdi.Focus();
            txtCihazAdi.SelectAll();
            LogHelper.Info(PageName, "EnterMode", $"Edit modu (CihazId={it.CihazId})");
        }
        private void ClearInputs()
        {
            txtCihazId.Text = "";
            txtFirmaId.Text = "";
            txtCihazAdi.Text = "";
            txtIpAdres.Text = "";
            txtPort.Text = "4370";
            txtAciklama.Text = "";
            if (cmbCihazTipleri.Items.Count > 0)
                cmbCihazTipleri.SelectedIndex = 0;
        }
        private void FillInputs(Cihaz c)
        {
            txtCihazId.Text = c.CihazId.ToString();
            txtFirmaId.Text = c.FirmaId.ToString();
            txtCihazAdi.Text = c.CihazAdi ?? "";
            txtIpAdres.Text = c.IPAdres ?? "";
            txtPort.Text = (c.Port > 0 ? c.Port : 4370).ToString();
            txtAciklama.Text = c.Notlar ?? "";
            if (cmbCihazTipleri.DataSource != null)
                cmbCihazTipleri.SelectedValue = c.CihazTipi;
        }
        private void FillInputsFromSelection()
        {
            var it = chkCihazlar.SelectedItem as CihazListDTO;
            if (it == null) { ClearInputs(); return; }

            var c = _svc.Get(it.CihazId);
            if (c == null) { ClearInputs(); return; }

            FillInputs(c);
        }
        private Cihaz CollectFromInputs()
        {
            if (string.IsNullOrWhiteSpace(txtCihazAdi.Text))
                throw new Exception("Cihaz Adı boş olamaz.");
            if (string.IsNullOrWhiteSpace(txtIpAdres.Text))
                throw new Exception("IP Adres boş olamaz.");

            int.TryParse(txtPort.Text, out var port);
            if (port <= 0) port = 4370;
            int.TryParse(txtFirmaId.Text, out var firmaId);

            return new Cihaz
            {
                CihazId = int.TryParse(txtCihazId.Text, out var id) ? id : 0,
                FirmaId = firmaId,
                CihazAdi = txtCihazAdi.Text.Trim(),
                IPAdres = txtIpAdres.Text.Trim(),
                Port = port,
                Notlar = string.IsNullOrWhiteSpace(txtAciklama.Text) ? null : txtAciklama.Text.Trim(),
                CihazTipi = Convert.ToInt32(cmbCihazTipleri.SelectedValue),
                AktifMi = true
            };
        }
        private void Save()
        {
            var cid = Guid.NewGuid().ToString("N");
            try
            {
                string need = _mode == ScreenMode.Edit ? YetkiTipleri.Update : YetkiTipleri.Create;
                if (!_auth.Can(PageName, need))
                {
                    MessageBox.Show($"Bu işlem için yetkiniz yok: {need}");
                    LogHelper.Warn(PageName, "Save", $"Yetkisiz işlem: {need}", cid: cid);
                    return;
                }

                var c = CollectFromInputs();
                bool ok;

                if (_mode == ScreenMode.Add)
                {
                    var newId = _svc.Ekle(c);
                    ok = newId > 0;
                    if (ok) txtCihazId.Text = newId.ToString();
                    LogHelper.Info(PageName, "Create",
                                  ok ? $"Cihaz eklendi (Id={newId})." : "Cihaz ekleme başarısız.",
                                  detayJson: $"{{\"FirmaId\":{c.FirmaId},\"Ad\":\"{LogHelper.Escape((string)c.CihazAdi)}\",\"Ip\":\"{LogHelper.Escape((string)c.IPAdres)}\",\"Port\":{c.Port},\"Tip\":{c.CihazTipi}}}",
                                  cid: cid);
                }
                else if (_mode == ScreenMode.Edit)
                {
                    if (c.CihazId <= 0) { MessageBox.Show("Geçersiz CihazId."); return; }
                    _svc.Guncelle(c);
                    ok = true;
                    LogHelper.Info(PageName, "Update",
                                   $"Cihaz güncellendi (Id={c.CihazId}).",
                                   detayJson: $"{{\"FirmaId\":{c.FirmaId},\"Ad\":\"{LogHelper.Escape((string)c.CihazAdi)}\",\"Ip\":\"{LogHelper.Escape((string)c.IPAdres)}\",\"Port\":{c.Port},\"Tip\":{c.CihazTipi}}}",
                                   cid: cid);
                }
                else return;

                if (!ok)
                {
                    MessageBox.Show("İşlem başarısız.", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show("Kayıt tamamlandı.", "Bilgi",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadList();
                EnterListMode();
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "Save", "Kayıt sırasında hata", ex, cid: cid);
                MessageBox.Show(ex.Message, "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void DeleteSelected()
        {
            var it = chkCihazlar.SelectedItem as CihazListDTO;
            if (it == null) return;

            if (!_auth.DeleteAbility(PageName))
            { MessageBox.Show("Silme (pasife çekme) yetkiniz yok."); return; }

            if (MessageBox.Show($"“{it.CihazAdi}” pasife çekilsin mi?",
                    "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var cid = Guid.NewGuid().ToString("N");
            try
            {
                _svc.PasifYap(it.CihazId);
                LoadList();
                EnterListMode();
                MessageBox.Show("Cihaz pasife çekildi.");
                LogHelper.Info(PageName, "Delete", $"Cihaz pasife çekildi (Id={it.CihazId}).",
                               detayJson: $"{{\"Ad\":\"{LogHelper.Escape((string)it.CihazAdi)}\"}}", cid: cid);
            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "Delete", "Cihaz pasife çekilirken hata", ex,
                               detayJson: $"{{\"Id\":{it.CihazId}}}", cid: cid);
                MessageBox.Show(ex.Message, "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}