# CeyPASS (WEB & WFA)

![CeyPASS](https://placeholder.com) <!-- Proje logosu varsa buraya eklenebilir -->

🇹🇷 **CeyPASS**, Cey Holding için geliştirilmiş kapsamlı bir **Personel Devam Kontrol Sistemidir (PDKS)**.
Proje, tek bir merkezi sisteme bağlı çalışan modern bir **Web arayüzü** ve güçlü bir **Masaüstü (Windows Forms) uygulamasından** oluşur.
Her iki platform da **birebir aynı yeteneklere sahiptir** ve kullanıcıların tercih ettikleri ortamdan tüm işlemleri gerçekleştirmesine olanak tanır.

🇺🇸 **CeyPASS** is a comprehensive **Personnel Attendance Control System (PDKS)** developed for Cey Holding.
The project consists of a modern **Web interface** and a robust **Desktop (Windows Forms) application** operating on a single centralized system.
Both platforms possess **identical capabilities (feature parity)**, allowing users to perform all operations seamlessly from their preferred environment.

---

## 🚀 Özellikler / Features

Hem Web hem de Masaüstü üzerinden aşağıdaki tüm işlemleri yapabilirsiniz:
(You can perform all the following operations via both Web and Desktop:)

*   **Personel Yönetimi (Personnel Management):** Detaylı özlük dosyaları, işe giriş-çıkış işlemleri.
*   **İzin İşlemleri (Leave Management):** İzin talebi, onayı ve takibi.
*   **Canlı İzleme (Live Monitoring):** Turnike ve cihazlardan anlık geçiş verilerinin takibi.
*   **Donanım Yönetimi (Hardware Control):** Cihazlara uzaktan kart gönderme, kapı açma, veri çekme.
*   **Raporlama (Reporting):** Gelişmiş PDKS raporları ve grafikler.
*   **Kart İşlemleri (Card Operations):** Kart atama, yetkilendirme ve güncelleme.
*   **Bildirimler (Notifications):** SignalR ile anlık sistem bildirimleri.

---

## 🛠 Mimari ve Teknolojiler / Architecture & Technologies

Proje, **Business, DataAccess, Entities ve Infrastructure** katmanlarından oluşan **Nx Katmanlı Mimari (N-Layered Architecture)** üzerine inşa edilmiştir. Bu sayede tüm iş mantığı ortaktır.

*   **Core:** .NET Core / .NET Framework
*   **UI Layers:** ASP.NET Core MVC & Windows Forms
*   **Database:** Microsoft SQL Server (Entity Framework Core)
*   **Real-time:** SignalR
*   **Shared Logic:** Dependency Injection, Repository Pattern

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
