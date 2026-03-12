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
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMemoryCache _cache;
        private const string PageName = "Raporlar";
        private const int DefaultPageSize = 100;
        private static readonly int[] AllowedPageSizes = new[] { 50, 100, 200, 500 };

        public RaporController(
            IRaporService raporService,
            IKullaniciQueryService kullaniciQueryService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService,
            IMemoryCache cache)
        {
            _raporService = raporService;
            _kullaniciQueryService = kullaniciQueryService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
            _cache = cache;
        }

        public IActionResult Index(string? procedureAdi = null, DateTime? tarihBaslangic = null, DateTime? tarihBitis = null, int? firmaId = null, int page = 1, int pageSize = DefaultPageSize)
        {
            // Check authorization
            if (!_authorizationService.ViewAbility(PageName))
            {
                TempData["Error"] = "Raporlar ekranını görüntüleme yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            if (page < 1) page = 1;
            if (!AllowedPageSizes.Contains(pageSize)) pageSize = DefaultPageSize;

            // Default tarih aralığı: Bu ay
            DateTime baslangicTarih = tarihBaslangic ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime bitisTarih = tarihBitis ?? DateTime.Today;

            // Rapor listesi
            var raporlar = _raporService.GetirRaporlar();

            // Seçili rapor (procedureAdi'ye göre; session için önce hesaplanmalı)
            var selectedRapor = raporlar.FirstOrDefault(r => r.ProcedureAdi == procedureAdi);

            // Seçili firma
            int selectedFirmaId = firmaId ?? (int)_sessionContext.AktifFirmaId;

            // Rapor verisi (eğer rapor seçilmişse)
            DataTable? raporData = null;
            int totalCount = 0;
            if (!string.IsNullOrWhiteSpace(procedureAdi))
            {
                try
                {
                    var isyeriIdList = _kullaniciQueryService.GetFirmayaAitIsyeriIdleri(selectedFirmaId);
                    string firmaIdCsv = selectedFirmaId > 0 ? selectedFirmaId.ToString() : "";
                    string isyeriIdCsv = (isyeriIdList != null && isyeriIdList.Count > 0) ? string.Join(",", isyeriIdList) : "";

                    var parametreler = new Dictionary<string, object>
                    {
                        { "@FirmaIdList", firmaIdCsv },
                        { "@IsyeriIdList", isyeriIdCsv },
                        { "@TarihBaslangic", baslangicTarih },
                        { "@TarihBitis", bitisTarih } 
                    };

                    var cacheKey = $"rapor_{selectedFirmaId}_{procedureAdi}_{baslangicTarih:yyyyMMdd}_{bitisTarih:yyyyMMdd}";
                    if (!_cache.TryGetValue(cacheKey, out DataTable cachedDt))
                    {
                        cachedDt = _raporService.CalistirRapor(procedureAdi, parametreler);
                        _cache.Set(cacheKey, cachedDt, TimeSpan.FromMinutes(2));
                    }

                    totalCount = cachedDt?.Rows?.Count ?? 0;
                    raporData = PageDataTable(cachedDt, page, pageSize);
                    
                    // Rapor verisini session'da sakla (export için)
                    HttpContext.Session.SetString("LastRaporData", SerializeDataTable(cachedDt));
                    HttpContext.Session.SetString("LastRaporAdi", selectedRapor?.RaporAdi ?? "Rapor");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Rapor çalıştırılırken hata oluştu: " + ex.Message;
                }
            }

            ViewBag.Raporlar = raporlar;
            ViewBag.SelectedProcedureAdi = procedureAdi;
            ViewBag.SelectedRapor = selectedRapor;
            ViewBag.BaslangicTarih = baslangicTarih;
            ViewBag.BitisTarih = bitisTarih;
            ViewBag.SelectedFirmaId = selectedFirmaId;
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
                // Session'dan rapor verisini al
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

                // Geçici dosya oluştur
                string fileName = $"{raporAdi}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fileName);

                ExportHelper.ExportToExcel(raporData, tempPath);

                // Dosyayı byte array olarak döndür
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
                // Session'dan rapor verisini al
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

                // Geçici dosya oluştur
                string fileName = $"{raporAdi}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fileName);

                ExportHelper.ExportToPdf(raporData, tempPath, raporAdi);

                // Dosyayı byte array olarak döndür
                byte[] fileBytes = System.IO.File.ReadAllBytes(tempPath);
                System.IO.File.Delete(tempPath);

                return File(fileBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "PDF export hatası: " + ex.Message });
            }
        }

        // Helper methods for DataTable serialization
        private string SerializeDataTable(DataTable dt)
        {
            if (dt == null) return string.Empty;
            // WriteXml için DataTable.TableName zorunlu; atanmamışsa hata verir.
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
