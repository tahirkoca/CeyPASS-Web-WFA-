# 🛠️ CeyPASS Teknik Doküman

Bu doküman **yazılım geliştiricileri ve sistem yöneticileri** içindir. Son kullanıcı (İK / puantaj) adımları için modüler **[Kullanıcı Kılavuzu](Kullanici-Kilavuzu.md)** ve [`docs/kilavuz/`](kilavuz/) bölümlerine bakınız.

Bu doküman CeyPASS yazılımının teknik mimarisini, katmanlarını, teknolojilerini ve tüm istemciler (Web, WFA, WPF, Mobile) ile API için yapılandırma ve dağıtım bilgilerini açıklar.

---

## 1. 🌐 Genel Bakış

**CeyPASS**, personel takip, puantaj ve geçiş kontrolü amacıyla kullanılan kurumsal bir uygulamadır. Dört istemci arayüzü ve bir REST API sunar:

| Bileşen | Simge | Teknoloji | Veri erişimi |
|---------|-------|-----------|--------------|
| **CeyPASS.Web** | 🌐 | ASP.NET Core MVC | Doğrudan Business katmanı |
| **CeyPASS.WFA** | 🖥️ | Windows Forms (.NET 8) | Doğrudan Business katmanı |
| **CeyPASS.WPF** | 🖥️ | WPF (.NET 8), MVVM | Doğrudan Business katmanı |
| **CeyPASS.Mobile** | 📱 | Expo / React Native / TypeScript | **CeyPASS.Api** üzerinden |
| **CeyPASS.Api** | 🔌 | ASP.NET Core Web API, JWT | Business katmanı |

Web, WFA ve WPF aynı veritabanını ve iş mantığını (Business / DataAccess) doğrudan kullanır. Mobile uygulama tüm işlemleri `api/v1` REST uç noktaları üzerinden gerçekleştirir.

---

## 2. 📦 Çözüm Yapısı

