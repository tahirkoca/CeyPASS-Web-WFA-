using CeyPASS.Business.Services;
using CeyPASS.DataAccess.Abstractions;
using Moq;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    public class CihazServiceTests
    {
        private readonly Mock<ICihazRepository> _repoMock = new();
        private readonly CihazService _sut;

        public CihazServiceTests()
        {
            _sut = new CihazService(_repoMock.Object);
        }

        [Fact]
        public void PasifYap_SetAktifFalseIleCagrilir()
        {
            _sut.PasifYap(42);

            _repoMock.Verify(r => r.SetAktif(42, false), Times.Once);
        }

        [Fact]
        public void AktifYap_SetAktifTrueIleCagrilir()
        {
            _sut.AktifYap(99);

            _repoMock.Verify(r => r.SetAktif(99, true), Times.Once);
        }
    }
}
