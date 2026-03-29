using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CeyPASS.Entities.Concrete;
using IAuthorizationService = CeyPASS.Business.Abstractions.IAuthorizationService;
using CeyPASS.Models;
using CeyPASS.Business.Abstractions;

namespace CeyPASS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PuantajController : ControllerBase
    {
        private readonly IPuantajService _puantajService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private const string PageName = "AylikPuantaj";

        public PuantajController(
            IPuantajService puantajService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService)
        {
            _puantajService = puantajService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
        }

        [HttpGet("{personelId}")]
        public ActionResult<ApiResult<List<PuantajGunSatirDTO>>> GetAy(int personelId, [FromQuery] int? yil, [FromQuery] int? ay)
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();

            int selectedYil = yil ?? DateTime.Today.Year;
            int selectedAy = ay ?? DateTime.Today.Month;

            try
            {
                var data = _puantajService.GetAy(personelId, selectedYil, selectedAy);
                return Ok(ApiResult<List<PuantajGunSatirDTO>>.Ok(data));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Failure($"Puantaj verisi alınamadı: {ex.Message}"));
            }
        }

        [HttpPost("onayla")]
        public ActionResult<ApiResult> Onayla([FromBody] PuantajOnayRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Approve)) return Forbid();
            if (!_sessionContext.AktifKullaniciId.HasValue) return Unauthorized();

            try
            {
                _puantajService.Onayla(
                    request.PersonelId, 
                    request.Tarih, 
                    request.DuzenlenmisFm, 
                    request.Aciklama ?? "", 
                    request.CalismaTipi ?? "", 
                    request.Saat, 
                    _sessionContext.AktifKullaniciId.Value);

                return Ok(ApiResult.Ok("Puantaj onaylandı."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Failure($"İşlem başarısız: {ex.Message}"));
            }
        }

        [HttpPost("reddet")]
        public ActionResult<ApiResult> Reddet([FromBody] PuantajRedRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Delete)) return Forbid();
            if (!_sessionContext.AktifKullaniciId.HasValue) return Unauthorized();

            try
            {
                _puantajService.Reddet(request.PersonelId, request.Tarih, request.Aciklama ?? "", _sessionContext.AktifKullaniciId.Value);
                return Ok(ApiResult.Ok("Puantaj reddedildi."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Failure($"İşlem başarısız: {ex.Message}"));
            }
        }

        [HttpGet("tipler")]
        public ActionResult<ApiResult<List<PuantajTipDTO>>> GetTipler()
        {
            var tipler = _puantajService.GetPuantajTipleri();
            return Ok(ApiResult<List<PuantajTipDTO>>.Ok(tipler));
        }
    }

    public class PuantajOnayRequest
    {
        public int PersonelId { get; set; }
        public DateTime Tarih { get; set; }
        public int DuzenlenmisFm { get; set; }
        public string? Aciklama { get; set; }
        public string? CalismaTipi { get; set; }
        public decimal Saat { get; set; }
    }

    public class PuantajRedRequest
    {
        public int PersonelId { get; set; }
        public DateTime Tarih { get; set; }
        public string? Aciklama { get; set; }
    }
}
