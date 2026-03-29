using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface IIzinTalepRepository
    {
        int Ekle(IzinTalep talep);
        IzinTalep? GetById(int talepId);
        IzinTalep? GetBySonucKisiIzinId(int kisiIzinId);
        List<IzinTalep> GetByPersonel(string personelId);
        List<IzinTalep> GetUstYetkiliBekleyenler(string ustYetkiliPersonelId);
        List<IzinTalep> GetIkBekleyenler();

        bool UstYetkiliGuncelle(int talepId, IzinOnayDurumu durum, string? aciklama);
        bool IkGuncelle(int talepId, IzinOnayDurumu durum, int ikKullaniciId, string? aciklama);

        bool SetSonucKisiIzinId(int talepId, int kisiIzinId);

        bool DonusImzasinaAc(int talepId, int ikKullaniciId);
        bool KullanimImzaAt(int talepId, int personelKullaniciId);
    }
}

