using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CeyPASS.Entities.Concrete;
using IAuthorizationService = CeyPASS.Business.Abstractions.IAuthorizationService;
using CeyPASS.Models;
using CeyPASS.Business.Abstractions;
using CeyPASS.Infrastructure.Helpers;
using System.Globalization;
using IsyeriItemEntity = CeyPASS.Entities.Concrete.IsyeriItem;

namespace CeyPASS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PuantajController : ControllerBase
    {
        private readonly IPuantajService _puantajService;
        private readonly IFirmaService _firmaService;
        private readonly IIsyeriService _isyeriService;
        private readonly IKisiService _kisiService;
        private readonly IKisiQueryService _kisiQueryService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private const string PageName = "AylikPuantaj";

        public PuantajController(
            IPuantajService puantajService,
            IFirmaService firmaService,
            IIsyeriService isyeriService,
            IKisiService kisiService,
            IKisiQueryService kisiQueryService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService)
        {
            _puantajService = puantajService;
            _firmaService = firmaService;
            _isyeriService = isyeriService;
            _kisiService = kisiService;
            _kisiQueryService = kisiQueryService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
        }

        [HttpGet("lookups")]
        public ActionResult<ApiResult<PuantajLookupsDto>> GetLookups([FromQuery] int? firmaId, [FromQuery] int? isyeriId, [FromQuery] int? yil, [FromQuery] int? ay)
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();
            if (!_sessionContext.AktifKullaniciId.HasValue) return Unauthorized();

            int selectedYil = yil ?? DateTime.Today.Year;
            int selectedAy = ay ?? DateTime.Today.Month;
            int selectedFirmaId = firmaId ?? (_sessionContext.AktifFirmaId.HasValue ? (int)_sessionContext.AktifFirmaId.Value : 0);
            int? selectedIsyeriId = isyeriId;

            try
            {
                var yetkiler = _puantajService.GetKullaniciFirmaIsyeriYetkileri(_sessionContext.AktifKullaniciId.Value) ?? new List<FirmaIsyeriYetkiDTO>();
                var firmaYetkileri = yetkiler.Select(y => y.FirmaId).Distinct().ToHashSet();

                var firmalar = _firmaService.GetPuantajFirmalar() ?? new List<Firma>();
                if (firmaYetkileri.Count > 0)
                    firmalar = firmalar.Where(f => firmaYetkileri.Contains(f.FirmaId)).OrderBy(f => f.FirmaAdi).ToList();

                if (firmaYetkileri.Count > 0 && !firmaYetkileri.Contains(selectedFirmaId))
                    selectedFirmaId = firmaYetkileri.First();

                List<IsyeriItemEntity> isyerleri = selectedFirmaId > 0 ? _isyeriService.GetIsyerleriByFirma(selectedFirmaId) : new List<IsyeriItemEntity>();

                List<PuantajPersonelItemDto> kisiler;
                if (selectedFirmaId > 0 && selectedIsyeriId.HasValue && selectedIsyeriId.Value > 0)
                {
                    var kp = _kisiService.GetKisilerForPuantaj(selectedFirmaId, selectedIsyeriId.Value, selectedYil, selectedAy) ?? new List<Kisi>();
                    kisiler = kp.Select(k => new PuantajPersonelItemDto
                    {
                        PersonelId = k.PersonelId,
                        AdSoyad = ((k.Ad ?? "") + " " + (k.Soyad ?? "")).Trim()
                    }).ToList();
                }
                else if (selectedFirmaId > 0)
                {
                    var kq = _kisiQueryService.GetAktifKisilerByFirma(selectedFirmaId) ?? new List<KisiListItem>();
                    kisiler = kq.Select(k => new PuantajPersonelItemDto { PersonelId = k.PersonelId, AdSoyad = k.AdSoyad ?? "" }).ToList();
                }
                else
                {
                    kisiler = new List<PuantajPersonelItemDto>();
                }

                var tipler = _puantajService.GetPuantajTipleri() ?? new List<PuantajTipDTO>();
                var ekKayitGun = _puantajService.GetEkKayitGun();

                var dto = new PuantajLookupsDto
                {
                    SelectedYil = selectedYil,
                    SelectedAy = selectedAy,
                    SelectedFirmaId = selectedFirmaId,
                    SelectedIsyeriId = selectedIsyeriId,
                    Firmalar = firmalar.Select(f => new PuantajFirmaItemDto { FirmaId = f.FirmaId, FirmaAdi = f.FirmaAdi }).ToList(),
                    Isyerleri = isyerleri.Select(i => new PuantajIsyeriItemDto { IsyeriId = i.IsyeriId, Ad = i.Ad }).ToList(),
                    Personeller = kisiler,
                    PuantajTipleri = tipler,
                    EkKayitGun = ekKayitGun,
                    CanUpdate = _authorizationService.Can(PageName, YetkiTipleri.Update),
                    CanApprove = _authorizationService.Can(PageName, YetkiTipleri.Approve),
                    CanDelete = _authorizationService.Can(PageName, YetkiTipleri.Delete),
                    CanExport = _authorizationService.Can(PageName, YetkiTipleri.Export),
                };

                return Ok(ApiResult<PuantajLookupsDto>.Ok(dto));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<PuantajLookupsDto>.Failure($"Lookups alınamadı: {ex.Message}"));
            }
        }

        [HttpGet("{personelId}")]
        public ActionResult<ApiResult<List<PuantajGunSatirDTO>>> GetAy(int personelId, [FromQuery] int? yil, [FromQuery] int? ay)
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();

            int selectedYil = yil ?? DateTime.Today.Year;
            int selectedAy = ay ?? DateTime.Today.Month;

            try
            {
                var data = _puantajService.GetAy(personelId, selectedYil, selectedAy);
                return Ok(ApiResult<List<PuantajGunSatirDTO>>.Ok(data));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Failure($"Puantaj verisi alınamadı: {ex.Message}"));
            }
        }

        [HttpPost("onayla")]
        public ActionResult<ApiResult> Onayla([FromBody] PuantajOnayRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Approve)) return Forbid();
            if (!_sessionContext.AktifKullaniciId.HasValue) return Unauthorized();

            try
            {
                decimal saat = request.Saat;
                if (!string.IsNullOrWhiteSpace(request.SaatText))
                {
                    if (!TryParseSaat(request.SaatText, out saat))
                        return BadRequest(ApiResult.Failure("Çalışma saati geçersiz (örn: 7,50 veya 7.50)."));
                }

                _puantajService.Onayla(
                    request.PersonelId, 
                    request.Tarih, 
                    request.DuzenlenmisFm, 
                    request.Aciklama ?? "", 
                    request.CalismaTipi ?? "", 
                    saat, 
                    _sessionContext.AktifKullaniciId.Value);

                return Ok(ApiResult.Ok("Puantaj onaylandı."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Failure($"İşlem başarısız: {ex.Message}"));
            }
        }

        [HttpPost("reddet")]
        public ActionResult<ApiResult> Reddet([FromBody] PuantajRedRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Delete)) return Forbid();
            if (!_sessionContext.AktifKullaniciId.HasValue) return Unauthorized();

            try
            {
                _puantajService.Reddet(request.PersonelId, request.Tarih, request.Aciklama ?? "", _sessionContext.AktifKullaniciId.Value);
                return Ok(ApiResult.Ok("Puantaj reddedildi."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Failure($"İşlem başarısız: {ex.Message}"));
            }
        }

        [HttpGet("tipler")]
        public ActionResult<ApiResult<List<PuantajTipDTO>>> GetTipler()
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();
            var tipler = _puantajService.GetPuantajTipleri();
            return Ok(ApiResult<List<PuantajTipDTO>>.Ok(tipler));
        }

        [HttpPost("toplu-onayla")]
        public ActionResult<ApiResult> TopluOnayla([FromBody] PuantajTopluOnayRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Approve)) return Forbid();
            if (!_sessionContext.AktifKullaniciId.HasValue) return Unauthorized();

            try
            {
                _puantajService.TopluOnayla(request.PersonelId, request.Yil, request.Ay, _sessionContext.AktifKullaniciId.Value);
                return Ok(ApiResult.Ok("Seçili personelin aylık puantajı başarıyla toplu onaylandı."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Failure($"İşlem başarısız: {ex.Message}"));
            }
        }

        [HttpPost("duzenle")]
        public ActionResult<ApiResult<PuantajDuzenleResponse>> Duzenle([FromBody] PuantajDuzenleRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update)) return Forbid();
            if (!_sessionContext.AktifKullaniciId.HasValue) return Unauthorized();

            decimal? saatDec = null;
            if (!string.IsNullOrWhiteSpace(request.SaatText))
            {
                if (!TryParseSaat(request.SaatText, out var parsed))
                    return BadRequest(ApiResult<PuantajDuzenleResponse>.Failure("Çalışma saati geçersiz (örn: 7,50 veya 7.50)."));
                saatDec = parsed;
            }

            try
            {
                int duzenlenenFmDakika = request.DuzenlenmisFm;

                if (!string.IsNullOrWhiteSpace(request.CalismaTipi) && saatDec.HasValue)
                {
                    duzenlenenFmDakika = _puantajService.HesaplaFazlaMesaiDakika(request.CalismaTipi, saatDec.Value);
                    _puantajService.DuzenleOnayla(
                        request.PersonelId,
                        request.Tarih,
                        request.DuzenlenmisFm,
                        request.Aciklama ?? "",
                        request.CalismaTipi,
                        saatDec.Value,
                        _sessionContext.AktifKullaniciId.Value);
                }
                else
                {
                    _puantajService.Duzenle(request.PersonelId, request.Tarih, request.DuzenlenmisFm, request.Aciklama ?? "", _sessionContext.AktifKullaniciId.Value);
                }

                var resp = new PuantajDuzenleResponse
                {
                    OnayDurumu = "Düzeltildi",
                    CalismaTipi = request.CalismaTipi ?? "",
                    Saat = saatDec,
                    Aciklama = request.Aciklama ?? "",
                    DuzenlenenFmDakika = duzenlenenFmDakika
                };

                return Ok(ApiResult<PuantajDuzenleResponse>.Ok(resp, "Puantaj başarıyla düzenlendi."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<PuantajDuzenleResponse>.Failure($"İşlem başarısız: {ex.Message}"));
            }
        }

        [HttpGet("ek-kayit-gun")]
        public ActionResult<ApiResult<int>> GetEkKayitGun()
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();
            try
            {
                return Ok(ApiResult<int>.Ok(_puantajService.GetEkKayitGun()));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<int>.Failure($"Ek kayıt günü alınamadı: {ex.Message}"));
            }
        }

        [HttpPost("ek-kayit-gun")]
        public ActionResult<ApiResult> SetEkKayitGun([FromBody] PuantajEkKayitGunRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update)) return Forbid();
            if (!_sessionContext.AktifKullaniciId.HasValue) return Unauthorized();

            try
            {
                _puantajService.SetEkKayitGun(request.Gun, _sessionContext.AktifKullaniciId.Value);
                return Ok(ApiResult.Ok($"Ek kayıt günü {request.Gun} olarak ayarlandı."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Failure($"İşlem başarısız: {ex.Message}"));
            }
        }

        [HttpPost("coklu-sicile-aktar")]
        public ActionResult<ApiResult> CokluSicileAktar([FromBody] PuantajCokluSicilRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update)) return Forbid();
            if (!_sessionContext.AktifKullaniciId.HasValue) return Unauthorized();

            try
            {
                _puantajService.CokluSicileAktar(request.PersonelId, request.Yil, request.Ay, _sessionContext.AktifKullaniciId.Value);
                return Ok(ApiResult.Ok("Çoklu sicil aktarımı tamamlandı."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Failure($"İşlem başarısız: {ex.Message}"));
            }
        }

        [HttpPost("export-excel")]
        public ActionResult ExportExcel([FromBody] PuantajExportApiRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Export)) return Forbid();
            if (!_sessionContext.AktifKullaniciId.HasValue) return Unauthorized();

            try
            {
                var yetkiler = _puantajService.GetKullaniciFirmaIsyeriYetkileri(_sessionContext.AktifKullaniciId.Value) ?? new List<FirmaIsyeriYetkiDTO>();
                var exportRequest = new PuantajExportRequest { Yil = request.Yil, Ay = request.Ay, Yetkiler = yetkiler };
                var exportData = _puantajService.PrepareMonthlyExport(exportRequest);
                if (exportData == null || exportData.Count == 0)
                    return BadRequest(ApiResult.Failure("Export edilecek veri bulunamadı."));

                string fileName = $"{request.Yil}_{request.Ay:D2}_Puantaj.xlsx";
                string tempPath = Path.Combine(Path.GetTempPath(), fileName);
                ExcelHelper.ExceleDonustur(exportData, tempPath);
                byte[] fileBytes = System.IO.File.ReadAllBytes(tempPath);
                System.IO.File.Delete(tempPath);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Failure($"Excel export hatası: {ex.Message}"));
            }
        }

        [HttpGet("export-excel")]
        public ActionResult ExportExcelGet([FromQuery] int yil, [FromQuery] int ay)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Export)) return Forbid();
            if (!_sessionContext.AktifKullaniciId.HasValue) return Unauthorized();

            try
            {
                var yetkiler = _puantajService.GetKullaniciFirmaIsyeriYetkileri(_sessionContext.AktifKullaniciId.Value) ?? new List<FirmaIsyeriYetkiDTO>();
                var exportRequest = new PuantajExportRequest { Yil = yil, Ay = ay, Yetkiler = yetkiler };
                var exportData = _puantajService.PrepareMonthlyExport(exportRequest);
                if (exportData == null || exportData.Count == 0)
                    return BadRequest(ApiResult.Failure("Export edilecek veri bulunamadı."));

                string fileName = $"{yil}_{ay:D2}_Puantaj.xlsx";
                string tempPath = Path.Combine(Path.GetTempPath(), fileName);
                ExcelHelper.ExceleDonustur(exportData, tempPath);
                byte[] fileBytes = System.IO.File.ReadAllBytes(tempPath);
                System.IO.File.Delete(tempPath);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Failure($"Excel export hatası: {ex.Message}"));
            }
        }

        /// <summary>
        /// Web/WFA ile aynı saat parse kuralı: virgül→nokta; 75→7,5; 750→7,5.
        /// </summary>
        private static bool TryParseSaat(string value, out decimal result)
        {
            result = 0;
            if (string.IsNullOrWhiteSpace(value)) return false;
            value = value.Trim();

            var normalized = value.Replace(",", ".");
            var tr = CultureInfo.GetCultureInfo("tr-TR");
            var inv = CultureInfo.InvariantCulture;

            if (decimal.TryParse(normalized, NumberStyles.Number, inv, out result)) goto Normalize;
            if (decimal.TryParse(value, NumberStyles.Number, tr, out result)) goto Normalize;
            if (decimal.TryParse(value, NumberStyles.Number, inv, out result)) goto Normalize;
            return false;

        Normalize:
            if (result > 24m)
            {
                if (result <= 99m) result /= 10m;
                else result /= 100m;
            }
            return true;
        }
    }

    public class PuantajOnayRequest
    {
        public int PersonelId { get; set; }
        public DateTime Tarih { get; set; }
        public int DuzenlenmisFm { get; set; }
        public string? Aciklama { get; set; }
        public string? CalismaTipi { get; set; }
        public decimal Saat { get; set; }
        public string? SaatText { get; set; }
    }

    public class PuantajRedRequest
    {
        public int PersonelId { get; set; }
        public DateTime Tarih { get; set; }
        public string? Aciklama { get; set; }
    }

    public class PuantajTopluOnayRequest
    {
        public int PersonelId { get; set; }
        public int Yil { get; set; }
        public int Ay { get; set; }
    }

    public class PuantajDuzenleRequest
    {
        public int PersonelId { get; set; }
        public DateTime Tarih { get; set; }
        public int DuzenlenmisFm { get; set; }
        public string? Aciklama { get; set; }
        public string? CalismaTipi { get; set; }
        public string? SaatText { get; set; }
    }

    public class PuantajDuzenleResponse
    {
        public string OnayDurumu { get; set; } = "";
        public string CalismaTipi { get; set; } = "";
        public decimal? Saat { get; set; }
        public string Aciklama { get; set; } = "";
        public int DuzenlenenFmDakika { get; set; }
    }

    public class PuantajEkKayitGunRequest
    {
        public int Gun { get; set; }
    }

    public class PuantajCokluSicilRequest
    {
        public int PersonelId { get; set; }
        public int Yil { get; set; }
        public int Ay { get; set; }
    }

    public class PuantajExportApiRequest
    {
        public int Yil { get; set; }
        public int Ay { get; set; }
    }

    public class PuantajLookupsDto
    {
        public int SelectedYil { get; set; }
        public int SelectedAy { get; set; }
        public int SelectedFirmaId { get; set; }
        public int? SelectedIsyeriId { get; set; }
        public List<PuantajFirmaItemDto> Firmalar { get; set; } = new();
        public List<PuantajIsyeriItemDto> Isyerleri { get; set; } = new();
        public List<PuantajPersonelItemDto> Personeller { get; set; } = new();
        public List<PuantajTipDTO> PuantajTipleri { get; set; } = new();
        public int EkKayitGun { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanApprove { get; set; }
        public bool CanDelete { get; set; }
        public bool CanExport { get; set; }
    }

    public class PuantajFirmaItemDto
    {
        public int FirmaId { get; set; }
        public string FirmaAdi { get; set; } = "";
    }

    public class PuantajIsyeriItemDto
    {
        public int IsyeriId { get; set; }
        public string Ad { get; set; } = "";
    }

    public class PuantajPersonelItemDto
    {
        public string PersonelId { get; set; } = "";
        public string AdSoyad { get; set; } = "";
    }
}
