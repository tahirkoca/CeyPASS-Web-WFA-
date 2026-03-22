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
    public class DepartmanControllerTests
    {
        private readonly Mock<IDepartmanService> _departmanMock = new();
        private readonly Mock<IAuthorizationService> _authMock = new();
        private readonly DepartmanController _sut;

        public DepartmanControllerTests()
        {
            _sut = new DepartmanController(_departmanMock.Object, _authMock.Object);

            var httpContext = new DefaultHttpContext();
            _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _sut.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        }

        // ─── Create POST ──────────────────────────────────────────────────────

        [Fact]
        public void Create_POST_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Departmanlar", YetkiTipleri.Create)).Returns(false);

            var sonuc = _sut.Create(new DepartmanFormModel());

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Create_POST_BosAd_GoruntumuDoner()
        {
            _authMock.Setup(a => a.Can("Departmanlar", YetkiTipleri.Create)).Returns(true);

            var model = new DepartmanFormModel { DepartmanId = 1, DepartmanAdi = "" };
            var sonuc = _sut.Create(model);

            var view = sonuc.Should().BeOfType<ViewResult>().Subject;
            _sut.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Create_POST_ServisBasarili_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Departmanlar", YetkiTipleri.Create)).Returns(true);
            _departmanMock.Setup(d => d.Add(5, "Muhasebe", "")).Returns(true);

            var model = new DepartmanFormModel { DepartmanId = 5, DepartmanAdi = "Muhasebe" };
            var sonuc = _sut.Create(model);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Success"]!).Should().NotBeNullOrEmpty();
        }

        // ─── Index ────────────────────────────────────────────────────────────

        [Fact]
        public void Index_Yetkisiz_HomeIndexeYonlendirir()
        {
            _authMock.Setup(a => a.ViewAbility("Departmanlar")).Returns(false);

            var sonuc = _sut.Index();

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Home");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        // ─── Edit POST ────────────────────────────────────────────────────────

        [Fact]
        public void Edit_POST_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Departmanlar", YetkiTipleri.Update)).Returns(false);

            var sonuc = _sut.Edit(new DepartmanFormModel { DepartmanId = 1 });

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Edit_POST_Basarili_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Departmanlar", YetkiTipleri.Update)).Returns(true);
            _departmanMock.Setup(d => d.Update(1, "IT", "")).Returns(true);

            var model = new DepartmanFormModel { DepartmanId = 1, DepartmanAdi = "IT" };
            var sonuc = _sut.Edit(model);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Success"]!).Should().NotBeNullOrEmpty();
        }

        // ─── Delete ───────────────────────────────────────────────────────────

        [Fact]
        public void Delete_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Departmanlar", YetkiTipleri.Delete)).Returns(false);

            var sonuc = _sut.Delete(1);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }
    }
}
