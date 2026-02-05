using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    public interface IKisiService
    {
        void YeniKisiEkle(Kisi kisi, bool firmaPersoneli, bool puantajYapilabilir, bool yemekHakkiVar, int gunlukYemekLimiti, string puantajsizKartId, string puantajsizKartNo, string puantajsizKartAdi);
        bool KisiIstenCikar(string personelId, DateTime cikisTarihi, string firmaDisiKartNo);
        bool KisiGuncelle(Kisi kisi,string originalPersonelId,bool firmaPersoneli,bool puantajYapilabilir,bool yemekHakkiVar,int gunlukYemekAdedi,string firmaDisiKartNo,bool fotoDegisti);
        List<Kisi> GetKisilerForPuantaj(int firmaId, int isyeriId, int yil, int ay);
        KisiAdSoyad GetAdSoyad(string personelId);
        (bool IsValid, string? Message) ValidateKisiKayit(KisiKayitValidasyonDTO dto);
    }
}
