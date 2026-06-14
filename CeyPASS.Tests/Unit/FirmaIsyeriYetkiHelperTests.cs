using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    public class FirmaIsyeriYetkiHelperTests
    {
        [Fact]
        public void FilterFirmalar_Admin_TumunuDoner()
        {
            var firmalar = new List<Firma> { new() { FirmaId = 1 }, new() { FirmaId = 2 } };
            FirmaIsyeriYetkiHelper.FilterFirmalar(firmalar, new List<FirmaIsyeriYetkiDTO> { new() { FirmaId = 1 } }, isAdmin: true)
                .Should().HaveCount(2);
        }

        [Fact]
        public void FilterFirmalar_YetkiVar_SadeceYetkili()
        {
            var firmalar = new List<Firma> { new() { FirmaId = 1 }, new() { FirmaId = 2 } };
            var yetkiler = new List<FirmaIsyeriYetkiDTO> { new() { FirmaId = 2 } };
            FirmaIsyeriYetkiHelper.FilterFirmalar(firmalar, yetkiler, isAdmin: false)
                .Should().ContainSingle(f => f.FirmaId == 2);
        }

        [Fact]
        public void FilterIsyeriler_IsyeriIdNull_TumIsyerler()
        {
            var isyerleri = new List<IsyeriItem>
            {
                new(1, 10, "A"),
                new(1, 20, "B")
            };
            var yetkiler = new List<FirmaIsyeriYetkiDTO> { new() { FirmaId = 1, IsyeriId = null } };
            FirmaIsyeriYetkiHelper.FilterIsyeriler(isyerleri, 1, yetkiler, isAdmin: false)
                .Should().HaveCount(2);
        }

        [Fact]
        public void FilterIsyeriler_SpesifikIsyeri_SadeceYetkili()
        {
            var isyerleri = new List<IsyeriItem>
            {
                new(1, 10, "A"),
                new(1, 20, "B")
            };
            var yetkiler = new List<FirmaIsyeriYetkiDTO> { new() { FirmaId = 1, IsyeriId = 20 } };
            FirmaIsyeriYetkiHelper.FilterIsyeriler(isyerleri, 1, yetkiler, isAdmin: false)
                .Should().ContainSingle(i => i.IsyeriId == 20);
        }

        [Fact]
        public void ResolveAktifFirmaId_YetkisizFirma_IlkYetkili()
        {
            var yetkiler = new List<FirmaIsyeriYetkiDTO>
            {
                new() { FirmaId = 5 },
                new() { FirmaId = 7 }
            };
            FirmaIsyeriYetkiHelper.ResolveAktifFirmaId(99, yetkiler, isAdmin: false)
                .Should().Be(5);
        }

        [Fact]
        public void ResolveKisiQuery_Tumu_Firma101_YalnizcaIsyeri10()
        {
            var yetkiler = new List<FirmaIsyeriYetkiDTO> { new() { FirmaId = 101, IsyeriId = 10 } };
            var (single, idIn) = FirmaIsyeriYetkiHelper.ResolveKisiQueryIsyeriFilter(101, null, yetkiler, isAdmin: false);
            single.Should().BeNull();
            idIn.Should().BeEquivalentTo(new[] { 10 });
        }

        [Fact]
        public void ResolveKisiQuery_Tumu_Firma287_UcIsyeri()
        {
            var yetkiler = new List<FirmaIsyeriYetkiDTO>
            {
                new() { FirmaId = 287, IsyeriId = 0 },
                new() { FirmaId = 287, IsyeriId = 1 },
                new() { FirmaId = 287, IsyeriId = 3 }
            };
            var (_, idIn) = FirmaIsyeriYetkiHelper.ResolveKisiQueryIsyeriFilter(287, 0, yetkiler, isAdmin: false);
            idIn.Should().BeEquivalentTo(new[] { 0, 1, 3 });
        }

        [Fact]
        public void ResolveKisiQuery_Tumu_FirmaGeneliYetki_FiltreYok()
        {
            var yetkiler = new List<FirmaIsyeriYetkiDTO> { new() { FirmaId = 1, IsyeriId = null } };
            var (single, idIn) = FirmaIsyeriYetkiHelper.ResolveKisiQueryIsyeriFilter(1, null, yetkiler, isAdmin: false);
            single.Should().BeNull();
            idIn.Should().BeNull();
        }

        [Fact]
        public void ResolveKisiQuery_Admin_Tumu_FiltreYok()
        {
            var (single, idIn) = FirmaIsyeriYetkiHelper.ResolveKisiQueryIsyeriFilter(101, null, new List<FirmaIsyeriYetkiDTO> { new() { FirmaId = 101, IsyeriId = 10 } }, isAdmin: true);
            single.Should().BeNull();
            idIn.Should().BeNull();
        }

        [Fact]
        public void ResolveKisiQuery_SeciliIsyeri10_TekFiltre()
        {
            var yetkiler = new List<FirmaIsyeriYetkiDTO> { new() { FirmaId = 101, IsyeriId = 10 } };
            var (single, idIn) = FirmaIsyeriYetkiHelper.ResolveKisiQueryIsyeriFilter(101, 10, yetkiler, isAdmin: false);
            single.Should().Be(10);
            idIn.Should().BeNull();
        }

        [Fact]
        public void ResolveKisiQuery_FirmayaYetkiYok_BosListe()
        {
            var yetkiler = new List<FirmaIsyeriYetkiDTO> { new() { FirmaId = 99, IsyeriId = 1 } };
            var (single, idIn) = FirmaIsyeriYetkiHelper.ResolveKisiQueryIsyeriFilter(101, null, yetkiler, isAdmin: false);
            single.Should().BeNull();
            idIn.Should().BeEmpty();
        }

        [Fact]
        public void ResolveRaporIsyeri_SecimYok_MaxCsvDoner()
        {
            var yetkiler = new List<FirmaIsyeriYetkiDTO> { new() { FirmaId = 1, IsyeriId = 10 } };
            var (csv, status) = FirmaIsyeriYetkiHelper.ResolveRaporIsyeriIdListCsv(1, null, "10,20,0", yetkiler, isAdmin: false);
            csv.Should().Be("10,20,0");
            status.Should().Be(FirmaIsyeriYetkiHelper.RaporIsyeriListStatus.Ok);
        }

        [Fact]
        public void ResolveRaporIsyeri_SeciliAltKume_CsvOlusur()
        {
            var yetkiler = new List<FirmaIsyeriYetkiDTO>
            {
                new() { FirmaId = 1, IsyeriId = 10 },
                new() { FirmaId = 1, IsyeriId = 20 }
            };
            var (csv, status) = FirmaIsyeriYetkiHelper.ResolveRaporIsyeriIdListCsv(
                1, new[] { 10 }, "10,20,0", yetkiler, isAdmin: false);
            csv.Should().Be("10,0");
            status.Should().Be(FirmaIsyeriYetkiHelper.RaporIsyeriListStatus.Ok);
        }

        [Fact]
        public void ResolveRaporIsyeri_YetkisizSecim_Red()
        {
            var yetkiler = new List<FirmaIsyeriYetkiDTO> { new() { FirmaId = 1, IsyeriId = 10 } };
            var (csv, status) = FirmaIsyeriYetkiHelper.ResolveRaporIsyeriIdListCsv(
                1, new[] { 99 }, "10,0", yetkiler, isAdmin: false);
            csv.Should().BeNull();
            status.Should().Be(FirmaIsyeriYetkiHelper.RaporIsyeriListStatus.UnauthorizedSelection);
        }

        [Fact]
        public void ParseIsyeriIds_Csv_Parselenir()
        {
            FirmaIsyeriYetkiHelper.ParseIsyeriIds("10, 20,20").Should().BeEquivalentTo(new[] { 10, 20 });
        }
    }
}
