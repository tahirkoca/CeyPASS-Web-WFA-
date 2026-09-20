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
    /// <summary>
    /// Puantaj hesaplamaları: fazla mesai dakikası, rapor gün gruplama, satır düzenlenebilirlik, FM1 ve GetAy erken/geç çıkış.
    /// </summary>
    public class PuantajServiceTests
    {
        private readonly Mock<IPuantajRepository> _repoMock = new();
        private readonly PuantajService _sut;

        public PuantajServiceTests()
        {
            _sut = new PuantajService(_repoMock.Object);
        }

        // ─── HesaplaFazlaMesaiDakika ──────────────────────────────────────────

        /// <summary>
        /// FM1/FM2 dışındaki çalışma kodlarında fazla mesai dakikası hesaplanmaz (0).
        /// </summary>
        [Theory]
        [InlineData("N")]
        [InlineData("HT")]
        [InlineData("R")]
        [InlineData("NG")]
        [InlineData("")]
        public void HesaplaFazlaMesaiDakika_FMKoduYoksa_SifirDoner(string kod)
        {
            _sut.HesaplaFazlaMesaiDakika(kod, 9.0m).Should().Be(0);
        }

        /// <summary>
        /// FM1 için tam 7,5 saat çalışmada fazla mesai dakikası yoktur.
        /// </summary>
        [Fact]
        public void HesaplaFazlaMesaiDakika_SaatTam7_5_SifirDoner()
        {
            _sut.HesaplaFazlaMesaiDakika("FM1", 7.5m).Should().Be(0);
        }

        /// <summary>
        /// FM1 için 7,5 saatin altında çalışmada fazla mesai dakikası negatif sayılmaz (0).
        /// </summary>
        [Fact]
        public void HesaplaFazlaMesaiDakika_SaatAlti7_5_SifirDoner()
        {
            _sut.HesaplaFazlaMesaiDakika("FM1", 7.0m).Should().Be(0);
        }

        /// <summary>
        /// FM1: (saat − 7,5) × 60 formülü; 9 saat → 90 dakika fazla mesai.
        /// </summary>
        [Fact]
        public void HesaplaFazlaMesaiDakika_9Saat_90DakikaDoner()
        {
            // (9.0 - 7.5) * 60 = 90
            _sut.HesaplaFazlaMesaiDakika("FM1", 9.0m).Should().Be(90);
        }

        /// <summary>
        /// FM1: 8,25 saat → 0,75 saat × 60 = 45 dakika fazla mesai.
        /// </summary>
        [Fact]
        public void HesaplaFazlaMesaiDakika_8_25Saat_45DakikaDoner()
        {
            // (8.25 - 7.5) * 60 = 45
            _sut.HesaplaFazlaMesaiDakika("FM1", 8.25m).Should().Be(45);
        }

        // ─── HesaplaRaporGunleri ──────────────────────────────────────────────

        /// <summary>
        /// Rapor tarih listesi null ise NG ve rapor gün sayıları 0.
        /// </summary>
        [Fact]
        public void HesaplaRaporGunleri_NullListe_SifirDoner()
        {
            var sonuc = _sut.HesaplaRaporGunleri(null!);
            sonuc.NgGunSayisi.Should().Be(0);
            sonuc.RaporGunSayisi.Should().Be(0);
        }

        /// <summary>
        /// Boş rapor tarih listesinde NG ve rapor gün sayıları 0.
        /// </summary>
        [Fact]
        public void HesaplaRaporGunleri_BosListe_SifirDoner()
        {
            var sonuc = _sut.HesaplaRaporGunleri(new List<DateTime>());
            sonuc.NgGunSayisi.Should().Be(0);
            sonuc.RaporGunSayisi.Should().Be(0);
        }

        /// <summary>
        /// Tek rapor günü henüz 3'lü blok oluşturmaz: 1 NG, 0 rapor bloğu.
        /// </summary>
        [Fact]
        public void HesaplaRaporGunleri_TekTarih_BirNGSifirRapor()
        {
            var tarihler = new List<DateTime> { new DateTime(2025, 1, 1) };
            var sonuc = _sut.HesaplaRaporGunleri(tarihler);
            sonuc.NgGunSayisi.Should().Be(1);
            sonuc.RaporGunSayisi.Should().Be(0);
        }

        /// <summary>
        /// İki ardışık rapor günü hâlâ rapor bloğu sayılmaz (2 NG, 0 rapor).
        /// </summary>
        [Fact]
        public void HesaplaRaporGunleri_IkiTarih_IkiNGSifirRapor()
        {
            var tarihler = new List<DateTime>
            {
                new DateTime(2025, 1, 1),
                new DateTime(2025, 1, 2)
            };
            var sonuc = _sut.HesaplaRaporGunleri(tarihler);
            sonuc.NgGunSayisi.Should().Be(2);
            sonuc.RaporGunSayisi.Should().Be(0);
        }

        /// <summary>
        /// Üç ardışık rapor gününde 2 NG + 1 rapor bloğu sayılır.
        /// </summary>
        [Fact]
        public void HesaplaRaporGunleri_UcArdisikTarih_IkiNGBirRapor()
        {
            var tarihler = new List<DateTime>
            {
                new DateTime(2025, 1, 1),
                new DateTime(2025, 1, 2),
                new DateTime(2025, 1, 3)
            };
            var sonuc = _sut.HesaplaRaporGunleri(tarihler);
            sonuc.NgGunSayisi.Should().Be(2);
            sonuc.RaporGunSayisi.Should().Be(1);
        }

        /// <summary>
        /// Beş ardışık rapor gününde NG/rapor dağılımı: 2 NG, 3 rapor günü.
        /// </summary>
        [Fact]
        public void HesaplaRaporGunleri_BesArdisikTarih_IkiNGUcRapor()
        {
            var tarihler = new List<DateTime>
            {
                new DateTime(2025, 1, 1),
                new DateTime(2025, 1, 2),
                new DateTime(2025, 1, 3),
                new DateTime(2025, 1, 4),
                new DateTime(2025, 1, 5)
            };
            var sonuc = _sut.HesaplaRaporGunleri(tarihler);
            sonuc.NgGunSayisi.Should().Be(2);
            sonuc.RaporGunSayisi.Should().Be(3);
        }

        /// <summary>
        /// Kopuk iki 3'lü rapor koşusunda her koşu ayrı rapor bloğu: toplam 4 NG, 2 rapor.
        /// </summary>
        [Fact]
        public void HesaplaRaporGunleri_IkiAyriKosu_HerKostanIkisiNG()
        {
            // [1,2,3 Ocak] + [10,11,12 Ocak] → NG=4, Rapor=2
            var tarihler = new List<DateTime>
            {
                new DateTime(2025, 1, 1),
                new DateTime(2025, 1, 2),
                new DateTime(2025, 1, 3),
                new DateTime(2025, 1, 10),
                new DateTime(2025, 1, 11),
                new DateTime(2025, 1, 12)
            };
            var sonuc = _sut.HesaplaRaporGunleri(tarihler);
            sonuc.NgGunSayisi.Should().Be(4);
            sonuc.RaporGunSayisi.Should().Be(2);
        }

        // ─── IsRowEditable ────────────────────────────────────────────────────

        /// <summary>
        /// Gelecek aya ait puantaj satırı düzenlenemez.
        /// </summary>
        [Fact]
        public void IsRowEditable_GelecekAy_FalseDoner()
        {
            var gelecekAy = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1);
            _sut.IsRowEditable(gelecekAy, 30).Should().BeFalse();
        }

        /// <summary>
        /// Geçmiş günler (bugün hariç) mevcut ay içinde düzenlenebilir.
        /// </summary>
        [Fact]
        public void IsRowEditable_BuAy_TrueDoner()
        {
            // Bugün değil, dün düzenlenebilir olmalı (yeni mantık: bugün ve gelecek false)
            _sut.IsRowEditable(DateTime.Today.AddDays(-1), 0).Should().BeTrue();
        }

        /// <summary>
        /// Geçen ay satırları ek kayıt süresi (ekKayitGun) dolmadan düzenlenebilir.
        /// </summary>
        [Fact]
        public void IsRowEditable_GecenAy_DeadlineGecmemis_TrueDoner()
        {
            var gecenAy = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1);
            // ekKayitGun=100 → deadline her zaman ileride
            _sut.IsRowEditable(gecenAy, 100).Should().BeTrue();
        }

        /// <summary>
        /// Geçen ay için ek kayıt süresi 0 ise ay sonu geçildiğinde satır kilitlenir.
        /// </summary>
        [Fact]
        public void IsRowEditable_GecenAy_DeadlineGecmis_FalseDoner()
        {
            var gecenAy = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1);
            // ekKayitGun=0 → deadline = prevMonthEnd, today > prevMonthEnd → false
            _sut.IsRowEditable(gecenAy, 0).Should().BeFalse();
        }

        // ─── HesaplaFM1CalismaSaati ───────────────────────────────────────────

        /// <summary>
        /// FM1 taban çalışma saati 7,5; ek fazla mesai dakikası yokken 7,5 döner.
        /// </summary>
        [Fact]
        public void HesaplaFM1CalismaSaati_SifirDakika_7_5Doner()
        {
            _sut.HesaplaFM1CalismaSaati(0).Should().Be(7.5m);
        }

        /// <summary>
        /// FM1 çalışma saati = 7,5 + (dakika/60); 60 dk → 8,5 saat.
        /// </summary>
        [Fact]
        public void HesaplaFM1CalismaSaati_60Dakika_8_5Doner()
        {
            _sut.HesaplaFM1CalismaSaati(60).Should().Be(8.5m);
        }

        /// <summary>
        /// 90 dakika fazla mesai FM1 toplam çalışma saatini 9,0 yapar.
        /// </summary>
        [Fact]
        public void HesaplaFM1CalismaSaati_90Dakika_9_0Doner()
        {
            _sut.HesaplaFM1CalismaSaati(90).Should().Be(9.0m);
        }

        /// <summary>
        /// FM1 çalışma saati iki ondalık basamağa yuvarlanır (7,5 + 1,25 = 8,75).
        /// </summary>
        [Fact]
        public void HesaplaFM1CalismaSaati_75Dakika_IkiOndaligaYuvarlanir()
        {
            // 7.5 + 75/60 = 7.5 + 1.25 = 8.75
            _sut.HesaplaFM1CalismaSaati(75).Should().Be(8.75m);
        }

        // ─── GetAy ────────────────────────────────────────────────────────────

        /// <summary>
        /// Vardiya başlangıcından önce ilk giriş varsa erken giriş dakikası fark kadar yazılır.
        /// </summary>
        [Fact]
        public void GetAy_IlkGirisVardiyaBasindanOnce_ErkenGirisDakikasiHesaplanir()
        {
            var satir = new PuantajGunSatirDTO
            {
                Tarih = new DateTime(2025, 1, 6),
                VardiyaBaslangic = new TimeSpan(8, 0, 0),
                IlkGiris = new TimeSpan(7, 30, 0),   // 30 dk erken
                VardiyaBitis = new TimeSpan(17, 0, 0),
                SonCikis = new TimeSpan(17, 0, 0)
            };
            _repoMock.Setup(r => r.SpPuantajAyOzet(1, 2025, 1)).Returns(new List<PuantajGunSatirDTO> { satir });

            var sonuc = _sut.GetAy(1, 2025, 1);

            sonuc[0].ErkenGirisDakika.Should().Be(30);
        }

        /// <summary>
        /// Vardiya başlangıcından sonra girişte erken giriş dakikası 0 (geç kalma ayrı alan).
        /// </summary>
        [Fact]
        public void GetAy_IlkGirisVardiyaBasindanSonra_ErkenGirisDakikasiSifir()
        {
            var satir = new PuantajGunSatirDTO
            {
                Tarih = new DateTime(2025, 1, 6),
                VardiyaBaslangic = new TimeSpan(8, 0, 0),
                IlkGiris = new TimeSpan(8, 15, 0),   // 15 dk geç
                VardiyaBitis = new TimeSpan(17, 0, 0),
                SonCikis = new TimeSpan(17, 0, 0)
            };
            _repoMock.Setup(r => r.SpPuantajAyOzet(1, 2025, 1)).Returns(new List<PuantajGunSatirDTO> { satir });

            var sonuc = _sut.GetAy(1, 2025, 1);

            sonuc[0].ErkenGirisDakika.Should().Be(0);
        }

        /// <summary>
        /// Vardiya bitişinden sonra çıkışta geç çıkış dakikası hesaplanır.
        /// </summary>
        [Fact]
        public void GetAy_SonCikisVardiyaBitisindanSonra_GecCikisDakikasiHesaplanir()
        {
            var satir = new PuantajGunSatirDTO
            {
                Tarih = new DateTime(2025, 1, 6),
                VardiyaBaslangic = new TimeSpan(8, 0, 0),
                IlkGiris = new TimeSpan(8, 0, 0),
                VardiyaBitis = new TimeSpan(17, 0, 0),
                SonCikis = new TimeSpan(17, 45, 0)   // 45 dk geç çıkış
            };
            _repoMock.Setup(r => r.SpPuantajAyOzet(1, 2025, 1)).Returns(new List<PuantajGunSatirDTO> { satir });

            var sonuc = _sut.GetAy(1, 2025, 1);

            sonuc[0].GecCikisDakika.Should().Be(45);
        }

        /// <summary>
        /// Negatif düzenlenen FM dakikası 0'a klamp edilir.
        /// </summary>
        [Fact]
        public void GetAy_DuzenlenenFMNegatif_SifiraKlamplanir()
        {
            var satir = new PuantajGunSatirDTO
            {
                Tarih = new DateTime(2025, 1, 6),
                DuzenlenenFMDakika = -15   // negatif → 0 a klamplar
            };
            _repoMock.Setup(r => r.SpPuantajAyOzet(1, 2025, 1)).Returns(new List<PuantajGunSatirDTO> { satir });

            var sonuc = _sut.GetAy(1, 2025, 1);

            sonuc[0].DuzenlenenFMDakika.Should().Be(0);
        }
    }
}
