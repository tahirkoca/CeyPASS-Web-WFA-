# CeyPASS Kullanıcı Kılavuzu

Bu kılavuz CeyPASS uygulamasının **Web** ve **Masaüstü (WFA)** sürümlerinde günlük kullanımı açıklar. Menüler ve ekranlar yetkinize göre değişebilir.

---

## 1. Giriş (Login)

### 1.1 Web

1. Tarayıcıda CeyPASS web adresini açın.
2. **Kullanıcı adı** ve **Şifre** alanlarını doldurun.
3. **Giriş** butonuna tıklayın.
4. Yetkiniz yoksa uygun hata mesajı gösterilir; yetkili kullanıcılar ana sayfaya yönlendirilir.

**Şifremi unuttum:** Giriş sayfasındaki "Şifremi unuttum" bağlantısı ile şifre sıfırlama e-postası talep edebilirsiniz (sistem yöneticisi tarafından tanımlı e-posta ile).

### 1.2 Masaüstü (WFA)

1. CeyPASS masaüstü programını çalıştırın (kısayol veya kurulum klasöründen).
2. Açılan giriş penceresinde **Kullanıcı adı** ve **Şifre** girin.
3. **Giriş** butonuna tıklayın veya Enter'a basın.
4. Giriş başarılıysa ana işlem ekranı açılır.

**Not:** İlk açılışta güncelleme kontrolü yapılabilir; güncelleme varsa indirip kurabilir veya "Atla" / "Sonra hatırlat" seçenekleriyle mevcut sürümle devam edebilirsiniz.

---

## 2. Ana Sayfa / Dashboard

### 2.1 Web

- Giriş sonrası **Ana Sayfa** (Dashboard) açılır.
- Firma seçimi (birden fazla firma yetkiniz varsa) üst/yan alandan yapılır.
- Günün özet bilgileri (içeridekiler, dışarıdakiler, gecikenler, devamsızlar vb.) kartlar veya liste halinde gösterilir.
- Dashboard kartlarına tıklayarak ilgili rapora doğrudan gidebilirsiniz.

### 2.2 Masaüstü (WFA)

- Girişten sonra **Ana Sayfa** (Dashboard) varsayılan olarak açılır.
- Sol menüden **Ana Sayfa** ile bu ekrana dönebilirsiniz.
- Kartlar ve özet bilgiler Web ile uyumlu mantıkta sunulur; kartlara tıklayınca ilgili rapor ekranı açılır.

---

## 3. Menü Yapısı (Ortak)

Aşağıdaki modüller hem Web hem WFA'da bulunur; görünürlük **yetkinize** göre değişir.

### 3.1 Ana Menü

| Menü / Ekran | Açıklama |
|---------------|----------|
| **Ana Sayfa** | Dashboard; günlük özet ve hızlı rapor geçişleri |
| **Personeller** | Personel listesi, yeni personel ekleme, düzenleme, puantajlı/puantsız kart atamaları |
| **Kişi Hareketleri** | Giriş/çıkış hareketleri, filtreleme (tarih, personel, firma vb.) |
| **İzinler** | İzin tanımları, izin ekleme/düzenleme/silme, listeleme |
| **Aylık Puantaj** | Aylık puantaj görüntüleme, onay/ret, düzeltme (yetkiye göre) |
| **Raporlar** | Rapor türü seçimi, tarih/firma filtreleri, Excel/PDF çıktı |

### 3.2 Tanımlamalar (POY)

| Menü / Ekran | Açıklama |
|---------------|----------|
| **Firma Tanımlama** | Firma ekleme, düzenleme, listeleme |
| **İşyeri Tanımlama** | İşyeri ekleme, düzenleme (firmaya bağlı) |
| **Departman Tanımlama** | Departman ekleme, düzenleme |
| **Pozisyon Tanımlama** | Pozisyon/ünvan ekleme, düzenleme |
| **Personel Tanımlama** | Personel ekleme, düzenleme, fotoğraf, kart atamaları |

### 3.3 Çalışma Şekli / Durumu (VMY)

| Menü / Ekran | Açıklama |
|---------------|----------|
| **Vardiyalar (Çalışma Şekilleri)** | Vardiya/çalışma şekli tanımları |
| **Çalışma Statüleri** | Çalışma durumu tanımları (aktif, pasif vb.) |

### 3.4 Ayarlar

| Menü / Ekran | Açıklama |
|---------------|----------|
| **Cihazlar** | Geçiş cihazları (turnike vb.) tanımlama |
| **Resmi Tatiller** | Resmi tatil günlerini tanımlama / içe aktarma |

