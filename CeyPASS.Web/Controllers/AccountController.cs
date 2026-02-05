using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;

namespace CeyPASS.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IKullaniciService _kullaniciService;
        private readonly ISessionContext _sessionContext;
        private readonly ISifreService _sifreService;
        private readonly IEmailService _emailService;

        public AccountController(IKullaniciService kullaniciService, ISessionContext sessionContext, ISifreService sifreService, IEmailService emailService)
        {
            _kullaniciService = kullaniciService;
            _sessionContext = sessionContext;
            _sifreService = sifreService;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (_sessionContext.CurrentUser != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    ViewBag.Error = "Kullanıcı adı ve şifre gereklidir.";
                    return View();
                }

                var kullanici = _kullaniciService.GirisYap(username, password);
                
                if (kullanici == null)
                {
                    ViewBag.Error = "Kullanıcı adı veya şifre hatalı. (Kullanıcı bulunamadı)";
                    return View();
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
                        RolId = kullanici.RolId
                    };

                    _sessionContext.SetCurrentUser(authUser);
                    _sessionContext.AktifKullaniciId = kullanici.KullaniciId;
                    _sessionContext.AktifFirmaId = kullanici.FirmaId;
                    _sessionContext.AdSoyad = kullanici.AdSoyad;
                    _sessionContext.RolAdi = kullanici.RolTanimi;

                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Error = "Kullanıcı adı veya şifre hatalı.";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Bir hata oluştu: {ex.Message}";
                return View();
            }
        }

        [HttpGet]
        [HttpPost]
        public IActionResult Logout()
        {
            _sessionContext.Clear();
            return RedirectToAction("Login");
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
                ViewBag.Error = sonuc.HataMesaji ?? "İşlem başarısız.";
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
    }
}
