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
    public class PozisyonController : ControllerBase
    {
        private readonly IPozisyonService _pozisyonService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private const string PageName = "Pozisyonlar";

        public PozisyonController(
            IPozisyonService pozisyonService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService)
        {
            _pozisyonService = pozisyonService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        public ActionResult<ApiResult<List<LookupItem>>> Get()
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();
            var list = _pozisyonService.GetAll();
            return Ok(ApiResult<List<LookupItem>>.Ok(list));
        }

        [HttpPost]
        public ActionResult<ApiResult> Post([FromBody] PozisyonRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create)) return Forbid();

            bool ok = _pozisyonService.Add(request.Ad, request.Aciklama ?? "");
            return ok ? Ok(ApiResult.Ok("Pozisyon başarıyla eklendi.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }

        [HttpPut("{id}")]
        public ActionResult<ApiResult> Put(int id, [FromBody] PozisyonRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update)) return Forbid();

            bool ok = _pozisyonService.Update(id, request.Ad, request.Aciklama ?? "");
            return ok ? Ok(ApiResult.Ok("Pozisyon güncellendi.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }

        [HttpDelete("{id}")]
        public ActionResult<ApiResult> Delete(int id)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Delete)) return Forbid();

            bool ok = _pozisyonService.Delete(id);
            return ok ? Ok(ApiResult.Ok("Pozisyon silindi.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }
    }

    public class PozisyonRequest
    {
        public string Ad { get; set; } = null!;
        public string? Aciklama { get; set; }
    }
}
