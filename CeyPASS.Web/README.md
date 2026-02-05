# CeyPASS Web Application

Bu proje, CeyPASS WinForms uygulamasının .NET Core MVC web versiyonudur.

## Gereksinimler

- .NET 8.0 SDK
- SQL Server
- Visual Studio 2022 veya VS Code

## Kurulum

1. Connection string'i `appsettings.json` dosyasında güncelleyin:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=CeyPASS;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=True;Encrypt=True"
  }
}
```

2. Projeyi çalıştırın:
```bash
dotnet run --project CeyPASS.Web
```

3. Tarayıcıda `https://localhost:5001` adresine gidin.

## Önemli Notlar

⚠️ **Entity Framework Geçişi Gerekli**

Bu proje şu anda Entity Framework 6 kullanıyor. EF Core'a geçiş yapılması gerekiyor. Detaylar için `MIGRATION_GUIDE.md` dosyasına bakın.

## Proje Yapısı

```
CeyPASS.Web/
├── Controllers/          # MVC Controllers
├── Views/               # Razor Views
├── wwwroot/            # Static files (CSS, JS, images)
├── Program.cs          # Application startup
└── appsettings.json    # Configuration
```

## Özellikler

- ✅ MVC yapısı
- ✅ Dependency Injection
- ✅ Session yönetimi
- ✅ Giriş sayfası
- ⏳ EF Core geçişi (devam ediyor)
- ⏳ Tüm sayfaların dönüşümü (devam ediyor)

## Geliştirme

Detaylı geçiş planı için `MIGRATION_GUIDE.md` dosyasına bakın.
