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
    /// <summary>
    /// Misafir/puantajsız kart oluşturma, atama ve kapatma.
    /// </summary>
    public class MisafirKartServiceTests
    {
        private readonly Mock<IKisiRepository> _kisiRepoMock = new();
        private readonly Mock<IPuantajsizKartAtamaRepository> _atamaRepoMock = new();
        private readonly Mock<IKisiHareketRepository> _hareketRepoMock = new();
        private readonly Mock<ICanliIzlemeKartKomutService> _kartKomutSvcMock = new();
        private readonly MisafirKartService _sut;

        public MisafirKartServiceTests()
        {
            _sut = new MisafirKartService(_kisiRepoMock.Object, _atamaRepoMock.Object, _hareketRepoMock.Object, _kartKomutSvcMock.Object);
        }

        // ─── GetCardsForNew ───────────────────────────────────────────────────

        /// <summary>
        /// NullPersonelId Atlanir
        /// </summary>
        [Fact]
        public void GetCardsForNew_NullPersonelId_Atlanir()
        {
            var kartlar = new List<KisiListItem>
            {
                new KisiListItem { PersonelId = null, AdSoyad = "Boş Kart" },
                new KisiListItem { PersonelId = "   ", AdSoyad = "Boş Kart 2" }
            };
            _kisiRepoMock.Setup(r => r.GetAktifByFirma(1, null, false, null, null, true, null, false, null)).Returns(kartlar);

            var sonuc = _sut.GetCardsForNew(1);

            sonuc.Should().BeEmpty();
        }

        /// <summary>
        /// AktifAtamaVarsa Atlanir
        /// </summary>
        [Fact]
        public void GetCardsForNew_AktifAtamaVarsa_Atlanir()
        {
            var kartlar = new List<KisiListItem>
            {
                new KisiListItem { PersonelId = "KART001", AdSoyad = "Ziyaretçi Kartı" }
            };
            _kisiRepoMock.Setup(r => r.GetAktifByFirma(1, null, false, null, null, true, null, false, null)).Returns(kartlar);
            _atamaRepoMock.Setup(a => a.ExistsActiveForCard("KART001")).Returns(true);

            var sonuc = _sut.GetCardsForNew(1);

            sonuc.Should().BeEmpty();
        }

        /// <summary>
        /// UygunKart ListeyeEklenir
        /// </summary>
        [Fact]
        public void GetCardsForNew_UygunKart_ListeyeEklenir()
        {
            var kartlar = new List<KisiListItem>
            {
                new KisiListItem { PersonelId = "KART001", AdSoyad = "Müsait Kart" }
            };
            _kisiRepoMock.Setup(r => r.GetAktifByFirma(1, null, false, null, null, true, null, false, null)).Returns(kartlar);
            _atamaRepoMock.Setup(a => a.ExistsActiveForCard("KART001")).Returns(false);

            var sonuc = _sut.GetCardsForNew(1);

            sonuc.Should().HaveCount(1);
            sonuc[0].PersonelId.Should().Be("KART001");
        }

        // ─── CreateAssignment ─────────────────────────────────────────────────

        /// <summary>
        /// MisafirAdiboş istisna fırlatılır
        /// </summary>
        [Fact]
        public void CreateAssignment_MisafirAdiBos_Exception()
        {
            Action act = () => _sut.CreateAssignment(1, "KART001", "  ", DateTime.Now, null, null, null, null);

            act.Should().Throw<ArgumentException>().WithMessage("*boş olamaz*");
        }

        /// <summary>
        /// TcVePasaportboş istisna fırlatılır
        /// </summary>
        [Fact]
        public void CreateAssignment_TcVePasaportBos_Exception()
        {
            Action act = () => _sut.CreateAssignment(1, "KART001", "Ali Veli", DateTime.Now, null, null, null, null);

            act.Should().Throw<ArgumentException>().WithMessage("*T.C. Kimlik No veya Pasaport No*");
        }

        /// <summary>
        /// TcMaskeli istisna fırlatılır
        /// </summary>
        [Fact]
        public void CreateAssignment_TcMaskeli_Exception()
        {
            Action act = () => _sut.CreateAssignment(1, "KART001", "Ali Veli", DateTime.Now, null, "1**********", null, null);

            act.Should().Throw<ArgumentException>().WithMessage("*11 haneli*");
        }

        /// <summary>
        /// SadecePasaport Insertçağrılır
        /// </summary>
        [Fact]
        public void CreateAssignment_SadecePasaport_InsertCagrilir()
        {
            _atamaRepoMock.Setup(a => a.CardBelongsToFirma("KART001", 1)).Returns(true);
            _atamaRepoMock.Setup(a => a.ExistsActiveForCard("KART001")).Returns(false);
            _atamaRepoMock.Setup(a => a.Insert(It.IsAny<PuantajsizKartAtama>())).Returns(7);

            var id = _sut.CreateAssignment(1, "KART001", "Ali Veli", DateTime.Now, null, null, null, "AB1234567");

            id.Should().Be(7);
            _atamaRepoMock.Verify(a => a.Insert(It.Is<PuantajsizKartAtama>(
                x => x.TCKimlikNo == null && x.PasaportNo == "AB1234567"
            )), Times.Once);
        }

        /// <summary>
        /// KartBaskaBirmaya istisna fırlatılır
        /// </summary>
        [Fact]
        public void CreateAssignment_KartBaskaBirmaya_Exception()
        {
            _atamaRepoMock.Setup(a => a.CardBelongsToFirma("KART001", 1)).Returns(false);

            Action act = () => _sut.CreateAssignment(1, "KART001", "Ali Veli", DateTime.Now, null, "12345678901", null, null);

            act.Should().Throw<InvalidOperationException>().WithMessage("*firmaya ait değil*");
        }

        /// <summary>
        /// AktifAtamaVar istisna fırlatılır
        /// </summary>
        [Fact]
        public void CreateAssignment_AktifAtamaVar_Exception()
        {
            _atamaRepoMock.Setup(a => a.CardBelongsToFirma("KART001", 1)).Returns(true);
            _atamaRepoMock.Setup(a => a.ExistsActiveForCard("KART001")).Returns(true);

            Action act = () => _sut.CreateAssignment(1, "KART001", "Ali Veli", DateTime.Now, null, "12345678901", null, null);

            act.Should().Throw<InvalidOperationException>().WithMessage("*aktif bir atama*");
        }

        /// <summary>
        /// GeçerliVeri Insertçağrılır
        /// </summary>
        [Fact]
        public void CreateAssignment_GecerliVeri_InsertCagrilir()
        {
            _atamaRepoMock.Setup(a => a.CardBelongsToFirma("KART001", 1)).Returns(true);
            _atamaRepoMock.Setup(a => a.ExistsActiveForCard("KART001")).Returns(false);
            _atamaRepoMock.Setup(a => a.Insert(It.IsAny<PuantajsizKartAtama>())).Returns(42);

            var id = _sut.CreateAssignment(1, "KART001", "  Ali Veli  ", DateTime.Now, null, "12345678901", null, null);

            id.Should().Be(42);
            _atamaRepoMock.Verify(a => a.Insert(It.Is<PuantajsizKartAtama>(
                x => x.MisafirAdSoyad == "Ali Veli" && x.TCKimlikNo == "12345678901" && x.PasaportNo == null
            )), Times.Once);
        }

        // ─── UpdateAssignment ─────────────────────────────────────────────────

        /// <summary>
        /// KayitYok istisna fırlatılır
        /// </summary>
        [Fact]
        public void UpdateAssignment_KayitYok_Exception()
        {
            _atamaRepoMock.Setup(a => a.GetById(99)).Returns((PuantajsizKartAtama)null);

            Action act = () => _sut.UpdateAssignment(99, "Ali Veli", DateTime.Now, null, null, null, null, null);

            act.Should().Throw<InvalidOperationException>().WithMessage("*bulunamadı*");
        }

        /// <summary>
        /// GeçerliVeri RepoUpdateçağrılır
        /// </summary>
        [Fact]
        public void UpdateAssignment_GecerliVeri_RepoUpdateCagrilir()
        {
            var mevcut = new PuantajsizKartAtama { AtamaId = 1, KartId = "KART001" };
            _atamaRepoMock.Setup(a => a.GetById(1)).Returns(mevcut);

            _sut.UpdateAssignment(1, "  Ali Veli  ", new DateTime(2025, 3, 10, 9, 0, 0),
                new DateTime(2025, 3, 10, 17, 0, 0), "Not", "12345678901", "Ahmet", "P123");

            _atamaRepoMock.Verify(a => a.Update(It.Is<PuantajsizKartAtama>(r =>
                r.MisafirAdSoyad == "Ali Veli" &&
                r.Notlar == "Not" &&
                r.TCKimlikNo == "12345678901" &&
                r.PasaportNo == "P123" &&
                r.ZiyaretEdilenKisi == "Ahmet"
            )), Times.Once);
        }

        /// <summary>
        /// BoşAdSoyad KayitVarken Argumentistisna fırlatılır
        /// </summary>
        [Fact]
        public void UpdateAssignment_BosAdSoyad_KayitVarken_ArgumentException()
        {
            var mevcut = new PuantajsizKartAtama { AtamaId = 2, KartId = "KART002" };
            _atamaRepoMock.Setup(a => a.GetById(2)).Returns(mevcut);

            Action act = () => _sut.UpdateAssignment(2, "   ", DateTime.Now, null, null, null, null, null);

            act.Should().Throw<ArgumentException>().WithMessage("*boş olamaz*");
        }

        /// <summary>
        /// Açık atama çıkış saatiyle kapanınca kart kısıtı kaldırılır ve true döner.
        /// </summary>
        [Fact]
        public void UpdateAssignment_AtamaKapaniyor_KisitKaldirilir()
        {
            var mevcut = new PuantajsizKartAtama { AtamaId = 3, KartId = "KART003", Bitis = null };
            _atamaRepoMock.Setup(a => a.GetById(3)).Returns(mevcut);
            _atamaRepoMock.Setup(a => a.GetCardFirmaId("KART003")).Returns(5);
            _kartKomutSvcMock.Setup(k => k.KisitVarsaKaldir(5, "KART003", 77)).Returns(true);

            var sonuc = _sut.UpdateAssignment(3, "Ali Veli", DateTime.Now.AddHours(-2), DateTime.Now,
                null, null, null, "P123", kullaniciId: 77);

            sonuc.Should().BeTrue();
            _kartKomutSvcMock.Verify(k => k.KisitVarsaKaldir(5, "KART003", 77), Times.Once);
        }

        /// <summary>
        /// Kapanan kart zaten serbestse false döner.
        /// </summary>
        [Fact]
        public void UpdateAssignment_AtamaKapaniyor_KartSerbest_FalseDoner()
        {
            var mevcut = new PuantajsizKartAtama { AtamaId = 4, KartId = "KART004", Bitis = null };
            _atamaRepoMock.Setup(a => a.GetById(4)).Returns(mevcut);
            _atamaRepoMock.Setup(a => a.GetCardFirmaId("KART004")).Returns(5);
            _kartKomutSvcMock.Setup(k => k.KisitVarsaKaldir(5, "KART004", null)).Returns(false);

            var sonuc = _sut.UpdateAssignment(4, "Ali Veli", DateTime.Now.AddHours(-2), DateTime.Now,
                null, null, null, "P123");

            sonuc.Should().BeFalse();
            _kartKomutSvcMock.Verify(k => k.KisitVarsaKaldir(5, "KART004", null), Times.Once);
        }

        /// <summary>
        /// Çıkış saati verilmezse (atama açık kalır) kısıta dokunulmaz.
        /// </summary>
        [Fact]
        public void UpdateAssignment_CikisSaatiYok_KisitaDokunulmaz()
        {
            var mevcut = new PuantajsizKartAtama { AtamaId = 5, KartId = "KART005", Bitis = null };
            _atamaRepoMock.Setup(a => a.GetById(5)).Returns(mevcut);

            var sonuc = _sut.UpdateAssignment(5, "Ali Veli", DateTime.Now, null, null, null, null, "P123");

            sonuc.Should().BeFalse();
            _kartKomutSvcMock.Verify(k => k.KisitVarsaKaldir(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int?>()), Times.Never);
        }

        /// <summary>
        /// Zaten kapalı atama yeniden kaydedilirse kısıt kaldırma tekrar tetiklenmez.
        /// </summary>
        [Fact]
        public void UpdateAssignment_ZatenKapali_KisitaDokunulmaz()
        {
            var mevcut = new PuantajsizKartAtama { AtamaId = 6, KartId = "KART006", Bitis = DateTime.Now.AddHours(-1) };
            _atamaRepoMock.Setup(a => a.GetById(6)).Returns(mevcut);

            var sonuc = _sut.UpdateAssignment(6, "Ali Veli", DateTime.Now.AddHours(-3), DateTime.Now, null, null, null, "P123");

            sonuc.Should().BeFalse();
            _kartKomutSvcMock.Verify(k => k.KisitVarsaKaldir(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int?>()), Times.Never);
        }

        // ─── GetMisafirBilgisiByTc ────────────────────────────────────────────

        /// <summary>
        /// BoşTC nulldöner
        /// </summary>
        [Fact]
        public void GetMisafirBilgisiByTc_BosTC_NullDoner()
        {
            _sut.GetMisafirBilgisiByTc(null).Should().BeNull();
            _sut.GetMisafirBilgisiByTc("   ").Should().BeNull();
        }

        /// <summary>
        /// GeçerliTcboşluklu TrimEdilipRepoçağrılır
        /// </summary>
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
