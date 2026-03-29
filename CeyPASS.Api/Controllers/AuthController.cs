using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Models;
using CeyPASS.Models.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CeyPASS.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IKullaniciService _kullaniciService;
        private readonly IKisiRepository _kisiRepository;
        private readonly IPersonelWebSifreRepository _personelWebSifreRepository;
        private readonly IConfiguration _configuration;

        public AuthController(
            IKullaniciService kullaniciService,
            IKisiRepository kisiRepository,
            IPersonelWebSifreRepository personelWebSifreRepository,
            IConfiguration configuration)
        {
            _kullaniciService = kullaniciService;
            _kisiRepository = kisiRepository;
            _personelWebSifreRepository = personelWebSifreRepository;
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
                
                // 2. Kurumsal değilse Personel Portalı Kontrolü
                if (kullanici == null)
                {
                    var kisi = _kisiRepository.GetByLoginIdentifier(request.Username);
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
