using CeyPASS.Business.Services;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using FluentAssertions;
using Moq;
using System;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    public class KisiIzinServiceTests
    {
        private readonly KisiIzinService _sut;

        public KisiIzinServiceTests()
        {
            _sut = new KisiIzinService(new Mock<IKisiIzinlerRepository>().Object);
        }

        [Fact]
        public void ValidateKayit_PersonelIdBos_HataVerir()
        {
            var dto = new IzinKayitValidasyonDTO { PersonelId = "" };
            var (isValid, message) = _sut.ValidateKayit(dto);
            isValid.Should().BeFalse();
            message.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void ValidateKayit_GunlukIzin_IzinTipSeçilmemis_HataVerir()
        {
            var dto = new IzinKayitValidasyonDTO
            {
                PersonelId = "123",
                SaatlikIzinMi = false,
                IzinTipId = null,
                BaslangicTarihi = new DateTime(2025, 1, 10),
                BitisTarihi = new DateTime(2025, 1, 12)
            };
            var (isValid, message) = _sut.ValidateKayit(dto);
            isValid.Should().BeFalse();
            message.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void ValidateKayit_SaatlikIzin_FarkliGunler_HataVerir()
        {
            var dto = new IzinKayitValidasyonDTO
            {
                PersonelId = "123",
                SaatlikIzinMi = true,
                BaslangicTarihi = new DateTime(2025, 1, 1),
                BitisTarihi = new DateTime(2025, 1, 2),
                BaslangicSaati = new TimeSpan(8, 0, 0),
                BitisSaati = new TimeSpan(10, 0, 0)
            };
            var (isValid, message) = _sut.ValidateKayit(dto);
            isValid.Should().BeFalse();
            message.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void ValidateKayit_SaatlikIzin_BititsBastanOnce_HataVerir()
        {
            var gun = new DateTime(2025, 1, 1);
            var dto = new IzinKayitValidasyonDTO
            {
                PersonelId = "123",
                SaatlikIzinMi = true,
                BaslangicTarihi = gun,
                BitisTarihi = gun,
                BaslangicSaati = new TimeSpan(10, 0, 0),
                BitisSaati = new TimeSpan(9, 0, 0)
            };
            var (isValid, message) = _sut.ValidateKayit(dto);
            isValid.Should().BeFalse();
            message.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void ValidateKayit_SaatlikIzin_BasBitisAyni_HataVerir()
        {
            var gun = new DateTime(2025, 1, 1);
            var dto = new IzinKayitValidasyonDTO
            {
                PersonelId = "123",
                SaatlikIzinMi = true,
                BaslangicTarihi = gun,
                BitisTarihi = gun,
                BaslangicSaati = new TimeSpan(9, 0, 0),
                BitisSaati = new TimeSpan(9, 0, 0)
            };
            var (isValid, message) = _sut.ValidateKayit(dto);
            isValid.Should().BeFalse();
            message.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void ValidateKayit_GunlukIzin_BitisBaslangictanOnce_HataVerir()
        {
            var dto = new IzinKayitValidasyonDTO
            {
                PersonelId = "123",
                SaatlikIzinMi = false,
                IzinTipId = 1,
                BaslangicTarihi = new DateTime(2025, 1, 10),
                BitisTarihi = new DateTime(2025, 1, 9)
            };
            var (isValid, message) = _sut.ValidateKayit(dto);
            isValid.Should().BeFalse();
            message.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void ValidateKayit_GecerliSaatlikIzin_Gecerli()
        {
            var gun = new DateTime(2025, 1, 1);
            var dto = new IzinKayitValidasyonDTO
            {
                PersonelId = "123",
                SaatlikIzinMi = true,
                BaslangicTarihi = gun,
                BitisTarihi = gun,
                BaslangicSaati = new TimeSpan(9, 0, 0),
                BitisSaati = new TimeSpan(11, 0, 0)
            };
            var (isValid, message) = _sut.ValidateKayit(dto);
            isValid.Should().BeTrue();
            message.Should().BeNull();
        }

        [Fact]
        public void ValidateKayit_GecerliGunlukIzin_Gecerli()
        {
            var dto = new IzinKayitValidasyonDTO
            {
                PersonelId = "123",
                SaatlikIzinMi = false,
                IzinTipId = 2,
                BaslangicTarihi = new DateTime(2025, 1, 10),
                BitisTarihi = new DateTime(2025, 1, 15)
            };
            var (isValid, message) = _sut.ValidateKayit(dto);
            isValid.Should().BeTrue();
            message.Should().BeNull();
        }

        [Fact]
        public void ValidateKayit_YarimGunYillik_Gecerli()
        {
            var gun = new DateTime(2025, 6, 10);
            var dto = new IzinKayitValidasyonDTO
            {
                PersonelId = "1426",
                YarimGunYillikIzinMi = true,
                SaatlikIzinMi = true,
                IzinTipId = 2,
                BaslangicTarihi = gun,
                BitisTarihi = gun,
                BaslangicSaati = new TimeSpan(8, 30, 0),
                BitisSaati = new TimeSpan(12, 15, 0)
            };
            var (isValid, message) = _sut.ValidateKayit(dto);
            isValid.Should().BeTrue();
            message.Should().BeNull();
        }

        [Fact]
        public void ValidateKayit_YarimGunYillik_YanlisIzinTipi_Hata()
        {
            var gun = new DateTime(2025, 6, 10);
            var dto = new IzinKayitValidasyonDTO
            {
                PersonelId = "1426",
                YarimGunYillikIzinMi = true,
                SaatlikIzinMi = true,
                IzinTipId = 7,
                BaslangicTarihi = gun,
                BitisTarihi = gun,
                BaslangicSaati = new TimeSpan(8, 30, 0),
                BitisSaati = new TimeSpan(12, 15, 0)
            };
            var (isValid, message) = _sut.ValidateKayit(dto);
            isValid.Should().BeFalse();
            message.Should().Contain("yıllık");
        }
    }
}
