# CeyPASS Kullanici Kilavuzu - SVG placeholder uretici
$root = Join-Path $PSScriptRoot "..\images"
$screens = @(
    @{ path = "web/giris/01-login"; title = "Web - Giris ekrani" },
    @{ path = "web/dashboard/01-ana-sayfa"; title = "Web - Ana Sayfa" },
    @{ path = "web/personel/01-liste-filtre"; title = "Web - Personel listesi ve filtreler" },
    @{ path = "web/personel/02-yeni-form"; title = "Web - Yeni personel formu" },
    @{ path = "web/personel/03-isten-cikis"; title = "Web - Isten cikis onayi" },
    @{ path = "web/izin/01-liste"; title = "Web - Izin listesi" },
    @{ path = "web/izin/02-form"; title = "Web - Izin ekleme/duzenleme" },
    @{ path = "web/kisi-hareket/01-filtre"; title = "Web - Kisi hareketleri filtre" },
    @{ path = "web/kisi-hareket/02-modal"; title = "Web - Hareket ekleme penceresi" },
    @{ path = "web/puantaj/01-filtre"; title = "Web - Puantaj filtre alani" },
    @{ path = "web/puantaj/02-tablo-onay"; title = "Web - Puantaj tablosu onay/ret" },
    @{ path = "web/puantaj/03-duzenle-modal"; title = "Web - Puantaj satir duzenleme" },
    @{ path = "web/rapor/01-parametre"; title = "Web - Rapor parametreleri" },
    @{ path = "web/tanim/firma-01"; title = "Web - Firma tanimlari" },
    @{ path = "web/tanim/cihaz-01"; title = "Web - Cihaz listesi" },
    @{ path = "web/talep/izin-01"; title = "Web - Izin talepleri" },
    @{ path = "wfa/giris/01-login"; title = "WFA - Giris ekrani" },
    @{ path = "wfa/personel/01-ekran"; title = "WFA - Personel tanimlama" },
    @{ path = "wfa/izin/01-ekran"; title = "WFA - Izinler ekrani" },
    @{ path = "wfa/kisi-hareket/01-ekran"; title = "WFA - Kisi hareketleri" },
    @{ path = "wfa/puantaj/01-filtre"; title = "WFA - Aylik puantaj filtre" },
    @{ path = "wfa/puantaj/02-grid-butonlar"; title = "WFA - ONAY/RET/DUZENLE kolonlari" },
    @{ path = "wfa/rapor/01-ekran"; title = "WFA - Raporlar" },
    @{ path = "wpf/giris/01-login"; title = "WPF - Giris ekrani" },
    @{ path = "wpf/personel/01-ekran"; title = "WPF - Personel tanimlama" },
    @{ path = "wpf/puantaj/01-toolbar"; title = "WPF - Puantaj ust arac cubugu" },
    @{ path = "wpf/puantaj/02-rehber"; title = "WPF - Islem rehberi" },
    @{ path = "wpf/kisi-hareket/01-ekran"; title = "WPF - Kisi hareketleri" },
    @{ path = "mobile/giris/01-login"; title = "Mobile - Giris ekrani" },
    @{ path = "mobile/dashboard/01-ana"; title = "Mobile - Ana Sayfa" },
    @{ path = "mobile/personel/01-liste"; title = "Mobile - Personel listesi" },
    @{ path = "mobile/personel/02-form"; title = "Mobile - Personel formu" },
    @{ path = "mobile/izin/01-liste"; title = "Mobile - Izinler" },
    @{ path = "mobile/kisi-hareket/01-liste"; title = "Mobile - Kisi hareketleri" },
    @{ path = "mobile/puantaj/01-kart"; title = "Mobile - Puantaj gun karti" },
    @{ path = "mobile/puantaj/02-detay"; title = "Mobile - Puantaj detay paneli" },
    @{ path = "mobile/rapor/01-ekran"; title = "Mobile - Raporlar" },
    @{ path = "mobile/profil/01-ekran"; title = "Mobile - Profil" },
    @{ path = "mobile/izinlerim/01-ekran"; title = "Mobile - Izinlerim" },
    @{ path = "mobile/qr/01-okuma"; title = "Mobile - QR giris" },
    @{ path = "mobile/canli/01-ekran"; title = "Mobile - Canli izleme" }
)

function New-PlaceholderSvg([string]$title) {
    $t = [System.Security.SecurityElement]::Escape($title)
    @"
<svg xmlns="http://www.w3.org/2000/svg" width="960" height="540" viewBox="0 0 960 540">
  <rect width="960" height="540" fill="#f8fafc"/>
  <rect x="24" y="24" width="912" height="492" rx="12" fill="#e2e8f0" stroke="#cbd5e1" stroke-width="2"/>
  <text x="480" y="250" text-anchor="middle" font-family="Segoe UI, Arial, sans-serif" font-size="22" fill="#475569" font-weight="600">Ekran goruntusu</text>
  <text x="480" y="290" text-anchor="middle" font-family="Segoe UI, Arial, sans-serif" font-size="16" fill="#64748b">$t</text>
  <text x="480" y="330" text-anchor="middle" font-family="Segoe UI, Arial, sans-serif" font-size="13" fill="#94a3b8">PNG ile degistirin - docs/images/README.md</text>
</svg>
"@
}

foreach ($s in $screens) {
    $file = Join-Path $root ($s.path + ".svg")
    $dir = Split-Path $file -Parent
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    New-PlaceholderSvg $s.title | Set-Content -Path $file -Encoding UTF8
    Write-Host "Created $file"
}

Write-Host "Done. $($screens.Count) placeholders."
