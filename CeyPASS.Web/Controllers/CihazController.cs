using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Linq;

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
    }
}
