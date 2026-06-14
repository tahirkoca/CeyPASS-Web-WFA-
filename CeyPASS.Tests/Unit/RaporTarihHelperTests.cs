using CeyPASS.Infrastructure.Helpers;
using FluentAssertions;
using System;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    public class RaporTarihHelperTests
    {
        [Fact]
        public void ToReportRangeStart_GunBasiDoner()
        {
            var input = new DateTime(2026, 5, 21, 14, 30, 45);
            RaporTarihHelper.ToReportRangeStart(input).Should().Be(new DateTime(2026, 5, 21, 0, 0, 0));
        }

        [Fact]
        public void ToReportRangeEnd_GunSonuDoner()
        {
            var input = new DateTime(2026, 5, 21, 0, 0, 0);
            RaporTarihHelper.ToReportRangeEnd(input).Should().Be(new DateTime(2026, 5, 21, 23, 59, 59));
        }
    }
}
