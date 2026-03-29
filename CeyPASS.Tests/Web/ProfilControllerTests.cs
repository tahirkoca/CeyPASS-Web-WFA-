using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Web.Controllers;
using CeyPASS.Web.Models.Profil;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using Xunit;

namespace CeyPASS.Tests.Web
{
    public class ProfilControllerTests
    {
        private readonly Mock<ISessionContext> _sessionMock = new();
        private readonly Mock<IAuthorizationService> _authMock = new();
        private readonly Mock<IIzinTalepService> _izinTalepServiceMock = new();
        private readonly Mock<IAvansService> _avansServiceMock = new();
        private readonly Mock<IIzinTipService> _izinTipServiceMock = new();
        private readonly Mock<IKisiIzinlerRepository> _kisiIzinlerRepoMock = new();
        private readonly Mock<IKisiQueryService> _kisiQueryServiceMock = new();
        private readonly Mock<IKisiEkraniLookUpService> _lookupMock = new();
        private readonly Mock<IKisiHareketService> _kisiHareketServiceMock = new();
        private readonly Mock<IUstYetkiliRepository> _ustYetkiliRepoMock = new();
        private readonly Mock<ISifreService> _sifreServiceMock = new();
        private readonly ProfilController _sut;

        public ProfilControllerTests()
        {
            _sut = new ProfilController(
                _sessionMock.Object,
                _authMock.Object,
                _izinTalepServiceMock.Object,
                _avansServiceMock.Object,
                _izinTipServiceMock.Object,
                _kisiIzinlerRepoMock.Object,
                _kisiQueryServiceMock.Object,
                _lookupMock.Object,
                _kisiHareketServiceMock.Object,
                _ustYetkiliRepoMock.Object,
                _sifreServiceMock.Object);

            var httpContext = new DefaultHttpContext();
            _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _sut.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            // defaults
            _sessionMock.Setup(s => s.AktifSicilNo).Returns("1001");
            _sessionMock.Setup(s => s.AktifKullaniciId).Returns(7);
            _authMock.Setup(a => a.ViewAbility("Profil")).Returns(true);
            _authMock.Setup(a => a.ViewAbility("IzinTalepleri")).Returns(true);
            _izinTipServiceMock.Setup(s => s.GetAktif()).Returns(new List<IzinTip>());
            _izinTalepServiceMock.Setup(s => s.PersonelTalepleri("1001")).Returns(new List<IzinTalep>());
            _kisiIzinlerRepoMock.Setup(r => r.GetByPerson("1001", null, null)).Returns(BuildKisiIzinDataTable());
            _kisiQueryServiceMock.Setup(s => s.GetKisiDetay("1001")).Returns(new KisiDetay
            {
                PersonelId = "1001",
                Ad = "Test",
                Soyad = "User",
                CepTel = "555",
                Email = "t@e.com",
                Fotograf = Array.Empty<byte>()
            });

            _lookupMock.Setup(l => l.GetDepartmanlar(It.IsAny<int?>())).Returns(new List<LookupItem>());
            _lookupMock.Setup(l => l.GetPozisyonlar(It.IsAny<int?>())).Returns(new List<LookupItem>());

            int total = 0;
            _kisiHareketServiceMock.Setup(s => s.GetByPersonsPaged(
                    It.IsAny<List<int>>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    out total))
                .Returns(new List<KisiHareketListRow>());
        }

        [Fact]
        public void Izinlerim_YetkiYok_HomeaYonlendirir_VeTempDataSet()
        {
            _authMock.Setup(a => a.ViewAbility("Profil")).Returns(false);

            var sonuc = _sut.Izinlerim();

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Home");
            _sut.TempData["Error"].Should().NotBeNull();
        }

        [Fact]
        public void Izinlerim_SicilYok_HomeaYonlendirir_VeTempDataSet()
        {
            _sessionMock.Setup(s => s.AktifSicilNo).Returns((string)null!);

            var sonuc = _sut.Izinlerim();

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Home");
            _sut.TempData["Error"].Should().NotBeNull();
        }

        [Fact]
        public void Izinlerim_YetkiVar_ViewDoner_ModelMapEdilir()
        {
            _izinTalepServiceMock.Setup(s => s.PersonelTalepleri("1001")).Returns(new List<IzinTalep>
            {
                new IzinTalep { TalepId = 1, PersonelId = "1001", IzinTipId = 7, Baslangic = DateTime.Today, Bitis = DateTime.Today.AddHours(1) }
            });

            var sonuc = _sut.Izinlerim();

            var view = sonuc.Should().BeOfType<ViewResult>().Subject;
            var model = view.Model.Should().BeAssignableTo<IzinlerimViewModel>().Subject;
            model.Talepler.Should().HaveCount(1);
            model.OnayliIzinler.Should().HaveCount(1);
            model.OnayliIzinler[0].KisiIzinId.Should().Be(10);
            model.OnayliIzinler[0].SaatlikIzinMi.Should().BeTrue();
            _izinTalepServiceMock.Verify(s => s.PersonelTalepleri("1001"), Times.Once);
            _kisiIzinlerRepoMock.Verify(r => r.GetByPerson("1001", null, null), Times.Once);
        }

        [Fact]
        public void KullanimImzaAt_TalepKisiyeAitDegil_HataVeRedirect()
        {
            _izinTalepServiceMock.Setup(s => s.PersonelTalepleri("1001")).Returns(new List<IzinTalep>());

            var sonuc = _sut.KullanimImzaAt(talepId: 99);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Izinlerim");
            _sut.TempData["Error"].Should().NotBeNull();
            _izinTalepServiceMock.Verify(s => s.KullanimImzaAt(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void KullanimImzaAt_TalepKisiyeAit_IzinTalepServiceCagrilir_VeSuccessSet()
        {
            _izinTalepServiceMock.Setup(s => s.PersonelTalepleri("1001")).Returns(new List<IzinTalep>
            {
                new IzinTalep { TalepId = 5, PersonelId = "1001" }
            });
            _izinTalepServiceMock.Setup(s => s.KullanimImzaAt(5, 7)).Returns(true);

            var sonuc = _sut.KullanimImzaAt(talepId: 5);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Izinlerim");
            _sut.TempData["Success"].Should().NotBeNull();
            _izinTalepServiceMock.Verify(s => s.KullanimImzaAt(5, 7), Times.Once);
        }

        private static DataTable BuildKisiIzinDataTable()
        {
            // Kolon adları KisiIzinlerRepositoryCore.GetByPerson ile aynı
            var dt = new DataTable();
            dt.Columns.Add("KisiIzinId", typeof(int));
            dt.Columns.Add("İzin Başlangıcı", typeof(DateTime));
            dt.Columns.Add("İzin Bitişi", typeof(DateTime));
            dt.Columns.Add("Süre(Saat)", typeof(decimal));
            dt.Columns.Add("Açıklama", typeof(string));
            dt.Columns.Add("İşlenme Tarihi", typeof(DateTime));
            dt.Columns.Add("Güncelleme Tarihi", typeof(DateTime));
            dt.Columns.Add("Saatlik İzin", typeof(string));

            dt.Rows.Add(
                10,
                new DateTime(2026, 1, 10, 8, 0, 0),
                new DateTime(2026, 1, 10, 12, 0, 0),
                4.0m,
                "Test",
                new DateTime(2026, 1, 9, 15, 30, 0),
                DBNull.Value,
                "EVET");

            return dt;
        }
    }
}

