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

            bool ok = _departmanService.Update(id, request.Ad, request.Aciklama ?? "");
            return ok ? Ok(ApiResult.Ok("Departman güncellendi.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }

        [HttpDelete("{id}")]
        public ActionResult<ApiResult> Delete(int id)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Delete)) return Forbid();

            bool ok = _departmanService.Delete(id);
            return ok ? Ok(ApiResult.Ok("Departman silindi.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }
    }

    public class DepartmanRequest
    {
        public string Ad { get; set; } = null!;
        public string? Aciklama { get; set; }
    }
}
