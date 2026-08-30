/**
 * WFA / WPF / Mobile kilavuz goruntuleri — statik fixture HTML + Playwright
 */
import { chromium } from "playwright";
import { mkdir, writeFile } from "fs/promises";
import { dirname, join } from "path";
import { fileURLToPath } from "url";

const __dirname = dirname(fileURLToPath(import.meta.url));
const fixturesDir = join(__dirname, "screenshot-fixtures");
const imagesRoot = join(__dirname, "..", "images");

const desktopShell = (title, body, w = 1280, accent = "#4f46e5") => `<!DOCTYPE html>
<html lang="tr"><head><meta charset="utf-8"/><title>${title}</title>
<style>
*{box-sizing:border-box;margin:0;padding:0;font-family:"Segoe UI",system-ui,sans-serif}
body{background:#f1f5f9;color:#1e293b;height:100vh;display:flex;overflow:hidden}
.sidebar{width:240px;background:#1e293b;color:#e2e8f0;padding:16px 0;flex-shrink:0}
.brand{padding:12px 20px 20px;font-weight:700;font-size:1.1rem;border-bottom:1px solid #334155}
.nav{padding:12px 0;font-size:.9rem}
.nav div{padding:10px 20px;color:#94a3b8}
.nav .active{background:#334155;color:#fff;border-left:3px solid ${accent}}
.main{flex:1;display:flex;flex-direction:column;min-width:0}
.topbar{height:52px;background:#fff;border-bottom:1px solid #e2e8f0;display:flex;align-items:center;padding:0 20px;gap:12px}
.content{padding:20px;overflow:auto;flex:1}
.card{background:#fff;border:1px solid #e2e8f0;border-radius:10px;padding:16px;margin-bottom:16px;box-shadow:0 1px 2px rgb(0 0 0/.05)}
h1{font-size:1.35rem;margin-bottom:14px}
label{font-size:.8rem;color:#64748b;display:block;margin-bottom:4px}
.row{display:flex;flex-wrap:wrap;gap:12px;margin-bottom:12px}
.field{min-width:140px}
select,input{padding:8px 10px;border:1px solid #cbd5e1;border-radius:6px;font-size:.85rem;min-width:130px;background:#fff}
.btn{padding:8px 14px;border-radius:6px;border:none;font-size:.85rem;cursor:pointer}
.btn-primary{background:${accent};color:#fff}
.btn-success{background:#10b981;color:#fff}
.btn-danger{background:#ef4444;color:#fff}
.btn-warn{background:#f59e0b;color:#fff}
table{width:100%;border-collapse:collapse;font-size:.82rem}
th,td{border:1px solid #e2e8f0;padding:8px;text-align:left}
th{background:#f8fafc}
.grid-actions button{margin:0 2px;padding:4px 8px;font-size:.75rem}
.toolbar{display:flex;gap:8px;margin-bottom:12px;flex-wrap:wrap}
.badge{display:inline-block;padding:2px 8px;border-radius:999px;font-size:.72rem}
.b-yellow{background:#fef3c7;color:#92400e}.b-green{background:#d1fae5;color:#065f46}.b-red{background:#fee2e2;color:#991b1b}
</style></head><body>${body}</body></html>`;

