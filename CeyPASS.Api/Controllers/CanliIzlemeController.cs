using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using IAuthorizationService = CeyPASS.Business.Abstractions.IAuthorizationService;
using CeyPASS.Entities.Concrete;
using CeyPASS.Models;
using System.Data;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace CeyPASS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CanliIzlemeController : ControllerBase
    {
        private readonly ICanliIzlemeService _canliIzlemeService;
        private readonly IKisiHareketService _kisiHareketService;
        private readonly IKisiDetayService _kisiDetayService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;

        public CanliIzlemeController(
            ICanliIzlemeService canliIzlemeService,
            IKisiHareketService kisiHareketService,
            IKisiDetayService kisiDetayService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService)
        {
            _canliIzlemeService = canliIzlemeService;
            _kisiHareketService = kisiHareketService;
            _kisiDetayService = kisiDetayService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
        }

        public sealed class FirmaOption
        {
            public int Id { get; set; }
            public string Ad { get; set; } = "";
        }

        [AllowAnonymous]
        [HttpGet("firmalar")]
        public ActionResult<ApiResult<List<FirmaOption>>> GetFirmalar()
        {
            var dt = _canliIzlemeService.GetFirmalar();
            var list = new List<FirmaOption>();
            if (dt == null) return Ok(ApiResult<List<FirmaOption>>.Ok(list));

            bool hasId = dt.Columns.Contains("FirmaId");
            bool hasAd = dt.Columns.Contains("FirmaAdi");
            if (!hasId || !hasAd) return Ok(ApiResult<List<FirmaOption>>.Ok(list));

            foreach (DataRow r in dt.Rows)
            {
                int id = r["FirmaId"] == DBNull.Value ? 0 : Convert.ToInt32(r["FirmaId"]);
                string ad = r["FirmaAdi"] == DBNull.Value ? "" : r["FirmaAdi"].ToString() ?? "";
                if (id > 0) list.Add(new FirmaOption { Id = id, Ad = ad });
            }

            return Ok(ApiResult<List<FirmaOption>>.Ok(list));
        }

        [AllowAnonymous]
        [HttpGet("kullanicilar")]
        public ActionResult<ApiResult<List<string>>> GetKullanicilar([FromQuery] int firmaId)
        {
            if (firmaId <= 0) return BadRequest(ApiResult.Failure("Firma seçin."));
            var list = _canliIzlemeService.GetKullaniciAdlariByFirma(firmaId) ?? new List<string>();
            return Ok(ApiResult<List<string>>.Ok(list));
        }

        public sealed class CanliIzlemeLoginRequest
        {
            public int FirmaId { get; set; }
            public string? KullaniciAdi { get; set; }
            public string? Sifre { get; set; }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public ActionResult<ApiResult<object>> Login([FromBody] CanliIzlemeLoginRequest request)
        {
            if (request.FirmaId <= 0) return BadRequest(ApiResult.Failure("Firma seçin."));
            if (string.IsNullOrWhiteSpace(request.KullaniciAdi)) return BadRequest(ApiResult.Failure("Kullanıcı adı boş olamaz."));
            if (string.IsNullOrWhiteSpace(request.Sifre)) return BadRequest(ApiResult.Failure("Şifre boş olamaz."));

            var auth = _canliIzlemeService.Login(request.FirmaId, request.KullaniciAdi.Trim(), request.Sifre);
            if (auth == null) return Unauthorized(ApiResult.Failure("Hatalı kullanıcı adı/şifre veya bu firma için yetki yok."));

            var token = GenerateJwtToken(auth);
            return Ok(ApiResult<object>.Ok(new
            {
                token,
                expiration = DateTime.Now.AddMinutes(double.Parse(HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Jwt:DurationInMinutes"] ?? "1440")),
                user = auth
            }));
        }

        private string GenerateJwtToken(AuthUserDTO user)
        {
            var cfg = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var jwtKey = cfg["Jwt:Key"] ?? "";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.KullaniciAdi ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.KullaniciId.ToString()),
                new Claim("FirmaId", user.FirmaId.ToString()),
                new Claim("SicilNo", user.SicilNo ?? ""),
                new Claim("RolId", (user.RolId ?? 0).ToString()),
                new Claim(ClaimTypes.Role, user.Rol ?? "CanliIzleme"),
                // Canlı izleme token'ını, ana mobil uygulama token'ından ayırmak için.
                new Claim("AuthKind", "CanliIzleme")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(double.Parse(cfg["Jwt:DurationInMinutes"] ?? "1440"));

            var token = new JwtSecurityToken(
                cfg["Jwt:Issuer"],
                cfg["Jwt:Audience"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet("son-gecisler")]
        public ActionResult<ApiResult<List<dynamic>>> GetSonGecisler([FromQuery] int take = 10)
        {
            // Web'deki gibi: Canlı İzleme kendi login'i ile token alan kullanıcılar erişebilir.
            // Ana mobil login token'ı ile bu endpoint'ler açılmasın.
            var authKind = User?.FindFirst("AuthKind")?.Value;
            if (!_sessionContext.IsAdmin() && authKind != "CanliIzleme") return Forbid();
            if (!_sessionContext.AktifFirmaId.HasValue) return BadRequest(ApiResult.Failure("Firma bilgisi bulunamadı."));

            // Web CanliIzlemeController.GetLastPassesInternal ile aynı: YEMEKHANE → yemekhane geçişleri
            var rol = _sessionContext.RolAdi;
            var passes = IsYemekhaneRole(rol)
                ? _canliIzlemeService.GetLastPassesYemekhane(_sessionContext.AktifFirmaId.Value, take)
                : _canliIzlemeService.GetLastPasses(_sessionContext.AktifFirmaId.Value, take);
            var result = passes.Select(x => new
            {
                personelId = x.PersonelId,
                adSoyad = x.AdSoyad,
                departmanAdi = x.DepartmanAdi,
                unvan = x.Unvan,
                zaman = x.Zaman,
                terminalAdi = x.TerminalAdi,
                girisMi = x.GirisMi,
                fotoBase64 = (x.Foto != null && x.Foto.Length > 0) ? Convert.ToBase64String(x.Foto) : null
            }).Cast<dynamic>().ToList();

            return Ok(ApiResult<List<dynamic>>.Ok(result));
        }

        [HttpGet("son-hareketler")]
        public ActionResult<ApiResult<List<dynamic>>> GetSonHareketler([FromQuery] int take = 15)
        {
            var authKind = User?.FindFirst("AuthKind")?.Value;
            if (!_sessionContext.IsAdmin() && authKind != "CanliIzleme") return Forbid();
            if (!_sessionContext.AktifFirmaId.HasValue) return BadRequest(ApiResult.Failure("Firma bilgisi bulunamadı."));

            // Web GetLastMovesInternal: YEMEKHANE ve DANIŞMA değilse yemekhane hareket listesi
            var rol = _sessionContext.RolAdi;
            var moves = (IsYemekhaneRole(rol) && !IsDanismaRole(rol))
                ? _kisiHareketService.GetLastMovesByFirmaYemekhane(take, _sessionContext.AktifFirmaId.Value)
                : _kisiHareketService.GetLastMovesByFirma(take, _sessionContext.AktifFirmaId.Value);
            var result = moves.Select(x => new
            {
                tarih = x.Tarih,
                adSoyad = x.AdSoyad,
                departman = x.Departman,
                unvan = x.Unvan,
                cihazAdi = x.CihazAdi,
                personelId = x.PersonelId
            }).Cast<dynamic>().ToList();

            return Ok(ApiResult<List<dynamic>>.Ok(result));
        }

        [HttpGet("kisi-detay")]
        public ActionResult<ApiResult<object>> KisiDetay([FromQuery] int kisiId)
        {
            var authKind = User?.FindFirst("AuthKind")?.Value;
            if (!_sessionContext.IsAdmin() && authKind != "CanliIzleme") return Forbid();
            if (kisiId <= 0) return BadRequest(ApiResult.Failure("Kişi seçin."));

            var dto = _kisiDetayService.GetDetay(kisiId);
            if (dto == null) return NotFound(ApiResult.Failure("Kişi bulunamadı."));

            return Ok(ApiResult<object>.Ok(new
            {
                adSoyad = dto.AdSoyad,
                unvan = dto.Unvan,
                departman = dto.Departman,
                fotoBase64 = (dto.Foto != null && dto.Foto.Length > 0) ? Convert.ToBase64String(dto.Foto) : null
            }));
        }

        private static bool IsYemekhaneRole(string? rolAdi) =>
            string.Equals(rolAdi ?? string.Empty, "YEMEKHANE", StringComparison.OrdinalIgnoreCase);

        private static bool IsDanismaRole(string? rolAdi)
        {
            var r = rolAdi ?? "";
            return r.IndexOf("DANIŞMA", StringComparison.OrdinalIgnoreCase) >= 0
                   || r.IndexOf("DANISMA", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
