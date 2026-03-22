using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Web.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using Xunit;

namespace CeyPASS.Tests.Web
{
    public class KisiHareketControllerTests : IDisposable
    {
        private readonly Mock<IKisiHareketService> _khMock = new();
        private readonly Mock<IKisiQueryService> _kisiQueryMock = new();
        private readonly Mock<ISessionContext> _sessionMock = new();
        private readonly Mock<IAuthorizationService> _authMock = new();
        private readonly Mock<IFirmaService> _firmaMock = new();
        private readonly IMemoryCache _cache;

        public KisiHareketControllerTests()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        public void Dispose() => _cache.Dispose();

        private KisiHareketController CreateSut()
        {
            var sut = new KisiHareketController(
                _khMock.Object,
                _kisiQueryMock.Object,
                _sessionMock.Object,
                _authMock.Object,
                _firmaMock.Object,
                _cache);

            var httpContext = new DefaultHttpContext();
            sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
            sut.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            return sut;
        }

        // ─── Index ────────────────────────────────────────────────────────────

        [Fact]
        public void Index_Yetkisiz_HomeIndexeYonlendirir()
        {
            _authMock.Setup(a => a.ViewAbility("KisiHareketler")).Returns(false);
            var sut = CreateSut();

            var sonuc = sut.Index();

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Home");
            ((string)sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Index_NonAdmin_FirmaIdSeciliTasarlanirAktifFirmaya()
        {
            _authMock.Setup(a => a.ViewAbility("KisiHareketler")).Returns(true);
            _authMock.Setup(a => a.Can(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _sessionMock.Setup(s => s.RolId).Returns(3);
            _sessionMock.Setup(s => s.AktifFirmaId).Returns(1);

            var dt = new DataTable();
            dt.Columns.Add("PersonelId", typeof(int));
            dt.Columns.Add("AdSoyad", typeof(string));
            _khMock.Setup(k => k.GetAktifKisilerWithSicil(It.IsAny<int>(), It.IsAny<bool>())).Returns(dt);

            var sut = CreateSut();

            var sonuc = sut.Index(firmaId: 99);

            sonuc.Should().BeOfType<ViewResult>();
            ((int)sut.ViewBag.SelectedFirmaId).Should().Be(1);
        }

        [Fact]
        public void Index_Admin_FirmalarViewBagdaYuklenirVeIsAdminTrue()
        {
            _authMock.Setup(a => a.ViewAbility("KisiHareketler")).Returns(true);
            _authMock.Setup(a => a.Can(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _sessionMock.Setup(s => s.RolId).Returns(1);
            _sessionMock.Setup(s => s.AktifFirmaId).Returns(1);

            var dt = new DataTable();
            dt.Columns.Add("PersonelId", typeof(int));
            dt.Columns.Add("AdSoyad", typeof(string));
            _khMock.Setup(k => k.GetAktifKisilerWithSicil(It.IsAny<int>(), It.IsAny<bool>())).Returns(dt);
            _firmaMock.Setup(f => f.GetAll()).Returns(new List<Firma>
            {
                new Firma { FirmaId = 1, FirmaAdi = "Firma A" }
            });

            var sut = CreateSut();

            var sonuc = sut.Index();

            sonuc.Should().BeOfType<ViewResult>();
            ((bool)sut.ViewBag.IsAdmin).Should().BeTrue();
            ((object)sut.ViewBag.Firmalar).Should().NotBeNull();
        }

        // ─── Ekle ─────────────────────────────────────────────────────────────

        [Fact]
        public void Ekle_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("KisiHareketler", YetkiTipleri.Create)).Returns(false);
            var sut = CreateSut();

            var sonuc = sut.Ekle(1, 100, DateTime.Today, "G");

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        // ─── Guncelle ─────────────────────────────────────────────────────────

        [Fact]
        public void Guncelle_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("KisiHareketler", YetkiTipleri.Update)).Returns(false);
            var sut = CreateSut();

            var sonuc = sut.Guncelle(1, DateTime.Today, "G");

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        // ─── PasifYap ─────────────────────────────────────────────────────────

        [Fact]
        public void PasifYap_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("KisiHareketler", YetkiTipleri.Delete)).Returns(false);
            var sut = CreateSut();

            var sonuc = sut.PasifYap(1);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }
    }
}
