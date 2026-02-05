using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.Business.Abstractions
{
    public interface IPuantajService
    {
        List<PuantajGunSatirDTO> GetAy(int personelId, int yil, int ay);
        void Onayla(int personelId, DateTime tarih, int duzenlenmisFm, string aciklama, string calismaTipi, decimal saat, int kullaniciId);
        void Reddet(int personelId, DateTime tarih, string aciklama, int kullaniciId);
        void Duzenle(int personelId, DateTime tarih, int duzenlenmisFm, string aciklama, int kullaniciId);
        List<PuantajTipDTO> GetPuantajTipleri();
        void DuzenleOnayla(int personelId, DateTime tarih, int duzenlenmisFm, string aciklama, string calismaTipi, decimal saat, int? kullaniciId);
        void CokluSicileAktar(int anaPersonelId, int yil, int ay, int? kullaniciId);
        int GetHedefSicilSayisi(int anaSicilNo);
        bool IsAnaSicil(int sicilNo);
        int GetEkKayitGun();
        void SetEkKayitGun(int gun, int uid);
        int HesaplaFazlaMesaiDakika(string calismaTipiKod, decimal saat);
        List<PuantajExportDTO> PrepareMonthlyExport(PuantajExportRequest request);
        RaporGunHesaplamaResult HesaplaRaporGunleri(List<DateTime> raporTarihleri);
        bool IsRowEditable(DateTime tarih, int ekKayitGun);
        decimal HesaplaFM1CalismaSaati(int fazlaMesaiDakika);
        List<FirmaIsyeriYetkiDTO> GetKullaniciFirmaIsyeriYetkileri(int kullaniciId);
        DataTable GetSiciller(int yil, int ay, List<FirmaIsyeriYetkiDTO> yetkiler);
        DataTable GetVeriGirisleri(int yil, int ay, List<FirmaIsyeriYetkiDTO> yetkiler);
    }
}
