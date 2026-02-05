using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Linq;

namespace CeyPASS.Web.Controllers
{
    public class CalismaSekliController : Controller
    {
        private readonly ICalismaSekliService _calismaSekliService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private const string PageName = "Vardiyalar";

        public CalismaSekliController(
            ICalismaSekliService calismaSekliService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService)
        {
            _calismaSekliService = calismaSekliService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
        }

        public IActionResult Index()
        {
            // Check authorization
            if (!_authorizationService.ViewAbility(PageName))
            {
                TempData["Error"] = "Vardiyalar ekranını görüntüleme yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            int firmaId = (int)_sessionContext.AktifFirmaId;
            var vardiyalar = _calismaSekliService.GetAll(firmaId, includeGlobal: true);

            ViewBag.CanCreate = _authorizationService.Can(PageName, YetkiTipleri.Create);
            ViewBag.CanUpdate = _authorizationService.Can(PageName, YetkiTipleri.Update);
            ViewBag.CanDelete = _authorizationService.Can(PageName, YetkiTipleri.Delete);

            return View(vardiyalar);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create))
            {
                TempData["Error"] = "Vardiya ekleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            var model = new CalismaSekli
            {
                FirmaId = (int)_sessionContext.AktifFirmaId,
                Baslangic = new TimeSpan(9, 0, 0),
                Bitis = new TimeSpan(18, 0, 0),
                BaslangicTolerans = TimeSpan.Zero,
                BitisTolerans = TimeSpan.Zero,
                YemekAktiflestirme = TimeSpan.Zero
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CalismaSekli vardiya, TimeSpan baslangic, TimeSpan bitis, TimeSpan baslangicTolerans, TimeSpan bitisTolerans, TimeSpan yemekAktiflestirme)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create))
            {
                TempData["Error"] = "Vardiya ekleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            vardiya.Baslangic = baslangic;
            vardiya.Bitis = bitis;
            vardiya.BaslangicTolerans = baslangicTolerans;
            vardiya.BitisTolerans = bitisTolerans;
            vardiya.YemekAktiflestirme = yemekAktiflestirme;

            if (ModelState.IsValid)
            {
                try
                {
                    _calismaSekliService.Add(vardiya);
                    TempData["Success"] = "Vardiya başarıyla eklendi.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Vardiya eklenirken bir hata oluştu: " + ex.Message);
                }
            }

            return View(vardiya);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update))
            {
                TempData["Error"] = "Vardiya güncelleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            int firmaId = (int)_sessionContext.AktifFirmaId;
            var vardiyalar = _calismaSekliService.GetAll(firmaId, includeGlobal: true);
            var vardiya = vardiyalar.FirstOrDefault(v => v.Id == id);

            if (vardiya == null)
            {
                return NotFound();
            }

            return View(vardiya);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CalismaSekli vardiya, TimeSpan baslangic, TimeSpan bitis, TimeSpan baslangicTolerans, TimeSpan bitisTolerans, TimeSpan yemekAktiflestirme)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update))
            {
                TempData["Error"] = "Vardiya güncelleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            vardiya.Baslangic = baslangic;
            vardiya.Bitis = bitis;
            vardiya.BaslangicTolerans = baslangicTolerans;
            vardiya.BitisTolerans = bitisTolerans;
            vardiya.YemekAktiflestirme = yemekAktiflestirme;

            if (ModelState.IsValid)
            {
                try
                {
                    bool success = _calismaSekliService.Update(vardiya);
                    if (success)
                    {
                        TempData["Success"] = "Vardiya başarıyla güncellendi.";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Vardiya güncellenemedi.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Vardiya güncellenirken bir hata oluştu: " + ex.Message);
                }
            }

            return View(vardiya);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Delete))
            {
                TempData["Error"] = "Vardiya silme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            try
            {
                int firmaId = (int)_sessionContext.AktifFirmaId;
                bool success = _calismaSekliService.Delete(id, firmaId);
                if (success)
                {
                    TempData["Success"] = "Vardiya başarıyla silindi.";
                }
                else
                {
                    TempData["Error"] = "Vardiya silinemedi.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hata: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
