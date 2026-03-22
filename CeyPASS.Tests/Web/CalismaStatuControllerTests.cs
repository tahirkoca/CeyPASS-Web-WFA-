using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Web.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Xunit;

namespace CeyPASS.Tests.Web
{
    public class CalismaStatuControllerTests
    {
        private readonly Mock<ICalismaStatuService> _statuMock = new();
        private readonly Mock<ISessionContext> _sessionMock = new();
        private readonly Mock<IAuthorizationService> _authMock = new();
        private readonly CalismaStatuController _sut;

        public CalismaStatuControllerTests()
        {
            _sut = new CalismaStatuController(_statuMock.Object, _sessionMock.Object, _authMock.Object);

            var httpContext = new DefaultHttpContext();
            _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _sut.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        }

        // ─── Create POST ──────────────────────────────────────────────────────

        [Fact]
        public void Create_POST_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("CalismaStatuleri", YetkiTipleri.Create)).Returns(false);

            var sonuc = _sut.Create("Tam Zamanlı");

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Create_POST_BosAd_TempDataHataVeRedirect()
        {
            _authMock.Setup(a => a.Can("CalismaStatuleri", YetkiTipleri.Create)).Returns(true);

            var sonuc = _sut.Create("   ");

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
            _statuMock.Verify(s => s.AddAuto(It.IsAny<string>()), Times.Never);
        }

        // ─── Index ────────────────────────────────────────────────────────────

        [Fact]
        public void Index_Yetkisiz_HomeIndexeYonlendirir()
        {
            _authMock.Setup(a => a.ViewAbility("CalismaStatuleri")).Returns(false);

            var sonuc = _sut.Index();

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Home");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        // ─── Update ───────────────────────────────────────────────────────────

        [Fact]
        public void Update_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("CalismaStatuleri", YetkiTipleri.Update)).Returns(false);

            var sonuc = _sut.Update(1, "Tam Zamanlı");

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Update_BosAd_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("CalismaStatuleri", YetkiTipleri.Update)).Returns(true);

            var sonuc = _sut.Update(1, "   ");

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
            _statuMock.Verify(s => s.Update(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        // ─── Delete ───────────────────────────────────────────────────────────

        [Fact]
        public void Delete_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("CalismaStatuleri", YetkiTipleri.Delete)).Returns(false);

            var sonuc = _sut.Delete(1);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }
    }
}
