using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Araç giriş kartı atama ve ziyaretçi kayıtları.</summary>
    public interface IAracKartiService
    {
        /// <summary>Yeni atama için uygun kartlar.</summary>
        List<KisiListItem> GetCardsForNew(int firmaId);

        /// <summary>Bugün aktif araç atamaları.</summary>
        List<PuantajsizKartAtama> GetTodayActiveAssignments(DateTime now, int firmaId);

        /// <summary>Çıkışı yapılmamış açık atamalar.</summary>
        List<PuantajsizKartAtama> GetOpenActiveAssignments(int firmaId);

        /// <summary>Yeni araç ziyaret ataması (plaka dahil).</summary>
        int CreateAssignment(int firmaId, string personelId, string adSoyad, DateTime girisSaati, string aciklama, string tcKimlikNo, string ziyaretEdilenKisi, string plaka, string pasaportNo);

        /// <summary>Mevcut atamayı günceller.</summary>
        void UpdateAssignment(int atamaId, string adSoyad, DateTime girisSaati, DateTime? cikisSaati, string aciklama, string tcKimlikNo, string ziyaretEdilenKisi, string plaka, string pasaportNo);

        /// <summary>TC ile son ziyaretçi/atama bilgisi.</summary>
        PuantajsizKartAtama GetBilgisiByTc(string tcKimlikNo);

        /// <summary>Geçmiş ziyaretçi arama.</summary>
        List<GecmisZiyaretciItem> SearchGecmisZiyaretciler(int firmaId, string adFilter);

        /// <summary>Aktif puantajsız kartlar.</summary>
        List<KisiListItem> GetAktifKartlar(int firmaId);

        /// <summary>Atama liste ekranı satırları.</summary>
        List<KartAtamaListeItem> GetAtamaListe(int firmaId, IReadOnlyList<KisiListItem>? cachedKartlar = null);
    }
}
