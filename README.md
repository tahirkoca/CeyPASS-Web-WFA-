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
