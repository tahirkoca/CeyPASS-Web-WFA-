using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CeyPASS.Entities.Concrete;
using CeyPASS.Business.Abstractions;
using IAuthorizationService = CeyPASS.Business.Abstractions.IAuthorizationService;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Models;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CeyPASS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ProfilController : ControllerBase
    {
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly IKisiQueryService _kisiQueryService;
        private readonly IIzinTalepService _izinTalepService;
        private readonly IAvansService _avansService;
        private readonly ISifreService _sifreService;
        private readonly IKisiIzinlerRepository _kisiIzinlerRepo;
        private readonly IKisiHareketService _kisiHareketService;
        private readonly IKisiEkraniLookUpService _lookupService;
        private readonly IPozisyonService _pozisyonService;
        private readonly IUstYetkiliRepository _ustYetkiliRepo;
        private readonly ICalismaSekliService _calismaSekliService;
        private const string PageName = "Profil";

        public ProfilController(
            ISessionContext sessionContext,
            IAuthorizationService authorizationService,
            IKisiQueryService kisiQueryService,
            IIzinTalepService izinTalepService,
            IAvansService avansService,
            ISifreService sifreService,
            IKisiIzinlerRepository kisiIzinlerRepo,
            IKisiHareketService kisiHareketService,
            IKisiEkraniLookUpService lookupService,
            IPozisyonService pozisyonService,
            IUstYetkiliRepository ustYetkiliRepo,
            ICalismaSekliService calismaSekliService)
        {
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
            _kisiQueryService = kisiQueryService;
            _izinTalepService = izinTalepService;
            _avansService = avansService;
            _sifreService = sifreService;
            _kisiIzinlerRepo = kisiIzinlerRepo;
            _kisiHareketService = kisiHareketService;
            _lookupService = lookupService;
            _pozisyonService = pozisyonService;
            _ustYetkiliRepo = ustYetkiliRepo;
            _calismaSekliService = calismaSekliService;
        }

        public sealed class ProfilDetailResponse
        {
            public string SicilNo { get; set; } = "";
            public KisiDetay Personel { get; set; } = null!;
            public string? FotografDataUrl { get; set; }
            public string? DepartmanAdi { get; set; }
            public string? PozisyonAdi { get; set; }
            public List<string> CalismaSekliAdlari { get; set; } = new();
            public bool IsSupervisor { get; set; }
            public bool HasPendingLeaves { get; set; }
            public int TotalPendingAdvances { get; set; }
            public bool? YemekHakkiVar { get; set; }
            public int? GunlukYemekAdedi { get; set; }
        }

        [HttpGet]
        public ActionResult<ApiResult<ProfilDetailResponse>> Get()
        {
            if (string.IsNullOrEmpty(_sessionContext.AktifSicilNo)) return Unauthorized();
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();

            var kisi = _kisiQueryService.GetKisiDetay(_sessionContext.AktifSicilNo);
            if (kisi == null) return NotFound(ApiResult.Failure("Personel bulunamadı."));

            var model = new ProfilDetailResponse
            {
                SicilNo = _sessionContext.AktifSicilNo,
                Personel = kisi,
                FotografDataUrl = ToImageDataUrl(kisi?.Fotograf),
                IsSupervisor = _izinTalepService.IsSupervisor(_sessionContext.AktifSicilNo),
                HasPendingLeaves = _izinTalepService.PersonelTalepleri(_sessionContext.AktifSicilNo).Any(x => x.UstYetkiliOnayDurumu == IzinOnayDurumu.Bekliyor),
                TotalPendingAdvances = _avansService.PersonelTalepleri(_sessionContext.AktifSicilNo).Count(x => x.Durum == AvansDurumu.Bekliyor),
                YemekHakkiVar = kisi?.YemekHakkiVar,
                GunlukYemekAdedi = kisi?.GunlukYemekAdedi
            };

            try
            {
                if (kisi?.DepartmanId != null)
                    model.DepartmanAdi = _lookupService.GetDepartmanlar().FirstOrDefault(x => x.Id == kisi.DepartmanId)?.Ad;
            }
            catch { }
            try
            {
                if (kisi?.PozisyonId != null)
                    model.PozisyonAdi = _pozisyonService.GetAll().FirstOrDefault(x => x.Id == kisi.PozisyonId)?.Ad;
            }
            catch { }
            try
            {
                model.CalismaSekliAdlari = ResolveCalismaSekliAdlari(kisi?.CalismaSekliCsv, _sessionContext.AktifFirmaId ?? 0);
            }
            catch { }

            return Ok(ApiResult<ProfilDetailResponse>.Ok(model));
        }

        private List<string> ResolveCalismaSekliAdlari(string? csv, int firmaId)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(csv)) return result;
            if (firmaId <= 0) return result;

            var ids = new List<int>();
            foreach (Match m in Regex.Matches(csv, @"\d+"))
            {
                if (int.TryParse(m.Value, out var id) && id > 0) ids.Add(id);
            }
            if (ids.Count == 0) return result;

            ids = ids.Distinct().ToList();
            var all = _calismaSekliService.GetAll(firmaId, includeGlobal: true) ?? new List<CalismaSekli>();
            var map = all.Where(x => x != null).ToDictionary(x => x.Id, x => x.Ad ?? "");
            foreach (var id in ids)
            {
                if (map.TryGetValue(id, out var ad) && !string.IsNullOrWhiteSpace(ad))
                    result.Add(ad.Trim());
            }
            return result;
        }

        private static string? ToImageDataUrl(byte[]? bytes)
        {
            if (bytes == null || bytes.Length == 0) return null;
            string mime = "image/jpeg";
            if (bytes.Length >= 8 &&
                bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 &&
                bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A)
            {
                mime = "image/png";
            }
            else if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            {
                mime = "image/jpeg";
            }
            return $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
        }

        public sealed class HareketlerimResponse
        {
            public List<KisiHareketListRow> Items { get; set; } = new();
            public int TotalCount { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
        }

        [HttpGet("hareketlerim")]
        public ActionResult<ApiResult<HareketlerimResponse>> Hareketlerim([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (string.IsNullOrEmpty(_sessionContext.AktifSicilNo)) return Unauthorized();
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            int firmaId = _sessionContext.AktifFirmaId ?? 0;
            if (firmaId == 0) return BadRequest(ApiResult.Failure("Firma bilgisi bulunamadı."));
            if (!int.TryParse(_sessionContext.AktifSicilNo, out var myId)) return BadRequest(ApiResult.Failure("Sicil bilgisi geçersiz."));

            var bas = DateTime.Today.AddDays(-7);
            var bit = DateTime.Now;

            int total;
            var items = _kisiHareketService.GetByPersonsPaged(
                new List<int> { myId },
                bas,
                bit,
                onlyAktif: true,
                onlyPasif: false,
                onlyYemekhane: false,
                firmaId: firmaId,
                page: page,
                pageSize: pageSize,
                out total
            ) ?? new List<KisiHareketListRow>();

            // Ensure newest-first ordering (web expectation: 30 -> 23)
            try
            {
                items = items.OrderByDescending(x => x.Tarih).ToList();
            }
            catch { }

            return Ok(ApiResult<HareketlerimResponse>.Ok(new HareketlerimResponse
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            }));
        }

        [HttpGet("izin-tipleri")]
        public ActionResult<ApiResult<List<IzinTip>>> IzinTipleri([FromServices] IIzinTipService izinTipService)
        {
            if (string.IsNullOrEmpty(_sessionContext.AktifSicilNo)) return Unauthorized();
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();
            var items = izinTipService.GetAktif();
            return Ok(ApiResult<List<IzinTip>>.Ok(items));
        }

        public sealed class IzinTalepRequest
        {
            public int IzinTipId { get; set; }
            public DateTime Baslangic { get; set; }
            public DateTime Bitis { get; set; }
            public bool SaatlikIzinMi { get; set; }
            public string? Aciklama { get; set; }
            public string? IzinAdres { get; set; }
            public string? TelefonNo { get; set; }
        }

        [HttpPost("izin-talep")]
        public ActionResult<ApiResult> IzinTalep([FromBody] IzinTalepRequest request)
        {
            if (string.IsNullOrEmpty(_sessionContext.AktifSicilNo)) return Unauthorized();
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();

            if (_sessionContext.AktifFirmaId is null) return BadRequest(ApiResult.Failure("Firma bilgisi bulunamadı."));

            var talep = new IzinTalep
            {
                PersonelId = _sessionContext.AktifSicilNo,
                FirmaId = _sessionContext.AktifFirmaId.Value,
                IzinTipId = request.IzinTipId,
                Baslangic = request.Baslangic,
                Bitis = request.Bitis,
                SaatlikIzinMi = request.SaatlikIzinMi,
                Aciklama = request.Aciklama,
                IzinAdres = request.IzinAdres,
                TelefonNo = request.TelefonNo
            };

            _izinTalepService.TalepOlustur(talep, _sessionContext.AktifKullaniciId ?? 0);
            return Ok(ApiResult.Ok("İzin talebiniz alındı."));
        }

        public sealed class KullanimImzaRequest
        {
            public int TalepId { get; set; }
        }

        [HttpPost("kullanim-imza")]
        public ActionResult<ApiResult> KullanimImza([FromBody] KullanimImzaRequest request)
        {
            if (string.IsNullOrEmpty(_sessionContext.AktifSicilNo)) return Unauthorized();
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();

            var t = _izinTalepService.PersonelTalepleri(_sessionContext.AktifSicilNo).FirstOrDefault(x => x.TalepId == request.TalepId);
            if (t == null) return NotFound(ApiResult.Failure("Talep bulunamadı."));

            var ok = _izinTalepService.KullanimImzaAt(request.TalepId, _sessionContext.AktifKullaniciId ?? 0);
            return ok ? Ok(ApiResult.Ok("İzni kullanan imzası kaydedildi.")) : BadRequest(ApiResult.Failure("İmza kaydedilemedi (İK henüz açmamış olabilir)."));
        }

        public sealed class AvansTalepRequest
        {
            public decimal Miktar { get; set; }
            public string? Aciklama { get; set; }
        }

        [HttpPost("avans-talep")]
        public ActionResult<ApiResult<int>> AvansTalep([FromBody] AvansTalepRequest request)
        {
            if (string.IsNullOrEmpty(_sessionContext.AktifSicilNo)) return Unauthorized();
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();

            int id = _avansService.TalepOlustur(_sessionContext.AktifSicilNo, request.Miktar, request.Aciklama);
            return Ok(ApiResult<int>.Ok(id, "Avans talebiniz alındı."));
        }

        public sealed class AvansUpdateRequest
        {
            public int AvansId { get; set; }
            public decimal Miktar { get; set; }
            public string? Aciklama { get; set; }
        }

        [HttpPost("avans-guncelle")]
        public ActionResult<ApiResult> AvansGuncelle([FromBody] AvansUpdateRequest request)
        {
            if (string.IsNullOrEmpty(_sessionContext.AktifSicilNo)) return Unauthorized();
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();

            var t = _avansService.PersonelTalepleri(_sessionContext.AktifSicilNo).FirstOrDefault(x => x.AvansId == request.AvansId);
            if (t == null || t.Durum != AvansDurumu.Bekliyor) return BadRequest(ApiResult.Failure("Güncellenemez bir talep veya yetkisiz işlem."));

            var ok = _avansService.Guncelle(request.AvansId, request.Miktar, request.Aciklama);
            return ok ? Ok(ApiResult.Ok("Avans talebiniz güncellendi.")) : BadRequest(ApiResult.Failure("Talep güncellenirken bir hata oluştu."));
        }

        public sealed class AvansCancelRequest
        {
            public int AvansId { get; set; }
        }

        [HttpPost("avans-iptal")]
        public ActionResult<ApiResult> AvansIptal([FromBody] AvansCancelRequest request)
        {
            if (string.IsNullOrEmpty(_sessionContext.AktifSicilNo)) return Unauthorized();
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();

            var t = _avansService.PersonelTalepleri(_sessionContext.AktifSicilNo).FirstOrDefault(x => x.AvansId == request.AvansId);
            if (t == null || t.Durum != AvansDurumu.Bekliyor) return BadRequest(ApiResult.Failure("İptal edilemez bir talep veya yetkisiz işlem."));

            var ok = _avansService.IptalEt(request.AvansId);
            return ok ? Ok(ApiResult.Ok("Avans talebiniz iptal edildi.")) : BadRequest(ApiResult.Failure("Talep iptal edilirken bir hata oluştu."));
        }

        [HttpPost("sifre-degistir")]
        public ActionResult<ApiResult> ChangePassword([FromBody] SifreDegistirRequest request)
        {
            if (string.IsNullOrEmpty(_sessionContext.AktifSicilNo)) return Unauthorized();

            bool success = _sifreService.SifreSifirlaManuel(_sessionContext.AktifSicilNo, request.YeniSifre);
            return success ? Ok(ApiResult.Ok("Şifreniz başarıyla güncellendi.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }

        [HttpGet("amir-onay-bekleyenler")]
        public ActionResult<ApiResult<List<IzinTalep>>> GetAmirBekleyenler()
        {
            if (string.IsNullOrEmpty(_sessionContext.AktifSicilNo)) return Unauthorized();
            if (!_izinTalepService.IsSupervisor(_sessionContext.AktifSicilNo)) return Forbid();

            var items = _izinTalepService.UstYetkiliBekleyenler(_sessionContext.AktifSicilNo);
            return Ok(ApiResult<List<IzinTalep>>.Ok(items));
        }

        [HttpGet("bagli-personellerim")]
        public ActionResult<ApiResult<List<KisiDetay>>> GetBagliPersonellerim()
        {
            if (string.IsNullOrEmpty(_sessionContext.AktifSicilNo)) return Unauthorized();
            if (!_izinTalepService.IsSupervisor(_sessionContext.AktifSicilNo)) return Forbid();

            var bagliIds = _ustYetkiliRepo.GetSubordinates(_sessionContext.AktifSicilNo);
            var bagliPersoneller = new List<KisiDetay>();
            foreach (var bid in bagliIds)
            {
                try
                {
                    var k = _kisiQueryService.GetKisiDetay(bid);
                    if (k != null) bagliPersoneller.Add(k);
                }
                catch { }
            }

            return Ok(ApiResult<List<KisiDetay>>.Ok(bagliPersoneller));
        }

        public sealed class SubordinateResetPasswordRequest
        {
            public string PersonelId { get; set; } = null!;
            public string YeniSifre { get; set; } = null!;
        }

        [HttpPost("subordinate-sifre-sifirla")]
        public ActionResult<ApiResult> ResetSubordinatePassword([FromBody] SubordinateResetPasswordRequest request)
        {
            if (string.IsNullOrEmpty(_sessionContext.AktifSicilNo)) return Unauthorized();
            if (!_izinTalepService.IsSupervisor(_sessionContext.AktifSicilNo)) return Forbid();

            var subordinates = _ustYetkiliRepo.GetSubordinates(_sessionContext.AktifSicilNo);
            if (!subordinates.Contains(request.PersonelId)) return Forbid();

            var ok = _sifreService.SifreSifirlaManuel(request.PersonelId, request.YeniSifre);
            return ok ? Ok(ApiResult.Ok("Şifre başarıyla güncellendi.")) : BadRequest(ApiResult.Failure("Şifre güncellenemedi."));
        }

        [HttpPost("amir-onayla")]
        public ActionResult<ApiResult> SupervisorApprove([FromBody] AmirOnayRequest request)
        {
            if (string.IsNullOrEmpty(_sessionContext.AktifSicilNo)) return Unauthorized();

            bool success = _izinTalepService.UstYetkiliOnayla(request.TalepId, _sessionContext.AktifSicilNo, request.Aciklama);
            return success ? Ok(ApiResult.Ok("Talep amir tarafında onaylandı.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }

        [HttpPost("amir-reddet")]
        public ActionResult<ApiResult> SupervisorReject([FromBody] AmirOnayRequest request)
        {
            if (string.IsNullOrEmpty(_sessionContext.AktifSicilNo)) return Unauthorized();

            bool success = _izinTalepService.UstYetkiliReddet(request.TalepId, _sessionContext.AktifSicilNo, request.Aciklama);
            return success ? Ok(ApiResult.Ok("Talep amir tarafında reddedildi.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }

        public sealed class IzinlerimResponse
        {
            public List<IzinTalep> Talepler { get; set; } = new();
            public List<KisiIzinGecmisRow> OnayliIzinler { get; set; } = new();
        }

        public sealed class KisiIzinGecmisRow
        {
            public int KisiIzinId { get; set; }
            public DateTime Baslangic { get; set; }
            public DateTime Bitis { get; set; }
            public decimal SureSaat { get; set; }
            public string? Aciklama { get; set; }
            public DateTime? IslenmeTarihi { get; set; }
            public bool SaatlikIzinMi { get; set; }
        }

        [HttpGet("izinlerim")]
        public ActionResult<ApiResult<IzinlerimResponse>> Izinlerim()
        {
            if (string.IsNullOrEmpty(_sessionContext.AktifSicilNo)) return Unauthorized();
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();

            var res = new IzinlerimResponse
            {
                Talepler = _izinTalepService.PersonelTalepleri(_sessionContext.AktifSicilNo) ?? new List<IzinTalep>()
            };

            try
            {
                DataTable dt = _kisiIzinlerRepo.GetByPerson(_sessionContext.AktifSicilNo);
                foreach (DataRow r in dt.Rows)
                {
                    res.OnayliIzinler.Add(new KisiIzinGecmisRow
                    {
                        KisiIzinId = ToInt(r["KisiIzinId"]),
                        Baslangic = ToDateTime(r["İzin Başlangıcı"]),
                        Bitis = ToDateTime(r["İzin Bitişi"]),
                        SureSaat = ToDecimal(r["Süre(Saat)"]),
                        Aciklama = r["Açıklama"]?.ToString(),
                        IslenmeTarihi = ToNullableDateTime(r["İşlenme Tarihi"]),
                        SaatlikIzinMi = string.Equals(r["Saatlik İzin"]?.ToString(), "EVET", StringComparison.OrdinalIgnoreCase)
                    });
                }
            }
            catch { }

            return Ok(ApiResult<IzinlerimResponse>.Ok(res));
        }

        [HttpGet("avanslarim")]
        public ActionResult<ApiResult<List<AvansTalep>>> Avanslarim()
        {
            if (string.IsNullOrEmpty(_sessionContext.AktifSicilNo)) return Unauthorized();
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();

            var items = _avansService.PersonelTalepleri(_sessionContext.AktifSicilNo) ?? new List<AvansTalep>();
            return Ok(ApiResult<List<AvansTalep>>.Ok(items));
        }

        private static int ToInt(object? v)
        {
            if (v == null || v == DBNull.Value) return 0;
            if (v is int i) return i;
            if (int.TryParse(v.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var x)) return x;
            return 0;
        }

        private static DateTime ToDateTime(object? v)
        {
            if (v == null || v == DBNull.Value) return DateTime.MinValue;
            if (v is DateTime dt) return dt;
            if (DateTime.TryParse(v.ToString(), CultureInfo.GetCultureInfo("tr-TR"), DateTimeStyles.AssumeLocal, out var x)) return x;
            if (DateTime.TryParse(v.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out x)) return x;
            return DateTime.MinValue;
        }

        private static DateTime? ToNullableDateTime(object? v)
        {
            if (v == null || v == DBNull.Value) return null;
            var dt = ToDateTime(v);
            return dt == DateTime.MinValue ? null : dt;
        }

        private static decimal ToDecimal(object? v)
        {
            if (v == null || v == DBNull.Value) return 0m;
            if (v is decimal d) return d;
            if (decimal.TryParse(v.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var x)) return x;
            if (decimal.TryParse(v.ToString(), NumberStyles.Number, CultureInfo.GetCultureInfo("tr-TR"), out x)) return x;
            return 0m;
        }
    }

    public class ProfilModel
    {
        public KisiDetay Personel { get; set; } = null!;
        public bool IsSupervisor { get; set; }
        public bool HasPendingLeaves { get; set; }
        public int TotalPendingAdvances { get; set; }
    }

    public class SifreDegistirRequest
    {
        public string EskiSifre { get; set; } = null!;
        public string YeniSifre { get; set; } = null!;
    }

    public class AmirOnayRequest
    {
        public int TalepId { get; set; }
        public string? Aciklama { get; set; }
    }
}
