using CeyPASS.Business.Services;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using FluentAssertions;
using Moq;
using System.Data;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    /// <summary>
    /// Admin işyeri listesinin DataTable kaynağından DTO'ya dönüşümü ve boş/null güvenliği.
    /// </summary>
    public class IsyeriServiceTests
    {
        private readonly Mock<IIsyeriRepository> _repoMock = new();
        private readonly IsyeriService _sut;

        public IsyeriServiceTests()
        {
            _sut = new IsyeriService(_repoMock.Object);
        }

        /// <summary>Repo DataTable satırları için test verisi.</summary>
        private static DataTable OrnekDataTable(params (int firmaId, int isyeriId, string ad)[] satirlar)
        {
            var dt = new DataTable();
            dt.Columns.Add("FirmaId", typeof(int));
            dt.Columns.Add("IsyeriId", typeof(int));
            dt.Columns.Add("IsyeriAdi", typeof(string));
            foreach (var (firmaId, isyeriId, ad) in satirlar)
                dt.Rows.Add(firmaId, isyeriId, ad);
            return dt;
        }

        /// <summary>
        /// GetAll null dönerse boş liste; hata fırlatılmaz.
        /// </summary>
        [Fact]
        public void GetListForAdmin_NullDataTable_BosListeDoner()
        {
            _repoMock.Setup(r => r.GetAll()).Returns((DataTable)null);

            var sonuc = _sut.GetListForAdmin();

            sonuc.Should().BeEmpty();
        }

        /// <summary>
        /// Her DataTable satırı bir IsyeriItem (FirmaId, IsyeriId, Ad) olarak map edilir.
        /// </summary>
        [Fact]
        public void GetListForAdmin_IkiSatir_IkiIsyeriItemDoner()
        {
            var dt = OrnekDataTable(
                (1, 10, "Merkez"),
                (2, 20, "Şube"));
            _repoMock.Setup(r => r.GetAll()).Returns(dt);

            var sonuc = _sut.GetListForAdmin();

            sonuc.Should().HaveCount(2);
            sonuc[0].FirmaId.Should().Be(1);
            sonuc[0].IsyeriId.Should().Be(10);
            sonuc[0].Ad.Should().Be("Merkez");
            sonuc[1].Ad.Should().Be("Şube");
        }

        /// <summary>
        /// IsyeriAdi null ise Ad alanı boş string.
        /// </summary>
        [Fact]
        public void GetListForAdmin_AdSutunuNull_BosStringKullanilir()
        {
            var dt = new DataTable();
            dt.Columns.Add("FirmaId", typeof(int));
            dt.Columns.Add("IsyeriId", typeof(int));
            dt.Columns.Add("IsyeriAdi", typeof(string));
            dt.Rows.Add(1, 5, (object)null);
            _repoMock.Setup(r => r.GetAll()).Returns(dt);

            var sonuc = _sut.GetListForAdmin();

            sonuc.Should().HaveCount(1);
            sonuc[0].Ad.Should().Be("");
        }
    }
}
