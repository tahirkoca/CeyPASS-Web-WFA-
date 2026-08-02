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
using Xunit;

namespace CeyPASS.Tests.Web
{
    public class IzinControllerTests : IDisposable
    {
        private readonly Mock<IKisiIzinService> _izinMock = new();
        private readonly Mock<IKisiQueryService> _kisiQueryMock = new();
        private readonly Mock<IIzinTipService> _izinTipMock = new();
        private readonly Mock<IIzinTalepService> _izinTalepMock = new();
        private readonly Mock<IFirmaService> _firmaMock = new();
        private readonly Mock<IPuantajService> _puantajMock = new();
        private readonly Mock<ISessionContext> _sessionMock = new();
        private readonly Mock<IAuthorizationService> _authMock = new();
        private readonly IMemoryCache _cache;
        private readonly IzinController _sut;

        public IzinControllerTests()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());

            _sut = new IzinController(
                _izinMock.Object,
                _kisiQueryMock.Object,
                _izinTipMock.Object,
                _izinTalepMock.Object,
                _firmaMock.Object,
                _puantajMock.Object,
                _sessionMock.Object,
                _authMock.Object,
                _cache);

            var httpContext = new DefaultHttpContext();
            _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _sut.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            _sessionMock.Setup(s => s.AktifFirmaId).Returns(1);
            _sessionMock.Setup(s => s.AktifKullaniciId).Returns(10);
        }

        public void Dispose() => _cache.Dispose();

        // ─── Index ────────────────────────────────────────────────────────────

        [Fact]
        public void Index_Yetkisiz_HomeIndexeYonlendirir()
        {
            _authMock.Setup(a => a.ViewAbility("Izinler")).Returns(false);

            var sonuc = _sut.Index();

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Home");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Index_Yetkili_GoruntumuDoner()
        {
            _authMock.Setup(a => a.ViewAbility("Izinler")).Returns(true);
            _authMock.Setup(a => a.Can(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            int totalCount = 0;
            _izinMock.Setup(s => s.GetTumIzinlerPaged(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int?>(),
                It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                It.IsAny<int>(), It.IsAny<int>(), out totalCount))
                .Returns(new List<KisiIzinListRow>());

            _kisiQueryMock.Setup(q => q.GetAktifKisilerByFirma(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool?>(), It.IsAny<int?>(), It.IsAny<IReadOnlyList<int>>(), It.IsAny<bool>())).Returns(new List<KisiListItem>());
            _izinTipMock.Setup(t => t.GetAktif()).Returns(new List<IzinTip>());

            var sonuc = _sut.Index();

            sonuc.Should().BeOfType<ViewResult>();
        }

        // ─── Create GET ───────────────────────────────────────────────────────

        [Fact]
        public void Create_GET_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Izinler", YetkiTipleri.Create)).Returns(false);

            var sonuc = _sut.Create();

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        // ─── Create POST ──────────────────────────────────────────────────────

        [Fact]
        public void Create_POST_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Izinler", YetkiTipleri.Create)).Returns(false);

            var sonuc = _sut.Create(new KisiIzin(), null, null);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
        }

        [Fact]
        public void Create_POST_GecersizValidasyon_GoruntumuDoner()
        {
            _authMock.Setup(a => a.Can("Izinler", YetkiTipleri.Create)).Returns(true);
            _izinMock.Setup(s => s.ValidateKayit(It.IsAny<IzinKayitValidasyonDTO>()))
                .Returns((false, "Geçersiz tarih aralığı."));

            _puantajMock.Setup(p => p.GetKullaniciFirmaIsyeriYetkileri(It.IsAny<int>()))
                .Returns(new List<FirmaIsyeriYetkiDTO>());
            _firmaMock.Setup(f => f.GetPuantajFirmalar()).Returns(new List<Firma>());
            _kisiQueryMock.Setup(q => q.GetAktifKisilerByFirma(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool?>(), It.IsAny<int?>(), It.IsAny<IReadOnlyList<int>>(), It.IsAny<bool>())).Returns(new List<KisiListItem>());
            _izinTipMock.Setup(t => t.GetAktif()).Returns(new List<IzinTip>());

            var izin = new KisiIzin { FirmaId = 1, Baslangic = DateTime.Today, Bitis = DateTime.Today };
            var sonuc = _sut.Create(izin, null, null);

            var view = sonuc.Should().BeOfType<ViewResult>().Subject;
            _sut.ModelState.IsValid.Should().BeFalse();
        }

        // ─── Edit GET ─────────────────────────────────────────────────────────

        [Fact]
        public void Edit_GET_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Izinler", YetkiTipleri.Update)).Returns(false);

            var sonuc = _sut.Edit(1);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        // ─── Edit POST ────────────────────────────────────────────────────────

        [Fact]
        public void Edit_POST_IzinBulunamadi_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Izinler", YetkiTipleri.Update)).Returns(true);
            _izinMock.Setup(s => s.GetById(99)).Returns((KisiIzin)null!);

            var izin = new KisiIzin { KisiIzinId = 99 };
            var sonuc = _sut.Edit(izin, null, null);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        // ─── Delete ───────────────────────────────────────────────────────────

        [Fact]
        public void Delete_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Izinler", YetkiTipleri.Delete)).Returns(false);

            var sonuc = _sut.Delete(1);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Delete_Yetkili_ServisBasarili_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Izinler", YetkiTipleri.Delete)).Returns(true);
            _izinMock.Setup(s => s.PasifYap(5)).Returns(true);
            _sessionMock.Setup(s => s.AktifFirmaId).Returns(1);

            var sonuc = _sut.Delete(5);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Success"]!).Should().NotBeNullOrEmpty();
        }

        // ─── GetKisiler ───────────────────────────────────────────────────────

        [Fact]
        public void GetKisiler_FirmaIdVerilir_JsonDoner()
        {
            _kisiQueryMock.Setup(q => q.GetAktifKisilerByFirma(2, It.IsAny<string>(), It.IsAny<bool?>(), It.IsAny<int?>(), It.IsAny<IReadOnlyList<int>>(), It.IsAny<bool>()))
                .Returns(new List<KisiListItem> { new KisiListItem { PersonelId = "P1", AdSoyad = "Ali Veli" } });

            var sonuc = _sut.GetKisiler(2);

            sonuc.Should().BeOfType<JsonResult>();
        }
    }
}
