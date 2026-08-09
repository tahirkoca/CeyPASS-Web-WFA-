using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    public interface IAracKartiService
    {
        List<KisiListItem> GetCardsForNew(int firmaId);
        List<PuantajsizKartAtama> GetTodayActiveAssignments(DateTime now, int firmaId);
        int CreateAssignment(int firmaId, string personelId, string adSoyad, DateTime girisSaati, string aciklama, string tcKimlikNo, string ziyaretEdilenKisi, string plaka);
        void UpdateAssignment(int atamaId, string adSoyad, DateTime girisSaati, DateTime? cikisSaati, string aciklama, string tcKimlikNo, string ziyaretEdilenKisi, string plaka);
        PuantajsizKartAtama GetBilgisiByTc(string tcKimlikNo);
        List<GecmisZiyaretciItem> SearchGecmisZiyaretciler(int firmaId, string adFilter);
    }
}
