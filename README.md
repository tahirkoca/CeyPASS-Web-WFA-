🌍 Language / Dil: [Türkçe](#turkce) | [English](#english)

---

<a name="turkce"></a>

# 🇹🇷 CeyPASS (WEB & WFA)

![CeyPASS](./CeyPASS.WFA/Resources/CeyPass%20200.png)

**CeyPASS**, Cey Holding için geliştirilmiş kapsamlı bir **Personel Devam Kontrol Sistemidir (PDKS)**.
Proje, tek bir merkezi sisteme bağlı çalışan modern bir **Web arayüzü** ve güçlü bir **Masaüstü (Windows Forms) uygulamasından** oluşur.
Her iki platform da **birebir aynı yeteneklere sahiptir** ve kullanıcıların tercih ettikleri ortamdan tüm işlemleri gerçekleştirmesine olanak tanır.

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

## ⚙️ Kurulum ve Güvenlik

Bu proje, hassas verileri (Veritabanı bağlantı cümleleri, E-posta şifreleri) korumak için özel bir yapılandırma kullanır.

1.  Proje klonlandıktan sonra `appsettings.json` dosyaları içinde **boş şablonlar** göreceksiniz.
2.  Kendi yerel ortamınızda çalışmak için:
    *   `appsettings.json` dosyasının bir kopyasını oluşturun ve adını `appsettings.Local.json` yapın.
    *   Bu `Local` dosya içine gerçek bağlantı bilgilerinizi ve şifrelerinizi girin.
    *   `appsettings.Local.json` dosyası `.gitignore` ile engellenmiştir, böylece şifreleriniz GitHub'a gitmez.

3.  **Veritabanı Kurulumu:**
    *   `database` klasörü içindeki `CeyPASSDBScript.sql` dosyasını bir SQL Server veritabanında çalıştırarak şemayı oluşturun.

## 📞 İletişim

**Tahir Koca**
📧 [tahirkoca95@gmail.com](mailto:tahirkoca95@gmail.com)
🔗 [GitHub Profil](https://github.com/tahirkoca)

---

<a name="english"></a>

# 🇺🇸 CeyPASS (WEB & WFA)

![CeyPASS](./CeyPASS.WFA/Resources/CeyPass%20200.png)

**CeyPASS** is a comprehensive **Personnel Attendance Control System (PDKS)** developed for Cey Holding.
The project consists of a modern **Web interface** and a robust **Desktop (Windows Forms) application** operating on a single centralized system.
Both platforms possess **identical capabilities (feature parity)**, allowing users to perform all operations seamlessly from their preferred environment.

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

## ⚙️ Setup & Security

This project uses a specific configuration to protect sensitive data (ConnectionString, SMTP passwords).

1.  After cloning, you will see **empty templates** in `appsettings.json`.
2.  To run in your local environment:
    *   Create a copy of `appsettings.json` and rename it to `appsettings.Local.json`.
    *   Enter your real connection details and passwords into this `Local` file.
    *   `appsettings.Local.json` is ignored by `.gitignore`, ensuring your secrets are safe.

3.  **Database Setup:**
    *   Run the `CeyPASSDBScript.sql` file located in the `database` folder on a SQL Server instance to create the schema.

## 📞 Contact

**Tahir Koca**
📧 [tahirkoca95@gmail.com](mailto:tahirkoca95@gmail.com)
🔗 [GitHub Profile](https://github.com/tahirkoca)
