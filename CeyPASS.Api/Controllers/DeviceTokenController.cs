using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CeyPASS.Models;
using System;
using System.Threading.Tasks;

namespace CeyPASS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DeviceTokenController : ControllerBase
    {
        private readonly IUserDeviceTokenRepository _tokenRepository;
        private readonly ISessionContext _sessionContext;

        public DeviceTokenController(
            IUserDeviceTokenRepository tokenRepository,
            ISessionContext sessionContext)
        {
            _tokenRepository = tokenRepository;
            _sessionContext = sessionContext;
        }

        [HttpPost("register")]
        public ActionResult<CeyPASS.Models.ApiResult> Register([FromBody] TokenRegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Token))
                return BadRequest(CeyPASS.Models.ApiResult.Failure("Token boş olamaz."));

            var tokenEntity = new UserDeviceToken
            {
                KullaniciId = _sessionContext.AktifKullaniciId?.ToString(),
                PersonelId = _sessionContext.AktifSicilNo,
                FCMToken = request.Token,
                DeviceType = request.DeviceType ?? "Unknown",
                LastUpdated = DateTime.Now,
                IsActive = true
            };

            bool ok = _tokenRepository.AddOrUpdate(tokenEntity);
            return ok ? Ok(CeyPASS.Models.ApiResult.Ok("Cihaz kaydedildi.")) : BadRequest(CeyPASS.Models.ApiResult.Failure("Kayıt başarısız."));
        }

        [HttpPost("unregister")]
        public ActionResult<CeyPASS.Models.ApiResult> Unregister([FromBody] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest(CeyPASS.Models.ApiResult.Failure("Token boş olamaz."));

            _tokenRepository.Deactivate(token);
            return Ok(CeyPASS.Models.ApiResult.Ok("Cihaz kaydı silindi."));
        }
    }

    public class TokenRegisterRequest
    {
        public string Token { get; set; } = null!;
        public string? DeviceType { get; set; }
    }
}
