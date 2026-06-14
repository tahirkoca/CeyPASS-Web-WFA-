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
    public class PersonelControllerTests
    {
        private readonly Mock<IKisiService> _kisiMock = new();
        private readonly Mock<IKisiQueryService> _kisiQueryMock = new();
        private readonly Mock<IKisiEkraniLookUpService> _lookupMock = new();
        private readonly Mock<ISessionContext> _sessionMock = new();
        private readonly Mock<IAuthorizationService> _authMock = new();
        private readonly Mock<ICalismaSekliService> _calismaSekliMock = new();
        private readonly Mock<IFirmaService> _firmaMock = new();
        private readonly Mock<IPuantajService> _puantajMock = new();
        private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private readonly PersonelController _sut;

        private const int DefaultPageSize = 20;

        public PersonelControllerTests()
        {
            _sut = new PersonelController(
                _kisiMock.Object,
                _kisiQueryMock.Object,
                _lookupMock.Object,
                _sessionMock.Object,
                _authMock.Object,
                _calismaSekliMock.Object,
                _firmaMock.Object,
                _puantajMock.Object,
                _cache);

            _puantajMock.Setup(p => p.GetKullaniciFirmaIsyeriYetkileri(It.IsAny<int>()))
                .Returns(new List<FirmaIsyeriYetkiDTO>());

            var httpContext = new DefaultHttpContext();
            _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _sut.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            // Default lookup stubs
            _lookupMock.Setup(l => l.GetDepartmanlar(It.IsAny<int?>())).Returns(new List<LookupItem>());
            _lookupMock.Setup(l => l.GetPozisyonlar(It.IsAny<int?>())).Returns(new List<LookupItem>());
            _lookupMock.Setup(l => l.GetIsyerleri(It.IsAny<int>())).Returns(new List<LookupItem>());
            _lookupMock.Setup(l => l.GetBolumler(It.IsAny<int>())).Returns(new List<LookupItem>());
            _lookupMock.Setup(l => l.GetCalismaStatuleri(It.IsAny<int?>())).Returns(new List<LookupItem>());
            _calismaSekliMock.Setup(c => c.GetAll(It.IsAny<int>(), It.IsAny<bool>())).Returns(new List<CalismaSekli>());
            _firmaMock.Setup(f => f.GetAll()).Returns(new List<Firma>());
        }

        [Fact]
        public void Index_Yetkisiz_HomeIndexeYonlendirir()
        {
            _authMock.Setup(a => a.ViewAbility("Personeller")).Returns(false);

            var sonuc = _sut.Index();

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Home");
        }

        [Fact]
        public void Delete_Basarili_IndexeYonlendirir_VeTempDataSet()
        {
            _authMock.Setup(a => a.Can("Personeller", YetkiTipleri.Delete)).Returns(true);
            _sessionMock.Setup(s => s.IsAdmin()).Returns(true);
            _kisiQueryMock.Setup(q => q.GetKisiDetay(It.IsAny<string>())).Returns(new KisiDetay { PersonelId = "TEST001", FirmaId = 1 });
            _kisiMock.Setup(k => k.KisiIstenCikar(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                     .Returns(true);

            var sonuc = _sut.Delete("TEST001", null, null, "P", firmaId: 1);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            _sut.TempData["Success"].Should().NotBeNull();
        }

        [Fact]
        public void Details_Basarili_ViewDoner()
        {
            _sessionMock.Setup(s => s.IsAdmin()).Returns(true);
            _kisiQueryMock.Setup(q => q.GetKisiDetay("TEST001")).Returns(new KisiDetay { PersonelId = "TEST001", FirmaId = 1 });

            var sonuc = _sut.Details("TEST001", "P", 1);

            sonuc.Should().BeOfType<ViewResult>();
        }
    }
}
