using CeyPASS.Infrastructure.Helpers;
using FluentAssertions;
using Xunit;

namespace CeyPASS.Tests.Unit;

public class CanliIzlemeRoleHelperTests
{
    [Theory]
    [InlineData("ARAÇ", true)]
    [InlineData("ARAC", true)]
    [InlineData("YEMEKHANE", false)]
    [InlineData("DANIŞMA", false)]
    public void IsArac_Beklenen(string rol, bool expected)
        => CanliIzlemeRoleHelper.IsArac(rol).Should().Be(expected);

    [Theory]
    [InlineData("YEMEKHANE", true)]
    [InlineData("ARAÇ", true)]
    [InlineData("ARAC", true)]
    [InlineData("DANIŞMA", false)]
    [InlineData("Operatör", false)]
    public void HideKartAtama_Beklenen(string rol, bool expected)
        => CanliIzlemeRoleHelper.HideKartAtama(rol).Should().Be(expected);
}