const fixtures = [
  {
    path: "wfa/giris/01-login",
    viewport: { width: 1280, height: 800 },
    html: desktopShell("WFA Giris", `<div style="flex:1;display:flex;align-items:center;justify-content:center;background:linear-gradient(135deg,#991b1b,#b91c1c)">
<div style="background:#fff;padding:32px;border-radius:12px;width:380px;box-shadow:0 20px 40px rgb(0 0 0/.2)">
<h2 style="text-align:center;margin-bottom:20px">CeyPASS Giris</h2>
<label>Kullanici Adi</label><select style="width:100%;margin:6px 0 14px"><option>ADMIN</option></select>
<label>Sifre</label><input type="password" value="********" style="width:100%;margin:6px 0 20px"/>
<button class="btn btn-primary" style="width:100%">Giris</button></div></div>`, 1280, "#b91c1c"),
  },
  {
    path: "wfa/personel/01-ekran",
    html: desktopShell("Personel Tanımlama", `<div class="sidebar"><div class="brand">CeyPASS WFA</div><div class="nav"><div class="active">Personel Tanımlama</div><div>İzinler</div><div>Kişi Hareketleri</div><div>Aylık Puantaj</div></div></div>
<div class="main"><div class="topbar"><strong>Personel Tanımlama</strong></div><div class="content">
<div class="card"><div class="row"><div class="field"><label>Firma</label><select><option>CEY Holding</option></select></div>
<div class="field"><label>İşyeri</label><select><option>Merkez</option></select></div>
<div class="field"><label>Ara</label><input value="sicil / ad"/></div><button class="btn btn-primary">Ara</button><button class="btn btn-success">Yeni</button></div></div>
<div class="card"><table><thead><tr><th>Sicil</th><th>Ad Soyad</th><th>Departman</th><th>Durum</th></tr></thead>
<tbody><tr><td>1001</td><td>Ornek Personel</td><td>İK</td><td>Aktif</td></tr>
<tr><td>1002</td><td>Test Kullanici</td><td>Üretim</td><td>Aktif</td></tr></tbody></table></div>
<div class="card"><h1 style="font-size:1rem">Personel Formu</h1><div class="row"><div class="field"><label>Ad</label><input value="Ornek"/></div><div class="field"><label>Soyad</label><input value="Personel"/></div></div>
<button class="btn btn-primary">Kaydet</button> <button class="btn">Vazgeç</button></div></div></div>`),
  },
  {
    path: "wfa/puantaj/01-filtre",
    html: desktopShell("Aylık Puantaj", `<div class="sidebar"><div class="brand">CeyPASS WFA</div><div class="nav"><div>Aylık Puantaj</div><div class="active">Aylık Puantaj</div></div></div>
<div class="main"><div class="topbar"><strong>Aylık Puantaj</strong></div><div class="content">
<div class="card"><div class="row"><div class="field"><label>Ay</label><select><option>Agustos 2026</option></select></div>
<div class="field"><label>Firma</label><select><option>CEY Holding</option></select></div>
<div class="field"><label>İşyeri *</label><select><option>Merkez</option></select></div>
<div class="field"><label>Personel</label><select><option>1001 - Ornek Personel</option></select></div></div></div></div></div>`),
  },
  {
    path: "wfa/puantaj/02-grid-butonlar",
    html: desktopShell("Puantaj ONAY/RET", `<div class="sidebar"><div class="brand">CeyPASS WFA</div></div><div class="main"><div class="topbar"><strong>Aylık Puantaj</strong></div><div class="content">
<div class="card"><table><thead><tr><th>Tarih</th><th>Giris</th><th>Cikis</th><th>Durum</th><th>ONAY</th><th>RET</th><th>DÜZENLE</th></tr></thead>
<tbody><tr><td>27.08.2026</td><td>08:02</td><td>17:01</td><td><span class="badge b-yellow">Bekliyor</span></td>
<td class="grid-actions"><button class="btn btn-success">ONAY</button></td><td><button class="btn btn-danger">RET</button></td><td><button class="btn btn-warn">DÜZENLE</button></td></tr>
<tr><td>26.08.2026</td><td>08:10</td><td>17:05</td><td><span class="badge b-green">Onaylandi</span></td><td>-</td><td>-</td><td>-</td></tr></tbody></table></div></div></div>`),
  },
  {
    path: "wpf/giris/01-login",
    html: desktopShell("WPF Giris", `<div style="flex:1;display:flex;align-items:center;justify-content:center;background:#eef2ff">
<div style="background:#fff;padding:36px;border-radius:14px;width:400px;border:1px solid #e2e8f0">
<h2 style="margin-bottom:16px;color:#4338ca">CeyPASS WPF</h2>
<label>Kullanici Adi</label><input style="width:100%;margin:6px 0 12px" value="ADMIN"/>
<label>Sifre</label><input type="password" style="width:100%;margin:6px 0 18px" value="********"/>
<button class="btn btn-primary" style="width:100%">Giris</button></div></div>`, 1280, "#6366f1"),
  },
  {
    path: "wpf/puantaj/01-toolbar",
    html: desktopShell("WPF Puantaj", `<div class="sidebar"><div class="brand">CeyPASS WPF</div><div class="nav"><div class="active">Aylık Puantaj</div></div></div>
<div class="main"><div class="topbar"><strong>Aylık Puantaj</strong><span style="margin-left:auto">❓ Islem rehberi</span></div><div class="content">
<div class="toolbar"><button class="btn btn-success">Onay</button><button class="btn btn-danger">Ret</button><button class="btn btn-warn">Duzenle</button>
<button class="btn btn-primary">Bugune Kadar Onayla</button><button class="btn">Puantaj Yap</button></div>
<div class="card"><table><thead><tr><th>Tarih</th><th>Giris</th><th>Cikis</th><th>Durum</th></tr></thead>
<tbody><tr style="background:#eff6ff"><td>27.08.2026</td><td>08:02</td><td>17:01</td><td>Bekliyor</td></tr></tbody></table>
<p style="margin-top:10px;font-size:.82rem;color:#64748b">Satir secin, ustteki Onay / Ret / Duzenle dugmelerini kullanin.</p></div></div></div>`, 1280, "#6366f1"),
  },
  {
    path: "wpf/puantaj/02-rehber",
    html: desktopShell("Islem rehberi", `<div class="sidebar"><div class="brand">CeyPASS WPF</div></div><div class="main"><div class="content">
<div class="card" style="max-width:520px;border:2px solid #6366f1"><h1>Islem rehberi — Puantaj</h1>
<ol style="padding-left:20px;line-height:1.8;font-size:.92rem"><li>Ay, firma ve isyeri secin.</li><li>Personel secin; liste yuklenir.</li><li>Tabloda gun satirini secin.</li><li>Ustten Onay, Ret veya Duzenle.</li></ol>
<button class="btn" style="margin-top:12px">Kapat (Esc)</button></div></div></div>`, 1280, "#6366f1"),
  },
];

