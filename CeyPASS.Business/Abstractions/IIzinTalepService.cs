using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    public interface IIzinTalepService
    {
        int TalepOlustur(IzinTalep talep, int talepEdenKullaniciId);
        List<IzinTalep> PersonelTalepleri(string personelId);

        List<IzinTalep> UstYetkiliBekleyenler(string ustYetkiliPersonelId);
        bool UstYetkiliOnayla(int talepId, string ustYetkiliPersonelId, string? aciklama);
        bool UstYetkiliReddet(int talepId, string ustYetkiliPersonelId, string? aciklama);

        List<IzinTalep> IkBekleyenler();
        bool IkOnayla(int talepId, int ikKullaniciId, string? aciklama);
        bool IkReddet(int talepId, int ikKullaniciId, string? aciklama);

        bool DonusImzasinaAc(int talepId, int ikKullaniciId);
        bool KullanimImzaAt(int talepId, int personelKullaniciId);
        bool IsSupervisor(string personelId);
    }
}

