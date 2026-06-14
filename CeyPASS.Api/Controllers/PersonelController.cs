using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CeyPASS.Entities.Concrete;
using CeyPASS.Business.Abstractions;
using IAuthorizationService = CeyPASS.Business.Abstractions.IAuthorizationService;
using CeyPASS.Models;
using CeyPASS.Infrastructure.Helpers;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Globalization;

namespace CeyPASS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PersonelController : ControllerBase
    {
        private readonly IKisiService _kisiService;
        private readonly IKisiQueryService _kisiQueryService;
        private readonly IKisiEkraniLookUpService _lookupService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly IFirmaService _firmaService;
        private readonly ICalismaSekliService _calismaSekliService;
        private readonly IPuantajService _puantajService;
        private const string PageName = "Personeller";

        public class PagedResponse<T>
        {
            public List<T> Items { get; set; } = new();
            public int TotalCount { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public int TotalPages { get; set; }
        }

        public sealed class PersonelDetailsDto : KisiDetay
        {
            public string? FirmaAdi { get; set; }
            public string? IsyeriAdi { get; set; }
            public string? DepartmanAdi { get; set; }
            public string? PozisyonAdi { get; set; }
            public string? BolumAdi { get; set; }
        }

        // Request payload DTOs must be nullable-friendly to avoid ApiController automatic 400
        // before we can normalize web/mobile differences (e.g., Kisi.CalismaStatusu).
        public sealed class KisiPayload
        {
            public string? PersonelId { get; set; }
            public string? Ad { get; set; }
            public string? Soyad { get; set; }
            public string? KartNo { get; set; }
            public string? TcKimlikNo { get; set; }
            public int? PozisyonId { get; set; }
            public int? DepartmanId { get; set; }
            public int? FirmaId { get; set; }
            public int? IsyeriId { get; set; }
            public int? BolumId { get; set; }
            public DateTime? DogumTarihi { get; set; }
            public DateTime? IseGirisTarihi { get; set; }
            public DateTime? IstenCikisTarihi { get; set; }
            public string? CalismaStatusu { get; set; }
            public string? CalismaStatusuText { get; set; }
            public string? CalismaSekli { get; set; }
            public string? CalismaSekliCsv { get; set; }
            public string? CepTel { get; set; }
            public string? Email { get; set; }
            public bool? PuantajYapilirMi { get; set; }
        }

        public class PersonelCreateRequest
        {
            public KisiPayload? Kisi { get; set; }
            public bool FirmaPersoneli { get; set; }
            public bool PuantajYapilabilir { get; set; }
            public bool YemekHakkiVar { get; set; }
            public int GunlukYemekAdedi { get; set; }
            public int GunlukYemekLimiti { get; set; }
            public string? FirmaDisiKartNo { get; set; }
            public bool ZiyaretciMi { get; set; }
            public bool AracKartiMi { get; set; }
            public bool TaseronCalisanMi { get; set; }
            public bool FotoDegisti { get; set; }
            public string? FotografBase64 { get; set; }
        }

        private string ResolveCalismaStatusu(int firmaId, string? idOrText, string? text)
        {
            var s = (idOrText ?? string.Empty).Trim();
            if (int.TryParse(s, out var id) && id > 0) return id.ToString();

            var name = (text ?? s).Trim();
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;

            try
            {
                var statu = _lookupService.GetCalismaStatuleri(firmaId)
                    ?.FirstOrDefault(x => (x.Ad ?? string.Empty).Trim().Equals(name, StringComparison.OrdinalIgnoreCase));
                if (statu != null && statu.Id > 0) return statu.Id.ToString();
            }
            catch { }

            // fallback: keep name (legacy)
            return name;
        }

        public sealed class PersonelUpdateRequest : PersonelCreateRequest
        {
            public string? OriginalPersonelId { get; set; }
        }

        public sealed class PersonelIstenCikarRequest
        {
            public string? PersonelId { get; set; }
            public string? CikisTarihi { get; set; } // yyyy-MM-dd
            public string? FirmaDisiKartNo { get; set; }
        }

        private static bool TryParseBase64(string? b64, out byte[] bytes)
        {
            bytes = Array.Empty<byte>();
            if (string.IsNullOrWhiteSpace(b64)) return false;
            var s = b64.Trim();
            // allow data URI
            var comma = s.IndexOf(',');
            if (comma >= 0) s = s.Substring(comma + 1);
            try
            {
                bytes = Convert.FromBase64String(s);
                return bytes.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        public PersonelController(
            IKisiService kisiService,
            IKisiQueryService kisiQueryService,
            IKisiEkraniLookUpService lookupService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService,
            IFirmaService firmaService,
            ICalismaSekliService calismaSekliService,
            IPuantajService puantajService)
        {
            _kisiService = kisiService;
            _kisiQueryService = kisiQueryService;
            _lookupService = lookupService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
            _firmaService = firmaService;
            _calismaSekliService = calismaSekliService;
            _puantajService = puantajService;
        }

        [HttpGet]
        public ActionResult<ApiResult<PagedResponse<KisiListItem>>> Get(
            [FromQuery] string? search,
            [FromQuery] int? firmaId,
            [FromQuery] int? isyeriId,
            [FromQuery] bool? puantajYapilirMi,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (!_authorizationService.ViewAbility(PageName))
            {
                return Forbid();
            }

            int effectiveFirmaId = _sessionContext.AktifFirmaId ?? 0;
            if (_sessionContext.IsAdmin() && firmaId.HasValue && firmaId.Value > 0)
            {
                effectiveFirmaId = firmaId.Value;
            }
            if (effectiveFirmaId == 0) return BadRequest(ApiResult.Failure("Firma bilgisi bulunamadı."));

            bool isAdmin = _sessionContext.IsAdmin();
            List<FirmaIsyeriYetkiDTO>? yetkiler = null;
            if (!isAdmin && _sessionContext.AktifKullaniciId.HasValue)
                yetkiler = _puantajService.GetKullaniciFirmaIsyeriYetkileri(_sessionContext.AktifKullaniciId.Value);
            var (queryIsyeriId, queryIsyeriIdIn) = FirmaIsyeriYetkiHelper.ResolveKisiQueryIsyeriFilter(
                effectiveFirmaId, isyeriId, yetkiler, isAdmin);

            int totalCount;
            bool effectivePuantaj = puantajYapilirMi ?? true;
            var items = _kisiQueryService.GetAktifKisilerByFirmaPaged(
                effectiveFirmaId, search, effectivePuantaj, queryIsyeriId, queryIsyeriIdIn, page, pageSize, out totalCount);

            var totalPages = pageSize <= 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize);
            if (totalPages < 1) totalPages = 1;

            var resp = new PagedResponse<KisiListItem>
            {
                Items = items?.ToList() ?? new List<KisiListItem>(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return Ok(ApiResult<PagedResponse<KisiListItem>>.Ok(resp));
        }

        [HttpPost]
        public ActionResult<ApiResult<object>> Create([FromBody] PersonelCreateRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create))
                return Forbid();

            var k = request.Kisi ?? new KisiPayload();
            var firmaId = (k.FirmaId ?? 0) > 0 ? k.FirmaId!.Value : (_sessionContext.AktifFirmaId ?? 0);
            var kisi = new Kisi
            {
                PersonelId = (k.PersonelId ?? string.Empty).Trim(),
                Ad = (k.Ad ?? string.Empty).Trim(),
                Soyad = (k.Soyad ?? string.Empty).Trim(),
                KartNo = (k.KartNo ?? string.Empty).Trim(),
                TcKimlikNo = (k.TcKimlikNo ?? string.Empty).Trim(),
                PozisyonId = k.PozisyonId,
                DepartmanId = k.DepartmanId,
                FirmaId = firmaId,
                IsyeriId = k.IsyeriId,
                BolumId = k.BolumId,
                DogumTarihi = k.DogumTarihi,
                IseGirisTarihi = k.IseGirisTarihi ?? DateTime.Today,
                IstenCikisTarihi = k.IstenCikisTarihi,
                CalismaSekli = ((k.CalismaSekli ?? k.CalismaSekliCsv) ?? string.Empty).Trim(),
                CepTel = (k.CepTel ?? string.Empty).Trim(),
                Email = (k.Email ?? string.Empty).Trim(),
                PuantajYapilirMi = k.PuantajYapilirMi ?? request.PuantajYapilabilir,
                // Normalize required field
                CalismaStatusu = ResolveCalismaStatusu(firmaId, k.CalismaStatusu, k.CalismaStatusuText)
            };
            // flags that are not part of Kisi model
            kisi.ZiyaretciMi = request.ZiyaretciMi;
            kisi.AracKartiMi = request.AracKartiMi;
            kisi.TaseronCalisanMi = request.TaseronCalisanMi;

            if (request.FotoDegisti && TryParseBase64(request.FotografBase64, out var fotoBytes))
            {
                kisi.Fotograf = fotoBytes;
            }

            var validationDto = new KisiKayitValidasyonDTO
            {
                PersonelId = kisi.PersonelId,
                FirmaPersoneli = request.FirmaPersoneli,
                PuantajYapilir = request.PuantajYapilabilir,
                YemekHakkiVar = request.YemekHakkiVar,
                YemekAdedi = request.GunlukYemekAdedi > 0 ? request.GunlukYemekAdedi : request.GunlukYemekLimiti,
                FirmaDisiKartNo = (request.FirmaDisiKartNo ?? string.Empty).Trim()
            };
            var validation = _kisiService.ValidateKisiKayit(validationDto);
            if (!validation.IsValid)
                return BadRequest(ApiResult.Failure(validation.Message ?? "Validasyon hatası."));

            try
            {
                _kisiService.YeniKisiEkle(
                    kisi,
                    request.FirmaPersoneli,
                    request.PuantajYapilabilir,
                    request.YemekHakkiVar,
                    request.GunlukYemekAdedi > 0 ? request.GunlukYemekAdedi : request.GunlukYemekLimiti,
                    puantajsizKartId: "",
                    puantajsizKartNo: (request.FirmaDisiKartNo ?? string.Empty).Trim(),
                    puantajsizKartAdi: ""
                );
                return Ok(ApiResult.Ok("Personel eklendi."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult.Failure("Personel eklenemedi: " + ex.Message));
            }
        }

        [HttpPut]
        public ActionResult<ApiResult<object>> Update([FromBody] PersonelUpdateRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update))
                return Forbid();

            var original = (request.OriginalPersonelId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(original))
                return BadRequest(ApiResult.Failure("OriginalPersonelId zorunludur."));

            // Prevent accidental data loss: start from existing record and override with provided fields.
            var existing = _kisiQueryService.GetKisiDetay(original);
            if (existing == null) return NotFound(ApiResult.Failure("Personel bulunamadı."));
            if (!_sessionContext.IsAdmin() && existing.FirmaId != _sessionContext.AktifFirmaId)
                return Forbid();

            var k = request.Kisi ?? new KisiPayload();
            string Coalesce(string? v, string? fallback) => string.IsNullOrWhiteSpace(v) ? (fallback ?? string.Empty) : v.Trim();
            int? CoalesceInt(int? v, int? fallback) => v.HasValue ? v : fallback;

            var effectiveFirmaId = (k.FirmaId ?? 0) > 0 ? k.FirmaId!.Value : existing.FirmaId;
            var kisi = new Kisi
            {
                PersonelId = Coalesce(k.PersonelId, existing.PersonelId),
                Ad = Coalesce(k.Ad, existing.Ad),
                Soyad = Coalesce(k.Soyad, existing.Soyad),
                KartNo = Coalesce(k.KartNo, existing.KartNo),
                TcKimlikNo = Coalesce(k.TcKimlikNo, existing.TcKimlikNo),
                PozisyonId = CoalesceInt(k.PozisyonId, existing.PozisyonId),
                DepartmanId = CoalesceInt(k.DepartmanId, existing.DepartmanId),
                FirmaId = effectiveFirmaId,
                IsyeriId = CoalesceInt(k.IsyeriId, existing.IsyeriId),
                BolumId = CoalesceInt(k.BolumId, existing.BolumId),
                DogumTarihi = k.DogumTarihi ?? existing.DogumTarihi,
                IseGirisTarihi = k.IseGirisTarihi ?? existing.IseGirisTarihi ?? DateTime.Today,
                IstenCikisTarihi = k.IstenCikisTarihi ?? existing.IstenCikisTarihi,
                CalismaSekli = Coalesce((k.CalismaSekli ?? k.CalismaSekliCsv), existing.CalismaSekliCsv),
                CepTel = Coalesce(k.CepTel, existing.CepTel),
                Email = Coalesce(k.Email, existing.Email),
                PuantajYapilirMi = k.PuantajYapilirMi ?? request.PuantajYapilabilir,
                CalismaStatusu = ResolveCalismaStatusu(effectiveFirmaId, k.CalismaStatusu, Coalesce(k.CalismaStatusuText, existing.CalismaStatusuText))
            };
            kisi.ZiyaretciMi = request.ZiyaretciMi;
            kisi.AracKartiMi = request.AracKartiMi;
            kisi.TaseronCalisanMi = request.TaseronCalisanMi;

            var fotoDegisti = request.FotoDegisti;
            if (fotoDegisti && TryParseBase64(request.FotografBase64, out var fotoBytes))
            {
                kisi.Fotograf = fotoBytes;
            }

            try
            {
                var ok = _kisiService.KisiGuncelle(
                    kisi,
                    original,
                    request.FirmaPersoneli,
                    request.PuantajYapilabilir,
                    request.YemekHakkiVar,
                    request.GunlukYemekAdedi > 0 ? request.GunlukYemekAdedi : request.GunlukYemekLimiti,
                    (request.FirmaDisiKartNo ?? string.Empty).Trim(),
                    fotoDegisti
                );
                if (!ok) return BadRequest(ApiResult.Failure("Personel güncellenemedi."));
                return Ok(ApiResult.Ok("Personel güncellendi."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult.Failure("Personel güncellenemedi: " + ex.Message));
            }
        }

        [HttpPost("isten-cikar")]
        public ActionResult<ApiResult<object>> IstenCikar([FromBody] PersonelIstenCikarRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Delete))
                return Forbid();

            var pid = (request.PersonelId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(pid))
                return BadRequest(ApiResult.Failure("PersonelId zorunludur."));

            DateTime cikis = DateTime.Today;
            if (!string.IsNullOrWhiteSpace(request.CikisTarihi))
            {
                // mobile sends yyyy-MM-dd; keep parsing strict to avoid culture issues
                if (!DateTime.TryParseExact(request.CikisTarihi.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out cikis))
                {
                    // fallback
                    DateTime.TryParse(request.CikisTarihi, out cikis);
                }
            }

            try
            {
                // IDOR protection: non-admin can only fire personnel in active firm
                var kisi = _kisiQueryService.GetKisiDetay(pid);
                if (kisi == null) return NotFound(ApiResult.Failure("Personel bulunamadı."));
                if (!_sessionContext.IsAdmin() && kisi.FirmaId != _sessionContext.AktifFirmaId)
                    return Forbid();

                var kartNo = (request.FirmaDisiKartNo ?? string.Empty).Trim();
                var ok = _kisiService.KisiIstenCikar(pid, cikis, kartNo);
                if (!ok) return BadRequest(ApiResult.Failure("İşten çıkarma başarısız."));
                return Ok(ApiResult.Ok("Personel işten çıkarıldı."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult.Failure("İşten çıkarma hatası: " + ex.Message));
            }
        }

        [HttpGet("{id}")]
        public ActionResult<ApiResult<PersonelDetailsDto>> GetDetails(string id)
        {
            var kisi = _kisiQueryService.GetKisiDetay(id);
            if (kisi == null) return NotFound(ApiResult.Failure("Personel bulunamadı."));

            // IDOR Protection: Firma bazlı filtreleme
            if (!_sessionContext.IsAdmin() && kisi.FirmaId != _sessionContext.AktifFirmaId)
            {
                return Forbid();
            }

            var firma = _firmaService.GetAll().FirstOrDefault(f => f.FirmaId == kisi.FirmaId);
            var isyeri = _lookupService.GetIsyerleri(kisi.FirmaId)?.FirstOrDefault(i => i.Id == (kisi.IsyeriId ?? 0));
            var departman = _lookupService.GetDepartmanlar(kisi.FirmaId)?.FirstOrDefault(d => d.Id == (kisi.DepartmanId ?? 0));
            var pozisyon = _lookupService.GetPozisyonlar(kisi.FirmaId)?.FirstOrDefault(p => p.Id == (kisi.PozisyonId ?? 0));
            var bolum = _lookupService.GetBolumler(kisi.FirmaId)?.FirstOrDefault(b => b.Id == (kisi.BolumId ?? 0));
            var statu = _lookupService.GetCalismaStatuleri(kisi.FirmaId)?.FirstOrDefault(s => s.Id == (kisi.CalismaStatusuId ?? 0));

            var dto = new PersonelDetailsDto
            {
                PersonelId = kisi.PersonelId,
                Ad = kisi.Ad,
                Soyad = kisi.Soyad,
                KartNo = kisi.KartNo,
                TcKimlikNo = kisi.TcKimlikNo,
                PozisyonId = kisi.PozisyonId,
                DepartmanId = kisi.DepartmanId,
                FirmaId = kisi.FirmaId,
                IsyeriId = kisi.IsyeriId,
                BolumId = kisi.BolumId,
                DogumTarihi = kisi.DogumTarihi,
                IseGirisTarihi = kisi.IseGirisTarihi,
                IstenCikisTarihi = kisi.IstenCikisTarihi,
                CalismaStatusuId = kisi.CalismaStatusuId,
                CalismaStatusuText = statu?.Ad ?? kisi.CalismaStatusuText,
                CalismaSekliCsv = kisi.CalismaSekliCsv,
                CepTel = kisi.CepTel,
                Email = kisi.Email,
                Fotograf = kisi.Fotograf,
                FirmaPersoneli = kisi.FirmaPersoneli,
                PuantajYapilabilir = kisi.PuantajYapilabilir,
                YemekHakkiVar = kisi.YemekHakkiVar,
                GunlukYemekAdedi = kisi.GunlukYemekAdedi,
                TaseronKartNo = kisi.TaseronKartNo,
                ZiyaretciMi = kisi.ZiyaretciMi,
                AracKartiMi = kisi.AracKartiMi,
                TaseronCalisanMi = kisi.TaseronCalisanMi,

                FirmaAdi = firma?.FirmaAdi,
                IsyeriAdi = isyeri?.Ad,
                DepartmanAdi = departman?.Ad,
                PozisyonAdi = pozisyon?.Ad,
                BolumAdi = bolum?.Ad,
            };

            return Ok(ApiResult<PersonelDetailsDto>.Ok(dto));
        }

        [HttpGet("lookups")]
        public ActionResult<ApiResult<object>> GetLookups([FromQuery] int? firmaId)
        {
            int effectiveFirmaId = _sessionContext.AktifFirmaId ?? 0;
            if (_sessionContext.IsAdmin() && firmaId.HasValue && firmaId.Value > 0)
            {
                effectiveFirmaId = firmaId.Value;
            }

            var aktifFirma = _firmaService.GetAll().FirstOrDefault(f => f.FirmaId == effectiveFirmaId);

            bool isAdmin = _sessionContext.IsAdmin();
            List<FirmaIsyeriYetkiDTO>? yetkiler = null;
            if (!isAdmin && _sessionContext.AktifKullaniciId.HasValue)
                yetkiler = _puantajService.GetKullaniciFirmaIsyeriYetkileri(_sessionContext.AktifKullaniciId.Value);
            var isyerleri = FirmaIsyeriYetkiHelper.FilterIsyeriLookup(
                _lookupService.GetIsyerleri(effectiveFirmaId) ?? new List<LookupItem>(),
                effectiveFirmaId,
                yetkiler,
                isAdmin);

            var lookups = new
            {
                AktifFirma = aktifFirma == null ? null : new { aktifFirma.FirmaId, aktifFirma.FirmaAdi },
                Firmalar = isAdmin
                    ? _firmaService.GetAll().OrderBy(f => f.FirmaAdi).ToList()
                    : null,
                Isyerleri = isyerleri,
                Departmanlar = _lookupService.GetDepartmanlar(effectiveFirmaId),
                Pozisyonlar = _lookupService.GetPozisyonlar(effectiveFirmaId),
                Bolumler = _lookupService.GetBolumler(effectiveFirmaId),
                CalismaStatuleri = _lookupService.GetCalismaStatuleri(effectiveFirmaId),
                CalismaSekilleri = _calismaSekliService.GetAll(effectiveFirmaId),
            };

            return Ok(ApiResult<object>.Ok(lookups));
        }
    }
}
