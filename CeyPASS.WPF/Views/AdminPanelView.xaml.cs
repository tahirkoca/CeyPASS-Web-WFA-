using CeyPASS.Business.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CeyPASS.WPF.Views;

/// <summary>
/// WFA ucAdminPanel ile aynı sekmeler. Sadece RolId==1 (süper admin) menüden açılır.
/// Cihaz ve Vardiya sekmeleri AdminPanelMode ile filtresiz liste gösterir.
/// </summary>
public partial class AdminPanelView : System.Windows.Controls.UserControl
{
    public AdminPanelView()
    {
        InitializeComponent();

        var session = App.Services.GetRequiredService<ISessionContext>();
        if (session.RolId != 1)
        {
            Content = new System.Windows.Controls.TextBlock
            {
                Text = "Admin Panel yalnızca süper yönetici (RolId=1) için açıktır.",
                Margin = new System.Windows.Thickness(24),
                FontSize = 14,
                Foreground = System.Windows.Media.Brushes.DarkRed,
                TextWrapping = System.Windows.TextWrapping.Wrap
            };
            return;
        }

        TabFirma.Content = new FirmaView();
        TabIsyeri.Content = new IsyeriView();
        TabCihaz.Content = new CihazView(adminPanelMode: true);
        TabDepartman.Content = new DepartmanView();
        TabPozisyon.Content = new PozisyonView();
        TabResmiTatil.Content = new ResmiTatilView();
        TabCalismaStatu.Content = new CalismaStatuView();
        TabVardiya.Content = new VardiyaView(adminPanelMode: true);
        TabGuncelleme.Content = new GuncellemeBildirimView();
    }
}
