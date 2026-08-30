using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CeyPASS.Entities.Concrete;
using CeyPASS.Business.Abstractions;
using IAuthorizationService = CeyPASS.Business.Abstractions.IAuthorizationService;
using CeyPASS.Models;
using CeyPASS.Infrastructure.Helpers;
using System;
using System.Linq;
using System.Collections.Generic;

namespace CeyPASS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class KisiHareketController : ControllerBase
    {
        private readonly IKisiHareketService _kisiHareketService;
        private readonly IKisiQueryService _kisiQueryService;
        private readonly IKisiEkraniLookUpService _lookupService;
        private readonly IPuantajService _puantajService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly IFirmaService _firmaService;
        private const string PageName = "KisiHareketler";

        public class PagedResponse<T>
        {
            public List<T> Items { get; set; } = new();
            public int TotalCount { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public int TotalPages { get; set; }
        }

        public sealed class PersonelLookupItem
        {
            public int Id { get; set; }
            public string Ad { get; set; } = string.Empty;
        }

        public KisiHareketController(
            IKisiHareketService kisiHareketService,
            IKisiQueryService kisiQueryService,
            IKisiEkraniLookUpService lookupService,
            IPuantajService puantajService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService,
            IFirmaService firmaService)
        {
            _kisiHareketService = kisiHareketService;
            _kisiQueryService = kisiQueryService;
            _lookupService = lookupService;
            _puantajService = puantajService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
            _firmaService = firmaService;
        }

        [HttpGet]
        public ActionResult<ApiResult<PagedResponse<KisiHareketListRow>>> Get(
            [FromQuery] int? firmaId,
            [FromQuery] string? personelIds,
            [FromQuery] DateTime? baslangic,
            [FromQuery] DateTime? bitis,
            [FromQuery] bool? sadeceAktif,
            [FromQuery] bool? sadecePasif,
            [FromQuery] bool? sadeceYemekhane,
            [FromQuery] string? kartTipi,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();

            int effectiveFirmaId = _sessionContext.AktifFirmaId ?? 0;
            if (_sessionContext.IsAdmin() && firmaId.HasValue && firmaId.Value > 0)
                effectiveFirmaId = firmaId.Value;
            if (effectiveFirmaId == 0) return BadRequest(ApiResult.Failure("Firma bilgisi bulunamadı."));

            List<int> pIds = new List<int>();
            if (!string.IsNullOrWhiteSpace(personelIds))
            {
                pIds = personelIds
                    .Split(',')
                    .Select(x => x.Trim())
                    .Where(x => int.TryParse(x, out _))
                    .Select(int.Parse)
                    .ToList();
            }
            else if (!_sessionContext.IsAdmin() && !string.IsNullOrEmpty(_sessionContext.AktifSicilNo))
            {
                // Normal kullanıcı ise sadece kendi hareketlerini görsün
                if (int.TryParse(_sessionContext.AktifSicilNo, out var myId)) pIds.Add(myId);
            }

            DateTime start = baslangic ?? DateTime.Today;
            DateTime end = bitis ?? DateTime.Today.AddDays(1).AddMinutes(-1);

            int totalCount;
            var items = _kisiHareketService.GetByPersonsPaged(
                pIds,
                start,
                end,
                sadeceAktif ?? false,
                sadecePasif ?? false,
                sadeceYemekhane ?? false,
                effectiveFirmaId,
                page,
                pageSize,
                out totalCount);

            var totalPages = pageSize <= 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize);
            if (totalPages < 1) totalPages = 1;

            var resp = new PagedResponse<KisiHareketListRow>
            {
                Items = items ?? new List<KisiHareketListRow>(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return Ok(ApiResult<PagedResponse<KisiHareketListRow>>.Ok(resp));
        }

        [HttpGet("lookups")]
        public ActionResult<ApiResult<object>> Lookups([FromQuery] int? firmaId, [FromQuery] string? kartTipi, [FromQuery] int? isyeriId = null)
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();

            int effectiveFirmaId = _sessionContext.AktifFirmaId ?? 0;
            if (_sessionContext.IsAdmin() && firmaId.HasValue && firmaId.Value > 0)
                effectiveFirmaId = firmaId.Value;
            if (effectiveFirmaId == 0) return BadRequest(ApiResult.Failure("Firma bilgisi bulunamadı."));

            bool isAdmin = _sessionContext.IsAdmin();
            List<FirmaIsyeriYetkiDTO> yetkiler = null;
            if (!isAdmin && _sessionContext.AktifKullaniciId.HasValue)
                yetkiler = _puantajService.GetKullaniciFirmaIsyeriYetkileri((int)_sessionContext.AktifKullaniciId);

            bool puantajYapilir = kartTipi != "puantajsiz";
            var (single, idIn) = FirmaIsyeriYetkiHelper.ResolveKisiQueryIsyeriFilter(
                effectiveFirmaId, isyeriId, yetkiler, isAdmin);
            var kisiler = _kisiQueryService.GetAktifKisilerByFirma(effectiveFirmaId, null, puantajYapilir, single, idIn)
                ?? new List<KisiListItem>();
            var list = new List<PersonelLookupItem>();
            foreach (var k in kisiler)
            {
                if (string.IsNullOrWhiteSpace(k.PersonelId) || string.IsNullOrWhiteSpace(k.AdSoyad))
                    continue;
                if (!int.TryParse(k.PersonelId, out int id) || id <= 0)
                    continue;
                list.Add(new PersonelLookupItem { Id = id, Ad = k.AdSoyad });
            }

            var isyerleri = FirmaIsyeriYetkiHelper.FilterIsyeriLookup(
                _lookupService.GetIsyerleri(effectiveFirmaId) ?? new List<LookupItem>(),
                effectiveFirmaId,
                yetkiler,
                isAdmin);

            var firmalar = _sessionContext.IsAdmin()
                ? _firmaService.GetAll().OrderBy(f => f.FirmaAdi).ToList()
                : null;
            var aktifFirma = _firmaService.GetAll().FirstOrDefault(f => f.FirmaId == effectiveFirmaId);

            return Ok(ApiResult<object>.Ok(new
            {
                AktifFirma = aktifFirma == null ? null : new { aktifFirma.FirmaId, aktifFirma.FirmaAdi },
                Firmalar = firmalar,
                Isyerleri = isyerleri,
                PersonelList = list
            }));
        }

        [HttpPost("ekle")]
        public ActionResult<ApiResult> Post([FromBody] HareketEkleRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create)) return Forbid();

            int firmaId = _sessionContext.AktifFirmaId ?? 0;
            if (firmaId == 0) return BadRequest(ApiResult.Failure("Firma bilgisi bulunamadı."));

            var tip = NormalizeTip(request.Tip);
            bool success = _kisiHareketService.InsertManual(firmaId, request.PersonelId, request.Tarih, tip);
            return success ? Ok(ApiResult.Ok("Hareket başarıyla eklendi.")) : BadRequest(ApiResult.Failure("Hareket eklenemedi."));
        }

        [HttpPut("{id}")]
        public ActionResult<ApiResult> Update(int id, [FromBody] HareketGuncelleRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update)) return Forbid();
            var tip = NormalizeTip(request.Tip);
            bool success = _kisiHareketService.UpdateManual(id, request.Tarih, tip);
            return success ? Ok(ApiResult.Ok("Hareket başarıyla güncellendi.")) : BadRequest(ApiResult.Failure("Hareket güncellenemedi."));
        }

        [HttpDelete("{id}")]
        public ActionResult<ApiResult> Delete(int id)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Delete)) return Forbid();

            bool success = _kisiHareketService.PasifYap(id);
            return success ? Ok(ApiResult.Ok("Hareket pasif yapıldı.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }

        [HttpPost("{id}/aktif")]
        public ActionResult<ApiResult> Aktif(int id)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Delete)) return Forbid();

            bool success = _kisiHareketService.AktifYap(id);
            return success
                ? Ok(ApiResult.Ok("Hareket tekrar aktif edildi."))
                : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }

        private static string NormalizeTip(string? tip)
        {
            var t = (tip ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(t)) return "Giriş";
            // Accept legacy codes too
            if (string.Equals(t, "G", StringComparison.OrdinalIgnoreCase)) return "Giriş";
            if (string.Equals(t, "C", StringComparison.OrdinalIgnoreCase)) return "Çıkış";
            if (string.Equals(t, "Ç", StringComparison.OrdinalIgnoreCase)) return "Çıkış";
            if (string.Equals(t, "GİRİŞ", StringComparison.OrdinalIgnoreCase)) return "Giriş";
            if (string.Equals(t, "GIRIS", StringComparison.OrdinalIgnoreCase)) return "Giriş";
            if (string.Equals(t, "ÇIKIŞ", StringComparison.OrdinalIgnoreCase)) return "Çıkış";
            if (string.Equals(t, "CIKIS", StringComparison.OrdinalIgnoreCase)) return "Çıkış";

            // Last resort: keep as-is (but avoid unexpected empties)
            return t;
        }
    }

    public class HareketEkleRequest
    {
        public int PersonelId { get; set; }
        public DateTime Tarih { get; set; }
        public string Tip { get; set; } = "Giriş";
    }

    public class HareketGuncelleRequest
    {
        public DateTime Tarih { get; set; }
        public string Tip { get; set; } = "Giriş";
    }
}