const mobileShell = (title, body) => `<!DOCTYPE html><html lang="tr"><head><meta charset="utf-8"/><meta name="viewport" content="width=390,initial-scale=1"/>
<title>${title}</title><style>
*{box-sizing:border-box;margin:0;padding:0;font-family:system-ui,-apple-system,sans-serif}
body{width:390px;min-height:844px;background:#f8fafc;color:#0f172a}
.header{background:#b91c1c;color:#fff;padding:14px 16px;font-weight:600;display:flex;align-items:center;gap:10px}
.content{padding:16px}
.card{background:#fff;border-radius:12px;padding:14px;margin-bottom:12px;border:1px solid #e2e8f0;box-shadow:0 1px 2px rgb(0 0 0/.04)}
.btn{display:block;width:100%;padding:12px;border:none;border-radius:10px;background:#b91c1c;color:#fff;font-weight:600;margin-top:10px}
input,select{width:100%;padding:10px;border:1px solid #cbd5e1;border-radius:8px;margin:6px 0 12px}
label{font-size:.78rem;color:#64748b}
.chip{display:inline-block;background:#fef3c7;color:#92400e;padding:4px 10px;border-radius:999px;font-size:.72rem}
.row{display:flex;gap:8px}.row .card{flex:1}
</style></head><body>${body}</body></html>`;

