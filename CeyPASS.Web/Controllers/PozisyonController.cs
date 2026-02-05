using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Entities.Concrete;
using CeyPASS.Web.Models.POY;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace CeyPASS.Web.Controllers
{
    public class PozisyonController : Controller
    {
        private readonly IPozisyonService _pozisyonService;
        private readonly IAuthorizationService _authorizationService;
        private const string PageName = "Pozisyonlar";

        public PozisyonController(IPozisyonService pozisyonService, IAuthorizationService authorizationService)
        {
            _pozisyonService = pozisyonService;
            _authorizationService = authorizationService;
        }

        public IActionResult Index()
        {
            if (!_authorizationService.ViewAbility(PageName))
            {
                TempData["Error"] = "Pozisyonlar ekranını görüntüleme yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            var list = _pozisyonService.GetAll() ?? new List<LookupItem>();
            ViewBag.CanCreate = _authorizationService.Can(PageName, YetkiTipleri.Create);
            ViewBag.CanUpdate = _authorizationService.Can(PageName, YetkiTipleri.Update);
            ViewBag.CanDelete = _authorizationService.Can(PageName, YetkiTipleri.Delete);
            return View(list);
        }

        [HttpGet]
        public IActionResult Create(string returnUrl = null)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create))
            {
                TempData["Error"] = "Pozisyon ekleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            return View(new PozisyonFormModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PozisyonFormModel model)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create))
            {
                TempData["Error"] = "Pozisyon ekleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(model.PozisyonAdi))
            {
                ModelState.AddModelError(nameof(model.PozisyonAdi), "Pozisyon adı boş olamaz.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!_pozisyonService.Add(model.PozisyonAdi.Trim(), (model.Aciklama ?? "").Trim()))
            {
                ModelState.AddModelError(string.Empty, "Pozisyon ekleme başarısız.");
                return View(model);
            }

            TempData["Success"] = "Pozisyon kaydedildi.";
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id, string returnUrl = null)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update))
            {
                TempData["Error"] = "Pozisyon güncelleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            var data = _pozisyonService.GetForEdit(id);
            if (!data.HasValue) return NotFound();

            var model = new PozisyonFormModel
            {
                PozisyonId = data.Value.id,
                PozisyonAdi = data.Value.ad,
                Aciklama = data.Value.ack,
                ReturnUrl = returnUrl
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(PozisyonFormModel model)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update))
            {
                TempData["Error"] = "Pozisyon güncelleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            if (model.PozisyonId <= 0) ModelState.AddModelError(nameof(model.PozisyonId), "Pozisyon Id geçersiz.");
            if (string.IsNullOrWhiteSpace(model.PozisyonAdi)) ModelState.AddModelError(nameof(model.PozisyonAdi), "Pozisyon adı boş olamaz.");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!_pozisyonService.Update(model.PozisyonId, model.PozisyonAdi.Trim(), (model.Aciklama ?? "").Trim()))
            {
                ModelState.AddModelError(string.Empty, "Pozisyon güncelleme başarısız.");
                return View(model);
            }

            TempData["Success"] = "Pozisyon güncellendi.";
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, string returnUrl = null)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Delete))
            {
                TempData["Error"] = "Pozisyon silme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            try
            {
                var ok = _pozisyonService.Delete(id);
                TempData[ok ? "Success" : "Error"] = ok ? "Pozisyon silindi." : "Pozisyon silinemedi.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hata: " + ex.Message;
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index");
        }
    }
}

