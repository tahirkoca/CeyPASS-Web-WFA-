using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Models;

namespace CeyPASS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class MetadataController : ControllerBase
    {
        private readonly IKisiEkraniLookUpService _lookupService;
        private readonly IFirmaService _firmaService;
        private readonly ICalismaSekliService _calismaSekliService;
        private readonly ISessionContext _sessionContext;

        public MetadataController(
            IKisiEkraniLookUpService lookupService,
            IFirmaService firmaService,
            ICalismaSekliService calismaSekliService,
            ISessionContext sessionContext)
        {
            _lookupService = lookupService;
            _firmaService = firmaService;
            _calismaSekliService = calismaSekliService;
            _sessionContext = sessionContext;
        }

        [HttpGet("lookups")]
        public ActionResult<ApiResult<object>> GetAllLookups()
        {
            int firmaId = _sessionContext.AktifFirmaId ?? 0;
            if (firmaId == 0) return BadRequest(ApiResult.Failure("Firma bilgisi bulunamadı."));

            var data = new
            {
                Isyerleri = _lookupService.GetIsyerleri(firmaId),
                Departmanlar = _lookupService.GetDepartmanlar(firmaId),
                Pozisyonlar = _lookupService.GetPozisyonlar(firmaId),
                Bolumler = _lookupService.GetBolumler(firmaId),
                CalismaStatuleri = _lookupService.GetCalismaStatuleri(firmaId),
                CalismaSekilleri = _calismaSekliService.GetAll(firmaId, true)
            };

            return Ok(ApiResult<object>.Ok(data));
        }

        [HttpGet("firmalar")]
        public ActionResult<ApiResult<List<LookupItem>>> GetFirmalar()
        {
            // Bu liste genelde admin veya geçiş yetkisi olanlar içindir.
            var items = _firmaService.GetLookup();
            return Ok(ApiResult<List<LookupItem>>.Ok(items));
        }
    }
}
