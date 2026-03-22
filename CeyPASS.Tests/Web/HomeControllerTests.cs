using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Web.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace CeyPASS.Tests.Web
{
    public class HomeControllerTests
    {
        private readonly Mock<IDashboardService> _dashboardMock = new();
        private readonly Mock<ISessionContext> _sessionMock = new();
        private readonly Mock<IAuthorizationService> _authMock = new();
        private readonly Mock<IFirmaService> _firmaMock = new();
        private readonly Mock<IPuantajService> _puantajMock = new();
        private readonly HomeController _sut;

        public HomeControllerTests()
        {
            _sut = new HomeController(
                _dashboardMock.Object,
                _sessionMock.Object,
                _authMock.Object,
                _firmaMock.Object,
                _puantajMock.Object);

            var httpContext = new DefaultHttpContext();
            _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _sut.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            // Default: logged-in non-admin user with firma 1
            _sessionMock.Setup(s => s.CurrentUser).Returns(new AuthUserDTO { KullaniciId = 1, FirmaId = 1 });
            _sessionMock.Setup(s => s.AktifFirmaId).Returns(1);
            _sessionMock.Setup(s => s.AktifKullaniciId).Returns(1);
            _sessionMock.Setup(s => s.RolId).Returns(3);
        }

        // ─── SetFirma ─────────────────────────────────────────────────────────

        [Fact]
        public void SetFirma_FirmaIdListedeYoksa_MevcutSessionFirmayaFallback()
        {
            // User has access to firmalar 2 and 3 only
            _sessionMock.Setup(s => s.RolId).Returns(3);
            _puantajMock.Setup(p => p.GetKullaniciFirmaIsyeriYetkileri(1))
                .Returns(new List<FirmaIsyeriYetkiDTO>
                {
                    new FirmaIsyeriYetkiDTO { FirmaId = 2 },
                    new FirmaIsyeriYetkiDTO { FirmaId = 3 }
                });
            _firmaMock.Setup(f => f.GetAll()).Returns(new List<Firma>
            {
                new Firma { FirmaId = 2, FirmaAdi = "Firma B" },
                new Firma { FirmaId = 3, FirmaAdi = "Firma C" }
            });

            // Request firmaId = 99 (not in the authorized list)
            _sut.SetFirma(firmaId: 99, returnUrl: null);

            // AktifFirmaId should be set to session firma (1), not 99
            _sessionMock.VerifySet(s => s.AktifFirmaId = 1, Times.Once);
        }

        [Fact]
        public void SetFirma_GecerliReturnUrl_RedirectDoner()
        {
            _sessionMock.Setup(s => s.RolId).Returns(1);
            _firmaMock.Setup(f => f.GetAll()).Returns(new List<Firma>
            {
                new Firma { FirmaId = 2, FirmaAdi = "Firma B" }
            });

            var sonuc = _sut.SetFirma(firmaId: 2, returnUrl: "/rapor");

            sonuc.Should().BeOfType<RedirectResult>()
                 .Which.Url.Should().Be("/rapor");
        }

        [Fact]
        public void SetFirma_KotuReturnUrl_IndexeRedirect()
        {
            _sessionMock.Setup(s => s.RolId).Returns(1);
            _firmaMock.Setup(f => f.GetAll()).Returns(new List<Firma>
            {
                new Firma { FirmaId = 2, FirmaAdi = "Firma B" }
            });

            var sonuc = _sut.SetFirma(firmaId: 2, returnUrl: "//evil.com");

            sonuc.Should().BeOfType<RedirectToActionResult>()
                 .Which.ActionName.Should().Be("Index");
        }

        // ─── GetFirmalarForUser (private) via Index ───────────────────────────

        [Fact]
        public void Index_YetkisizKullaniciYetkisiZFirmalar_NullDoner()
        {
            _authMock.Setup(a => a.ViewAbility("Dashboard")).Returns(true);
            _sessionMock.Setup(s => s.RolId).Returns(3);
            _sessionMock.Setup(s => s.AktifFirmaId).Returns(1);

            // No yetkiler → GetFirmalarForUser returns null
            _puantajMock.Setup(p => p.GetKullaniciFirmaIsyeriYetkileri(1))
                .Returns(new List<FirmaIsyeriYetkiDTO>());
            _firmaMock.Setup(f => f.GetAll()).Returns(new List<Firma>());
            _dashboardMock.Setup(d => d.GetDashboardForToday(It.IsAny<int>()))
                .Returns(new DashboardResult());

            var sonuc = _sut.Index();

            var view = sonuc.Should().BeOfType<ViewResult>().Subject;
            // ViewBag.ShowFirmaCombo should be false when firmalar is null
            ((bool)_sut.ViewBag.ShowFirmaCombo).Should().BeFalse();
        }
    }
}
