using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Personel avans talep ve onay akışı.</summary>
    public interface IAvansService
    {
        /// <summary>Yeni avans talebi oluşturur.</summary>
        int TalepOlustur(string personelId, decimal miktar, string? aciklama);

        /// <summary>Personelin kendi avans taleplerini listeler.</summary>
        List<AvansTalep> PersonelTalepleri(string personelId);

        /// <summary>Tüm avans taleplerini listeler (yönetim).</summary>
        List<AvansTalep> TumTalepler();

        /// <summary>Avans talebini onaylar.</summary>
        bool Onayla(int avansId, int onaylayanKullaniciId, string? aciklama);

        /// <summary>Avans talebini reddeder.</summary>
        bool Reddet(int avansId, int onaylayanKullaniciId, string? aciklama);

        /// <summary>Bekleyen talebi iptal eder.</summary>
        bool IptalEt(int avansId);

        /// <summary>Onay öncesi talep miktar/açıklama güncellemesi.</summary>
        bool Guncelle(int avansId, decimal miktar, string? aciklama);
    }
}

