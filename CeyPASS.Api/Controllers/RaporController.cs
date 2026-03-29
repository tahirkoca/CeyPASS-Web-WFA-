using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CeyPASS.Entities.Concrete;
using IAuthorizationService = CeyPASS.Business.Abstractions.IAuthorizationService;
using CeyPASS.Models;
using CeyPASS.Business.Abstractions;
using CeyPASS.Infrastructure.Helpers;
using System.Data;

namespace CeyPASS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class RaporController : ControllerBase
    {
        private readonly IRaporService _raporService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISessionContext _sessionContext;
        private const string PageName = "Raporlar";

        public RaporController(
            IRaporService raporService,
            IAuthorizationService authorizationService,
            ISessionContext sessionContext)
        {
            _raporService = raporService;
            _authorizationService = authorizationService;
            _sessionContext = sessionContext;
        }

        [HttpGet("list")]
        public ActionResult<ApiResult<List<CeyPASS.Entities.Concrete.RaporTanimi>>> GetRaporlar()
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();
            var raporlar = _raporService.GetirRaporlar();
            return Ok(ApiResult<List<CeyPASS.Entities.Concrete.RaporTanimi>>.Ok(raporlar));
        }

        [HttpPost("export")]
        public async Task<IActionResult> Export([FromBody] RaporExportRequest request)
        {
            if (!_authorizationService.Can(PageName, "Export")) return Forbid();

            try
            {
                // FirmaId validation: Always override with session's FirmaId for security unless Admin
                if (!_sessionContext.IsAdmin())
                {
                    request.Params["@FirmaId"] = _sessionContext.AktifFirmaId ?? 0;
                }

                DataTable dt = _raporService.CalistirRapor(request.ProcedureName, request.Params);
                if (dt == null || dt.Rows.Count == 0) return BadRequest(ApiResult.Failure("Rapor için veri bulunamadı."));

                string fileName = $"{request.ExportTitle}_{DateTime.Now:yyyyMMddHHmm}";
                string extension = request.Format.ToLower() == "excel" ? "xlsx" : "pdf";
                string fullFileName = $"{fileName}.{extension}";
                string contentType = request.Format.ToLower() == "excel" 
                    ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" 
                    : "application/pdf";

                string tempPath = Path.Combine(Path.GetTempPath(), fullFileName);

                if (request.Format.ToLower() == "excel")
                {
                    ExportHelper.ExportToExcel(dt, tempPath);
                }
                else
                {
                    ExportHelper.ExportToPdf(dt, tempPath, request.ExportTitle);
                }

                byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(tempPath);
                System.IO.File.Delete(tempPath);

                return File(fileBytes, contentType, fullFileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Failure($"Rapor oluşturulurken hata: {ex.Message}"));
            }
        }
    }

    public class RaporExportRequest
    {
        public string ProcedureName { get; set; } = string.Empty;
        public string ExportTitle { get; set; } = "Rapor";
        public string Format { get; set; } = "pdf"; // pdf or excel
        public Dictionary<string, object> Params { get; set; } = new();
    }
}
