using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface IKisiRepository
    {
        bool Exists(string personelId);
        KisiAdSoyad FindByPersonelId(string personelId);
        KisiAdSoyad FindByTcKimlikNo(string tcKimlikNo);
        KisiAdSoyad FindByKartNo(string kartNo);
        List<KisiListItem> GetAktifByFirma(int firmId, string search = null, bool? puantajYapilirMi = true, int? isyeriId = null, IReadOnlyList<int> isyeriIdIn = null, bool? ziyaretciMi = null, bool? aracKartiMi = null, bool sadeceIstenCikanlar = false);
        List<KisiListItem> GetAktifByFirmaPaged(int firmId, string search, bool? puantajYapilirMi, int? isyeriId, IReadOnlyList<int> isyeriIdIn, bool sadeceIstenCikanlar, int page, int pageSize, out int totalCount);
        List<KisiSearchResultItem> SearchByFirmaPaged(KisiSearchFilter filter, int page, int pageSize, out int totalCount);
        KisiDetay GetDetay(string personelId);
        void SetIstenCikisTarihi(string personelId, DateTime tarih);
        bool TekrarAktifEt(string personelId, bool puantajYapilirMi);
        List<Kisi> GetKisilerForPuantaj(int firmaId, int isyeriId, int yil, int ay);
        bool Update(Kisi k, string originalPersonelId, bool fotoDirty, string firmaDisiKartNo = null);
        void Insert(Kisi k, string firmaDisiKartNo = null);
        KisiAdSoyad GetAdSoyadByPersonelId(string personelId);
        List<PersonelCihazItem> GetAktifKartliPersonellerForSync();
        List<PersonelAdSoyad> GetAktifPersonellerIdAd();
        KisiDetayDTO GetById(int kisiId);
        Kisi GetByLoginIdentifier(string identifier);
    }
}