| Proje | Hedef | Açıklama |
|-------|--------|-----------|
| CeyPASS.Entities | netstandard2.0 | Domain modelleri, DTO'lar, enum'lar (veritabanından bağımsız) |
| CeyPASS.DataAccess | net8.0 | EF Core DbContext, Repository'ler, veritabanı entity'leri |
| CeyPASS.Business | net8.0 | Servisler, iş kuralları |
| CeyPASS.Infrastructure | net8.0 | Yardımcı sınıflar (DatabaseHelperCore, MailHelper, EncryptionHelper vb.) |
| CeyPASS.Models | net8.0 | API istek/yanıt modelleri (`ApiResult`, Mobile DTO'ları) |
| CeyPASS.Web | net8.0 | ASP.NET Core MVC web uygulaması |
| CeyPASS.WFA | net8.0-windows | Windows Forms masaüstü uygulaması |
| CeyPASS.WPF | net8.0-windows | WPF masaüstü uygulaması (DevExpress WPF) |
| CeyPASS.Api | net8.0 | REST API (Mobile ve harici tüketiciler) |
| CeyPASS.Tests | net8.0 | xUnit birim / entegrasyon testleri |
| CeyPASSDesktop.Setup | – | Visual Studio Installer (MSI) — WFA kurulumu |
| CeyPASS.Mobile | – | Expo / React Native (Node.js; solution dışı klasör) |

---

## 3. 🏗️ Katmanlı Mimari

```
┌──────────────────────────────────────────────────────────────────────────┐
│  UI Katmanı                                                               │
│  CeyPASS.Web (MVC)  │  CeyPASS.WFA  │  CeyPASS.WPF  │  CeyPASS.Mobile   │
└──────────┬──────────┴───────┬───────┴───────┬───────┴─────────┬─────────┘
           │                   │               │                 │
           │                   │               │                 ▼
           │                   │               │         ┌───────────────┐
           └───────────────────┴───────────────┘         │ CeyPASS.Api   │
                           │                            │ (REST + JWT)  │
                           ▼                            └───────┬───────┘
┌─────────────────────────────────────────────────────────────┴────────────┐
│  CeyPASS.Business (Servisler, iş kuralları)                             │
└─────────────────────────────┬──────────────────────────────────────────┘
                              │
                              ▼
┌──────────────────────────────────────────────────────────────────────────┐
│  CeyPASS.DataAccess (EF Core DbContext, Repository'ler)                   │
└─────────────────────────────┬────────────────────────────────────────────┘
                              │
                              ▼
┌──────────────────────────────────────────────────────────────────────────┐
│  CeyPASS.Entities (Modeller, DTO'lar, enum'lar)                           │
└──────────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────────┐
│  CeyPASS.Infrastructure (Ortak yardımcılar)                                │
└──────────────────────────────────────────────────────────────────────────┘
```

- **Web**, **WFA** ve **WPF** doğrudan **Business** katmanını kullanır; DataAccess'e referans vermez (Business üzerinden erişir).
- **Mobile** → **Api** → **Business** zinciri izlenir.
- **Api**, **Models** + **Business** + **DataAccess** + **Entities** + **Infrastructure** referanslarına sahiptir.
- **Business** hem **DataAccess** hem **Entities**'e referans verir.
- **DataAccess** veritabanı işlemlerini yapar; dışarıya **Entities.Concrete** tiplerini döner.

---

## 4. 🗄️ Veri Modelleri (Entities vs DataAccess)

### 4.1 İki tür entity

| Konum | Örnek tipler | Kullanım |
|-------|---------------|----------|
| **CeyPASS.DataAccess** (root .cs) | `Kisiler`, `Firmalar`, `Departmanlar` | EF Core DbContext içinde; veritabanı tablolarına 1:1 |
| **CeyPASS.Entities.Concrete** | `Kisi`, `Firma`, `KisiListItem`, `KisiDetayDTO` | Business ve UI arasında ortak sözleşme tipi |
| **CeyPASS.Models** | `ApiResult<T>`, Mobile istek/yanıt tipleri | Api controller sözleşmeleri |

### 4.2 Veri akışı

1. **Okuma:** Veritabanı → DataAccess entity (`Kisiler` vb.) → Repository map → **Entities** tipi (`Kisi`, `KisiListItem` vb.) → Business → UI / Api.
2. **Yazma:** UI / Api → Business → **Entities** tipi → Repository map → DataAccess entity → Veritabanı.

Entities katmanındaki modeller veritabanı şemasına bağımlı değildir; katmanlar arası veri taşıma bu tiplerle yapılır.

---

## 5. ⚙️ Teknolojiler

| Bileşen | Teknoloji |
|---------|-----------|
| Çerçeve | .NET 8 |
| Veritabanı erişimi | Entity Framework Core 8, SQL Server |
| Web UI | ASP.NET Core MVC, Bootstrap 5, DataTables, Toastr |
| Masaüstü UI (WFA) | Windows Forms (.NET 8) |
| Masaüstü UI (WPF) | WPF, DevExpress WPF Grid/Printing, MVVM |
| Mobile UI | Expo, React Native, TypeScript, NativeWind |
| API | ASP.NET Core Web API, JWT Bearer, Swagger |
| Bağımlılık enjeksiyonu | Microsoft.Extensions.DependencyInjection |
| Yapılandırma (Web / WFA / WPF / Api) | appsettings.json, IConfiguration |
| Yapılandırma (WFA fallback) | App.config (ConnectionString, SMTP) |
| Masaüstü güncelleme | AutoUpdater.NET (WFA ve WPF) |
| Kurulum (WFA) | Visual Studio Installer (.vdproj → MSI) |
| Test | xUnit, Moq, FluentAssertions |

---

## 6. 🔧 Yapılandırma

### 6.1 🌐 Web (CeyPASS.Web)

- **appsettings.json** / **appsettings.Development.json**: Connection string, SMTP, uygulama ayarları.
- **appsettings.Local.json** (gitignore): Yerel gerçek bağlantı (opsiyonel).
- Bağlantı adı: `DefaultConnection`.
- Yer tutucu (`YOUR_SERVER` / `YOUR_PASSWORD` vb.) veya boş ise: ortam değişkeni `ConnectionStrings__DefaultConnection` (veya `CEYPASS_DEFAULT_CONNECTION`) kullanılabilir; aksi halde uygulama başlangıçta açıklayıcı hata verir.

### 6.2 🔌 API (CeyPASS.Api)

- **appsettings.json**: Varsayılan bağlantı şablonu, JWT ayarları, CORS.
- **appsettings.Local.json** (gitignore): Gerçek SQL Server bağlantısı ve JWT anahtarı için.
- Ortam: `ConnectionStrings__DefaultConnection`, `CEYPASS_DEFAULT_CONNECTION`, `Jwt__Key`.
- Tüm controller'lar `api/v1/[controller]` route öneki altındadır.
- JWT imza anahtarı repoda tutulmaz; ortam değişkeni veya Local dosyadan okunur.
- Swagger: `/swagger` (Development veya yapılandırmaya göre).
- Mobile istemci base URL: `https://{sunucu}/api/v1` (`CeyPASS.Mobile/services/api.ts`).

### 6.3 🖥️ Masaüstü (CeyPASS.WFA)

- **appsettings.json**: Öncelikli; `ConnectionStrings.DefaultConnection`, `SmtpSettings`.
- **App.config**: Fallback; `connectionStrings`, `appSettings` (SMTP). Program.cs önce appsettings.json'a bakar, boş veya geçersizse App.config kullanılır.
- Bağlantı sırası: appsettings.json → App.config → ortam (`ConnectionStrings__DefaultConnection` / `CEYPASS_DEFAULT_CONNECTION`). Repoda gömülü şifre/sunucu yoktur.

### 6.4 🖥️ Masaüstü (CeyPASS.WPF)

- **appsettings.json**: WFA ile aynı mantık; `ConnectionStrings.DefaultConnection`, SMTP.
- **appsettings.Local.json** (gitignore, opsiyonel): Yerel geliştirme bağlantısı.
- Çıktı klasörüne `appsettings.json` kopyalanır (`CopyToOutputDirectory`).
- AutoUpdater.NET ile güncelleme (WFA ile benzer `update.xml` akışı).

### 6.5 📱 Mobile (CeyPASS.Mobile)

- **app.json** / Expo yapılandırması: uygulama meta verileri.
- **services/api.ts**: API base URL çözümlemesi:
  - `EXPO_PUBLIC_API_URL` ortam değişkeni (tercih edilen),
  - geliştirmede Metro packager IP + port 5126 (`/api/v1` normalize edilir),
  - oturum token'ı `Authorization: Bearer` header ile gönderilir.
- Filtre tercihleri: Expo SecureStore (`services/pageFilterPrefs.ts`).

---

## 7. 🔐 Yetkilendirme

- Sayfa bazlı yetkiler: **YetkiTipleri** (View, Create, Update, Delete, Export, Approve) enum'u ile tanımlı.
- **CeyPASS.Business** içindeki **AuthorizationService** ile kontrol edilir.
- **Web**, **WFA**, **WPF** ve **Mobile** menü öğeleri kullanıcının yetkisine göre gizlenir/gösterilir (ör. `ViewAbility("Personeller")`).
- **Api** controller'ları `[Authorize]` ile korunur; JWT token login sonrası `AuthController` üzerinden alınır.
- Mobile abilities (view/create/update vb.) login yanıtında döner ve istemci tarafında menü filtrelemesi yapılır.

---

## 8. 🎨 Çoklu İstemci UX Altyapısı

Aşağıdaki tablo, son sürümde ortak kullanıcı deneyimi bileşenlerinin teknik konumlarını özetler.

| Özellik | WPF | WFA | Web | Mobile |
|---------|-----|-----|-----|--------|
| **↩️ Geri al** | `UiToast.SuccessWithUndo`, `UiDialog` | `UiUndo` (durum çubuğu) | `CeyPASS.undo` (`site.js`) | `StatusPopup` `onUndo` |
| **⌨️ Kısayollar** | `ShortcutCatalog`, `MainWindow` overlay | `UiShortcutsForm`, `ShortcutCatalog` | `CeyPASS.shortcuts`, `_Layout` modal | — |
| **💡 Sayfa rehberi** | `CeypassHelpTip`, `PageHelpCatalog` | — | — | `TipsSheet`, yan menü «İpuçları» |
| **🔍 Filtre tercihleri** | `PageFilterPrefsStore` (`%LocalAppData%\CeyPASS\`) | aynı desen | — | `pageFilterPrefs.ts` (SecureStore) |
| **📊 Durum çubuğu** | `UiStatus` | `islemEkrani` alt bar | `#ceypassStatusBar` | `App.tsx` status mesajı |

### 8.1 ↩️ Geri al (Undo) akışları

Dört işlemde soft-delete / pasife alma sonrası ~7 saniyelik tek seferlik Geri al penceresi sunulur:

| İşlem | Business metodu | Api uç noktası (Mobile) |
|-------|-----------------|-------------------------|
| 👤 Personel işten çıkış | `KisiService.KisiTekrarAktifEt` | `POST /api/v1/Personel/tekrar-aktif-et` |
| 🗓️ İzin pasife alma | `KisiIzinService.AktifYap` | `POST /api/v1/Izin/{id}/aktif` |
| 🚪 Kişi hareket pasife alma | `KisiHareketService.AktifYap` | `POST /api/v1/KisiHareket/{id}/aktif` |
| 🔌 Cihaz pasife alma | `CihazService.AktifYap` | `POST /api/v1/Cihaz/{id}/aktif` |

Web ve masaüstü istemciler aynı Business servislerini doğrudan çağırır; Mobile ilgili Api controller'ları kullanır.

**Kaynak dosyalar:**

- Business: `CeyPASS.Business/Services/KisiIzinService.cs`, `KisiHareketService.cs`, `CihazService.cs`, `KisiService.cs`
- Api: `CeyPASS.Api/Controllers/IzinController.cs`, `KisiHareketController.cs`, `CihazController.cs`, `PersonelController.cs`
- Web: `CeyPASS.Web/wwwroot/js/site.js` (`CeyPASS.undo`)
- WPF: `CeyPASS.WPF/UiToast.cs`, ilgili ViewModels
- WFA: `CeyPASS.WFA/UiUndo.cs`
- Mobile: `CeyPASS.Mobile/components/StatusPopup.tsx`, ekran servisleri (`izinApi.ts`, `kisiHareketApi.ts` vb.)

### 8.2 ⌨️ Klavye kısayolları

Web, WPF ve WFA'da üst başlıkta **klavye simgesi** ile açılır; **F1** veya **Ctrl+/** kısayolu da geçerlidir. **Esc** paneli kapatır.

- WPF: `CeyPASS.WPF/ShortcutCatalog.cs`, `Views/MainWindow.xaml`
- WFA: `CeyPASS.WFA/UiShortcutsForm.cs`, `ShortcutCatalog.cs`
- Web: `CeyPASS.Web/wwwroot/js/site.js` (`CeyPASS.shortcuts`), `Views/Shared/_Layout.cshtml`

Sayfa bazlı ek kısayollar `data-page-shortcuts` attribute veya `ShortcutCatalog.ForPage(pageKey)` ile tanımlanır.

### 8.3 💡 Sayfa rehberi / ipuçları

- **WPF:** Her sayfada `CeypassHelpTip` kontrolü (`?` simgesi → «İşlem rehberi» popup). **Esc** veya **✕** ile kapanır. İçerik `PageHelpCatalog.cs` üzerinden gelir.
- **Mobile:** Yan menüden «İpuçları» → `TipsSheet`. Geri tuşu veya **✕** ile kapanır.
- **Web / WFA:** Sayfa rehberi bileşeni yoktur.

> **💡 Not:** Klavye simgesi = kısayol listesi; `?` (WPF) = işlem rehberi. Bu ayrım kullanıcı arayüzünde bilinçli olarak korunur.

---

## 9. 🚀 Dağıtım

### 9.1 🌐 Web

- ASP.NET Core uygulaması; IIS veya Kestrel ile yayınlanır.
- Yayın klasörü: CeyPASS.Web çıktısı (`dotnet publish`).
- Veritabanı bağlantı bilgisi appsettings.json (veya ortam değişkenleri) ile sağlanır.

### 9.2 🖥️ Masaüstü (WFA)

- **CeyPASSDesktop.Setup** projesi ile MSI üretilir.
- Kurulum sonrası kısayollar masaüstü ve Başlat menüsüne eklenir.
- Güncelleme: Uygulama başlarken AutoUpdater ile sunucudaki `update.xml` kontrol edilir; güncelleme zorunlu değildir.

### 9.3 🖥️ Masaüstü (WPF)

- `dotnet publish CeyPASS.WPF/CeyPASS.WPF.csproj -c Release` ile klasör yayını alınır.
- `appsettings.json` (ve isteğe bağlı `appsettings.Local.json`) çıktı klasöründe bulunmalıdır.
- AutoUpdater.NET ile WFA'ya benzer güncelleme akışı desteklenir.
- DevExpress lisansı dağıtım ortamında geçerli olmalıdır.

### 9.4 🔌 API

- IIS reverse proxy veya doğrudan Kestrel (`5126` geliştirme portu).
- `dotnet publish CeyPASS.Api/CeyPASS.Api.csproj -c Release`.
- JWT anahtarı ve connection string ortam değişkenleri veya `appsettings.Local.json` ile sağlanır.
- Mobile istemcinin erişebileceği HTTPS URL ve CORS ayarları yapılandırılmalıdır.

### 9.5 📱 Mobile

- Expo / EAS Build ile Android (APK/AAB) veya iOS paketi üretilir.
- `EXPO_PUBLIC_API_URL` production Api adresine işaret etmelidir (ör. `https://sunucu/api/v1`).
- Api sunucusu Mobile cihazlardan erişilebilir olmalıdır (LAN veya VPN).

---

## 10. 🔗 Proje Referansları (Özet)

- **Entities**: Hiçbir projeye referans vermez.
- **Models**: Hiçbir CeyPASS projesine referans vermez (saf DTO).
- **DataAccess**: Entities.
- **Business**: DataAccess, Entities.
- **Infrastructure**: Business, Entities.
- **Web**: Business, DataAccess, Entities, Infrastructure.
- **WFA**: Business, DataAccess, Entities, Infrastructure.
- **WPF**: Business, DataAccess, Entities, Infrastructure.
- **Api**: Models, Business, DataAccess, Entities, Infrastructure.
- **Tests**: Business, DataAccess, Entities, Infrastructure, Web.
- **Mobile**: Bağımsız Node/Expo projesi; yalnızca Api'ye HTTP ile bağlanır.

---

Bu doküman CeyPASS'in teknik yapısını özetler; geliştirme ve bakım sırasında referans olarak kullanılabilir. Son güncelleme: dört istemci (🌐 Web, 🖥️ WFA, 🖥️ WPF, 📱 Mobile) ve REST API mimarisini yansıtır.
