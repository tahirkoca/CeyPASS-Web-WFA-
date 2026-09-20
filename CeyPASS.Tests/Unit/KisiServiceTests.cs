using CeyPASS.Business.Services;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using FluentAssertions;
using Moq;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    /// <summary>
    /// Kişi/personel kaydı: validasyon, güncelleme ve silme kuralları.
    /// </summary>
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
            KartNo = "K001",
            IsyeriId = 1
        };

        /// <summary>
        /// PersonelIdboş Hataverir
        /// </summary>
        [Fact]
        public void ValidateKisiKayit_PersonelIdBos_HataVerir()
        {
            var dto = new KisiKayitValidasyonDTO { PersonelId = "" };
            var (isValid, message) = _sut.ValidateKisiKayit(dto);
            isValid.Should().BeFalse();
            message.Should().NotBeNullOrWhiteSpace();
        }

        /// <summary>
        /// Isyeriboş Hataverir
        /// </summary>
        [Fact]
        public void ValidateKisiKayit_IsyeriBos_HataVerir()
        {
            var dto = FirmaPuantajliGecerli();
            dto.IsyeriId = null;

            var (isValid, message) = _sut.ValidateKisiKayit(dto);

            isValid.Should().BeFalse();
            message.Should().Contain("İşyeri");
        }

        /// <summary>
        /// FirmaDisiPersonel PuantajYapilabilir Hataverir
        /// </summary>
        [Fact]
        public void ValidateKisiKayit_FirmaDisiPersonel_PuantajYapilabilir_HataVerir()
        {
            var dto = new KisiKayitValidasyonDTO
            {
                PersonelId = "123",
                FirmaPersoneli = false,
                PuantajYapilir = true,
                YemekHakkiVar = false,
                IsyeriId = 1
            };
            var (isValid, message) = _sut.ValidateKisiKayit(dto);
            isValid.Should().BeFalse();
            message.Should().NotBeNullOrWhiteSpace();
        }

        /// <summary>
        /// FirmaPersoneli PuantajsizVeDisSicilboş Geçerli
        /// </summary>
        [Fact]
        public void ValidateKisiKayit_FirmaPersoneli_PuantajsizVeDisSicilBos_Gecerli()
        {
            var dto = new KisiKayitValidasyonDTO
            {
                PersonelId = "123",
                FirmaPersoneli = true,
                PuantajYapilir = false,
                YemekHakkiVar = false,
                TcKimlikNo = "12345678901",
                FirmaDisiKartNo = "",
                IsyeriId = 1
            };
            var (isValid, message) = _sut.ValidateKisiKayit(dto);
            isValid.Should().BeTrue();
            message.Should().BeNull();
        }

        /// <summary>
        /// FirmaDisiPersonel PuantajsizYemekVarDisSicilboş Geçerli
        /// </summary>
        [Fact]
        public void ValidateKisiKayit_FirmaDisiPersonel_PuantajsizYemekVarDisSicilBos_Gecerli()
        {
            var dto = new KisiKayitValidasyonDTO
            {
                PersonelId = "123",
                FirmaPersoneli = false,
                PuantajYapilir = false,
                YemekHakkiVar = true,
                YemekAdedi = 1,
                FirmaDisiKartNo = "",
                IsyeriId = 1
            };
            var (isValid, message) = _sut.ValidateKisiKayit(dto);
            isValid.Should().BeTrue();
            message.Should().BeNull();
        }

        /// <summary>
        /// YemekHakkiVarAmaAdedisıfır Hataverir
        /// </summary>
        [Fact]
        public void ValidateKisiKayit_YemekHakkiVarAmaAdediSifir_HataVerir()
        {
            var dto = new KisiKayitValidasyonDTO
            {
                PersonelId = "123",
                FirmaPersoneli = true,
                PuantajYapilir = true,
                YemekHakkiVar = true,
                YemekAdedi = 0,
                IsyeriId = 1
            };
            var (isValid, message) = _sut.ValidateKisiKayit(dto);
            isValid.Should().BeFalse();
            message.Should().NotBeNullOrWhiteSpace();
        }

        /// <summary>
        /// FirmaPersoneli Tcboş Hataverir
        /// </summary>
        [Fact]
        public void ValidateKisiKayit_FirmaPersoneli_TcBos_HataVerir()
        {
            var dto = FirmaPuantajliGecerli();
            dto.TcKimlikNo = "";

            var (isValid, message) = _sut.ValidateKisiKayit(dto);

            isValid.Should().BeFalse();
            message.Should().Contain("T.C.");
        }

        /// <summary>
        /// FirmaPersoneli Kartboş Geçerli
        /// </summary>
        [Fact]
        public void ValidateKisiKayit_FirmaPersoneli_KartBos_Gecerli()
        {
            var dto = FirmaPuantajliGecerli();
            dto.KartNo = "";

            var (isValid, message) = _sut.ValidateKisiKayit(dto);

            isValid.Should().BeTrue();
            message.Should().BeNull();
        }

        /// <summary>
        /// Taseron Kartboş Geçerli
        /// </summary>
        [Fact]
        public void ValidateKisiKayit_Taseron_KartBos_Gecerli()
        {
            var dto = new KisiKayitValidasyonDTO
            {
                PersonelId = "T1",
                TaseronCalisanMi = true,
                TcKimlikNo = "12345678901",
                KartNo = "",
                IsyeriId = 1
            };

            var (isValid, message) = _sut.ValidateKisiKayit(dto);

            isValid.Should().BeTrue();
            message.Should().BeNull();
        }

        /// <summary>
        /// Ziyaretci Kartboş Hataverir
        /// </summary>
        [Fact]
        public void ValidateKisiKayit_Ziyaretci_KartBos_HataVerir()
        {
            var dto = new KisiKayitValidasyonDTO
            {
                PersonelId = "Z1",
                ZiyaretciMi = true,
                KartNo = "",
                IsyeriId = 1
            };

            var (isValid, message) = _sut.ValidateKisiKayit(dto);

            isValid.Should().BeFalse();
            message.Should().Contain("Kart No");
        }

        /// <summary>
        /// AracKarti Kartboş Hataverir
        /// </summary>
        [Fact]
        public void ValidateKisiKayit_AracKarti_KartBos_HataVerir()
        {
            var dto = new KisiKayitValidasyonDTO
            {
                PersonelId = "A1",
                AracKartiMi = true,
                KartNo = "",
                IsyeriId = 1
            };

            var (isValid, message) = _sut.ValidateKisiKayit(dto);

            isValid.Should().BeFalse();
            message.Should().Contain("Kart No");
        }

        /// <summary>
        /// TcOnHane Hataverir
        /// </summary>
        [Fact]
        public void ValidateKisiKayit_TcOnHane_HataVerir()
        {
            var dto = FirmaPuantajliGecerli();
            dto.TcKimlikNo = "1234567890";

            var (isValid, message) = _sut.ValidateKisiKayit(dto);

            isValid.Should().BeFalse();
            message.Should().Contain("11 haneli");
        }

        /// <summary>
        /// TcOnIkiHane Hataverir
        /// </summary>
        [Fact]
        public void ValidateKisiKayit_TcOnIkiHane_HataVerir()
        {
            var dto = FirmaPuantajliGecerli();
            dto.TcKimlikNo = "123456789012";

            var (isValid, message) = _sut.ValidateKisiKayit(dto);

            isValid.Should().BeFalse();
            message.Should().Contain("11 haneli");
        }

        /// <summary>
        /// TcHarfVar Hataverir
        /// </summary>
        [Fact]
        public void ValidateKisiKayit_TcHarfVar_HataVerir()
        {
            var dto = FirmaPuantajliGecerli();
            dto.TcKimlikNo = "1234567890A";

            var (isValid, message) = _sut.ValidateKisiKayit(dto);

            isValid.Should().BeFalse();
            message.Should().Contain("11 haneli");
        }

        /// <summary>
        /// SicilCakisma Hataverir
        /// </summary>
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

        /// <summary>
        /// TcCakisma Hataverir
        /// </summary>
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

        /// <summary>
        /// KartNoCakisma Hataverir
        /// </summary>
        [Fact]
        public void ValidateKisiKayit_KartNoCakisma_HataVerir()
        {
            var dto = new KisiKayitValidasyonDTO
            {
                PersonelId = "Z2",
                ZiyaretciMi = true,
                KartNo = "K001",
                IsyeriId = 1
            };
            _kisiRepo.Setup(r => r.FindByKartNo("K001"))
                .Returns(new KisiAdSoyad { PersonelId = "88", Ad = "Can", Soyad = "Yılmaz" });

            var (isValid, message) = _sut.ValidateKisiKayit(dto);

            isValid.Should().BeFalse();
            message.Should().Contain("Kart No");
            message.Should().Contain("88");
        }

        /// <summary>
        /// FirmaPersoneliPuantajli Geçerli
        /// </summary>
        [Fact]
        public void ValidateKisiKayit_FirmaPersoneliPuantajli_Gecerli()
        {
            var (isValid, message) = _sut.ValidateKisiKayit(FirmaPuantajliGecerli());
            isValid.Should().BeTrue();
            message.Should().BeNull();
        }

        /// <summary>
        /// FirmaDisiPersoneliKartli Geçerli
        /// </summary>
        [Fact]
        public void ValidateKisiKayit_FirmaDisiPersoneliKartli_Gecerli()
        {
            var dto = new KisiKayitValidasyonDTO
            {
                PersonelId = "456",
                FirmaPersoneli = false,
                PuantajYapilir = false,
                YemekHakkiVar = false,
                FirmaDisiKartNo = "KART001",
                IsyeriId = 1
            };
            var (isValid, message) = _sut.ValidateKisiKayit(dto);
            isValid.Should().BeTrue();
            message.Should().BeNull();
        }
    }
}
