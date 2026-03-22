using CeyPASS.Business.Services;
using CeyPASS.DataAccess.Abstractions;
using FluentAssertions;
using Moq;
using System;
using System.Data;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    public class DepartmanServiceTests
    {
        private readonly Mock<IDepartmanRepository> _repoMock = new();
        private readonly DepartmanService _sut;

        public DepartmanServiceTests()
        {
            _sut = new DepartmanService(_repoMock.Object);
        }

        private static DataTable BosDataTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("DepartmanId", typeof(int));
            dt.Columns.Add("DepartmanAdi", typeof(string));
            dt.Columns.Add("Aciklama", typeof(string));
            return dt;
        }

        private static DataTable DolusDataTable()
        {
            var dt = BosDataTable();
            dt.Rows.Add(1, "Muhasebe", "Mali işler birimi");
            dt.Rows.Add(2, "Bilgi Teknolojileri", "IT birimi");
            return dt;
        }

        // ─── GetAll ───────────────────────────────────────────────────────────

        [Fact]
        public void GetAll_BosDataTable_BosListeDoner()
        {
            _repoMock.Setup(r => r.GetAll()).Returns(BosDataTable());

            var result = _sut.GetAll();

            result.Should().BeEmpty();
        }

        [Fact]
        public void GetAll_IkiSatir_IkiLookupItemDoner()
        {
            _repoMock.Setup(r => r.GetAll()).Returns(DolusDataTable());

            var result = _sut.GetAll();

            result.Should().HaveCount(2);
            result[0].Id.Should().Be(1);
            result[0].Ad.Should().Be("Muhasebe");
            result[1].Id.Should().Be(2);
            result[1].Ad.Should().Be("Bilgi Teknolojileri");
        }

        [Fact]
        public void GetAll_AdSutunuNull_BosStringKullanilir()
        {
            var dt = new DataTable();
            dt.Columns.Add("DepartmanId", typeof(int));
            dt.Columns.Add("DepartmanAdi", typeof(string));
            dt.Columns.Add("Aciklama", typeof(string));
            dt.Rows.Add(3, DBNull.Value, DBNull.Value);
            _repoMock.Setup(r => r.GetAll()).Returns(dt);

            var result = _sut.GetAll();

            result.Should().HaveCount(1);
            result[0].Ad.Should().Be("");
        }

        // ─── GetListForAdmin ──────────────────────────────────────────────────

        [Fact]
        public void GetListForAdmin_NullDataTable_BosListeDoner()
        {
            _repoMock.Setup(r => r.GetAll()).Returns((DataTable)null!);

            var result = _sut.GetListForAdmin();

            result.Should().BeEmpty();
        }

        [Fact]
        public void GetListForAdmin_VeriDolu_DogruDTOlarDoner()
        {
            _repoMock.Setup(r => r.GetAll()).Returns(DolusDataTable());

            var result = _sut.GetListForAdmin();

            result.Should().HaveCount(2);
            result[0].Id.Should().Be(1);
            result[0].Ad.Should().Be("Muhasebe");
            result[0].Aciklama.Should().Be("Mali işler birimi");
        }

        // ─── GetRowById ───────────────────────────────────────────────────────

        [Fact]
        public void GetRowById_IdMevcut_DogruSatirDoner()
        {
            _repoMock.Setup(r => r.GetAll()).Returns(DolusDataTable());

            var row = _sut.GetRowById(2);

            row.Should().NotBeNull();
            row!.Field<int>("DepartmanId").Should().Be(2);
        }

        [Fact]
        public void GetRowById_IdYok_NullDoner()
        {
            _repoMock.Setup(r => r.GetAll()).Returns(DolusDataTable());

            var row = _sut.GetRowById(999);

            row.Should().BeNull();
        }

        [Fact]
        public void GetRowById_BosDataTable_NullDoner()
        {
            _repoMock.Setup(r => r.GetAll()).Returns(BosDataTable());

            var row = _sut.GetRowById(1);

            row.Should().BeNull();
        }
    }
}
