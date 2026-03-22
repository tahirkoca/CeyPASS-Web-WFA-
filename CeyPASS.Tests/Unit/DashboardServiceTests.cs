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
    public class DashboardServiceTests
    {
        private readonly Mock<IDashboardRepository> _repoMock = new();
        private readonly DashboardService _sut;

        public DashboardServiceTests()
        {
            _sut = new DashboardService(_repoMock.Object);
        }

        [Fact]
        public void GetDashboardForToday_AyBas_AyinIlkGunuOlmali()
        {
            DateTime capturedAyBas = default;
            _repoMock.Setup(r => r.ExecuteDashboard(
                    It.IsAny<string>(), It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                    It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
                .Callback<string, DateTime, DateTime, DateTime, double, double, int>(
                    (_, __, ayBas, ___, ____, _____, ______) => capturedAyBas = ayBas)
                .Returns(new DashboardResult());

            _sut.GetDashboardForToday(1);

            var bugun = DateTime.Today;
            capturedAyBas.Should().Be(new DateTime(bugun.Year, bugun.Month, 1));
        }

        [Fact]
        public void GetDashboard_NullFirmaIdList_BosStringGonderilir()
        {
            string capturedFirmaCsv = null!;
            _repoMock.Setup(r => r.ExecuteDashboard(
                    It.IsAny<string>(), It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                    It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
                .Callback<string, DateTime, DateTime, DateTime, double, double, int>(
                    (csv, _, _, _, _, _, _) => capturedFirmaCsv = csv)
                .Returns(new DashboardResult());

            _sut.GetDashboard(null, DateTime.Today, DateTime.Today, DateTime.Today, 0.25, 0.25, 30);

            capturedFirmaCsv.Should().Be("");
        }

        [Fact]
        public void GetDashboard_CokluFirma_VirgulylaAyrılmisStringGonderilir()
        {
            string capturedFirmaCsv = null!;
            _repoMock.Setup(r => r.ExecuteDashboard(
                    It.IsAny<string>(), It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                    It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
                .Callback<string, DateTime, DateTime, DateTime, double, double, int>(
                    (csv, _, _, _, _, _, _) => capturedFirmaCsv = csv)
                .Returns(new DashboardResult());

            _sut.GetDashboard(new[] { 1, 2, 3 }, DateTime.Today, DateTime.Today, DateTime.Today, 0.25, 0.25, 30);

            capturedFirmaCsv.Should().Be("1,2,3");
        }

        [Fact]
        public void GetDashboardForToday_AySon_AyinSonGunuOlmali()
        {
            DateTime capturedAySon = default;
            _repoMock.Setup(r => r.ExecuteDashboard(
                    It.IsAny<string>(), It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                    It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
                .Callback<string, DateTime, DateTime, DateTime, double, double, int>(
                    (_, __, ___, aySon, ____, _____, ______) => capturedAySon = aySon)
                .Returns(new DashboardResult());

            _sut.GetDashboardForToday(1);

            var bugun = DateTime.Today;
            int beklenenSonGun = DateTime.DaysInMonth(bugun.Year, bugun.Month);
            capturedAySon.Day.Should().Be(beklenenSonGun);
            capturedAySon.Month.Should().Be(bugun.Month);
        }
    }
}
