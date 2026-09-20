using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Puantajsız kart atama kayıtları.</summary>
    public interface IPuantajsizKartAtamaRepository
    {
        /// <summary>Today Active sorgularını getirir.</summary>
        List<PuantajsizKartAtama> GetTodayActive(DateTime now, int firmaId, bool? ziyaretciMi = null, bool? aracKartiMi = null);
        /// <summary>Bitis == null açık atamalar (gün filtresi yok).</summary>
        List<PuantajsizKartAtama> GetOpenActive(int firmaId, bool? ziyaretciMi = null, bool? aracKartiMi = null);
        /// <summary>Card Belongs To Firma işlemini gerçekleştirir.</summary>
        bool CardBelongsToFirma(string personelId, int firmaId);
        /// <summary>Exists Active For Card işlemini gerçekleştirir.</summary>
        bool ExistsActiveForCard(string personelId);
        /// <summary>Yeni kayıt ekler.</summary>
        int Insert(PuantajsizKartAtama a);
        /// <summary>Kimliğe göre kaydı getirir.</summary>
        PuantajsizKartAtama GetById(int id);
        /// <summary>Kaydı günceller.</summary>
        void Update(PuantajsizKartAtama a);
        /// <summary>Son Atama By Tc Kimlik No sorgularını getirir.</summary>
        PuantajsizKartAtama GetSonAtamaByTcKimlikNo(string tcKimlikNo);
        /// <summary>Gecmis Ziyaretciler sorgularını getirir.</summary>
        List<GecmisZiyaretciItem> GetGecmisZiyaretciler(int firmaId, string adFilter, bool? ziyaretciMi, bool? aracKartiMi);
    }
}