### 3.5 Admin (Sadece yetkili kullanıcılar)

- **Admin Panel**: Firma, işyeri, cihaz, departman, pozisyon, resmi tatil, çalışma statüsü, vardiya gibi tüm tanımların tek ekrandan (sekmeli) yönetimi.
- Web'de **Admin** menüsü, WFA'da **Admin Panel** butonu ile erişilir; sadece yetkili roller görür.

---

## 4. Ortak İşlemler

### 4.1 Personel

- **Liste:** Firma ve arama kriterine göre personel listesi görüntülenir.
- **Yeni:** Personel ekleme formu doldurulur (ad, soyad, sicil, TC, firma, departman, pozisyon vb.).
- **Düzenle:** Mevcut personel seçilir, bilgiler güncellenir; fotoğraf ve puantajsız kart ataması yapılabilir.
- **Puantajsız kart:** Puantaj yapılmayan kartlar için ayrı atama ekranı (Web'de Personel altında, WFA'da ilgili formdan) kullanılır.

### 4.2 İzinler

- İzin listesi tarih ve filtrelerle görüntülenir.
- Yeni izin eklenirken personel, izin tipi, başlangıç/bitiş tarihi seçilir.
- İzin düzenleme ve silme yetkiniz varsa ilgili butonlar açık olur.

### 4.3 Kişi Hareketleri

- Tarih aralığı, firma, personel vb. filtreler seçilir.
- Sonuçlar tablo halinde listelenir; gerekiyorsa dışa aktarım (Excel vb.) yapılabilir.

### 4.4 Aylık Puantaj

- Firma, işyeri, yıl ve ay seçilir.
- Puantaj satırları listelenir; onay/ret/düzeltme yetkiniz varsa ilgili aksiyonlar kullanılır.
- WFA'da satır bazlı onay/ret/düzenleme ekranları açılabilir.

### 4.5 Raporlar

- Rapor türü seçilir (içeridekiler, dışarıdakiler, gecikenler, devamsızlar, izinliler vb.).
- Tarih ve firma filtreleri uygulanır.
- Rapor getirilir; Excel veya PDF'e aktarım (yetki varsa) yapılabilir.

---

## 5. Web'e Özel Notlar

- Menü sol tarafta sidebar olarak sabittir; yetkisiz modüller gizlenir.
- Firma seçimi üst/yan alandaki açılır listeden yapılır (birden fazla firma yetkisi varsa).
- Sayfalar tarayıcıda açılır; çıkış için sağ üstteki kullanıcı menüsünden **Çıkış** kullanılır.

---

## 6. Masaüstüne (WFA) Özel Notlar

- Sol menü (sidebar) açılır/kapanır; daraltılmış halde sadece ikonlar görünür.
- Ana sayfa açıldığında dashboard kartlarına tıklayarak rapor ekranına geçilir.
- Güncelleme: Program başlarken sunucu kontrol edilir; güncelleme isteğe bağlıdır (Atla / Sonra hatırlat).
- Çıkış: Sol alttaki kullanıcı alanı veya menüdeki çıkış seçeneği ile oturum kapatılır; program kapanır veya giriş ekranına dönülür (uygulama davranışına göre).

---

## 7. Yetki ve Rol

- Hangi menülerin görüneceği ve hangi işlemlerin (görüntüleme, ekleme, düzenleme, silme, dışa aktarma, onay vb.) yapılabileceği **rol ve sayfa yetkilerine** bağlıdır.
- Yetkiniz olmayan bir sayfaya doğrudan link/URL ile gitmeye çalışırsanız erişim reddedilir veya ana sayfaya yönlendirilirsiniz.
- Yetki değişikliği için sistem yöneticinize başvurun.

---

## 8. Sorun Giderme

- **Web'de giriş yapamıyorum:** Kullanıcı adı/şifre kontrol edin; hesap kilitli veya pasif olabilir.
- **WFA açılmıyor:** Kurulum klasöründeki exe'yi çalıştırdığınızdan emin olun; güncelleme sunucusu yanıt vermiyorsa "Atla" ile devam edebilirsiniz.
- **Veritabanı hatası:** Bağlantı bilgileri (ConnectionString) sunucu/yapılandırma ile uyumlu olmalı; teknik ekip veya yönetici ile iletişime geçin.
- **Menü görünmüyor:** Yetkiniz o modül için tanımlı olmayabilir; yöneticiye yetki talebi iletin.

Bu kılavuz CeyPASS Web ve Masaüstü sürümlerinin günlük kullanımı için temel bilgileri içerir; ekranlar sürümle birlikte güncellenebilir.
