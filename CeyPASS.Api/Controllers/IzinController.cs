using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CeyPASS.Entities.Concrete;
using CeyPASS.Business.Abstractions;
using IAuthorizationService = CeyPASS.Business.Abstractions.IAuthorizationService;
using CeyPASS.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CeyPASS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class IzinController : ControllerBase
    {
        private readonly IKisiIzinService _kisiIzinService;
        private readonly IIzinTalepService _izinTalepService;
        private readonly IIzinTipService _izinTipService;
        private readonly IKisiQueryService _kisiQueryService;
        private readonly IPuantajService _puantajService;
        private readonly IFirmaService _firmaService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        
        private const string PageName = "Izinler";
        private const string TalepPageName = "IzinTalepleri";

        public class PagedResponse<T>
        {
            public List<T> Items { get; set; } = new();
            public int TotalCount { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public int TotalPages { get; set; }
        }

        public IzinController(
            IKisiIzinService kisiIzinService,
            IIzinTalepService izinTalepService,
            IIzinTipService izinTipService,
            IKisiQueryService kisiQueryService,
            IPuantajService puantajService,
            IFirmaService firmaService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService)
        {
            _kisiIzinService = kisiIzinService;
            _izinTalepService = izinTalepService;
            _izinTipService = izinTipService;
            _kisiQueryService = kisiQueryService;
            _puantajService = puantajService;
            _firmaService = firmaService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        public ActionResult<ApiResult<PagedResponse<KisiIzinListRow>>> GetRecords(
            [FromQuery] string? personelId,
            [FromQuery] int? izinTipId,
            [FromQuery] DateTime? baslangic,
            [FromQuery] DateTime? bitis,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();

            int firmaId = _sessionContext.AktifFirmaId ?? 0;
            DateTime start = baslangic ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime end = bitis ?? start.AddMonths(1).AddDays(-1);

            int totalCount;
            var items = _kisiIzinService.GetTumIzinlerPaged(
                firmaId,
                personelId == "ALL" ? null : personelId,
                izinTipId == 0 ? null : izinTipId,
                start,
                end,
                page,
                pageSize,
                out totalCount
            );

            var totalPages = pageSize <= 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize);
            if (totalPages < 1) totalPages = 1;

            var resp = new PagedResponse<KisiIzinListRow>
            {
                Items = items ?? new List<KisiIzinListRow>(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return Ok(ApiResult<PagedResponse<KisiIzinListRow>>.Ok(resp));
        }

        [HttpGet("lookups")]
        public ActionResult<ApiResult<object>> Lookups([FromQuery] int? firmaId = null)
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();

            if (!_sessionContext.AktifKullaniciId.HasValue)
                return Unauthorized(ApiResult.Failure("Oturum bilgisi bulunamadı."));

            var yetkiler = _puantajService.GetKullaniciFirmaIsyeriYetkileri(_sessionContext.AktifKullaniciId.Value);
            var firmaYetkileri = yetkiler.Select(y => y.FirmaId).Distinct().ToHashSet();

            var firmalar = _firmaService.GetPuantajFirmalar();
            if (firmaYetkileri.Count > 0) firmalar = firmalar.Where(f => firmaYetkileri.Contains(f.FirmaId)).ToList();
            firmalar = firmalar.OrderBy(f => f.FirmaAdi).ToList();

            int effectiveFirmaId = firmaId ?? (_sessionContext.AktifFirmaId ?? 0);
            if (effectiveFirmaId == 0 && firmalar.Count > 0) effectiveFirmaId = firmalar[0].FirmaId;
            if (effectiveFirmaId == 0) return BadRequest(ApiResult.Failure("Firma bilgisi bulunamadı."));
            if (firmaYetkileri.Count > 0 && !firmaYetkileri.Contains(effectiveFirmaId))
                effectiveFirmaId = _sessionContext.AktifFirmaId ?? effectiveFirmaId;

            var kisiler = _kisiQueryService.GetAktifKisilerByFirma(effectiveFirmaId);
            var izinTipleri = _izinTipService.GetAktif();

            var aktifFirma = firmalar.FirstOrDefault(f => f.FirmaId == effectiveFirmaId);

            return Ok(ApiResult<object>.Ok(new
            {
                Firmalar = firmalar,
                AktifFirma = aktifFirma == null ? null : new { aktifFirma.FirmaId, aktifFirma.FirmaAdi },
                Kisiler = kisiler,
                IzinTipleri = izinTipleri
            }));
        }

        public sealed class IzinUpsertRequest
        {
            public int? FirmaId { get; set; }
            public string PersonelId { get; set; } = "";
            public int IzinId { get; set; }
            public bool SaatlikIzinMi { get; set; }
            // "yyyy-MM-dd"
            public string BaslangicTarih { get; set; } = "";
            public string BitisTarih { get; set; } = "";
            // "HH:mm" (optional when SaatlikIzinMi=true)
            public string? BaslangicSaat { get; set; }
            public string? BitisSaat { get; set; }
            public string? Aciklama { get; set; }
        }

        [HttpPost]
        public ActionResult<ApiResult> Create([FromBody] IzinUpsertRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create)) return Forbid();
            if (!_sessionContext.AktifKullaniciId.HasValue) return Unauthorized();

            int firmaId = request.FirmaId ?? (_sessionContext.AktifFirmaId ?? 0);
            if (firmaId == 0) return BadRequest(ApiResult.Failure("Firma bilgisi bulunamadı."));
            if (string.IsNullOrWhiteSpace(request.PersonelId)) return BadRequest(ApiResult.Failure("Personel seçiniz."));
            if (request.IzinId <= 0) return BadRequest(ApiResult.Failure("İzin tipi seçiniz."));

            if (!TryParseDateOnly(request.BaslangicTarih, out var basTarih) || !TryParseDateOnly(request.BitisTarih, out var bitTarih))
                return BadRequest(ApiResult.Failure("Tarih formatı hatalı."));

            TimeSpan? basSaat = TryParseTime(request.BaslangicSaat);
            TimeSpan? bitSaat = TryParseTime(request.BitisSaat);

            var validationDto = new IzinKayitValidasyonDTO
            {
                SaatlikIzinMi = request.SaatlikIzinMi,
                PersonelId = request.PersonelId,
                IzinTipId = request.IzinId,
                BaslangicTarihi = basTarih,
                BitisTarihi = bitTarih,
                BaslangicSaati = basSaat,
                BitisSaati = bitSaat
            };

            var validation = _kisiIzinService.ValidateKayit(validationDto);
            if (!validation.IsValid) return BadRequest(ApiResult.Failure(validation.Message ?? "Validasyon hatası."));

            var izin = new KisiIzin
            {
                FirmaId = firmaId,
                PersonelId = request.PersonelId,
                IzinId = request.IzinId,
                SaatlikIzinMi = request.SaatlikIzinMi,
                Aciklama = request.Aciklama ?? "",
                Baslangic = request.SaatlikIzinMi && basSaat.HasValue ? basTarih.Date.Add(basSaat.Value) : basTarih.Date,
                Bitis = request.SaatlikIzinMi && bitSaat.HasValue ? bitTarih.Date.Add(bitSaat.Value) : bitTarih.Date,
                OlusturanKullaniciId = _sessionContext.AktifKullaniciId.Value
            };

            bool ok = _kisiIzinService.Ekle(izin);
            return ok ? Ok(ApiResult.Ok("İzin başarıyla eklendi.")) : BadRequest(ApiResult.Failure("İzin eklenemedi."));
        }

        [HttpPut("{id}")]
        public ActionResult<ApiResult> Update(int id, [FromBody] IzinUpsertRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update)) return Forbid();
            if (!_sessionContext.AktifKullaniciId.HasValue) return Unauthorized();

            var mevcut = _kisiIzinService.GetById(id);
            if (mevcut == null) return NotFound(ApiResult.Failure("İzin bulunamadı."));

            if (request.IzinId <= 0) return BadRequest(ApiResult.Failure("İzin tipi seçiniz."));
            if (!TryParseDateOnly(request.BaslangicTarih, out var basTarih) || !TryParseDateOnly(request.BitisTarih, out var bitTarih))
                return BadRequest(ApiResult.Failure("Tarih formatı hatalı."));

            TimeSpan? basSaat = TryParseTime(request.BaslangicSaat);
            TimeSpan? bitSaat = TryParseTime(request.BitisSaat);

            var validationDto = new IzinKayitValidasyonDTO
            {
                SaatlikIzinMi = request.SaatlikIzinMi,
                PersonelId = mevcut.PersonelId,
                IzinTipId = request.IzinId,
                BaslangicTarihi = basTarih,
                BitisTarihi = bitTarih,
                BaslangicSaati = basSaat,
                BitisSaati = bitSaat
            };

            var validation = _kisiIzinService.ValidateKayit(validationDto);
            if (!validation.IsValid) return BadRequest(ApiResult.Failure(validation.Message ?? "Validasyon hatası."));

            mevcut.IzinId = request.IzinId;
            mevcut.SaatlikIzinMi = request.SaatlikIzinMi;
            mevcut.Aciklama = request.Aciklama ?? "";
            mevcut.Baslangic = request.SaatlikIzinMi && basSaat.HasValue ? basTarih.Date.Add(basSaat.Value) : basTarih.Date;
            mevcut.Bitis = request.SaatlikIzinMi && bitSaat.HasValue ? bitTarih.Date.Add(bitSaat.Value) : bitTarih.Date;

            bool ok = _kisiIzinService.Guncelle(mevcut);
            return ok ? Ok(ApiResult.Ok("İzin başarıyla güncellendi.")) : BadRequest(ApiResult.Failure("İzin güncellenemedi."));
        }

        [HttpDelete("{id}")]
        public ActionResult<ApiResult> Delete(int id)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Delete)) return Forbid();
            bool ok = _kisiIzinService.PasifYap(id);
            return ok ? Ok(ApiResult.Ok("İzin başarıyla silindi.")) : BadRequest(ApiResult.Failure("İzin silinemedi."));
        }

        private static bool TryParseDateOnly(string? value, out DateTime date)
        {
            date = default;
            if (string.IsNullOrWhiteSpace(value)) return false;
            return DateTime.TryParseExact(value.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }

        private static TimeSpan? TryParseTime(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (TimeSpan.TryParseExact(value.Trim(), "hh\\:mm", CultureInfo.InvariantCulture, out var t)) return t;
            if (TimeSpan.TryParse(value.Trim(), CultureInfo.InvariantCulture, out var t2)) return t2;
            return null;
        }

        [HttpGet("talepler")]
        public ActionResult<ApiResult<List<IzinTalepListItem>>> GetTalepler()
        {
            if (!_authorizationService.ViewAbility(TalepPageName)) return Forbid();

            // Sadece IK Bekleyenleri (Web'deki talep listesi mantığı)
            var items = _izinTalepService.IkBekleyenler();
            
            // FirmaId filter: Ensure we only see requests for the current firm if not admin
            if (!_sessionContext.IsAdmin())
            {
                // Note: IzinTalep usually has PersonelId, need to check its Firm
                // For simplicity, assuming the service already filters or we filter here
                items = items.Where(x => {
                    var k = _kisiQueryService.GetKisiDetay(x.PersonelId);
                    return k != null && k.FirmaId == _sessionContext.AktifFirmaId;
                }).ToList();
            }

            var pIds = items.Select(x => x.PersonelId).Distinct().ToList();
            var pNames = new Dictionary<string, string>();
            foreach (var id in pIds)
            {
                var k = _kisiQueryService.GetKisiDetay(id);
                if (k != null) pNames[id] = $"{k.Ad} {k.Soyad}";
            }
            var iTypes = _izinTipService.GetAktif().ToDictionary(x => x.IzinTipId, x => x.Ad);

            var mapped = items.Select(t =>
            {
                var pName = pNames.TryGetValue(t.PersonelId, out var n) ? n : t.PersonelId;
                var iName = (t.IzinTipId.HasValue && iTypes.TryGetValue(t.IzinTipId.Value, out var it)) ? it : t.IzinTipId?.ToString();
                return new IzinTalepListItem
                {
                    TalepId = t.TalepId,
                    PersonelId = t.PersonelId,
                    PersonelAdSoyad = pName,
                    IzinTipId = t.IzinTipId,
                    IzinTipAdi = iName,
                    Baslangic = t.Baslangic,
                    Bitis = t.Bitis,
                    SaatlikIzinMi = t.SaatlikIzinMi,
                    UstYetkiliOnayDurumu = t.UstYetkiliOnayDurumu,
                    IkOnayDurumu = t.IkOnayDurumu,
                    SonucKisiIzinId = t.SonucKisiIzinId,
                    KullanimImzaIstenen = t.KullanimImzaIstenen,
                    KullanimImzaTarihi = t.KullanimImzaTarihi
                };
            }).ToList();

            return Ok(ApiResult<List<IzinTalepListItem>>.Ok(mapped));
        }

        public sealed class TalepActionRequest
        {
            public string? Aciklama { get; set; }
        }

        [HttpPost("onayla/{id}")]
        public ActionResult<ApiResult> Onayla(int id, [FromBody] TalepActionRequest? request)
        {
            if (!_authorizationService.Can(TalepPageName, YetkiTipleri.Update)) return Forbid();
            if (!_sessionContext.AktifKullaniciId.HasValue) return Unauthorized();

            bool ok = _izinTalepService.IkOnayla(id, _sessionContext.AktifKullaniciId.Value, request?.Aciklama);
            return ok ? Ok(ApiResult.Ok("Talep onaylandı.")) : BadRequest(ApiResult.Failure("Talep onaylanamadı."));
        }

        [HttpPost("reddet/{id}")]
        public ActionResult<ApiResult> Reddet(int id, [FromBody] TalepActionRequest? request)
        {
            if (!_authorizationService.Can(TalepPageName, YetkiTipleri.Update)) return Forbid();
            if (!_sessionContext.AktifKullaniciId.HasValue) return Unauthorized();

            bool ok = _izinTalepService.IkReddet(id, _sessionContext.AktifKullaniciId.Value, request?.Aciklama);
            return ok ? Ok(ApiResult.Ok("Talep reddedildi.")) : BadRequest(ApiResult.Failure("Talep reddedilemedi."));
        }

        [HttpPost("donus-imzasina-ac/{id}")]
        public ActionResult<ApiResult> DonusImzasinaAc(int id)
        {
            if (!_authorizationService.Can(TalepPageName, YetkiTipleri.Update)) return Forbid();
            if (!_sessionContext.AktifKullaniciId.HasValue) return Unauthorized();

            bool ok = _izinTalepService.DonusImzasinaAc(id, _sessionContext.AktifKullaniciId.Value);
            return ok ? Ok(ApiResult.Ok("Dönüş imzasına açıldı.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }

        [HttpGet("tipler")]
        public ActionResult<ApiResult<List<IzinTip>>> GetIzinTipleri()
        {
            var items = _izinTipService.GetAktif();
            return Ok(ApiResult<List<IzinTip>>.Ok(items));
        }

        public sealed class IzinTalepListItem
        {
            public int TalepId { get; set; }
            public string PersonelId { get; set; } = "";
            public string PersonelAdSoyad { get; set; } = "";
            public int? IzinTipId { get; set; }
            public string? IzinTipAdi { get; set; }
            public DateTime Baslangic { get; set; }
            public DateTime Bitis { get; set; }
            public bool SaatlikIzinMi { get; set; }
            public IzinOnayDurumu? UstYetkiliOnayDurumu { get; set; }
            public IzinOnayDurumu? IkOnayDurumu { get; set; }
            public int? SonucKisiIzinId { get; set; }
            public bool? KullanimImzaIstenen { get; set; }
            public DateTime? KullanimImzaTarihi { get; set; }
        }
    }
}
