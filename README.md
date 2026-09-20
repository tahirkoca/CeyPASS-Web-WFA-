🌍 Language / Dil: [Türkçe](#turkce) | [English](#english)

---

<a name="turkce"></a>

# 🇹🇷 CeyPASS (Web, WFA, WPF & Mobil)

![CeyPASS](./CeyPASS.WPF/Assets/CeyPass200.png)

**CeyPASS**, Cey Holding için geliştirilmiş kapsamlı bir **Personel Devam Kontrol Sistemidir (PDKS)**.
Proje, tek bir merkezi sisteme bağlı çalışan modern bir **Web arayüzü**, **Windows Forms (WFA)** ve **WPF** masaüstü istemcileri ile **iOS/Android üzerinde Expo ile geliştirilmiş mobil istemci** (`CeyPASS.Mobile`) içerir; tüm istemciler ortak iş mantığına (`CeyPASS.Business`) ve REST **`CeyPASS.Api`** katmanına bağlanır.
Web, WFA ve WPF **birebir aynı yeteneklere** yakın bir parite hedefler; mobil uygulama API üzerinden aynı veri ve iş kurallarını kullanır. Güncel masaüstü arayüzü **WPF**’tir (`docs/kilavuz/` ekran görüntüleri WPF’ten alınır).

## 🚀 Özellikler

Hem Web hem de masaüstü (WFA / WPF) üzerinden aşağıdaki tüm işlemleri yapabilirsiniz:

*   **Personel Yönetimi:** Detaylı özlük dosyaları, işe giriş-çıkış işlemleri.
*   **İzin İşlemleri:** İzin talebi, onayı ve takibi.
*   **Canlı İzleme:** Turnike ve cihazlardan anlık geçiş verilerinin takibi.
*   **Donanım Yönetimi:** Cihazlara uzaktan kart gönderme, kapı açma, veri çekme.
*   **Raporlama:** Gelişmiş PDKS raporları ve grafikler.
*   **Kart İşlemleri:** Kart atama, yetkilendirme ve güncelleme.
*   **Bildirimler:** SignalR ile anlık sistem bildirimleri.
*   **Mobil QR Geçiş Sistemi:** Personelin mobil uygulama üzerinden cihazlardaki QR kodları okutarak hızlı ve güvenli geçiş yapabilmesi.
*   **Güvenlik Katmanları:**
    *   **Geofencing (Konum Doğrulama):** İşlem sırasında personelin cihazın fiziksel konumuna yakınlığı (varsayılan 50m) kontrol edilir.
    *   **Sahte Konum (Mock Location) Koruması:** Fake GPS uygulamalarıyla yapılan girişimler anında tespit edilir ve reddedilir.
    *   **Gerçek Cihaz Kontrolü:** İşlemlerin emülatörler üzerinden değil, yalnızca fiziksel mobil cihazlar üzerinden yapılması zorunlu kılınmıştır.
    *   **Mükerrer Kayıt Engelleme:** 5 saniye içindeki ardışık denemeler filtrelenerek sistem yükü ve hatalı kayıtlar önlenir.
    *   **JWT & API Güvenliği:** Mobil iletişim tamamen şifrelenmiş JWT tokenları üzerinden yürütülür.

## 🛠 Mimari ve Teknolojiler

Proje, **Business, DataAccess, Entities ve Infrastructure** katmanlarından oluşan **Nx Katmanlı Mimari (N-Layered Architecture)** üzerine inşa edilmiştir. Bu sayede tüm iş mantığı ortaktır.

*   **Core:** .NET 8
*   **Arayüz katmanları:** ASP.NET Core MVC (Web), Windows Forms (WFA), WPF (DevExpress), Expo / React Native (Mobile)
*   **API:** ASP.NET Core Web API, JWT, Swagger (`api/v1`)
*   **Veritabanı:** Microsoft SQL Server (Entity Framework Core)
*   **Gerçek zamanlı iletişim:** SignalR
*   **Ortak yapı:** Dependency Injection, Repository Pattern, katmanlı mimari (Business, DataAccess, Entities, Infrastructure)

## 📚 Dokümantasyon

| Doküman | Hedef kitle | Kaynak | PDF çıktısı |
|---------|-------------|--------|-------------|
| [Kullanıcı Kılavuzu](docs/Kullanici-Kilavuzu.md) | İK, puantaj, idari işler | `docs/kilavuz/` (Web / WPF / Mobil) | `docs/output/Kullanici-Kilavuzu.pdf` |
| [Teknik Doküman](docs/Teknik-Dokuman.md) | Yazılım geliştiriciler, sistem yöneticileri | Mimari, katmanlar, yapılandırma | `docs/output/Teknik-Dokuman.pdf` |

<a name="gelistirici-kurulumu"></a>

## Geliştirici kurulumu

### Önkoşullar

*   **Windows** (WFA, WPF ve önerilen geliştirme ortamı; Web/API diğer işletim sistemlerinde de derlenebilir)
*   [.NET 8 SDK](https://dotnet.microsoft.com/download)
*   [Node.js](https://nodejs.org/) LTS (`CeyPASS.Mobile` / Expo için)
*   Erişebildiğiniz bir **Microsoft SQL Server** (şema kurulumu için)

### Veritabanı şeması

1.  SQL Server’da `database/CeyPASSDBScript.sql` dosyasını çalıştırın.
2.  `CeyPASS` veritabanının oluştuğundan emin olun.

### .NET yapılandırması (Api, Web, WFA, WPF)

Repodaki `appsettings.json` dosyalarında veritabanı için **şablon** (`YOUR_SERVER`, `YOUR_USER`, `YOUR_PASSWORD`) bulunur. Yerelde çalıştırmak için:

1.  İlgili projede `appsettings.Local.json.example` dosyasını **`appsettings.Local.json`** adıyla kopyalayın (`CeyPASS.Api`, `CeyPASS.Web`, `CeyPASS.WFA`, `CeyPASS.WPF`).
2.  `ConnectionStrings:DefaultConnection` değerini kendi sunucunuza göre doldurun.
3.  Bu dosya **`.gitignore`** ile dışlanır; parolalar GitHub’a gitmez.

**Alternatif:** ortam değişkeni `ConnectionStrings__DefaultConnection` (ASP.NET Core’da `ConnectionStrings:DefaultConnection` ile eşlenir).

### CeyPASS.Api

1.  Çözümde başlangıç projesi olarak `CeyPASS.Api` seçin veya:

    ```bash
    dotnet run --project CeyPASS.Api/CeyPASS.Api.csproj --launch-profile https
    ```

    (İsterseniz `--launch-profile http` da kullanılabilir.)

2.  [`CeyPASS.Api/Properties/launchSettings.json`](CeyPASS.Api/Properties/launchSettings.json): **http** profili `http://0.0.0.0:5126` (LAN); **https** profili `https://localhost:7061` ve aynı HTTP uçları.
3.  Swagger: `https://localhost:7061/swagger` veya `http://localhost:5126/swagger` (profile göre).
4.  **Mobil / fiziksel cihaz:** Telefondan erişim için genelde **`http://<geliştirme_PC_IP>:5126`** kullanılır; API’nin aynı Wi‑Fi’de dinliyor olması gerekir.

### CeyPASS.Web

```bash
dotnet run --project CeyPASS.Web/CeyPASS.Web.csproj
```

Tarayıcı adresi için Visual Studio veya [`CeyPASS.Web/Properties/launchSettings.json`](CeyPASS.Web/Properties/launchSettings.json) içindeki URL’ye bakın (ör. `https://localhost:5xxx`).

### CeyPASS.WFA

Visual Studio’da **CeyPASS.WFA** başlangıç projesi olarak ayarlanıp çalıştırılır. `appsettings.json` çıktı klasörüne kopyalanır; `appsettings.Local.json` proje kökünde varsa birlikte yüklenir.

### CeyPASS.WPF

Güncel masaüstü arayüzü (DevExpress WPF, MVVM):

```bash
dotnet run --project CeyPASS.WPF/CeyPASS.WPF.csproj
```

`appsettings.json` çıktı klasörüne kopyalanır; bağlantı dizesi WFA ile aynı mantıkta (`appsettings.Local.json` veya ortam değişkeni). Dağıtım paketi GitHub Actions ile üretilir (aşağıya bakın).

### CeyPASS.Mobile (Expo)

*   **Teknoloji:** [Expo](https://expo.dev/) (React Native), TypeScript; geliştirme için **Expo Go** kullanılabilir.
*   **Backend:** Doğrudan SQL’e bağlanmaz; **`CeyPASS.Api`** üzerinden `.../api/v1` ve JWT kullanır. Önce API’nin ayakta olduğundan emin olun.

**Adımlar:**

1.  `cd CeyPASS.Mobile`
2.  `npm install`
3.  `npm run start` veya **`npm run start:lan`** (aynı ağdaki cihazlar için önerilir)
4.  İsteğe bağlı: `npm run start:tunnel` (dış ağ; kurumsal proxy/firewall bazen engeller)
5.  Önbellek sorununda: `npx expo start -c`

**API adresinin verilmesi (öncelik sırası):**

1.  Ortam değişkeni **`EXPO_PUBLIC_API_BASE_URL`** — örnek (PowerShell; `YOUR_PC_IP` yerine `ipconfig` ile IPv4 yazın):

    ```powershell
    $env:EXPO_PUBLIC_API_BASE_URL="http://YOUR_PC_IP:5126"
    cd CeyPASS.Mobile
    npm run start:lan
    ```

2.  [`CeyPASS.Mobile/app.json`](CeyPASS.Mobile/app.json) içinde `expo.extra.apiBaseUrl` (repoda genelde `https://localhost:7061`; fiziksel telefonda `localhost` telefonu gösterir).
3.  Geliştirme sırasında [`CeyPASS.Mobile/services/api.ts`](CeyPASS.Mobile/services/api.ts) içinde, `localhost` + Expo Go kullanılırken Metro’nun verdiği makine IP’si ile otomatik HTTP `:5126` denemesi yapılabilir; yine de **`EXPO_PUBLIC_API_BASE_URL`** en net yöntemdir.

**Pratik notlar:**

*   Fiziksel telefonda **`localhost`** = telefonun kendisi; PC’deki API için **PC’nin LAN IP’si** gerekir.
*   Yerel geliştirmede çoğu zaman **HTTP 5126** kullanımı, self-signed HTTPS’ten daha az sorun çıkarır.
*   Kurumsal ağda tunnel/webSocket kesiliyorsa: telefon hotspot veya `start:lan` deneyin.

## Güvenlik ve GitHub

Özet: Hassas bağlantı ve SMTP bilgileri **`appsettings.Local.json`** veya ortam değişkenlerinde tutulur; bu dosyalar **commit edilmez**. Klonladıktan sonra her `.NET` projesi için `appsettings.Local.json.example` → `appsettings.Local.json` kopyalayıp doldurun. Mobil tarafta API URL’si için `.env` (gitignore) veya `EXPO_PUBLIC_API_BASE_URL` kullanılabilir; örnek için `CeyPASS.Mobile/.env.example`.

## 🧪 Testler

Proje, **xUnit + Moq + FluentAssertions** kütüphaneleri kullanılarak yazılmış kapsamlı bir test paketine sahiptir.
Tüm testler `CeyPASS.Tests` projesinde toplanmıştır ve kaynak kodda herhangi bir erişim belirteci değişikliği yapılmadan uygulanmıştır.

| Kategori | Dosya Sayısı | Test Sayısı |
|---|---|---|
| **Birim Testleri** (Business Servisler) | 24 | 180+ |
| **Kontrolcü Testleri** (Web Controllers) | 17 | 138+ |
| **Toplam** | **41** | **329+** |

**Kapsam:**
- Tüm iş mantığı servisleri (yetkilendirme, puantaj, izin, personel, bildirim vb.)
- Tüm ASP.NET Core MVC kontrolcüleri (yetki korumaları, başarı ve hata senaryoları)

**Testleri çalıştırmak için:**

```bash
dotnet test CeyPASS.Tests/CeyPASS.Tests.csproj
```

## 🔁 CI/CD Entegrasyonu

Bu depo, GitHub Actions tabanlı bir **CI/CD hattına** sahiptir ([`.github/workflows/ci-cd.yml`](.github/workflows/ci-cd.yml)):

*   `main` dalına push sonrası **CeyPASS.WPF** Release derlemesi (`win-x86`, self-contained) yapılır.
*   Çıktı **`CeyPASS-{sürüm}.zip`** olarak paketlenir; kurulum kısayolu uyumu için yürütülebilir dosya adı **`CeyPASS.WFA.exe`** olarak yeniden adlandırılır.
*   **`update.xml`** üretilir; AutoUpdater.NET ile masaüstü güncellemesi bu paketten yapılabilir.
*   Web ve WFA projeleri için ek build adımları ihtiyaç halinde genişletilebilir.

## 📞 İletişim

**Tahir Koca**
📧 [tahirkoca95@gmail.com](mailto:tahirkoca95@gmail.com)
🔗 [GitHub Profil](https://github.com/tahirkoca)

---

<a name="english"></a>

# 🇺🇸 CeyPASS (Web, WFA, WPF & Mobile)

![CeyPASS](./CeyPASS.WPF/Assets/CeyPass200.png)

**CeyPASS** is a comprehensive **Personnel Attendance Control System (PDKS)** developed for Cey Holding.
The solution includes a modern **Web interface**, **Windows Forms (WFA)** and **WPF** desktop clients, and a **mobile client** built with **Expo** (`CeyPASS.Mobile`) for iOS/Android—all sharing **`CeyPASS.Business`** and the **`CeyPASS.Api`** REST layer.
Web, WFA, and WPF target **feature parity**; the mobile app uses the same data and business rules through the API. The current desktop UI is **WPF** (user-guide screenshots under `docs/kilavuz/` are taken from WPF).

## 🚀 Features

You can perform all the following operations via Web and desktop (WFA / WPF):

*   **Personnel Management:** Detailed personnel files, onboarding/offboarding processes.
*   **Leave Management:** Leave request, approval, and tracking.
*   **Live Monitoring:** Real-time tracking of data from turnstiles and access control devices.
*   **Hardware Control:** Remote card sending, gate opening, data retrieval.
*   **Reporting:** Advanced PDKS reports and charts.
*   **Card Operations:** Card assignment, authorization, and updates.
*   **Notifications:** Instant system notifications via SignalR.
*   **Mobile QR Access System:** Quick and secure access by scanning QR codes on devices via the mobile app.
*   **Security Layers:**
    *   **Geofencing (Location Validation):** Verifies the user's proximity (default 50m) to the physical device during the transaction.
    *   **Mock Location Protection:** Attempts using Fake GPS applications are instantly detected and rejected.
    *   **Real Device Enforcement:** Transactions are only allowed from physical mobile devices, blocking emulators.
    *   **Double-Tap Protection:** Consecutive attempts within 5 seconds are filtered to prevent duplicate logs.
    *   **JWT & API Security:** All mobile communications are secured via encrypted JWT tokens.

## 🛠 Architecture & Technologies

The project is built on **Nx Layered Architecture (N-Layered Architecture)** consisting of **Business, DataAccess, Entities, and Infrastructure** layers. This ensures all business logic is shared.

*   **Core:** .NET 8
*   **UI layers:** ASP.NET Core MVC (Web), Windows Forms (WFA), WPF (DevExpress), Expo / React Native (Mobile)
*   **API:** ASP.NET Core Web API, JWT, Swagger (`api/v1`)
*   **Database:** Microsoft SQL Server (Entity Framework Core)
*   **Real-time:** SignalR
*   **Shared logic:** Dependency Injection, Repository Pattern, layered architecture (Business, DataAccess, Entities, Infrastructure)

## 📚 Documentation

| Document | Audience | Source | PDF output |
|----------|----------|--------|------------|
| [User Guide (TR)](docs/Kullanici-Kilavuzu.md) | HR, attendance, admin staff | `docs/kilavuz/` (Web / WPF / Mobile) | `docs/output/Kullanici-Kilavuzu.pdf` |
| [Technical Document (TR)](docs/Teknik-Dokuman.md) | Developers, system administrators | Architecture, layers, deployment | `docs/output/Teknik-Dokuman.pdf` |

<a name="developer-setup"></a>

## Developer setup

### Prerequisites

*   **Windows** is the recommended dev environment (WFA, WPF); Web/API can be built on other OSes.
*   [.NET 8 SDK](https://dotnet.microsoft.com/download)
*   [Node.js](https://nodejs.org/) LTS (for `CeyPASS.Mobile` / Expo)
*   A reachable **Microsoft SQL Server** instance (for schema setup)

### Database schema

1.  Run `database/CeyPASSDBScript.sql` on your SQL Server.
2.  Ensure the `CeyPASS` database exists.

### .NET configuration (Api, Web, WFA, WPF)

Committed `appsettings.json` files contain **template** placeholders (`YOUR_SERVER`, etc.). To run locally:

1.  Copy `appsettings.Local.json.example` to **`appsettings.Local.json`** in each project (`CeyPASS.Api`, `CeyPASS.Web`, `CeyPASS.WFA`, `CeyPASS.WPF`).
2.  Fill in `ConnectionStrings:DefaultConnection`.
3.  That file is **gitignored** and will not be pushed to GitHub.

**Alternative:** environment variable `ConnectionStrings__DefaultConnection` (maps to `ConnectionStrings:DefaultConnection`).

### CeyPASS.Api

1.  Set startup project to `CeyPASS.Api` or run:

    ```bash
    dotnet run --project CeyPASS.Api/CeyPASS.Api.csproj --launch-profile https
    ```

    (You can use `--launch-profile http` if you prefer.)

2.  [`CeyPASS.Api/Properties/launchSettings.json`](CeyPASS.Api/Properties/launchSettings.json): **http** profile listens on `http://0.0.0.0:5126` (LAN); **https** profile uses `https://localhost:7061` plus the same HTTP endpoints.
3.  Swagger: `https://localhost:7061/swagger` or `http://localhost:5126/swagger` depending on profile.
4.  **Phone / physical device:** Use **`http://<dev_pc_ip>:5126`** from the same network; the API must be listening for LAN access.

### CeyPASS.Web

```bash
dotnet run --project CeyPASS.Web/CeyPASS.Web.csproj
```

Check Visual Studio or [`CeyPASS.Web/Properties/launchSettings.json`](CeyPASS.Web/Properties/launchSettings.json) for the HTTPS/HTTP URL (e.g. `https://localhost:5xxx`).

### CeyPASS.WFA

Run **CeyPASS.WFA** as the startup project in Visual Studio. `appsettings.json` is copied to the output folder; `appsettings.Local.json` if present in the project folder is merged in.

### CeyPASS.WPF

Current desktop UI (DevExpress WPF, MVVM):

```bash
dotnet run --project CeyPASS.WPF/CeyPASS.WPF.csproj
```

`appsettings.json` is copied to the output folder; connection strings follow the same rules as WFA (`appsettings.Local.json` or environment variables). Release packages are produced via GitHub Actions (see below).

### CeyPASS.Mobile (Expo)

*   **Stack:** [Expo](https://expo.dev/) (React Native) and TypeScript; **Expo Go** is fine for development.
*   **Backend:** Does not connect to SQL; uses **`CeyPASS.Api`** at `.../api/v1` with JWT. Start the API first.

**Steps:**

1.  `cd CeyPASS.Mobile`
2.  `npm install`
3.  `npm run start` or **`npm run start:lan`** (recommended on the same LAN)
4.  Optional: `npm run start:tunnel` (may fail behind corporate proxy/firewall)
5.  If config seems stuck: `npx expo start -c`

**API base URL (priority):**

1.  **`EXPO_PUBLIC_API_BASE_URL`** — example (PowerShell; replace `YOUR_PC_IP` with your IPv4 from `ipconfig`):

    ```powershell
    $env:EXPO_PUBLIC_API_BASE_URL="http://YOUR_PC_IP:5126"
    cd CeyPASS.Mobile
    npm run start:lan
    ```

2.  [`CeyPASS.Mobile/app.json`](CeyPASS.Mobile/app.json) → `expo.extra.apiBaseUrl` (often `https://localhost:7061` in repo; on a real phone `localhost` is the phone).
3.  In development, [`CeyPASS.Mobile/services/api.ts`](CeyPASS.Mobile/services/api.ts) may derive the dev PC from Metro when using Expo Go with `localhost` in config; **`EXPO_PUBLIC_API_BASE_URL`** is still the clearest approach.

**Notes:**

*   On a physical device, **`localhost`** is the device itself; use your **PC’s LAN IP** to reach the API.
*   HTTP **5126** is usually easier than self-signed HTTPS during local dev.
*   If tunnel/WebSocket is blocked on corporate Wi‑Fi, try phone hotspot or `start:lan`.

## Security and GitHub

Keep secrets in **`appsettings.Local.json`** or environment variables; those files are **not committed**. After cloning, copy `appsettings.Local.json.example` to `appsettings.Local.json` per .NET project and fill in values. For mobile, use `.env` (gitignored) or `EXPO_PUBLIC_API_BASE_URL`; see `CeyPASS.Mobile/.env.example`.

## 🧪 Testing

The project includes a comprehensive test suite written with **xUnit + Moq + FluentAssertions**.
All tests reside in the `CeyPASS.Tests` project and were implemented without modifying any access modifiers in the source code.

| Category | Files | Tests |
|---|---|---|
| **Unit Tests** (Business Services) | 24 | 180+ |
| **Controller Tests** (Web Controllers) | 17 | 138+ |
| **Total** | **41** | **329+** |

**Coverage:**
- All business logic services (authorization, attendance, leave, personnel, notifications, etc.)
- All ASP.NET Core MVC controllers (authorization guards, success and error paths)

**To run the tests:**

```bash
dotnet test CeyPASS.Tests/CeyPASS.Tests.csproj
```

## 🔁 CI/CD Integration

This repository includes a GitHub Actions **CI/CD pipeline** ([`.github/workflows/ci-cd.yml`](.github/workflows/ci-cd.yml)):

*   On push to `main`, **CeyPASS.WPF** is published as a Release build (`win-x86`, self-contained).
*   Output is packaged as **`CeyPASS-{version}.zip`**; the executable is renamed to **`CeyPASS.WFA.exe`** for shortcut/installer compatibility.
*   An **`update.xml`** file is generated for AutoUpdater.NET desktop updates.
*   Additional Web/WFA build steps can be added as needed.

## 📞 Contact

**Tahir Koca**
📧 [tahirkoca95@gmail.com](mailto:tahirkoca95@gmail.com)
🔗 [GitHub Profile](https://github.com/tahirkoca)
