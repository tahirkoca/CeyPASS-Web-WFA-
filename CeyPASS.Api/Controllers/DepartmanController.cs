using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using IAuthorizationService = CeyPASS.Business.Abstractions.IAuthorizationService;
using CeyPASS.Entities.Concrete;
using CeyPASS.Models;
using System.Data;

namespace CeyPASS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DepartmanController : ControllerBase
    {
        private readonly IDepartmanService _departmanService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private const string PageName = "Departmanlar";

        public DepartmanController(
            IDepartmanService departmanService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService)
        {
            _departmanService = departmanService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        public ActionResult<ApiResult<List<LookupItem>>> Get()
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();
            var list = _departmanService.GetAll(_sessionContext.AktifFirmaId);
            return Ok(ApiResult<List<LookupItem>>.Ok(list));
        }

        [HttpGet("{id}")]
        public ActionResult<ApiResult<DepartmanDetail>> GetById(int id)
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();

            var row = _departmanService.GetRowById(id);
            if (row == null) return NotFound(ApiResult<DepartmanDetail>.Failure("Kayıt bulunamadı."));

            // IDOR Protection: Ensure department belongs to caller's firm
            if (!_sessionContext.IsAdmin() && row.Table.Columns.Contains("FirmaId") && row["FirmaId"] != null && row["FirmaId"] != DBNull.Value)
            {
                var fId = Convert.ToInt32(row["FirmaId"]);
                if (fId != (_sessionContext.AktifFirmaId ?? 0)) return Forbid();
            }

            var detail = new DepartmanDetail
            {
                Id = id,
                Ad = row.Table.Columns.Contains("DepartmanAdi") ? (row["DepartmanAdi"]?.ToString() ?? "") : "",
                Aciklama = row.Table.Columns.Contains("Aciklama") ? (row["Aciklama"]?.ToString() ?? "") : ""
            };
            return Ok(ApiResult<DepartmanDetail>.Ok(detail));
        }

        [HttpPost]
        public ActionResult<ApiResult<int>> Post([FromBody] DepartmanRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create)) return Forbid();

            int id = _departmanService.GetNextId();
            bool ok = _departmanService.Add(id, request.Ad, request.Aciklama ?? "");
            return ok ? Ok(ApiResult<int>.Ok(id, "Departman başarıyla eklendi.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }

        [HttpPut("{id}")]
        public ActionResult<ApiResult> Put(int id, [FromBody] DepartmanRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update)) return Forbid();

            var existing = _departmanService.GetRowById(id);
            if (existing == null) return NotFound(ApiResult.Failure("Kayıt bulunamadı."));
            if (!_sessionContext.IsAdmin() && existing.Table.Columns.Contains("FirmaId") && existing["FirmaId"] != null && existing["FirmaId"] != DBNull.Value)
            {
                var fId = Convert.ToInt32(existing["FirmaId"]);
                if (fId != (_sessionContext.AktifFirmaId ?? 0)) return Forbid();
            }

            bool ok = _departmanService.Update(id, request.Ad, request.Aciklama ?? "");
            return ok ? Ok(ApiResult.Ok("Departman güncellendi.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }

        [HttpDelete("{id}")]
        public ActionResult<ApiResult> Delete(int id)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Delete)) return Forbid();

            var existing = _departmanService.GetRowById(id);
            if (existing == null) return NotFound(ApiResult.Failure("Kayıt bulunamadı."));
            if (!_sessionContext.IsAdmin() && existing.Table.Columns.Contains("FirmaId") && existing["FirmaId"] != null && existing["FirmaId"] != DBNull.Value)
            {
                var fId = Convert.ToInt32(existing["FirmaId"]);
                if (fId != (_sessionContext.AktifFirmaId ?? 0)) return Forbid();
            }

            bool ok = _departmanService.Delete(id);
            return ok ? Ok(ApiResult.Ok("Departman silindi.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }
    }

    public class DepartmanDetail
    {
        public int Id { get; set; }
        public string Ad { get; set; } = null!;
        public string? Aciklama { get; set; }
    }

    public class DepartmanRequest
    {
        public string Ad { get; set; } = null!;
        public string? Aciklama { get; set; }
    }
}
