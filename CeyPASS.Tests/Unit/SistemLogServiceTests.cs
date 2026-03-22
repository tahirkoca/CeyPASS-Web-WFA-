using CeyPASS.Business.Services;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using FluentAssertions;
using Moq;
using System;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    public class SistemLogServiceTests
    {
        private readonly Mock<ISistemLogRepository> _repoMock = new();
        private readonly SistemLogService _sut;

        public SistemLogServiceTests()
        {
            _sut = new SistemLogService(_repoMock.Object);
        }

        [Fact]
        public void Info_DogruIslemTuruVeAlanlarlaInsertCagrilir()
        {
            SistemLog captured = null!;
            _repoMock.Setup(r => r.Insert(It.IsAny<SistemLog>()))
                .Callback<SistemLog>(log => captured = log);

            _sut.Info(5, "Kaynak", "Islem", "Mesaj", "1.2.3.4", "PC1", "{}", "CID1");

            captured.Should().NotBeNull();
            captured.IslemTuru.Should().Be(IslemTuru.Info);
            captured.KullaniciId.Should().Be(5);
            captured.Kaynak.Should().Be("Kaynak");
            captured.Mesaj.Should().Be("Mesaj");
            captured.DetayJson.Should().Be("{}");
            captured.KorelasyonId.Should().Be("CID1");
        }

        [Fact]
        public void Warn_WarnIslemTuruyleInsertCagrilir()
        {
            SistemLog captured = null!;
            _repoMock.Setup(r => r.Insert(It.IsAny<SistemLog>()))
                .Callback<SistemLog>(log => captured = log);

            _sut.Warn(null, "Kaynak", "Islem", "Mesaj", "1.2.3.4", "PC1");

            captured.IslemTuru.Should().Be(IslemTuru.Warn);
            captured.KullaniciId.Should().BeNull();
        }

        [Fact]
        public void Error_ExceptionNullOldugunda_HataMesajiNullOlur()
        {
            SistemLog captured = null!;
            _repoMock.Setup(r => r.Insert(It.IsAny<SistemLog>()))
                .Callback<SistemLog>(log => captured = log);

            _sut.Error(1, "Kaynak", "Islem", "Mesaj", "1.2.3.4", "PC1", null!);

            captured.IslemTuru.Should().Be(IslemTuru.Error);
            captured.HataMesaji.Should().BeNull();
        }

        [Fact]
        public void Error_ExceptionVerildiginde_HataMesajiDoluOlur()
        {
            SistemLog captured = null!;
            _repoMock.Setup(r => r.Insert(It.IsAny<SistemLog>()))
                .Callback<SistemLog>(log => captured = log);

            var ex = new InvalidOperationException("Test hata mesajı");
            _sut.Error(1, "Kaynak", "Islem", "Mesaj", "1.2.3.4", "PC1", ex);

            captured.HataMesaji.Should().Contain("Test hata mesajı");
        }
    }
}
