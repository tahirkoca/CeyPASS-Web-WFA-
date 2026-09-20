using CeyPASS.Business.Services;
using CeyPASS.DataAccess.Abstractions;
using Moq;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    /// <summary>
    /// Turnike/cihaz kayıtlarında aktif/pasif durumunun repository üzerinden güncellenmesi.
    /// </summary>
    public class CihazServiceTests
    {
        private readonly Mock<ICihazRepository> _repoMock = new();
        private readonly CihazService _sut;

        public CihazServiceTests()
        {
            _sut = new CihazService(_repoMock.Object);
        }

        /// <summary>
        /// PasifYap cihazı SetAktif(id, false) ile devre dışı bırakır.
        /// </summary>
        [Fact]
        public void PasifYap_SetAktifFalseIleCagrilir()
        {
            _sut.PasifYap(42);

            _repoMock.Verify(r => r.SetAktif(42, false), Times.Once);
        }

        /// <summary>
        /// AktifYap cihazı SetAktif(id, true) ile etkinleştirir.
        /// </summary>
        [Fact]
        public void AktifYap_SetAktifTrueIleCagrilir()
        {
            _sut.AktifYap(99);

            _repoMock.Verify(r => r.SetAktif(99, true), Times.Once);
        }
    }
}
