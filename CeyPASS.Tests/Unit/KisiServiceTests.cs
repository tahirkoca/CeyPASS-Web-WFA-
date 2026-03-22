using CeyPASS.Business.Services;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using FluentAssertions;
using Moq;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    public class KisiServiceTests
    {
        private readonly KisiService _sut;

        public KisiServiceTests()
        {
            _sut = new KisiService(
                new Mock<IKisiRepository>().Object,
                new Mock<IYemekhaneRepository>().Object);
        }

        [Fact]
        public void ValidateKisiKayit_PersonelIdBos_HataVerir()
        {
            var dto = new KisiKayitValidasyonDTO { PersonelId = "" };
            var (isValid, message) = _sut.ValidateKisiKayit(dto);
            isValid.Should().BeFalse();
            message.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void ValidateKisiKayit_FirmaDisiPersonel_PuantajYapilabilir_HataVerir()
        {
            var dto = new KisiKayitValidasyonDTO
            {
                PersonelId = "123",
                FirmaPersoneli = false,
                PuantajYapilir = true,
                YemekHakkiVar = false
            };
            var (isValid, message) = _sut.ValidateKisiKayit(dto);
            isValid.Should().BeFalse();
            message.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void ValidateKisiKayit_FirmaPersoneli_PuantajsizVeKartBos_HataVerir()
        {
            // Firma personeli + puantajsız → kart gerekli
            var dto = new KisiKayitValidasyonDTO
            {
                PersonelId = "123",
                FirmaPersoneli = true,
                PuantajYapilir = false,
                YemekHakkiVar = false,
                FirmaDisiKartNo = ""
            };
            var (isValid, message) = _sut.ValidateKisiKayit(dto);
            isValid.Should().BeFalse();
            message.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void ValidateKisiKayit_FirmaDisiPersonel_PuantajsizYemekVarKartBos_HataVerir()
        {
            // Firma dışı + puantajsız + yemek var → kart gerekli
            var dto = new KisiKayitValidasyonDTO
            {
                PersonelId = "123",
                FirmaPersoneli = false,
                PuantajYapilir = false,
                YemekHakkiVar = true,
                YemekAdedi = 1,
                FirmaDisiKartNo = ""
            };
            var (isValid, message) = _sut.ValidateKisiKayit(dto);
            isValid.Should().BeFalse();
            message.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void ValidateKisiKayit_YemekHakkiVarAmaAdediSifir_HataVerir()
        {
            var dto = new KisiKayitValidasyonDTO
            {
                PersonelId = "123",
                FirmaPersoneli = true,
                PuantajYapilir = true,
                YemekHakkiVar = true,
                YemekAdedi = 0
            };
            var (isValid, message) = _sut.ValidateKisiKayit(dto);
            isValid.Should().BeFalse();
            message.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void ValidateKisiKayit_FirmaPersoneliPuantajli_Gecerli()
        {
            var dto = new KisiKayitValidasyonDTO
            {
                PersonelId = "123",
                FirmaPersoneli = true,
                PuantajYapilir = true,
                YemekHakkiVar = false
            };
            var (isValid, message) = _sut.ValidateKisiKayit(dto);
            isValid.Should().BeTrue();
            message.Should().BeNull();
        }

        [Fact]
        public void ValidateKisiKayit_FirmaDisiPersoneliKartli_Gecerli()
        {
            // Firma dışı + puantajsız + yemeksiz + kart var → geçerli
            var dto = new KisiKayitValidasyonDTO
            {
                PersonelId = "456",
                FirmaPersoneli = false,
                PuantajYapilir = false,
                YemekHakkiVar = false,
                FirmaDisiKartNo = "KART001"
            };
            var (isValid, message) = _sut.ValidateKisiKayit(dto);
            isValid.Should().BeTrue();
            message.Should().BeNull();
        }
    }
}
