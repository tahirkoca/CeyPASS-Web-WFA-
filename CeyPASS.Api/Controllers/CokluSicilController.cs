using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Models;
using IAuthorizationService = CeyPASS.Business.Abstractions.IAuthorizationService;

namespace CeyPASS.Api.Controllers
{
    /// <summary>Personel çoklu sicil bağlantıları.</summary>
    [Authorize]
    [ApiController]
    [Route("api/v1/personel/{personelId:int}/coklu-sicil")]
    public class CokluSicilController : ControllerBase
    {
        private readonly ICokluSicilService _cokluSicilService;
        private readonly IKisiQueryService _kisiQueryService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private const string PageName = "Personeller";

        public CokluSicilController(
            ICokluSicilService cokluSicilService,
            IKisiQueryService kisiQueryService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService)
        {
            _cokluSicilService = cokluSicilService;
            _kisiQueryService = kisiQueryService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
        }

        
        /// <summary>Ana personel çoklu sicil bağlantıları ve özet.</summary>
        [HttpGet]
        public ActionResult<ApiResult<object>> GetListe(int personelId)
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();
            try
            {
                var baglantilar = _cokluSicilService.GetByAnaPersonelId(personelId);
                var ozet = _cokluSicilService.GetOzet(personelId);
                return Ok(ApiResult<object>.Ok(new { baglantilar, ozet }));
            }
            catch (System.Exception ex)
            {
                return BadRequest(ApiResult.Failure(ex.Message));
            }
        }

        
        /// <summary>Çoklu sicil özet DTO.</summary>
        [HttpGet("ozet")]
        public ActionResult<ApiResult<CokluSicilOzetDTO>> GetOzet(int personelId)
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();
            return Ok(ApiResult<CokluSicilOzetDTO>.Ok(_cokluSicilService.GetOzet(personelId)));
        }

        
        /// <summary>Bağlanabilecek hedef personel adayları.</summary>
        [HttpGet("hedef-adaylari")]
        public ActionResult<ApiResult<System.Collections.Generic.List<CokluSicilHedefAdayDTO>>> GetHedefAdaylari(int personelId)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update)) return Forbid();
            var detay = _kisiQueryService.GetKisiDetay(personelId.ToString());
            if (detay == null) return NotFound(ApiResult.Failure("Personel bulunamadı."));
            var tc = detay.TcKimlikNo ?? "";
            return Ok(ApiResult<System.Collections.Generic.List<CokluSicilHedefAdayDTO>>.Ok(
                _cokluSicilService.GetHedefAdaylari(personelId, tc)));
        }

        
        /// <summary>Çoklu sicil bağlantısı kaydeder.</summary>
        [HttpPost]
        public ActionResult<ApiResult> Upsert(int personelId, [FromBody] CokluSicilUpsertRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update)) return Forbid();
            if (!_sessionContext.AktifKullaniciId.HasValue) return Unauthorized();
            try
            {
                var detay = _kisiQueryService.GetKisiDetay(personelId.ToString());
                if (detay == null) return NotFound(ApiResult.Failure("Personel bulunamadı."));
                _cokluSicilService.Upsert(personelId, detay.TcKimlikNo ?? "", request, _sessionContext.AktifKullaniciId);
                return Ok(ApiResult.Ok("Kaydedildi."));
            }
            catch (System.Exception ex)
            {
                return BadRequest(ApiResult.Failure(ex.Message));
            }
        }

        
        /// <summary>Tek bağlantıyı aktif/pasif yapar.</summary>
        [HttpPatch("{hedefPersonelId:int}/aktif")]
        public ActionResult<ApiResult> SetAktif(int personelId, int hedefPersonelId, [FromQuery] bool aktif = false)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update)) return Forbid();
            if (!_sessionContext.AktifKullaniciId.HasValue) return Unauthorized();
            try
            {
                _cokluSicilService.SetAktif(personelId, hedefPersonelId, aktif, _sessionContext.AktifKullaniciId);
                return Ok(ApiResult.Ok(aktif ? "Bağlantı aktifleştirildi." : "Bağlantı pasifleştirildi."));
            }
            catch (System.Exception ex)
            {
                return BadRequest(ApiResult.Failure(ex.Message));
            }
        }

        
        /// <summary>Tüm bağlantıları pasifleştirir.</summary>
        [HttpPost("pasiflestir-tumunu")]
        public ActionResult<ApiResult> PasiflestirTumunu(int personelId)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update)) return Forbid();
            if (!_sessionContext.AktifKullaniciId.HasValue) return Unauthorized();
            try
            {
                _cokluSicilService.PasifleştirTümünü(personelId, _sessionContext.AktifKullaniciId);
                return Ok(ApiResult.Ok("Tüm bağlantılar pasifleştirildi."));
            }
            catch (System.Exception ex)
            {
                return BadRequest(ApiResult.Failure(ex.Message));
            }
        }
    }
}
