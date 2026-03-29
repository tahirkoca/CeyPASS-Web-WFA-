using CeyPASS.Business.Abstractions;
using CeyPASS.Business.Services;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using FluentAssertions;
using Moq;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    public class SifreServiceTests
    {
        private readonly Mock<IKullaniciRepository> _repoMock = new();
        private readonly Mock<IEmailService> _emailMock = new();
        private readonly Mock<IKisiRepository> _kisiRepoMock = new();
        private readonly Mock<IPersonelWebSifreRepository> _personelWebSifreRepoMock = new();
        private readonly Mock<IBildirimService> _bildirimServiceMock = new();
        private readonly Mock<IUstYetkiliRepository> _ustYetkiliRepoMock = new();
        private readonly SifreService _sut;

        public SifreServiceTests()
        {
            _sut = new SifreService(
                _repoMock.Object, 
                _emailMock.Object, 
                _kisiRepoMock.Object, 
                _personelWebSifreRepoMock.Object,
                _bildirimServiceMock.Object,
                _ustYetkiliRepoMock.Object);
        }

        // ─── SifreSifirlamaTamamla ────────────────────────────────────────────

        [Fact]
        public void SifreSifirlamaTamamla_GirilenKodBos_HataVerir()
        {
            var sonuc = _sut.SifreSifirlamaTamamla("kullanici", "", "Sifre123", "Sifre123");
            sonuc.Basarili.Should().BeFalse();
            sonuc.HataMesaji.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void SifreSifirlamaTamamla_YeniSifreBos_HataVerir()
        {
            var sonuc = _sut.SifreSifirlamaTamamla("kullanici", "123456", "", "");
            sonuc.Basarili.Should().BeFalse();
            sonuc.HataMesaji.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void SifreSifirlamaTamamla_SifreAltialtiKarakter_HataVerir()
        {
            var sonuc = _sut.SifreSifirlamaTamamla("kullanici", "123456", "abc", "abc");
            sonuc.Basarili.Should().BeFalse();
            sonuc.HataMesaji.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void SifreSifirlamaTamamla_SifrelerEslesmez_HataVerir()
        {
            var sonuc = _sut.SifreSifirlamaTamamla("kullanici", "123456", "Sifre123", "Sifre999");
            sonuc.Basarili.Should().BeFalse();
            sonuc.HataMesaji.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void SifreSifirlamaTamamla_KullaniciBulunamaz_HataVerir()
        {
            _repoMock.Setup(r => r.GetByUserName("yok")).Returns((Kullanici)null!);

            var sonuc = _sut.SifreSifirlamaTamamla("yok", "123456", "Sifre123", "Sifre123");
            sonuc.Basarili.Should().BeFalse();
            sonuc.HataMesaji.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void SifreSifirlamaTamamla_KodEslesmez_HataVerir()
        {
            var kullanici = new Kullanici { KullaniciId = 1, KullaniciAdi = "user1" };
            _repoMock.Setup(r => r.GetByUserName("user1")).Returns(kullanici);
            _repoMock.Setup(r => r.GetKurtarmaKodu(1)).Returns("999999");

            var sonuc = _sut.SifreSifirlamaTamamla("user1", "111111", "Sifre123", "Sifre123");
            sonuc.Basarili.Should().BeFalse();
            sonuc.HataMesaji.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void SifreSifirlamaTamamla_TumKosullarSaglanir_SifreSifirlanir()
        {
            var kullanici = new Kullanici { KullaniciId = 1, KullaniciAdi = "user1" };
            _repoMock.Setup(r => r.GetByUserName("user1")).Returns(kullanici);
            _repoMock.Setup(r => r.GetKurtarmaKodu(1)).Returns("123456");
            _repoMock.Setup(r => r.SifreGuncelle("user1", "Sifre123")).Returns(true);

            var sonuc = _sut.SifreSifirlamaTamamla("user1", "123456", "Sifre123", "Sifre123");

            sonuc.Basarili.Should().BeTrue();
            _repoMock.Verify(r => r.SifreGuncelle("user1", "Sifre123"), Times.Once);
            _repoMock.Verify(r => r.KurtarmaKodunuTemizle(1), Times.Once);
        }

        [Fact]
        public void SifreSifirlamaTamamla_SifreGuncelleFalse_BasarisizDoner()
        {
            var kullanici = new Kullanici { KullaniciId = 1, KullaniciAdi = "user1" };
            _repoMock.Setup(r => r.GetByUserName("user1")).Returns(kullanici);
            _repoMock.Setup(r => r.GetKurtarmaKodu(1)).Returns("123456");
            _repoMock.Setup(r => r.SifreGuncelle("user1", "Sifre123")).Returns(false);

            var sonuc = _sut.SifreSifirlamaTamamla("user1", "123456", "Sifre123", "Sifre123");

            sonuc.Basarili.Should().BeFalse();
            sonuc.HataMesaji.Should().NotBeNullOrWhiteSpace();
            _repoMock.Verify(r => r.KurtarmaKodunuTemizle(It.IsAny<int>()), Times.Never);
        }

        // ─── SifreSifirlamaBaslat ─────────────────────────────────────────────

        [Fact]
        public void SifreSifirlamaBaslat_KullaniciBulunamaz_HataVerir()
        {
            _repoMock.Setup(r => r.GetByUserName("yok")).Returns((Kullanici)null!);

            var sonuc = _sut.SifreSifirlamaBaslat("yok");
            sonuc.Basarili.Should().BeFalse();
            sonuc.HataMesaji.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void SifreSifirlamaBaslat_EmailYok_HataVerir()
        {
            var kullanici = new Kullanici { KullaniciId = 1, KullaniciAdi = "user1", Email = "" };
            _repoMock.Setup(r => r.GetByUserName("user1")).Returns(kullanici);

            var sonuc = _sut.SifreSifirlamaBaslat("user1");
            sonuc.Basarili.Should().BeFalse();
            sonuc.HataMesaji.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void SifreSifirlamaBaslat_GecerliKullanici_KodGonderilir_BasariliDoner()
        {
            var kullanici = new Kullanici
            {
                KullaniciId = 1,
                KullaniciAdi = "user1",
                Email = "user1@test.com"
            };
            _repoMock.Setup(r => r.GetByUserName("user1")).Returns(kullanici);

            var sonuc = _sut.SifreSifirlamaBaslat("user1");

            sonuc.Basarili.Should().BeTrue();
            sonuc.Email.Should().Be("user1@test.com");
            _emailMock.Verify(e => e.SendVerificationCode("user1@test.com", It.IsAny<string>()), Times.Once);
        }
    }
}
