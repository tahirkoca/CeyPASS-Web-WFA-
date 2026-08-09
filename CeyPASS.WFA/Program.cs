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

            try
            {
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
                // Akıllı klasör kontrolü: Eğer dosya ana klasörde yoksa alt klasörlere bak.
                var baseDir = AppContext.BaseDirectory;
                var configBuilder = new ConfigurationBuilder()
                    .SetBasePath(baseDir)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                    .AddJsonFile(Path.Combine(baseDir, "net8.0-windows", "appsettings.json"), optional: true)
                    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
                    .AddJsonFile(Path.Combine(baseDir, "net8.0-windows", "appsettings.Local.json"), optional: true);
                IConfiguration configuration = configBuilder.Build();

                var services = new ServiceCollection();

                // IConfiguration (singleton)
                services.AddSingleton(configuration);

                // Oturum(Session)
                services.AddSingleton<ISessionContext, SessionContext>();

                // IMemoryCache (KisiEkraniLookupService vb. için gerekli)
                services.AddMemoryCache();

                // ILogger<T> (FcmPushService vb.) — WinForms’ta varsayılan konsol/debug olmadan da factory kaydı yeterli
                services.AddLogging();

                // Connection string: appsettings → App.config → ortam (secret repoda yok)
                var connectionString = configuration.GetConnectionString("DefaultConnection")?.Trim();
                if (string.IsNullOrEmpty(connectionString) || DatabaseHelperCore.LooksLikePlaceholder(connectionString))
                {
                    connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString?.Trim();
                }
                if (string.IsNullOrEmpty(connectionString) || DatabaseHelperCore.LooksLikePlaceholder(connectionString))
                {
                    connectionString = DatabaseHelperCore.TryGetConnectionStringFromEnvironment();
                }
                if (string.IsNullOrEmpty(connectionString) || DatabaseHelperCore.LooksLikePlaceholder(connectionString))
                {
                    throw new InvalidOperationException(
                        "ConnectionStrings:DefaultConnection yapılandırılmadı. appsettings.Local.json, App.config veya ConnectionStrings__DefaultConnection ortam değişkenini kullanın.");
                }

                // Faz 4.2: DbContext ve veri katmanı Scoped (form bazlı unit of work)
                services.AddDbContext<CeyPASSDataConnectionCore>(options =>
                    options.UseSqlServer(connectionString), ServiceLifetime.Scoped);

                // DataAccess (EF Core *RepositoryCore) — Faz 4.2: Scoped
                services.AddScoped<IAuthorizationRepository, AuthorizationRepositoryCore>();
                services.AddScoped<IBolumRepository, BolumRepositoryCore>();
                services.AddScoped<ICalismaSekliRepository, CalismaSekliRepositoryCore>();
                services.AddScoped<IPersonelVardiyaYemekYetkiRepository, PersonelVardiyaYemekYetkiRepositoryCore>();
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
                services.AddScoped<IKullaniciFirmaIsyeriYetkiRepository, KullaniciFirmaIsyeriYetkiRepositoryCore>();
                services.AddScoped<IKullaniciFirmaIsyeriYetkiService, KullaniciFirmaIsyeriYetkiService>();
                services.AddScoped<IPuantajRepository, PuantajRepositoryCore>();
                services.AddScoped<IPuantajsizKartAtamaRepository, PuantajsizKartAtamaRepositoryCore>();
                services.AddScoped<IRaporRepository, RaporRepositoryCore>();
                services.AddScoped<IResmiTatilRepository, ResmiTatilRepositoryCore>();
                services.AddScoped<ISistemLogRepository, SistemLogRepositoryCore>();
                services.AddScoped<IYemekhaneRepository, YemekhaneRepositoryCore>();
                services.AddScoped<IMailRepository, MailRepositoryCore>();
                services.AddScoped<IBildirimRepository, BildirimRepositoryCore>();
                services.AddScoped<IUserDeviceTokenRepository, UserDeviceTokenRepositoryCore>();
                services.AddScoped<IUstYetkiliRepository, UstYetkiliRepositoryCore>();
                services.AddScoped<IPersonelWebSifreRepository, PersonelWebSifreRepositoryCore>();
                services.AddScoped<IAvansRepository, AvansRepositoryCore>();

                // Business — Faz 4.2: Scoped (DbContext ile aynı scope)
                services.AddScoped<IAuthorizationService, AuthorizationService>();
                services.AddScoped<ICalismaSekliService, CalismaSekliService>();
                services.AddScoped<IPersonelVardiyaYemekYetkiService, PersonelVardiyaYemekYetkiService>();
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
                services.AddScoped<IAracKartiService, AracKartiService>();
                services.AddScoped<IPozisyonService, PozisyonService>();
                services.AddScoped<IPuantajService, PuantajService>();
                services.AddScoped<IRaporService, RaporService>();
                services.AddScoped<IResmiTatilService, ResmiTatilService>();
                services.AddScoped<ISifreService, SifreService>();
                services.AddScoped<ISistemLogService, SistemLogService>();
                services.AddScoped<IMailService, MailService>();
                services.AddScoped<IBildirimService, BildirimManager>();
                services.AddScoped<IPushNotificationService, FcmPushService>();
                services.AddScoped<INotificationService, NotificationService>();

                // UI — Faz 4.2: girisEkrani ve islemEkrani Scoped (scope ile açılır)
                services.AddScoped<girisEkrani>();
                services.AddScoped<islemEkrani>();
                services.AddScoped<canliIzlemeGirisEkrani>();
                services.AddScoped<canliIzlemeVeriEkrani>();
                services.AddScoped<puantajSatirDuzenlemeEkrani>();
                services.AddScoped<sifremiUnuttumEkrani>();
                services.AddScoped<reddetmeEkrani>();
                services.AddTransient<frmKisiAra>();
                services.AddScoped<ucCihazlar>();
                services.AddScoped<ucResmiTatiller>();
                services.AddScoped<KisiKartKontrolu>();
                services.AddScoped<misafirKartAtama>();
                services.AddScoped<aracKartiAtama>();
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
            catch (Exception ex)
            {
                MessageBox.Show($"Uygulama başlatılırken bir hata oluştu:\n\n{ex.Message}\n\nİç Hata: {ex.InnerException?.Message}", "Başlatma Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
