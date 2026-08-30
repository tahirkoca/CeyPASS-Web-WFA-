# 14. Ekler — Geri al, kısayollar, sorun giderme

[← Ana içindekiler](../Kullanici-Kilavuzu.md)

---

## 14.1 Geri al (Undo)

### Hangi işlemlerde?

| İşlem | Geri alınca |
|-------|--------------|
| Personel işten çıkış | Personel tekrar **aktif** |
| İzin pasife alma | İzin tekrar **aktif** |
| Kişi hareketi pasife alma | Hareket tekrar **aktif** |
| Cihaz pasife alma | Cihaz tekrar **aktif** |

### Nerede görünür? (~7 saniye)

| Platform | Konum |
|----------|--------|
| 🌐 Web | Sağ üst yeşil bildirim → **Geri al** |
| 🖥️ WPF | Sağ üst bildirim → **Geri al** |
| 🖥️ WFA | Alt durum çubuğu → **Geri al** bağlantısı |
| 📱 Mobile | Başarı popup → **Geri al** |

### Adımlar

1. Pasife alma / işten çıkış işlemini tamamlayın.
2. Bildirim belirir belirmez **Geri al**'a tıklayın/dokunun.
3. Kayıt eski durumuna döner.

⚠️ Yeni bir işlem yaparsanız veya süre dolarsa geri alma **kapanır**.

---

## 14.2 Klavye kısayolları (Web, WFA, WPF)

Üst başlıktaki **⌨️** simgesi veya **F1** / **Ctrl+/** ile liste açılır. **Esc** ile kapanır.

| Kısayol | İşlev |
|---------|--------|
| **F1** / **Ctrl+/** | Kısayol listesi |
| **Esc** | Açık modal/pencereyi kapat |
| **Ctrl+F** | Tabloda arama (desteklenen ekranlarda) |
| **Ctrl+S** | Kaydet (form odaktayken) |
| **Ctrl+P** | Yazdır / önizleme (rapor ekranları) |

### WPF — İşlem rehberi vs kısayollar

| Simge | Anlam |
|-------|--------|
| **❓** | Sayfa **işlem rehberi** (Puantaj vb.) — Esc ile kapanır |
| **⌨️** | Genel **klavye kısayolları** listesi |

📱 **Mobile'da klavye kısayolu yok** — yan menü **İpuçları**'nı kullanın.

---

## 14.3 İşlem rehberi / İpuçları

| Platform | Açma | Kapatma |
|----------|------|---------|
| WPF | Sayfadaki **❓** | ✕ veya **Esc** |
| Mobile | Menü → **İpuçları** | ✕ veya geri |
| Web / WFA | Yok | — |

---

## 14.4 Sık karşılaşılan sorunlar

| Sorun | Çözüm |
|-------|--------|
| Web'e giremiyorum | Kullanıcı adı/şifre; hesap kilitli/pasif — yöneticiye başvur |
| WFA açılmıyor | Doğru exe; güncelleme ekranında **Atla** |
| WPF açılmıyor | BT: bağlantı/kurulum ayarları |
| Mobile veri gelmiyor | İnternet/VPN; sunucu adresi (BT) |
| Menüde modül yok | Yetki yok — yöneticiye başvur |
| Puantaj listesi boş | Personel seçildi mi? Web/Mobile: **Puantaj Getir**; WFA/WPF: **işyeri zorunlu** |
| Geri al görünmüyor | 7 sn geçti veya araya başka işlem girdi |
| Excel indirmiyor | Dışa aktarma yetkisi; pop-up engelleyici (Web) |
| Kişi hareket Excel | Bu ekranda yok — **Raporlar** kullanın |

---

## 14.5 Destek

- **Yetki / hesap:** Sistem yöneticiniz
- **Kurulum / bağlantı:** BT birimi
- **Teknik mimari:** [Teknik Doküman](../Teknik-Dokuman.md)

---

**Önceki:** [13. Canlı izleme ←](13-canli-izleme.md)  
**Ana içindekiler:** [Kullanıcı Kılavuzu](../Kullanici-Kilavuzu.md)
