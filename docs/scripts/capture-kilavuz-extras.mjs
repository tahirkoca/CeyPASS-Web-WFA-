/** Ek ekran goruntuleri — modal / ozel aksiyonlar */
import { chromium } from "playwright";
import { mkdir } from "fs/promises";
import { dirname, join } from "path";
import { fileURLToPath } from "url";
import { readFileSync } from "fs";

const __dirname = dirname(fileURLToPath(import.meta.url));
const imagesRoot = join(__dirname, "..", "images");
const baseUrl = process.env.CEYPASS_WEB_URL || "http://localhost:5002";
const localSettings = join(__dirname, "..", "..", "CeyPASS.Web", "appsettings.Local.json");

function loadCreds() {
  if (process.env.CEYPASS_DOC_USER && process.env.CEYPASS_DOC_PASS) {
    return { user: process.env.CEYPASS_DOC_USER, pass: process.env.CEYPASS_DOC_PASS };
  }
  const cfg = JSON.parse(readFileSync(localSettings, "utf8"));
  const conn = cfg.ConnectionStrings.DefaultConnection;
  return { user: "ADMIN", conn };
}

async function login(page, user, pass) {
  await page.goto(`${baseUrl}/Account/Login`, { waitUntil: "networkidle" });
  await page.fill("#username", user);
  await page.fill("#password", pass);
  await page.click("#loginBtn, button[type=submit].btn-login");
  await page.waitForURL((u) => !u.pathname.toLowerCase().includes("/account/login"), { timeout: 30000 });
}

async function save(page, relPath) {
  const out = join(imagesRoot, relPath);
  await mkdir(dirname(out), { recursive: true });
  await page.screenshot({ path: out, fullPage: true });
  console.log(`OK ${out}`);
}

async function main() {
  let user = process.env.CEYPASS_DOC_USER;
  let pass = process.env.CEYPASS_DOC_PASS;
  if (!user || !pass) {
    const { execSync } = await import("child_process");
    const cfg = JSON.parse(readFileSync(localSettings, "utf8"));
    const conn = cfg.ConnectionStrings.DefaultConnection;
    const m = conn.match(/Server=([^;]+).*Database=([^;]+).*User Id=([^;]+).*Password=([^;]+)/);
    pass = execSync(
      `sqlcmd -S "${m[1]}" -U ${m[3]} -P "${m[4]}" -d ${m[2]} -Q "SET NOCOUNT ON; SELECT Sifre FROM Kullanicilar WHERE KullaniciAdi='ADMIN'" -h -1 -W`,
      { encoding: "utf8" }
    )
      .split("\n")
      .map((l) => l.trim())
      .find((l) => l && !l.startsWith("-"));
    user = "ADMIN";
  }

  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage({ viewport: { width: 1440, height: 900 } });
  await login(page, user, pass);

  // Personel duzenle (isten cikis dugmesi gorunur)
  await page.goto(`${baseUrl}/Personel/Index`, { waitUntil: "networkidle" });
  const editLink = page.locator("a[href*='/Personel/Edit'], a.btn:has-text('Düzenle'), a:has(.bi-pencil)").first();
  if (await editLink.count()) {
    await editLink.click();
    await page.waitForLoadState("networkidle");
    await save(page, "web/personel/03-isten-cikis.png");
  }

  // Kisi hareket modal
  await page.goto(`${baseUrl}/KisiHareket/Index`, { waitUntil: "networkidle" });
  const addBtn = page.locator("button:has-text('Yeni'), a:has-text('Yeni'), button:has-text('Ekle'), [data-bs-target*='Modal']").first();
  if (await addBtn.count()) {
    await addBtn.click();
    await page.waitForSelector(".modal.show, .modal-dialog", { timeout: 8000 }).catch(() => {});
    await page.waitForTimeout(500);
    await save(page, "web/kisi-hareket/02-modal.png");
    await page.keyboard.press("Escape");
  }

  // Puantaj duzenle modal
  await page.goto(`${baseUrl}/Puantaj/Index`, { waitUntil: "networkidle" });
  const personelSelect = page.locator("select[name='personelId'], #personelId");
  if (await personelSelect.count()) {
    const opts = await personelSelect.locator("option").all();
    for (const opt of opts.slice(1, 6)) {
      const val = await opt.getAttribute("value");
      if (val) {
        await personelSelect.selectOption(val);
        break;
      }
    }
    const getir = page.locator("button:has-text('Puantaj Getir')");
    if (await getir.count()) await getir.first().click();
    await page.waitForTimeout(2500);
    const duzenle = page.locator("button.js-duzenle").first();
    if (await duzenle.count()) {
      await duzenle.click();
      await page.waitForSelector(".modal.show, .modal-dialog", { timeout: 8000 }).catch(() => {});
      await page.waitForTimeout(500);
      await save(page, "web/puantaj/03-duzenle-modal.png");
    }
  }

  await browser.close();
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
