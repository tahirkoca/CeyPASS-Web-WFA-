using CeyPASS.Business.Abstractions;
using CeyPASS.Business.Services;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.DataAccess;
using CeyPASS.DataAccess.Repositories;
using CeyPASS.Infrastructure.Helpers;
using CeyPASS.Infrastructure.Pdf;
using Microsoft.EntityFrameworkCore;
using CeyPASS.Web.Services;

// PDF export (MigraDoc) için Windows'ta Arial vb. sistem fontlarının kullanılması - ilk PDF'den önce ayarlanmalı
ExportHelper.ConfigurePdfFonts();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Razor view render (HTML → PDF) support
builder.Services.AddScoped<CeyPASS.Web.Services.IRazorViewToStringRenderer, CeyPASS.Web.Services.RazorViewToStringRenderer>();

builder.Services.Configure<PlaywrightPdfOptions>(builder.Configuration.GetSection("Pdf"));
builder.Services.AddSingleton<IPlaywrightPdfService, PlaywrightPdfService>();

// In-memory cache (tek sunucu cache senaryoları için)
builder.Services.AddMemoryCache();

// Session Configuration
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Database Context - EF Core (secret repoda yok: appsettings.Local.json / User Secrets / ConnectionStrings__DefaultConnection)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString) || DatabaseHelperCore.LooksLikePlaceholder(connectionString))
{
    connectionString = DatabaseHelperCore.TryGetConnectionStringFromEnvironment();
}
if (string.IsNullOrWhiteSpace(connectionString) || DatabaseHelperCore.LooksLikePlaceholder(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection yapılandırılmadı. appsettings.Local.json, User Secrets veya ConnectionStrings__DefaultConnection ortam değişkenini kullanın.");
}

builder.Services.AddDbContext<CeyPASSDataConnectionCore>(options =>
    options.UseSqlServer(connectionString));

// HttpContextAccessor - SessionContext için gerekli
builder.Services.AddHttpContextAccessor();

// Session Context (Scoped - her HTTP request için yeni instance)
// NOT: ASP.NET Core'da SessionContext'i Scoped olarak kaydetmek daha uygun
// çünkü her HTTP request'te yeni bir instance oluşturulur ve HttpContext'e erişim sağlanabilir
builder.Services.AddScoped<ISessionContext, CeyPASS.Web.Services.SessionContext>();

// DataAccess Layer (Transient)
// EF Core repository'leri kullan (geçiş devam ediyor)
builder.Services.AddTransient<IAuthorizationRepository, AuthorizationRepositoryCore>();
builder.Services.AddTransient<IKullaniciRepository, KullaniciRepositoryCore>();
builder.Services.AddTransient<IBolumRepository, BolumRepositoryCore>();
builder.Services.AddTransient<IDepartmanRepository, DepartmanRepositoryCore>();
builder.Services.AddTransient<IFirmaRepository, FirmaRepositoryCore>();
builder.Services.AddTransient<IPozisyonRepository, PozisyonRepositoryCore>();
builder.Services.AddTransient<IIzinTipRepository, IzinTipRepositoryCore>();
builder.Services.AddTransient<IIsyeriRepository, IsyeriRepositoryCore>();
builder.Services.AddTransient<ICalismaSekliRepository, CalismaSekliRepositoryCore>();
builder.Services.AddTransient<ICalismaStatuRepository, CalismaStatuRepositoryCore>();
builder.Services.AddTransient<IResmiTatilRepository, ResmiTatilRepositoryCore>();
builder.Services.AddTransient<ICihazRepository, CihazRepositoryCore>();
builder.Services.AddTransient<ISistemLogRepository, SistemLogRepositoryCore>();
builder.Services.AddTransient<IMailRepository, MailRepositoryCore>();
builder.Services.AddTransient<IYemekhaneRepository, YemekhaneRepositoryCore>();
builder.Services.AddTransient<ICanliIzlemeRepository, CanliIzlemeRepositoryCore>();
builder.Services.AddTransient<IDashboardRepository, DashboardRepositoryCore>();
builder.Services.AddTransient<IPuantajsizKartAtamaRepository, PuantajsizKartAtamaRepositoryCore>();
builder.Services.AddTransient<IRaporRepository, RaporRepositoryCore>();
builder.Services.AddTransient<IKisiHareketRepository, KisiHareketRepositoryCore>();
builder.Services.AddTransient<IKisiIzinlerRepository, KisiIzinlerRepositoryCore>();
builder.Services.AddTransient<IKisiRepository, KisiRepositoryCore>();
builder.Services.AddTransient<IKullaniciFirmaIsyeriYetkiRepository, KullaniciFirmaIsyeriYetkiRepositoryCore>();
builder.Services.AddTransient<IKullaniciFirmaIsyeriYetkiService, KullaniciFirmaIsyeriYetkiService>();
builder.Services.AddTransient<IPuantajRepository, PuantajRepositoryCore>();

// Yeni modüller: İzin Talepleri / ÜstYetkili / Avans
builder.Services.AddTransient<IIzinTalepRepository, IzinTalepRepositoryCore>();
builder.Services.AddTransient<IUstYetkiliRepository, UstYetkiliRepositoryCore>();
builder.Services.AddTransient<IAvansRepository, AvansRepositoryCore>();
builder.Services.AddTransient<IAdminKullaniciRepository, AdminKullaniciRepositoryCore>();
builder.Services.AddTransient<IPersonelWebSifreRepository, PersonelWebSifreRepositoryCore>();
builder.Services.AddTransient<IBildirimRepository, BildirimRepositoryCore>();
builder.Services.AddTransient<IUserDeviceTokenRepository, UserDeviceTokenRepositoryCore>();

// Business Layer (Transient)
builder.Services.AddTransient<IBildirimService, BildirimManager>();
builder.Services.AddTransient<IAuthorizationService, AuthorizationService>();
builder.Services.AddTransient<ICalismaSekliService, CalismaSekliService>();
builder.Services.AddTransient<ICalismaStatuService, CalismaStatuService>();
builder.Services.AddTransient<ICanliIzlemeService, CanliIzlemeService>();
builder.Services.AddTransient<ICihazService, CihazService>();
builder.Services.AddTransient<IDashboardService, DashboardService>();
builder.Services.AddTransient<IDepartmanService, DepartmanService>();
builder.Services.AddTransient<IEmailService, CeyPASS.Web.Services.EmailServiceCore>();
builder.Services.AddTransient<IFazlaMesaiService, FazlaMesaiService>();
builder.Services.AddTransient<IFirmaService, FirmaService>();
builder.Services.AddTransient<IIsyeriService, IsyeriService>();
builder.Services.AddTransient<IIzinTipService, IzinTipService>();
builder.Services.AddTransient<IKisiDetayService, KisiDetayService>();
builder.Services.AddTransient<IKisiEkraniLookUpService, KisiEkraniLookupService>();
builder.Services.AddTransient<IKisiHareketService, KisiHareketService>();
builder.Services.AddTransient<IKisiIzinService, KisiIzinService>();
builder.Services.AddTransient<IKisiQueryService, KisiQueryService>();
builder.Services.AddTransient<IKisiService, KisiService>();
builder.Services.AddTransient<IKullaniciQueryService, KullaniciQueryService>();
builder.Services.AddTransient<IKullaniciService, KullaniciService>();
builder.Services.AddTransient<IMisafirKartService, MisafirKartService>();
builder.Services.AddTransient<IPozisyonService, PozisyonService>();
builder.Services.AddTransient<IPuantajService, PuantajService>();
builder.Services.AddTransient<IRaporService, RaporService>();
builder.Services.AddTransient<IResmiTatilService, ResmiTatilService>();
builder.Services.AddTransient<ISifreService, SifreService>();
builder.Services.AddTransient<ISistemLogService, SistemLogService>();
builder.Services.AddTransient<IMailService, MailService>();
builder.Services.AddTransient<INotificationService, NotificationService>();

builder.Services.AddTransient<IIzinTalepService, IzinTalepService>();
builder.Services.AddTransient<IAvansService, AvansService>();
builder.Services.AddTransient<IPushNotificationService, FcmPushService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
