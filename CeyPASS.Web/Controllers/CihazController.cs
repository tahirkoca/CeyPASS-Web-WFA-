using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Linq;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace CeyPASS.Web.Controllers
{
    public class CihazController : Controller
    {
        private readonly ICihazService _cihazService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private const string PageName = "Cihazlar";

        public CihazController(
            ICihazService cihazService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService)
        {
            _cihazService = cihazService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
        }

        public IActionResult Index()
        {
            // Check authorization
            if (!_authorizationService.ViewAbility(PageName))
            {
                TempData["Error"] = "Cihazlar ekranını görüntüleme yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            var cihazlar = _cihazService.GetListe(sadeceAktif: false, firmaId: (int)_sessionContext.AktifFirmaId);
            var cihazTipleri = _cihazService.GetCihazTipleri();

            ViewBag.CihazTipleri = cihazTipleri;
            ViewBag.CanCreate = _authorizationService.Can(PageName, YetkiTipleri.Create);
            ViewBag.CanUpdate = _authorizationService.Can(PageName, YetkiTipleri.Update);
            ViewBag.CanDelete = _authorizationService.Can(PageName, YetkiTipleri.Delete);

            return View(cihazlar);
        }

        [HttpGet]
        public IActionResult Create(string returnUrl = null)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create))
            {
                TempData["Error"] = "Cihaz ekleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            var model = new Cihaz
            {
                FirmaId = (int)_sessionContext.AktifFirmaId,
                Port = 4370,
                AktifMi = true
            };

            ViewBag.CihazTipleri = _cihazService.GetCihazTipleri();
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Cihaz cihaz, string returnUrl = null)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create))
            {
                TempData["Error"] = "Cihaz ekleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _cihazService.Ekle(cihaz);
                    TempData["Success"] = "Cihaz başarıyla eklendi.";
                    if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Cihaz eklenirken bir hata oluştu: " + ex.Message);
                }
            }

            ViewBag.CihazTipleri = _cihazService.GetCihazTipleri();
            return View(cihaz);
        }

        [HttpGet]
        public IActionResult Edit(int id, string returnUrl = null)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update))
            {
                TempData["Error"] = "Cihaz güncelleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            var cihaz = _cihazService.Get(id);
            if (cihaz == null)
            {
                return NotFound();
            }

            ViewBag.CihazTipleri = _cihazService.GetCihazTipleri();
            ViewBag.ReturnUrl = returnUrl;
            return View(cihaz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Cihaz cihaz, string returnUrl = null)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update))
            {
                TempData["Error"] = "Cihaz güncelleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _cihazService.Guncelle(cihaz);
                    TempData["Success"] = "Cihaz başarıyla güncellendi.";
                    if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Cihaz güncellenirken bir hata oluştu: " + ex.Message);
                }
            }

            ViewBag.CihazTipleri = _cihazService.GetCihazTipleri();
            return View(cihaz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, string returnUrl = null)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Delete))
            {
                TempData["Error"] = "Cihaz silme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            try
            {
                _cihazService.PasifYap(id);
                TempData["Success"] = "Cihaz başarıyla pasif yapıldı.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hata: " + ex.Message;
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AktifYap(int id)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update))
            {
                TempData["Error"] = "Cihaz aktifleştirme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            try
            {
                _cihazService.AktifYap(id);
                TempData["Success"] = "Cihaz başarıyla aktif yapıldı.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hata: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Server-side QR kod üretimi – CDN bağımlılığı yok.
        /// Tarayıcıya doğrudan image/png döndürür.
        /// </summary>
        [HttpGet]
        public IActionResult QrKod(int id)
        {
            var cihaz = _cihazService.Get(id);
            if (cihaz == null) return NotFound();

            if (!cihaz.Latitude.HasValue || !cihaz.Longitude.HasValue)
            {
                return BadRequest("Bu cihaz için Enlem ve Boylam (Konum) tanımlanmamış. Güvenlik nedeniyle QR kod üretilemez.");
            }

            var payload = System.Text.Json.JsonSerializer.Serialize(new { CihazId = id });

            // QR kod matrisini oluştur
            using var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.H);
            using var qrCode = new BitmapByteQRCode(qrData);
            var qrBytes = qrCode.GetGraphic(20); // 20px/modül → A4'te keskin QR

            using var qrStream = new MemoryStream(qrBytes);
            using var qrBitmap = new Bitmap(qrStream);

            // Logoyu yükle
            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "ceyLogo.png");
            using var result = new Bitmap(qrBitmap.Width, qrBitmap.Height);
            using (var g = Graphics.FromImage(result))
            {
                g.DrawImage(qrBitmap, 0, 0);

                if (System.IO.File.Exists(logoPath))
                {
                    using var logo = Image.FromFile(logoPath);
                    int logoSize = result.Width / 4;  // QR genişliğinin %25'i
                    int x = (result.Width  - logoSize) / 2;
                    int y = (result.Height - logoSize) / 2;

                    // Beyaz arka plan
                    g.FillRectangle(Brushes.White, x - 8, y - 8, logoSize + 16, logoSize + 16);
                    g.DrawImage(logo, x, y, logoSize, logoSize);
                }
            }

            using var output = new MemoryStream();
            result.Save(output, ImageFormat.Png);
            return File(output.ToArray(), "image/png");
        }
    }
}
