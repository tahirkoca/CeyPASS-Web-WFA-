using CeyPASS.Business.Abstractions;
using CeyPASS.Business.Services;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    public class PersonelVardiyaYemekYetkiServiceTests
    {
        private readonly Mock<IPersonelVardiyaYemekYetkiRepository> _repo = new();
        private readonly PersonelVardiyaYemekYetkiService _sut;

        public PersonelVardiyaYemekYetkiServiceTests()
        {
            _sut = new PersonelVardiyaYemekYetkiService(_repo.Object);
        }

        private static PersonelVardiyaYemekYetki ValidItem(int id = 0) => new()
        {
            Id = id,
            CalismaSekliId = 34,
            IsyeriId = 1,
            CihazId = 7,
            YemekBaslangicSaati = new TimeSpan(11, 30, 0),
            YemekBitisSaati = new TimeSpan(12, 30, 0),
            AktifMi = true
        };

        [Fact]
        public void Add_BitisBaslangictanKucuk_Red()
        {
            var item = ValidItem();
            item.YemekBitisSaati = new TimeSpan(10, 0, 0);

            var (ok, error) = _sut.Add(item);

            ok.Should().BeFalse();
            error.Should().Contain("büyük");
            _repo.Verify(r => r.Insert(It.IsAny<PersonelVardiyaYemekYetki>()), Times.Never);
        }

        [Fact]
        public void Add_CihazSecilmedi_Red()
        {
            var item = ValidItem();
            item.CihazId = 0;

            var (ok, error) = _sut.Add(item);

            ok.Should().BeFalse();
            error.Should().Contain("Cihaz");
            _repo.Verify(r => r.Insert(It.IsAny<PersonelVardiyaYemekYetki>()), Times.Never);
        }

        [Fact]
        public void Add_AyniCihazVar_Red()
        {
            var item = ValidItem();
            _repo.Setup(r => r.ExistsForCihaz(34, 7, null)).Returns(true);

            var (ok, error) = _sut.Add(item);

            ok.Should().BeFalse();
            error.Should().Contain("zaten");
            _repo.Verify(r => r.Insert(It.IsAny<PersonelVardiyaYemekYetki>()), Times.Never);
        }

        [Fact]
        public void Add_Gecerli_InsertCagirilir()
        {
            var item = ValidItem();
            _repo.Setup(r => r.ExistsForCihaz(34, 7, null)).Returns(false);
            _repo.Setup(r => r.Insert(item)).Returns(10);

            var (ok, error) = _sut.Add(item);

            ok.Should().BeTrue();
            error.Should().BeNull();
            _repo.Verify(r => r.Insert(item), Times.Once);
        }

        [Fact]
        public void Update_DuplicateCihazExcludeSelf_Ok()
        {
            var item = ValidItem(5);
            _repo.Setup(r => r.ExistsForCihaz(34, 7, 5)).Returns(false);
            _repo.Setup(r => r.Update(item)).Returns(true);

            var (ok, error) = _sut.Update(item);

            ok.Should().BeTrue();
            error.Should().BeNull();
        }

        [Fact]
        public void FirmaHasSaatPenceresiAktif_RepoSonucunuDoner()
        {
            _repo.Setup(r => r.FirmaHasSaatPenceresiAktif(101)).Returns(true);
            _sut.FirmaHasSaatPenceresiAktif(101).Should().BeTrue();
        }

        [Fact]
        public void GetByCalismaSekliId_ListeyiDoner()
        {
            var list = new List<PersonelVardiyaYemekYetki> { ValidItem(1) };
            _repo.Setup(r => r.GetByCalismaSekliId(34)).Returns(list);

            _sut.GetByCalismaSekliId(34).Should().BeSameAs(list);
        }
    }
}
