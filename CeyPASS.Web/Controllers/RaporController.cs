using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;

namespace CeyPASS.Web.Controllers
{
    public class RaporController : Controller
    {
        private readonly IRaporService _raporService;
        private readonly IKullaniciQueryService _kullaniciQueryService;
        private readonly IKullaniciFirmaIsyeriYetkiService _yetkiSvc;
        private readonly IKisiEkraniLookUpService _lookupService;
        private readonly ICihazService _cihazService;
        private readonly IFirmaService _firmaService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMemoryCache _cache;
        private const string PageName = "Raporlar";
        private const int DefaultPageSize = 100;
        private static readonly int[] AllowedPageSizes = new[] { 50, 100, 200, 500 };

        public RaporController(
            IRaporService raporService,
            IKullaniciQueryService kullaniciQueryService,
            IKullaniciFirmaIsyeriYetkiService yetkiSvc,
            IKisiEkraniLookUpService lookupService,
            ICihazService cihazService,
            IFirmaService firmaService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService,
            IMemoryCache cache)
        {
            _raporService = raporService;
            _kullaniciQueryService = kullaniciQueryService;
            _yetkiSvc = yetkiSvc;
            _lookupService = lookupService;
            _cihazService = cihazService;
            _firmaService = firmaService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
            _cache = cache;
        }

