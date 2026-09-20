using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Personel (Kisiler) master veri erişimi.</summary>
    public interface IKisiRepository
    {
        /// <summary>Exists işlemini gerçekleştirir.</summary>
        bool Exists(string personelId);
        /// <summary>Find By Personel Id işlemini gerçekleştirir.</summary>
        KisiAdSoyad FindByPersonelId(string personelId);
        /// <summary>Find By Tc Kimlik No işlemini gerçekleştirir.</summary>
        KisiAdSoyad FindByTcKimlikNo(string tcKimlikNo);
        /// <summary>Find By Kart No işlemini gerçekleştirir.</summary>
        KisiAdSoyad FindByKartNo(string kartNo);
        /// <summary>Aktif By Firma sorgularını getirir; isteğe bağlı bölüm filtresi.</summary>
        List<KisiListItem> GetAktifByFirma(int firmId, string search = null, bool? puantajYapilirMi = true, int? isyeriId = null, IReadOnlyList<int> isyeriIdIn = null, bool? ziyaretciMi = null, bool? aracKartiMi = null, bool sadeceIstenCikanlar = false, int? bolumId = null);
        /// <summary>Aktif By Firma Paged sorgularını getirir; isteğe bağlı bölüm filtresi.</summary>
        List<KisiListItem> GetAktifByFirmaPaged(int firmId, string search, bool? puantajYapilirMi, int? isyeriId, IReadOnlyList<int> isyeriIdIn, bool sadeceIstenCikanlar, int page, int pageSize, out int totalCount, int? bolumId = null);
        /// <summary>Search By Firma Paged işlemini gerçekleştirir.</summary>
        List<KisiSearchResultItem> SearchByFirmaPaged(KisiSearchFilter filter, int page, int pageSize, out int totalCount);
        /// <summary>Detay sorgularını getirir.</summary>
        KisiDetay GetDetay(string personelId);
        /// <summary>Isten Cikis Tarihi değerini ayarlar.</summary>
        void SetIstenCikisTarihi(string personelId, DateTime tarih);
        /// <summary>Tekrar Aktif Et işlemini gerçekleştirir.</summary>
        bool TekrarAktifEt(string personelId, bool puantajYapilirMi);
        /// <summary>Kisiler For Puantaj sorgularını getirir.</summary>
        List<Kisi> GetKisilerForPuantaj(int firmaId, int isyeriId, int yil, int ay);
        /// <summary>Kaydı günceller.</summary>
        bool Update(Kisi k, string originalPersonelId, bool fotoDirty, string firmaDisiKartNo = null);
        /// <summary>Yeni kayıt ekler.</summary>
        void Insert(Kisi k, string firmaDisiKartNo = null);
        /// <summary>Ad Soyad By Personel Id sorgularını getirir.</summary>
        KisiAdSoyad GetAdSoyadByPersonelId(string personelId);
        /// <summary>Aktif Kartli Personeller For Sync sorgularını getirir.</summary>
        List<PersonelCihazItem> GetAktifKartliPersonellerForSync();
        /// <summary>Aktif Personeller Id Ad sorgularını getirir.</summary>
        List<PersonelAdSoyad> GetAktifPersonellerIdAd();
        /// <summary>Kimliğe göre kaydı getirir.</summary>
        KisiDetayDTO GetById(int kisiId);
        /// <summary>By Login Identifier sorgularını getirir.</summary>
        Kisi GetByLoginIdentifier(string identifier);
    }
}
