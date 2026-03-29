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
using System;

namespace CeyPASS.Tests.Web
{
    public class DepartmanControllerTests
    {
        private readonly Mock<IDepartmanService> _departmanMock = new();
        private readonly Mock<IAuthorizationService> _authMock = new();
        private readonly Mock<IKisiEkraniLookUpService> _lookupMock = new();
        private readonly Mock<ISessionContext> _sessionMock = new();
        private readonly DepartmanController _sut;

        public DepartmanControllerTests()
        {
            _sut = new DepartmanController(_departmanMock.Object, _authMock.Object, _lookupMock.Object, _sessionMock.Object);

            var httpContext = new DefaultHttpContext();
            _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _sut.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        }

        // ─── Create ───────────────────────────────────────────────────────────

        [Fact]
        public void Create_POST_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Departmanlar", YetkiTipleri.Create)).Returns(false);

            var model = new DepartmanFormModel { DepartmanAdi = "IT" };
            var sonuc = _sut.Create(model);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
        }

        [Fact]
        public void Create_POST_Basarili_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Departmanlar", YetkiTipleri.Create)).Returns(true);
            _departmanMock.Setup(d => d.Add(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            var model = new DepartmanFormModel { DepartmanId = 1, DepartmanAdi = "IT" };
            var sonuc = _sut.Create(model);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            _sut.TempData["Success"].Should().NotBeNull();
        }

        // ─── Edit ─────────────────────────────────────────────────────────────

        [Fact]
        public void Edit_POST_Basarili_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Departmanlar", YetkiTipleri.Update)).Returns(true);
            _sessionMock.Setup(s => s.IsAdmin()).Returns(true);
            
            var dt = new System.Data.DataTable();
            dt.Columns.Add("FirmaId", typeof(int));
            var row = dt.NewRow();
            row["FirmaId"] = 1;
            _departmanMock.Setup(d => d.GetRowById(It.IsAny<int>())).Returns(row);
            _departmanMock.Setup(d => d.Update(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            var model = new DepartmanFormModel { DepartmanId = 1, DepartmanAdi = "IT" };
            var sonuc = _sut.Edit(model);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            _sut.TempData["Success"].Should().NotBeNull();
        }

        // ─── Delete ───────────────────────────────────────────────────────────

        [Fact]
        public void Delete_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Departmanlar", YetkiTipleri.Delete)).Returns(false);

            var sonuc = _sut.Delete(1);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
        }
    }
}
