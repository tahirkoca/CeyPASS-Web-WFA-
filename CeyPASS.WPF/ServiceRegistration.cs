using CeyPASS.Business.Abstractions;
using CeyPASS.Business.Services;
using CeyPASS.DataAccess;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.DataAccess.Repositories;
using CeyPASS.Infrastructure.Configuration;
using CeyPASS.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace CeyPASS.WPF;

public static class ServiceRegistration
{
    public static ServiceProvider Build()
    {
        var baseDir = AppContext.BaseDirectory;
        var configuration = new ConfigurationBuilder()
            .SetBasePath(baseDir)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<ISessionContext, SessionContext>();
        services.AddMemoryCache();
        services.AddLogging();

        var connectionString = configuration.GetConnectionString("DefaultConnection")?.Trim();
        if (string.IsNullOrEmpty(connectionString) || DatabaseHelperCore.LooksLikePlaceholder(connectionString))
            connectionString = DatabaseHelperCore.TryGetConnectionStringFromEnvironment();
        if (string.IsNullOrEmpty(connectionString) || DatabaseHelperCore.LooksLikePlaceholder(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection yapılandırılmadı. CeyPASS.WPF/appsettings.Local.json kullanın (WFA ile aynı).");
        }

        services.AddDbContext<CeyPASSDataConnectionCore>(options =>
            options.UseSqlServer(connectionString), ServiceLifetime.Scoped);

        // Repos
        services.AddScoped<IAuthorizationRepository, AuthorizationRepositoryCore>();
        services.AddScoped<IBildirimRepository, BildirimRepositoryCore>();
        services.AddScoped<IBolumRepository, BolumRepositoryCore>();
        services.AddScoped<ICalismaStatuRepository, CalismaStatuRepositoryCore>();
        services.AddScoped<ICalismaSekliRepository, CalismaSekliRepositoryCore>();
        services.AddScoped<ICanliIzlemeRepository, CanliIzlemeRepositoryCore>();
        services.AddScoped<IDashboardRepository, DashboardRepositoryCore>();
        services.AddScoped<IDepartmanRepository, DepartmanRepositoryCore>();
        services.AddScoped<IFirmaRepository, FirmaRepositoryCore>();
        services.AddScoped<IIsyeriRepository, IsyeriRepositoryCore>();
        services.AddScoped<IIzinTipRepository, IzinTipRepositoryCore>();
        services.AddScoped<IKisiIzinlerRepository, KisiIzinlerRepositoryCore>();
        services.AddScoped<IKisiHareketRepository, KisiHareketRepositoryCore>();
        services.AddScoped<IKisiRepository, KisiRepositoryCore>();
        services.AddScoped<IKullaniciRepository, KullaniciRepositoryCore>();
        services.AddScoped<IPozisyonRepository, PozisyonRepositoryCore>();
        services.AddScoped<IKullaniciFirmaIsyeriYetkiRepository, KullaniciFirmaIsyeriYetkiRepositoryCore>();
        services.AddScoped<IPersonelWebSifreRepository, PersonelWebSifreRepositoryCore>();
        services.AddScoped<ISistemLogRepository, SistemLogRepositoryCore>();
        services.AddScoped<IUserDeviceTokenRepository, UserDeviceTokenRepositoryCore>();
        services.AddScoped<IUstYetkiliRepository, UstYetkiliRepositoryCore>();
        services.AddScoped<IYemekhaneRepository, YemekhaneRepositoryCore>();
        services.AddScoped<ICihazRepository, CihazRepositoryCore>();
        services.AddScoped<IPersonelVardiyaYemekYetkiRepository, PersonelVardiyaYemekYetkiRepositoryCore>();
        services.AddScoped<IResmiTatilRepository, ResmiTatilRepositoryCore>();
        services.AddScoped<IMailRepository, MailRepositoryCore>();
        services.AddScoped<IRaporRepository, RaporRepositoryCore>();
        services.AddScoped<IPuantajRepository, PuantajRepositoryCore>();

        services.AddScoped<IPuantajsizKartAtamaRepository, PuantajsizKartAtamaRepositoryCore>();
        services.AddScoped<IMisafirKartService, MisafirKartService>();
        services.AddScoped<IAracKartiService, AracKartiService>();

        // Business
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        services.AddScoped<IBildirimService, BildirimManager>();
        services.AddScoped<ICanliIzlemeService, CanliIzlemeService>();
        services.AddScoped<ICalismaSekliService, CalismaSekliService>();
        services.AddScoped<ICihazService, CihazService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IDepartmanService, DepartmanService>();
        services.AddScoped<IPozisyonService, PozisyonService>();
        services.AddScoped<ICalismaStatuService, CalismaStatuService>();
        services.AddScoped<IEmailService>(sp => new EmailService(CreateSmtp(sp.GetRequiredService<IConfiguration>())));
        services.AddScoped<IMailService, MailService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IFirmaService, FirmaService>();
        services.AddScoped<IIsyeriService, IsyeriService>();
        services.AddScoped<IIzinTipService, IzinTipService>();
        services.AddScoped<IKisiIzinService, KisiIzinService>();
        services.AddScoped<IKisiDetayService, KisiDetayService>();
        services.AddScoped<IKisiEkraniLookUpService, KisiEkraniLookupService>();
        services.AddScoped<IKisiHareketService, KisiHareketService>();
        services.AddScoped<IKisiQueryService, KisiQueryService>();
        services.AddScoped<IKisiService, KisiService>();
        services.AddScoped<IKullaniciFirmaIsyeriYetkiService, KullaniciFirmaIsyeriYetkiService>();
        services.AddScoped<IKullaniciService, KullaniciService>();
        services.AddScoped<IKullaniciQueryService, KullaniciQueryService>();
        services.AddScoped<IPersonelVardiyaYemekYetkiService, PersonelVardiyaYemekYetkiService>();
        services.AddScoped<IResmiTatilService, ResmiTatilService>();
        services.AddScoped<IRaporService, RaporService>();
        services.AddScoped<IPushNotificationService, FcmPushService>();
        services.AddScoped<ISifreService, SifreService>();
        services.AddScoped<ISistemLogService, SistemLogService>();
        services.AddScoped<IPuantajService, PuantajService>();

        services.AddTransient<Views.LoginWindow>();
        services.AddTransient<Views.MainWindow>();
        services.AddTransient<Views.KisiHareketView>();
        services.AddTransient<Views.DashboardView>();
        services.AddTransient<Views.DepartmanView>();
        services.AddTransient<Views.PozisyonView>();
        services.AddTransient<Views.FirmaView>();
        services.AddTransient<Views.IsyeriView>();
        services.AddTransient<Views.IzinlerView>();
        services.AddTransient<Views.CalismaStatuView>();
        services.AddTransient<Views.VardiyaView>();
        services.AddTransient<Views.CihazView>();
        services.AddTransient<Views.ResmiTatilView>();
        services.AddTransient<Views.GuncellemeBildirimView>();
        services.AddTransient<Views.AdminPanelView>();
        services.AddTransient<Views.RaporlarView>();
        services.AddTransient<Views.AylikPuantajView>();
        services.AddTransient<Views.PersonelView>();
        services.AddTransient<Views.ForgotPasswordWindow>();
        services.AddTransient<Views.CanliIzlemeLoginWindow>();
        services.AddTransient<Views.CanliIzlemeWindow>();

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Öncelik: appsettings(.Local).json SmtpSettings — WFA Local ile aynı kaynak.
    /// Yoksa App.config / ConfigurationManager.
    /// </summary>
    private static SmtpConfiguration CreateSmtp(IConfiguration configuration)
    {
        var section = configuration.GetSection("SmtpSettings");
        var host = section["Host"];
        if (!string.IsNullOrWhiteSpace(host))
        {
            var port = int.TryParse(section["Port"], out var p) ? p : 587;
            var ssl = !bool.TryParse(section["EnableSsl"], out var s) || s;
            return new SmtpConfiguration(
                host,
                port,
                ssl,
                section["Username"] ?? "",
                section["Password"] ?? "",
                section["FromAddress"] ?? "",
                section["FromName"] ?? "CeyPASS Sistem");
        }

        return new SmtpConfiguration();
    }
}