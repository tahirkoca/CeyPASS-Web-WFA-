using CeyPASS.Business.Services;
using CeyPASS.DataAccess.Abstractions;
using FluentAssertions;
using Moq;
using System;
using System.Data;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    public class PozisyonServiceTests
    {
        private readonly Mock<IPozisyonRepository> _repoMock = new();
        private readonly PozisyonService _sut;

        public PozisyonServiceTests()
        {
            _sut = new PozisyonService(_repoMock.Object);
        }

        private static DataRow BuildRow(int id, object ad, object aciklama)
        {
            var dt = new DataTable();
            dt.Columns.Add("PozisyonId", typeof(int));
            dt.Columns.Add("PozisyonAdi", typeof(string));
            dt.Columns.Add("Aciklama", typeof(string));
            var row = dt.NewRow();
            row["PozisyonId"] = id;
            row["PozisyonAdi"] = ad;
            row["Aciklama"] = aciklama;
            dt.Rows.Add(row);
            return row;
        }

        // ─── GetForEdit ───────────────────────────────────────────────────────

        [Fact]
        public void GetForEdit_GetByIdNullDoner_NullDoner()
        {
            _repoMock.Setup(r => r.GetById(99)).Returns((DataRow)null!);

            var result = _sut.GetForEdit(99);

            result.Should().BeNull();
        }

        [Fact]
        public void GetForEdit_GecerliDataRow_DogruTupleDoner()
        {
            var row = BuildRow(1, "Müdür", "Açıklama");
            _repoMock.Setup(r => r.GetById(1)).Returns(row);

            var result = _sut.GetForEdit(1);

            result.Should().NotBeNull();
            result!.Value.id.Should().Be(1);
            result.Value.ad.Should().Be("Müdür");
            result.Value.ack.Should().Be("Açıklama");
        }

        [Fact]
        public void GetForEdit_AciklamaDBNull_BosStringDoner()
        {
            var row = BuildRow(2, "Uzman", DBNull.Value);
            _repoMock.Setup(r => r.GetById(2)).Returns(row);

            var result = _sut.GetForEdit(2);

            result.Should().NotBeNull();
            result!.Value.ack.Should().Be("");
        }
    }
}
