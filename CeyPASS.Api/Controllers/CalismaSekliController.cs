using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using IAuthorizationService = CeyPASS.Business.Abstractions.IAuthorizationService;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using CeyPASS.Models;

namespace CeyPASS.Api.Controllers
{
    /// <summary>Vardiya (çalışma şekli) tanımları.</summary>
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CalismaSekliController : ControllerBase
    {
        private readonly ICalismaSekliService _calismaSekliService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly IKullaniciFirmaIsyeriYetkiService _yetkiService;
        private const string PageName = "Vardiyalar";

        public CalismaSekliController(
            ICalismaSekliService calismaSekliService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService,
            IKullaniciFirmaIsyeriYetkiService yetkiService)
        {
            _calismaSekliService = calismaSekliService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
            _yetkiService = yetkiService;
        }

        
        /// <summary>Firmaya göre vardiya listesi; admin firmaId=-1/null ile TÜMÜ.</summary>
        [HttpGet]
        public ActionResult<ApiResult<List<CalismaSekli>>> Get([FromQuery] int? firmaId = null)
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();

            bool isAdmin = _sessionContext.IsAdmin();
            if (isAdmin && (!firmaId.HasValue || firmaId.Value < 0))
            {
                var all = _calismaSekliService.GetAllForAdmin();
                return Ok(ApiResult<List<CalismaSekli>>.Ok(all));
            }

            int fid = firmaId ?? _sessionContext.AktifFirmaId ?? 0;
            if (!isAdmin)
            {
                var yetkiler = _sessionContext.AktifKullaniciId.HasValue
                    ? (_yetkiService.GetYetkiler(_sessionContext.AktifKullaniciId.Value) ?? new List<FirmaIsyeriYetkiDTO>())
                    : new List<FirmaIsyeriYetkiDTO>();
                if (fid <= 0 || !FirmaIsyeriYetkiHelper.IsFirmaAuthorized(fid, yetkiler, false))
                    fid = _sessionContext.AktifFirmaId ?? 0;
            }

            if (fid <= 0)
                return BadRequest(ApiResult<List<CalismaSekli>>.Failure("Firma seçili değil."));

            var list = _calismaSekliService.GetAll(fid, true);
            return Ok(ApiResult<List<CalismaSekli>>.Ok(list));
        }

        
        /// <summary>Yeni vardiya ekler.</summary>
        [HttpPost]
        public ActionResult<ApiResult<int>> Post([FromBody] CalismaSekli request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create)) return Forbid();

            if (!_sessionContext.IsAdmin()) request.FirmaId = _sessionContext.AktifFirmaId ?? 0;

            int id = _calismaSekliService.Add(request);
            return Ok(ApiResult<int>.Ok(id, "Vardiya başarıyla eklendi."));
        }

        
        /// <summary>Vardiya günceller.</summary>
        [HttpPut("{id}")]
        public ActionResult<ApiResult> Put(int id, [FromBody] CalismaSekli request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update)) return Forbid();

            request.Id = id;
            if (!_sessionContext.IsAdmin()) request.FirmaId = _sessionContext.AktifFirmaId ?? 0;

            bool ok = _calismaSekliService.Update(request);
            return ok ? Ok(ApiResult.Ok("Vardiya güncellendi.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }

        
        /// <summary>Vardiya siler.</summary>
        [HttpDelete("{id}")]
        public ActionResult<ApiResult> Delete(int id)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Delete)) return Forbid();

            int firmaId = _sessionContext.AktifFirmaId ?? 0;
            bool ok = _calismaSekliService.Delete(id, firmaId);
            return ok ? Ok(ApiResult.Ok("Vardiya silindi.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }
    }
}
