using CeyPASS.WFA.UserControls;
using CeyPASS.WFA.UserControls.Ayarlar;
using CeyPASS.WFA.UserControls.EO;
using CeyPASS.WFA.UserControls.VMY;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Forms;

namespace CeyPASS.WFA.UserControls.Admin
{
    /// <summary>
    /// Web Admin Panel ile aynı sekmeler: Firma, İşyeri, Cihaz, Departman, Pozisyon, Resmi Tatil, Çalışma Statüsü, Vardiya, Güncelleme Bildirimi.
    /// Sadece RolId==1 (süper admin) görür.
    /// </summary>
    public partial class ucAdminPanel : UserControl
    {
        private static readonly string PageNameUI = "Admin Panel";
        private readonly IServiceProvider _sp;

        public ucAdminPanel(IServiceProvider sp)
        {
            InitializeComponent();
            _sp = sp;
        }

        private void ucAdminPanel_Load(object sender, System.EventArgs e)
        {
            HostControl(pnlFirma, _sp.GetRequiredService<ucFirmaTanimlama>());
            HostControl(pnlIsyeri, _sp.GetRequiredService<ucIsyeriTanimlama>());

            var ucCihaz = _sp.GetRequiredService<ucCihazlar>();
            ucCihaz.AdminPanelMode = true;
            ucCihaz.RefreshList();
            HostControl(pnlCihaz, ucCihaz);

            HostControl(pnlDepartman, _sp.GetRequiredService<ucDepartmanTanimlama>());
            HostControl(pnlPozisyon, _sp.GetRequiredService<ucPozisyonTanimla>());
            HostControl(pnlResmiTatil, _sp.GetRequiredService<ucResmiTatiller>());
            HostControl(pnlCalismaStatu, _sp.GetRequiredService<ucCalismaStatuleri>());

            var ucVardiya = _sp.GetRequiredService<ucCalismaSekilleri>();
            ucVardiya.AdminPanelMode = true;
            ucVardiya.RefreshList();
            HostControl(pnlVardiya, ucVardiya);

            HostControl(pnlGuncellemeMail, _sp.GetRequiredService<ucGuncellemeMailEkrani>());
        }

        private static void HostControl(Panel panel, UserControl uc)
        {
            uc.Dock = DockStyle.Fill;
            panel.Controls.Clear();
            panel.Controls.Add(uc);
        }
    }
}
