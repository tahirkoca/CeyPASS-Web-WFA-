using CeyPASS.Infrastructure.Helpers;
using FluentAssertions;
using System;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    /// <summary>
    /// Rapor sorgularında tek gün aralığı için gün başı / gün sonu zaman damgası normalizasyonu.
    /// </summary>
    public class RaporTarihHelperTests
    {
        /// <summary>
        /// Seçilen tarih saat bilgisinden bağımsız olarak aralık başlangıcı 00:00:00.
        /// </summary>
        [Fact]
        public void ToReportRangeStart_GunBasiDoner()
        {
            var input = new DateTime(2026, 5, 21, 14, 30, 45);
            RaporTarihHelper.ToReportRangeStart(input).Should().Be(new DateTime(2026, 5, 21, 0, 0, 0));
        }

        /// <summary>
        /// Aralık bitişi aynı günün 23:59:59 anı.
        /// </summary>
        [Fact]
        public void ToReportRangeEnd_GunSonuDoner()
        {
            var input = new DateTime(2026, 5, 21, 0, 0, 0);
            RaporTarihHelper.ToReportRangeEnd(input).Should().Be(new DateTime(2026, 5, 21, 23, 59, 59));
        }
    }
}
