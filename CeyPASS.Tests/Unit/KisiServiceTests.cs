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
        private readonly Mock<IKisiRepository> _kisiRepo = new();
        private readonly KisiService _sut;

        public KisiServiceTests()
        {
            _sut = new KisiService(_kisiRepo.Object, new Mock<IYemekhaneRepository>().Object);
        }

        private static KisiKayitValidasyonDTO FirmaPuantajliGecerli() => new()
        {
            PersonelId = "123",
            FirmaPersoneli = true,
            PuantajYapilir = true,
            YemekHakkiVar = false,
            TcKimlikNo = "12345678901",
            KartNo = "K001"
        };

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
        public void ValidateKisiKayit_FirmaPersoneli_TcBos_HataVerir()
        {
            var dto = FirmaPuantajliGecerli();
            dto.TcKimlikNo = "";

            var (isValid, message) = _sut.ValidateKisiKayit(dto);

            isValid.Should().BeFalse();
            message.Should().Contain("T.C.");
        }

        [Fact]
        public void ValidateKisiKayit_FirmaPersoneli_KartBos_Gecerli()
        {
            var dto = FirmaPuantajliGecerli();
            dto.KartNo = "";

            var (isValid, message) = _sut.ValidateKisiKayit(dto);

            isValid.Should().BeTrue();
            message.Should().BeNull();
        }

        [Fact]
        public void ValidateKisiKayit_Taseron_KartBos_Gecerli()
        {
            var dto = new KisiKayitValidasyonDTO
            {
                PersonelId = "T1",
                TaseronCalisanMi = true,
                TcKimlikNo = "12345678901",
                KartNo = ""
            };

            var (isValid, message) = _sut.ValidateKisiKayit(dto);

            isValid.Should().BeTrue();
            message.Should().BeNull();
        }

        [Fact]
        public void ValidateKisiKayit_Ziyaretci_KartBos_HataVerir()
        {
            var dto = new KisiKayitValidasyonDTO
            {
                PersonelId = "Z1",
                ZiyaretciMi = true,
                KartNo = ""
            };

            var (isValid, message) = _sut.ValidateKisiKayit(dto);

            isValid.Should().BeFalse();
            message.Should().Contain("Kart No");
        }

        [Fact]
        public void ValidateKisiKayit_AracKarti_KartBos_HataVerir()
        {
            var dto = new KisiKayitValidasyonDTO
            {
                PersonelId = "A1",
                AracKartiMi = true,
                KartNo = ""
            };

            var (isValid, message) = _sut.ValidateKisiKayit(dto);

            isValid.Should().BeFalse();
            message.Should().Contain("Kart No");
        }

        [Fact]
        public void ValidateKisiKayit_TcOnHane_HataVerir()
        {
            var dto = FirmaPuantajliGecerli();
            dto.TcKimlikNo = "1234567890";

            var (isValid, message) = _sut.ValidateKisiKayit(dto);

            isValid.Should().BeFalse();
            message.Should().Contain("11 haneli");
        }

        [Fact]
        public void ValidateKisiKayit_TcOnIkiHane_HataVerir()
        {
            var dto = FirmaPuantajliGecerli();
            dto.TcKimlikNo = "123456789012";

            var (isValid, message) = _sut.ValidateKisiKayit(dto);

            isValid.Should().BeFalse();
            message.Should().Contain("11 haneli");
        }

        [Fact]
        public void ValidateKisiKayit_TcHarfVar_HataVerir()
        {
            var dto = FirmaPuantajliGecerli();
            dto.TcKimlikNo = "1234567890A";

            var (isValid, message) = _sut.ValidateKisiKayit(dto);

            isValid.Should().BeFalse();
            message.Should().Contain("11 haneli");
        }

        [Fact]
        public void ValidateKisiKayit_SicilCakisma_HataVerir()
        {
            var dto = FirmaPuantajliGecerli();
            _kisiRepo.Setup(r => r.FindByPersonelId("123"))
                .Returns(new KisiAdSoyad { PersonelId = "123", Ad = "Ali", Soyad = "Veli" });

            var (isValid, message) = _sut.ValidateKisiKayit(dto);

            isValid.Should().BeFalse();
            message.Should().Contain("Sicil No");
            message.Should().Contain("Ali Veli");
        }

        [Fact]
        public void ValidateKisiKayit_TcCakisma_HataVerir()
        {
            var dto = FirmaPuantajliGecerli();
            _kisiRepo.Setup(r => r.FindByTcKimlikNo("12345678901"))
                .Returns(new KisiAdSoyad { PersonelId = "999", Ad = "Ayşe", Soyad = "Demir" });

            var (isValid, message) = _sut.ValidateKisiKayit(dto);

            isValid.Should().BeFalse();
            message.Should().Contain("T.C. Kimlik No");
            message.Should().Contain("999");
        }

        [Fact]
        public void ValidateKisiKayit_KartNoCakisma_HataVerir()
        {
            var dto = new KisiKayitValidasyonDTO
            {
                PersonelId = "Z2",
                ZiyaretciMi = true,
                KartNo = "K001"
            };
            _kisiRepo.Setup(r => r.FindByKartNo("K001"))
                .Returns(new KisiAdSoyad { PersonelId = "88", Ad = "Can", Soyad = "Yılmaz" });

            var (isValid, message) = _sut.ValidateKisiKayit(dto);

            isValid.Should().BeFalse();
            message.Should().Contain("Kart No");
            message.Should().Contain("88");
        }

        [Fact]
        public void ValidateKisiKayit_FirmaPersoneliPuantajli_Gecerli()
        {
            var (isValid, message) = _sut.ValidateKisiKayit(FirmaPuantajliGecerli());
            isValid.Should().BeTrue();
            message.Should().BeNull();
        }

        [Fact]
        public void ValidateKisiKayit_FirmaDisiPersoneliKartli_Gecerli()
        {
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
