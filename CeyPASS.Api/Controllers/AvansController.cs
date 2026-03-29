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
    public class AvansController : ControllerBase
    {
        private readonly IAvansService _avansService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private const string PageName = "Avans";

        public AvansController(
            IAvansService avansService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService)
        {
            _avansService = avansService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        public ActionResult<ApiResult<List<AvansTalep>>> Get()
        {
            if (_authorizationService.ViewAbility(PageName))
            {
                var items = _avansService.TumTalepler();
                return Ok(ApiResult<List<AvansTalep>>.Ok(items));
            }

            // Yetki yoksa sadece kendi taleplerini görsün
            if (!string.IsNullOrEmpty(_sessionContext.AktifSicilNo))
            {
                var items = _avansService.PersonelTalepleri(_sessionContext.AktifSicilNo);
                return Ok(ApiResult<List<AvansTalep>>.Ok(items));
            }

            return Forbid();
        }

        [HttpGet("kendi")]
        public ActionResult<ApiResult<List<AvansTalep>>> GetKendi()
        {
            if (string.IsNullOrEmpty(_sessionContext.AktifSicilNo)) return Unauthorized();
            var items = _avansService.PersonelTalepleri(_sessionContext.AktifSicilNo);
            return Ok(ApiResult<List<AvansTalep>>.Ok(items));
        }

        [HttpPost("talep")]
        public ActionResult<ApiResult<int>> Post([FromBody] AvansTalepRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create)) return Forbid();
            if (string.IsNullOrEmpty(_sessionContext.AktifSicilNo)) return Unauthorized();

            int id = _avansService.TalepOlustur(_sessionContext.AktifSicilNo, request.Miktar, request.Aciklama);
            return Ok(ApiResult<int>.Ok(id, "Avans talebiniz alındı."));
        }

        [HttpPost("onayla")]
        public ActionResult<ApiResult> Onayla([FromBody] AvansOnayRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update)) return Forbid();
            if (!_sessionContext.AktifKullaniciId.HasValue) return Unauthorized();

            bool success = _avansService.Onayla(request.AvansId, _sessionContext.AktifKullaniciId.Value, request.Aciklama);
            return success ? Ok(ApiResult.Ok("Avans onaylandı.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }

        [HttpPost("reddet")]
        public ActionResult<ApiResult> Reddet([FromBody] AvansOnayRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update)) return Forbid();
            if (!_sessionContext.AktifKullaniciId.HasValue) return Unauthorized();

            bool success = _avansService.Reddet(request.AvansId, _sessionContext.AktifKullaniciId.Value, request.Aciklama);
            return success ? Ok(ApiResult.Ok("Avans reddedildi.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }

        [HttpDelete("{id}")]
        public ActionResult<ApiResult> Delete(int id)
        {
            bool success = _avansService.IptalEt(id);
            return success ? Ok(ApiResult.Ok("Avans talebi iptal edildi.")) : BadRequest(ApiResult.Failure("Talep iptal edilemedi."));
        }
    }

    public class AvansTalepRequest
    {
        public decimal Miktar { get; set; }
        public string? Aciklama { get; set; }
    }

    public class AvansOnayRequest
    {
        public int AvansId { get; set; }
        public string? Aciklama { get; set; }
    }
}
