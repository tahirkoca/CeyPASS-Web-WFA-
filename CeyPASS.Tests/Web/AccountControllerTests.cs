using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Web.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace CeyPASS.Tests.Web
{
    public class AccountControllerTests
    {
        private readonly Mock<IKullaniciService> _kullaniciMock = new();
        private readonly Mock<ISessionContext> _sessionMock = new();
        private readonly Mock<ISifreService> _sifreMock = new();
        private readonly Mock<IEmailService> _emailMock = new();
        private readonly AccountController _sut;

        public AccountControllerTests()
        {
            _sut = new AccountController(
                _kullaniciMock.Object,
                _sessionMock.Object,
                _sifreMock.Object,
                _emailMock.Object);

            var httpContext = new DefaultHttpContext();
            _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _sut.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        }

        // ─── Login GET ────────────────────────────────────────────────────────

        [Fact]
        public void Login_GET_OturumAciksa_HomeaYonlendirir()
        {
            _sessionMock.Setup(s => s.CurrentUser).Returns(new AuthUserDTO { KullaniciAdi = "admin" });

            var sonuc = _sut.Login();

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Home");
        }

        [Fact]
        public void Login_GET_OturumYoksa_ViewDoner()
        {
            _sessionMock.Setup(s => s.CurrentUser).Returns((AuthUserDTO)null);
            _kullaniciMock.Setup(k => k.GetTumKullaniciAdlari()).Returns(new List<string>());

            var sonuc = _sut.Login();

            sonuc.Should().BeOfType<ViewResult>();
        }

        // ─── Login POST ───────────────────────────────────────────────────────

        [Fact]
        public void Login_POST_BosKimlik_HataGosterir()
        {
            var sonuc = _sut.Login("", "");

            sonuc.Should().BeOfType<ViewResult>();
            ((string)_sut.ViewBag.Error).Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Login_POST_YanlisKimlik_HataGosterir()
        {
            _kullaniciMock.Setup(k => k.GirisYap("user", "wrong")).Returns((Kullanici)null);

            var sonuc = _sut.Login("user", "wrong");

            sonuc.Should().BeOfType<ViewResult>();
            ((string)_sut.ViewBag.Error).Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Login_POST_BasariliGiris_SessionSetEdilir_HomeaYonlendirir()
        {
            var kullanici = new Kullanici
            {
                KullaniciId = 1,
                KullaniciAdi = "admin",
                AdSoyad = "Test Admin",
                FirmaId = 1,
                RolId = 1,
                RolTanimi = "Admin"
            };
            _kullaniciMock.Setup(k => k.GirisYap("admin", "pass")).Returns(kullanici);

            var sonuc = _sut.Login("admin", "pass");

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Home");
            _sessionMock.Verify(s => s.SetCurrentUser(It.IsAny<AuthUserDTO>()), Times.Once);
        }

        // ─── Logout ───────────────────────────────────────────────────────────

        [Fact]
        public void Logout_OturumTemizlenir_LoginaYonlendirir()
        {
            var sonuc = _sut.Logout();

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Login");
            _sessionMock.Verify(s => s.Clear(), Times.Once);
        }

        // ─── ForgotPassword GET ───────────────────────────────────────────────

        [Fact]
        public void ForgotPassword_GET_OturumAciksa_HomeaYonlendirir()
        {
            _sessionMock.Setup(s => s.CurrentUser).Returns(new AuthUserDTO { KullaniciAdi = "admin" });

            var sonuc = _sut.ForgotPassword();

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Home");
        }

        // ─── ForgotPassword POST ──────────────────────────────────────────────

        [Fact]
        public void ForgotPassword_POST_BosKullaniciAdi_HataGosterir()
        {
            _sessionMock.Setup(s => s.CurrentUser).Returns((AuthUserDTO)null);

            var sonuc = _sut.ForgotPassword("   ");

            sonuc.Should().BeOfType<ViewResult>();
            ((string)_sut.ViewBag.Error).Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void ForgotPassword_POST_ServisBasarisiz_HataGosterir()
        {
            _sessionMock.Setup(s => s.CurrentUser).Returns((AuthUserDTO)null);
            _sifreMock.Setup(s => s.SifreSifirlamaBaslat("user"))
                      .Returns(new SifreSifirlamaSureci { Basarili = false, HataMesaji = "Kullanıcı bulunamadı." });

            var sonuc = _sut.ForgotPassword("user");

            sonuc.Should().BeOfType<ViewResult>();
            ((string)_sut.ViewBag.Error).Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void ForgotPassword_POST_Basarili_ConfirmaYonlendirir()
        {
            _sessionMock.Setup(s => s.CurrentUser).Returns((AuthUserDTO)null);
            _sifreMock.Setup(s => s.SifreSifirlamaBaslat("user"))
                      .Returns(new SifreSifirlamaSureci { Basarili = true, Email = "u@example.com" });
            _emailMock.Setup(e => e.MaskEmail("u@example.com")).Returns("u***@example.com");

            var sonuc = _sut.ForgotPassword("user");

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("ForgotPasswordConfirm");
        }

        // ─── ForgotPasswordConfirm POST ───────────────────────────────────────

        [Fact]
        public void ForgotPasswordConfirm_POST_Basarili_LoginaYonlendirir()
        {
            _sessionMock.Setup(s => s.CurrentUser).Returns((AuthUserDTO)null);
            _sifreMock.Setup(s => s.SifreSifirlamaTamamla("user", "123456", "Yeni1!", "Yeni1!"))
                      .Returns(new SifreSifirlamaTamamlayici { Basarili = true });

            var sonuc = _sut.ForgotPasswordConfirm("user", "123456", "Yeni1!", "Yeni1!");

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Login");
        }
    }
}
