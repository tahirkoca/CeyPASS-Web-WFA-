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
using System.Text.Json;
using Xunit;

namespace CeyPASS.Tests.Web
{
    public class PuantajControllerTests
    {
        private readonly Mock<IPuantajService> _puantajMock = new();
        private readonly Mock<IFirmaService> _firmaMock = new();
        private readonly Mock<IIsyeriService> _isyeriMock = new();
        private readonly Mock<IKisiService> _kisiMock = new();
        private readonly Mock<IKisiQueryService> _kisiQueryMock = new();
        private readonly Mock<ISessionContext> _sessionMock = new();
        private readonly Mock<IAuthorizationService> _authMock = new();
        private readonly PuantajController _sut;

        public PuantajControllerTests()
        {
            _sut = new PuantajController(
                _puantajMock.Object,
                _firmaMock.Object,
                _isyeriMock.Object,
                _kisiMock.Object,
                _kisiQueryMock.Object,
                _sessionMock.Object,
                _authMock.Object);

            // AJAX mode: X-Requested-With header causes JSON responses
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Append("X-Requested-With", "XMLHttpRequest");
            _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _sut.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            // Authorization granted
            _authMock.Setup(a => a.Can("AylikPuantaj", YetkiTipleri.Update)).Returns(true);

            // Session user
            _sessionMock.Setup(s => s.AktifKullaniciId).Returns(1);
        }

        private static string SerializeJsonValue(object value)
            => JsonSerializer.Serialize(value);

        // ─── Duzenle — TryParseSaat dolaylı testleri ─────────────────────────

        [Fact]
        public void Duzenle_GecersizSaat_JsonHataDoner()
        {
            var sonuc = _sut.Duzenle(1, DateTime.Today, 0, null, null, "abc");

            var json = sonuc.Should().BeOfType<JsonResult>().Subject;
            SerializeJsonValue(json.Value).Should().Contain("\"success\":false");
        }

        [Fact]
        public void Duzenle_VirguluSaat_JsonBasariliDoner()
        {
            // "7,50" → replaces comma with dot → 7.50 → no normalization needed
            var sonuc = _sut.Duzenle(1, DateTime.Today, 0, null, calismaTipi: null, saat: "7,50");

            var json = sonuc.Should().BeOfType<JsonResult>().Subject;
            SerializeJsonValue(json.Value).Should().Contain("\"success\":true");
        }

        [Fact]
        public void Duzenle_IntegerSaat75_JsonBasariliDoner()
        {
            // "75" → > 24 and ≤ 99 → /10 → 7.5
            var sonuc = _sut.Duzenle(1, DateTime.Today, 0, null, calismaTipi: null, saat: "75");

            var json = sonuc.Should().BeOfType<JsonResult>().Subject;
            SerializeJsonValue(json.Value).Should().Contain("\"success\":true");
        }

        [Fact]
        public void Duzenle_IntegerSaat750_JsonBasariliDoner()
        {
            // "750" → > 99 → /100 → 7.5
            var sonuc = _sut.Duzenle(1, DateTime.Today, 0, null, calismaTipi: null, saat: "750");

            var json = sonuc.Should().BeOfType<JsonResult>().Subject;
            SerializeJsonValue(json.Value).Should().Contain("\"success\":true");
        }

        // ─── Index ────────────────────────────────────────────────────────────

        [Fact]
        public void Index_Yetkisiz_HomeIndexeYonlendirir()
        {
            _authMock.Setup(a => a.ViewAbility("AylikPuantaj")).Returns(false);
            _sessionMock.Setup(s => s.AktifFirmaId).Returns(1);
            _sessionMock.Setup(s => s.AktifKullaniciId).Returns(1);
            _puantajMock.Setup(p => p.GetKullaniciFirmaIsyeriYetkileri(It.IsAny<int>())).Returns(new List<FirmaIsyeriYetkiDTO>());

            // Non-AJAX context for Index
            var httpContext = new DefaultHttpContext();
            _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _sut.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            var sonuc = _sut.Index();

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Home");
        }

