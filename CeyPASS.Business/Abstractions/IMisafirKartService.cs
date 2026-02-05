using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    public interface IMisafirKartService
    {
        List<PuantajsizKart> GetCardsForNew(int firmaId);
        List<PuantajsizKartAtama> GetTodayActiveAssignments(DateTime now, int firmaId);
        int CreateAssignment(int firmaId, int kartId, string misafirAdSoyad, DateTime girisSaati, string aciklama);
        void UpdateAssignment(int atamaId, string misafirAdSoyad, DateTime girisSaati, DateTime? cikisSaati, string aciklama);
    }
}
