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
    public class ResmiTatilController : ControllerBase
    {
        private readonly IResmiTatilService _resmiTatilService;
        private readonly IAuthorizationService _authorizationService;
        private const string PageName = "ResmiTatiller";

        public ResmiTatilController(
            IResmiTatilService resmiTatilService,
            IAuthorizationService authorizationService)
        {
            _resmiTatilService = resmiTatilService;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        public ActionResult<ApiResult<List<ResmiTatilDTO>>> Get([FromQuery] int? yil)
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();

            int selectedYil = yil ?? DateTime.Today.Year;
            var list = _resmiTatilService.GetList(selectedYil);
            return Ok(ApiResult<List<ResmiTatilDTO>>.Ok(list));
        }

        [HttpPost]
        public ActionResult<ApiResult> Post([FromBody] ResmiTatilSaveRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create)) return Forbid();

            try
            {
                _resmiTatilService.EkleVeyaGuncelle(request.Tarih, request.Ad, request.CalismaSaat);
                return Ok(ApiResult.Ok("Resmi tatil kaydedildi."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Failure($"Hata: {ex.Message}"));
            }
        }

        [HttpPost("doldur-sabit")]
        public ActionResult<ApiResult> DoldurSabit([FromBody] ResmiTatilDoldurSabitRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Approve)) return Forbid();

            if (request.BaslangicYili < 2000 || request.BitisYili < 2000 || request.BaslangicYili > 2100 || request.BitisYili > 2100)
                return BadRequest(ApiResult.Failure("Yıl aralığı geçersiz."));
            if (request.BitisYili < request.BaslangicYili)
                return BadRequest(ApiResult.Failure("Bitiş yılı başlangıç yılından küçük olamaz."));

            try
            {
                _resmiTatilService.DoldurSabit(request.BaslangicYili, request.BitisYili);
                return Ok(ApiResult.Ok($"Sabit resmi tatiller {request.BaslangicYili}-{request.BitisYili} aralığı için işlendi."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Failure($"Hata: {ex.Message}"));
            }
        }
    }

    public class ResmiTatilSaveRequest
    {
        public DateTime Tarih { get; set; }
        public string Ad { get; set; } = null!;
        public decimal? CalismaSaat { get; set; }
    }

    public class ResmiTatilDoldurSabitRequest
    {
        public int BaslangicYili { get; set; }
        public int BitisYili { get; set; }
    }
}
