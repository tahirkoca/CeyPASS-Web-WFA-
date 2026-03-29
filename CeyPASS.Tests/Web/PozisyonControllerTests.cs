using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Web.Controllers;
using CeyPASS.Web.Models.POY;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Xunit;

namespace CeyPASS.Tests.Web
{
    public class PozisyonControllerTests
    {
        private readonly Mock<IPozisyonService> _pozisyonMock = new();
        private readonly Mock<IAuthorizationService> _authMock = new();
        private readonly Mock<IKisiEkraniLookUpService> _lookupMock = new();
        private readonly PozisyonController _sut;

        public PozisyonControllerTests()
        {
            _sut = new PozisyonController(_pozisyonMock.Object, _authMock.Object, _lookupMock.Object);

            var httpContext = new DefaultHttpContext();
            _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _sut.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        }

        // ─── Edit POST ────────────────────────────────────────────────────────

        [Fact]
        public void Edit_POST_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Pozisyonlar", YetkiTipleri.Update)).Returns(false);

            var sonuc = _sut.Edit(new PozisyonFormModel { PozisyonId = 1 });

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Edit_POST_BosAd_GoruntumuDoner()
        {
            _authMock.Setup(a => a.Can("Pozisyonlar", YetkiTipleri.Update)).Returns(true);

            var model = new PozisyonFormModel { PozisyonId = 1, PozisyonAdi = "" };
            var sonuc = _sut.Edit(model);

            var view = sonuc.Should().BeOfType<ViewResult>().Subject;
            _sut.ModelState.IsValid.Should().BeFalse();
        }

        // ─── Index ────────────────────────────────────────────────────────────

        [Fact]
        public void Index_Yetkisiz_HomeIndexeYonlendirir()
        {
            _authMock.Setup(a => a.ViewAbility("Pozisyonlar")).Returns(false);

            var sonuc = _sut.Index();

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Home");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        // ─── Create GET ───────────────────────────────────────────────────────

        [Fact]
        public void Create_GET_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Pozisyonlar", YetkiTipleri.Create)).Returns(false);

            var sonuc = _sut.Create();

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        // ─── Create POST ──────────────────────────────────────────────────────

        [Fact]
        public void Create_POST_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Pozisyonlar", YetkiTipleri.Create)).Returns(false);

            var sonuc = _sut.Create(new PozisyonFormModel { PozisyonAdi = "Müdür" });

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Create_POST_BosAd_GoruntumuDoner()
        {
            _authMock.Setup(a => a.Can("Pozisyonlar", YetkiTipleri.Create)).Returns(true);

            var model = new PozisyonFormModel { PozisyonAdi = "" };
            var sonuc = _sut.Create(model);

            sonuc.Should().BeOfType<ViewResult>();
            _sut.ModelState.IsValid.Should().BeFalse();
        }

        // ─── Edit GET ─────────────────────────────────────────────────────────

        [Fact]
        public void Edit_GET_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Pozisyonlar", YetkiTipleri.Update)).Returns(false);

            var sonuc = _sut.Edit(1);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        // ─── Delete ───────────────────────────────────────────────────────────

        [Fact]
        public void Delete_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Pozisyonlar", YetkiTipleri.Delete)).Returns(false);

            var sonuc = _sut.Delete(1);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }
    }
}
