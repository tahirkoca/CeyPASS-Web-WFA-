using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.AspNetCore.Mvc;

namespace CeyPASS.Web.Controllers
{
    public class AvansController : Controller
    {
        private const string PageName = "Avans";

        private readonly IAvansService _avansService;
        private readonly IKisiRepository _kisiRepo;
        private readonly ISessionContext _session;
        private readonly IAuthorizationService _auth;

        public AvansController(IAvansService avansService, IKisiRepository kisiRepo, ISessionContext session, IAuthorizationService auth)
        {
            _avansService = avansService;
            _kisiRepo = kisiRepo;
            _session = session;
            _auth = auth;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!_auth.ViewAbility(PageName))
            {
                TempData["Error"] = "Avans talepleri ekranını görüntüleme yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }
            var items = _avansService.TumTalepler();
            
            // Fetch personnel names
            var pIds = items.Select(x => x.PersonelId).Distinct().ToList();
            var names = new Dictionary<string, string>();
            foreach (var id in pIds)
            {
                var k = _kisiRepo.GetDetay(id);
                if (k != null) names[id] = $"{k.Ad} {k.Soyad}";
            }
            ViewBag.Personeller = names;

            ViewBag.CanUpdate = _auth.Can(PageName, YetkiTipleri.Update);
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Onayla(int id, string? aciklama)
        {
            if (!_auth.Can(PageName, YetkiTipleri.Update))
            {
                TempData["Error"] = "Avans onaylama yetkiniz yok.";
                return RedirectToAction(nameof(Index));
            }
            if (!_session.AktifKullaniciId.HasValue)
            {
                TempData["Error"] = "Oturum bilgisi bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var ok = _avansService.Onayla(id, _session.AktifKullaniciId.Value, aciklama);
            TempData[ok ? "Success" : "Error"] = ok ? "Avans onaylandı." : "İşlem başarısız.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reddet(int id, string? aciklama)
        {
            if (!_auth.Can(PageName, YetkiTipleri.Update))
            {
                TempData["Error"] = "Avans reddetme yetkiniz yok.";
                return RedirectToAction(nameof(Index));
            }
            if (!_session.AktifKullaniciId.HasValue)
            {
                TempData["Error"] = "Oturum bilgisi bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var ok = _avansService.Reddet(id, _session.AktifKullaniciId.Value, aciklama);
            TempData[ok ? "Success" : "Error"] = ok ? "Avans reddedildi." : "İşlem başarısız.";
            return RedirectToAction(nameof(Index));
        }
    }
}

