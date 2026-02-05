using CeyPASS.Business.Abstractions;
using CeyPASS.Business.Services;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.DataAccess;
using CeyPASS.DataAccess.Repositories;
using CeyPASS.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using CeyPASS.Web.Data;
using CeyPASS.Web.Services;

// PDF export (MigraDoc) için Windows'ta Arial vb. sistem fontlarının kullanılması - ilk PDF'den önce ayarlanmalı
ExportHelper.ConfigurePdfFonts();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Session Configuration
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Database Context - EF Core
// Connection string appsettings.json'dan veya Infrastructure.DatabaseHelperCore'dan alınabilir
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("YOUR_PASSWORD"))
{
    connectionString = CeyPASS.Infrastructure.Helpers.DatabaseHelperCore.GetSqlConnectionString("CeyPASS.Web");
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
builder.Services.AddTransient<IPuantajsizKartRepository, PuantajsizKartRepositoryCore>();
builder.Services.AddTransient<IPuantajsizKartAtamaRepository, PuantajsizKartAtamaRepositoryCore>();
builder.Services.AddTransient<IRaporRepository, RaporRepositoryCore>();
builder.Services.AddTransient<IKisiHareketRepository, KisiHareketRepositoryCore>();
builder.Services.AddTransient<IKisiIzinlerRepository, KisiIzinlerRepositoryCore>();
builder.Services.AddTransient<IKisiRepository, KisiRepositoryCore>();
builder.Services.AddTransient<IPuantajRepository, PuantajRepositoryCore>();

// Business Layer (Transient)
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
