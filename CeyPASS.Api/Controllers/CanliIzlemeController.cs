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
    public class CanliIzlemeController : ControllerBase
    {
        private readonly ICanliIzlemeService _canliIzlemeService;
        private readonly IKisiHareketService _kisiHareketService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;

        public CanliIzlemeController(
            ICanliIzlemeService canliIzlemeService,
            IKisiHareketService kisiHareketService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService)
        {
            _canliIzlemeService = canliIzlemeService;
            _kisiHareketService = kisiHareketService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
        }

        [HttpGet("son-gecisler")]
        public ActionResult<ApiResult<List<dynamic>>> GetSonGecisler([FromQuery] int take = 10)
        {
            // Bu ekran amirlere veya özel yetkili kullanıcılara (Danışma/Yemekhane) açıktır.
            if (!_sessionContext.IsAdmin() && _sessionContext.RolId != 1) return Forbid();
            if (!_sessionContext.AktifFirmaId.HasValue) return BadRequest(ApiResult.Failure("Firma bilgisi bulunamadı."));

            var passes = _canliIzlemeService.GetLastPasses(_sessionContext.AktifFirmaId.Value, take);
            var result = passes.Select(x => new
            {
                personelId = x.PersonelId,
                adSoyad = x.AdSoyad,
                departmanAdi = x.DepartmanAdi,
                unvan = x.Unvan,
                zaman = x.Zaman,
                terminalAdi = x.TerminalAdi,
                girisMi = x.GirisMi,
                fotoBase64 = (x.Foto != null && x.Foto.Length > 0) ? Convert.ToBase64String(x.Foto) : null
            }).Cast<dynamic>().ToList();

            return Ok(ApiResult<List<dynamic>>.Ok(result));
        }

        [HttpGet("son-hareketler")]
        public ActionResult<ApiResult<List<dynamic>>> GetSonHareketler([FromQuery] int take = 15)
        {
            if (!_sessionContext.IsAdmin() && _sessionContext.RolId != 1) return Forbid();
            if (!_sessionContext.AktifFirmaId.HasValue) return BadRequest(ApiResult.Failure("Firma bilgisi bulunamadı."));

            var moves = _kisiHareketService.GetLastMovesByFirma(take, _sessionContext.AktifFirmaId.Value);
            var result = moves.Select(x => new
            {
                tarih = x.Tarih,
                adSoyad = x.AdSoyad,
                departman = x.Departman,
                unvan = x.Unvan,
                cihazAdi = x.CihazAdi,
                personelId = x.PersonelId
            }).Cast<dynamic>().ToList();

            return Ok(ApiResult<List<dynamic>>.Ok(result));
        }
    }
}
