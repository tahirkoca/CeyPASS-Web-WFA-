# 0. Önce bunları okuyun

[← Ana içindekiler](../Kullanici-Kilavuzu.md)

Bu sayfa, diğer bölümlere geçmeden önce tüm platformlarda geçerli kuralları özetler.

---

## 0.1 Yetki ve görünürlük

- Menüde gördüğünüz modüller **rolünüze** bağlıdır. Yetkiniz olmayan ekran hiç listelenmeyebilir.
- Aynı ekranda bile **Görüntüle**, **Ekle**, **Düzenle**, **Sil/Pasife al**, **Onay**, **Excel** gibi işlemler ayrı ayrı açılıp kapatılabilir.
- Yetkisiz bir sayfaya girmeye çalışırsanız uyarı alır veya ana sayfaya yönlendirilirsiniz.
- Yetki talebi için **sistem yöneticinize** başvurun.

---

## 0.2 Pasife alma ve Geri al

Birçok «silme» işlemi aslında **pasife alma**dır (kayıt veritabanından tamamen silinmez).

| İşlem | Pasife alınca | Geri alınca |
|-------|---------------|-------------|
| Personel işten çıkış | Personel «işten çıkan» olur | Tekrar **aktif** |
| İzin silme | İzin pasif | Tekrar **aktif** |
| Kişi hareketi silme | Hareket pasif | Tekrar **aktif** |
| Cihaz silme | Cihaz pasif | Tekrar **aktif** |

**Geri al** bildirimi yaklaşık **7 saniye** görünür. Aynı anda yalnızca **bir** geri alma bekler; araya başka işlem girerseniz önceki fırsat kapanır.

| Platform | Geri al nerede? |
|----------|-----------------|
| Web | Sağ üst yeşil bildirim |
| WPF | Sağ üst bildirim |
| WFA | Alt durum çubuğu |
| Mobile | Başarı popup'ı |

Ayrıntılar: [Ekler — Geri al](14-ekler-geri-al-kisayollar.md)

---

## 0.3 Puantaj durum renkleri

Tüm platformlarda benzer anlam taşır:

| Renk | Anlam |
|------|--------|
| 🟢 Yeşil | Onaylandı |
| 🔴 Kırmızı | Reddedildi |
| 🔵 Mavi | Düzeltildi |
| 🟡 Sarı | Onay bekliyor |
| ⚪ Gri | Kilitli (bugün, gelecek veya süresi dolmuş ay) |

---

## 0.4 Platformlar arası farklar (kritik)

Aynı iş **farklı menüde** veya **farklı butonla** yapılabilir:

| Konu | Web | WFA | WPF | Mobile |
|------|-----|-----|-----|--------|
| Puantaj menüsü | Ana Menü | Ekstra İşlemler | Ekstra İşlemler | Ana Menü |
| Puantaj işyeri | Opsiyonel | **Zorunlu** | **Zorunlu** | Opsiyonel |
| Puantaj onay | Satır ✓ / ✗ / kalem | Grid ONAY/RET/DÜZENLE | Satır seç + üst buton | Kart → detay panel |
| «Bugüne kadar onayla» | Yok | Var | Var | Yok |
| Personel formu | Ayrı sayfa | Aynı ekran | Aynı ekran | Tam ekran modal |
| Klavye kısayolları | Var | Var | Var | Yok |
| İşlem rehberi (?) | Yok | Yok | Var (Puantaj vb.) | İpuçları menüsü |

En ayrıntılı modül: [Aylık puantaj](07-puantaj.md)

---

## 0.5 Kişisel veri ve ekran görüntüsü

Kılavuzdaki ekran görüntülerini güncellerken mümkünse **test personeli** kullanın; TC kimlik no, tam ad gibi alanları maskeleyin veya bulanıklaştırın.

---

**Sonraki:** [1. Giriş ve hesap →](01-giris.md)
