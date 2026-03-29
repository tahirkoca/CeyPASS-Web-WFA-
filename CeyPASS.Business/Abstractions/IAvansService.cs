using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    public interface IAvansService
    {
        int TalepOlustur(string personelId, decimal miktar, string? aciklama);
        List<AvansTalep> PersonelTalepleri(string personelId);
        List<AvansTalep> TumTalepler();
        bool Onayla(int avansId, int onaylayanKullaniciId, string? aciklama);
        bool Reddet(int avansId, int onaylayanKullaniciId, string? aciklama);
        bool IptalEt(int avansId);
        bool Guncelle(int avansId, decimal miktar, string? aciklama);
    }
}

