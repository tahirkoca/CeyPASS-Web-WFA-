using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using System.Linq;

namespace CeyPASS.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly IFirmaService _firmaService;
        private readonly IPuantajService _puantajService;

        public HomeController(
            IDashboardService dashboardService, 
            ISessionContext sessionContext,
            IAuthorizationService authorizationService,
            IFirmaService firmaService,
            IPuantajService puantajService)
        {
            _dashboardService = dashboardService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
            _firmaService = firmaService;
            _puantajService = puantajService;
        }

        public IActionResult Index(int? firmaId = null)
        {
            if (_sessionContext.CurrentUser == null)
                return RedirectToAction("Login", "Account");

            if (!_authorizationService.ViewAbility("Dashboard"))
            {
                TempData["Error"] = "Ana sayfa ekranını görüntüleme yetkiniz yok.";
                return RedirectToAction("Login", "Account");
            }

            bool isAdmin = _sessionContext.RolId == 1 || _sessionContext.RolId == 2;
            int sessionFirmaId = (int)_sessionContext.AktifFirmaId;
            var firmalar = GetFirmalarForUser(isAdmin);
            var firmaIds = firmalar?.Select(f => f.FirmaId).ToHashSet() ?? new HashSet<int>();

            int selectedFirmaId = firmaId ?? sessionFirmaId;

            if (firmaIds.Count > 0 && !firmaIds.Contains(selectedFirmaId))
                selectedFirmaId = firmaIds.Contains(sessionFirmaId) ? sessionFirmaId : firmaIds.First();
            else if (firmaIds.Count == 0)
                selectedFirmaId = sessionFirmaId;

            if (firmaId.HasValue && selectedFirmaId != sessionFirmaId)
                _sessionContext.AktifFirmaId = selectedFirmaId;

            var dashboardData = _dashboardService.GetDashboardForToday(selectedFirmaId);

            ViewBag.SelectedFirmaId = selectedFirmaId;
            ViewBag.Firmalar = firmalar;
            ViewBag.ShowFirmaCombo = firmalar != null && firmalar.Count > 1;

            return View(dashboardData);
        }

        private List<Firma>? GetFirmalarForUser(bool isAdmin)
        {
            if (isAdmin)
                return _firmaService.GetAll().OrderBy(f => f.FirmaAdi).ToList();

            var yetkiler = _puantajService.GetKullaniciFirmaIsyeriYetkileri((int)_sessionContext.AktifKullaniciId!);
            var firmaIds = yetkiler.Select(y => y.FirmaId).Distinct().ToHashSet();
            if (firmaIds.Count == 0)
                return null;

            return _firmaService.GetAll()
                .Where(f => firmaIds.Contains(f.FirmaId))
                .OrderBy(f => f.FirmaAdi)
                .ToList();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult SetFirma(int firmaId, string? returnUrl = null)
        {
            if (_sessionContext.CurrentUser == null)
                return RedirectToAction("Login", "Account");

            bool isAdmin = _sessionContext.RolId == 1 || _sessionContext.RolId == 2;
            var firmalar = GetFirmalarForUser(isAdmin);
            var firmaIds = firmalar?.Select(f => f.FirmaId).ToHashSet() ?? new HashSet<int>();

            if (firmaIds.Count > 0 && !firmaIds.Contains(firmaId))
                firmaId = (int)_sessionContext.AktifFirmaId;

            _sessionContext.AktifFirmaId = firmaId;

            if (!string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith("/") && !returnUrl.StartsWith("//"))
                return Redirect(returnUrl);
            return RedirectToAction("Index");
        }
    }
}
