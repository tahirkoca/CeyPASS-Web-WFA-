using AutoUpdaterDotNET;
using CeyPASS.Business.Abstractions;
using CeyPASS.Business.Services;
using CeyPASS.DataAccess;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.DataAccess.Repositories;
using CeyPASS.Infrastructure.Helpers;
using CeyPASS.WFA.Forms;
using CeyPASS.WFA.UserControls;
using CeyPASS.WFA.UserControls.Ayarlar;
using CeyPASS.WFA.UserControls.Canlı_İzleme;
using CeyPASS.WFA.UserControls.Dashboard;
using CeyPASS.WFA.UserControls.EO;
using CeyPASS.WFA.UserControls.Izinler;
using CeyPASS.WFA.UserControls.Raporlar;
using CeyPASS.WFA.UserControls.VMY;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Windows.Forms;

namespace CeyPASS.WFA
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Faz 4.3: Güncelleme AutoUpdater ile; güncelleme varsa uygula, yapamazsa mevcut sürümle devam
            // Synchronous = false: Giriş ekranı hemen açılsın; sunucu ulaşılamazsa takılma
            // Mandatory = false, ShowSkipButton = true: Güncelleme indirilemezse/uygulanamazsa kullanıcı atlayıp mevcut sürümle devam edebilir
            // InstallationPath: Güncelleme dosyalarının yazılacağı klasör; ayarlanmazsa yanlış yere yazılıp uygulama açılmayabilir.
            // Zip yapısı: bin\Release\net8.0-windows\ İÇİNDEKİ dosyaları zip'e ekleyin (net8.0-windows klasörünü eklemeyin).
            // Arşivin kökünde CeyPASS.WFA.exe ve tüm dll'ler olmalı; aksi halde dosyalar kurulum\net8.0-windows\ altına gider ve uygulama açılmaz.
            try
            {
                AutoUpdater.InstallationPath = Application.StartupPath ?? AppContext.BaseDirectory;
                AutoUpdater.Mandatory = false;
                AutoUpdater.UpdateMode = Mode.ForcedDownload;
                AutoUpdater.ShowSkipButton = true;
                AutoUpdater.ShowRemindLaterButton = true;
                AutoUpdater.ReportErrors = false;
                AutoUpdater.Synchronous = false;
                AutoUpdater.RunUpdateAsAdmin = true;
                AutoUpdater.ApplicationExitEvent += () =>
                {
                    try { Application.Exit(); } catch { }
                };
                AutoUpdater.Start(@"http://192.168.0.23/CeyPASS-Updates/update.xml");
            }
            catch (Exception)
            {
                // Güncelleme kontrolü başarısız olsa bile program açılsın
            }

            // Faz 4.1: appsettings.json + IConfiguration (Web ile aynı yapı)
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false);
            IConfiguration configuration = configBuilder.Build();

            var services = new ServiceCollection();

            // IConfiguration (singleton)
            services.AddSingleton(configuration);

            // Oturum(Session)
            services.AddSingleton<ISessionContext, SessionContext>();

            // Connection string: önce appsettings.json, sonra App.config, sonra Infrastructure.DatabaseHelperCore
            var connectionString = configuration.GetConnectionString("DefaultConnection")?.Trim();
            if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("YOUR_PASSWORD"))
            {
                connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString?.Trim();
            }
            if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("YOUR_PASSWORD"))
            {
                connectionString = CeyPASS.Infrastructure.Helpers.DatabaseHelperCore.GetSqlConnectionString("CeyPASS.WFA");
            }

            // Faz 4.2: DbContext ve veri katmanı Scoped (form bazlı unit of work)
            services.AddDbContext<CeyPASSDataConnectionCore>(options =>
                options.UseSqlServer(connectionString), ServiceLifetime.Scoped);

            // DataAccess (EF Core *RepositoryCore) — Faz 4.2: Scoped
            services.AddScoped<IAuthorizationRepository, AuthorizationRepositoryCore>();
            services.AddScoped<IBolumRepository, BolumRepositoryCore>();
            services.AddScoped<ICalismaSekliRepository, CalismaSekliRepositoryCore>();
            services.AddScoped<ICalismaStatuRepository, CalismaStatuRepositoryCore>();
            services.AddScoped<ICanliIzlemeRepository, CanliIzlemeRepositoryCore>();
            services.AddScoped<ICihazRepository, CihazRepositoryCore>();
            services.AddScoped<IDashboardRepository, DashboardRepositoryCore>();
            services.AddScoped<IDepartmanRepository, DepartmanRepositoryCore>();
            services.AddScoped<IFirmaRepository, FirmaRepositoryCore>();
            services.AddScoped<IIsyeriRepository, IsyeriRepositoryCore>();
            services.AddScoped<IIzinTipRepository, IzinTipRepositoryCore>();
            services.AddScoped<IKisiHareketRepository, KisiHareketRepositoryCore>();
            services.AddScoped<IKisiIzinlerRepository, KisiIzinlerRepositoryCore>();
            services.AddScoped<IKisiRepository, KisiRepositoryCore>();
            services.AddScoped<IKullaniciRepository, KullaniciRepositoryCore>();
            services.AddScoped<IPozisyonRepository, PozisyonRepositoryCore>();
            services.AddScoped<IPuantajRepository, PuantajRepositoryCore>();
            services.AddScoped<IPuantajsizKartAtamaRepository, PuantajsizKartAtamaRepositoryCore>();
            services.AddScoped<IPuantajsizKartRepository, PuantajsizKartRepositoryCore>();
            services.AddScoped<IRaporRepository, RaporRepositoryCore>();
            services.AddScoped<IResmiTatilRepository, ResmiTatilRepositoryCore>();
            services.AddScoped<ISistemLogRepository, SistemLogRepositoryCore>();
            services.AddScoped<IYemekhaneRepository, YemekhaneRepositoryCore>();
            services.AddScoped<IMailRepository, MailRepositoryCore>();

            // Business — Faz 4.2: Scoped (DbContext ile aynı scope)
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<ICalismaSekliService, CalismaSekliService>();
            services.AddScoped<ICalismaStatuService, CalismaStatuService>();
            services.AddScoped<ICanliIzlemeService, CanliIzlemeService>();
            services.AddScoped<ICihazService, CihazService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IDepartmanService, DepartmanService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IFazlaMesaiService, FazlaMesaiService>();
            services.AddScoped<IFirmaService, FirmaService>();
            services.AddScoped<IIsyeriService, IsyeriService>();
            services.AddScoped<IIzinTipService, IzinTipService>();
            services.AddScoped<IKisiDetayService, KisiDetayService>();
            services.AddScoped<IKisiEkraniLookUpService, KisiEkraniLookupService>();
            services.AddScoped<IKisiHareketService, KisiHareketService>();
            services.AddScoped<IKisiIzinService, KisiIzinService>();
            services.AddScoped<IKisiQueryService, KisiQueryService>();
            services.AddScoped<IKisiService, KisiService>();
            services.AddScoped<IKullaniciQueryService, KullaniciQueryService>();
            services.AddScoped<IKullaniciService, KullaniciService>();
            services.AddScoped<IMisafirKartService, MisafirKartService>();
            services.AddScoped<IPozisyonService, PozisyonService>();
            services.AddScoped<IPuantajService, PuantajService>();
            services.AddScoped<IRaporService, RaporService>();
            services.AddScoped<IResmiTatilService, ResmiTatilService>();
            services.AddScoped<ISifreService, SifreService>();
            services.AddScoped<ISistemLogService, SistemLogService>();
            services.AddScoped<IMailService, MailService>();
            services.AddScoped<INotificationService, NotificationService>();

            // UI — Faz 4.2: girisEkrani ve islemEkrani Scoped (scope ile açılır)
            services.AddScoped<girisEkrani>();
            services.AddScoped<islemEkrani>();
            services.AddScoped<canliIzlemeGirisEkrani>();
            services.AddScoped<canliIzlemeVeriEkrani>();
            services.AddScoped<puantajSatirDuzenlemeEkrani>();
            services.AddScoped<sifremiUnuttumEkrani>();
            services.AddScoped<reddetmeEkrani>();
            services.AddScoped<ucCihazlar>();
            services.AddScoped<ucResmiTatiller>();
            services.AddScoped<KisiKartKontrolu>();
            services.AddScoped<misafirKartAtama>();
            services.AddScoped<ucDashboard>();
            services.AddScoped<ucAylikPuantajEkrani>();
            services.AddScoped<ucKisiHareketler>();
            services.AddScoped<ucIzinler>();
            services.AddScoped<ucDepartmanTanimlama>();
            services.AddScoped<ucFirmaTanimlama>();
            services.AddScoped<ucIsyeriTanimlama>();
            services.AddScoped<ucPersonelTanimlama>();
            services.AddScoped<ucPozisyonTanimla>();
            services.AddScoped<ucRaporlar>();
            services.AddScoped<ucCalismaSekilleri>();
            services.AddScoped<ucCalismaStatuleri>();
            services.AddScoped<ucGuncellemeMailEkrani>();
            services.AddScoped<CeyPASS.WFA.UserControls.Admin.ucAdminPanel>();

            var sp = services.BuildServiceProvider();

            var session = sp.GetRequiredService<ISessionContext>();

            // Faz 4.2: Giriş ekranı Scoped; bir scope açıp girisEkrani bu scope'tan alınır
            var girisScope = sp.CreateScope();
            LogHelper.Configure(girisScope.ServiceProvider.GetRequiredService<ISistemLogService>(), session);
            var giris = girisScope.ServiceProvider.GetRequiredService<girisEkrani>();
            try
            {
                Application.Run(giris);
            }
            finally
            {
                girisScope.Dispose();
            }
        }
    }
}
