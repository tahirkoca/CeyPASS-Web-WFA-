using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Web.Controllers;
using CeyPASS.Web.Models.POY;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace CeyPASS.Tests.Web
{
    public class IsyeriControllerTests
    {
        private readonly Mock<IIsyeriService> _isyeriMock = new();
        private readonly Mock<IFirmaService> _firmaMock = new();
        private readonly Mock<IAuthorizationService> _authMock = new();
        private readonly Mock<IKisiEkraniLookUpService> _lookupMock = new();
        private readonly IsyeriController _sut;

        public IsyeriControllerTests()
        {
            _sut = new IsyeriController(_isyeriMock.Object, _firmaMock.Object, _authMock.Object, _lookupMock.Object);

            var httpContext = new DefaultHttpContext();
            _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _sut.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            _firmaMock.Setup(f => f.GetLookup()).Returns(new List<LookupItem>());
        }

        // ─── Create POST ──────────────────────────────────────────────────────

        [Fact]
        public void Create_POST_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Isyerler", YetkiTipleri.Create)).Returns(false);

            var sonuc = _sut.Create(new IsyeriFormModel());

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Create_POST_BosIsyeriAdi_GoruntumuDoner()
        {
            _authMock.Setup(a => a.Can("Isyerler", YetkiTipleri.Create)).Returns(true);

            var model = new IsyeriFormModel { FirmaId = 1, IsyeriId = 10, IsyeriAdi = "" };
            var sonuc = _sut.Create(model);

            var view = sonuc.Should().BeOfType<ViewResult>().Subject;
            _sut.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Create_POST_FirmaIdSifir_GoruntumuDoner()
        {
            _authMock.Setup(a => a.Can("Isyerler", YetkiTipleri.Create)).Returns(true);

            var model = new IsyeriFormModel { FirmaId = 0, IsyeriId = 10, IsyeriAdi = "Merkez" };
            var sonuc = _sut.Create(model);

            var view = sonuc.Should().BeOfType<ViewResult>().Subject;
            _sut.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Create_POST_ServisBasarili_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Isyerler", YetkiTipleri.Create)).Returns(true);
            _isyeriMock.Setup(s => s.AddManual(1, 10, "Merkez")).Returns(true);

            var model = new IsyeriFormModel { FirmaId = 1, IsyeriId = 10, IsyeriAdi = "Merkez" };
            var sonuc = _sut.Create(model);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Success"]!).Should().NotBeNullOrEmpty();
        }

        // ─── Index ────────────────────────────────────────────────────────────

        [Fact]
        public void Index_Yetkisiz_HomeIndexeYonlendirir()
        {
            _authMock.Setup(a => a.ViewAbility("Isyerler")).Returns(false);
            _isyeriMock.Setup(s => s.GetAll()).Returns(new System.Data.DataTable());

            var sonuc = _sut.Index();

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Home");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        // ─── Edit GET ─────────────────────────────────────────────────────────

        [Fact]
        public void Edit_GET_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Isyerler", YetkiTipleri.Update)).Returns(false);

            var sonuc = _sut.Edit(1, 10);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        // ─── Edit POST ────────────────────────────────────────────────────────

        [Fact]
        public void Edit_POST_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Isyerler", YetkiTipleri.Update)).Returns(false);

            var sonuc = _sut.Edit(new IsyeriFormModel { FirmaId = 1, IsyeriId = 10, IsyeriAdi = "Merkez" });

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Edit_POST_BosIsyeriAdi_GoruntumuDoner()
        {
            _authMock.Setup(a => a.Can("Isyerler", YetkiTipleri.Update)).Returns(true);

            var model = new IsyeriFormModel { FirmaId = 1, IsyeriId = 10, IsyeriAdi = "" };
            var sonuc = _sut.Edit(model);

            var view = sonuc.Should().BeOfType<ViewResult>().Subject;
            _sut.ModelState.IsValid.Should().BeFalse();
        }

        // ─── Delete ───────────────────────────────────────────────────────────

        [Fact]
        public void Delete_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Isyerler", YetkiTipleri.Delete)).Returns(false);

            var sonuc = _sut.Delete(1, 10);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }
    }
}
