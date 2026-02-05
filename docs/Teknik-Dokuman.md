# CeyPASS Teknik Doküman

Bu doküman CeyPASS yazılımının teknik mimarisini, katmanlarını, teknolojilerini ve her iki platform (Web ve Masaüstü / WFA) için yapılandırma ve dağıtım bilgilerini açıklar.

---

## 1. Genel Bakış

**CeyPASS**, personel takip, puantaj ve geçiş kontrolü amacıyla kullanılan kurumsal bir uygulamadır. İki arayüz sunar:

- **CeyPASS.Web** – Tarayıcı tabanlı ASP.NET Core MVC uygulaması
- **CeyPASS.WFA** – Windows Forms masaüstü uygulaması

Her iki arayüz de aynı veritabanını ve aynı iş mantığını (Business / DataAccess katmanları) kullanır.

---

## 2. Çözüm Yapısı

| Proje | Hedef | Açıklama |
|-------|--------|-----------|
| CeyPASS.Entities | net8.0 | Domain modelleri, DTO'lar, enum'lar (veritabanından bağımsız) |
| CeyPASS.DataAccess | net8.0 | EF Core DbContext, Repository'ler, veritabanı entity'leri |
| CeyPASS.Business | net8.0 | Servisler, iş kuralları |
| CeyPASS.Infrastructure | net8.0 | Yardımcı sınıflar (DatabaseHelperCore, MailHelper, EncryptionHelper vb.) |
| CeyPASS.Web | net8.0 | ASP.NET Core MVC web uygulaması |
| CeyPASS.WFA | net8.0-windows | Windows Forms masaüstü uygulaması |
| CeyPASSDesktop.Setup | – | Visual Studio Installer (MSI) kurulum projesi |

---

## 3. Katmanlı Mimari

```
┌─────────────────────────────────────────────────────────────────┐
│  UI Katmanı                                                      │
│  CeyPASS.Web (MVC)          │  CeyPASS.WFA (WinForms)            │
└─────────────────────────────┼────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  CeyPASS.Business (Servisler, iş kuralları)                     │
└─────────────────────────────┬────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  CeyPASS.DataAccess (EF Core DbContext, Repository'ler)          │
└─────────────────────────────┬────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  CeyPASS.Entities (Modeller, DTO'lar, enum'lar)                  │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  CeyPASS.Infrastructure (Ortak yardımcılar)                      │
└─────────────────────────────────────────────────────────────────┘
```

- **Web** ve **WFA** doğrudan **Business** katmanını kullanır; DataAccess'e referans vermez (Business üzerinden erişir).
- **Business** hem **DataAccess** hem **Entities**'e referans verir.
- **DataAccess** veritabanı işlemlerini yapar; dışarıya **Entities.Concrete** tiplerini döner.

---

## 4. Veri Modelleri (Entities vs DataAccess)

### 4.1 İki tür entity

| Konum | Örnek tipler | Kullanım |
|-------|---------------|----------|
| **CeyPASS.DataAccess** (root .cs) | `Kisiler`, `Firmalar`, `Departmanlar` | EF Core DbContext içinde; veritabanı tablolarına 1:1 |
| **CeyPASS.Entities.Concrete** | `Kisi`, `Firma`, `KisiListItem`, `KisiDetayDTO` | Business ve UI arasında ortak sözleşme tipi |

### 4.2 Veri akışı

1. **Okuma:** Veritabanı → DataAccess entity (`Kisiler` vb.) → Repository map → **Entities** tipi (`Kisi`, `KisiListItem` vb.) → Business → UI.
2. **Yazma:** UI → Business → **Entities** tipi → Repository map → DataAccess entity → Veritabanı.

Entities katmanındaki modeller veritabanı şemasına bağımlı değildir; tüm katmanlar arası veri taşıma bu tiplerle yapılır.

---

## 5. Teknolojiler

| Bileşen | Teknoloji |
|---------|-----------|
| Çerçeve | .NET 8 |
| Veritabanı erişimi | Entity Framework Core 8, SQL Server |
| Web UI | ASP.NET Core MVC, Bootstrap 5 |
| Masaüstü UI | Windows Forms (.NET 8) |
| Bağımlılık enjeksiyonu | Microsoft.Extensions.DependencyInjection |
| Yapılandırma (Web) | appsettings.json, IConfiguration |
| Yapılandırma (WFA) | appsettings.json (öncelikli), App.config (fallback: ConnectionString, SMTP) |
| Masaüstü güncelleme | AutoUpdater.NET (sunucudaki update.xml) |
| Kurulum | Visual Studio Installer (.vdproj → MSI) |

---

## 6. Yapılandırma

### 6.1 Web (CeyPASS.Web)

- **appsettings.json** / **appsettings.Development.json**: Connection string, SMTP, uygulama ayarları.
- Bağlantı adı: `DefaultConnection`.
- Veritabanı bağlantısı ek olarak **CeyPASS.Web.Data.DatabaseHelperCore** üzerinden Infrastructure ile uyumlu şekilde alınabilir.

### 6.2 Masaüstü (CeyPASS.WFA)

- **appsettings.json**: Öncelikli; `ConnectionStrings.DefaultConnection`, `SmtpSettings`.
- **App.config**: Fallback; `connectionStrings`, `appSettings` (SMTP). Program.cs önce appsettings.json'a bakar, boş veya geçersizse App.config kullanılır.
- Bağlantı sırası: appsettings.json → App.config → Infrastructure.DatabaseHelperCore.GetSqlConnectionString("CeyPASS.WFA").

---

## 7. Yetkilendirme

- Sayfa bazlı yetkiler: **YetkiTipleri** (View, Create, Update, Delete, Export, Approve) enum'u ile tanımlı.
- **CeyPASS.Business** içindeki **AuthorizationService** ile kontrol edilir.
- Web ve WFA'da menü öğeleri kullanıcının yetkisine göre gizlenir/gösterilir (ör. ViewAbility("Personeller")).

---

## 8. Dağıtım

### 8.1 Web

- ASP.NET Core uygulaması; IIS veya Kestrel ile yayınlanır.
- Yayın klasörü: CeyPASS.Web çıktısı (dotnet publish).
- Veritabanı bağlantı bilgisi appsettings.json (veya ortam değişkenleri) ile sağlanır.

### 8.2 Masaüstü (WFA)

- **CeyPASSDesktop.Setup** projesi ile MSI üretilir.
- Kurulum sonrası kısayollar masaüstü ve Başlat menüsüne eklenir.
- Güncelleme: Uygulama başlarken AutoUpdater ile sunucudaki `update.xml` (ör. `http://192.168.0.23/CeyPASS-Updates/update.xml`) kontrol edilir; güncelleme zorunlu değildir, atlama / sonra hatırlat seçenekleri vardır.

---

## 9. Proje Referansları (Özet)

- **Entities**: Hiçbir projeye referans vermez.
- **DataAccess**: Entities.
- **Business**: DataAccess, Entities.
- **Infrastructure**: Business, Entities.
- **Web**: Business, DataAccess, Entities, Infrastructure.
- **WFA**: Business, DataAccess, Entities, Infrastructure.

Bu doküman CeyPASS'in teknik yapısını özetler; geliştirme ve bakım sırasında referans olarak kullanılabilir.
