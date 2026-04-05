using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Models;
using CeyPASS.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using IAuthorizationService = CeyPASS.Business.Abstractions.IAuthorizationService;

namespace CeyPASS.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IKullaniciService _kullaniciService;
        private readonly IKisiRepository _kisiRepository;
        private readonly IPersonelWebSifreRepository _personelWebSifreRepository;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISessionContext _sessionContext;
        private readonly IIzinTalepService _izinTalepService;
        private readonly ISifreService _sifreService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AuthController(
            IKullaniciService kullaniciService,
            IKisiRepository kisiRepository,
            IPersonelWebSifreRepository personelWebSifreRepository,
            IAuthorizationService authorizationService,
            ISessionContext sessionContext,
            IIzinTalepService izinTalepService,
            ISifreService sifreService,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _kullaniciService = kullaniciService;
            _kisiRepository = kisiRepository;
            _personelWebSifreRepository = personelWebSifreRepository;
            _authorizationService = authorizationService;
            _sessionContext = sessionContext;
            _izinTalepService = izinTalepService;
            _sifreService = sifreService;
            _emailService = emailService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public ActionResult<ApiResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(ApiResult.Failure("Kullanıcı adı ve şifre gereklidir."));
                }

                // 1. Kurumsal Kullanıcı Kontrolü
                var kullanici = _kullaniciService.GirisYap(request.Username, request.Password);
                
                // 2. Kurumsal değilse: (Web ile birebir) username bir kimlik (TC/Sicil/Email) ise kurumsal hesaba düşmeyi dene
                if (kullanici == null)
                {
                    var kisiForCorp = _kisiRepository.GetByLoginIdentifier(request.Username);
                    if (kisiForCorp != null)
                    {
                        var corpAccount = _kullaniciService.GetByPersonelId(kisiForCorp.PersonelId);
                        if (corpAccount != null && corpAccount.Sifre == request.Password)
                        {
                            return Success(new CeyPASS.Entities.Concrete.AuthUserDTO
                            {
                                KullaniciId = corpAccount.KullaniciId,
                                FirmaId = corpAccount.FirmaId ?? 0,
                                KullaniciAdi = corpAccount.KullaniciAdi,
                                AdSoyad = corpAccount.AdSoyad,
                                Rol = corpAccount.RolTanimi,
                                RolId = corpAccount.RolId,
                                SicilNo = corpAccount.PersonelId?.ToString()
                            });
                        }
                    }

                    // 3. Kurumsal değilse Personel Portalı Kontrolü (Web ile aynı fallback)
                    var kisi = kisiForCorp ?? _kisiRepository.GetByLoginIdentifier(request.Username);
                    if (kisi != null && _personelWebSifreRepository.Dogrula(kisi.PersonelId, request.Password))
                    {
                        return Success(new CeyPASS.Entities.Concrete.AuthUserDTO
                        {
                            KullaniciId = 0,
                            FirmaId = kisi.FirmaId,
                            KullaniciAdi = kisi.PersonelId,
                            AdSoyad = $"{kisi.Ad} {kisi.Soyad}",
                            Rol = "Personel",
                            RolId = 5,
                            SicilNo = kisi.PersonelId
                        });
                    }
                }
                else
                {
                    return Success(new CeyPASS.Entities.Concrete.AuthUserDTO
                    {
                        KullaniciId = kullanici.KullaniciId,
                        FirmaId = kullanici.FirmaId ?? 0,
                        KullaniciAdi = kullanici.KullaniciAdi,
                        AdSoyad = kullanici.AdSoyad,
                        Rol = kullanici.RolTanimi,
                        RolId = kullanici.RolId,
                        SicilNo = kullanici.PersonelId?.ToString()
                    });
                }

                return Unauthorized(ApiResult.Failure("Hatalı kullanıcı adı veya şifre."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult.Failure($"Giriş hatası: {ex.Message}"));
            }
        }

        public sealed class AbilitiesResponse
        {
            public Dictionary<string, bool> View { get; set; } = new();
            public Dictionary<string, Dictionary<string, bool>> Actions { get; set; } = new();
            public bool IsSupervisor { get; set; }
            public int? RolId { get; set; }
            public string? RolAdi { get; set; }
        }

        [Authorize]
        [HttpGet("abilities")]
        public ActionResult<ApiResult<AbilitiesResponse>> GetAbilities()
        {
            var pages = new[]
            {
                "Dashboard",
                "Profil",
                "IzinTalepleri",
                "Avans",
                "Personeller",
                "KisiHareketler",
                "Izinler",
                "AylikPuantaj",
                "Raporlar",
                "Firmalar",
                "Isyerler",
                "Departmanlar",
                "Pozisyonlar",
                "Vardiyalar",
                "CalismaStatuleri",
                "Cihazlar",
                "ResmiTatiller",
            };

            var view = new Dictionary<string, bool>();
            foreach (var p in pages)
            {
                view[p] = _authorizationService.ViewAbility(p);
            }

            // Mobile needs action-level permissions (create/update/delete) per page.
            var actions = new Dictionary<string, Dictionary<string, bool>>();
            foreach (var p in pages)
            {
                actions[p] = new Dictionary<string, bool>
                {
                    ["Create"] = _authorizationService.Can(p, YetkiTipleri.Create),
                    ["Update"] = _authorizationService.Can(p, YetkiTipleri.Update),
                    ["Delete"] = _authorizationService.Can(p, YetkiTipleri.Delete),
                    ["Export"] = _authorizationService.Can(p, YetkiTipleri.Export),
                    ["Approve"] = _authorizationService.Can(p, YetkiTipleri.Approve),
                };
            }

            // Web'deki gibi: sicilNo varsa supervisor kontrolü
            bool isSupervisor = false;
            if (!string.IsNullOrWhiteSpace(_sessionContext.AktifSicilNo))
            {
                try { isSupervisor = _izinTalepService.IsSupervisor(_sessionContext.AktifSicilNo); } catch { }
            }

            return Ok(ApiResult<AbilitiesResponse>.Ok(new AbilitiesResponse
            {
                View = view,
                Actions = actions,
                IsSupervisor = isSupervisor,
                RolId = _sessionContext.RolId,
                RolAdi = _sessionContext.RolAdi
            }));
        }

        public sealed class ForgotPasswordStartRequest
        {
            public string? Username { get; set; }
        }

        [HttpPost("forgot-password")]
        public ActionResult<ApiResult<object>> ForgotPassword([FromBody] ForgotPasswordStartRequest request)
        {
            var username = (request.Username ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(username))
                return BadRequest(ApiResult.Failure("Kullanıcı adını girin."));

            var sonuc = _sifreService.SifreSifirlamaBaslat(username);
            if (!sonuc.Basarili)
            {
                var msg = sonuc.HataMesaji ?? "İşlem başarısız.";
                if (msg.StartsWith("NO_EMAIL|")) msg = msg.Replace("NO_EMAIL|", "");
                return BadRequest(ApiResult.Failure(msg));
            }

            var maskedEmail = _emailService.MaskEmail(sonuc.Email ?? string.Empty);
            return Ok(ApiResult<object>.Ok(new { maskedEmail }));
        }

        public sealed class ForgotPasswordConfirmRequest
        {
            public string? Username { get; set; }
            public string? Kod { get; set; }
            public string? YeniSifre { get; set; }
            public string? YeniSifreTekrar { get; set; }
        }

        [HttpPost("forgot-password/confirm")]
        public ActionResult<ApiResult<object>> ForgotPasswordConfirm([FromBody] ForgotPasswordConfirmRequest request)
        {
            var username = (request.Username ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(username))
                return BadRequest(ApiResult.Failure("Kullanıcı adını girin."));

            var sonuc = _sifreService.SifreSifirlamaTamamla(
                username,
                request.Kod ?? string.Empty,
                request.YeniSifre ?? string.Empty,
                request.YeniSifreTekrar ?? string.Empty
            );

            if (!sonuc.Basarili)
                return BadRequest(ApiResult.Failure(sonuc.HataMesaji ?? "Şifre güncellenemedi."));

            return Ok(ApiResult<object>.Ok(new { ok = true }));
        }

        private ActionResult Success(CeyPASS.Entities.Concrete.AuthUserDTO user)
        {
            var token = GenerateJwtToken(user);
            return Ok(ApiResult<LoginResponse>.Ok(new LoginResponse
            {
                Token = token,
                Expiration = DateTime.Now.AddMinutes(double.Parse(_configuration["Jwt:DurationInMinutes"] ?? "1440")),
                User = new CeyPASS.Models.Auth.AuthUserDTO // Corrected namespace
                {
                    KullaniciId = user.KullaniciId,
                    FirmaId = user.FirmaId,
                    KullaniciAdi = user.KullaniciAdi,
                    AdSoyad = user.AdSoyad,
                    Rol = user.Rol,
                    RolId = user.RolId,
                    SicilNo = user.SicilNo
                }
            }));
        }

        private string GenerateJwtToken(CeyPASS.Entities.Concrete.AuthUserDTO user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.KullaniciAdi ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.KullaniciId.ToString()),
                new Claim("FirmaId", user.FirmaId.ToString()),
                new Claim("SicilNo", user.SicilNo ?? ""),
                new Claim("RolId", user.RolId.ToString() ?? ""),
                new Claim(ClaimTypes.Role, user.Rol ?? "Personel")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? ""));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(double.Parse(_configuration["Jwt:DurationInMinutes"] ?? "1440"));

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
