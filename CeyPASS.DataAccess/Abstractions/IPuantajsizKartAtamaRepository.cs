using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface IPuantajsizKartAtamaRepository
    {
        List<PuantajsizKartAtama> GetTodayActive(DateTime now, int firmaId);
        bool CardBelongsToFirma(string personelId, int firmaId);
        bool ExistsActiveForCard(string personelId);
        int Insert(PuantajsizKartAtama a);
        PuantajsizKartAtama GetById(int id);
        void Update(PuantajsizKartAtama a);
        PuantajsizKartAtama GetSonAtamaByTcKimlikNo(string tcKimlikNo);
    }
}
