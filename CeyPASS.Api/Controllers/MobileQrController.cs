using System.Linq;
using CeyPASS.Business.Abstractions;
using CeyPASS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CeyPASS.Api.Controllers
{
    /// <summary>Mobil QR okutma; sicil JWT claim'inden personel kimliği alınır.</summary>
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

        /// <summary>QR payload'ını işler; giriş/çıkış veya yemek vb. senaryo serviste çözülür.</summary>
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