fixtures.push(
  { path: "mobile/giris/01-login", viewport: { width: 390, height: 844 }, html: mobileShell("Giris", `<div class="header">☰ CeyPASS</div><div class="content"><div class="card"><h2 style="margin-bottom:12px">Giris Yap</h2>
<label>Giris Bilgisi</label><input value="sicil / ad / e-posta"/><label>Sifre</label><input type="password" value="********"/><button class="btn">Giris Yap</button></div></div>`) },
  { path: "mobile/dashboard/01-ana", viewport: { width: 390, height: 844 }, html: mobileShell("Ana Sayfa", `<div class="header">☰ Ana Sayfa</div><div class="content"><div class="row"><div class="card"><div style="font-size:.75rem;color:#64748b">Bugun</div><strong>142</strong><div>Giris</div></div><div class="card"><strong>8</strong><div>Gec kalma</div></div></div><div class="card"><strong>Devamsizlik Ozeti</strong><p style="margin-top:8px;font-size:.85rem;color:#64748b">Asagiya cekerek yenileyin.</p></div></div>`) },
  { path: "mobile/personel/01-liste", viewport: { width: 390, height: 844 }, html: mobileShell("Personeller", `<div class="header">☰ Personeller</div><div class="content"><div class="card"><input placeholder="Ara..."/><div style="margin-top:10px;padding:10px 0;border-bottom:1px solid #f1f5f9"><strong>1001</strong> Ornek Personel<br/><span class="chip">Aktif</span></div><div style="padding:10px 0"><strong>1002</strong> Test Kullanici</div></div></div>`) },
  { path: "mobile/puantaj/01-kart", viewport: { width: 390, height: 844 }, html: mobileShell("Puantaj", `<div class="header">☰ Puantaj</div><div class="content"><div class="card"><label>Yil / Ay</label><input value="2026 / Agustos"/><label>Personel</label><input value="1001 Ornek Personel"/><button class="btn" style="background:#4f46e5">Puantaj Getir</button></div>
<div class="card" style="border-left:4px solid #f59e0b"><strong>27 Agustos</strong><div style="font-size:.82rem;margin-top:4px">08:02 — 17:01</div><span class="chip">Bekliyor</span></div></div>`) },
  { path: "mobile/puantaj/02-detay", viewport: { width: 390, height: 844 }, html: mobileShell("Detay", `<div class="header">← Puantaj Detay</div><div class="content"><div class="card"><strong>27 Agustos 2026</strong><p style="margin:8px 0;font-size:.85rem">Giris 08:02 · Cikis 17:01</p><button class="btn" style="background:#10b981">Onayla</button><button class="btn" style="background:#ef4444;margin-top:8px">Reddet</button><button class="btn" style="background:#f59e0b;margin-top:8px">Duzenle</button></div></div>`) },
);

