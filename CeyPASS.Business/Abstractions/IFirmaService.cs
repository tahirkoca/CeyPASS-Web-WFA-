using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    public interface IFirmaService
    {
        List<Firma> GetAll();
        List<LookupItem> GetLookup();
        int SuggestNextId();
        bool Add(int id, string ad, string itMail, out string msg);
        bool Update(int id, string ad, string itMail, out string msg);
        bool Delete(int id);
        List<Firma> GetPuantajFirmalar();
    }
}
