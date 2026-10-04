using CeyPASS.Business.Services;
using FluentAssertions;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    /// <summary>
    /// Yemek hakkı değişikliğinin yemekhane cihazı etkisi metinleri.
    /// </summary>
    public class YemekhaneEtkiMesajiTests
    {
        [Theory]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        public void KaldirmaOnayiGerekir_YalnizcaKaldirmada(bool onceki, bool yeni, bool beklenen)
        {
            YemekhaneEtkiMesaji.KaldirmaOnayiGerekir(onceki, yeni).Should().Be(beklenen);
        }

        [Fact]
        public void Guncelleme_Verildi()
        {
            YemekhaneEtkiMesaji.Guncelleme(false, true).Should().Be(YemekhaneEtkiMesaji.Verildi);
        }

        [Fact]
        public void Guncelleme_Kaldirildi()
        {
            YemekhaneEtkiMesaji.Guncelleme(true, false).Should().Be(YemekhaneEtkiMesaji.Kaldirildi);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Guncelleme_DegismediyseNull(bool durum)
        {
            YemekhaneEtkiMesaji.Guncelleme(durum, durum).Should().BeNull();
        }

        [Fact]
        public void YeniKayit_YemekHakkiYoksaPasifMetni()
        {
            YemekhaneEtkiMesaji.YeniKayit(false).Should().Be(YemekhaneEtkiMesaji.YemekHakkiYok);
            YemekhaneEtkiMesaji.YeniKayit(true).Should().Be(YemekhaneEtkiMesaji.Verildi);
        }

        [Fact]
        public void Ekle_EtkiYoksaMesajAyni()
        {
            YemekhaneEtkiMesaji.Ekle("Kayıt güncellendi.", null).Should().Be("Kayıt güncellendi.");
            YemekhaneEtkiMesaji.Ekle("Kayıt güncellendi.", "X").Should().Be("Kayıt güncellendi.\n\nX");
        }
    }
}
