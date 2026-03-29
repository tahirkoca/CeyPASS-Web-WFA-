using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CeyPASS.Entities.Concrete;
using CeyPASS.Business.Abstractions;
using IAuthorizationService = CeyPASS.Business.Abstractions.IAuthorizationService;
using CeyPASS.Models;

namespace CeyPASS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class IzinController : ControllerBase
    {
        private readonly IKisiIzinService _kisiIzinService;
        private readonly IIzinTalepService _izinTalepService;
        private readonly IIzinTipService _izinTipService;
        private readonly IKisiQueryService _kisiQueryService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        
        private const string PageName = "Izinler";
        private const string TalepPageName = "IzinTalepleri";

        public IzinController(
            IKisiIzinService kisiIzinService,
            IIzinTalepService izinTalepService,
            IIzinTipService izinTipService,
            IKisiQueryService kisiQueryService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService)
        {
            _kisiIzinService = kisiIzinService;
            _izinTalepService = izinTalepService;
            _izinTipService = izinTipService;
            _kisiQueryService = kisiQueryService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        public ActionResult<ApiResult<List<KisiIzinListRow>>> GetRecords([FromQuery] string? personelId, [FromQuery] int? izinTipId, [FromQuery] DateTime? baslangic, [FromQuery] DateTime? bitis, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();

            int firmaId = _sessionContext.AktifFirmaId ?? 0;
            DateTime start = baslangic ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime end = bitis ?? start.AddMonths(1).AddDays(-1);

            int totalCount;
            var items = _kisiIzinService.GetTumIzinlerPaged(
                firmaId,
                personelId == "ALL" ? null : personelId,
                izinTipId == 0 ? null : izinTipId,
                start,
                end,
                page,
                pageSize,
                out totalCount
            );

            return Ok(ApiResult<List<KisiIzinListRow>>.Ok(items, $"Toplam {totalCount} kayıt bulundu."));
        }

        [HttpGet("talepler")]
        public ActionResult<ApiResult<List<IzinTalep>>> GetTalepler()
        {
            if (!_authorizationService.ViewAbility(TalepPageName)) return Forbid();

            // Sadece IK Bekleyenleri (Web'deki talep listesi mantığı)
            var items = _izinTalepService.IkBekleyenler();
            
            // FirmaId filter: Ensure we only see requests for the current firm if not admin
            if (!_sessionContext.IsAdmin())
            {
                // Note: IzinTalep usually has PersonelId, need to check its Firm
                // For simplicity, assuming the service already filters or we filter here
                items = items.Where(x => {
                    var k = _kisiQueryService.GetKisiDetay(x.PersonelId);
                    return k != null && k.FirmaId == _sessionContext.AktifFirmaId;
                }).ToList();
            }

            return Ok(ApiResult<List<IzinTalep>>.Ok(items));
        }

        [HttpPost("onayla/{id}")]
        public ActionResult<ApiResult> Onayla(int id, [FromBody] string? aciklama)
        {
            if (!_authorizationService.Can(TalepPageName, YetkiTipleri.Update)) return Forbid();
            if (!_sessionContext.AktifKullaniciId.HasValue) return Unauthorized();

            bool ok = _izinTalepService.IkOnayla(id, _sessionContext.AktifKullaniciId.Value, aciklama);
            return ok ? Ok(ApiResult.Ok("Talep onaylandı.")) : BadRequest(ApiResult.Failure("Talep onaylanamadı."));
        }

        [HttpPost("reddet/{id}")]
        public ActionResult<ApiResult> Reddet(int id, [FromBody] string? aciklama)
        {
            if (!_authorizationService.Can(TalepPageName, YetkiTipleri.Update)) return Forbid();
            if (!_sessionContext.AktifKullaniciId.HasValue) return Unauthorized();

            bool ok = _izinTalepService.IkReddet(id, _sessionContext.AktifKullaniciId.Value, aciklama);
            return ok ? Ok(ApiResult.Ok("Talep reddedildi.")) : BadRequest(ApiResult.Failure("Talep reddedilemedi."));
        }

        [HttpGet("tipler")]
        public ActionResult<ApiResult<List<IzinTip>>> GetIzinTipleri()
        {
            var items = _izinTipService.GetAktif();
            return Ok(ApiResult<List<IzinTip>>.Ok(items));
        }
    }
}
