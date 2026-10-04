using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Misafir kartı atama ve ziyaretçi kayıtları.</summary>
    public interface IMisafirKartService
    {
        /// <summary>Yeni atama için uygun kartlar.</summary>
        List<KisiListItem> GetCardsForNew(int firmaId);

        /// <summary>Bugün aktif misafir atamaları.</summary>
        List<PuantajsizKartAtama> GetTodayActiveAssignments(DateTime now, int firmaId);

        /// <summary>Çıkışı yapılmamış açık atamalar.</summary>
        List<PuantajsizKartAtama> GetOpenActiveAssignments(int firmaId);

        /// <summary>Yeni misafir ataması.</summary>
        int CreateAssignment(int firmaId, string personelId, string misafirAdSoyad, DateTime girisSaati, string aciklama, string tcKimlikNo, string ziyaretEdilenKisi, string pasaportNo);

        /// <summary>Mevcut atamayı günceller. Atama kapanırken kart cihazda kısıtlıysa kısıtı kaldırır.</summary>
        /// <returns>Cihaz kısıtı kaldırıldıysa true.</returns>
        bool UpdateAssignment(int atamaId, string misafirAdSoyad, DateTime girisSaati, DateTime? cikisSaati, string aciklama, string tcKimlikNo, string ziyaretEdilenKisi, string pasaportNo, int? kullaniciId = null);

        /// <summary>TC ile misafir/atama bilgisi.</summary>
        PuantajsizKartAtama GetMisafirBilgisiByTc(string tcKimlikNo);

        /// <summary>Geçmiş ziyaretçi arama.</summary>
        List<GecmisZiyaretciItem> SearchGecmisZiyaretciler(int firmaId, string adFilter);

        /// <summary>Aktif puantajsız kartlar.</summary>
        List<KisiListItem> GetAktifKartlar(int firmaId);

        /// <summary>Atama liste ekranı satırları.</summary>
        List<KartAtamaListeItem> GetAtamaListe(int firmaId, IReadOnlyList<KisiListItem>? cachedKartlar = null);
    }
}
