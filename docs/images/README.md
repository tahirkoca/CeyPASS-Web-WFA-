# Kullanıcı Kılavuzu — Ekran Görüntüleri

Bu klasör, kılavuzdaki `![...](../images/...)` referanslarına karşılık gelen **PNG** dosyalarını içerir.

## Klasör yapısı

```
images/
  web/          → Canlı CeyPASS.Web (Playwright ile alınır)
  wfa/          → WFA arayüzü (fixture render)
  wpf/          → WPF arayüzü (fixture render)
  mobile/       → Mobile arayüzü (fixture render)
```

## Yeniden üretme

Depo kökünden (Web sunucusu `http://localhost:5002` açık olmalı veya script başlatır):

```powershell
.\docs\scripts\capture-kilavuz-screenshots.ps1
```

Ek modal görüntüleri:

```powershell
node docs\scripts\capture-kilavuz-extras.mjs
```

Kimlik bilgileri `CeyPASS.Web\appsettings.Local.json` üzerinden okunur; repoya yazılmaz.

## Notlar

- **Web** görüntüleri gerçek uygulamadan alınır.
- **WFA / WPF / Mobile** görüntüleri UI yapısına uygun fixture ile üretilir (masaüstü pencere otomasyonu olmadan).
- Kişisel veri içeren görüntüler test ortamından alınmıştır; dağıtımdan önce maskeleme yapılabilir.
