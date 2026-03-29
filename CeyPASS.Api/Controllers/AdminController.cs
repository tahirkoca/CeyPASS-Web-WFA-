using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Models;

namespace CeyPASS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ISessionContext _sessionContext;
        private readonly IAdminKullaniciRepository _adminKullaniciRepo;
        private readonly IUstYetkiliRepository _ustYetkiliRepo;
        private readonly INotificationService _notificationService;

        public AdminController(
            ISessionContext sessionContext,
            IAdminKullaniciRepository adminKullaniciRepo,
            IUstYetkiliRepository ustYetkiliRepo,
            INotificationService notificationService)
        {
            _sessionContext = sessionContext;
            _adminKullaniciRepo = adminKullaniciRepo;
            _ustYetkiliRepo = ustYetkiliRepo;
            _notificationService = notificationService;
        }

        private bool IsAdmin() => _sessionContext.RolId == 1;

        [HttpPost("kullanici-personel")]
        public ActionResult<ApiResult> SetKullaniciPersonel([FromBody] KullaniciPersonelRequest request)
        {
            if (!IsAdmin()) return Forbid();

            bool ok = _adminKullaniciRepo.SetPersonelId(request.KullaniciId, request.PersonelId);
            return ok ? Ok(ApiResult.Ok("Kullanıcı-personel bağlantısı güncellendi.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }

        [HttpPost("ust-yetkili")]
        public ActionResult<ApiResult> SetUstYetkili([FromBody] UstYetkiliRequest request)
        {
            if (!IsAdmin()) return Forbid();

            bool ok;
            if (string.IsNullOrWhiteSpace(request.UstYetkiliPersonelId))
                ok = _ustYetkiliRepo.Sil(request.PersonelId);
            else
                ok = _ustYetkiliRepo.EkleVeyaGuncelle(request.PersonelId, request.UstYetkiliPersonelId);

            return ok ? Ok(ApiResult.Ok("Üst yetkili kaydı güncellendi.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }

        [HttpPost("guncelleme-mail")]
        public async Task<ActionResult<ApiResult>> SendUpdateMail([FromBody] GuncellemeNotifikasyonDTO model)
        {
            if (!IsAdmin()) return Forbid();

            try
            {
                // Logo base64 logic is usually in the controller in Web.
                // For simplicity, we assume the service handles it or we pass empty.
                bool basarili = await _notificationService.GuncellemeNotifikasyonuGonderAsync(model, null);
                return basarili ? Ok(ApiResult.Ok("Güncelleme bildirimi gönderildi.")) : BadRequest(ApiResult.Failure("Mail gönderilemedi."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Failure(ex.Message));
            }
        }
    }

    public class KullaniciPersonelRequest
    {
        public int KullaniciId { get; set; }
        public int? PersonelId { get; set; }
    }

    public class UstYetkiliRequest
    {
        public string PersonelId { get; set; } = null!;
        public string? UstYetkiliPersonelId { get; set; }
    }
}
