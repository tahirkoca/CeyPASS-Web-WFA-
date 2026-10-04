using CeyPASS.Business.Abstractions;
using CeyPASS.Business.Services;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using FluentAssertions;
using Moq;
using System;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    /// <summary>
    /// Araç kartı atama kapanınca cihaz kısıtının otomatik kaldırılması.
    /// </summary>
    public class AracKartiServiceTests
    {
        private readonly Mock<IKisiRepository> _kisiRepoMock = new();
        private readonly Mock<IPuantajsizKartAtamaRepository> _atamaRepoMock = new();
        private readonly Mock<IKisiHareketRepository> _hareketRepoMock = new();
        private readonly Mock<ICanliIzlemeKartKomutService> _kartKomutSvcMock = new();
        private readonly AracKartiService _sut;

        public AracKartiServiceTests()
        {
            _sut = new AracKartiService(_kisiRepoMock.Object, _atamaRepoMock.Object, _hareketRepoMock.Object, _kartKomutSvcMock.Object);
        }

        [Fact]
        public void UpdateAssignment_AtamaKapaniyor_KisitKaldirilir()
        {
            var mevcut = new PuantajsizKartAtama { AtamaId = 1, KartId = "ARAC001", Bitis = null };
            _atamaRepoMock.Setup(a => a.GetById(1)).Returns(mevcut);
            _atamaRepoMock.Setup(a => a.GetCardFirmaId("ARAC001")).Returns(5);
            _kartKomutSvcMock.Setup(k => k.KisitVarsaKaldir(5, "ARAC001", 77)).Returns(true);

            var sonuc = _sut.UpdateAssignment(1, "Ali Veli", DateTime.Now.AddHours(-2), DateTime.Now,
                null, null, null, "34abc123", "P123", kullaniciId: 77);

            sonuc.Should().BeTrue();
            mevcut.Plaka.Should().Be("34ABC123");
            _kartKomutSvcMock.Verify(k => k.KisitVarsaKaldir(5, "ARAC001", 77), Times.Once);
        }

        [Fact]
        public void UpdateAssignment_CikisSaatiYok_KisitaDokunulmaz()
        {
            var mevcut = new PuantajsizKartAtama { AtamaId = 2, KartId = "ARAC002", Bitis = null };
            _atamaRepoMock.Setup(a => a.GetById(2)).Returns(mevcut);

            var sonuc = _sut.UpdateAssignment(2, "Ali Veli", DateTime.Now, null, null, null, null, "34ABC123", "P123");

            sonuc.Should().BeFalse();
            _kartKomutSvcMock.Verify(k => k.KisitVarsaKaldir(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public void UpdateAssignment_KartFirmasiBulunamaz_FalseDoner()
        {
            var mevcut = new PuantajsizKartAtama { AtamaId = 3, KartId = "ARAC003", Bitis = null };
            _atamaRepoMock.Setup(a => a.GetById(3)).Returns(mevcut);
            _atamaRepoMock.Setup(a => a.GetCardFirmaId("ARAC003")).Returns((int?)null);

            var sonuc = _sut.UpdateAssignment(3, "Ali Veli", DateTime.Now.AddHours(-1), DateTime.Now, null, null, null, "34ABC123", "P123");

            sonuc.Should().BeFalse();
            _kartKomutSvcMock.Verify(k => k.KisitVarsaKaldir(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int?>()), Times.Never);
        }
    }
}
