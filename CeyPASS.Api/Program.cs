using System.Collections.Generic;
using System.Text;
using CeyPASS.Api.Infrastructure;
using CeyPASS.Api.Services;
using CeyPASS.Business.Abstractions;
using CeyPASS.Business.Services;
using CeyPASS.DataAccess;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.DataAccess.Repositories;
using CeyPASS.Infrastructure.Helpers;
using CeyPASS.Infrastructure.Pdf;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// PDF export (MigraDoc/PdfSharp) için Windows'ta sistem fontlarını kullan.
// PdfSharp font çözümleme ilk kullanımda cache'lendiği için bunu uygulama başlangıcında yapmak kritik.
ExportHelper.ConfigurePdfFonts();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// JWT imza anahtarı: repoda tutulmaz. Öncelik: Jwt__Key ortam değişkeni → appsettings → Development varsayılanı.
var jwtSigningKey = ResolveJwtSigningKey(builder);
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["Jwt:Key"] = jwtSigningKey
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();

// In-Memory Cache
builder.Services.AddMemoryCache();

// Global Exception Handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Swagger Documentation with JWT Support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CeyPASS Mobil API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { }
        }
    });
});

// Database Context (secret repoda yok: appsettings.Local.json / User Secrets / ConnectionStrings__DefaultConnection)
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

// HttpContextAccessor for ApiSessionContext
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ISessionContext, ApiSessionContext>();

// JWT Authentication (jwtSigningKey tüm uygulama için Configuration üzerinden erişilebilir)
var jwtKey = jwtSigningKey;
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// Register Dependencies (Repositories & Services)
// We should ideally move this to a shared extension method in Business or Infrastructure.
// For now, mirroring Web's registrations to ensure exact functionality.
RegisterCeyPassServices(builder.Services);

// Razor view render (HTML → PDF) support
builder.Services.AddScoped<CeyPASS.Api.Services.IRazorViewToStringRenderer, CeyPASS.Api.Services.RazorViewToStringRenderer>();

builder.Services.Configure<PlaywrightPdfOptions>(builder.Configuration.GetSection("Pdf"));
builder.Services.AddSingleton<IPlaywrightPdfService, PlaywrightPdfService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CeyPASS Mobil API v1"));
}

app.UseExceptionHandler(); // This will use the registered GlobalExceptionHandler

app.UseCors("AllowAll");
// Dev'de fiziksel cihazdan LAN üstünden test için HTTP'yi açık bırakıyoruz.
// Aksi halde http -> https redirect self-signed sertifika nedeniyle mobilde düşebiliyor.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static string ResolveJwtSigningKey(WebApplicationBuilder builder)
{
    var fromEnv = Environment.GetEnvironmentVariable("Jwt__Key") ?? Environment.GetEnvironmentVariable("JWT__KEY");
    if (!string.IsNullOrWhiteSpace(fromEnv))
        return fromEnv.Trim();

    var fromConfig = builder.Configuration["Jwt:Key"];
    if (!string.IsNullOrWhiteSpace(fromConfig))
        return fromConfig.Trim();

    if (builder.Environment.IsDevelopment())
        return "CeyPASS_Development_Only_JWT_Signing_Key__ReplaceInProd__";

    throw new InvalidOperationException(
        "Production Jwt signing key is not configured. Set environment variable Jwt__Key (or JWT__KEY) or Jwt:Key in appsettings / secrets manager.");
}

