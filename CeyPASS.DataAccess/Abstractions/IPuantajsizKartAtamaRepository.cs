using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface IPuantajsizKartAtamaRepository
    {
        List<PuantajsizKartAtama> GetTodayActive(DateTime now, int firmaId);
        bool CardBelongsToFirma(int kartId, int firmaId);
        bool ExistsActiveForCard(int kartId);
        int Insert(PuantajsizKartAtama a);
        PuantajsizKartAtama GetById(int id);
        void Update(PuantajsizKartAtama a);
    }
}
