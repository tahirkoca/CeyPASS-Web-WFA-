using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Web.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace CeyPASS.Tests.Web
{
    public class CalismaSekliControllerTests
    {
        private readonly Mock<ICalismaSekliService> _svcMock = new();
        private readonly Mock<ISessionContext> _sessionMock = new();
        private readonly Mock<IAuthorizationService> _authMock = new();
        private readonly Mock<IKisiEkraniLookUpService> _lookupMock = new();
        private readonly CalismaSekliController _sut;

        public CalismaSekliControllerTests()
        {
            _sut = new CalismaSekliController(_svcMock.Object, _sessionMock.Object, _authMock.Object, _lookupMock.Object);

            var httpContext = new DefaultHttpContext();
            _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _sut.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            _sessionMock.Setup(s => s.AktifFirmaId).Returns(1);
        }

        // ─── Edit GET ─────────────────────────────────────────────────────────

        [Fact]
        public void Edit_GET_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Vardiyalar", YetkiTipleri.Update)).Returns(false);

            var sonuc = _sut.Edit(1);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Edit_GET_VardiyaBulunamadi_NotFoundDoner()
        {
            _authMock.Setup(a => a.Can("Vardiyalar", YetkiTipleri.Update)).Returns(true);
            _svcMock.Setup(s => s.GetAll(It.IsAny<int>(), It.IsAny<bool>())).Returns(new List<CalismaSekli>());

            var sonuc = _sut.Edit(999);

            sonuc.Should().BeOfType<NotFoundResult>();
        }

        // ─── Edit POST ────────────────────────────────────────────────────────

        [Fact]
        public void Edit_POST_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Vardiyalar", YetkiTipleri.Update)).Returns(false);

            var vardiya = new CalismaSekli { Id = 1, Ad = "Sabah" };
            var sonuc = _sut.Edit(vardiya,
                new TimeSpan(9, 0, 0), new TimeSpan(18, 0, 0),
                TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        // ─── Index ────────────────────────────────────────────────────────────

        [Fact]
        public void Index_Yetkisiz_HomeIndexeYonlendirir()
        {
            _authMock.Setup(a => a.ViewAbility("Vardiyalar")).Returns(false);

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
            _authMock.Setup(a => a.Can("Vardiyalar", YetkiTipleri.Create)).Returns(false);

            var sonuc = _sut.Create();

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        // ─── Create POST ──────────────────────────────────────────────────────

        [Fact]
        public void Create_POST_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Vardiyalar", YetkiTipleri.Create)).Returns(false);

            var vardiya = new CalismaSekli { Ad = "Gece" };
            var sonuc = _sut.Create(vardiya,
                new TimeSpan(22, 0, 0), new TimeSpan(6, 0, 0),
                TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        // ─── Delete ───────────────────────────────────────────────────────────

        [Fact]
        public void Delete_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Vardiyalar", YetkiTipleri.Delete)).Returns(false);

            var sonuc = _sut.Delete(1);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }
    }
}