void RegisterCeyPassServices(IServiceCollection services)
{
    // DataAccess Layer
    services.AddTransient<IAuthorizationRepository, AuthorizationRepositoryCore>();
    services.AddTransient<IKullaniciRepository, KullaniciRepositoryCore>();
    services.AddTransient<IBolumRepository, BolumRepositoryCore>();
    services.AddTransient<IDepartmanRepository, DepartmanRepositoryCore>();
    services.AddTransient<IFirmaRepository, FirmaRepositoryCore>();
    services.AddTransient<IPozisyonRepository, PozisyonRepositoryCore>();
    services.AddTransient<IIzinTipRepository, IzinTipRepositoryCore>();
    services.AddTransient<IIsyeriRepository, IsyeriRepositoryCore>();
    services.AddTransient<ICalismaSekliRepository, CalismaSekliRepositoryCore>();
    services.AddTransient<ICalismaStatuRepository, CalismaStatuRepositoryCore>();
    services.AddTransient<IResmiTatilRepository, ResmiTatilRepositoryCore>();
    services.AddTransient<ICihazRepository, CihazRepositoryCore>();
    services.AddTransient<ISistemLogRepository, SistemLogRepositoryCore>();
    services.AddTransient<IMailRepository, MailRepositoryCore>();
    services.AddTransient<ICanliIzlemeRepository, CanliIzlemeRepositoryCore>();
    services.AddTransient<IDashboardRepository, DashboardRepositoryCore>();
    services.AddTransient<IRaporRepository, RaporRepositoryCore>();
    services.AddTransient<IKisiHareketRepository, KisiHareketRepositoryCore>();
    services.AddTransient<IKisiIzinlerRepository, KisiIzinlerRepositoryCore>();
    services.AddTransient<IKisiRepository, KisiRepositoryCore>();
    services.AddTransient<IPuantajRepository, PuantajRepositoryCore>();
    services.AddTransient<IIzinTalepRepository, IzinTalepRepositoryCore>();
    services.AddTransient<IUstYetkiliRepository, UstYetkiliRepositoryCore>();
    services.AddTransient<IAvansRepository, AvansRepositoryCore>();
    services.AddTransient<IYemekhaneRepository, YemekhaneRepositoryCore>();
    services.AddTransient<IPuantajsizKartAtamaRepository, PuantajsizKartAtamaRepositoryCore>();
    services.AddTransient<IAdminKullaniciRepository, AdminKullaniciRepositoryCore>();
    services.AddTransient<IPersonelWebSifreRepository, PersonelWebSifreRepositoryCore>();
    services.AddTransient<IBildirimRepository, BildirimRepositoryCore>();
    services.AddTransient<IUserDeviceTokenRepository, UserDeviceTokenRepositoryCore>();

    // Business Layer
    services.AddTransient<IBildirimService, BildirimManager>();
    services.AddTransient<IAuthorizationService, AuthorizationService>();
    services.AddTransient<ICalismaSekliService, CalismaSekliService>();
    services.AddTransient<ICalismaStatuService, CalismaStatuService>();
    services.AddTransient<ICanliIzlemeService, CanliIzlemeService>();
    services.AddTransient<ICihazService, CihazService>();
    services.AddTransient<IDashboardService, DashboardService>();
    services.AddTransient<IDepartmanService, DepartmanService>();
    services.AddTransient<IFazlaMesaiService, FazlaMesaiService>();
    services.AddTransient<IFirmaService, FirmaService>();
    services.AddTransient<IIsyeriService, IsyeriService>();
    services.AddTransient<IIzinTipService, IzinTipService>();
    services.AddTransient<IKisiDetayService, KisiDetayService>();
    services.AddTransient<IKisiHareketService, KisiHareketService>();
    services.AddTransient<IKisiIzinService, KisiIzinService>();
    services.AddTransient<IKisiQueryService, KisiQueryService>();
    services.AddTransient<IKisiService, KisiService>();
    services.AddTransient<IKullaniciQueryService, KullaniciQueryService>();
    services.AddTransient<IKullaniciService, KullaniciService>();
    services.AddTransient<IMisafirKartService, MisafirKartService>();
    services.AddTransient<IPozisyonService, PozisyonService>();
    services.AddTransient<IPuantajService, PuantajService>();
    services.AddTransient<IRaporService, RaporService>();
    services.AddTransient<IResmiTatilService, ResmiTatilService>();
    services.AddTransient<ISifreService, SifreService>();
    services.AddTransient<ISistemLogService, SistemLogService>();
    services.AddTransient<IMailService, MailService>();
    services.AddTransient<INotificationService, NotificationService>();
    services.AddTransient<IIzinTalepService, IzinTalepService>();
    services.AddTransient<IAvansService, AvansService>();
    services.AddTransient<IEmailService, ApiEmailService>();
    services.AddTransient<IKisiEkraniLookUpService, KisiEkraniLookupService>();
    services.AddTransient<IPushNotificationService, FcmPushService>();
    services.AddTransient<IMobileQrService, MobileQrService>();
}
