using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Personel avans taleplerinin veri erişimi.</summary>
    public interface IAvansRepository
    {
        /// <summary>Ekle işlemini gerçekleştirir.</summary>
        int Ekle(AvansTalep talep);
        /// <summary>By Personel sorgularını getirir.</summary>
        List<AvansTalep> GetByPersonel(string personelId);
        /// <summary>All sorgularını getirir.</summary>
        List<AvansTalep> GetAll();
        /// <summary>Kimliğe göre kaydı getirir.</summary>
        AvansTalep? GetById(int avansId);
        /// <summary>Guncelle Onay işlemini gerçekleştirir.</summary>
        bool GuncelleOnay(int avansId, AvansDurumu durum, int onaylayanId, string? aciklama);
        /// <summary>Sil işlemini gerçekleştirir.</summary>
        bool Sil(int avansId);
        /// <summary>Guncelle işlemini gerçekleştirir.</summary>
        bool Guncelle(int avansId, decimal miktar, string? aciklama);
    }
}

