using CeyPASS.Business.Abstractions;
using CeyPASS.Business.Services;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using CeyPASS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.Api.Controllers
{
    /// <summary>Canlı izleme misafir/araç kart atamaları ve terminal kart komutları (<c>api/v1/CanliIzleme</c> alt yolu).</summary>
    [Authorize]
    [ApiController]
    [Route("api/v1/CanliIzleme")]
    public class CanliIzlemeKartController : ControllerBase
    {
        private readonly IMisafirKartService _misafirSvc;
        private readonly IAracKartiService _aracSvc;
        private readonly ICanliIzlemeKartKomutService _kartKomutSvc;
        private readonly ISessionContext _sessionContext;

        public CanliIzlemeKartController(
            IMisafirKartService misafirSvc,
            IAracKartiService aracSvc,
            ICanliIzlemeKartKomutService kartKomutSvc,
            ISessionContext sessionContext)
        {
            _misafirSvc = misafirSvc;
            _aracSvc = aracSvc;
            _kartKomutSvc = kartKomutSvc;
            _sessionContext = sessionContext;
        }

        public sealed class KartKomutRequest
        {
            public string? PersonelId { get; set; }
            public bool Pasif { get; set; }
        }

        public sealed class CreateKartRequest
        {
            public string? PersonelId { get; set; }
            public string? AdSoyad { get; set; }
            public DateTime GirisSaati { get; set; } = DateTime.Now;
            public string? Aciklama { get; set; }
            public string? TcKimlikNo { get; set; }
            public string? PasaportNo { get; set; }
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
            public string? PasaportNo { get; set; }
            public string? ZiyaretEdilenKisi { get; set; }
            public string? Plaka { get; set; }
        }

        // Danışma vb. rollerde kart atama ekranı kapalı; mobil ana JWT ile de erişilemez.
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
            if (CanliIzlemeRoleHelper.HideKartAtama(_sessionContext.RolAdi))
            {
                forbidOrBad = Forbid();
                return false;
            }
            firmaId = _sessionContext.AktifFirmaId.Value;
            return true;
        }

        private static object MapAtama(PuantajsizKartAtama a) => new
        {
            atamaId = a.AtamaId,
            kartId = a.KartId,
            adSoyad = a.MisafirAdSoyad,
            tcKimlikNo = a.TCKimlikNo,
            pasaportNo = a.PasaportNo,
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
            pasaportNo = x.PasaportNo,
            ziyaretEdilenKisi = x.ZiyaretEdilenKisi,
            plaka = x.Plaka,
            notlar = x.Notlar,
            sonZiyaret = x.SonZiyaret,
            gosterim = x.Gosterim
        };

        // ─── Misafir ─────────────────────────────────────────────────────────

        /// <summary>Yeni misafir ataması için boşta kart listesi.</summary>
        [HttpGet("misafir-kart/kartlar")]
        public ActionResult<ApiResult<List<object>>> MisafirKartlar()
        {
            if (!EnsureCanliIzlemeAuth(out var firmaId, out var err)) return err!;
            var list = _misafirSvc.GetCardsForNew(firmaId)
                .Select(c => (object)new { personelId = c.PersonelId, adSoyad = c.AdSoyad })
                .ToList();
            return Ok(ApiResult<List<object>>.Ok(list));
        }

        /// <summary>Açık misafir kart atamaları.</summary>
        [HttpGet("misafir-kart/aktif")]
        public ActionResult<ApiResult<List<object>>> MisafirAktif()
        {
            if (!EnsureCanliIzlemeAuth(out var firmaId, out var err)) return err!;
            var list = _misafirSvc.GetOpenActiveAssignments(firmaId)
                .Select(MapAtama)
                .ToList();
            return Ok(ApiResult<List<object>>.Ok(list));
        }

        /// <summary>Yeni misafir kart ataması oluşturur.</summary>
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
                    req.ZiyaretEdilenKisi ?? "",
                    req.PasaportNo);
                return Ok(ApiResult<object>.Ok(new { atamaId = id }, "Kayıt başarıyla oluşturuldu."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Failure(ex.Message));
            }
        }

        /// <summary>Mevcut misafir atamasını günceller veya çıkış saati işler.</summary>
        [HttpPut("misafir-kart/{id:int}")]
        public ActionResult<ApiResult<object>> MisafirUpdate(int id, [FromBody] UpdateKartRequest req)
        {
            if (!EnsureCanliIzlemeAuth(out _, out var err)) return err!;
            try
            {
                var kisitKaldirildi = _misafirSvc.UpdateAssignment(
                    id,
                    req.AdSoyad ?? "",
                    req.GirisSaati,
                    req.CikisSaati,
                    req.Aciklama ?? "",
                    req.TcKimlikNo ?? "",
                    req.ZiyaretEdilenKisi ?? "",
                    req.PasaportNo,
                    _sessionContext.AktifKullaniciId);
                return Ok(ApiResult<object>.Ok(new { kisitKaldirildi }, KartAtamaMesajlari.Guncellendi(kisitKaldirildi)));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Failure(ex.Message));
            }
        }

        /// <summary>TC ile önceki misafir kaydından otomatik doldurma.</summary>
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
                pasaportNo = rec.PasaportNo,
                ziyaretEdilenKisi = rec.ZiyaretEdilenKisi,
                aciklama = rec.Notlar
            }));
        }

        /// <summary>Geçmiş misafir ziyaret araması.</summary>
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

        /// <summary>Yeni araç ataması için kullanılabilir kartlar.</summary>
        [HttpGet("arac-kart/kartlar")]
        public ActionResult<ApiResult<List<object>>> AracKartlar()
        {
            if (!EnsureCanliIzlemeAuth(out var firmaId, out var err)) return err!;
            var list = _aracSvc.GetCardsForNew(firmaId)
                .Select(c => (object)new { personelId = c.PersonelId, adSoyad = c.AdSoyad })
                .ToList();
            return Ok(ApiResult<List<object>>.Ok(list));
        }

        /// <summary>Açık araç kart atamaları.</summary>
        [HttpGet("arac-kart/aktif")]
        public ActionResult<ApiResult<List<object>>> AracAktif()
        {
            if (!EnsureCanliIzlemeAuth(out var firmaId, out var err)) return err!;
            var list = _aracSvc.GetOpenActiveAssignments(firmaId)
                .Select(MapAtama)
                .ToList();
            return Ok(ApiResult<List<object>>.Ok(list));
        }

        /// <summary>Yeni araç kart ataması.</summary>
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
                    req.Plaka ?? "",
                    req.PasaportNo);
                return Ok(ApiResult<object>.Ok(new { atamaId = id }, "Kayıt başarıyla oluşturuldu."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Failure(ex.Message));
            }
        }

        /// <summary>Araç atamasını günceller.</summary>
        [HttpPut("arac-kart/{id:int}")]
        public ActionResult<ApiResult<object>> AracUpdate(int id, [FromBody] UpdateKartRequest req)
        {
            if (!EnsureCanliIzlemeAuth(out _, out var err)) return err!;
            try
            {
                var kisitKaldirildi = _aracSvc.UpdateAssignment(
                    id,
                    req.AdSoyad ?? "",
                    req.GirisSaati,
                    req.CikisSaati,
                    req.Aciklama ?? "",
                    req.TcKimlikNo ?? "",
                    req.ZiyaretEdilenKisi ?? "",
                    req.Plaka ?? "",
                    req.PasaportNo,
                    _sessionContext.AktifKullaniciId);
                return Ok(ApiResult<object>.Ok(new { kisitKaldirildi }, KartAtamaMesajlari.Guncellendi(kisitKaldirildi)));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Failure(ex.Message));
            }
        }

        /// <summary>TC ile araç ziyaretçi bilgisi ön doldurma.</summary>
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
                pasaportNo = rec.PasaportNo,
                ziyaretEdilenKisi = rec.ZiyaretEdilenKisi,
                plaka = rec.Plaka,
                aciklama = rec.Notlar
            }));
        }

        /// <summary>Misafir/araç birleşik atama listesi; komut kuyruk durumu satırlara eklenir.</summary>
        /// <param name="tip">tumu | misafir | arac filtre anahtarı.</param>
        [HttpGet("atama-liste")]
        public ActionResult<ApiResult<List<KartAtamaListeSatir>>> AtamaListe([FromQuery] string tip = "tumu")
        {
            if (!EnsureCanliIzlemeAuth(out var firmaId, out var err)) return err!;
            var list = KartAtamaListePresenter.Load(_misafirSvc, _aracSvc, _kartKomutSvc, firmaId, tip);
            return Ok(ApiResult<List<KartAtamaListeSatir>>.Ok(list));
        }

        /// <summary>Turnike/cihaz için kart kısıtlama veya kısıt kaldırma komutunu kuyruğa alır.</summary>
        [HttpPost("kart-komut")]
        public ActionResult<ApiResult<object>> KartKomut([FromBody] KartKomutRequest req)
        {
            if (!EnsureCanliIzlemeAuth(out var firmaId, out var err)) return err!;
            var pid = (req?.PersonelId ?? "").Trim();
            if (string.IsNullOrEmpty(pid))
                return BadRequest(ApiResult.Failure("PersonelId gerekli."));
            try
            {
                // Komut anında cihaza gitmez; CanliIzlemeKartKomut kuyruğu terminal senkronu ile işler.
                if (req!.Pasif)
                    _kartKomutSvc.EnqueuePasif(firmaId, pid, _sessionContext.AktifKullaniciId);
                else
                    _kartKomutSvc.EnqueueAktif(firmaId, pid, _sessionContext.AktifKullaniciId);
                return Ok(ApiResult<object>.Ok(new { }, req.Pasif
                    ? "Kısıtlama komutu kuyruğa alındı."
                    : "Kısıt kaldırma komutu kuyruğa alındı."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Failure(ex.Message));
            }
        }

        /// <summary>Geçmiş araç ziyaret araması.</summary>
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
