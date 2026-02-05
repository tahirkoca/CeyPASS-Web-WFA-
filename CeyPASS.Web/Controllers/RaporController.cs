using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
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
        private const string PageName = "Raporlar";

        public RaporController(
            IRaporService raporService,
            IKullaniciQueryService kullaniciQueryService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService)
        {
            _raporService = raporService;
            _kullaniciQueryService = kullaniciQueryService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
        }

        public IActionResult Index(string? procedureAdi = null, DateTime? tarihBaslangic = null, DateTime? tarihBitis = null, int? firmaId = null)
        {
            // Check authorization
            if (!_authorizationService.ViewAbility(PageName))
            {
                TempData["Error"] = "Raporlar ekranını görüntüleme yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

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

                    raporData = _raporService.CalistirRapor(procedureAdi, parametreler);
                    
                    // Rapor verisini session'da sakla (export için)
                    HttpContext.Session.SetString("LastRaporData", SerializeDataTable(raporData));
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
    }
}
