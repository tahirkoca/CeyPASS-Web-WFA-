/**
 * CeyPASS kilavuz ekran goruntuleri — Playwright
 * Kimlik bilgileri ortam degiskeninden: CEYPASS_DOC_USER, CEYPASS_DOC_PASS
 * Calistirma: docs/scripts/capture-kilavuz-screenshots.ps1
 */
import { chromium } from "playwright";
import { mkdir } from "fs/promises";
import { dirname, join } from "path";
import { fileURLToPath } from "url";

const __dirname = dirname(fileURLToPath(import.meta.url));
const imagesRoot = join(__dirname, "..", "images");
const baseUrl = process.env.CEYPASS_WEB_URL || "http://localhost:5002";
const user = process.env.CEYPASS_DOC_USER || "";
const pass = process.env.CEYPASS_DOC_PASS || "";

/** @type {{ path: string, url: string, wait?: string, viewport?: { width: number, height: number }, auth?: boolean }[]} */
const webShots = [
  { path: "web/giris/01-login", url: "/Account/Login", auth: false },
  { path: "web/dashboard/01-ana-sayfa", url: "/Home/Index", wait: ".sidebar-nav" },
  { path: "web/personel/01-liste-filtre", url: "/Personel/Index", wait: "#filterForm" },
  { path: "web/personel/02-yeni-form", url: "/Personel/Create", wait: "form" },
  { path: "web/izin/01-liste", url: "/Izin/Index", wait: ".container-fluid" },
  { path: "web/izin/02-form", url: "/Izin/Create", wait: "form" },
  { path: "web/kisi-hareket/01-filtre", url: "/KisiHareket/Index", wait: ".container-fluid" },
  { path: "web/puantaj/01-filtre", url: "/Puantaj/Index", wait: "#puantajFilterForm, form, .container-fluid" },
  { path: "web/rapor/01-parametre", url: "/Rapor/Index", wait: ".container-fluid" },
  { path: "web/tanim/firma-01", url: "/Firma/Index", wait: ".container-fluid" },
  { path: "web/tanim/cihaz-01", url: "/Cihaz/Index", wait: ".container-fluid" },
  { path: "web/talep/izin-01", url: "/Izin/TalepListesi", wait: ".container-fluid" },
];

async function ensureDir(filePath) {
  await mkdir(dirname(filePath), { recursive: true });
}

async function login(page) {
  await page.goto(`${baseUrl}/Account/Login`, { waitUntil: "networkidle" });
  await page.fill("#username", user);
  await page.fill("#password", pass);
  await page.click("#loginBtn, button[type=submit].btn-login");
  await page.waitForURL((u) => !u.pathname.toLowerCase().includes("/account/login"), {
    timeout: 30000,
  });
}

async function shot(page, spec) {
  const out = join(imagesRoot, `${spec.path}.png`);
  await ensureDir(out);
  const vp = spec.viewport || { width: 1440, height: 900 };
  await page.setViewportSize(vp);
  await page.goto(`${baseUrl}${spec.url}`, { waitUntil: "networkidle", timeout: 60000 });
  if (spec.wait) {
    await page.waitForSelector(spec.wait, { timeout: 15000 }).catch(() => {});
  }
  await page.waitForTimeout(800);
  await page.screenshot({ path: out, fullPage: true });
  console.log(`OK ${out}`);
}

async function main() {
  if (!user || !pass) {
    console.error("CEYPASS_DOC_USER ve CEYPASS_DOC_PASS gerekli.");
    process.exit(1);
  }

  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    locale: "tr-TR",
    ignoreHTTPSErrors: true,
  });
  const page = await context.newPage();

  for (const spec of webShots.filter((s) => s.auth === false)) {
    await shot(page, spec);
  }

  await login(page);

  for (const spec of webShots.filter((s) => s.auth !== false)) {
    await shot(page, spec);
  }

  // Puantaj tablosu: personel secili URL dene
  try {
    const puantajPage = await context.newPage();
    await puantajPage.setViewportSize({ width: 1440, height: 900 });
    await puantajPage.goto(`${baseUrl}/Puantaj/Index`, { waitUntil: "networkidle" });
    const personelSelect = puantajPage.locator("select[name='personelId'], #personelId");
    if ((await personelSelect.count()) > 0) {
      const options = await personelSelect.locator("option").all();
      for (const opt of options.slice(1, 4)) {
        const val = await opt.getAttribute("value");
        if (val) {
          await personelSelect.selectOption(val);
          break;
        }
      }
      const getir = puantajPage.locator("button:has-text('Puantaj Getir'), input[value*='Puantaj Getir']");
      if ((await getir.count()) > 0) {
        await getir.first().click();
        await puantajPage.waitForTimeout(2000);
      }
    }
    const out2 = join(imagesRoot, "web/puantaj/02-tablo-onay.png");
    await ensureDir(out2);
    await puantajPage.screenshot({ path: out2, fullPage: true });
    console.log(`OK ${out2}`);
    await puantajPage.close();
  } catch (e) {
    console.warn("Puantaj tablo ekran goruntusu atlandi:", e.message);
  }

  await browser.close();
  console.log("Web ekran goruntuleri tamamlandi.");
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
