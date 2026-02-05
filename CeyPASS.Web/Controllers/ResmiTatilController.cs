using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Linq;

namespace CeyPASS.Web.Controllers
{
    public class ResmiTatilController : Controller
    {
        private readonly IResmiTatilService _resmiTatilService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private const string PageName = "ResmiTatiller";

        public ResmiTatilController(
            IResmiTatilService resmiTatilService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService)
        {
            _resmiTatilService = resmiTatilService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
        }

        public IActionResult Index(int? yil = null)
        {
            // Check authorization
            if (!_authorizationService.ViewAbility(PageName))
            {
                TempData["Error"] = "Resmi Tatiller ekranını görüntüleme yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            int selectedYil = yil ?? DateTime.Today.Year;
            var resmiTatiller = _resmiTatilService.GetList(selectedYil);

            ViewBag.SelectedYil = selectedYil;
            ViewBag.CanCreate = _authorizationService.Can(PageName, YetkiTipleri.Create);
            ViewBag.CanApprove = _authorizationService.Can(PageName, YetkiTipleri.Approve);

            return View(resmiTatiller);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DoldurSabit(int baslangicYili, int bitisYili)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Approve))
            {
                TempData["Error"] = "Sabit resmi tatilleri aktarma yetkiniz yok.";
                return RedirectToAction("Index");
            }

            try
            {
                _resmiTatilService.DoldurSabit(baslangicYili, bitisYili);
                TempData["Success"] = $"Sabit resmi tatiller {baslangicYili}-{bitisYili} aralığı için işlendi.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hata: " + ex.Message;
            }

            return RedirectToAction("Index", new { yil = baslangicYili });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EkleVeyaGuncelle(DateTime tarih, string ad, decimal? calismaSaat)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create))
            {
                TempData["Error"] = "Resmi tatil ekleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            try
            {
                _resmiTatilService.EkleVeyaGuncelle(tarih, ad, calismaSaat);
                TempData["Success"] = "Resmi tatil başarıyla eklendi/güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hata: " + ex.Message;
            }

            return RedirectToAction("Index", new { yil = tarih.Year });
        }
    }
}
