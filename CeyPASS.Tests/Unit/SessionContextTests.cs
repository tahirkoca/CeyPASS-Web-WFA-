using CeyPASS.Entities.Concrete;
using CeyPASS.Web.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    public class SessionContextTests
    {
        // Null HttpContext → LoadFromSession and SaveToSession are no-ops.
        // Only in-memory state is exercised.
        private readonly SessionContext _sut;

        public SessionContextTests()
        {
            var mockAccessor = new Mock<IHttpContextAccessor>();
            mockAccessor.Setup(a => a.HttpContext).Returns((HttpContext)null);
            _sut = new SessionContext(mockAccessor.Object);
        }

        [Fact]
        public void CurrentUser_BaslangictaNullDoner()
        {
            _sut.CurrentUser.Should().BeNull();
        }

        [Fact]
        public void SetCurrentUser_SonraCurrentUserDolu()
        {
            var user = new AuthUserDTO { KullaniciId = 5, FirmaId = 2, Rol = "Admin" };

            _sut.SetCurrentUser(user);

            _sut.CurrentUser.Should().NotBeNull();
            _sut.CurrentUser.KullaniciId.Should().Be(5);
        }

        [Fact]
        public void AktifKullaniciId_CurrentUserDoluyken_UserIdDoner()
        {
            _sut.SetCurrentUser(new AuthUserDTO { KullaniciId = 7, FirmaId = 1 });

            _sut.AktifKullaniciId.Should().Be(7);
        }

        [Fact]
        public void AktifFirmaId_CurrentUserDoluyken_FirmaIdDoner()
        {
            _sut.SetCurrentUser(new AuthUserDTO { KullaniciId = 1, FirmaId = 3 });

            _sut.AktifFirmaId.Should().Be(3);
        }

        [Fact]
        public void Clear_SonraCurrentUserVeAktifIdNullDoner()
        {
            _sut.SetCurrentUser(new AuthUserDTO { KullaniciId = 10, FirmaId = 5 });

            _sut.Clear();

            _sut.CurrentUser.Should().BeNull();
            _sut.AktifKullaniciId.Should().BeNull();
            _sut.AktifFirmaId.Should().BeNull();
        }
    }
}
