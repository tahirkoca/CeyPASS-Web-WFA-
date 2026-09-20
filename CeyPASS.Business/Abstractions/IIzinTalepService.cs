using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>İzin talep onay zinciri (üst yetkili, İK, imza).</summary>
    public interface IIzinTalepService
    {
        /// <summary>Yeni izin talebi oluşturur.</summary>
        int TalepOlustur(IzinTalep talep, int talepEdenKullaniciId);

        /// <summary>Personelin kendi izin talepleri.</summary>
        List<IzinTalep> PersonelTalepleri(string personelId);

        /// <summary>Üst yetkili onayı bekleyen talepler.</summary>
        List<IzinTalep> UstYetkiliBekleyenler(string ustYetkiliPersonelId);

        /// <summary>Üst yetkili onayı.</summary>
        bool UstYetkiliOnayla(int talepId, string ustYetkiliPersonelId, string? aciklama);

        /// <summary>Üst yetkili reddi.</summary>
        bool UstYetkiliReddet(int talepId, string ustYetkiliPersonelId, string? aciklama);

        /// <summary>İK onayı bekleyen talepler.</summary>
        List<IzinTalep> IkBekleyenler();

        /// <summary>İK onayı.</summary>
        bool IkOnayla(int talepId, int ikKullaniciId, string? aciklama);

        /// <summary>İK reddi.</summary>
        bool IkReddet(int talepId, int ikKullaniciId, string? aciklama);

        /// <summary>İzin dönüş imza adımına açar.</summary>
        bool DonusImzasinaAc(int talepId, int ikKullaniciId);

        /// <summary>Personel kullanım imzasını kaydeder.</summary>
        bool KullanimImzaAt(int talepId, int personelKullaniciId);

        /// <summary>Personelin supervisor olup olmadığı.</summary>
        bool IsSupervisor(string personelId);
    }
}

