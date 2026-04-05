🌍 Language / Dil: [Türkçe](#turkce) | [English](#english)

---

<a name="turkce"></a>

# 🇹🇷 CeyPASS (Web, WFA & Mobil)

![CeyPASS](./CeyPASS.WFA/Resources/CeyPass%20200.png)

**CeyPASS**, Cey Holding için geliştirilmiş kapsamlı bir **Personel Devam Kontrol Sistemidir (PDKS)**.
Proje, tek bir merkezi sisteme bağlı çalışan modern bir **Web arayüzü**, güçlü bir **Masaüstü (Windows Forms) uygulaması** ve **iOS/Android üzerinde Expo ile geliştirilmiş mobil istemci** (`CeyPASS.Mobile`) içerir.
Web ve masaüstü **birebir aynı yeteneklere** yakın bir parite hedefler; mobil uygulama **CeyPASS.Api** REST API’si üzerinden aynı veri ve iş kurallarına bağlanır.

## 🚀 Özellikler

Hem Web hem de Masaüstü üzerinden aşağıdaki tüm işlemleri yapabilirsiniz:

*   **Personel Yönetimi:** Detaylı özlük dosyaları, işe giriş-çıkış işlemleri.
*   **İzin İşlemleri:** İzin talebi, onayı ve takibi.
*   **Canlı İzleme:** Turnike ve cihazlardan anlık geçiş verilerinin takibi.
*   **Donanım Yönetimi:** Cihazlara uzaktan kart gönderme, kapı açma, veri çekme.
*   **Raporlama:** Gelişmiş PDKS raporları ve grafikler.
*   **Kart İşlemleri:** Kart atama, yetkilendirme ve güncelleme.
*   **Bildirimler:** SignalR ile anlık sistem bildirimleri.

## 🛠 Mimari ve Teknolojiler

Proje, **Business, DataAccess, Entities ve Infrastructure** katmanlarından oluşan **Nx Katmanlı Mimari (N-Layered Architecture)** üzerine inşa edilmiştir. Bu sayede tüm iş mantığı ortaktır.

*   **Core:** .NET Core / .NET Framework
*   **Arayüz Katmanları:** ASP.NET Core MVC & Windows Forms
*   **Veritabanı:** Microsoft SQL Server (Entity Framework Core)
*   **Gerçek Zamanlı İletişim:** SignalR
*   **Ortak Yapı:** Dependency Injection, Repository Pattern

<a name="gelistirici-kurulumu"></a>

## Geliştirici kurulumu

### Önkoşullar

*   **Windows** (WFA ve önerilen geliştirme ortamı; Web/API diğer işletim sistemlerinde de derlenebilir)
*   [.NET 8 SDK](https://dotnet.microsoft.com/download)
*   [Node.js](https://nodejs.org/) LTS (`CeyPASS.Mobile` / Expo için)
*   Erişebildiğiniz bir **Microsoft SQL Server** (şema kurulumu için)

### Veritabanı şeması

1.  SQL Server’da `database/CeyPASSDBScript.sql` dosyasını çalıştırın.
2.  `CeyPASS` veritabanının oluştuğundan emin olun.

### .NET yapılandırması (Api, Web, WFA)

Repodaki `appsettings.json` dosyalarında veritabanı için **şablon** (`YOUR_SERVER`, `YOUR_USER`, `YOUR_PASSWORD`) bulunur. Yerelde çalıştırmak için:

1.  İlgili projede `appsettings.Local.json.example` dosyasını **`appsettings.Local.json`** adıyla kopyalayın (`CeyPASS.Api`, `CeyPASS.Web`, `CeyPASS.WFA`).
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

Bu depo, GitHub Actions tabanlı bir **CI/CD hattına** sahiptir:

*   Her push/pull request sonrasında Web ve WFA projeleri için otomatik **build & temel kontroller** çalıştırılır.
*   Ana dala yapılan onaylı commit'lerde, web uygulaması için **publish artifact'leri**, Windows Forms uygulaması için ise AutoUpdater.NET ile uyumlu **güncelleme paketleri** üretilip CI çıktıları olarak saklanır.
*   Sürüm süreci, GitHub Releases üzerinden yönetilecek şekilde kurgulanmıştır; böylece WFA istemcileri yeni sürümleri doğrudan bu pipeline ile dağıtılan paketlerden alabilir.

## 📞 İletişim

**Tahir Koca**
📧 [tahirkoca95@gmail.com](mailto:tahirkoca95@gmail.com)
🔗 [GitHub Profil](https://github.com/tahirkoca)

---

<a name="english"></a>

# 🇺🇸 CeyPASS (Web, WFA & Mobile)

![CeyPASS](./CeyPASS.WFA/Resources/CeyPass%20200.png)

**CeyPASS** is a comprehensive **Personnel Attendance Control System (PDKS)** developed for Cey Holding.
The solution includes a modern **Web interface**, a **Windows Forms desktop client**, and a **mobile client** built with **Expo** (`CeyPASS.Mobile`) for iOS/Android.
Web and desktop target **feature parity**; the mobile app talks to the same business rules through the **`CeyPASS.Api`** REST API.

## 🚀 Features

You can perform all the following operations via both Web and Desktop:

*   **Personnel Management:** Detailed personnel files, onboarding/offboarding processes.
*   **Leave Management:** Leave request, approval, and tracking.
*   **Live Monitoring:** Real-time tracking of data from turnstiles and access control devices.
*   **Hardware Control:** Remote card sending, gate opening, data retrieval.
*   **Reporting:** Advanced PDKS reports and charts.
*   **Card Operations:** Card assignment, authorization, and updates.
*   **Notifications:** Instant system notifications via SignalR.

## 🛠 Architecture & Technologies

The project is built on **Nx Layered Architecture (N-Layered Architecture)** consisting of **Business, DataAccess, Entities, and Infrastructure** layers. This ensures all business logic is shared.

*   **Core:** .NET Core / .NET Framework
*   **UI Layers:** ASP.NET Core MVC & Windows Forms
*   **Database:** Microsoft SQL Server (Entity Framework Core)
*   **Real-time:** SignalR
*   **Shared Logic:** Dependency Injection, Repository Pattern

<a name="developer-setup"></a>

## Developer setup

### Prerequisites

*   **Windows** is the recommended dev environment (WFA); Web/API can be built on other OSes.
*   [.NET 8 SDK](https://dotnet.microsoft.com/download)
*   [Node.js](https://nodejs.org/) LTS (for `CeyPASS.Mobile` / Expo)
*   A reachable **Microsoft SQL Server** instance (for schema setup)

### Database schema

1.  Run `database/CeyPASSDBScript.sql` on your SQL Server.
2.  Ensure the `CeyPASS` database exists.

### .NET configuration (Api, Web, WFA)

Committed `appsettings.json` files contain **template** placeholders (`YOUR_SERVER`, etc.). To run locally:

1.  Copy `appsettings.Local.json.example` to **`appsettings.Local.json`** in each project (`CeyPASS.Api`, `CeyPASS.Web`, `CeyPASS.WFA`).
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

This repository includes a GitHub Actions–based **CI/CD pipeline**:

*   On every push / pull request, Web and WFA projects are automatically **built and sanity-checked**.
*   For approved commits to the main branch, the pipeline produces **publish artifacts** for the web app and **update packages** compatible with AutoUpdater.NET for the Windows Forms app.
*   The release flow is designed to work with **GitHub Releases**, so WFA clients can consume new versions directly from the artifacts generated by this pipeline.

## 📞 Contact

**Tahir Koca**
📧 [tahirkoca95@gmail.com](mailto:tahirkoca95@gmail.com)
🔗 [GitHub Profile](https://github.com/tahirkoca)
