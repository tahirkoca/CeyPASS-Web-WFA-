using CeyPASS.Business.Abstractions;
using CeyPASS.Business.Services;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using FluentAssertions;
using Moq;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    public class AuthorizationServiceTests
    {
        private readonly Mock<ISessionContext> _sessionMock = new();
        private readonly Mock<IAuthorizationRepository> _repoMock = new();
        private readonly AuthorizationService _sut;

        public AuthorizationServiceTests()
        {
            _sut = new AuthorizationService(_sessionMock.Object, _repoMock.Object);
        }

        // ─── Can() ────────────────────────────────────────────────────────────

        [Fact]
        public void Can_AktifKullaniciYok_FalseDoner()
        {
            _sessionMock.Setup(s => s.AktifKullaniciId).Returns((int?)null);

            _sut.Can("Personel", YetkiTipleri.View).Should().BeFalse();
        }

        [Fact]
        public void Can_KullaniciIdSifir_FalseDoner()
        {
            _sessionMock.Setup(s => s.AktifKullaniciId).Returns(0);

            _sut.Can("Personel", YetkiTipleri.View).Should().BeFalse();
        }

        [Fact]
        public void Can_RolId1_HerZamanTrue_RepoCagrilmaz()
        {
            _sessionMock.Setup(s => s.AktifKullaniciId).Returns(5);
            _sessionMock.Setup(s => s.RolId).Returns(1);

            _sut.Can("Personel", YetkiTipleri.Delete).Should().BeTrue();
            _repoMock.Verify(r => r.CheckPermission(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Can_RolId2_HerZamanTrue_RepoCagrilmaz()
        {
            _sessionMock.Setup(s => s.AktifKullaniciId).Returns(5);
            _sessionMock.Setup(s => s.RolId).Returns(2);

            _sut.Can("Personel", YetkiTipleri.Delete).Should().BeTrue();
            _repoMock.Verify(r => r.CheckPermission(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Can_NormalKullanici_RepoyuSorgulaTrue_TrueDoner()
        {
            _sessionMock.Setup(s => s.AktifKullaniciId).Returns(10);
            _sessionMock.Setup(s => s.RolId).Returns(3);
            _repoMock.Setup(r => r.CheckPermission(10, "Personel", YetkiTipleri.View)).Returns(true);

            _sut.Can("Personel", YetkiTipleri.View).Should().BeTrue();
        }

        [Fact]
        public void Can_NormalKullanici_RepoyuSorgulaFalse_FalseDoner()
        {
            _sessionMock.Setup(s => s.AktifKullaniciId).Returns(10);
            _sessionMock.Setup(s => s.RolId).Returns(3);
            _repoMock.Setup(r => r.CheckPermission(10, "Personel", YetkiTipleri.View)).Returns(false);

            _sut.Can("Personel", YetkiTipleri.View).Should().BeFalse();
        }

        // ─── Ability kısayolları ──────────────────────────────────────────────

        [Fact]
        public void ViewAbility_ViewYetkiKoduyleCagrilir()
        {
            _sessionMock.Setup(s => s.AktifKullaniciId).Returns(10);
            _sessionMock.Setup(s => s.RolId).Returns(3);
            _repoMock.Setup(r => r.CheckPermission(10, "Personel", YetkiTipleri.View)).Returns(true);

            _sut.ViewAbility("Personel").Should().BeTrue();
            _repoMock.Verify(r => r.CheckPermission(10, "Personel", YetkiTipleri.View), Times.Once);
        }

        [Fact]
        public void CreateAbility_CreateYetkiKoduyleCagrilir()
        {
            _sessionMock.Setup(s => s.AktifKullaniciId).Returns(10);
            _sessionMock.Setup(s => s.RolId).Returns(3);
            _repoMock.Setup(r => r.CheckPermission(10, "Personel", YetkiTipleri.Create)).Returns(true);

            _sut.CreateAbility("Personel").Should().BeTrue();
            _repoMock.Verify(r => r.CheckPermission(10, "Personel", YetkiTipleri.Create), Times.Once);
        }

        [Fact]
        public void DeleteAbility_DeleteYetkiKoduyleCagrilir()
        {
            _sessionMock.Setup(s => s.AktifKullaniciId).Returns(10);
            _sessionMock.Setup(s => s.RolId).Returns(3);
            _repoMock.Setup(r => r.CheckPermission(10, "Personel", YetkiTipleri.Delete)).Returns(false);

            _sut.DeleteAbility("Personel").Should().BeFalse();
            _repoMock.Verify(r => r.CheckPermission(10, "Personel", YetkiTipleri.Delete), Times.Once);
        }

        [Fact]
        public void ExportAbility_ExportYetkiKoduyleCagrilir()
        {
            _sessionMock.Setup(s => s.AktifKullaniciId).Returns(10);
            _sessionMock.Setup(s => s.RolId).Returns(3);
            _repoMock.Setup(r => r.CheckPermission(10, "Rapor", YetkiTipleri.Export)).Returns(true);

            _sut.ExportAbility("Rapor").Should().BeTrue();
            _repoMock.Verify(r => r.CheckPermission(10, "Rapor", YetkiTipleri.Export), Times.Once);
        }

        [Fact]
        public void ApproveAbility_ApproveYetkiKoduyleCagrilir()
        {
            _sessionMock.Setup(s => s.AktifKullaniciId).Returns(10);
            _sessionMock.Setup(s => s.RolId).Returns(3);
            _repoMock.Setup(r => r.CheckPermission(10, "Puantaj", YetkiTipleri.Approve)).Returns(true);

            _sut.ApproveAbility("Puantaj").Should().BeTrue();
            _repoMock.Verify(r => r.CheckPermission(10, "Puantaj", YetkiTipleri.Approve), Times.Once);
        }

        [Fact]
        public void UpdateAbility_UpdateYetkiKoduyleCagrilir()
        {
            _sessionMock.Setup(s => s.AktifKullaniciId).Returns(10);
            _sessionMock.Setup(s => s.RolId).Returns(3);
            _repoMock.Setup(r => r.CheckPermission(10, "Cihazlar", YetkiTipleri.Update)).Returns(true);

            _sut.UpdateAbility("Cihazlar").Should().BeTrue();
            _repoMock.Verify(r => r.CheckPermission(10, "Cihazlar", YetkiTipleri.Update), Times.Once);
        }
    }
}
