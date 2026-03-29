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
    public class CihazController : ControllerBase
    {
        private readonly ICihazService _cihazService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private const string PageName = "Cihazlar";

        public CihazController(
            ICihazService cihazService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService)
        {
            _cihazService = cihazService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        public ActionResult<ApiResult<List<CihazListDTO>>> Get([FromQuery] bool sadeceAktif = false)
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();

            int? firmaId = _sessionContext.IsAdmin() ? null : _sessionContext.AktifFirmaId;
            var list = _cihazService.GetListe(sadeceAktif, firmaId);
            return Ok(ApiResult<List<CihazListDTO>>.Ok(list));
        }

        [HttpGet("{id}")]
        public ActionResult<ApiResult<Cihaz>> Get(int id)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update)) return Forbid();

            var item = _cihazService.Get(id);
            if (item == null) return NotFound(ApiResult.Failure("Cihaz bulunamadı."));
            
            // Multitenancy check
            if (!_sessionContext.IsAdmin() && item.FirmaId != _sessionContext.AktifFirmaId) return Forbid();

            return Ok(ApiResult<Cihaz>.Ok(item));
        }

        [HttpPost]
        public ActionResult<ApiResult<int>> Post([FromBody] Cihaz cihaz)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create)) return Forbid();

            if (!_sessionContext.IsAdmin()) cihaz.FirmaId = _sessionContext.AktifFirmaId ?? 0;

            int id = _cihazService.Ekle(cihaz);
            return Ok(ApiResult<int>.Ok(id, "Cihaz başarıyla eklendi."));
        }

        [HttpPut("{id}")]
        public ActionResult<ApiResult> Put(int id, [FromBody] Cihaz cihaz)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update)) return Forbid();
            
            cihaz.CihazId = id;
            if (!_sessionContext.IsAdmin()) cihaz.FirmaId = _sessionContext.AktifFirmaId ?? 0;

            _cihazService.Guncelle(cihaz);
            return Ok(ApiResult.Ok("Cihaz güncellendi."));
        }

        [HttpDelete("{id}")]
        public ActionResult<ApiResult> Delete(int id)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Delete)) return Forbid();

            _cihazService.PasifYap(id);
            return Ok(ApiResult.Ok("Cihaz pasif yapıldı."));
        }

        [HttpPost("{id}/aktif")]
        public ActionResult<ApiResult> Activate(int id)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update)) return Forbid();

            _cihazService.AktifYap(id);
            return Ok(ApiResult.Ok("Cihaz aktif yapıldı."));
        }

        [HttpGet("tipler")]
        public ActionResult<ApiResult<List<CihazTip>>> GetTipler()
        {
            var tipler = _cihazService.GetCihazTipleri();
            return Ok(ApiResult<List<CihazTip>>.Ok(tipler));
        }
    }
}
