using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Models;
using System.IO;

namespace CeyPASS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ISessionContext _sessionContext;
        private readonly IAdminKullaniciRepository _adminKullaniciRepo;
        private readonly IKisiRepository _kisiRepo;
        private readonly IUstYetkiliRepository _ustYetkiliRepo;
        private readonly INotificationService _notificationService;
        private readonly IWebHostEnvironment _env;

        public AdminController(
            ISessionContext sessionContext,
            IAdminKullaniciRepository adminKullaniciRepo,
            IKisiRepository kisiRepo,
            IUstYetkiliRepository ustYetkiliRepo,
            INotificationService notificationService,
            IWebHostEnvironment env)
        {
            _sessionContext = sessionContext;
            _adminKullaniciRepo = adminKullaniciRepo;
            _kisiRepo = kisiRepo;
            _ustYetkiliRepo = ustYetkiliRepo;
            _notificationService = notificationService;
            _env = env;
        }

        private bool IsAdmin() => _sessionContext.RolId == 1;

        [HttpGet("panel")]
        public ActionResult<ApiResult<AdminPanelDto>> Panel()
        {
            if (!IsAdmin()) return Forbid();

            var model = new AdminPanelDto
            {
                Kullanicilar = _adminKullaniciRepo.GetAll() ?? new List<KullaniciAdminRow>(),
                Personeller = _kisiRepo.GetAktifPersonellerIdAd() ?? new List<PersonelAdSoyad>(),
                UstYetkililer = _ustYetkiliRepo.GetAll() ?? new List<UstYetkili>(),
            };
            return Ok(ApiResult<AdminPanelDto>.Ok(model));
        }

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

            if (!GuncellemeDogrula(model, out var hata))
                return BadRequest(ApiResult.Failure(hata));

            try
            {
                string? logoBase64 = GetLogoBase64FromWwwRoot();
                bool basarili = await _notificationService.GuncellemeNotifikasyonuGonderAsync(model, logoBase64);
                return basarili ? Ok(ApiResult.Ok("Güncelleme bildirimi gönderildi.")) : BadRequest(ApiResult.Failure("Mail gönderilemedi."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Failure(ex.Message));
            }
        }

        [HttpPost("guncelleme-mail/preview")]
        public ActionResult<ApiResult<string>> PreviewUpdateMail([FromBody] GuncellemeNotifikasyonDTO model)
        {
            if (!IsAdmin()) return Forbid();

            if (!GuncellemeDogrula(model, out var hata))
                return BadRequest(ApiResult<string>.Failure(hata));

            try
            {
                string? logoBase64 = GetLogoBase64FromWwwRoot();
                var html = _notificationService.OnizlemeHtmlOlustur(model, logoBase64);
                return Ok(ApiResult<string>.Ok(html));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<string>.Failure(ex.Message));
            }
        }

        private string? GetLogoBase64FromWwwRoot()
        {
            try
            {
                var candidates = new List<string>();

                string contentRoot = _env.ContentRootPath ?? "";
                string webRoot = _env.WebRootPath ?? Path.Combine(contentRoot, "wwwroot");

                candidates.Add(Path.Combine(webRoot, "images", "logo.png"));
                candidates.Add(Path.Combine(webRoot, "logo.png"));

                // Dev fallback: read from CeyPASS.Web if co-located in solution
                var solutionRoot = string.IsNullOrWhiteSpace(contentRoot) ? "" : Directory.GetParent(contentRoot)?.FullName ?? "";
                if (!string.IsNullOrWhiteSpace(solutionRoot))
                {
                    candidates.Add(Path.Combine(solutionRoot, "CeyPASS.Web", "wwwroot", "images", "logo.png"));
                    candidates.Add(Path.Combine(solutionRoot, "CeyPASS.Web", "wwwroot", "logo.png"));
                }

                foreach (var path in candidates.Distinct())
                {
                    if (string.IsNullOrWhiteSpace(path)) continue;
                    if (!System.IO.File.Exists(path)) continue;
                    byte[] bytes = System.IO.File.ReadAllBytes(path);
                    if (bytes == null || bytes.Length == 0) continue;
                    return Convert.ToBase64String(bytes);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private static bool GuncellemeDogrula(GuncellemeNotifikasyonDTO dto, out string hata)
        {
            hata = "";
            if (dto == null) { hata = "Model boş olamaz."; return false; }
            if (string.IsNullOrWhiteSpace(dto.VersiyonNumarasi)) { hata = "Versiyon numarası giriniz."; return false; }
            if (string.IsNullOrWhiteSpace(dto.GuncellemeTipi)) { hata = "Güncelleme tipini seçiniz."; return false; }
            if ((dto.YeniOzellikler?.Count ?? 0) == 0 &&
                (dto.Iyilestirmeler?.Count ?? 0) == 0 &&
                (dto.HataDuzeltmeleri?.Count ?? 0) == 0 &&
                (dto.KritikDegisiklikler?.Count ?? 0) == 0)
            {
                hata = "En az bir kategoriye madde eklemelisiniz (her satır bir madde).";
                return false;
            }
            return true;
        }
    }

    public class AdminPanelDto
    {
        public List<KullaniciAdminRow> Kullanicilar { get; set; } = new();
        public List<PersonelAdSoyad> Personeller { get; set; } = new();
        public List<UstYetkili> UstYetkililer { get; set; } = new();
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
