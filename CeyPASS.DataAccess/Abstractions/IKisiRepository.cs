using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface IKisiRepository
    {
        bool Exists(string personelId);
        List<KisiListItem> GetAktifByFirma(int firmId, string search = null, bool? puantajYapilirMi = true, int? isyeriId = null);
        KisiDetay GetDetay(string personelId);
        void SetIstenCikisTarihi(string personelId, DateTime tarih);
        List<Kisi> GetKisilerForPuantaj(int firmaId, int isyeriId, int yil, int ay);
        bool Update(Kisi k, string originalPersonelId, bool fotoDirty, string firmaDisiKartNo = null);
        void Insert(Kisi k, string firmaDisiKartNo = null);
        KisiAdSoyad GetAdSoyadByPersonelId(string personelId);
        List<PersonelCihazItem> GetAktifKartliPersonellerForSync();
        List<PersonelAdSoyad> GetAktifPersonellerIdAd();
        KisiDetayDTO GetById(int kisiId);
    }
}
