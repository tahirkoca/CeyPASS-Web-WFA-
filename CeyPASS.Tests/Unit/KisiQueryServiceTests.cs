using CeyPASS.Business.Services;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using FluentAssertions;
using Moq;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    public class KisiQueryServiceTests
    {
        private readonly Mock<IKisiRepository> _repoMock = new();
        private readonly KisiQueryService _sut;

        public KisiQueryServiceTests()
        {
            _sut = new KisiQueryService(_repoMock.Object);
        }

        [Fact]
        public void GetDetayOrPuantajsizKart_NullId_NullVeFalseDoner()
        {
            var (detay, isPuantajsiz) = _sut.GetDetayOrPuantajsizKart(null);

            detay.Should().BeNull();
            isPuantajsiz.Should().BeFalse();
            _repoMock.Verify(r => r.GetDetay(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void GetDetayOrPuantajsizKart_WhitespaceId_NullVeFalseDoner()
        {
            var (detay, isPuantajsiz) = _sut.GetDetayOrPuantajsizKart("   ");

            detay.Should().BeNull();
            isPuantajsiz.Should().BeFalse();
            _repoMock.Verify(r => r.GetDetay(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void GetDetayOrPuantajsizKart_GecerliId_RepoCagrilirVeIsPuantajsizFalse()
        {
            var beklenen = new KisiDetay { PersonelId = "ABC123" };
            _repoMock.Setup(r => r.GetDetay("ABC123")).Returns(beklenen);

            var (detay, isPuantajsiz) = _sut.GetDetayOrPuantajsizKart("ABC123");

            _repoMock.Verify(r => r.GetDetay("ABC123"), Times.Once);
            detay.Should().Be(beklenen);
            isPuantajsiz.Should().BeFalse();
        }
    }
}
