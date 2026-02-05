using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Entities.Concrete;
using System;
using System.Linq;

namespace CeyPASS.Web.Controllers
{
    public class CalismaStatuController : Controller
    {
        private readonly ICalismaStatuService _calismaStatuService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private const string PageName = "CalismaStatuleri";

        public CalismaStatuController(
            ICalismaStatuService calismaStatuService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService)
        {
            _calismaStatuService = calismaStatuService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
        }

        public IActionResult Index()
        {
            // Check authorization
            if (!_authorizationService.ViewAbility(PageName))
            {
                TempData["Error"] = "Çalışma Statüleri ekranını görüntüleme yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            var calismaStatuleri = _calismaStatuService.GetAll();

            ViewBag.CanCreate = _authorizationService.Can(PageName, YetkiTipleri.Create);
            ViewBag.CanUpdate = _authorizationService.Can(PageName, YetkiTipleri.Update);
            ViewBag.CanDelete = _authorizationService.Can(PageName, YetkiTipleri.Delete);

            return View(calismaStatuleri);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string ad)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create))
            {
                TempData["Error"] = "Çalışma statüsü ekleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(ad))
            {
                TempData["Error"] = "Çalışma statüsü adı boş olamaz.";
                return RedirectToAction("Index");
            }

            try
            {
                bool success = _calismaStatuService.AddAuto(ad.Trim());
                if (success)
                {
                    TempData["Success"] = "Çalışma statüsü başarıyla eklendi.";
                }
                else
                {
                    TempData["Error"] = "Çalışma statüsü eklenemedi.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hata: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(int id, string ad)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update))
            {
                TempData["Error"] = "Çalışma statüsü güncelleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(ad))
            {
                TempData["Error"] = "Çalışma statüsü adı boş olamaz.";
                return RedirectToAction("Index");
            }

            try
            {
                bool success = _calismaStatuService.Update(id, ad.Trim());
                if (success)
                {
                    TempData["Success"] = "Çalışma statüsü başarıyla güncellendi.";
                }
                else
                {
                    TempData["Error"] = "Çalışma statüsü güncellenemedi.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hata: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, string returnUrl = null)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Delete))
            {
                TempData["Error"] = "Çalışma statüsü silme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            try
            {
                bool success = _calismaStatuService.Delete(id);
                if (success)
                {
                    TempData["Success"] = "Çalışma statüsü başarıyla silindi.";
                }
                else
                {
                    TempData["Error"] = "Çalışma statüsü silinemedi.";
                }
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
