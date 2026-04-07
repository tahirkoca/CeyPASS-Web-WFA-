using System.Linq;
using CeyPASS.Business.Abstractions;
using CeyPASS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CeyPASS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class MobileQrController : ControllerBase
    {
        private readonly IMobileQrService _mobileQrService;

        public MobileQrController(IMobileQrService mobileQrService)
        {
            _mobileQrService = mobileQrService;
        }

        [HttpPost("Okut")]
        public ActionResult<ApiResult<string>> Okut([FromBody] QrIstekModel request)
        {
            // 1. Kimliği Doğrula (Token'dan SicilNo al)
            var sicilNoClaim = User.Claims.FirstOrDefault(c => c.Type == "SicilNo");
            if (sicilNoClaim == null || string.IsNullOrEmpty(sicilNoClaim.Value))
            {
                sicilNoClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
            }

            if (sicilNoClaim == null || string.IsNullOrEmpty(sicilNoClaim.Value))
                return Unauthorized(ApiResult.Failure("Kullanıcı kimliği doğrulanamadı."));

            string personelId = sicilNoClaim.Value;

            // 2. İşlemi Servise Devret
            var result = _mobileQrService.ProcessQrScan(request, personelId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
