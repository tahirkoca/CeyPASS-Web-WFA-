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
    public class ProfilController : ControllerBase
    {
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly IKisiQueryService _kisiQueryService;
        private readonly IIzinTalepService _izinTalepService;
        private readonly IAvansService _avansService;
        private readonly ISifreService _sifreService;
        private const string PageName = "Profil";

        public ProfilController(
            ISessionContext sessionContext,
            IAuthorizationService authorizationService,
            IKisiQueryService kisiQueryService,
            IIzinTalepService izinTalepService,
            IAvansService avansService,
            ISifreService sifreService)
        {
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
            _kisiQueryService = kisiQueryService;
            _izinTalepService = izinTalepService;
            _avansService = avansService;
            _sifreService = sifreService;
        }

        [HttpGet]
        public ActionResult<ApiResult<ProfilModel>> Get()
        {
            if (string.IsNullOrEmpty(_sessionContext.AktifSicilNo)) return Unauthorized();

            var kisi = _kisiQueryService.GetKisiDetay(_sessionContext.AktifSicilNo);
            if (kisi == null) return NotFound(ApiResult.Failure("Personel bulunamadı."));

            var model = new ProfilModel
            {
                Personel = kisi,
                IsSupervisor = _izinTalepService.IsSupervisor(_sessionContext.AktifSicilNo),
                HasPendingLeaves = _izinTalepService.PersonelTalepleri(_sessionContext.AktifSicilNo).Any(x => x.UstYetkiliOnayDurumu == IzinOnayDurumu.Bekliyor),
                TotalPendingAdvances = _avansService.PersonelTalepleri(_sessionContext.AktifSicilNo).Count(x => x.Durum == AvansDurumu.Bekliyor)
            };

            return Ok(ApiResult<ProfilModel>.Ok(model));
        }

        [HttpPost("sifre-degistir")]
        public ActionResult<ApiResult> ChangePassword([FromBody] SifreDegistirRequest request)
        {
            if (string.IsNullOrEmpty(_sessionContext.AktifSicilNo)) return Unauthorized();

            bool success = _sifreService.SifreSifirlaManuel(_sessionContext.AktifSicilNo, request.YeniSifre);
            return success ? Ok(ApiResult.Ok("Şifreniz başarıyla güncellendi.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }

        [HttpGet("amir-onay-bekleyenler")]
        public ActionResult<ApiResult<List<IzinTalep>>> GetAmirBekleyenler()
        {
            if (string.IsNullOrEmpty(_sessionContext.AktifSicilNo)) return Unauthorized();
            if (!_izinTalepService.IsSupervisor(_sessionContext.AktifSicilNo)) return Forbid();

            var items = _izinTalepService.UstYetkiliBekleyenler(_sessionContext.AktifSicilNo);
            return Ok(ApiResult<List<IzinTalep>>.Ok(items));
        }

        [HttpPost("amir-onayla")]
        public ActionResult<ApiResult> SupervisorApprove([FromBody] AmirOnayRequest request)
        {
            if (string.IsNullOrEmpty(_sessionContext.AktifSicilNo)) return Unauthorized();

            bool success = _izinTalepService.UstYetkiliOnayla(request.TalepId, _sessionContext.AktifSicilNo, request.Aciklama);
            return success ? Ok(ApiResult.Ok("Talep amir tarafında onaylandı.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }

        [HttpPost("amir-reddet")]
        public ActionResult<ApiResult> SupervisorReject([FromBody] AmirOnayRequest request)
        {
            if (string.IsNullOrEmpty(_sessionContext.AktifSicilNo)) return Unauthorized();

            bool success = _izinTalepService.UstYetkiliReddet(request.TalepId, _sessionContext.AktifSicilNo, request.Aciklama);
            return success ? Ok(ApiResult.Ok("Talep amir tarafında reddedildi.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }
    }

    public class ProfilModel
    {
        public KisiDetay Personel { get; set; } = null!;
        public bool IsSupervisor { get; set; }
        public bool HasPendingLeaves { get; set; }
        public int TotalPendingAdvances { get; set; }
    }

    public class SifreDegistirRequest
    {
        public string EskiSifre { get; set; } = null!;
        public string YeniSifre { get; set; } = null!;
    }

    public class AmirOnayRequest
    {
        public int TalepId { get; set; }
        public string? Aciklama { get; set; }
    }
}
