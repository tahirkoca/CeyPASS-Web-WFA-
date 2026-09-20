using CeyPASS.Business.Services;
using CeyPASS.Entities.Concrete;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace CeyPASS.Tests.Unit;

/// <summary>
/// Puantajsız kart atama listesinde durum (HAZIR/ATANMIŞ/GİRİŞ/ÇIKIŞ) çözümlemesi ve liste oluşturma.
/// </summary>
public class KartAtamaListeDurumHelperTests
{
    /// <summary>
    /// Atama yoksa HAZIR; atama var giriş/çıkış bilinmiyorsa ATANMIŞ; son hareket yönüne göre GİRİŞ/ÇIKIŞ.
    /// </summary>
    [Theory]
    [InlineData(false, null, KartAtamaListeDurum.Hazir)]
    [InlineData(true, null, KartAtamaListeDurum.Atanmis)]
    [InlineData(true, true, KartAtamaListeDurum.Giris)]
    [InlineData(true, false, KartAtamaListeDurum.Cikis)]
    public void Resolve_Beklenen(bool hasAtama, bool? girisMi, KartAtamaListeDurum expected)
        => KartAtamaListeDurumHelper.Resolve(hasAtama, girisMi).Should().Be(expected);

    /// <summary>
    /// Atama başlangıcından sonra hareket yoksa kart ATANMIŞ kalır (giriş/çıkış sayılmaz).
    /// </summary>
    [Fact]
    public void Build_AtamaSonrasiHareketYoksa_Atanmis()
    {
        var kartlar = new List<KisiListItem> { new() { PersonelId = "10", AdSoyad = "Kart A" } };
        var atama = new List<PuantajsizKartAtama>
        {
            new() { AtamaId = 1, KartId = "10", MisafirAdSoyad = "Ali", Baslangic = new DateTime(2026, 9, 9, 12, 0, 0) }
        };
        var yon = new Dictionary<string, PersonelSonHareketYon>(StringComparer.OrdinalIgnoreCase)
        {
            ["10"] = new PersonelSonHareketYon
            {
                PersonelId = "10",
                GirisMi = true,
                Tarih = new DateTime(2026, 9, 8, 10, 0, 0) // atamadan önce
            }
        };

        var list = KartAtamaListeBuilder.Build(kartlar, atama, yon);

        list.Should().ContainSingle();
        list[0].Durum.Should().Be(KartAtamaListeDurum.Atanmis);
        list[0].DurumText.Should().Be("ATANMIŞ");
        list[0].AtamaId.Should().Be(1);
    }

    /// <summary>
    /// Atamasız kart HAZIR durumunda ve AtamaId null.
    /// </summary>
    [Fact]
    public void Build_HazirKart_AtamaIdNull()
    {
        var kartlar = new List<KisiListItem> { new() { PersonelId = "5", AdSoyad = "Boş" } };
        var list = KartAtamaListeBuilder.Build(kartlar, new List<PuantajsizKartAtama>(), new Dictionary<string, PersonelSonHareketYon>());

        list[0].Durum.Should().Be(KartAtamaListeDurum.Hazir);
        list[0].AtamaId.Should().BeNull();
        list[0].DurumText.Should().Be("HAZIR");
    }
}
