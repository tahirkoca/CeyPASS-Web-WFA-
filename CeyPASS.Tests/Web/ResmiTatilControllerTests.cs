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
    public class ResmiTatilControllerTests
    {
        private readonly Mock<IResmiTatilService> _tatilMock = new();
        private readonly Mock<ISessionContext> _sessionMock = new();
        private readonly Mock<IAuthorizationService> _authMock = new();
        private readonly ResmiTatilController _sut;

        public ResmiTatilControllerTests()
        {
            _sut = new ResmiTatilController(_tatilMock.Object, _sessionMock.Object, _authMock.Object);

            var httpContext = new DefaultHttpContext();
            _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _sut.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        }

        // ─── Index ────────────────────────────────────────────────────────────

        [Fact]
        public void Index_Yetkisiz_HomeIndexeYonlendirir()
        {
            _authMock.Setup(a => a.ViewAbility("ResmiTatiller")).Returns(false);

            var sonuc = _sut.Index();

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Home");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Index_YilParametresiYok_BugununYiliKullanilir()
        {
            _authMock.Setup(a => a.ViewAbility("ResmiTatiller")).Returns(true);
            _authMock.Setup(a => a.Can(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _tatilMock.Setup(t => t.GetList(It.IsAny<int?>())).Returns(new List<ResmiTatilDTO>());

            var sonuc = _sut.Index(yil: null);

            sonuc.Should().BeOfType<ViewResult>();
            ((int)_sut.ViewBag.SelectedYil).Should().Be(DateTime.Today.Year);
        }

        // ─── DoldurSabit ──────────────────────────────────────────────────────

        [Fact]
        public void DoldurSabit_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("ResmiTatiller", YetkiTipleri.Approve)).Returns(false);

            var sonuc = _sut.DoldurSabit(2025, 2026);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void DoldurSabit_Yetkili_ServisBasarili_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("ResmiTatiller", YetkiTipleri.Approve)).Returns(true);

            var sonuc = _sut.DoldurSabit(2025, 2026);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Success"]!).Should().NotBeNullOrEmpty();
        }

        // ─── EkleVeyaGuncelle ─────────────────────────────────────────────────

        [Fact]
        public void EkleVeyaGuncelle_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("ResmiTatiller", YetkiTipleri.Create)).Returns(false);

            var sonuc = _sut.EkleVeyaGuncelle(new DateTime(2025, 1, 1), "Yılbaşı", null);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }
    }
}
