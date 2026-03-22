using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Web.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CeyPASS.Tests.Web
{
    public class RaporControllerTests : IDisposable
    {
        private readonly Mock<IRaporService> _raporMock = new();
        private readonly Mock<IKullaniciQueryService> _kullaniciMock = new();
        private readonly Mock<ISessionContext> _sessionMock = new();
        private readonly Mock<IAuthorizationService> _authMock = new();
        private readonly IMemoryCache _cache;

        public RaporControllerTests()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        public void Dispose() => _cache.Dispose();

        private RaporController CreateSut(ISession? session = null)
        {
            var sut = new RaporController(
                _raporMock.Object,
                _kullaniciMock.Object,
                _sessionMock.Object,
                _authMock.Object,
                _cache);

            var httpContext = new DefaultHttpContext();
            if (session != null)
            {
                httpContext.Features.Set<ISessionFeature>(new FakeSessionFeature(session));
            }
            sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
            sut.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            return sut;
        }

        // ─── Index ────────────────────────────────────────────────────────────

        [Fact]
        public void Index_Yetkisiz_HomeIndexeYonlendirir()
        {
            _authMock.Setup(a => a.ViewAbility("Raporlar")).Returns(false);
            var sut = CreateSut();

            var sonuc = sut.Index();

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Home");
        }

        [Fact]
        public void Index_ProcedureAdiYok_GoruntumuNull()
        {
            _authMock.Setup(a => a.ViewAbility("Raporlar")).Returns(true);
            _authMock.Setup(a => a.Can(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _sessionMock.Setup(s => s.AktifFirmaId).Returns(1);
            _raporMock.Setup(r => r.GetirRaporlar()).Returns(new List<RaporTanimi>());
            var sut = CreateSut();

            var sonuc = sut.Index(procedureAdi: null);

            var view = sonuc.Should().BeOfType<ViewResult>().Subject;
            view.Model.Should().BeNull();
        }

        // ─── ExportExcel ──────────────────────────────────────────────────────

        [Fact]
        public void ExportExcel_Yetkisiz_403Doner()
        {
            _authMock.Setup(a => a.Can("Raporlar", YetkiTipleri.Export)).Returns(false);
            var sut = CreateSut();

            var sonuc = sut.ExportExcel();

            var status = sonuc.Should().BeOfType<ObjectResult>().Subject;
            status.StatusCode.Should().Be(403);
        }

        [Fact]
        public void ExportExcel_SesyonBosSerialized_400Doner()
        {
            _authMock.Setup(a => a.Can("Raporlar", YetkiTipleri.Export)).Returns(true);

            byte[]? outBytes = null;
            var mockSession = new Mock<ISession>();
            mockSession.Setup(s => s.TryGetValue("LastRaporData", out outBytes)).Returns(false);

            var sut = CreateSut(mockSession.Object);

            var sonuc = sut.ExportExcel();

            var status = sonuc.Should().BeOfType<ObjectResult>().Subject;
            status.StatusCode.Should().Be(400);
        }

        // ─── ExportPdf ────────────────────────────────────────────────────────

        [Fact]
        public void ExportPdf_Yetkisiz_403Doner()
        {
            _authMock.Setup(a => a.Can("Raporlar", YetkiTipleri.Export)).Returns(false);
            var sut = CreateSut();

            var sonuc = sut.ExportPdf();

            var status = sonuc.Should().BeOfType<ObjectResult>().Subject;
            status.StatusCode.Should().Be(403);
        }

        [Fact]
        public void ExportPdf_SesyonBosSerialized_400Doner()
        {
            _authMock.Setup(a => a.Can("Raporlar", YetkiTipleri.Export)).Returns(true);

            byte[]? outBytes = null;
            var mockSession = new Mock<ISession>();
            mockSession.Setup(s => s.TryGetValue("LastRaporData", out outBytes)).Returns(false);

            var sut = CreateSut(mockSession.Object);

            var sonuc = sut.ExportPdf();

            var status = sonuc.Should().BeOfType<ObjectResult>().Subject;
            status.StatusCode.Should().Be(400);
        }

        // ─── Helper ───────────────────────────────────────────────────────────

        private sealed class FakeSessionFeature : ISessionFeature
        {
            public FakeSessionFeature(ISession session) => Session = session;
            public ISession Session { get; set; }
        }
    }
}
