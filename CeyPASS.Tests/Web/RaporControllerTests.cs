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
using System.Data;
using System.Linq;
using Xunit;

namespace CeyPASS.Tests.Web
{
    public class RaporControllerTests : IDisposable
    {
        private readonly Mock<IRaporService> _raporMock = new();
        private readonly Mock<IKullaniciQueryService> _kullaniciMock = new();
        private readonly Mock<IKullaniciFirmaIsyeriYetkiService> _yetkiMock = new();
        private readonly Mock<IKisiEkraniLookUpService> _lookupMock = new();
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
                _yetkiMock.Object,
                _lookupMock.Object,
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
            _sessionMock.Setup(s => s.AktifKullaniciId).Returns(5);
            _sessionMock.Setup(s => s.IsAdmin()).Returns(false);
            _raporMock.Setup(r => r.GetirRaporlar()).Returns(new List<RaporTanimi>());
            _yetkiMock.Setup(y => y.GetYetkiler(5)).Returns(new List<FirmaIsyeriYetkiDTO>());
            _lookupMock.Setup(l => l.GetIsyerleri(1)).Returns(new List<LookupItem>());
            var sut = CreateSut();

            var sonuc = sut.Index(procedureAdi: null);

            var view = sonuc.Should().BeOfType<ViewResult>().Subject;
            view.Model.Should().BeNull();
        }

        [Fact]
        public void Index_YetkiliIsyeri10_Sadece10Ve0Gonderilir()
        {
            _authMock.Setup(a => a.ViewAbility("Raporlar")).Returns(true);
            _authMock.Setup(a => a.Can(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _sessionMock.Setup(s => s.AktifFirmaId).Returns(1);
            _sessionMock.Setup(s => s.AktifKullaniciId).Returns(5);
            _sessionMock.Setup(s => s.IsAdmin()).Returns(false);

            var yetkiler = new List<FirmaIsyeriYetkiDTO> { new() { FirmaId = 1, IsyeriId = 10 } };
            _yetkiMock.Setup(y => y.GetYetkiler(5)).Returns(yetkiler);
            _kullaniciMock.Setup(k => k.GetFirmayaAitIsyeriIdleri(1)).Returns(new List<int> { 10, 20, 30 });
            _yetkiMock.Setup(y => y.BuildIsyeriIdListCsv(1, yetkiler, false, It.IsAny<IReadOnlyList<int>>()))
                .Returns("10,0");
            _lookupMock.Setup(l => l.GetIsyerleri(1)).Returns(new List<LookupItem> { new() { Id = 10, Ad = "Merkez" } });
            _raporMock.Setup(r => r.GetirRaporlar()).Returns(new List<RaporTanimi> { new() { ProcedureAdi = "sp_test", RaporAdi = "Test" } });

            Dictionary<string, object>? captured = null;
            _raporMock.Setup(r => r.CalistirRapor("sp_test", It.IsAny<Dictionary<string, object>>()))
                .Callback<string, Dictionary<string, object>>((_, p) => captured = p)
                .Returns(new DataTable());

            var sut = CreateSut();
            sut.Index(procedureAdi: "sp_test");

            captured.Should().NotBeNull();
            captured!["@IsyeriIdList"].Should().Be("10,0");
        }

        [Fact]
        public void Index_SeciliIsyeri20_Csv10_20_0()
        {
            _authMock.Setup(a => a.ViewAbility("Raporlar")).Returns(true);
            _authMock.Setup(a => a.Can(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _sessionMock.Setup(s => s.AktifFirmaId).Returns(1);
            _sessionMock.Setup(s => s.AktifKullaniciId).Returns(5);
            _sessionMock.Setup(s => s.IsAdmin()).Returns(false);

            var yetkiler = new List<FirmaIsyeriYetkiDTO>
            {
                new() { FirmaId = 1, IsyeriId = 10 },
                new() { FirmaId = 1, IsyeriId = 20 }
            };
            _yetkiMock.Setup(y => y.GetYetkiler(5)).Returns(yetkiler);
            _kullaniciMock.Setup(k => k.GetFirmayaAitIsyeriIdleri(1)).Returns(new List<int> { 10, 20 });
            _yetkiMock.Setup(y => y.BuildIsyeriIdListCsv(1, yetkiler, false, It.IsAny<IReadOnlyList<int>>()))
                .Returns("10,20,0");
            _lookupMock.Setup(l => l.GetIsyerleri(1)).Returns(new List<LookupItem>());
            _raporMock.Setup(r => r.GetirRaporlar()).Returns(new List<RaporTanimi> { new() { ProcedureAdi = "sp_test", RaporAdi = "Test" } });

            Dictionary<string, object>? captured = null;
            _raporMock.Setup(r => r.CalistirRapor("sp_test", It.IsAny<Dictionary<string, object>>()))
                .Callback<string, Dictionary<string, object>>((_, p) => captured = p)
                .Returns(new DataTable());

            var sut = CreateSut();
            sut.Index(procedureAdi: "sp_test", isyeriIds: "20");

            captured!["@IsyeriIdList"].Should().Be("20,0");
        }

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

        [Fact]
        public void ExportPdf_Yetkisiz_403Doner()
        {
            _authMock.Setup(a => a.Can("Raporlar", YetkiTipleri.Export)).Returns(false);
            var sut = CreateSut();

            var sonuc = sut.ExportPdf();

            var status = sonuc.Should().BeOfType<ObjectResult>().Subject;
            status.StatusCode.Should().Be(403);
        }

        private sealed class FakeSessionFeature : ISessionFeature
        {
            public FakeSessionFeature(ISession session) => Session = session;
            public ISession Session { get; set; }
        }
    }
}
