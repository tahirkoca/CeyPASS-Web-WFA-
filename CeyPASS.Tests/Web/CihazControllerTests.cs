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
    /// <summary>
    /// Cihaz tanım ve aktif/pasif MVC.
    /// </summary>
    public class CihazControllerTests
    {
        private readonly Mock<ICihazService> _cihazMock = new();
        private readonly Mock<ISessionContext> _sessionMock = new();
        private readonly Mock<IAuthorizationService> _authMock = new();
        private readonly CihazController _sut;

        public CihazControllerTests()
        {
            _sut = new CihazController(_cihazMock.Object, _sessionMock.Object, _authMock.Object);

            var httpContext = new DefaultHttpContext();
            _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _sut.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            _sessionMock.Setup(s => s.AktifFirmaId).Returns(1);
            _cihazMock.Setup(c => c.GetCihazTipleri()).Returns(new List<CihazTip>());
        }

        // ─── Index ────────────────────────────────────────────────────────────

        /// <summary>
        /// Yetkisiz HomeIndexeYonlendirir
        /// </summary>
        [Fact]
        public void Index_Yetkisiz_HomeIndexeYonlendirir()
        {
            _authMock.Setup(a => a.ViewAbility("Cihazlar")).Returns(false);

            var sonuc = _sut.Index();

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Home");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        // ─── Create POST ──────────────────────────────────────────────────────

        /// <summary>
        /// POST Yetkisiz IndexeYonlendirir
        /// </summary>
        [Fact]
        public void Create_POST_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Cihazlar", YetkiTipleri.Create)).Returns(false);

            var sonuc = _sut.Create(new Cihaz(), null);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        // ─── Edit GET ─────────────────────────────────────────────────────────

        /// <summary>
        /// GET Yetkisiz IndexeYonlendirir
        /// </summary>
        [Fact]
        public void Edit_GET_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Cihazlar", YetkiTipleri.Update)).Returns(false);

            var sonuc = _sut.Edit(1);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// GET CihazBulunamadi NotFounddöner
        /// </summary>
        [Fact]
        public void Edit_GET_CihazBulunamadi_NotFoundDoner()
        {
            _authMock.Setup(a => a.Can("Cihazlar", YetkiTipleri.Update)).Returns(true);
            _cihazMock.Setup(c => c.Get(99)).Returns((Cihaz)null!);

            var sonuc = _sut.Edit(99);

            sonuc.Should().BeOfType<NotFoundResult>();
        }

        // ─── Edit POST ────────────────────────────────────────────────────────

        /// <summary>
        /// POST Yetkisiz IndexeYonlendirir
        /// </summary>
        [Fact]
        public void Edit_POST_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Cihazlar", YetkiTipleri.Update)).Returns(false);

            var sonuc = _sut.Edit(new Cihaz { CihazId = 1 }, null);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        // ─── Delete ───────────────────────────────────────────────────────────

        /// <summary>
        /// Yetkisiz IndexeYonlendirir
        /// </summary>
        [Fact]
        public void Delete_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Cihazlar", YetkiTipleri.Delete)).Returns(false);

            var sonuc = _sut.Delete(1);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        // ─── AktifYap ─────────────────────────────────────────────────────────

        /// <summary>
        /// Yetkisiz TempDataHataVeIndexeYonlendirir
        /// </summary>
        [Fact]
        public void AktifYap_Yetkisiz_TempDataHataVeIndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Cihazlar", YetkiTipleri.Update)).Returns(false);

            var sonuc = _sut.AktifYap(1);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// Yetkili ServisAktifYapçağrılır
        /// </summary>
        [Fact]
        public void AktifYap_Yetkili_ServisAktifYapCagrilir()
        {
            _authMock.Setup(a => a.Can("Cihazlar", YetkiTipleri.Update)).Returns(true);

            var sonuc = _sut.AktifYap(42);

            _cihazMock.Verify(c => c.AktifYap(42), Times.Once);
            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
        }
    }
}
