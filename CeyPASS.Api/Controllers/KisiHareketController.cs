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
    public class KisiHareketController : ControllerBase
    {
        private readonly IKisiHareketService _kisiHareketService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private const string PageName = "KisiHareketler";

        public KisiHareketController(
            IKisiHareketService kisiHareketService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService)
        {
            _kisiHareketService = kisiHareketService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        public ActionResult<ApiResult<List<KisiHareketListRow>>> Get([FromQuery] string? personelIds, [FromQuery] DateTime? baslangic, [FromQuery] DateTime? bitis, [FromQuery] bool sadeceAktif = true, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();

            int firmaId = _sessionContext.AktifFirmaId ?? 0;
            if (firmaId == 0) return BadRequest(ApiResult.Failure("Firma bilgisi bulunamadı."));

            List<int> pIds = new List<int>();
            if (!string.IsNullOrWhiteSpace(personelIds))
            {
                pIds = personelIds.Split(',').Select(int.Parse).ToList();
            }
            else if (!_sessionContext.IsAdmin() && !string.IsNullOrEmpty(_sessionContext.AktifSicilNo))
            {
                // Normal kullanıcı ise sadece kendi hareketlerini görsün
                if (int.TryParse(_sessionContext.AktifSicilNo, out var myId)) pIds.Add(myId);
            }

            DateTime start = baslangic ?? DateTime.Today;
            DateTime end = bitis ?? DateTime.Today.AddDays(1).AddMinutes(-1);

            int totalCount;
            var items = _kisiHareketService.GetByPersonsPaged(pIds, start, end, sadeceAktif, false, false, firmaId, page, pageSize, out totalCount);

            return Ok(ApiResult<List<KisiHareketListRow>>.Ok(items, $"Toplam {totalCount} kayıt bulundu."));
        }

        [HttpPost("ekle")]
        public ActionResult<ApiResult> Post([FromBody] HareketEkleRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create)) return Forbid();

            int firmaId = _sessionContext.AktifFirmaId ?? 0;
            if (firmaId == 0) return BadRequest(ApiResult.Failure("Firma bilgisi bulunamadı."));

            bool success = _kisiHareketService.InsertManual(firmaId, request.PersonelId, request.Tarih, request.Tip);
            return success ? Ok(ApiResult.Ok("Hareket başarıyla eklendi.")) : BadRequest(ApiResult.Failure("Hareket eklenemedi."));
        }

        [HttpDelete("{id}")]
        public ActionResult<ApiResult> Delete(int id)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Delete)) return Forbid();

            bool success = _kisiHareketService.PasifYap(id);
            return success ? Ok(ApiResult.Ok("Hareket pasif yapıldı.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }
    }

    public class HareketEkleRequest
    {
        public int PersonelId { get; set; }
        public DateTime Tarih { get; set; }
        public string Tip { get; set; } = "G"; // G or Ç
    }
}
