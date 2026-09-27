using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using IAuthorizationService = CeyPASS.Business.Abstractions.IAuthorizationService;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using CeyPASS.Models;

namespace CeyPASS.Api.Controllers
{
    /// <summary>Terminal/cihaz tanımları; firma izolasyonu.</summary>
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CihazController : ControllerBase
    {
        private readonly ICihazService _cihazService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly IKullaniciFirmaIsyeriYetkiService _yetkiService;
        private const string PageName = "Cihazlar";

        public CihazController(
            ICihazService cihazService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService,
            IKullaniciFirmaIsyeriYetkiService yetkiService)
        {
            _cihazService = cihazService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
            _yetkiService = yetkiService;
        }

        /// <summary>Cihaz listesi; admin TÜMÜ veya firmaId ile daraltma.</summary>
        [HttpGet]
        public ActionResult<ApiResult<List<CihazListDTO>>> Get([FromQuery] bool sadeceAktif = false, [FromQuery] int? firmaId = null)
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();

            bool isAdmin = _sessionContext.IsAdmin();
            int? resolved;
            if (isAdmin)
            {
                if (!firmaId.HasValue || firmaId.Value < 0)
                    resolved = null;
                else
                    resolved = firmaId;
            }
            else
            {
                int fid = firmaId ?? _sessionContext.AktifFirmaId ?? 0;
                var yetkiler = _sessionContext.AktifKullaniciId.HasValue
                    ? (_yetkiService.GetYetkiler(_sessionContext.AktifKullaniciId.Value) ?? new List<FirmaIsyeriYetkiDTO>())
                    : new List<FirmaIsyeriYetkiDTO>();
                if (fid <= 0 || !FirmaIsyeriYetkiHelper.IsFirmaAuthorized(fid, yetkiler, false))
                    fid = _sessionContext.AktifFirmaId ?? 0;
                resolved = fid > 0 ? fid : _sessionContext.AktifFirmaId;
            }

            var list = _cihazService.GetListe(sadeceAktif, resolved);
            return Ok(ApiResult<List<CihazListDTO>>.Ok(list));
        }

        /// <summary>Tek cihaz detayı (firma kontrolü).</summary>
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

        
        /// <summary>Yeni cihaz.</summary>
        [HttpPost]
        public ActionResult<ApiResult<int>> Post([FromBody] Cihaz cihaz)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create)) return Forbid();

            if (!_sessionContext.IsAdmin()) cihaz.FirmaId = _sessionContext.AktifFirmaId ?? 0;

            int id = _cihazService.Ekle(cihaz);
            return Ok(ApiResult<int>.Ok(id, "Cihaz başarıyla eklendi."));
        }

        
        /// <summary>Cihaz güncelleme.</summary>
        [HttpPut("{id}")]
        public ActionResult<ApiResult> Put(int id, [FromBody] Cihaz cihaz)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update)) return Forbid();
            
            cihaz.CihazId = id;
            if (!_sessionContext.IsAdmin()) cihaz.FirmaId = _sessionContext.AktifFirmaId ?? 0;

            _cihazService.Guncelle(cihaz);
            return Ok(ApiResult.Ok("Cihaz güncellendi."));
        }

        
        /// <summary>Cihazı pasifleştirir.</summary>
        [HttpDelete("{id}")]
        public ActionResult<ApiResult> Delete(int id)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Delete)) return Forbid();

            _cihazService.PasifYap(id);
            return Ok(ApiResult.Ok("Cihaz pasif yapıldı."));
        }

        
        /// <summary>Pasif cihazı aktifleştirir.</summary>
        [HttpPost("{id}/aktif")]
        public ActionResult<ApiResult> Activate(int id)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update)) return Forbid();

            _cihazService.AktifYap(id);
            return Ok(ApiResult.Ok("Cihaz aktif yapıldı."));
        }

        
        /// <summary>Cihaz tip lookup.</summary>
        [HttpGet("tipler")]
        public ActionResult<ApiResult<List<CihazTip>>> GetTipler()
        {
            var tipler = _cihazService.GetCihazTipleri();
            return Ok(ApiResult<List<CihazTip>>.Ok(tipler));
        }
    }
}