        [Fact]
        public void Index_Yetkili_ViewDoner()
        {
            _authMock.Setup(a => a.ViewAbility("AylikPuantaj")).Returns(true);
            _authMock.Setup(a => a.Can(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _sessionMock.Setup(s => s.AktifFirmaId).Returns(1);
            _sessionMock.Setup(s => s.AktifKullaniciId).Returns(1);
            _puantajMock.Setup(p => p.GetKullaniciFirmaIsyeriYetkileri(It.IsAny<int>())).Returns(new List<FirmaIsyeriYetkiDTO>());
            _firmaMock.Setup(f => f.GetPuantajFirmalar()).Returns(new List<Firma>());
            _isyeriMock.Setup(i => i.GetIsyerleriByFirma(It.IsAny<int>())).Returns(new List<IsyeriItem>());
            _kisiQueryMock.Setup(q => q.GetAktifKisilerByFirma(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool?>(), It.IsAny<int?>())).Returns(new List<KisiListItem>());
            _puantajMock.Setup(p => p.GetPuantajTipleri()).Returns(new List<PuantajTipDTO>());
            _puantajMock.Setup(p => p.GetEkKayitGun()).Returns(0);

            var httpContext = new DefaultHttpContext();
            _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _sut.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            var sonuc = _sut.Index();

            sonuc.Should().BeOfType<ViewResult>();
        }

        // ─── Onayla ───────────────────────────────────────────────────────────

        [Fact]
        public void Onayla_Yetkisiz_JsonHataDoner()
        {
            _authMock.Setup(a => a.Can("AylikPuantaj", YetkiTipleri.Approve)).Returns(false);

            var sonuc = _sut.Onayla(1, DateTime.Today, 0, null, null, "7.5");

            var json = sonuc.Should().BeOfType<JsonResult>().Subject;
            SerializeJsonValue(json.Value).Should().Contain("\"success\":false");
        }

        // ─── Reddet ───────────────────────────────────────────────────────────

        [Fact]
        public void Reddet_Yetkisiz_JsonHataDoner()
        {
            _authMock.Setup(a => a.Can("AylikPuantaj", YetkiTipleri.Delete)).Returns(false);

            var sonuc = _sut.Reddet(1, DateTime.Today, null);

            var json = sonuc.Should().BeOfType<JsonResult>().Subject;
            SerializeJsonValue(json.Value).Should().Contain("\"success\":false");
        }

        // ─── SetEkKayitGun ────────────────────────────────────────────────────

        [Fact]
        public void SetEkKayitGun_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("AylikPuantaj", YetkiTipleri.Update)).Returns(false);

            var httpContext = new DefaultHttpContext();
            _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _sut.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            var sonuc = _sut.SetEkKayitGun(5);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        // ─── CokluSicileAktar ─────────────────────────────────────────────────

        [Fact]
        public void CokluSicileAktar_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("AylikPuantaj", YetkiTipleri.Update)).Returns(false);

            var httpContext = new DefaultHttpContext();
            _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _sut.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            var sonuc = _sut.CokluSicileAktar(1, 2025, 3);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        // ─── GetKisiler ───────────────────────────────────────────────────────

        [Fact]
        public void GetKisiler_IsyeriIdYok_KisiQueryServiseCagrilir()
        {
            _kisiQueryMock.Setup(q => q.GetAktifKisilerByFirma(1, It.IsAny<string>(), It.IsAny<bool?>(), It.IsAny<int?>()))
                .Returns(new List<KisiListItem> { new KisiListItem { PersonelId = "P1", AdSoyad = "Ali Veli" } });

            var sonuc = _sut.GetKisiler(1, null, null, null);

            sonuc.Should().BeOfType<JsonResult>();
        }
    }
}