// WFA/WPF extras referenced in kilavuz
const extraPaths = [
  ["wfa/izin/01-ekran", "WFA Izinler", desktopShell("Izinler", `<div class="sidebar"><div class="brand">CeyPASS WFA</div><div class="nav"><div class="active">İzinler</div></div></div><div class="main"><div class="content"><div class="card"><table><thead><tr><th>Personel</th><th>Tur</th><th>Tarih</th></tr></thead><tbody><tr><td>Ornek Personel</td><td>Yillik</td><td>01-05.09.2026</td></tr></tbody></table><button class="btn btn-success" style="margin-top:10px">Yeni Izin</button></div></div></div>`)],
  ["wfa/kisi-hareket/01-ekran", "WFA Hareket", desktopShell("Kisi Hareketleri", `<div class="sidebar"><div class="brand">WFA</div></div><div class="main"><div class="content"><div class="card"><div class="row"><div class="field"><label>Tarih</label><input value="01.08.2026"/></div><div class="field"><label>Personel</label><select><option>Ornek</option></select></div><button class="btn btn-primary">Getir</button></div></div></div></div>`)],
  ["wfa/rapor/01-ekran", "WFA Rapor", desktopShell("Raporlar", `<div class="sidebar"><div class="brand">WFA</div></div><div class="main"><div class="content"><div class="card"><label>Rapor Turu</label><select><option>Devamsizlik</option></select><button class="btn btn-primary">Getir</button></div></div></div>`)],
  ["wpf/personel/01-ekran", "WPF Personel", desktopShell("Personel", `<div class="sidebar"><div class="brand">WPF</div><div class="nav"><div class="active">Personel Tanımlama</div></div></div><div class="main"><div class="content"><div class="card"><table><tr><th>Sicil</th><th>Ad</th></tr><tr><td>1001</td><td>Ornek</td></tr></table></div></div></div>`, 1280, "#6366f1")],
  ["wpf/kisi-hareket/01-ekran", "WPF Hareket", desktopShell("Hareket", `<div class="sidebar"><div class="brand">WPF</div></div><div class="main"><div class="content"><div class="card"><button class="btn btn-success">Yeni Hareket</button></div></div></div>`, 1280, "#6366f1")],
  ["mobile/personel/02-form", "Mobile Form", mobileShell("Form", `<div class="header">← Personel</div><div class="content"><div class="card"><label>Ad</label><input value="Ornek"/><label>Soyad</label><input value="Personel"/><button class="btn">Kaydet</button></div></div>`)],
  ["mobile/izin/01-liste", "Mobile Izin", mobileShell("Izin", `<div class="header">☰ Izinler</div><div class="content"><div class="card"><strong>Yillik Izin</strong><div style="font-size:.82rem">01-03.09.2026</div></div></div>`)],
  ["mobile/kisi-hareket/01-liste", "Mobile Hareket", mobileShell("Hareket", `<div class="header">☰ Kisi Hareketleri</div><div class="content"><div class="card">27.08 08:02 Giris</div></div>`)],
  ["mobile/rapor/01-ekran", "Mobile Rapor", mobileShell("Rapor", `<div class="header">☰ Raporlar</div><div class="content"><div class="card"><select><option>Devamsizlik</option></select><button class="btn">Onizleme</button></div></div>`)],
  ["mobile/profil/01-ekran", "Profil", mobileShell("Profil", `<div class="header">☰ Profil</div><div class="content"><div class="card"><strong>Ornek Personel</strong><p style="font-size:.85rem;margin-top:8px">Sicil: 1001</p></div></div>`)],
  ["mobile/izinlerim/01-ekran", "Izinlerim", mobileShell("Izinlerim", `<div class="header">☰ Izinlerim</div><div class="content"><div class="card"><span class="chip">Bekliyor</span><div style="margin-top:8px">5 gun yillik izin</div><button class="btn">Yeni Talep</button></div></div>`)],
  ["mobile/qr/01-okuma", "QR", mobileShell("QR", `<div class="header">QR Giris</div><div class="content"><div class="card" style="height:280px;background:#111;color:#fff;display:flex;align-items:center;justify-content:center">Kamera onizleme</div></div>`)],
  ["mobile/canli/01-ekran", "Canli", mobileShell("Canli", `<div class="header">Canli Izleme</div><div class="content"><div class="card"><strong>Ornek Personel</strong><div>08:02 · Giris</div></div></div>`)],
];
for (const [path, title, html] of extraPaths) {
  fixtures.push({ path, html, viewport: path.startsWith("mobile") ? { width: 390, height: 844 } : { width: 1280, height: 800 } });
}

async function main() {
  await mkdir(fixturesDir, { recursive: true });
  const browser = await chromium.launch({ headless: true });

  for (const f of fixtures) {
    const htmlPath = join(fixturesDir, `${f.path.replace(/\//g, "-")}.html`);
    await writeFile(htmlPath, f.html, "utf8");
    const page = await browser.newPage();
    await page.setViewportSize(f.viewport || { width: 1280, height: 800 });
    await page.goto(`file:///${htmlPath.replace(/\\/g, "/")}`, { waitUntil: "load" });
    const out = join(imagesRoot, `${f.path}.png`);
    await mkdir(dirname(out), { recursive: true });
    await page.screenshot({ path: out, fullPage: true });
    await page.close();
    console.log(`OK ${out}`);
  }

  await browser.close();
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
