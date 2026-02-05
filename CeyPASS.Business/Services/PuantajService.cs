using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace CeyPASS.Business.Services
{
    public class PuantajService : IPuantajService
    {
        private readonly IPuantajRepository _repo;

        public PuantajService(IPuantajRepository repo)
        {
            _repo = repo;
        }
        private int Yuvarla30(int dakika)
        {
            if (dakika <= 0) return 0;
            return (int)Math.Round(dakika / 30.0, MidpointRounding.AwayFromZero) * 30;
        }
        public List<PuantajGunSatirDTO> GetAy(int personelId, int yil, int ay)
        {
            var rows = _repo.SpPuantajAyOzet(personelId, yil, ay);

            foreach (var r in rows)
            {
                // Erken Giriş
                if (r.IlkGiris.HasValue && r.VardiyaBaslangic.HasValue)
                {
                    var ilk = r.IlkGiris.Value;
                    var bas = r.VardiyaBaslangic.Value;
                    r.ErkenGirisDakika = ilk < bas ? (int)(bas - ilk).TotalMinutes : 0;
                }
                else r.ErkenGirisDakika = 0;

                // Geç Çıkış
                if (r.SonCikis.HasValue && r.VardiyaBitis.HasValue)
                {
                    var son = r.SonCikis.Value;
                    var bit = r.VardiyaBitis.Value;
                    r.GecCikisDakika = son > bit ? (int)(son - bit).TotalMinutes : 0;
                }
                else r.GecCikisDakika = 0;

                r.SistemFMDakika = Yuvarla30(r.ErkenGirisDakika) + Yuvarla30(r.GecCikisDakika);

                if (r.DuzenlenenFMDakika < 0) r.DuzenlenenFMDakika = 0;
            }

            return rows;
        }
        public void Onayla(int personelId, DateTime tarih, int duzenlenmisFm, string aciklama, string calismaTipi, decimal saat, int kullaniciId)
        {
            _repo.ApproveAndWriteFinal(personelId, tarih,
                                     onayDurumu: (int)OnayDurumu.Onaylandı,
                                     duzenlenmisFm: duzenlenmisFm,
                                     aciklama: aciklama,
                                     calismaTipi: calismaTipi,
                                     saat: saat,
                                     kullaniciId: kullaniciId);
        }
        public void Reddet(int personelId, DateTime tarih, string aciklama, int kullaniciId) => _repo.OnayUpsert(personelId, tarih, (int)OnayDurumu.Reddedildi, 0, aciklama, kullaniciId);
        public void Duzenle(int personelId, DateTime tarih, int duzenlenmisFm, string aciklama, int kullaniciId) => _repo.OnayUpsert(personelId, tarih, (int)OnayDurumu.Düzeltildi, duzenlenmisFm, aciklama, kullaniciId);
        public List<PuantajTipDTO> GetPuantajTipleri() => _repo.GetPuantajTipleri();
        public void DuzenleOnayla(int personelId, DateTime tarih, int duzenlenmisFm, string aciklama, string calismaTipi, decimal saat, int? kullaniciId)
        {
            _repo.ApproveAndWriteFinal(personelId, tarih, (int)OnayDurumu.Düzeltildi, duzenlenmisFm, aciklama, calismaTipi, saat, kullaniciId);
        }
        public void CokluSicileAktar(int anaPersonelId, int yil, int ay, int? kullaniciId) => _repo.CokluSicileAktar(anaPersonelId, yil, ay, kullaniciId);
        public int GetHedefSicilSayisi(int anaSicilNo) => _repo.GetHedefSicilSayisi(anaSicilNo);
        public bool IsAnaSicil(int sicilNo) => _repo.IsAnaSicil(sicilNo);
        public int GetEkKayitGun() => _repo.GetEkKayitGun();
        public void SetEkKayitGun(int gun, int uid) => _repo.SetEkKayitGun(gun, uid);
        public int HesaplaFazlaMesaiDakika(string calismaTipiKod, decimal saat)
        {
            const decimal NormalGunSaati = 7.5m;

            if (string.IsNullOrWhiteSpace(calismaTipiKod) ||
                !calismaTipiKod.StartsWith("FM", StringComparison.OrdinalIgnoreCase))
                return 0;

            var fazlaSaat = Math.Max(0m, saat - NormalGunSaati);
            return (int)Math.Round(fazlaSaat * 60m);
        }
        private decimal ParseSaatValue(object saatObj)
        {
            string saatStr = saatObj.ToString().Replace(",", ".");
            if (decimal.TryParse(saatStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal saat))
            {
                if (saat > 1000) saat /= 100;
                return saat;
            }
            return 0;
        }
        private PuantajExportDTO HesaplaPuantajSatiri(DataRow sicilRow, IEnumerable<dynamic> toplamSaatler, DataTable calismaSaatleriTablosu, int yil, int ay)
        {
            string sicilNo = sicilRow["SicilNo"].ToString();

            // 1. Kişinin saatlerini ve günlerini al
            var kisiSaatleri = toplamSaatler.Where(x => x.SicilNo == sicilNo);
            var saatGruplari = kisiSaatleri.ToDictionary(x => (string)x.CalismaTipi, x => (decimal)x.ToplamSaat);
            var gunGruplari = kisiSaatleri.ToDictionary(x => (string)x.CalismaTipi, x => (int)x.GunSayisi);

            // 2. Saat bazında değerleri çek
            var saatler = ExtractSaatler(saatGruplari);

            // 3. Gün bazında değerleri çek
            var gunler = ExtractGunler(gunGruplari);

            // 4. DTO oluştur ve temel bilgileri doldur
            var dto = new PuantajExportDTO
            {
                SicilNo = sicilNo,
                Ad = sicilRow["Ad"].ToString(),
                Soyad = sicilRow["Soyad"].ToString(),
                Firma = sicilRow["Firma"].ToString(),
                Bolum = sicilRow["Bolum"].ToString(),
                Isyeri = sicilRow["Isyeri"].ToString()
            };

            // 5. Hafta Tatili hesapla
            dto.HaftaTatiliGun = gunler.HtmGun + gunler.HtGun;

            // 6. Resmi Tatil hesapla
            dto.ResmiTatilGun = gunler.RtGun + gunler.BGun + gunler.BbGun + gunler.BbfGun + gunler.AafGun;

            // 7. Ücretli İzin hesapla
            dto.UcretliIzinGun = gunler.YiGun;

            // 8. Ücretsiz İzin hesapla
            dto.UcretsizIzinSaat = saatler.UiSaat + saatler.DSaat;

            // 9. Rapor günlerini hesapla
            var raporTarihleri = GetRaporTarihleri(calismaSaatleriTablosu, sicilNo);
            var raporSonuc = HesaplaRaporGunleri(raporTarihleri);
            dto.RaporGun = raporSonuc.RaporGunSayisi + gunler.RrGun;

            // 10. Normal saat hesapla
            decimal raporKaynakliNgSaat = raporSonuc.NgGunSayisi * 7.5m;
            decimal normalSaat = saatler.ASaat + saatler.DiSaat + saatler.OiSaat + saatler.EiSaat +
                                 saatler.NgSaat + ((gunler.AaGun + gunler.Fm1Gun) * 7.5m) + raporKaynakliNgSaat;
            dto.NormalSaat = normalSaat;

            // 11. Fazla Mesai hesapla
            dto.FazlaMesaiSaat = saatler.Fm1Saat - (gunler.Fm1Gun * 7.5m);

            // 12. Fazla Çalışma hesapla
            dto.FazlaCalismaSaat = HesaplaFazlaCalismaSaati(saatler, gunler);

            // 13. Fazla Mesai %125 hesapla
            dto.FazlaMesai125Saat = HesaplaFazlaMesai125(sicilRow, gunler);

            // 14. Resmi Tatil Saati hesapla
            dto.ResmiTatilSaat = saatler.AaSaat + (gunler.AafGun * 3.75m) + saatler.BbSaat + (gunler.BbfGun * 7.5m);

            // 15. SSK Eksik Çalışma Nedeni hesapla
            dto.SskEksikNedeni = HesaplaSskEksikNedeni(gunler, raporSonuc.RaporGunSayisi);

            // 16. İşkur Eksik Çalışma Nedeni hesapla
            dto.IskurEksikNedeni = HesaplaIskurEksikNedeni(gunler, raporSonuc.RaporGunSayisi);

            return dto;
        }
        private class SaatDegerleri
        {
            public decimal NgSaat { get; set; }
            public decimal Fm1Saat { get; set; }
            public decimal UiSaat { get; set; }
            public decimal RSaat { get; set; }
            public decimal HtSaat { get; set; }
            public decimal HtmSaat { get; set; }
            public decimal YiSaat { get; set; }
            public decimal RtSaat { get; set; }
            public decimal EiSaat { get; set; }
            public decimal OiSaat { get; set; }
            public decimal DiSaat { get; set; }
            public decimal DSaat { get; set; }
            public decimal BSaat { get; set; }
            public decimal BbSaat { get; set; }
            public decimal BbfSaat { get; set; }
            public decimal ASaat { get; set; }
            public decimal AaSaat { get; set; }
            public decimal AafSaat { get; set; }
            public decimal RrSaat { get; set; }
        }
        private class GunDegerleri
        {
            public int NgGun { get; set; }
            public int Fm1Gun { get; set; }
            public int UiGun { get; set; }
            public int RGun { get; set; }
            public int HtGun { get; set; }
            public int HtmGun { get; set; }
            public int YiGun { get; set; }
            public int RtGun { get; set; }
            public int EiGun { get; set; }
            public int OiGun { get; set; }
            public int DiGun { get; set; }
            public int DGun { get; set; }
            public int BGun { get; set; }
            public int BbGun { get; set; }
            public int BbfGun { get; set; }
            public int AGun { get; set; }
            public int AaGun { get; set; }
            public int AafGun { get; set; }
            public int RrGun { get; set; }
        }
        private SaatDegerleri ExtractSaatler(Dictionary<string, decimal> saatGruplari)
        {
            return new SaatDegerleri
            {
                NgSaat = saatGruplari.ContainsKey("NG") ? saatGruplari["NG"] : 0,
                Fm1Saat = saatGruplari.ContainsKey("FM1") ? saatGruplari["FM1"] : 0,
                UiSaat = saatGruplari.ContainsKey("Üİ") ? saatGruplari["Üİ"] : 0,
                RSaat = saatGruplari.ContainsKey("R") ? saatGruplari["R"] : 0,
                HtSaat = saatGruplari.ContainsKey("HT") ? saatGruplari["HT"] : 0,
                HtmSaat = saatGruplari.ContainsKey("HTM") ? saatGruplari["HTM"] : 0,
                YiSaat = saatGruplari.ContainsKey("Yİ") ? saatGruplari["Yİ"] : 0,
                RtSaat = saatGruplari.ContainsKey("RT") ? saatGruplari["RT"] : 0,
                EiSaat = saatGruplari.ContainsKey("Eİ") ? saatGruplari["Eİ"] : 0,
                OiSaat = saatGruplari.ContainsKey("Öİ") ? saatGruplari["Öİ"] : 0,
                DiSaat = saatGruplari.ContainsKey("Dİ") ? saatGruplari["Dİ"] : 0,
                DSaat = saatGruplari.ContainsKey("D") ? saatGruplari["D"] : 0,
                BSaat = saatGruplari.ContainsKey("B") ? saatGruplari["B"] : 0,
                BbSaat = saatGruplari.ContainsKey("BB") ? saatGruplari["BB"] : 0,
                BbfSaat = saatGruplari.ContainsKey("BBF") ? saatGruplari["BBF"] : 0,
                ASaat = saatGruplari.ContainsKey("A") ? saatGruplari["A"] : 0,
                AaSaat = saatGruplari.ContainsKey("AA") ? saatGruplari["AA"] : 0,
                AafSaat = saatGruplari.ContainsKey("AAF") ? saatGruplari["AAF"] : 0,
                RrSaat = saatGruplari.ContainsKey("RR") ? saatGruplari["RR"] : 0
            };
        }
        private GunDegerleri ExtractGunler(Dictionary<string, int> gunGruplari)
        {
            return new GunDegerleri
            {
                NgGun = gunGruplari.ContainsKey("NG") ? gunGruplari["NG"] : 0,
                Fm1Gun = gunGruplari.ContainsKey("FM1") ? gunGruplari["FM1"] : 0,
                UiGun = gunGruplari.ContainsKey("Üİ") ? gunGruplari["Üİ"] : 0,
                RGun = gunGruplari.ContainsKey("R") ? gunGruplari["R"] : 0,
                HtGun = gunGruplari.ContainsKey("HT") ? gunGruplari["HT"] : 0,
                HtmGun = gunGruplari.ContainsKey("HTM") ? gunGruplari["HTM"] : 0,
                YiGun = gunGruplari.ContainsKey("Yİ") ? gunGruplari["Yİ"] : 0,
                RtGun = gunGruplari.ContainsKey("RT") ? gunGruplari["RT"] : 0,
                EiGun = gunGruplari.ContainsKey("Eİ") ? gunGruplari["Eİ"] : 0,
                OiGun = gunGruplari.ContainsKey("Öİ") ? gunGruplari["Öİ"] : 0,
                DiGun = gunGruplari.ContainsKey("Dİ") ? gunGruplari["Dİ"] : 0,
                DGun = gunGruplari.ContainsKey("D") ? gunGruplari["D"] : 0,
                BGun = gunGruplari.ContainsKey("B") ? gunGruplari["B"] : 0,
                BbGun = gunGruplari.ContainsKey("BB") ? gunGruplari["BB"] : 0,
                BbfGun = gunGruplari.ContainsKey("BBF") ? gunGruplari["BBF"] : 0,
                AGun = gunGruplari.ContainsKey("A") ? gunGruplari["A"] : 0,
                AaGun = gunGruplari.ContainsKey("AA") ? gunGruplari["AA"] : 0,
                AafGun = gunGruplari.ContainsKey("AAF") ? gunGruplari["AAF"] : 0,
                RrGun = gunGruplari.ContainsKey("RR") ? gunGruplari["RR"] : 0
            };
        }
        private List<DateTime> GetRaporTarihleri(DataTable calismaSaatleriTablosu, string sicilNo)
        {
            var raporTarihleri = new List<DateTime>();

            foreach (DataRow rRow in calismaSaatleriTablosu.AsEnumerable().OrderBy(x => x.Field<DateTime>("Tarih")))
            {
                if (rRow["SicilNo"].ToString() == sicilNo && rRow["CalismaTipi"].ToString() == "R")
                {
                    if (DateTime.TryParse(rRow["Tarih"].ToString(), out DateTime tarih))
                    {
                        raporTarihleri.Add(tarih);
                    }
                }
            }

            return raporTarihleri;
        }
        private decimal HesaplaFazlaCalismaSaati(SaatDegerleri saatler, GunDegerleri gunler)
        {
            decimal fazlaCalismaSaat = 0;
            decimal aafHesap = 0;
            decimal bbfHesap = 0;

            if (saatler.AafSaat >= 3.75m)
            {
                aafHesap = saatler.AafSaat - (gunler.AafGun * 3.75m);
            }

            if (saatler.BbfSaat >= 7.5m)
            {
                bbfHesap = saatler.BbfSaat - (gunler.BbfGun * 7.5m);
            }

            fazlaCalismaSaat = aafHesap + bbfHesap + saatler.HtmSaat;
            return fazlaCalismaSaat;
        }
        private string HesaplaFazlaMesai125(DataRow sicilRow, GunDegerleri gunler)
        {
            if (sicilRow.Table.Columns.Contains("DokPersoneliMi") &&
                Convert.ToInt32(sicilRow["DokPersoneliMi"]) == 1)
            {
                return ((gunler.NgGun + gunler.Fm1Gun) * 3).ToString();
            }
            return " ";
        }
        private string HesaplaSskEksikNedeni(GunDegerleri gunler, int raporluGun)
        {
            int uiGunVarlik = gunler.UiGun > 0 ? 1 : 0;
            int rGunVarlik = raporluGun >= 1 ? 1 : 0;
            int dGunVarlik = gunler.DGun > 0 ? 1 : 0;
            int rrGunVarlik = gunler.RrGun > 0 ? 1 : 0;

            if (uiGunVarlik == 1 && rGunVarlik + dGunVarlik + rrGunVarlik == 0)
                return "21";
            else if (rGunVarlik == 1 && uiGunVarlik + dGunVarlik + rrGunVarlik == 0)
                return "1";
            else if (rrGunVarlik == 1 && uiGunVarlik + dGunVarlik + rGunVarlik == 0)
                return "1";
            else if (dGunVarlik == 1 && uiGunVarlik + rGunVarlik + rrGunVarlik == 0)
                return "15";
            else if (uiGunVarlik + rGunVarlik + dGunVarlik + rrGunVarlik > 1)
                return "12";
            else
                return "0";
        }
        private string HesaplaIskurEksikNedeni(GunDegerleri gunler, int raporluGun)
        {
            if (raporluGun >= 1)
                return "1";
            else if (gunler.RrGun > 0)
                return "1";
            else if (gunler.UiGun > 0 && gunler.RGun <= 2)
                return "2";
            else if (gunler.RGun > 2 && gunler.UiGun > 0)
                return "1";
            else
                return "0";
        }
        public RaporGunHesaplamaResult HesaplaRaporGunleri(List<DateTime> tarihler)
        {
            if (tarihler == null || tarihler.Count == 0)
                return new RaporGunHesaplamaResult { RaporGunSayisi = 0, NgGunSayisi = 0 };

            tarihler = tarihler.OrderBy(t => t).ToList();

            // Ardışık günleri grupla
            List<List<DateTime>> gruplar = new List<List<DateTime>>();
            List<DateTime> mevcutGrup = new List<DateTime> { tarihler[0] };

            for (int i = 1; i < tarihler.Count; i++)
            {
                if ((tarihler[i] - tarihler[i - 1]).TotalDays == 1)
                {
                    mevcutGrup.Add(tarihler[i]);
                }
                else
                {
                    gruplar.Add(mevcutGrup);
                    mevcutGrup = new List<DateTime> { tarihler[i] };
                }
            }
            gruplar.Add(mevcutGrup);

            int toplamGun = gruplar.Sum(g => g.Count);

            if (toplamGun <= 2)
            {
                return new RaporGunHesaplamaResult
                {
                    RaporGunSayisi = 0,
                    NgGunSayisi = toplamGun
                };
            }
            else
            {
                int raporGun = 0;
                int ngGun = 0;

                if (gruplar.Count == 1)
                {
                    ngGun = Math.Min(2, gruplar[0].Count);
                    raporGun = Math.Max(gruplar[0].Count - 2, 0);
                }
                else
                {
                    foreach (var grup in gruplar)
                    {
                        int adet = grup.Count;
                        ngGun += Math.Min(adet, 2);
                        raporGun += Math.Max(adet - 2, 0);
                    }
                }

                return new RaporGunHesaplamaResult
                {
                    RaporGunSayisi = raporGun,
                    NgGunSayisi = ngGun
                };
            }
        }
        private void DenklestirmeYap(List<PuantajExportDTO> liste, Dictionary<int, string> tcMap, int yil, int ay)
        {
            int ayGunSayisi = DateTime.DaysInMonth(yil, ay);

            // TC'ye göre grupla
            var tcGruplari = liste
                .GroupBy(dto => GetTcFromMap(dto.SicilNo, tcMap))
                .Where(g => !string.IsNullOrEmpty(g.Key))
                .ToList();

            foreach (var grup in tcGruplari)
            {
                // Toplam gün hesapla
                int toplamGun = grup.Sum(dto =>
                {
                    int normalGun = (int)Math.Floor(dto.NormalSaat / 7.5m);
                    return normalGun + dto.HaftaTatiliGun + dto.UcretliIzinGun + dto.ResmiTatilGun;
                });

                // Eğer toplam gün = ay günü değilse, denkleme yapma
                if (toplamGun != ayGunSayisi)
                    continue;

                // En yüksek değerlere sahip satırları bul
                var ngMax = grup.OrderByDescending(dto => dto.NormalSaat).FirstOrDefault();
                var htMax = grup.OrderByDescending(dto => dto.HaftaTatiliGun).FirstOrDefault();
                var yiMax = grup.OrderByDescending(dto => dto.UcretliIzinGun).FirstOrDefault();

                if (ngMax == null || htMax == null || yiMax == null)
                    continue;

                // Denkleme uygula
                if (ngMax.NormalSaat >= 7.5m)
                {
                    UygulaDuzenleme_Normal(ngMax, ayGunSayisi);
                }
                else if (htMax.HaftaTatiliGun >= 1)
                {
                    UygulaDuzenleme_HaftaTatili(htMax, ayGunSayisi);
                }
                else if (yiMax.UcretliIzinGun >= 1)
                {
                    UygulaDuzenleme_UcretliIzin(yiMax, ayGunSayisi);
                }
            }
        }
        private string GetTcFromMap(string sicilNo, Dictionary<int, string> tcMap)
        {
            if (!int.TryParse(sicilNo, out int personelId))
                return null;

            return tcMap.TryGetValue(personelId, out var tc) ? tc : null;
        }
        private void UygulaDuzenleme_Normal(PuantajExportDTO dto, int ayGun)
        {
            if (ayGun == 28)
                dto.NormalSaat += 15;
            else if (ayGun == 29)
                dto.NormalSaat += 7.5m;
            else if (ayGun == 30)
                dto.NormalSaat += 0;
            else
                dto.NormalSaat -= 7.5m;
        }
        private void UygulaDuzenleme_HaftaTatili(PuantajExportDTO dto, int ayGun)
        {
            int duzelt = (ayGun == 28) ? 2 : (ayGun == 29) ? 1 : (ayGun == 30) ? 0 : -1;
            dto.HaftaTatiliGun += duzelt;
        }
        private void UygulaDuzenleme_UcretliIzin(PuantajExportDTO dto, int ayGun)
        {
            int duzelt = (ayGun == 28) ? 2 : (ayGun == 29) ? 1 : (ayGun == 30) ? 0 : -1;
            dto.UcretliIzinGun += duzelt;
        }
        public bool IsRowEditable(DateTime tarih, int ekKayitGun)
        {
            DateTime today = DateTime.Today;
            DateTime currMonthBeg = new DateTime(today.Year, today.Month, 1);
            DateTime prevMonthBeg = currMonthBeg.AddMonths(-1);
            DateTime prevMonthEnd = currMonthBeg.AddDays(-1);

            if (ekKayitGun < 0) ekKayitGun = 0;

            // Gelecek ay tarihleri düzenlenemez
            if (tarih >= currMonthBeg.AddMonths(1))
                return false;

            // Bu ay tarihleri düzenlenebilir
            if (tarih >= currMonthBeg && tarih < currMonthBeg.AddMonths(1))
                return true;

            // Geçen ay için deadline kontrolü
            DateTime deadline = prevMonthEnd.AddDays(ekKayitGun);
            if (tarih >= prevMonthBeg && tarih <= prevMonthEnd)
                return today <= deadline;

            return false;
        }
        public decimal HesaplaFM1CalismaSaati(int fazlaMesaiDakika)
        {
            return Math.Round(7.5m + (decimal)fazlaMesaiDakika / 60m, 2);
        }
        public List<FirmaIsyeriYetkiDTO> GetKullaniciFirmaIsyeriYetkileri(int kullaniciId)=> _repo.GetKullaniciFirmaIsyeriYetkileri(kullaniciId);
        public DataTable GetSiciller(int yil, int ay, List<FirmaIsyeriYetkiDTO> yetkiler)
        {
            return _repo.GetSicillerAyIcin(yil, ay, yetkiler);
        }
        public DataTable GetVeriGirisleri(int yil, int ay, List<FirmaIsyeriYetkiDTO> yetkiler)
        {
            return _repo.GetVeriGirisleriAyIcin(yil, ay, yetkiler);
        }
        public List<PuantajExportDTO> PrepareMonthlyExport(PuantajExportRequest request)
        {
            var result = new List<PuantajExportDTO>();

            // 1. Verileri çek - YENİ YÖNTEM: Yetkiler listesini kullan
            var sicillerTablosu = GetSiciller(request.Yil, request.Ay, request.Yetkiler);
            var calismaSaatleriTablosu = GetVeriGirisleri(request.Yil, request.Ay, request.Yetkiler);

            // 2. TC Map oluştur
            var tcMap = sicillerTablosu.AsEnumerable()
                .ToDictionary(r => Convert.ToInt32(r["SicilNo"]),
                              r => r["TcKimlikNo"] == DBNull.Value ? null : r["TcKimlikNo"].ToString());

            // 3. Toplamları hesapla
            var toplamSaatler = calismaSaatleriTablosu.AsEnumerable()
                .GroupBy(x => new
                {
                    SicilNo = x["SicilNo"].ToString(),
                    Ad = x["Ad"].ToString(),
                    Soyad = x["Soyad"].ToString(),
                    CalismaTipi = x["CalismaTipi"].ToString()
                })
                .Select(grp => new
                {
                    SicilNo = grp.Key.SicilNo,
                    Ad = grp.Key.Ad,
                    Soyad = grp.Key.Soyad,
                    CalismaTipi = grp.Key.CalismaTipi,
                    ToplamSaat = grp.Sum(x => ParseSaatValue(x["Saat"])),
                    GunSayisi = grp.Select(x => x["Tarih"].ToString()).Distinct().Count()
                }).ToList();

            // 4. Her sicil için satır oluştur
            foreach (DataRow row in sicillerTablosu.Rows)
            {
                var dto = HesaplaPuantajSatiri(row, toplamSaatler, calismaSaatleriTablosu, request.Yil, request.Ay);
                result.Add(dto);
            }

            // 5. Denklestirme yap
            DenklestirmeYap(result, tcMap, request.Yil, request.Ay);

            return result;
        }
    }
}
