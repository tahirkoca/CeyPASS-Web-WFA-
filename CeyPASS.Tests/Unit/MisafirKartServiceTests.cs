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
    public class MisafirKartServiceTests
    {
        private readonly Mock<IKisiRepository> _kisiRepoMock = new();
        private readonly Mock<IPuantajsizKartAtamaRepository> _atamaRepoMock = new();
        private readonly MisafirKartService _sut;

        public MisafirKartServiceTests()
        {
            _sut = new MisafirKartService(_kisiRepoMock.Object, _atamaRepoMock.Object);
        }

        // ─── GetCardsForNew ───────────────────────────────────────────────────

        [Fact]
        public void GetCardsForNew_NullPersonelId_Atlanir()
        {
            var kartlar = new List<KisiListItem>
            {
                new KisiListItem { PersonelId = null, AdSoyad = "Boş Kart" },
                new KisiListItem { PersonelId = "   ", AdSoyad = "Boş Kart 2" }
            };
            _kisiRepoMock.Setup(r => r.GetAktifByFirma(1, null, false, null, null, true)).Returns(kartlar);

            var sonuc = _sut.GetCardsForNew(1);

            sonuc.Should().BeEmpty();
        }

        [Fact]
        public void GetCardsForNew_AktifAtamaVarsa_Atlanir()
        {
            var kartlar = new List<KisiListItem>
            {
                new KisiListItem { PersonelId = "KART001", AdSoyad = "Ziyaretçi Kartı" }
            };
            _kisiRepoMock.Setup(r => r.GetAktifByFirma(1, null, false, null, null, true)).Returns(kartlar);
            _atamaRepoMock.Setup(a => a.ExistsActiveForCard("KART001")).Returns(true);

            var sonuc = _sut.GetCardsForNew(1);

            sonuc.Should().BeEmpty();
        }

        [Fact]
        public void GetCardsForNew_UygunKart_ListeyeEklenir()
        {
            var kartlar = new List<KisiListItem>
            {
                new KisiListItem { PersonelId = "KART001", AdSoyad = "Müsait Kart" }
            };
            _kisiRepoMock.Setup(r => r.GetAktifByFirma(1, null, false, null, null, true)).Returns(kartlar);
            _atamaRepoMock.Setup(a => a.ExistsActiveForCard("KART001")).Returns(false);

            var sonuc = _sut.GetCardsForNew(1);

            sonuc.Should().HaveCount(1);
            sonuc[0].PersonelId.Should().Be("KART001");
        }

        // ─── CreateAssignment ─────────────────────────────────────────────────

        [Fact]
        public void CreateAssignment_MisafirAdiBos_Exception()
        {
            Action act = () => _sut.CreateAssignment(1, "KART001", "  ", DateTime.Now, null, null, null);

            act.Should().Throw<ArgumentException>().WithMessage("*boş olamaz*");
        }

        [Fact]
        public void CreateAssignment_KartBaskaBirmaya_Exception()
        {
            _atamaRepoMock.Setup(a => a.CardBelongsToFirma("KART001", 1)).Returns(false);

            Action act = () => _sut.CreateAssignment(1, "KART001", "Ali Veli", DateTime.Now, null, null, null);

            act.Should().Throw<InvalidOperationException>().WithMessage("*firmaya ait değil*");
        }

        [Fact]
        public void CreateAssignment_AktifAtamaVar_Exception()
        {
            _atamaRepoMock.Setup(a => a.CardBelongsToFirma("KART001", 1)).Returns(true);
            _atamaRepoMock.Setup(a => a.ExistsActiveForCard("KART001")).Returns(true);

            Action act = () => _sut.CreateAssignment(1, "KART001", "Ali Veli", DateTime.Now, null, null, null);

            act.Should().Throw<InvalidOperationException>().WithMessage("*aktif bir atama*");
        }

        [Fact]
        public void CreateAssignment_GecerliVeri_InsertCagrilir()
        {
            _atamaRepoMock.Setup(a => a.CardBelongsToFirma("KART001", 1)).Returns(true);
            _atamaRepoMock.Setup(a => a.ExistsActiveForCard("KART001")).Returns(false);
            _atamaRepoMock.Setup(a => a.Insert(It.IsAny<PuantajsizKartAtama>())).Returns(42);

            var id = _sut.CreateAssignment(1, "KART001", "  Ali Veli  ", DateTime.Now, null, null, null);

            id.Should().Be(42);
            _atamaRepoMock.Verify(a => a.Insert(It.Is<PuantajsizKartAtama>(
                x => x.MisafirAdSoyad == "Ali Veli" && x.TCKimlikNo == null
            )), Times.Once);
        }

        // ─── UpdateAssignment ─────────────────────────────────────────────────

        [Fact]
        public void UpdateAssignment_KayitYok_Exception()
        {
            _atamaRepoMock.Setup(a => a.GetById(99)).Returns((PuantajsizKartAtama)null);

            Action act = () => _sut.UpdateAssignment(99, "Ali Veli", DateTime.Now, null, null, null, null);

            act.Should().Throw<InvalidOperationException>().WithMessage("*bulunamadı*");
        }

        [Fact]
        public void UpdateAssignment_GecerliVeri_RepoUpdateCagrilir()
        {
            var mevcut = new PuantajsizKartAtama { AtamaId = 1, KartId = "KART001" };
            _atamaRepoMock.Setup(a => a.GetById(1)).Returns(mevcut);

            _sut.UpdateAssignment(1, "  Ali Veli  ", new DateTime(2025, 3, 10, 9, 0, 0),
                new DateTime(2025, 3, 10, 17, 0, 0), "Not", "12345678901", "Ahmet");

            _atamaRepoMock.Verify(a => a.Update(It.Is<PuantajsizKartAtama>(r =>
                r.MisafirAdSoyad == "Ali Veli" &&
                r.Notlar == "Not" &&
                r.TCKimlikNo == "12345678901" &&
                r.ZiyaretEdilenKisi == "Ahmet"
            )), Times.Once);
        }

        [Fact]
        public void UpdateAssignment_BosAdSoyad_KayitVarken_ArgumentException()
        {
            var mevcut = new PuantajsizKartAtama { AtamaId = 2, KartId = "KART002" };
            _atamaRepoMock.Setup(a => a.GetById(2)).Returns(mevcut);

            Action act = () => _sut.UpdateAssignment(2, "   ", DateTime.Now, null, null, null, null);

            act.Should().Throw<ArgumentException>().WithMessage("*boş olamaz*");
        }

        // ─── GetMisafirBilgisiByTc ────────────────────────────────────────────

        [Fact]
        public void GetMisafirBilgisiByTc_BosTC_NullDoner()
        {
            _sut.GetMisafirBilgisiByTc(null).Should().BeNull();
            _sut.GetMisafirBilgisiByTc("   ").Should().BeNull();
        }

        [Fact]
        public void GetMisafirBilgisiByTc_GecerliTcBosluklu_TrimEdilipRepoCagrilir()
        {
            var beklenen = new PuantajsizKartAtama { AtamaId = 5 };
            _atamaRepoMock.Setup(r => r.GetSonAtamaByTcKimlikNo("12345678901")).Returns(beklenen);

            var sonuc = _sut.GetMisafirBilgisiByTc("  12345678901  ");

            _atamaRepoMock.Verify(r => r.GetSonAtamaByTcKimlikNo("12345678901"), Times.Once);
            sonuc.Should().Be(beklenen);
        }
    }
}
