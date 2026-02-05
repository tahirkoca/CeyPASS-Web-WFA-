using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using System;
using System.Globalization;
using System.Windows.Forms;

namespace CeyPASS.WFA.UserControls.Ayarlar
{
    public partial class ucResmiTatiller : UserControl
    {
        private readonly ISessionContext _session;
        private readonly IResmiTatilService _rsvc;
        private readonly IAuthorizationService _auth;
        AuthorizationHelper authHelp;
        private const string PageName = "ResmiTatiller";
        private const string PageNameUI = "Resmi Tatiller";

        public ucResmiTatiller(ISessionContext session, IResmiTatilService rsvc, IAuthorizationService auth)
        {
            InitializeComponent();
            _session = session;
            _rsvc = rsvc;
            _auth = auth;
            authHelp = new AuthorizationHelper(_session, _auth);
            btnSabitResmiTatilleriAktar.Tag = YetkiTipleri.Approve;
            btnDigerResmiTatilleriAktar.Tag = YetkiTipleri.Create;

            WinFormsAuthHelper.ApplyPageAuthorization(_auth, _session, PageName, this);

            if (!_auth.ViewAbility(PageName))
            {
                MessageBox.Show("Resmî Tatiller ekranını görüntüleme yetkiniz yok");
                this.Visible = false;
                return;
            }

            nudCalismaSaati.DecimalPlaces = 2;
            nudCalismaSaati.Minimum = 0;
            nudCalismaSaati.Maximum = 24;
            nudCalismaSaati.Increment = 0.25M;

            var y = DateTime.Today.Year;
            txtBaslangicYili.Text = y.ToString();
            txtBitisYili.Text = y.ToString();
            AppTheme.ApplyToControl(this);
            LogHelper.Info(PageName, "Open", $"Resmi Tatiller ekranı açıldı ({y}).");
            ListeyiYenile();
        }

        private void btnSabitResmiTatilleriAktar_Click(object sender, EventArgs e)
        {
            var cid = Guid.NewGuid().ToString("N");
            try
            {
                if (!_auth.Can(PageName, YetkiTipleri.Approve))
                {
                    MessageBox.Show("Sabit resmî tatilleri aktarma yetkiniz yok.");
                    LogHelper.Warn(PageName, "SabitAktar", "Yetki yok", cid: cid);
                    return;
                }
                if (!InputHelper.TryParseYear(txtBaslangicYili.Text, out var basYil) || !InputHelper.TryParseYear(txtBitisYili.Text, out var bitYil))
                {
                    MessageBox.Show("Başlangıç/Bitiş yılı geçerli değil.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LogHelper.Warn(PageName, "SabitAktar", "Geçersiz yıl aralığı",
                                  detayJson: $"{{\"Baslangic\": \"{txtBaslangicYili.Text}\", \"Bitis\": \"{txtBitisYili.Text}\"}}",
                                  cid: cid);
                    return;
                }

                _rsvc.DoldurSabit(basYil, bitYil);
                LogHelper.Info(PageName, "SabitAktar",
                               $"Sabit resmi tatiller işlendi ({basYil}-{bitYil}).",
                               detayJson: $"{{\"BasYil\":{basYil},\"BitYil\":{bitYil}}}",
                               cid: cid);
                MessageBox.Show($"Sabit resmi tatiller {basYil}-{bitYil} aralığı için işlendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "SabitAktar", "Sabit resmi tatil aktarımı hatası", ex, cid: cid);
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnDigerResmiTatilleriAktar_Click(object sender, EventArgs e)
        {
            var cid = Guid.NewGuid().ToString("N");
            try
            {
                if (!_auth.Can(PageName, YetkiTipleri.Create))
                {
                    MessageBox.Show("Resmî tatil ekleme yetkiniz yok.");
                    LogHelper.Warn(PageName, "TekilEkle", "Yetki yok", cid: cid);
                    return;
                }

                var tarih = dtpEklenecekTarih.Value.Date;
                var ad = (txtEklenecekResmiTatil.Text ?? "").Trim();

                decimal? calismaSaati = nudCalismaSaati.Value;

                _rsvc.KaydetTekil(tarih, ad, calismaSaati);
                LogHelper.Info(PageName, "TekilEkle",
                               $"Tekil resmi tatil kaydı eklendi: {ad} ({tarih:yyyy-MM-dd}).",
                               detayJson: $"{{\"Tarih\":\"{tarih:yyyy-MM-dd}\",\"Ad\":\"{LogHelper.Escape((string)ad)}\",\"CalismaSaati\":{(calismaSaati ?? 0M).ToString(CultureInfo.InvariantCulture)}}}",
                               cid: cid);
                MessageBox.Show("Resmi tatil kaydı işlendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                LogHelper.Error(PageName, "TekilEkle", "Tekil resmi tatil ekleme hatası", ex, cid: cid);
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ListeyiYenile(int? yil = null)
        {
            lstTatiller.BeginUpdate();
            lstTatiller.Items.Clear();
            foreach (var x in _rsvc.GetList(yil)) lstTatiller.Items.Add(x.ListeMetni);
            lstTatiller.EndUpdate();
            LogHelper.Info(PageName, "ListeYenile", $"Liste yenilendi{(yil.HasValue ? $" (yıl={yil})" : "")}.");
        }
    }
}
