# Gece / Gündüz Modu (Dark / Light Theme) Planı

## Amaç

Tüm sayfalarda ve modallarda gece/gündüz modu desteği: kullanıcı tercihi (toggle) ile tema seçimi, tercihin saklanması ve tüm arayüzün (sidebar, header, kartlar, tablolar, formlar, modallar, dropdown'lar, login sayfaları) aynı temaya uyması.

## Strateji

- **Gündüz:** Mevcut `:root` değişkenleri aynen kalır (varsayılan).
- **Gece:** `[data-theme="dark"]` (veya `html[data-theme="dark"]`) altında aynı CSS değişkenleri koyu palet ile override edilir; böylece mevcut `var(--gray-*)` vb. kullanan tüm stiller otomatik uyum sağlar.
- **Toggle:** Header'da (Ayarlar yanında) tema butonu; tıklanınca `document.documentElement.setAttribute('data-theme', 'dark'|'light')` ve `localStorage.setItem('theme', ...)`.
- **İlk yükleme:** Script ile `localStorage.getItem('theme')` okunur; yoksa `prefers-color-scheme: dark` ile sistem tercihi kullanılabilir (isteğe bağlı).

---

## 1. CSS: Tema Değişkenleri

**Dosya:** `CeyPASS.Web/wwwroot/css/site.css`

### 1.1 Gece paleti bloğu

`:root` bloğundan sonra `[data-theme="dark"]` için yeni bir blok eklenecek. Aynı değişken isimleri koyu değerlerle override edilecek:

- **Nötr renkler:** `--gray-50` … `--gray-900` tersine çevrilecek (gray-50 koyu, gray-900 açık).
- **Primary:** Gece modunda kontrast için uygun tonlar.
- **Semantik:** `--success`, `--warning`, `--danger`, `--info` ve `-light` varyantları gece için uyumlu tonlara çekilecek.
- **Gölgeler:** Gece modunda daha hafif/koyu gölge değerleri.
- **Sidebar:** Zaten koyu; gece modunda sabit koyu renk ile override (çünkü gray değişkenleri dark'ta açık olacak).

### 1.2 Sabit "white" kullanımlarının tema ile uyumu

Aşağıdaki seçicilerde `background: white` yerine tema değişkeni (örn. `--surface` veya `var(--gray-50)`):

- `.top-header`
- `.card`
- `.stat-card`
- `.form-control`, `.form-select`
- `.dropdown-menu` (dark blokta arka plan/border override)
- `.modal-content`, `.modal-header`, `.modal-body`, `.modal-footer`
- `.dashboard-card`
- `.data-table-wrapper` ve `.data-table-wrapper .table thead th`
- Diğer `background: white` geçen tüm kurallar

### 1.3 Gece modunda özel override'lar

- **Sidebar:** `[data-theme="dark"] .sidebar` ile sabit koyu arka plan (değişkenler dark'ta açık olacağı için).
- **Modal .btn-close:** Dark'ta ikon rengi okunabilir.
- **Toastr:** Harici kütüphane; dark modda container arka plan/yazı rengi override.

---

## 2. Toggle ve Persistence

### 2.1 Toggle yerleşimi

**Dosya:** `CeyPASS.Web/Views/Shared/_Layout.cshtml`

- Header'da Ayarlar dropdown'ının soluna (veya sağına) tema butonu.
- İkon: gece modunda güneş (gündüze geç), gündüz modunda ay (geceye geç). Örn. `bi-moon-stars` / `bi-sun`.
- Tıklanınca: `data-theme` ve `localStorage.setItem('theme', 'dark'|'light')`.

### 2.2 Sayfa yüklenirken tema uygulama

**Dosya:** `CeyPASS.Web/wwwroot/js/site.js` veya _Layout.cshtml inline script

- DOM hazır olduğunda (mümkünse `<head>` sonunda erken) script:
  - `localStorage.getItem('theme')` oku; `'dark'` veya `'light'` ise `document.documentElement.setAttribute('data-theme', saved)`.
  - Yoksa (isteğe bağlı): `prefers-color-scheme: dark` ile ilk tema belirle.
- Toggle butonu da aynı mantıkla günceller.

---

## 3. Modallar

Tüm modallar Bootstrap yapısında: `modal-content` → `modal-header`, `modal-body`, `modal-footer`. Renkler Bootstrap varsayılanı + site.css.

**Yapılacak:** site.css içinde `[data-theme="dark"]` altında:

- `.modal-content`, `.modal-header`, `.modal-body`, `.modal-footer`: arka plan, border, metin rengi.
- `.modal-title`, `.btn-close` okunabilir.

Böylece aşağıdaki modallar tek merkezden tema alır; view'larda değişiklik gerekmez:

- **İzinler:** izinSilModal
- **Kişi Hareketleri:** hareketEkleModal, hareketGuncelleModal, silOnayModal
- **Personel:** istenCikarModal, istenCikarOnayModal
- **Puantaj:** duzenleModal, reddetModal, cokluSicilModal
- **Admin:** guncellemeMailOnayModal, silOnayModal
- **Çalışma Statü, Cihaz:** silOnayModal
- **Canlı İzleme:** misafir modal (_MisafirKartYeni, _MisafirKartGuncelle)

---

## 4. Sayfa Grupları ve Özel Durumlar

### 4.1 Layout kullanan tüm sayfalar

Home, Personel, KisiHareket, İzinler, Rapor, Puantaj, Admin, Firma, İşyeri, Departman, Pozisyon, Çalışma Statü, Çalışma Şekli, Cihaz, Resmi Tatil vb. — sadece site.css + Layout (toggle + script) yeterli.

### 4.2 Login ve şifre sayfaları (Layout yok)

- Account/Login.cshtml
- Account/ForgotPassword.cshtml, ForgotPasswordConfirm.cshtml
- CanliIzleme/Login.cshtml

Bu sayfalar kendi `<style>` içinde `:root` kullanıyor. Seçenekler:

- **A)** Bu sayfalara da tema script'i eklenir; `<html data-theme="...">` set edilir. Renkler ana site ile aynı değişkenlere taşınır; site.css bu layout'larda da yüklü olmalı.
- **B)** Login sayfalarında kendi `[data-theme="dark"]` blokları kopyalanır.

Öneri: **A** — Ortak tema script'i + site.css; login stilleri mümkünse ortak değişkenlere taşınır.

### 4.3 Canlı İzleme Index

CanliIzleme/Index.cshtml: `bg-light`, `bg-dark`, `table-dark` vb. — Layout ile aynı `data-theme` kullanılıyorsa site.css dark override'ları ve gerekirse tablo/sınıf override'ları ile uyum sağlanır.

### 4.4 Diğer view'lardaki sabit renkler

- **Rapor/Index.cshtml:** `background: #fff` → tema değişkeni veya class.
- **ResmiTatil/Index.cshtml:** `card bg-light` → tema uyumlu class.
- **Puantaj/Index.cshtml:** Badge sınıfları — Bootstrap dark veya site.css badge override'ları.

---

## 5. Bootstrap ile uyum (isteğe bağlı)

Bootstrap 5.3+ `data-bs-theme="dark"` destekler. İki seçenek:

- **Sadece site.css:** Tüm dark stiller `[data-theme="dark"]` altında kendi override'larımızla.
- **Bootstrap theme + site.css:** Toggle'da `data-bs-theme` de set edilir; form/buton/dropdown Bootstrap dark alır, site.css özel bileşenleri tamamlar.

Bu plan sadece site.css ile tam kontrolü öngörüyor.

---

## 6. Dosya ve Görev Özeti

| Dosya / Bölüm | Yapılacak |
|---------------|------------|
| site.css | `[data-theme="dark"]` paleti; white → tema değişkeni; modal/dropdown/table/sidebar dark override'ları |
| _Layout.cshtml | Tema toggle butonu (header); tema script'i (localStorage + data-theme) |
| site.js | (İsteğe bağlı) Tema init ve toggle fonksiyonu |
| Login / ForgotPassword / CanliIzleme Login | Tema script'i + renkleri ortak değişkenlere taşıma; site.css yüklemesi |
| CanliIzleme/Index, Rapor/Index, ResmiTatil, Puantaj | Sabit renk → tema duyarlı küçük düzenlemeler |
| Modallar | Sadece site.css dark override'ları; view'da değişiklik yok |

---

## 7. Akış özeti

- **Sayfa yükleme:** Script çalışır → localStorage.theme varsa `data-theme` set edilir; yoksa isteğe bağlı `prefers-color-scheme` ile ilk tema.
- **Kullanıcı tıklar:** Tema butonu → `data-theme` + localStorage güncelle → CSS değişkenleri otomatik değişir.

Bu plan uygulandığında modallar ve tüm sayfalar gece/gündüz modu ile uyumlu olur; tek merkezden tema yönetimi ve minimum view değişikliği hedeflenir.