        public IActionResult Index(string? procedureAdi = null, DateTime? tarihBaslangic = null, DateTime? tarihBitis = null, int? firmaId = null, string? isyeriIds = null, string? cihazIds = null, int page = 1, int pageSize = DefaultPageSize)
        {
            if (!_authorizationService.ViewAbility(PageName))
            {
                TempData["Error"] = "Raporlar ekranını görüntüleme yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            if (page < 1) page = 1;
            if (!AllowedPageSizes.Contains(pageSize)) pageSize = DefaultPageSize;

            DateTime baslangicTarih = RaporTarihHelper.ToReportRangeStart(
                tarihBaslangic ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));
            DateTime bitisTarih = RaporTarihHelper.ToReportRangeEnd(tarihBitis ?? DateTime.Today);

            var raporlar = _raporService.GetirRaporlar();
            var selectedRapor = raporlar.FirstOrDefault(r => r.ProcedureAdi == procedureAdi);
            var spParams = string.IsNullOrWhiteSpace(procedureAdi)
                ? Array.Empty<string>()
                : _raporService.GetProcedureParameterNames(procedureAdi);
            var multiKind = RaporParametreHelper.GetMultiSelect(spParams);

            bool isAdmin = _sessionContext.IsAdmin();
            List<FirmaIsyeriYetkiDTO>? yetkiler = null;
            if (_sessionContext.AktifKullaniciId.HasValue)
                yetkiler = _yetkiSvc.GetYetkiler((int)_sessionContext.AktifKullaniciId);

            var firmalar = FirmaIsyeriYetkiHelper.FilterFirmalar(_firmaService.GetAll(), yetkiler, isAdmin)
                .OrderBy(f => f.FirmaAdi)
                .Select(f => new LookupItem { Id = f.FirmaId, Ad = f.FirmaAdi ?? $"Firma {f.FirmaId}" })
                .ToList();
            if (isAdmin)
                firmalar.Insert(0, new LookupItem { Id = 0, Ad = "TÜMÜ" });

            int selectedFirmaId = firmaId ?? (_sessionContext.AktifFirmaId ?? 0);
            if (selectedFirmaId == 0 && !isAdmin)
            {
                selectedFirmaId = firmalar.FirstOrDefault(f => f.Id > 0)?.Id
                    ?? (_sessionContext.AktifFirmaId ?? 0);
            }
            else if (firmaId == null && selectedFirmaId > 0 && firmalar.All(f => f.Id != selectedFirmaId))
            {
                selectedFirmaId = firmalar.FirstOrDefault(f => f.Id > 0)?.Id ?? 0;
            }

            var selectedIsyeriIdList = FirmaIsyeriYetkiHelper.ParseIsyeriIds(isyeriIds);
            var selectedCihazIdList = FirmaIsyeriYetkiHelper.ParseIsyeriIds(cihazIds);

            DataTable? raporData = null;
            int totalCount = 0;
            if (!string.IsNullOrWhiteSpace(procedureAdi))
            {
                if (selectedFirmaId < 0 || (selectedFirmaId == 0 && !isAdmin))
                {
                    TempData["Error"] = "Firma bilgisi bulunamadı.";
                }
                else if (selectedFirmaId > 0 && !FirmaIsyeriYetkiHelper.IsFirmaAuthorized(selectedFirmaId, yetkiler, isAdmin))
                {
                    TempData["Error"] = "Seçili firma için rapor görüntüleme yetkiniz yok.";
                }
                else
                {
                    try
                    {
                        string isyeriIdCsv = "";
                        string cihazIdCsv = "";
                        var canRun = true;

                        if (selectedFirmaId > 0)
                        {
                            if (multiKind == RaporParametreHelper.MultiSelectKind.Isyeri)
                            {
                                var (csv, status) = BuildRaporIsyeriIdListCsv(
                                    selectedFirmaId, selectedIsyeriIdList, yetkiler, isAdmin);

                                if (status == FirmaIsyeriYetkiHelper.RaporIsyeriListStatus.UnauthorizedSelection)
                                {
                                    TempData["Error"] = "Seçilen işyerlerden bazıları için yetkiniz yok.";
                                    canRun = false;
                                }
                                else if (status == FirmaIsyeriYetkiHelper.RaporIsyeriListStatus.NoAccess)
                                {
                                    TempData["Error"] = "Seçili firma için rapor görüntüleme yetkiniz yok.";
                                    canRun = false;
                                }
                                else
                                    isyeriIdCsv = csv ?? "";
                            }
                            else if (multiKind == RaporParametreHelper.MultiSelectKind.Cihaz)
                            {
                                cihazIdCsv = selectedCihazIdList.Count > 0 ? string.Join(",", selectedCihazIdList) : "";
                            }
                        }

                        if (canRun)
                        {
                            string firmaIdCsv = selectedFirmaId > 0 ? selectedFirmaId.ToString() : "";

                            var parametreler = new Dictionary<string, object>
                            {
                                { RaporParametreHelper.FirmaIdList, firmaIdCsv },
                                { RaporParametreHelper.IsyeriIdList, isyeriIdCsv },
                                { RaporParametreHelper.CihazIdList, cihazIdCsv },
                                { RaporParametreHelper.TarihBaslangic, baslangicTarih },
                                { RaporParametreHelper.TarihBitis, bitisTarih }
                            };

                            var isyeriSeg = string.IsNullOrEmpty(isyeriIdCsv) ? "none" : isyeriIdCsv.Replace(",", "_");
                            var cihazSeg = string.IsNullOrEmpty(cihazIdCsv) ? "none" : cihazIdCsv.Replace(",", "_");
                            var cacheKey = $"rapor_{selectedFirmaId}_{isyeriSeg}_{cihazSeg}_{procedureAdi}_{baslangicTarih:yyyyMMdd}_{bitisTarih:yyyyMMdd}";
                            if (!_cache.TryGetValue(cacheKey, out DataTable cachedDt))
                            {
                                cachedDt = _raporService.CalistirRapor(procedureAdi, parametreler);
                                _cache.Set(cacheKey, cachedDt, TimeSpan.FromMinutes(2));
                            }

                            totalCount = cachedDt?.Rows?.Count ?? 0;
                            raporData = PageDataTable(cachedDt, page, pageSize);

                            HttpContext.Session.SetString("LastRaporData", SerializeDataTable(cachedDt));
                            HttpContext.Session.SetString("LastRaporAdi", selectedRapor?.RaporAdi ?? "Rapor");
                        }
                    }
                    catch (Exception ex)
                    {
                        TempData["Error"] = "Rapor çalıştırılırken hata oluştu: " + ex.Message;
                    }
                }
            }

            ViewBag.Raporlar = raporlar;
            ViewBag.Firmalar = firmalar;
            ViewBag.IsAdmin = isAdmin;
            ViewBag.SelectedProcedureAdi = procedureAdi;
            ViewBag.SelectedRapor = selectedRapor;
            ViewBag.BaslangicTarih = baslangicTarih;
            ViewBag.BitisTarih = bitisTarih;
            ViewBag.SelectedFirmaId = selectedFirmaId;
            ViewBag.IsyeriIdsParam = isyeriIds ?? "";
            ViewBag.SelectedIsyeriIds = selectedIsyeriIdList;
            ViewBag.CihazIdsParam = cihazIds ?? "";
            ViewBag.SelectedCihazIds = selectedCihazIdList;
            ViewBag.MultiSelectKind = selectedFirmaId > 0 ? multiKind : RaporParametreHelper.MultiSelectKind.None;
            ViewBag.Isyerleri = selectedFirmaId > 0 && multiKind == RaporParametreHelper.MultiSelectKind.Isyeri
                ? GetYetkiliIsyeriLookups(selectedFirmaId, yetkiler, isAdmin)
                : new List<LookupItem>();
            ViewBag.Cihazlar = selectedFirmaId > 0 && multiKind == RaporParametreHelper.MultiSelectKind.Cihaz
                ? (_cihazService.GetListe(true, selectedFirmaId) ?? new List<CihazListDTO>())
                : new List<CihazListDTO>();
            ViewBag.CanExport = _authorizationService.Can(PageName, YetkiTipleri.Export);
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalCount > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 1;

            return View(raporData);
        }

