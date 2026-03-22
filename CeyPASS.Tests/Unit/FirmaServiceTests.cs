using CeyPASS.Business.Services;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    public class FirmaServiceTests
    {
        private readonly Mock<IFirmaRepository> _repoMock = new();
        private readonly FirmaService _sut;

        public FirmaServiceTests()
        {
            _sut = new FirmaService(_repoMock.Object);
        }

        // ─── SuggestNextId ────────────────────────────────────────────────────

        [Fact]
        public void SuggestNextId_MaxIdNull_101Doner()
        {
            _repoMock.Setup(r => r.GetMaxId()).Returns((int?)null);
            _sut.SuggestNextId().Should().Be(101);
        }

        [Fact]
        public void SuggestNextId_MaxId200_201Doner()
        {
            _repoMock.Setup(r => r.GetMaxId()).Returns(200);
            _sut.SuggestNextId().Should().Be(201);
        }

        // ─── Add — Validasyon ─────────────────────────────────────────────────

        [Fact]
        public void Add_IdSifir_HataVerir()
        {
            var result = _sut.Add(0, "Test Firma", "", out var msg);
            result.Should().BeFalse();
            msg.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Add_AdBos_HataVerir()
        {
            var result = _sut.Add(5, "  ", "", out var msg);
            result.Should().BeFalse();
            msg.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Add_GecersizEmail_HataVerir()
        {
            var result = _sut.Add(5, "Test Firma", "gecersiz-mail", out var msg);
            result.Should().BeFalse();
            msg.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Add_MukerrerFirmaId_HataVerir()
        {
            _repoMock.Setup(r => r.GetAll()).Returns(new List<Firma>
            {
                new Firma { FirmaId = 5, FirmaAdi = "Mevcut Firma" }
            });

            var result = _sut.Add(5, "Yeni Firma", "", out var msg);
            result.Should().BeFalse();
            msg.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Add_GecerliVeri_KayitGerceklesir()
        {
            _repoMock.Setup(r => r.GetAll()).Returns(new List<Firma>());
            _repoMock.Setup(r => r.Insert(It.IsAny<Firma>())).Returns(true);

            var result = _sut.Add(10, "Yeni Firma", "", out var msg);

            result.Should().BeTrue();
            _repoMock.Verify(r => r.Insert(It.Is<Firma>(f => f.FirmaId == 10 && f.FirmaAdi == "Yeni Firma")), Times.Once);
        }

        // ─── Update — Validasyon ──────────────────────────────────────────────

        [Fact]
        public void Update_AdBos_HataVerir()
        {
            var result = _sut.Update(5, "", "", out var msg);
            result.Should().BeFalse();
            msg.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Update_GecersizEmail_HataVerir()
        {
            var result = _sut.Update(5, "Test Firma", "@bozukmail", out var msg);
            result.Should().BeFalse();
            msg.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Update_GecerliVeri_KayitGuncellenir()
        {
            _repoMock.Setup(r => r.Update(It.IsAny<Firma>())).Returns(true);

            var result = _sut.Update(5, "Guncellenmis Firma", "it@test.com", out var msg);

            result.Should().BeTrue();
            _repoMock.Verify(r => r.Update(It.Is<Firma>(f => f.FirmaId == 5 && f.FirmaAdi == "Guncellenmis Firma")), Times.Once);
        }

        // ─── IsValidEmail (Add/Update üzerinden) ─────────────────────────────

        [Fact]
        public void IsValidEmail_GecerliEmail_Kabul()
        {
            _repoMock.Setup(r => r.GetAll()).Returns(new List<Firma>());
            _repoMock.Setup(r => r.Insert(It.IsAny<Firma>())).Returns(true);

            var result = _sut.Add(20, "Firma", "it@cey.com", out _);
            result.Should().BeTrue();
        }

        [Fact]
        public void IsValidEmail_GecersizEmail_Red()
        {
            var result = _sut.Add(20, "Firma", "bozuk@@mail", out var msg);
            result.Should().BeFalse();
            msg.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void IsValidEmail_BosEmail_AtlanilirKayitGerceklesir()
        {
            _repoMock.Setup(r => r.GetAll()).Returns(new List<Firma>());
            _repoMock.Setup(r => r.Insert(It.IsAny<Firma>())).Returns(true);

            // Boş email → validasyon atlanır
            var result = _sut.Add(20, "Firma", "", out _);
            result.Should().BeTrue();
        }

        [Fact]
        public void IsValidEmail_BirdenFazlaAt_Gecersiz()
        {
            var result = _sut.Add(20, "Firma", "a@@b.com", out var msg);
            result.Should().BeFalse();
            msg.Should().NotBeNullOrWhiteSpace();
        }
    }
}
