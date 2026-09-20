using CeyPASS.Business.Services;
using CeyPASS.DataAccess.Abstractions;
using FluentAssertions;
using Moq;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    /// <summary>
    /// Çalışma statüsü tanımlarında otomatik ID ile ekleme (AddAuto) davranışı.
    /// </summary>
    public class CalismaStatuServiceTests
    {
        private readonly Mock<ICalismaStatuRepository> _repoMock = new();
        private readonly CalismaStatuService _sut;

        public CalismaStatuServiceTests()
        {
            _sut = new CalismaStatuService(_repoMock.Object);
        }

        // ─── AddAuto ──────────────────────────────────────────────────────────

        /// <summary>
        /// AddAuto bir sonraki ID ile Insert çağırır; başarılı insert true döner.
        /// </summary>
        [Fact]
        public void AddAuto_GetNextIdSonucuInsertaGonder_TrueDoner()
        {
            _repoMock.Setup(r => r.GetNextId()).Returns(5);
            _repoMock.Setup(r => r.Insert(5, "Tam Zamanlı")).Returns(true);

            var result = _sut.AddAuto("Tam Zamanlı");

            result.Should().BeTrue();
            _repoMock.Verify(r => r.Insert(5, "Tam Zamanlı"), Times.Once);
        }

        /// <summary>
        /// Insert başarısızsa AddAuto false döner.
        /// </summary>
        [Fact]
        public void AddAuto_InsertBasarisiz_FalseDoner()
        {
            _repoMock.Setup(r => r.GetNextId()).Returns(5);
            _repoMock.Setup(r => r.Insert(5, "Yarı Zamanlı")).Returns(false);

            var result = _sut.AddAuto("Yarı Zamanlı");

            result.Should().BeFalse();
        }
    }
}
