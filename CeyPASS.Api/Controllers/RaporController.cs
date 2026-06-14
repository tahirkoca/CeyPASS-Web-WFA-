using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CeyPASS.Entities.Concrete;
using IAuthorizationService = CeyPASS.Business.Abstractions.IAuthorizationService;
using CeyPASS.Models;
using CeyPASS.Business.Abstractions;
using CeyPASS.Infrastructure.Helpers;
using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Globalization;

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
        private readonly IKullaniciQueryService _kullaniciQueryService;
        private readonly IKullaniciFirmaIsyeriYetkiService _yetkiSvc;
        private readonly IKisiEkraniLookUpService _lookupService;
        private const string PageName = "Raporlar";

        public RaporController(
            IRaporService raporService,
            IAuthorizationService authorizationService,
            ISessionContext sessionContext,
            IKullaniciQueryService kullaniciQueryService,
            IKullaniciFirmaIsyeriYetkiService yetkiSvc,
            IKisiEkraniLookUpService lookupService)
        {
            _raporService = raporService;
            _authorizationService = authorizationService;
            _sessionContext = sessionContext;
            _kullaniciQueryService = kullaniciQueryService;
            _yetkiSvc = yetkiSvc;
            _lookupService = lookupService;
        }

        public sealed class PagedResponse<T>
        {
            public List<T> Items { get; set; } = new();
            public int TotalCount { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public int TotalPages { get; set; }
        }

        public sealed class ReportTable
        {
            public List<string> Columns { get; set; } = new();
            public List<List<string?>> Rows { get; set; } = new();
        }

        public sealed class RunReportRequest
        {
            public string ProcedureAdi { get; set; } = string.Empty;
            public int? FirmaId { get; set; }
            public List<int>? IsyeriIds { get; set; }
            public DateTime TarihBaslangic { get; set; }
            public DateTime TarihBitis { get; set; }
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 100;
        }

        [HttpGet("list")]
        public ActionResult<ApiResult<List<CeyPASS.Entities.Concrete.RaporTanimi>>> GetRaporlar()
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();
            var raporlar = _raporService.GetirRaporlar();
            return Ok(ApiResult<List<CeyPASS.Entities.Concrete.RaporTanimi>>.Ok(raporlar));
        }

        [HttpGet("isyerleri")]
        public ActionResult<ApiResult<List<LookupItem>>> GetIsyerleri()
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();

            int firmaId = _sessionContext.AktifFirmaId ?? 0;
            if (firmaId == 0) return BadRequest(ApiResult.Failure("Firma bilgisi bulunamadı."));

            bool isAdmin = _sessionContext.IsAdmin();
            List<FirmaIsyeriYetkiDTO>? yetkiler = null;
            if (_sessionContext.AktifKullaniciId.HasValue)
                yetkiler = _yetkiSvc.GetYetkiler(_sessionContext.AktifKullaniciId.Value);

            if (!FirmaIsyeriYetkiHelper.IsFirmaAuthorized(firmaId, yetkiler, isAdmin))
                return Forbid();

            var items = FirmaIsyeriYetkiHelper.FilterIsyeriLookup(
                _lookupService.GetIsyerleri(firmaId) ?? new List<LookupItem>(),
                firmaId,
                yetkiler,
                isAdmin);

            return Ok(ApiResult<List<LookupItem>>.Ok(items));
        }

        [HttpPost("run")]
        public ActionResult<ApiResult<PagedResponse<ReportTable>>> Run([FromBody] RunReportRequest request)
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();
            if (string.IsNullOrWhiteSpace(request.ProcedureAdi))
                return BadRequest(ApiResult.Failure("Rapor seçiniz."));

            int firmaId = request.FirmaId ?? (_sessionContext.AktifFirmaId ?? 0);
            if (!_sessionContext.IsAdmin())
                firmaId = _sessionContext.AktifFirmaId ?? firmaId;
            if (firmaId == 0) return BadRequest(ApiResult.Failure("Firma bilgisi bulunamadı."));

            if (request.Page < 1) request.Page = 1;
            if (request.PageSize < 1) request.PageSize = 100;

            bool isAdmin = _sessionContext.IsAdmin();
            List<FirmaIsyeriYetkiDTO>? yetkiler = null;
            if (_sessionContext.AktifKullaniciId.HasValue)
                yetkiler = _yetkiSvc.GetYetkiler(_sessionContext.AktifKullaniciId.Value);

            if (!FirmaIsyeriYetkiHelper.IsFirmaAuthorized(firmaId, yetkiler, isAdmin))
                return Forbid();

            var (isyeriIdCsv, status) = BuildRaporIsyeriIdListCsv(
                firmaId,
                request.IsyeriIds,
                yetkiler,
                isAdmin);

            if (status == FirmaIsyeriYetkiHelper.RaporIsyeriListStatus.UnauthorizedSelection)
                return BadRequest(ApiResult.Failure("Seçilen işyerlerden bazıları için yetkiniz yok."));
            if (status == FirmaIsyeriYetkiHelper.RaporIsyeriListStatus.NoAccess)
                return Forbid();

            string firmaIdCsv = firmaId > 0 ? firmaId.ToString() : "";

            var parametreler = new Dictionary<string, object>
            {
                { "@FirmaIdList", firmaIdCsv },
                { "@IsyeriIdList", isyeriIdCsv ?? "" },
                { "@TarihBaslangic", RaporTarihHelper.ToReportRangeStart(request.TarihBaslangic) },
                { "@TarihBitis", RaporTarihHelper.ToReportRangeEnd(request.TarihBitis) },
            };

            DataTable dt = _raporService.CalistirRapor(request.ProcedureAdi, parametreler);
            if (dt == null) return Ok(ApiResult<PagedResponse<ReportTable>>.Ok(new PagedResponse<ReportTable> { Items = new() }));

            int totalCount = dt.Rows.Count;
            int totalPages = request.PageSize <= 0 ? 1 : (int)Math.Ceiling(totalCount / (double)request.PageSize);
            if (totalPages < 1) totalPages = 1;
            if (request.Page > totalPages) request.Page = totalPages;

            int start = (request.Page - 1) * request.PageSize;
            int endExclusive = Math.Min(totalCount, start + request.PageSize);

            var table = new ReportTable
            {
                Columns = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList(),
                Rows = new List<List<string?>>()
            };

            for (int i = start; i < endExclusive; i++)
            {
                var r = dt.Rows[i];
                var row = new List<string?>(dt.Columns.Count);
                foreach (DataColumn c in dt.Columns)
                {
                    var v = r[c];
                    row.Add(v == null || v == DBNull.Value ? null : v.ToString());
                }
                table.Rows.Add(row);
            }

            var resp = new PagedResponse<ReportTable>
            {
                Items = new List<ReportTable> { table },
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = totalPages
            };

            return Ok(ApiResult<PagedResponse<ReportTable>>.Ok(resp));
        }

        [HttpPost("export")]
        public async Task<IActionResult> Export([FromBody] RaporExportRequest request)
        {
            if (!_authorizationService.Can(PageName, "Export")) return Forbid();

            try
            {
                if (request.Params == null) request.Params = new Dictionary<string, object>();
                if (string.IsNullOrWhiteSpace(request.ProcedureName))
                    return BadRequest(ApiResult.Failure("Rapor seçiniz."));

                int firmaId = _sessionContext.AktifFirmaId ?? 0;
                if (firmaId == 0) return BadRequest(ApiResult.Failure("Firma bilgisi bulunamadı."));

                bool isAdmin = _sessionContext.IsAdmin();
                List<FirmaIsyeriYetkiDTO>? yetkiler = null;
                if (_sessionContext.AktifKullaniciId.HasValue)
                    yetkiler = _yetkiSvc.GetYetkiler(_sessionContext.AktifKullaniciId.Value);

                if (!FirmaIsyeriYetkiHelper.IsFirmaAuthorized(firmaId, yetkiler, isAdmin))
                    return Forbid();

                if (!request.Params.ContainsKey("@FirmaIdList"))
                    request.Params["@FirmaIdList"] = firmaId.ToString();

                if (!request.Params.ContainsKey("@IsyeriIdList"))
                {
                    var (isyeriIdCsv, status) = BuildRaporIsyeriIdListCsv(
                        firmaId,
                        request.IsyeriIds,
                        yetkiler,
                        isAdmin);

                    if (status == FirmaIsyeriYetkiHelper.RaporIsyeriListStatus.UnauthorizedSelection)
                        return BadRequest(ApiResult.Failure("Seçilen işyerlerden bazıları için yetkiniz yok."));
                    if (status == FirmaIsyeriYetkiHelper.RaporIsyeriListStatus.NoAccess)
                        return Forbid();

                    request.Params["@IsyeriIdList"] = isyeriIdCsv ?? "";
                }

                if (!request.Params.ContainsKey("@TarihBaslangic") || !request.Params.ContainsKey("@TarihBitis"))
                    return BadRequest(ApiResult.Failure("Tarih parametreleri eksik."));

                if (TryGetDateParam(request.Params, "@TarihBaslangic", out var tb))
                    request.Params["@TarihBaslangic"] = RaporTarihHelper.ToReportRangeStart(tb);
                if (TryGetDateParam(request.Params, "@TarihBitis", out var te))
                    request.Params["@TarihBitis"] = RaporTarihHelper.ToReportRangeEnd(te);

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

        private (string? csv, FirmaIsyeriYetkiHelper.RaporIsyeriListStatus status) BuildRaporIsyeriIdListCsv(
            int firmaId,
            IReadOnlyList<int>? selectedIsyeriIds,
            List<FirmaIsyeriYetkiDTO>? yetkiler,
            bool isAdmin)
        {
            var firmaIsyeriIds = _kullaniciQueryService.GetFirmayaAitIsyeriIdleri(firmaId) ?? new List<int>();
            var maxCsv = _yetkiSvc.BuildIsyeriIdListCsv(firmaId, yetkiler, isAdmin, firmaIsyeriIds);
            return FirmaIsyeriYetkiHelper.ResolveRaporIsyeriIdListCsv(
                firmaId,
                selectedIsyeriIds,
                maxCsv,
                yetkiler,
                isAdmin);
        }

        private static bool TryGetDateParam(Dictionary<string, object> p, string key, out DateTime dt)
        {
            dt = default;
            if (!p.TryGetValue(key, out var raw) || raw == null) return false;

            if (raw is DateTime d) { dt = d; return true; }

            if (raw is JsonElement je)
            {
                if (je.ValueKind == JsonValueKind.String)
                {
                    var s = je.GetString();
                    return TryParseDateTimeLoose(s, out dt);
                }
                if (je.ValueKind == JsonValueKind.Number && je.TryGetInt64(out var l))
                {
                    try { dt = DateTimeOffset.FromUnixTimeMilliseconds(l).DateTime; return true; } catch { return false; }
                }
                return false;
            }

            return TryParseDateTimeLoose(raw.ToString(), out dt);
        }

        private static bool TryParseDateTimeLoose(string? s, out DateTime dt)
        {
            dt = default;
            if (string.IsNullOrWhiteSpace(s)) return false;
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dt)) return true;
            return DateTime.TryParse(s, CultureInfo.GetCultureInfo("tr-TR"), DateTimeStyles.AssumeLocal, out dt);
        }
    }

    public class RaporExportRequest
    {
        public string ProcedureName { get; set; } = string.Empty;
        public string ExportTitle { get; set; } = "Rapor";
        public string Format { get; set; } = "pdf";
        public List<int>? IsyeriIds { get; set; }
        public Dictionary<string, object> Params { get; set; } = new();
    }
}
