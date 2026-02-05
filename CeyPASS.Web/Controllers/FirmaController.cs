using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Web.Models.POY;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace CeyPASS.Web.Controllers
{
    public class FirmaController : Controller
    {
        private readonly IFirmaService _firmaService;
        private readonly IAuthorizationService _authorizationService;
        private const string PageName = "Firmalar";

        public FirmaController(IFirmaService firmaService, IAuthorizationService authorizationService)
        {
            _firmaService = firmaService;
            _authorizationService = authorizationService;
        }

        public IActionResult Index()
        {
            if (!_authorizationService.ViewAbility(PageName))
            {
                TempData["Error"] = "Firmalar ekranını görüntüleme yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            var list = _firmaService.GetAll() ?? new System.Collections.Generic.List<Firma>();
            ViewBag.CanCreate = _authorizationService.Can(PageName, YetkiTipleri.Create);
            ViewBag.CanUpdate = _authorizationService.Can(PageName, YetkiTipleri.Update);
            ViewBag.CanDelete = _authorizationService.Can(PageName, YetkiTipleri.Delete);
            return View(list.OrderBy(x => x.FirmaAdi).ToList());
        }

        [HttpGet]
        public IActionResult Create(string returnUrl = null)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create))
            {
                TempData["Error"] = "Firma ekleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            var model = new FirmaFormModel
            {
                FirmaId = _firmaService.SuggestNextId(),
                ReturnUrl = returnUrl
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(FirmaFormModel model)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create))
            {
                TempData["Error"] = "Firma ekleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(model.FirmaAdi))
            {
                ModelState.AddModelError(nameof(model.FirmaAdi), "Firma adı boş olamaz.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!_firmaService.Add(model.FirmaId, model.FirmaAdi.Trim(), (model.ITBirimMail ?? "").Trim(), out var msg))
            {
                ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(msg) ? "Firma ekleme başarısız." : msg);
                return View(model);
            }

            TempData["Success"] = "Firma kaydedildi.";
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id, string returnUrl = null)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update))
            {
                TempData["Error"] = "Firma güncelleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            var row = (_firmaService.GetAll() ?? new System.Collections.Generic.List<Firma>()).FirstOrDefault(x => x.FirmaId == id);
            if (row == null) return NotFound();

            var model = new FirmaFormModel
            {
                FirmaId = row.FirmaId,
                FirmaAdi = row.FirmaAdi,
                ITBirimMail = row.ITBirimMail,
                ReturnUrl = returnUrl
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(FirmaFormModel model)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update))
            {
                TempData["Error"] = "Firma güncelleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(model.FirmaAdi))
            {
                ModelState.AddModelError(nameof(model.FirmaAdi), "Firma adı boş olamaz.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!_firmaService.Update(model.FirmaId, model.FirmaAdi.Trim(), (model.ITBirimMail ?? "").Trim(), out var msg))
            {
                ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(msg) ? "Firma güncelleme başarısız." : msg);
                return View(model);
            }

            TempData["Success"] = "Firma güncellendi.";
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
                TempData["Error"] = "Firma silme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            try
            {
                var ok = _firmaService.Delete(id);
                TempData[ok ? "Success" : "Error"] = ok ? "Firma silindi." : "Firma silinemedi.";
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

