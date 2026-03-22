using CeyPASS.Business.Abstractions;
using CeyPASS.Business.Services;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    public class NotificationServiceTests
    {
        private readonly Mock<IMailService> _mailServiceMock = new();
        private readonly Mock<IMailRepository> _mailRepoMock = new();

        private NotificationService CreateSut(Dictionary<string, List<string>>? aliciGruplari = null)
        {
            var gruplar = aliciGruplari ?? new Dictionary<string, List<string>>
            {
                ["TumKullanicilar"] = new List<string> { "tumkullanicilar@test.com" },
                ["Yoneticiler"]     = new List<string> { "yoneticiler@test.com" },
                ["UstYonetim"]      = new List<string> { "ustyonetim@test.com" }
            };

            _mailRepoMock.Setup(r => r.AliciGruplariniGetir()).Returns(gruplar);
            _mailServiceMock
                .Setup(m => m.SendEmailAsync(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            return new NotificationService(_mailServiceMock.Object, _mailRepoMock.Object);
        }

        [Fact]
        public async Task GuncellemeNotifikasyonu_MajorGuncelleme_UcGrupDaHilEdilir()
        {
            var sut = CreateSut();
            var dto = new GuncellemeNotifikasyonDTO
            {
                VersiyonNumarasi = "2.0.0",
                GuncellemeTipi = "Major"
            };

            await sut.GuncellemeNotifikasyonuGonderAsync(dto, "");

            _mailServiceMock.Verify(m => m.SendEmailAsync(
                It.Is<List<string>>(alicilar =>
                    alicilar.Contains("tumkullanicilar@test.com") &&
                    alicilar.Contains("yoneticiler@test.com") &&
                    alicilar.Contains("ustyonetim@test.com")),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task GuncellemeNotifikasyonu_KritikDegisiklikVar_UcGrupDaHilEdilir()
        {
            var sut = CreateSut();
            var dto = new GuncellemeNotifikasyonDTO
            {
                VersiyonNumarasi = "1.5.0",
                GuncellemeTipi = "Minor",
                KritikDegisiklikler = new List<string> { "Veritabanı şeması güncellendi." }
            };

            await sut.GuncellemeNotifikasyonuGonderAsync(dto, "");

            _mailServiceMock.Verify(m => m.SendEmailAsync(
                It.Is<List<string>>(alicilar =>
                    alicilar.Contains("tumkullanicilar@test.com") &&
                    alicilar.Contains("yoneticiler@test.com") &&
                    alicilar.Contains("ustyonetim@test.com")),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task GuncellemeNotifikasyonu_MinorVeKritikYok_SadaceYoneticiler()
        {
            var sut = CreateSut();
            var dto = new GuncellemeNotifikasyonDTO
            {
                VersiyonNumarasi = "1.1.0",
                GuncellemeTipi = "Minor"
                // KritikDegisiklikler boş (constructor'da new List<string>())
            };

            await sut.GuncellemeNotifikasyonuGonderAsync(dto, "");

            _mailServiceMock.Verify(m => m.SendEmailAsync(
                It.Is<List<string>>(alicilar =>
                    alicilar.Contains("yoneticiler@test.com") &&
                    !alicilar.Contains("tumkullanicilar@test.com") &&
                    !alicilar.Contains("ustyonetim@test.com")),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>()), Times.Once);
        }

        // ─── AliciGrupGetir ───────────────────────────────────────────────────

        [Fact]
        public void AliciGrupGetir_MevcutGrup_ListeyiDoner()
        {
            var sut = CreateSut();

            var sonuc = sut.AliciGrupGetir("Yoneticiler");

            sonuc.Should().ContainSingle(e => e == "yoneticiler@test.com");
        }

        [Fact]
        public void AliciGrupGetir_YoktuGrup_BosListeDoner()
        {
            var sut = CreateSut();

            var sonuc = sut.AliciGrupGetir("OlmayenGrup");

            sonuc.Should().BeEmpty();
        }

        // ─── OnizlemeHtmlOlustur ──────────────────────────────────────────────

        [Fact]
        public void OnizlemeHtmlOlustur_DtoVerilir_HtmlIcerirVersiyonBilgisi()
        {
            var sut = CreateSut();
            var dto = new GuncellemeNotifikasyonDTO
            {
                VersiyonNumarasi = "3.1.4",
                GuncellemeTipi = "Minor"
            };

            var html = sut.OnizlemeHtmlOlustur(dto, "");

            html.Should().NotBeNullOrEmpty();
            html.Should().Contain("3.1.4");
        }

        // ─── OzelNotifikasyonGonderAsync ──────────────────────────────────────

        [Fact]
        public async Task OzelNotifikasyonGonderAsync_AliciListesiVerilir_MailServiseCagrilir()
        {
            var sut = CreateSut();
            var alicilar = new List<string> { "ozel@test.com" };

            await sut.OzelNotifikasyonGonderAsync(alicilar, "Test Konusu", "Test Mesaji");

            _mailServiceMock.Verify(m => m.SendEmailAsync(
                It.Is<List<string>>(l => l.Contains("ozel@test.com")),
                "Test Konusu",
                "Test Mesaji",
                true), Times.Once);
        }
    }
}
