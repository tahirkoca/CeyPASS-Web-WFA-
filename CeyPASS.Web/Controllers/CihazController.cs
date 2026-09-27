using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace CeyPASS.Web.Controllers
{
    /// <summary>Terminal/cihaz tanim yonetimi.</summary>
    public class CihazController : Controller
    {
        private readonly ICihazService _cihazService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly IFirmaService _firmaService;
        private readonly IKullaniciFirmaIsyeriYetkiService _yetkiService;
        private const string PageName = "Cihazlar";

        public CihazController(
            ICihazService cihazService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService,
            IFirmaService firmaService,
            IKullaniciFirmaIsyeriYetkiService yetkiService)
        {
            _cihazService = cihazService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
            _firmaService = firmaService;
            _yetkiService = yetkiService;
        }

        /// <summary>Liste veya ana ekran.</summary>
        public IActionResult Index(int? firmaId = null)
        {
            if (!_authorizationService.ViewAbility(PageName))
            {
                TempData["Error"] = "Cihazlar ekranını görüntüleme yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            bool isAdmin = _sessionContext.IsAdmin();
            var yetkiler = _sessionContext.AktifKullaniciId.HasValue
                ? (_yetkiService.GetYetkiler((int)_sessionContext.AktifKullaniciId.Value) ?? new List<FirmaIsyeriYetkiDTO>())
                : new List<FirmaIsyeriYetkiDTO>();
            var firmalar = FirmaIsyeriYetkiHelper.FilterFirmalar(
                _firmaService.GetAll() ?? new List<Firma>(), yetkiler, isAdmin);

            int? filterFirmaId = ResolveFirmaFilter(firmaId, isAdmin, firmalar);
            var cihazlar = _cihazService.GetListe(sadeceAktif: false, firmaId: filterFirmaId)
                           ?? new List<CihazListDTO>();
            var cihazTipleri = _cihazService.GetCihazTipleri();

            ViewBag.CihazTipleri = cihazTipleri;
            ViewBag.Firmalar = firmalar;
            ViewBag.IsAdmin = isAdmin;
            ViewBag.SelectedFirmaId = filterFirmaId;
            ViewBag.CanCreate = _authorizationService.Can(PageName, YetkiTipleri.Create);
            ViewBag.CanUpdate = _authorizationService.Can(PageName, YetkiTipleri.Update);
            ViewBag.CanDelete = _authorizationService.Can(PageName, YetkiTipleri.Delete);

            return View(cihazlar);
        }

        private int? ResolveFirmaFilter(int? requested, bool isAdmin, List<Firma> firmalar)
        {
            if (isAdmin && (!requested.HasValue || requested.Value < 0))
                return null;
            int fid = requested ?? _sessionContext.AktifFirmaId ?? 0;
            if (fid <= 0)
                return isAdmin ? null : _sessionContext.AktifFirmaId;
            if (!isAdmin && firmalar.All(f => f.FirmaId != fid))
                return _sessionContext.AktifFirmaId;
            return fid;
        }

        /// <summary>Yeni kayit formu ve kaydetme.</summary>
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

        /// <summary>Yeni kayit formu ve kaydetme.</summary>
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

        /// <summary>Kayit guncelleme.</summary>
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

        /// <summary>Kayit guncelleme.</summary>
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

        /// <summary>Kayit silme veya pasiflestirme.</summary>
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

        /// <summary>Pasif kaydi tekrar aktif eder.</summary>
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
