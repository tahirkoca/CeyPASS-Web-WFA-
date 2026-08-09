using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Web.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Xunit;

namespace CeyPASS.Tests.Web
{
    public class CanliIzlemeControllerTests
    {
        private readonly Mock<ICanliIzlemeService> _svcMock = new();
        private readonly Mock<IKisiHareketService> _khMock = new();
        private readonly Mock<IKisiDetayService> _kdMock = new();
        private readonly Mock<IMisafirKartService> _mMock = new();
        private readonly Mock<IAracKartiService> _aracMock = new();

        private CanliIzlemeController CreateSutWithRole(string rol)
        {
            var sut = new CanliIzlemeController(
                _svcMock.Object,
                _khMock.Object,
                _kdMock.Object,
                _mMock.Object,
                _aracMock.Object);

            // Serialize an AuthUserDTO into the session key "CanliIzlemeUser"
            var user = new AuthUserDTO { FirmaId = 1, Rol = rol, KullaniciId = 1 };
            var json = JsonSerializer.Serialize(user);
            var bytes = Encoding.UTF8.GetBytes(json);

            var mockSession = new Mock<ISession>();
            mockSession.Setup(s => s.TryGetValue("CanliIzlemeUser", out bytes)).Returns(true);

            var httpContext = new DefaultHttpContext();
            httpContext.Features.Set<ISessionFeature>(new FakeSessionFeature(mockSession.Object));

            sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
            sut.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            // Stub service calls made during Index
            _svcMock.Setup(s => s.GetLastPasses(It.IsAny<int>(), It.IsAny<int>())).Returns(new List<LastPassDTO>());
            _svcMock.Setup(s => s.GetLastPassesYemekhane(It.IsAny<int>(), It.IsAny<int>())).Returns(new List<LastPassDTO>());
            _khMock.Setup(k => k.GetLastMovesByFirma(It.IsAny<int>(), It.IsAny<int>())).Returns(new List<KisiHareketDTO>());
            _khMock.Setup(k => k.GetLastMovesByFirmaYemekhane(It.IsAny<int>(), It.IsAny<int>())).Returns(new List<KisiHareketDTO>());

            return sut;
        }

        // ─── Index — IsYemekhaneRole / IsDanismaRole (private static) ─────────

        [Fact]
        public void Index_YemekhaneRolu_IsYemekhaneTrueOlmali()
        {
            var sut = CreateSutWithRole("YEMEKHANE");

            var sonuc = sut.Index();

            sonuc.Should().BeOfType<ViewResult>();
            ((bool)sut.ViewBag.IsYemekhane).Should().BeTrue();
        }

        [Fact]
        public void Index_DanismaRolu_IsDanismaTrueOlmali()
        {
            var sut = CreateSutWithRole("DANIŞMA GÖREVLİSİ");

            var sonuc = sut.Index();

            sonuc.Should().BeOfType<ViewResult>();
            ((bool)sut.ViewBag.IsDanisma).Should().BeTrue();
        }

        [Fact]
        public void Index_NormalRol_CanMisafirKartTrueOlmali()
        {
            // Normal role: IsYemekhane=false → CanMisafirKart = !(false && !false) = true
            var sut = CreateSutWithRole("Operatör");

            var sonuc = sut.Index();

            sonuc.Should().BeOfType<ViewResult>();
            ((bool)sut.ViewBag.CanMisafirKart).Should().BeTrue();
        }

        // ─── Helper: Fake ISessionFeature ────────────────────────────────────

        private sealed class FakeSessionFeature : ISessionFeature
        {
            public FakeSessionFeature(ISession session) => Session = session;
            public ISession Session { get; set; }
        }
    }
}
