using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Entities.Concrete;
using CeyPASS.Web.Models.POY;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace CeyPASS.Web.Controllers
{
    public class DepartmanController : Controller
    {
        private readonly IDepartmanService _departmanService;
        private readonly IAuthorizationService _authorizationService;
        private const string PageName = "Departmanlar";

        public DepartmanController(IDepartmanService departmanService, IAuthorizationService authorizationService)
        {
            _departmanService = departmanService;
            _authorizationService = authorizationService;
        }

        public IActionResult Index()
        {
            if (!_authorizationService.ViewAbility(PageName))
            {
                TempData["Error"] = "Departmanlar ekranını görüntüleme yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            var list = _departmanService.GetAll() ?? new List<LookupItem>();
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
                TempData["Error"] = "Departman ekleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            var model = new DepartmanFormModel
            {
                DepartmanId = _departmanService.GetNextId(),
                ReturnUrl = returnUrl
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DepartmanFormModel model)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create))
            {
                TempData["Error"] = "Departman ekleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(model.DepartmanAdi))
            {
                ModelState.AddModelError(nameof(model.DepartmanAdi), "Departman adı boş olamaz.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!_departmanService.Add(model.DepartmanId, model.DepartmanAdi.Trim(), (model.Aciklama ?? "").Trim()))
            {
                ModelState.AddModelError(string.Empty, "Departman ekleme başarısız.");
                return View(model);
            }

            TempData["Success"] = "Departman kaydedildi.";
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id, string returnUrl = null)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update))
            {
                TempData["Error"] = "Departman güncelleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            var row = _departmanService.GetRowById(id);
            if (row == null) return NotFound();

            var model = new DepartmanFormModel
            {
                DepartmanId = id,
                DepartmanAdi = row["DepartmanAdi"]?.ToString() ?? "",
                Aciklama = row["Aciklama"]?.ToString() ?? "",
                ReturnUrl = returnUrl
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(DepartmanFormModel model)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update))
            {
                TempData["Error"] = "Departman güncelleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(model.DepartmanAdi))
            {
                ModelState.AddModelError(nameof(model.DepartmanAdi), "Departman adı boş olamaz.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!_departmanService.Update(model.DepartmanId, model.DepartmanAdi.Trim(), (model.Aciklama ?? "").Trim()))
            {
                ModelState.AddModelError(string.Empty, "Departman güncelleme başarısız.");
                return View(model);
            }

            TempData["Success"] = "Departman güncellendi.";
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
                TempData["Error"] = "Departman silme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            try
            {
                var ok = _departmanService.Delete(id);
                TempData[ok ? "Success" : "Error"] = ok ? "Departman silindi." : "Departman silinemedi.";
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