        [HttpPost]
        public IActionResult ExportExcel()
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Export))
            {
                return StatusCode(403, new { success = false, message = "Excel export yetkiniz yok." });
            }

            try
            {
                string serializedData = HttpContext.Session.GetString("LastRaporData");
                string raporAdi = HttpContext.Session.GetString("LastRaporAdi") ?? "Rapor";

                if (string.IsNullOrEmpty(serializedData))
                {
                    return StatusCode(400, new { success = false, message = "Export edilecek veri bulunamadı. Lütfen önce bir rapor çalıştırın." });
                }

                DataTable raporData = DeserializeDataTable(serializedData);

                if (raporData == null || raporData.Rows.Count == 0)
                {
                    return StatusCode(400, new { success = false, message = "Export edilecek veri bulunamadı." });
                }

                string fileName = $"{raporAdi}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fileName);

                ExportHelper.ExportToExcel(raporData, tempPath);

                byte[] fileBytes = System.IO.File.ReadAllBytes(tempPath);
                System.IO.File.Delete(tempPath);

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Excel export hatası: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult ExportPdf()
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Export))
            {
                return StatusCode(403, new { success = false, message = "PDF export yetkiniz yok." });
            }

            try
            {
                string serializedData = HttpContext.Session.GetString("LastRaporData");
                string raporAdi = HttpContext.Session.GetString("LastRaporAdi") ?? "Rapor";

                if (string.IsNullOrEmpty(serializedData))
                {
                    return StatusCode(400, new { success = false, message = "Export edilecek veri bulunamadı. Lütfen önce bir rapor çalıştırın." });
                }

                DataTable raporData = DeserializeDataTable(serializedData);

                if (raporData == null || raporData.Rows.Count == 0)
                {
                    return StatusCode(400, new { success = false, message = "Export edilecek veri bulunamadı." });
                }

                string fileName = $"{raporAdi}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fileName);

                ExportHelper.ExportToPdf(raporData, tempPath, raporAdi);

                byte[] fileBytes = System.IO.File.ReadAllBytes(tempPath);
                System.IO.File.Delete(tempPath);

                return File(fileBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "PDF export hatası: " + ex.Message });
            }
        }

        private (string? csv, FirmaIsyeriYetkiHelper.RaporIsyeriListStatus status) BuildRaporIsyeriIdListCsv(
            int firmaId,
            IReadOnlyList<int> selectedIsyeriIds,
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

        private List<LookupItem> GetYetkiliIsyeriLookups(int firmaId, List<FirmaIsyeriYetkiDTO>? yetkiler, bool isAdmin)
        {
            return FirmaIsyeriYetkiHelper.FilterIsyeriLookup(
                _lookupService.GetIsyerleri(firmaId) ?? new List<LookupItem>(),
                firmaId,
                yetkiler,
                isAdmin);
        }

        private string SerializeDataTable(DataTable dt)
        {
            if (dt == null) return string.Empty;
            if (string.IsNullOrEmpty(dt.TableName))
                dt.TableName = "RaporData";
            using (var sw = new System.IO.StringWriter())
            {
                dt.WriteXml(sw, XmlWriteMode.WriteSchema);
                return sw.ToString();
            }
        }

        private DataTable DeserializeDataTable(string xml)
        {
            var dt = new DataTable();
            using (var sr = new System.IO.StringReader(xml))
            {
                dt.ReadXml(sr);
            }
            return dt;
        }

        private static DataTable? PageDataTable(DataTable? dt, int page, int pageSize)
        {
            if (dt == null) return null;
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 100;

            var start = (page - 1) * pageSize;
            if (start >= dt.Rows.Count) start = Math.Max(0, (dt.Rows.Count - 1) / pageSize * pageSize);
            var endExclusive = Math.Min(dt.Rows.Count, start + pageSize);

            var paged = dt.Clone();
            for (int i = start; i < endExclusive; i++)
            {
                paged.ImportRow(dt.Rows[i]);
            }
            paged.TableName = dt.TableName;
            return paged;
        }
    }
}
