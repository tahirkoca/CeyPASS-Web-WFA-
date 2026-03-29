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
    public class PersonelController : ControllerBase
    {
        private readonly IKisiService _kisiService;
        private readonly IKisiQueryService _kisiQueryService;
        private readonly IKisiEkraniLookUpService _lookupService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private const string PageName = "Personeller";

        public PersonelController(
            IKisiService kisiService,
            IKisiQueryService kisiQueryService,
            IKisiEkraniLookUpService lookupService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService)
        {
            _kisiService = kisiService;
            _kisiQueryService = kisiQueryService;
            _lookupService = lookupService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        public ActionResult<ApiResult<List<KisiListItem>>> Get([FromQuery] string? search, [FromQuery] int? isyeriId, [FromQuery] bool puantajYapilirMi = true, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (!_authorizationService.ViewAbility(PageName))
            {
                return Forbid();
            }

            int firmaId = _sessionContext.AktifFirmaId ?? 0;
            if (firmaId == 0) return BadRequest(ApiResult.Failure("Firma bilgisi bulunamadı."));

            int totalCount;
            var items = _kisiQueryService.GetAktifKisilerByFirmaPaged(firmaId, search, puantajYapilirMi, isyeriId, page, pageSize, out totalCount);

            return Ok(ApiResult<List<KisiListItem>>.Ok(items, $"Toplam {totalCount} kayıt bulundu."));
        }

        [HttpGet("{id}")]
        public ActionResult<ApiResult<KisiDetay>> GetDetails(string id)
        {
            var kisi = _kisiQueryService.GetKisiDetay(id);
            if (kisi == null) return NotFound(ApiResult.Failure("Personel bulunamadı."));

            // IDOR Protection: Firma bazlı filtreleme
            if (!_sessionContext.IsAdmin() && kisi.FirmaId != _sessionContext.AktifFirmaId)
            {
                return Forbid();
            }

            return Ok(ApiResult<KisiDetay>.Ok(kisi));
        }

        [HttpGet("lookups")]
        public ActionResult<ApiResult<object>> GetLookups()
        {
            int firmaId = _sessionContext.AktifFirmaId ?? 0;
            
            var lookups = new
            {
                Isyerleri = _lookupService.GetIsyerleri(firmaId),
                Departmanlar = _lookupService.GetDepartmanlar(firmaId),
                Pozisyonlar = _lookupService.GetPozisyonlar(firmaId),
                Bolumler = _lookupService.GetBolumler(firmaId),
                CalismaStatuleri = _lookupService.GetCalismaStatuleri(firmaId)
            };

            return Ok(ApiResult<object>.Ok(lookups));
        }
    }
}
