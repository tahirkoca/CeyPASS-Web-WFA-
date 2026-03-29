using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface IAvansRepository
    {
        int Ekle(AvansTalep talep);
        List<AvansTalep> GetByPersonel(string personelId);
        List<AvansTalep> GetAll();
        AvansTalep? GetById(int avansId);
        bool GuncelleOnay(int avansId, AvansDurumu durum, int onaylayanId, string? aciklama);
        bool Sil(int avansId);
        bool Guncelle(int avansId, decimal miktar, string? aciklama);
    }
}

