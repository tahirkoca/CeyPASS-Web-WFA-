using CeyPASS.Business.Services;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    public class PuantajServiceExportTests
    {
        private readonly Mock<IPuantajRepository> _repoMock = new();
        private readonly PuantajService _sut;

        public PuantajServiceExportTests()
        {
            _sut = new PuantajService(_repoMock.Object);
        }

        // ─── DataTable yardımcıları ───────────────────────────────────────────

        private static DataTable BuildSicillerTable(int sicilNo = 1)
        {
            var dt = new DataTable();
            dt.Columns.Add("SicilNo", typeof(int));
            dt.Columns.Add("Ad", typeof(string));
            dt.Columns.Add("Soyad", typeof(string));
            dt.Columns.Add("Firma", typeof(string));
            dt.Columns.Add("Bolum", typeof(string));
            dt.Columns.Add("Isyeri", typeof(string));
            dt.Columns.Add("TcKimlikNo", typeof(string));
            dt.Columns.Add("DokPersoneliMi", typeof(int));
            // TcKimlikNo = DBNull → DenklestirmeYap grupları yoksayılır
            dt.Rows.Add(sicilNo, "Test", "Kişi", "Firma A", "IT", "Merkez", DBNull.Value, 0);
            return dt;
        }

        private static DataTable BuildCalismaSaatleriTable(
            params (int sicilNo, string tip, DateTime tarih, decimal saat)[] rows)
        {
            var dt = new DataTable();
            dt.Columns.Add("SicilNo", typeof(int));
            dt.Columns.Add("Ad", typeof(string));
            dt.Columns.Add("Soyad", typeof(string));
            dt.Columns.Add("CalismaTipi", typeof(string));
            dt.Columns.Add("Tarih", typeof(DateTime));
            dt.Columns.Add("Saat", typeof(decimal));
            foreach (var (sicilNo, tip, tarih, saat) in rows)
                dt.Rows.Add(sicilNo, "Test", "Kişi", tip, tarih, saat);
            return dt;
        }

        private void SetupRepo(DataTable sicillerDt, DataTable calismaDt)
        {
            _repoMock
                .Setup(r => r.GetSicillerAyIcin(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<FirmaIsyeriYetkiDTO>>()))
                .Returns(sicillerDt);
            _repoMock
                .Setup(r => r.GetVeriGirisleriAyIcin(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<FirmaIsyeriYetkiDTO>>()))
                .Returns(calismaDt);
        }

        private static PuantajExportRequest Req()
            => new PuantajExportRequest { Yil = 2025, Ay = 3, Yetkiler = new List<FirmaIsyeriYetkiDTO>() };

        // ─── HesaplaSskEksikNedeni ─────────────────────────────────────────────

        [Fact]
        public void PrepareMonthlyExport_SadeceUcretsizIzin_SskKodu21Doner()
        {
            var calisma = BuildCalismaSaatleriTable(
                (1, "Üİ", new DateTime(2025, 3, 5), 7.5m));
            SetupRepo(BuildSicillerTable(), calisma);

            var result = _sut.PrepareMonthlyExport(Req());

            result.Should().HaveCount(1);
            result[0].SskEksikNedeni.Should().Be("21");
        }

        [Fact]
        public void PrepareMonthlyExport_Rapor3ArdisikGun_SskKodu1Doner()
        {
            var calisma = BuildCalismaSaatleriTable(
                (1, "R", new DateTime(2025, 3, 3), 7.5m),
                (1, "R", new DateTime(2025, 3, 4), 7.5m),
                (1, "R", new DateTime(2025, 3, 5), 7.5m));
            SetupRepo(BuildSicillerTable(), calisma);

            var result = _sut.PrepareMonthlyExport(Req());

            // 3 ardışık R → raporGunSayisi=1 → rGunVarlik=1 → "1"
            result[0].SskEksikNedeni.Should().Be("1");
        }

        [Fact]
        public void PrepareMonthlyExport_SadeceDevreDisi_SskKodu15Doner()
        {
            var calisma = BuildCalismaSaatleriTable(
                (1, "D", new DateTime(2025, 3, 5), 7.5m));
            SetupRepo(BuildSicillerTable(), calisma);

            var result = _sut.PrepareMonthlyExport(Req());

            result[0].SskEksikNedeni.Should().Be("15");
        }

        [Fact]
        public void PrepareMonthlyExport_UiVeRapor3GunBirlikte_SskKodu12Doner()
        {
            var calisma = BuildCalismaSaatleriTable(
                (1, "Üİ", new DateTime(2025, 3, 6), 7.5m),
                (1, "R", new DateTime(2025, 3, 3), 7.5m),
                (1, "R", new DateTime(2025, 3, 4), 7.5m),
                (1, "R", new DateTime(2025, 3, 5), 7.5m));
            SetupRepo(BuildSicillerTable(), calisma);

            var result = _sut.PrepareMonthlyExport(Req());

            // uiGunVarlik=1, rGunVarlik=1 → sum > 1 → "12"
            result[0].SskEksikNedeni.Should().Be("12");
        }

        [Fact]
        public void PrepareMonthlyExport_SadeceNormalGun_SskKodu0Doner()
        {
            var calisma = BuildCalismaSaatleriTable(
                (1, "NG", new DateTime(2025, 3, 5), 7.5m));
            SetupRepo(BuildSicillerTable(), calisma);

            var result = _sut.PrepareMonthlyExport(Req());

            result[0].SskEksikNedeni.Should().Be("0");
        }

        // ─── HesaplaIskurEksikNedeni ───────────────────────────────────────────

        [Fact]
        public void PrepareMonthlyExport_SadeceRR_IskurKodu1Doner()
        {
            var calisma = BuildCalismaSaatleriTable(
                (1, "RR", new DateTime(2025, 3, 5), 7.5m));
            SetupRepo(BuildSicillerTable(), calisma);

            var result = _sut.PrepareMonthlyExport(Req());

            // RrGun=1, raporluGun=0 → ikinci if → "1"
            result[0].IskurEksikNedeni.Should().Be("1");
            // SSK tarafı da RR alone → "1"
            result[0].SskEksikNedeni.Should().Be("1");
        }

        [Fact]
        public void PrepareMonthlyExport_UiVe1RGun_IskurKodu2Doner()
        {
            var calisma = BuildCalismaSaatleriTable(
                (1, "Üİ", new DateTime(2025, 3, 4), 7.5m),
                (1, "R",  new DateTime(2025, 3, 5), 7.5m));
            SetupRepo(BuildSicillerTable(), calisma);

            var result = _sut.PrepareMonthlyExport(Req());

            // raporluGun=0 (1 R day → not enough for rapor), RrGun=0,
            // UiGun=1 > 0, RGun=1 ≤ 2 → "2"
            result[0].IskurEksikNedeni.Should().Be("2");
        }

        // ─── HesaplaFazlaMesai125 ──────────────────────────────────────────────

        [Fact]
        public void PrepareMonthlyExport_DokPersoneliMi1_Fm125BosDegilDoner()
        {
            var sicillerDt = new DataTable();
            sicillerDt.Columns.Add("SicilNo", typeof(int));
            sicillerDt.Columns.Add("Ad", typeof(string));
            sicillerDt.Columns.Add("Soyad", typeof(string));
            sicillerDt.Columns.Add("Firma", typeof(string));
            sicillerDt.Columns.Add("Bolum", typeof(string));
            sicillerDt.Columns.Add("Isyeri", typeof(string));
            sicillerDt.Columns.Add("TcKimlikNo", typeof(string));
            sicillerDt.Columns.Add("DokPersoneliMi", typeof(int));
            sicillerDt.Rows.Add(1, "DOK", "Kisi", "Firma A", "IT", "Merkez", DBNull.Value, 1);

            var calisma = BuildCalismaSaatleriTable(
                (1, "NG", new DateTime(2025, 3, 3), 7.5m));
            SetupRepo(sicillerDt, calisma);

            var result = _sut.PrepareMonthlyExport(Req());

            result.Should().HaveCount(1);
            result[0].FazlaMesai125Saat.Should().NotBe(" ");
        }

        // ─── HesaplaFazlaCalismaSaati ──────────────────────────────────────────

        [Fact]
        public void PrepareMonthlyExport_AafSaatEsikUstunde_FazlaCalismaSaatDogruHesaplanir()
        {
            var calisma = BuildCalismaSaatleriTable(
                (1, "AAF", new DateTime(2025, 3, 1), 4.0m),
                (1, "AAF", new DateTime(2025, 3, 2), 4.0m));
            SetupRepo(BuildSicillerTable(), calisma);

            var result = _sut.PrepareMonthlyExport(Req());

            // AafSaat=8 ≥ 3.75, AafGun=2 → aafHesap = 8 − (2 × 3.75) = 0.5
            result[0].FazlaCalismaSaat.Should().Be(0.5m);
        }
    }
}
