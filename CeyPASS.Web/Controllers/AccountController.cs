using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;

namespace CeyPASS.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IKullaniciService _kullaniciService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISifreService _sifreService;
        private readonly IEmailService _emailService;
        private readonly IKisiRepository _kisiRepository;
        private readonly IPersonelWebSifreRepository _personelWebSifreRepository;
        private readonly IDataProtector _dataProtector;

        public AccountController(
            IKullaniciService kullaniciService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService,
            ISifreService sifreService,
            IEmailService emailService,
            IKisiRepository kisiRepository,
            IPersonelWebSifreRepository personelWebSifreRepository,
            IDataProtectionProvider dataProtectionProvider)
        {
            _kullaniciService = kullaniciService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
            _sifreService = sifreService;
            _emailService = emailService;
            _kisiRepository = kisiRepository;
            _personelWebSifreRepository = personelWebSifreRepository;
            _dataProtector = dataProtectionProvider.CreateProtector("CeyPASS.RememberMeAuth");
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (_sessionContext.CurrentUser != null)
            {
                if (_authorizationService.ViewAbility("Dashboard"))
                    return RedirectToAction("Index", "Home");

                if (!string.IsNullOrEmpty(_sessionContext.AktifSicilNo))
                    return RedirectToAction("Index", "Profil");

                _sessionContext.Clear();
            }

            // Auto-login from Remember Me cookie
            var rmCookie = Request.Cookies["CeyPASS_RM"];
            if (!string.IsNullOrEmpty(rmCookie))
            {
                try
                {
                    var decrypted = _dataProtector.Unprotect(rmCookie);
                    var parts = decrypted.Split('|', 2);
                    if (parts.Length == 2)
                    {
                        var result = ProcessLogin(parts[0], parts[1], isToken: true);
                        if (result != null) return result;
                    }
                }
                catch { /* ignore decode errors */ }
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password, bool rememberMe = false)
        {
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    ViewBag.Error = "Kullanıcı adı ve şifre gereklidir.";
                    return View();
                }

                var successResult = ProcessLogin(username, password);
                
                if (successResult != null)
                {
                    if (rememberMe)
                    {
                        // Secure signature instead of plaintext password
                        var signature = GeneratePasswordSignature(password);
                        var payload = _dataProtector.Protect($"{username}|{signature}");
                        var cookieOptions = new CookieOptions 
                        { 
                            Expires = DateTime.Now.AddDays(30), 
                            HttpOnly = true, 
                            Secure = Request.IsHttps 
                        };
                        Response.Cookies.Append("CeyPASS_RM", payload, cookieOptions);
                    }
                    return successResult;
                }

                ViewBag.Error = "Girdiğiniz bilgilere ait bir kayıt bulunamadı veya şifre hatalı.";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Bir hata oluştu: {ex.Message}";
                return View();
            }
        }
        private IActionResult ProcessLogin(string username, string password, bool isToken = false)
        {
            // 1. Try direct corporate login (KullaniciAdi)
            Kullanici? kullanici = null;
            
            if (isToken)
            {
                // In token mode, 'password' parameter is actually the SIGNATURE.
                // We need to fetch the current password from DB and verify signature matches.
                // NOTE: This is safe because DB stores plaintext passwords (the current constraint).
                var userFromDb = _kullaniciService.GetByUserName(username);
                if (userFromDb != null && GeneratePasswordSignature(userFromDb.Sifre) == password)
                {
                    kullanici = userFromDb;
                }
            }
            else
            {
                kullanici = _kullaniciService.GirisYap(username, password);
            }
            
            // 2. If failed, check if 'username' is an identifier (TC/Sicil/Email) and has a corporate account
            if (kullanici == null && !isToken) // Token login only supports primary username for now for simplicity
            {
                var kisiForCorp = _kisiRepository.GetByLoginIdentifier(username);
                if (kisiForCorp != null)
                {
                    var corpAccount = _kullaniciService.GetByPersonelId(kisiForCorp.PersonelId);
                    if (corpAccount != null && corpAccount.Sifre == password) // Manual password check for alternate identifier
                    {
                        kullanici = corpAccount;
                    }
                }
            }

            if (kullanici != null)
            {
                var authUser = new AuthUserDTO
                {
                    KullaniciId = kullanici.KullaniciId,
                    FirmaId = kullanici.FirmaId ?? 0,
                    KullaniciAdi = kullanici.KullaniciAdi,
                    AdSoyad = kullanici.AdSoyad,
                    Rol = kullanici.RolTanimi,
                    RolId = kullanici.RolId,
                    SicilNo = kullanici.PersonelId?.ToString()
                };

                _sessionContext.SetCurrentUser(authUser);
                _sessionContext.AktifKullaniciId = kullanici.KullaniciId;
                _sessionContext.AktifFirmaId = kullanici.FirmaId;
                _sessionContext.AktifSicilNo = authUser.SicilNo;
                _sessionContext.AdSoyad = kullanici.AdSoyad;
                _sessionContext.RolAdi = kullanici.RolTanimi;

                if (_authorizationService.ViewAbility("Dashboard"))
                    return RedirectToAction("Index", "Home");

                if (kullanici.PersonelId.HasValue)
                    return RedirectToAction("Index", "Profil");

                return RedirectToAction("Index", "Home");
            }

            // 3. Check Personnel Portal (Kisiler + WebSifreler Tablosu)
            var kisi = _kisiRepository.GetByLoginIdentifier(username);
            if (kisi != null)
            {
                bool passValid = false;
                if (isToken)
                {
                    // For personnel, verify signature against their web password
                    var currentWebPass = _personelWebSifreRepository.GetSifreById(kisi.PersonelId);
                    passValid = !string.IsNullOrEmpty(currentWebPass) && GeneratePasswordSignature(currentWebPass) == password;
                }
                else
                {
                    passValid = _personelWebSifreRepository.Dogrula(kisi.PersonelId, password);
                }

                if (passValid)
                {
                    var authUser = new AuthUserDTO
                    {
                        KullaniciId = 0, // Personnel don't have Admin KullaniciId
                        FirmaId = kisi.FirmaId,
                        KullaniciAdi = kisi.PersonelId,
                        AdSoyad = $"{kisi.Ad} {kisi.Soyad}",
                        Rol = "Personel",
                        RolId = 5,
                        SicilNo = kisi.PersonelId
                    };

                    _sessionContext.SetCurrentUser(authUser);
                    _sessionContext.AktifKullaniciId = null; 
                    _sessionContext.AktifFirmaId = kisi.FirmaId;
                    _sessionContext.AktifSicilNo = authUser.SicilNo;
                    _sessionContext.AdSoyad = authUser.AdSoyad;
                    _sessionContext.RolAdi = authUser.Rol;

                    return RedirectToAction("Index", "Profil");
                }
            }

            return null;
        }

        [HttpGet]
        [HttpPost]
        public IActionResult Logout()
        {
            _sessionContext.Clear();
            return RedirectToAction("Login");
        }

        

        private string GeneratePasswordSignature(string rawPassword)
        {
            // Simple HMAC-like signature based on raw password and a fixed server key (or part of data protector)
            // This prevents storing the actual password in the cookie.
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(rawPassword + "CeyPASS_INTERNAL_SALT_2024");
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (_sessionContext.CurrentUser != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(string username)
        {
            if (_sessionContext.CurrentUser != null)
                return RedirectToAction("Index", "Home");

            username = (username ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(username))
            {
                ViewBag.Error = "Kullanıcı adını girin.";
                return View();
            }

            var sonuc = _sifreService.SifreSifirlamaBaslat(username);
            if (!sonuc.Basarili)
            {
                ViewBag.Username = username;
                if (sonuc.HataMesaji != null && sonuc.HataMesaji.StartsWith("NO_EMAIL|"))
                {
                    ViewBag.NoEmail = true;
                    ViewBag.Error = sonuc.HataMesaji.Replace("NO_EMAIL|", "");
                }
                else
                {
                    ViewBag.Error = sonuc.HataMesaji ?? "İşlem başarısız.";
                }
                return View();
            }

            var maskedEmail = _emailService.MaskEmail(sonuc.Email ?? string.Empty);
            TempData["ForgotSuccess"] = $"Doğrulama kodu {maskedEmail} adresine gönderildi.";
            return RedirectToAction("ForgotPasswordConfirm", new { username });
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirm(string username)
        {
            if (_sessionContext.CurrentUser != null)
                return RedirectToAction("Index", "Home");
            if (string.IsNullOrWhiteSpace(username))
                return RedirectToAction("ForgotPassword");
            ViewBag.Username = username;
            ViewBag.ForgotSuccess = TempData["ForgotSuccess"];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPasswordConfirm(string username, string kod, string yeniSifre, string yeniSifreTekrar)
        {
            if (_sessionContext.CurrentUser != null)
                return RedirectToAction("Index", "Home");

            username = (username ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("ForgotPassword");

            var sonuc = _sifreService.SifreSifirlamaTamamla(username, kod ?? string.Empty, yeniSifre ?? string.Empty, yeniSifreTekrar ?? string.Empty);
            if (!sonuc.Basarili)
            {
                ViewBag.Username = username;
                ViewBag.Error = sonuc.HataMesaji ?? "Şifre güncellenemedi.";
                return View();
            }

            TempData["LoginSuccess"] = "Şifreniz başarıyla güncellendi. Yeni şifrenizle giriş yapabilirsiniz.";
            return RedirectToAction("Login");
        }
        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (_sessionContext.CurrentUser == null)
                return RedirectToAction("Login");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string mevcutSifre, string yeniSifre, string yeniSifreTekrar)
        {
            if (_sessionContext.CurrentUser == null)
                return RedirectToAction("Login");

            if (string.IsNullOrWhiteSpace(yeniSifre) || yeniSifre.Length < 6)
            {
                ViewBag.Error = "Yeni şifre en az 6 karakter olmalıdır.";
                return View();
            }

            if (yeniSifre != yeniSifreTekrar)
            {
                ViewBag.Error = "Yeni şifreler birbiriyle uyuşmuyor.";
                return View();
            }

            // Verify current password first
            string loginName = _sessionContext.CurrentUser.KullaniciAdi;
            bool isCorporate = _sessionContext.AktifKullaniciId.HasValue;
            bool currentValid = false;

            if (isCorporate)
            {
                var corpUser = _kullaniciService.GirisYap(loginName, mevcutSifre);
                currentValid = corpUser != null;
            }
            else
            {
                currentValid = _personelWebSifreRepository.Dogrula(loginName, mevcutSifre);
            }

            if (!currentValid)
            {
                ViewBag.Error = "Mevcut şifreniz hatalı.";
                return View();
            }

            bool ok = _sifreService.SifreyiGuncelle(loginName, yeniSifre, isCorporate);
            if (ok)
            {
                TempData["PasswordSuccess"] = "Şifreniz başarıyla değiştirildi.";
                return RedirectToAction("ChangePassword");
            }

            ViewBag.Error = "Şifre güncellenirken bir hata oluştu.";
            return View();
        }
    }
}
