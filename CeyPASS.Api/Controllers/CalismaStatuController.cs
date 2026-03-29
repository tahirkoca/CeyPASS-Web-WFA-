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
    public class CalismaStatuController : ControllerBase
    {
        private readonly ICalismaStatuService _calismaStatuService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private const string PageName = "Statuleri";

        public CalismaStatuController(
            ICalismaStatuService calismaStatuService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService)
        {
            _calismaStatuService = calismaStatuService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        public ActionResult<ApiResult<List<LookupItem>>> Get()
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();
            var list = _calismaStatuService.GetAll();
            return Ok(ApiResult<List<LookupItem>>.Ok(list));
        }

        [HttpPost]
        public ActionResult<ApiResult<int>> Post([FromBody] CalismaStatuRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create)) return Forbid();

            int id = _calismaStatuService.GetNextId();
            bool ok = _calismaStatuService.Add(id, request.Ad);
            return ok ? Ok(ApiResult<int>.Ok(id, "Çalışma statüsü başarıyla eklendi.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }

        [HttpPut("{id}")]
        public ActionResult<ApiResult> Put(int id, [FromBody] CalismaStatuRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update)) return Forbid();

            bool ok = _calismaStatuService.Update(id, request.Ad);
            return ok ? Ok(ApiResult.Ok("Çalışma statüsü güncellendi.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }

        [HttpDelete("{id}")]
        public ActionResult<ApiResult> Delete(int id)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Delete)) return Forbid();

            bool ok = _calismaStatuService.Delete(id);
            return ok ? Ok(ApiResult.Ok("Çalışma statüsü silindi.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }
    }

    public class CalismaStatuRequest
    {
        public string Ad { get; set; } = null!;
    }
}
