using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using CeyPASS.Models;

namespace CeyPASS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ISessionContext _sessionContext;
        private readonly IIzinTalepService _izinTalepService;
        private readonly IAvansService _avansService;
        private readonly IKisiHareketService _kisiHareketService;
        private readonly IBildirimService _bildirimService;
        private readonly IDashboardService _dashboardService;

        public DashboardController(
            ISessionContext sessionContext,
            IIzinTalepService izinTalepService,
            IAvansService avansService,
            IKisiHareketService kisiHareketService,
            IBildirimService bildirimService,
            IDashboardService dashboardService)
        {
            _sessionContext = sessionContext;
            _izinTalepService = izinTalepService;
            _avansService = avansService;
            _kisiHareketService = kisiHareketService;
            _bildirimService = bildirimService;
            _dashboardService = dashboardService;
        }

        [HttpGet("ozet")]
        public ActionResult<ApiResult<DashboardOzet>> GetOzet()
        {
            int firmaId = _sessionContext.AktifFirmaId ?? 0;
            string? sicilNo = _sessionContext.AktifSicilNo;
            int? kullaniciId = _sessionContext.AktifKullaniciId;

            var ozet = new DashboardOzet();

            if (!string.IsNullOrEmpty(sicilNo))
            {
                ozet.PendingLeaves = _izinTalepService.PersonelTalepleri(sicilNo).Count(x => x.UstYetkiliOnayDurumu == Entities.Concrete.IzinOnayDurumu.Bekliyor);
                ozet.PendingAdvances = _avansService.PersonelTalepleri(sicilNo).Count(x => x.Durum == Entities.Concrete.AvansDurumu.Bekliyor);
                ozet.UnreadNotifications = _bildirimService.GetUnreadCount(sicilNo, kullaniciId);
                
                // Son 24 saat hareket sayısı
                int total;
                var moves = _kisiHareketService.GetByPersonsPaged(new List<int> { int.Parse(sicilNo) }, DateTime.Now.AddDays(-1), DateTime.Now, true, false, false, firmaId, 1, 10, out total);
                ozet.DailyMovesCount = total;
            }

            if (_sessionContext.IsAdmin() || _sessionContext.RolId == 1) // Admin yetkisi
            {
                ozet.TotalPendingApprovals = _izinTalepService.IkBekleyenler().Count;
                ozet.TotalPendingAdvanceApprovals = _avansService.TumTalepler().Count(x => x.Durum == Entities.Concrete.AvansDurumu.Bekliyor);
            }
            else if (!string.IsNullOrEmpty(sicilNo) && _izinTalepService.IsSupervisor(sicilNo))
            {
                ozet.TotalPendingApprovals = _izinTalepService.UstYetkiliBekleyenler(sicilNo).Count;
            }

            return Ok(ApiResult<DashboardOzet>.Ok(ozet));
        }

        [HttpGet("full")]
        public ActionResult<ApiResult<CeyPASS.Entities.Concrete.DashboardResult>> GetFull()
        {
            int firmaId = _sessionContext.AktifFirmaId ?? 0;
            var result = _dashboardService.GetDashboardForToday(firmaId);
            return Ok(ApiResult<CeyPASS.Entities.Concrete.DashboardResult>.Ok(result));
        }
    }

    public class DashboardOzet
    {
        public int PendingLeaves { get; set; }
        public int PendingAdvances { get; set; }
        public int DailyMovesCount { get; set; }
        public int UnreadNotifications { get; set; }
        public int TotalPendingApprovals { get; set; }
        public int TotalPendingAdvanceApprovals { get; set; }
    }
}
