using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using IAuthorizationService = CeyPASS.Business.Abstractions.IAuthorizationService;
using CeyPASS.Entities.Concrete;
using CeyPASS.Models;

namespace CeyPASS.Api.Controllers
{
    /// <summary>Firma master verisi (Firmalar sayfası).</summary>
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class FirmaController : ControllerBase
    {
        private readonly IFirmaService _firmaService;
        private readonly IAuthorizationService _authorizationService;
        private const string PageName = "Firmalar";

        public FirmaController(IFirmaService firmaService, IAuthorizationService authorizationService)
        {
            _firmaService = firmaService;
            _authorizationService = authorizationService;
        }

        
        /// <summary>Firma listesi.</summary>
        [HttpGet]
        public ActionResult<ApiResult<List<FirmaRow>>> Get()
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();

            var list = _firmaService.GetAll() ?? new List<Firma>();
            var rows = list
                .OrderBy(x => x.FirmaAdi)
                .Select(x => new FirmaRow(x.FirmaId, x.FirmaAdi, x.ITBirimMail))
                .ToList();

            return Ok(ApiResult<List<FirmaRow>>.Ok(rows));
        }

        
        /// <summary>Yeni firma için önerilen Id.</summary>
        [HttpGet("nextId")]
        public ActionResult<ApiResult<int>> NextId()
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create)) return Forbid();
            var id = _firmaService.SuggestNextId();
            return Ok(ApiResult<int>.Ok(id));
        }

        
        /// <summary>Firma ekler.</summary>
        [HttpPost]
        public ActionResult<ApiResult<int>> Post([FromBody] FirmaUpsertRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create)) return Forbid();

            var firmaAdi = (request.FirmaAdi ?? "").Trim();
            if (string.IsNullOrWhiteSpace(firmaAdi))
                return BadRequest(ApiResult<int>.Failure("Firma adı boş olamaz."));

            var itMail = (request.ITBirimMail ?? "").Trim();
            var id = request.FirmaId.GetValueOrDefault();
            if (id <= 0) id = _firmaService.SuggestNextId();

            if (!_firmaService.Add(id, firmaAdi, itMail, out var msg))
                return BadRequest(ApiResult<int>.Failure(string.IsNullOrWhiteSpace(msg) ? "İşlem başarısız." : msg));

            return Ok(ApiResult<int>.Ok(id, "Firma kaydedildi."));
        }

        
        /// <summary>Firma günceller.</summary>
        [HttpPut("{id}")]
        public ActionResult<ApiResult> Put(int id, [FromBody] FirmaUpsertRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update)) return Forbid();

            var firmaAdi = (request.FirmaAdi ?? "").Trim();
            if (string.IsNullOrWhiteSpace(firmaAdi))
                return BadRequest(ApiResult.Failure("Firma adı boş olamaz."));

            var itMail = (request.ITBirimMail ?? "").Trim();
            if (!_firmaService.Update(id, firmaAdi, itMail, out var msg))
                return BadRequest(ApiResult.Failure(string.IsNullOrWhiteSpace(msg) ? "İşlem başarısız." : msg));

            return Ok(ApiResult.Ok("Firma güncellendi."));
        }

        
        /// <summary>Firma siler.</summary>
        [HttpDelete("{id}")]
        public ActionResult<ApiResult> Delete(int id)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Delete)) return Forbid();

            try
            {
                var ok = _firmaService.Delete(id);
                return ok ? Ok(ApiResult.Ok("Firma silindi.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Failure("Hata: " + ex.Message));
            }
        }
    }

    public record FirmaRow(int FirmaId, string FirmaAdi, string? ITBirimMail);

    public class FirmaUpsertRequest
    {
        public int? FirmaId { get; set; }
        public string? FirmaAdi { get; set; }
        public string? ITBirimMail { get; set; }
    }
}

