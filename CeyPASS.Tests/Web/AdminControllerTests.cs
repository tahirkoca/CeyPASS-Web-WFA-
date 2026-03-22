using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Web.Controllers;
using CeyPASS.Web.Models.Admin;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CeyPASS.Tests.Web
{
    public class AdminControllerTests
    {
        private readonly Mock<ISessionContext> _sessionMock = new();
        private readonly Mock<IFirmaService> _firmaMock = new();
        private readonly Mock<IIsyeriService> _isyeriMock = new();
        private readonly Mock<ICihazService> _cihazMock = new();
        private readonly Mock<IDepartmanService> _departmanMock = new();
        private readonly Mock<IPozisyonService> _pozisyonMock = new();
        private readonly Mock<IResmiTatilService> _resmiTatilMock = new();
        private readonly Mock<ICalismaStatuService> _calismaStatuMock = new();
        private readonly Mock<ICalismaSekliService> _calismaSekliMock = new();
        private readonly Mock<INotificationService> _notificationMock = new();
        private readonly Mock<IWebHostEnvironment> _envMock = new();
        private readonly AdminController _sut;

        public AdminControllerTests()
        {
            // IWebHostEnvironment: return a non-existent path so logo = null
            _envMock.Setup(e => e.WebRootPath).Returns("C:\\nonexistent");
            _envMock.Setup(e => e.ContentRootPath).Returns("C:\\nonexistent");

            _sut = new AdminController(
                _sessionMock.Object,
                _firmaMock.Object,
                _isyeriMock.Object,
                _cihazMock.Object,
                _departmanMock.Object,
                _pozisyonMock.Object,
                _resmiTatilMock.Object,
                _calismaStatuMock.Object,
                _calismaSekliMock.Object,
                _notificationMock.Object,
                _envMock.Object);

            var httpContext = new DefaultHttpContext();
            _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _sut.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            // Admin user setup
            _sessionMock.Setup(s => s.CurrentUser).Returns(new AuthUserDTO { KullaniciId = 1 });
            _sessionMock.Setup(s => s.RolId).Returns(1);
        }

        private static GuncellemeMailViewModel GecerliModel(
            string yeniOzellikler = "Özellik 1") =>
            new GuncellemeMailViewModel
            {
                VersiyonNumarasi = "2.0.0",
                GuncellemeTipi = "Major",
                YayinTarihi = DateTime.Today,
                YeniOzelliklerMetni = yeniOzellikler
            };

        // ─── GuncellemeDogrula via GuncellemeMailOnizleme ─────────────────────

        [Fact]
        public void GuncellemeMailOnizleme_VersiyonBos_HataylaRedirect()
        {
            var model = new GuncellemeMailViewModel { VersiyonNumarasi = "", GuncellemeTipi = "Minor" };

            var sonuc = _sut.GuncellemeMailOnizleme(model);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            _sut.TempData["Error"].Should().NotBeNull();
        }

        [Fact]
        public void GuncellemeMailOnizleme_TipBos_HataylaRedirect()
        {
            var model = new GuncellemeMailViewModel { VersiyonNumarasi = "1.0.0", GuncellemeTipi = "" };

            var sonuc = _sut.GuncellemeMailOnizleme(model);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            _sut.TempData["Error"].Should().NotBeNull();
        }

        [Fact]
        public void GuncellemeMailOnizleme_TumListelerBos_HataylaRedirect()
        {
            var model = new GuncellemeMailViewModel
            {
                VersiyonNumarasi = "1.0.0",
                GuncellemeTipi = "Minor",
                YeniOzelliklerMetni = null,
                IyilestirmelerMetni = null,
                HataDuzeltmeleriMetni = null,
                KritikDegisikliklerMetni = null
            };

            var sonuc = _sut.GuncellemeMailOnizleme(model);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            _sut.TempData["Error"].Should().NotBeNull();
        }

        // ─── SatirlariListeye via GuncellemeMailOnizleme ──────────────────────

        [Fact]
        public void GuncellemeMailOnizleme_CokSatirliMetin_DogruListeyeAyrilir()
        {
            GuncellemeNotifikasyonDTO capturedDto = null;
            _notificationMock.Setup(n => n.OnizlemeHtmlOlustur(
                    It.IsAny<GuncellemeNotifikasyonDTO>(), It.IsAny<string>()))
                .Callback<GuncellemeNotifikasyonDTO, string>((dto, _) => capturedDto = dto)
                .Returns("<html>test</html>");

            var model = GecerliModel("Özellik 1\nÖzellik 2");

            _sut.GuncellemeMailOnizleme(model);

            capturedDto.Should().NotBeNull();
            capturedDto.YeniOzellikler.Should().HaveCount(2);
            capturedDto.YeniOzellikler[0].Should().Be("Özellik 1");
            capturedDto.YeniOzellikler[1].Should().Be("Özellik 2");
        }

        [Fact]
        public void GuncellemeMailOnizleme_BosluklarVeYeniSatirlar_Temizlenir()
        {
            GuncellemeNotifikasyonDTO capturedDto = null;
            _notificationMock.Setup(n => n.OnizlemeHtmlOlustur(
                    It.IsAny<GuncellemeNotifikasyonDTO>(), It.IsAny<string>()))
                .Callback<GuncellemeNotifikasyonDTO, string>((dto, _) => capturedDto = dto)
                .Returns("<html>test</html>");

            // Boş satırlar ve whitespace'ler filtrelenmelidir
            var model = GecerliModel("Madde 1\n\n   \r\nMadde 2");

            _sut.GuncellemeMailOnizleme(model);

            capturedDto.YeniOzellikler.Should().HaveCount(2);
            capturedDto.YeniOzellikler[0].Should().Be("Madde 1");
            capturedDto.YeniOzellikler[1].Should().Be("Madde 2");
        }

        [Fact]
        public void GuncellemeMailOnizleme_Gecerli_ContentResultDoner()
        {
            _notificationMock.Setup(n => n.OnizlemeHtmlOlustur(
                    It.IsAny<GuncellemeNotifikasyonDTO>(), It.IsAny<string>()))
                .Returns("<html>önizleme</html>");

            var sonuc = _sut.GuncellemeMailOnizleme(GecerliModel());

            var content = sonuc.Should().BeOfType<ContentResult>().Subject;
            content.Content.Should().Contain("önizleme");
        }

        // ─── Index ────────────────────────────────────────────────────────────

        [Fact]
        public void Index_AdminDegil_HomeIndexeYonlendirir()
        {
            _sessionMock.Setup(s => s.RolId).Returns(3);

            _firmaMock.Setup(f => f.GetAll()).Returns(new List<Firma>());
            _isyeriMock.Setup(i => i.GetListForAdmin()).Returns(new List<IsyeriItem>());
            _cihazMock.Setup(c => c.GetListe(It.IsAny<bool>(), It.IsAny<int?>())).Returns(new List<CihazListDTO>());
            _departmanMock.Setup(d => d.GetListForAdmin()).Returns(new List<DepartmanListDTO>());
            _pozisyonMock.Setup(p => p.GetListForAdmin()).Returns(new List<PozisyonListDTO>());
            _resmiTatilMock.Setup(r => r.GetList(It.IsAny<int?>())).Returns(new List<ResmiTatilDTO>());
            _calismaStatuMock.Setup(c => c.GetAll()).Returns(new List<LookupItem>());
            _calismaSekliMock.Setup(c => c.GetAllForAdmin()).Returns(new List<CalismaSekli>());

            var sonuc = _sut.Index(null);

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Home");
        }

        [Fact]
        public void Index_Admin_ViewDoner()
        {
            _firmaMock.Setup(f => f.GetAll()).Returns(new List<Firma>());
            _isyeriMock.Setup(i => i.GetListForAdmin()).Returns(new List<IsyeriItem>());
            _cihazMock.Setup(c => c.GetListe(It.IsAny<bool>(), It.IsAny<int?>())).Returns(new List<CihazListDTO>());
            _departmanMock.Setup(d => d.GetListForAdmin()).Returns(new List<DepartmanListDTO>());
            _pozisyonMock.Setup(p => p.GetListForAdmin()).Returns(new List<PozisyonListDTO>());
            _resmiTatilMock.Setup(r => r.GetList(It.IsAny<int?>())).Returns(new List<ResmiTatilDTO>());
            _calismaStatuMock.Setup(c => c.GetAll()).Returns(new List<LookupItem>());
            _calismaSekliMock.Setup(c => c.GetAllForAdmin()).Returns(new List<CalismaSekli>());

            var sonuc = _sut.Index(null);

            sonuc.Should().BeOfType<ViewResult>();
        }

        // ─── GuncellemeMailGonder ─────────────────────────────────────────────

        [Fact]
        public async Task GuncellemeMailGonder_AdminDegil_LoginaYonlendirir()
        {
            _sessionMock.Setup(s => s.CurrentUser).Returns((AuthUserDTO)null!);

            var sonuc = await _sut.GuncellemeMailGonder(GecerliModel());

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Login");
        }

        [Fact]
        public async Task GuncellemeMailGonder_Gecerli_BasariliTempDataSet()
        {
            _notificationMock.Setup(n => n.GuncellemeNotifikasyonuGonderAsync(
                    It.IsAny<GuncellemeNotifikasyonDTO>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            var sonuc = await _sut.GuncellemeMailGonder(GecerliModel());

            var redirect = sonuc.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            _sut.TempData["Success"].Should().NotBeNull();
        }
    }
}
