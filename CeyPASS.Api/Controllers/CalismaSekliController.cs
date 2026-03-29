using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using IAuthorizationService = CeyPASS.Business.Abstractions.IAuthorizationService;
using CeyPASS.Entities.Concrete;
using CeyPASS.Models;

namespace CeyPASS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CalismaSekliController : ControllerBase
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

        [HttpGet]
        public ActionResult<ApiResult<List<CalismaSekli>>> Get()
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();
            
            int firmaId = _sessionContext.AktifFirmaId ?? 0;
            var list = _calismaSekliService.GetAll(firmaId, true);
            return Ok(ApiResult<List<CalismaSekli>>.Ok(list));
        }

        [HttpPost]
        public ActionResult<ApiResult<int>> Post([FromBody] CalismaSekli request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create)) return Forbid();

            if (!_sessionContext.IsAdmin()) request.FirmaId = _sessionContext.AktifFirmaId ?? 0;

            int id = _calismaSekliService.Add(request);
            return Ok(ApiResult<int>.Ok(id, "Vardiya başarıyla eklendi."));
        }

        [HttpPut("{id}")]
        public ActionResult<ApiResult> Put(int id, [FromBody] CalismaSekli request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update)) return Forbid();

            request.Id = id;
            if (!_sessionContext.IsAdmin()) request.FirmaId = _sessionContext.AktifFirmaId ?? 0;

            bool ok = _calismaSekliService.Update(request);
            return ok ? Ok(ApiResult.Ok("Vardiya güncellendi.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }

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
