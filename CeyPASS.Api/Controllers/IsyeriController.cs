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
    public class IsyeriController : ControllerBase
    {
        private readonly IIsyeriService _isyeriService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private const string PageName = "Isyerler";

        public IsyeriController(
            IIsyeriService isyeriService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService)
        {
            _isyeriService = isyeriService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        public ActionResult<ApiResult<List<IsyeriItem>>> Get()
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();

            int firmaId = _sessionContext.AktifFirmaId ?? 0;
            var dt = _isyeriService.GetAll();
            var list = new List<IsyeriItem>();

            if (dt != null)
            {
                foreach (DataRow r in dt.Rows)
                {
                    int fId = r.Table.Columns.Contains("FirmaId") ? Convert.ToInt32(r["FirmaId"]) : 0;
                    if (!_sessionContext.IsAdmin() && fId != firmaId) continue;

                    int iId = r.Table.Columns.Contains("IsyeriId") ? Convert.ToInt32(r["IsyeriId"]) : 0;
                    string ad = r.Table.Columns.Contains("IsyeriAdi") ? (r["IsyeriAdi"]?.ToString() ?? "") : "";
                    
                    if (iId > 0) list.Add(new IsyeriItem(fId, iId, ad));
                }
            }

            return Ok(ApiResult<List<IsyeriItem>>.Ok(list.OrderBy(x => x.Ad).ToList()));
        }

        [HttpPost]
        public ActionResult<ApiResult> Post([FromBody] IsyeriItem item)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create)) return Forbid();

            int firmaId = _sessionContext.IsAdmin() ? item.FirmaId : (_sessionContext.AktifFirmaId ?? 0);
            if (firmaId == 0) return BadRequest(ApiResult.Failure("Firma bilgisi seçilmeli."));

            bool ok = _isyeriService.AddManual(firmaId, item.IsyeriId, item.Ad);
            return ok ? Ok(ApiResult.Ok("İşyeri başarıyla eklendi.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }

        [HttpPut("{id}")]
        public ActionResult<ApiResult> Put(int id, [FromBody] IsyeriItem item)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update)) return Forbid();

            int firmaId = _sessionContext.IsAdmin() ? item.FirmaId : (_sessionContext.AktifFirmaId ?? 0);
            
            bool ok = _isyeriService.Update(firmaId, id, item.Ad);
            return ok ? Ok(ApiResult.Ok("İşyeri güncellendi.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }

        [HttpDelete("{firmaId}/{isyeriId}")]
        public ActionResult<ApiResult> Delete(int firmaId, int isyeriId)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Delete)) return Forbid();
            if (!_sessionContext.IsAdmin() && firmaId != _sessionContext.AktifFirmaId) return Forbid();

            bool ok = _isyeriService.Delete(firmaId, isyeriId);
            return ok ? Ok(ApiResult.Ok("İşyeri silindi.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }
    }

    public record IsyeriItem(int FirmaId, int IsyeriId, string Ad);
}
