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
        private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private readonly PersonelController _sut;

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
                _cache);

            var httpContext = new DefaultHttpContext();
            _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _sut.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            // Default lookup stubs so LoadLookupData never throws
            _lookupMock.Setup(l => l.GetDepartmanlar()).Returns(new List<LookupItem>());
            _lookupMock.Setup(l => l.GetPozisyonlar()).Returns(new List<LookupItem>());
            _lookupMock.Setup(l => l.GetIsyerleri(It.IsAny<int>())).Returns(new List<LookupItem>());
            _lookupMock.Setup(l => l.GetBolumler(It.IsAny<int>())).Returns(new List<LookupItem>());
            _lookupMock.Setup(l => l.GetCalismaStatuleri()).Returns(new List<LookupItem>());
            _calismaSekliMock.Setup(c => c.GetAll(It.IsAny<int>(), It.IsAny<bool>())).Returns(new List<CalismaSekli>());
            _firmaMock.Setup(f => f.GetAll()).Returns(new List<Firma>());
        }

        // ─── Index ─────────────────────────────────────────────────────────────

        [Fact]
        public void Index_YetkiYok_HomeaYonlendirir()
        {
            _authMock.Setup(a => a.ViewAbility("Personeller")).Returns(false);

            var sonuc = _sut.Index();

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Home");
        }

        [Fact]
        public void Index_YetkiVar_ViewDoner()
        {
            _authMock.Setup(a => a.ViewAbility("Personeller")).Returns(true);
            _authMock.Setup(a => a.Can(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
            _sessionMock.Setup(s => s.AktifFirmaId).Returns(1);
            _sessionMock.Setup(s => s.RolId).Returns(3);

            int totalCount = 0;
            _kisiQueryMock.Setup(q => q.GetAktifKisilerByFirmaPaged(
                    It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool?>(),
                    It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>(), out totalCount))
                .Returns(new List<KisiListItem>());

            var sonuc = _sut.Index();

            sonuc.Should().BeOfType<ViewResult>();
        }

        // ─── Create GET ───────────────────────────────────────────────────────

        [Fact]
        public void Create_GET_YetkiYok_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Personeller", YetkiTipleri.Create)).Returns(false);

            var sonuc = _sut.Create();

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
        }

        [Fact]
        public void Create_GET_YetkiVar_ViewDoner()
        {
            _authMock.Setup(a => a.Can("Personeller", YetkiTipleri.Create)).Returns(true);
            _sessionMock.Setup(s => s.AktifFirmaId).Returns(1);

            var sonuc = _sut.Create();

            sonuc.Should().BeOfType<ViewResult>();
        }

        // ─── Create POST ──────────────────────────────────────────────────────

        [Fact]
        public void Create_POST_YetkiYok_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Personeller", YetkiTipleri.Create)).Returns(false);

            var sonuc = _sut.Create(
                new Kisi { FirmaId = 1 },
                firmaPersoneli: true, puantajYapilabilir: true,
                yemekHakkiVar: false, gunlukYemekLimiti: 0,
                puantajsizKartId: null, puantajsizKartNo: null, puantajsizKartAdi: null,
                fotograf: null, ziyaretciMi: false, aracKartiMi: false, taseronCalisanMi: false);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
        }

        [Fact]
        public void Create_POST_ValidationHatasi_ViewDoner()
        {
            _authMock.Setup(a => a.Can("Personeller", YetkiTipleri.Create)).Returns(true);
            _sessionMock.Setup(s => s.AktifFirmaId).Returns(1);
            _kisiMock.Setup(k => k.ValidateKisiKayit(It.IsAny<KisiKayitValidasyonDTO>()))
                     .Returns((false, "Personel ID zorunludur."));

            var sonuc = _sut.Create(
                new Kisi { FirmaId = 1 },
                firmaPersoneli: true, puantajYapilabilir: true,
                yemekHakkiVar: false, gunlukYemekLimiti: 0,
                puantajsizKartId: null, puantajsizKartNo: null, puantajsizKartAdi: null,
                fotograf: null, ziyaretciMi: false, aracKartiMi: false, taseronCalisanMi: false);

            sonuc.Should().BeOfType<ViewResult>();
            _sut.ModelState.IsValid.Should().BeFalse();
        }

        // ─── Delete ───────────────────────────────────────────────────────────

        [Fact]
        public void Delete_YetkiYok_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Personeller", YetkiTipleri.Delete)).Returns(false);

            var sonuc = _sut.Delete("TEST001", null, null, null, null);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
        }

        [Fact]
        public void Delete_BosId_NotFoundDoner()
        {
            _authMock.Setup(a => a.Can("Personeller", YetkiTipleri.Delete)).Returns(true);

            var sonuc = _sut.Delete(null, null, null, null, null);

            sonuc.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public void Delete_Basarili_IndexeYonlendirir_VeTempDataSet()
        {
            _authMock.Setup(a => a.Can("Personeller", YetkiTipleri.Delete)).Returns(true);
            _kisiMock.Setup(k => k.KisiIstenCikar("TEST001", It.IsAny<DateTime>(), It.IsAny<string>()))
                     .Returns(true);

            var sonuc = _sut.Delete("TEST001", null, null, null, firmaId: 1);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            _sut.TempData["Success"].Should().NotBeNull();
        }

        // ─── Edit GET ─────────────────────────────────────────────────────────

        [Fact]
        public void Edit_GET_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Personeller", YetkiTipleri.Update)).Returns(false);

            var sonuc = _sut.Edit("P001", null, null);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Edit_GET_NullId_NotFoundDoner()
        {
            _authMock.Setup(a => a.Can("Personeller", YetkiTipleri.Update)).Returns(true);

            var sonuc = _sut.Edit(null, null, null);

            sonuc.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public void Edit_GET_KisiBulunamadi_NotFoundDoner()
        {
            _authMock.Setup(a => a.Can("Personeller", YetkiTipleri.Update)).Returns(true);
            _kisiQueryMock.Setup(q => q.GetKisiDetay("P001")).Returns((KisiDetay)null!);
            _sessionMock.Setup(s => s.AktifFirmaId).Returns(1);

            var sonuc = _sut.Edit("P001", null, null);

            sonuc.Should().BeOfType<NotFoundResult>();
        }

        // ─── Edit POST ────────────────────────────────────────────────────────

        [Fact]
        public void Edit_POST_Yetkisiz_IndexeYonlendirir()
        {
            _authMock.Setup(a => a.Can("Personeller", YetkiTipleri.Update)).Returns(false);

            var sonuc = _sut.Edit("P001", new KisiDetay(), false, false, false, 0, null, false, null, null, null);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            ((string)_sut.TempData["Error"]!).Should().NotBeNullOrEmpty();
        }

        // ─── Details ─────────────────────────────────────────────────────────

        [Fact]
        public void Details_NullId_NotFoundDoner()
        {
            var sonuc = _sut.Details(null, null, null);

            sonuc.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public void Details_KisiBulunamadi_NotFoundDoner()
        {
            _kisiQueryMock.Setup(q => q.GetKisiDetay("P999")).Returns((KisiDetay)null!);
            _sessionMock.Setup(s => s.AktifFirmaId).Returns(1);
            _firmaMock.Setup(f => f.GetAll()).Returns(new List<Firma>());

            var sonuc = _sut.Details("P999", null, null);

            sonuc.Should().BeOfType<NotFoundResult>();
        }

        // ─── AJAX Lookups ─────────────────────────────────────────────────────

        [Fact]
        public void GetDepartmanlar_JsonListDoner()
        {
            _lookupMock.Setup(l => l.GetDepartmanlar()).Returns(new List<LookupItem> { new LookupItem { Id = 1, Ad = "IT" } });

            var sonuc = _sut.GetDepartmanlar();

            sonuc.Should().BeOfType<JsonResult>();
        }

        [Fact]
        public void GetPozisyonlar_JsonListDoner()
        {
            _lookupMock.Setup(l => l.GetPozisyonlar()).Returns(new List<LookupItem> { new LookupItem { Id = 1, Ad = "Müdür" } });

            var sonuc = _sut.GetPozisyonlar();

            sonuc.Should().BeOfType<JsonResult>();
        }

        [Fact]
        public void GetIsyerleri_JsonListDoner()
        {
            _lookupMock.Setup(l => l.GetIsyerleri(1)).Returns(new List<LookupItem> { new LookupItem { Id = 10, Ad = "Merkez" } });

            var sonuc = _sut.GetIsyerleri(1);

            sonuc.Should().BeOfType<JsonResult>();
        }

        [Fact]
        public void GetBolumler_JsonListDoner()
        {
            _lookupMock.Setup(l => l.GetBolumler(1)).Returns(new List<LookupItem> { new LookupItem { Id = 5, Ad = "Yazılım" } });

            var sonuc = _sut.GetBolumler(1);

            sonuc.Should().BeOfType<JsonResult>();
        }
    }
}
