using CeyPASS.Infrastructure.Helpers;
using FluentAssertions;
using Xunit;

namespace CeyPASS.Tests.Unit;

/// <summary>
/// Canlı izleme ekranında rol adına göre araç/canlı izleme ayrımı ve UI görünürlük kuralları.
/// </summary>
public class CanliIzlemeRoleHelperTests
{
    /// <summary>
    /// Rol metni ARAÇ/ARAC varyantlarında araç modu; diğer rollerde değil.
    /// </summary>
    [Theory]
    [InlineData("ARAÇ", true)]
    [InlineData("ARAC", true)]
    [InlineData("YEMEKHANE", false)]
    [InlineData("DANIŞMA", false)]
    public void IsArac_Beklenen(string rol, bool expected)
        => CanliIzlemeRoleHelper.IsArac(rol).Should().Be(expected);

    /// <summary>
    /// Yalnızca "canlı izleme" rolü (Türkçe/ASCII, büyük-küçük harf duyarsız) canlı izleme modu sayılır.
    /// </summary>
    [Theory]
    [InlineData("CANLI İZLEME", true)]
    [InlineData("CANLI IZLEME", true)]
    [InlineData("canli izleme", true)]
    [InlineData("DANIŞMA", false)]
    [InlineData("YEMEKHANE", false)]
    [InlineData("ARAÇ", false)]
    [InlineData("Operatör", false)]
    [InlineData("", false)]
    public void IsCanliIzleme_Beklenen(string rol, bool expected)
        => CanliIzlemeRoleHelper.IsCanliIzleme(rol).Should().Be(expected);

    /// <summary>
    /// Hareket listesi yalnızca canlı izleme rolünde gösterilir.
    /// </summary>
    [Theory]
    [InlineData("CANLI İZLEME", true)]
    [InlineData("DANIŞMA", false)]
    [InlineData("YEMEKHANE", false)]
    public void ShowHareketListesi_Beklenen(string rol, bool expected)
        => CanliIzlemeRoleHelper.ShowHareketListesi(rol).Should().Be(expected);

    /// <summary>
    /// Kart atama paneli canlı izleme rolünde gizlenir; diğer operasyon rollerinde görünür.
    /// </summary>
    [Theory]
    [InlineData("YEMEKHANE", true)]
    [InlineData("ARAÇ", true)]
    [InlineData("ARAC", true)]
    [InlineData("DANIŞMA", true)]
    [InlineData("Operatör", true)]
    [InlineData("CANLI İZLEME", false)]
    [InlineData("CANLI IZLEME", false)]
    public void HideKartAtama_Beklenen(string rol, bool expected)
        => CanliIzlemeRoleHelper.HideKartAtama(rol).Should().Be(expected);
}
