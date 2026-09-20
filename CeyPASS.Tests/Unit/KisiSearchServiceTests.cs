using CeyPASS.Business.Services;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    /// <summary>
    /// Kişi arama (sayfalı) servisinin filtre ve sayfalama parametrelerini repoya iletmesi.
    /// </summary>
    public class KisiSearchServiceTests
    {
        private readonly Mock<IKisiRepository> _repoMock = new();
        private readonly KisiQueryService _sut;

        public KisiSearchServiceTests()
        {
            _sut = new KisiQueryService(_repoMock.Object);
        }

        /// <summary>
        /// SearchKisilerPaged filtre, sayfa ve boyutu SearchByFirmaPaged ile aynı şekilde iletir.
        /// </summary>
        [Fact]
        public void SearchKisilerPaged_RepoMetodunuCagirir()
        {
            var filter = new KisiSearchFilter
            {
                FirmaId = 101,
                Sicil = "123"
            };
            var beklenen = new List<KisiSearchResultItem>
            {
                new KisiSearchResultItem { PersonelId = "123", AdSoyad = "Test Kisi" }
            };

            _repoMock
                .Setup(r => r.SearchByFirmaPaged(filter, 1, 25, out It.Ref<int>.IsAny))
                .Returns(beklenen);

            var sonuc = _sut.SearchKisilerPaged(filter, 1, 25, out var total);

            _repoMock.Verify(r => r.SearchByFirmaPaged(filter, 1, 25, out It.Ref<int>.IsAny), Times.Once);
            sonuc.Should().BeEquivalentTo(beklenen);
        }

        /// <summary>
        /// E-posta filtresi repoya aynen aktarılır.
        /// </summary>
        [Fact]
        public void SearchKisilerPaged_EmailFiltresi_RepoSonucunuDoner()
        {
            var filter = new KisiSearchFilter
            {
                FirmaId = 101,
                Email = "test@firma.com"
            };

            _repoMock
                .Setup(r => r.SearchByFirmaPaged(filter, 2, 10, out It.Ref<int>.IsAny))
                .Returns(new List<KisiSearchResultItem>());

            _sut.SearchKisilerPaged(filter, 2, 10, out _);

            _repoMock.Verify(r => r.SearchByFirmaPaged(
                It.Is<KisiSearchFilter>(f => f.Email == "test@firma.com"),
                2,
                10,
                out It.Ref<int>.IsAny), Times.Once);
        }
    }
}
