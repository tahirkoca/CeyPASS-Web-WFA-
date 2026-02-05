using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface IPuantajRepository
    {
        List<PuantajGunSatirDTO> SpPuantajAyOzet(int personelId, int yil, int ay);
        void Sp_OnayUpsert(object con, int personelId, DateTime tarih, int onayDurumu, int duzenlenmisFm, string aciklama, int? kullaniciId);
        void Sp_FinalUpsert(object con, int personelId, DateTime tarih, string calismaTipi, decimal saat, int? kullaniciId);
        void ApproveAndWriteFinal(int personelId, DateTime tarih, int onayDurumu, int duzenlenmisFm, string aciklama, string calismaTipi, decimal saat, int? kullaniciId);
        void OnayUpsert(int personelId, DateTime tarih, int onayDurumu, int duzenlenmisFm, string aciklama, int? kullaniciId);
        List<PuantajTipDTO> GetPuantajTipleri();
        void CokluSicileAktar(int anaKey, int yil, int ay, int? kullaniciId);
        int GetHedefSicilSayisi(int anaSicilNo);
        bool IsAnaSicil(int sicilNo);
        int GetEkKayitGun();
        void SetEkKayitGun(int gun, int? kullaniciId);
        List<FirmaIsyeriYetkiDTO> GetKullaniciFirmaIsyeriYetkileri(int kullaniciId);
        DataTable GetSicillerAyIcin(int yil, int ay, List<FirmaIsyeriYetkiDTO> yetkiler);
        DataTable GetVeriGirisleriAyIcin(int yil, int ay, List<FirmaIsyeriYetkiDTO> yetkiler);
    }
}
