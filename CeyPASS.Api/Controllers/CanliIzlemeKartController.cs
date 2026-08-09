using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/CanliIzleme")]
    public class CanliIzlemeKartController : ControllerBase
    {
        private readonly IMisafirKartService _misafirSvc;
        private readonly IAracKartiService _aracSvc;
        private readonly ISessionContext _sessionContext;

        public CanliIzlemeKartController(
            IMisafirKartService misafirSvc,
            IAracKartiService aracSvc,
            ISessionContext sessionContext)
        {
            _misafirSvc = misafirSvc;
            _aracSvc = aracSvc;
            _sessionContext = sessionContext;
        }

        public sealed class CreateKartRequest
        {
            public string? PersonelId { get; set; }
            public string? AdSoyad { get; set; }
            public DateTime GirisSaati { get; set; } = DateTime.Now;
            public string? Aciklama { get; set; }
            public string? TcKimlikNo { get; set; }
            public string? ZiyaretEdilenKisi { get; set; }
            public string? Plaka { get; set; }
        }

        public sealed class UpdateKartRequest
        {
            public string? AdSoyad { get; set; }
            public DateTime GirisSaati { get; set; }
            public DateTime? CikisSaati { get; set; }
            public string? Aciklama { get; set; }
            public string? TcKimlikNo { get; set; }
            public string? ZiyaretEdilenKisi { get; set; }
            public string? Plaka { get; set; }
        }

        private bool EnsureCanliIzlemeAuth(out int firmaId, out ActionResult? forbidOrBad)
        {
            firmaId = 0;
            forbidOrBad = null;
            var authKind = User?.FindFirst("AuthKind")?.Value;
            if (!_sessionContext.IsAdmin() && authKind != "CanliIzleme")
            {
                forbidOrBad = Forbid();
                return false;
            }
            if (!_sessionContext.AktifFirmaId.HasValue)
            {
                forbidOrBad = BadRequest(ApiResult.Failure("Firma bilgisi bulunamadı."));
                return false;
            }
            if (IsYemekhaneRole(_sessionContext.RolAdi) && !IsDanismaRole(_sessionContext.RolAdi))
            {
                forbidOrBad = Forbid();
                return false;
            }
            firmaId = _sessionContext.AktifFirmaId.Value;
            return true;
        }

        private static bool IsYemekhaneRole(string? rolAdi) =>
            string.Equals(rolAdi ?? string.Empty, "YEMEKHANE", StringComparison.OrdinalIgnoreCase);

        private static bool IsDanismaRole(string? rolAdi)
        {
            var r = rolAdi ?? "";
            return r.IndexOf("DANIŞMA", StringComparison.OrdinalIgnoreCase) >= 0
                   || r.IndexOf("DANISMA", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static object MapAtama(PuantajsizKartAtama a) => new
        {
            atamaId = a.AtamaId,
            kartId = a.KartId,
            adSoyad = a.MisafirAdSoyad,
            tcKimlikNo = a.TCKimlikNo,
            ziyaretEdilenKisi = a.ZiyaretEdilenKisi,
            plaka = a.Plaka,
            kartAdi = a.KartAdi,
            baslangic = a.Baslangic,
            bitis = a.Bitis,
            notlar = a.Notlar
        };

        private static object MapGecmis(GecmisZiyaretciItem x) => new
        {
            adSoyad = x.AdSoyad,
            tcKimlikNo = x.TCKimlikNo,
            ziyaretEdilenKisi = x.ZiyaretEdilenKisi,
            plaka = x.Plaka,
            sonZiyaret = x.SonZiyaret
        };

        // ─── Misafir ─────────────────────────────────────────────────────────

        [HttpGet("misafir-kart/kartlar")]
        public ActionResult<ApiResult<List<object>>> MisafirKartlar()
        {
            if (!EnsureCanliIzlemeAuth(out var firmaId, out var err)) return err!;
            var list = _misafirSvc.GetCardsForNew(firmaId)
                .Select(c => (object)new { personelId = c.PersonelId, adSoyad = c.AdSoyad })
                .ToList();
            return Ok(ApiResult<List<object>>.Ok(list));
        }

        [HttpGet("misafir-kart/aktif")]
        public ActionResult<ApiResult<List<object>>> MisafirAktif()
        {
            if (!EnsureCanliIzlemeAuth(out var firmaId, out var err)) return err!;
            var list = _misafirSvc.GetTodayActiveAssignments(DateTime.Now, firmaId)
                .Select(MapAtama)
                .ToList();
            return Ok(ApiResult<List<object>>.Ok(list));
        }

        [HttpPost("misafir-kart")]
        public ActionResult<ApiResult<object>> MisafirCreate([FromBody] CreateKartRequest req)
        {
            if (!EnsureCanliIzlemeAuth(out var firmaId, out var err)) return err!;
            try
            {
                var id = _misafirSvc.CreateAssignment(
                    firmaId,
                    req.PersonelId ?? "",
                    req.AdSoyad ?? "",
                    req.GirisSaati,
                    req.Aciklama ?? "",
                    req.TcKimlikNo ?? "",
                    req.ZiyaretEdilenKisi ?? "");
                return Ok(ApiResult<object>.Ok(new { atamaId = id }, "Kayıt başarıyla oluşturuldu."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Failure(ex.Message));
            }
        }

        [HttpPut("misafir-kart/{id:int}")]
        public ActionResult<ApiResult<object>> MisafirUpdate(int id, [FromBody] UpdateKartRequest req)
        {
            if (!EnsureCanliIzlemeAuth(out _, out var err)) return err!;
            try
            {
                _misafirSvc.UpdateAssignment(
                    id,
                    req.AdSoyad ?? "",
                    req.GirisSaati,
                    req.CikisSaati,
                    req.Aciklama ?? "",
                    req.TcKimlikNo ?? "",
                    req.ZiyaretEdilenKisi ?? "");
                return Ok(ApiResult<object>.Ok(new { }, "Kayıt güncellendi."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Failure(ex.Message));
            }
        }

        [HttpGet("misafir-kart/by-tc")]
        public ActionResult<ApiResult<object>> MisafirByTc([FromQuery] string tc)
        {
            if (!EnsureCanliIzlemeAuth(out _, out var err)) return err!;
            if (string.IsNullOrWhiteSpace(tc))
                return BadRequest(ApiResult.Failure("T.C. kimlik numarası boş olamaz."));

            var rec = _misafirSvc.GetMisafirBilgisiByTc(tc);
            if (rec == null)
                return Ok(ApiResult<object>.Failure("Kayıt bulunamadı.", 404));

            return Ok(ApiResult<object>.Ok(new
            {
                adSoyad = rec.MisafirAdSoyad,
                tcKimlikNo = rec.TCKimlikNo,
                ziyaretEdilenKisi = rec.ZiyaretEdilenKisi,
                aciklama = rec.Notlar
            }));
        }

        [HttpGet("misafir-kart/gecmis")]
        public ActionResult<ApiResult<List<object>>> MisafirGecmis([FromQuery] string? ad = null)
        {
            if (!EnsureCanliIzlemeAuth(out var firmaId, out var err)) return err!;
            var list = _misafirSvc.SearchGecmisZiyaretciler(firmaId, ad ?? "")
                .Select(MapGecmis)
                .ToList();
            return Ok(ApiResult<List<object>>.Ok(list));
        }

        // ─── Araç ────────────────────────────────────────────────────────────

        [HttpGet("arac-kart/kartlar")]
        public ActionResult<ApiResult<List<object>>> AracKartlar()
        {
            if (!EnsureCanliIzlemeAuth(out var firmaId, out var err)) return err!;
            var list = _aracSvc.GetCardsForNew(firmaId)
                .Select(c => (object)new { personelId = c.PersonelId, adSoyad = c.AdSoyad })
                .ToList();
            return Ok(ApiResult<List<object>>.Ok(list));
        }

        [HttpGet("arac-kart/aktif")]
        public ActionResult<ApiResult<List<object>>> AracAktif()
        {
            if (!EnsureCanliIzlemeAuth(out var firmaId, out var err)) return err!;
            var list = _aracSvc.GetTodayActiveAssignments(DateTime.Now, firmaId)
                .Select(MapAtama)
                .ToList();
            return Ok(ApiResult<List<object>>.Ok(list));
        }

        [HttpPost("arac-kart")]
        public ActionResult<ApiResult<object>> AracCreate([FromBody] CreateKartRequest req)
        {
            if (!EnsureCanliIzlemeAuth(out var firmaId, out var err)) return err!;
            try
            {
                var id = _aracSvc.CreateAssignment(
                    firmaId,
                    req.PersonelId ?? "",
                    req.AdSoyad ?? "",
                    req.GirisSaati,
                    req.Aciklama ?? "",
                    req.TcKimlikNo ?? "",
                    req.ZiyaretEdilenKisi ?? "",
                    req.Plaka ?? "");
                return Ok(ApiResult<object>.Ok(new { atamaId = id }, "Kayıt başarıyla oluşturuldu."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Failure(ex.Message));
            }
        }

        [HttpPut("arac-kart/{id:int}")]
        public ActionResult<ApiResult<object>> AracUpdate(int id, [FromBody] UpdateKartRequest req)
        {
            if (!EnsureCanliIzlemeAuth(out _, out var err)) return err!;
            try
            {
                _aracSvc.UpdateAssignment(
                    id,
                    req.AdSoyad ?? "",
                    req.GirisSaati,
                    req.CikisSaati,
                    req.Aciklama ?? "",
                    req.TcKimlikNo ?? "",
                    req.ZiyaretEdilenKisi ?? "",
                    req.Plaka ?? "");
                return Ok(ApiResult<object>.Ok(new { }, "Kayıt güncellendi."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Failure(ex.Message));
            }
        }

        [HttpGet("arac-kart/by-tc")]
        public ActionResult<ApiResult<object>> AracByTc([FromQuery] string tc)
        {
            if (!EnsureCanliIzlemeAuth(out _, out var err)) return err!;
            if (string.IsNullOrWhiteSpace(tc))
                return BadRequest(ApiResult.Failure("T.C. kimlik numarası boş olamaz."));

            var rec = _aracSvc.GetBilgisiByTc(tc);
            if (rec == null)
                return Ok(ApiResult<object>.Failure("Kayıt bulunamadı.", 404));

            return Ok(ApiResult<object>.Ok(new
            {
                adSoyad = rec.MisafirAdSoyad,
                tcKimlikNo = rec.TCKimlikNo,
                ziyaretEdilenKisi = rec.ZiyaretEdilenKisi,
                plaka = rec.Plaka,
                aciklama = rec.Notlar
            }));
        }

        [HttpGet("arac-kart/gecmis")]
        public ActionResult<ApiResult<List<object>>> AracGecmis([FromQuery] string? ad = null)
        {
            if (!EnsureCanliIzlemeAuth(out var firmaId, out var err)) return err!;
            var list = _aracSvc.SearchGecmisZiyaretciler(firmaId, ad ?? "")
                .Select(MapGecmis)
                .ToList();
            return Ok(ApiResult<List<object>>.Ok(list));
        }
    }
}
