using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface IIzinTipRepository
    {
        List<IzinTip> GetAktifIzinTipleri();     
        int? GetSaatlikKullanilabilirTipId();
    }
}
