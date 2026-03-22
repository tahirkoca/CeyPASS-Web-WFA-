using CeyPASS.Business.Services;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using FluentAssertions;
using Moq;
using System;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    public class KisiServiceAdditionalTests
    {
        private readonly Mock<IKisiRepository> _kisiRepoMock = new();
        private readonly Mock<IYemekhaneRepository> _yemekhaneRepoMock = new();
        private readonly KisiService _sut;

        public KisiServiceAdditionalTests()
        {
            _sut = new KisiService(_kisiRepoMock.Object, _yemekhaneRepoMock.Object);
        }

        private static Kisi OrnekKisi(string personelId = "TEST001") =>
            new Kisi { PersonelId = personelId, FirmaId = 1 };

        // ─── KisiGuncelle ─────────────────────────────────────────────────────

        [Fact]
        public void KisiGuncelle_RepoBBasarisiz_FalseDoner()
        {
            _kisiRepoMock.Setup(r => r.Update(It.IsAny<Kisi>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>()))
                         .Returns(false);

            var sonuc = _sut.KisiGuncelle(OrnekKisi(), "TEST001", true, true, false, 0, null, false);

            sonuc.Should().BeFalse();
        }

        [Fact]
        public void KisiGuncelle_YemekHakkiVarAdediPositif_UpsertLimitCagrilir()
        {
            _kisiRepoMock.Setup(r => r.Update(It.IsAny<Kisi>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>()))
                         .Returns(true);

            var sonuc = _sut.KisiGuncelle(OrnekKisi(), "TEST001", true, true, true, 3, null, false);

            sonuc.Should().BeTrue();
            _yemekhaneRepoMock.Verify(y => y.UpsertLimit("TEST001", 3), Times.Once);
            _yemekhaneRepoMock.Verify(y => y.PasifEtByPersonel(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void KisiGuncelle_YemekHakkiYok_PasifEtByPersonelCagrilir()
        {
            _kisiRepoMock.Setup(r => r.Update(It.IsAny<Kisi>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>()))
                         .Returns(true);

            var sonuc = _sut.KisiGuncelle(OrnekKisi(), "TEST001", true, true, false, 0, null, false);

            sonuc.Should().BeTrue();
            _yemekhaneRepoMock.Verify(y => y.PasifEtByPersonel("TEST001"), Times.Once);
            _yemekhaneRepoMock.Verify(y => y.UpsertLimit(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void KisiGuncelle_ExceptionFirlatilinca_FalseDoner()
        {
            _kisiRepoMock.Setup(r => r.Update(It.IsAny<Kisi>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>()))
                         .Throws(new Exception("DB hatası"));

            var sonuc = _sut.KisiGuncelle(OrnekKisi(), "TEST001", true, true, false, 0, null, false);

            sonuc.Should().BeFalse();
        }

        // ─── YeniKisiEkle ─────────────────────────────────────────────────────

        [Fact]
        public void YeniKisiEkle_PuantajYapilabilirTrue_PuantajYapilirMiTrue()
        {
            var kisi = OrnekKisi();

            _sut.YeniKisiEkle(kisi, true, true, false, 0, null, null, null);

            kisi.PuantajYapilirMi.Should().BeTrue();
        }

        [Fact]
        public void YeniKisiEkle_PuantajYapilabilirFalse_PuantajYapilirMiFalse()
        {
            var kisi = OrnekKisi();

            _sut.YeniKisiEkle(kisi, true, false, false, 0, null, null, null);

            kisi.PuantajYapilirMi.Should().BeFalse();
        }

        [Fact]
        public void YeniKisiEkle_YemekHakkiVar_InsertLimitCagrilir()
        {
            var kisi = OrnekKisi();

            _sut.YeniKisiEkle(kisi, true, true, true, 3, null, null, null);

            _yemekhaneRepoMock.Verify(y => y.InsertLimit(kisi.PersonelId, 3), Times.Once);
        }

        [Fact]
        public void YeniKisiEkle_YemekHakkiYok_InsertLimitCagrilmaz()
        {
            var kisi = OrnekKisi();

            _sut.YeniKisiEkle(kisi, true, true, false, 0, null, null, null);

            _yemekhaneRepoMock.Verify(y => y.InsertLimit(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        // ─── KisiIstenCikar ───────────────────────────────────────────────────

        [Fact]
        public void KisiIstenCikar_Basarili_TrueDoner()
        {
            var sonuc = _sut.KisiIstenCikar("TEST001", DateTime.Today, null);

            sonuc.Should().BeTrue();
            _kisiRepoMock.Verify(r => r.SetIstenCikisTarihi("TEST001", DateTime.Today), Times.Once);
            _yemekhaneRepoMock.Verify(y => y.PasifEtByPersonel("TEST001"), Times.Once);
        }

        [Fact]
        public void KisiIstenCikar_RepoException_FalseDoner()
        {
            _kisiRepoMock.Setup(r => r.SetIstenCikisTarihi(It.IsAny<string>(), It.IsAny<DateTime>()))
                         .Throws(new Exception("DB hatası"));

            var sonuc = _sut.KisiIstenCikar("TEST001", DateTime.Today, null);

            sonuc.Should().BeFalse();
        }
    }
}
