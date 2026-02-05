# CeyPASS (WEB & WFA)

![CeyPASS](https://placeholder.com) <!-- Proje logosu varsa buraya eklenebilir -->

🇹🇷 **CeyPASS**, Cey Holding için geliştirilmiş kapsamlı bir **Personel Devam Kontrol Sistemidir (PDKS)**. Proje, yönetim paneli olarak çalışan modern bir **Web uygulaması** ve sahadaki cihazlar/kartlar ile etkileşime giren, canlı izleme yapan güçlü bir **Masaüstü (Windows Forms) uygulamasından** oluşur.

🇺🇸 **CeyPASS** is a comprehensive **Personnel Attendance Control System (PDKS)** developed for Cey Holding. The project consists of a modern **Web application** serving as an administration panel and a robust **Desktop (Windows Forms) application** for live monitoring and interaction with field devices/cards.

---

## 🚀 Özellikler / Features

### 🇹🇷 Türkçe
*   **Web Paneli (.NET):**
    *   Detaylı personel yönetimi ve özlük dosyaları.
    *   İzin talebi, onayı ve takibi.
    *   Gelişmiş raporlama seçenekleri.
    *   Sistem genel ayarları ve kullanıcı yetkilendirme.
*   **Masaüstü Uygulaması (Windows Forms):**
    *   **Canlı İzleme:** Turnike ve geçiş kontrol cihazlarından gelen verilerin anlık takibi.
    *   **Kart Yönetimi:** Hızlı kart atama, güncelleme ve yetkilendirme.
    *   **AutoUpdater:** Otomatik güncelleme desteği ile her zaman güncel sürüm.
    *   **SignalR:** Anlık bildirimler ve veri akışı.
*   **Mimari:** Sürdürülebilir, çok katmanlı mimari (Business, DataAccess, Entities, Infrastructure).

### 🇺🇸 English
*   **Web Panel (.NET):**
    *   Detailed personnel management and files.
    *   Leave request, approval, and tracking.
    *   Advanced reporting options.
    *   System settings and user authorization.
*   **Desktop App (Windows Forms):**
    *   **Live Monitoring:** Real-time tracking of data from turnstiles and access control devices.
    *   **Card Management:** Fast card assignment, updates, and authorization.
    *   **AutoUpdater:** Always up-to-date with automatic update support.
    *   **SignalR:** Instant notifications and data streaming.
*   **Architecture:** Maintainable, multi-layered architecture (Business, DataAccess, Entities, Infrastructure).

---

## 🛠 Teknolojiler / Technologies

*   **Backend:** .NET Core / .NET Framework
*   **Frontend:** ASP.NET Core MVC / Windows Forms
*   **Database:** Microsoft SQL Server (Entity Framework Core)
*   **Real-time:** SignalR
*   **Tools:** AutoUpdater.NET, Dependency Injection

---

## ⚙️ Kurulum ve Güvenlik / Setup & Security

Bu proje, hassas verileri (Veritabanı bağlantı cümleleri, E-posta şifreleri) korumak için özel bir yapılandırma kullanır.

1.  Proje klonlandıktan sonra `appsettings.json` dosyaları içinde **boş şablonlar** göreceksiniz.
2.  Kendi yerel ortamınızda çalışmak için:
    *   `appsettings.json` dosyasının bir kopyasını oluşturun ve adını `appsettings.Local.json` yapın.
    *   Bu `Local` dosya içine gerçek bağlantı bilgilerinizi ve şifrelerinizi girin.
    *   `appsettings.Local.json` dosyası `.gitignore` ile engellenmiştir, böylece şifreleriniz GitHub'a gitmez.

This project uses a specific configuration to protect sensitive data (ConnectionString, SMTP passwords).

1.  After cloning, you will see **empty templates** in `appsettings.json`.
2.  To run in your local environment:
    *   Create a copy of `appsettings.json` and rename it to `appsettings.Local.json`.
    *   Enter your real connection details and passwords into this `Local` file.
    *   `appsettings.Local.json` is ignored by `.gitignore`, ensuring your secrets are safe.

---

## 📞 İletişim / Contact

**Tahir Koca**
📧 [tahirkoca95@gmail.com](mailto:tahirkoca95@gmail.com)
🔗 [GitHub Profile](https://github.com/tahirkoca)
