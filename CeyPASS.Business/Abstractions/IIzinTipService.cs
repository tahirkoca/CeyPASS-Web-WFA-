using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    public interface IIzinTipService
    {
        List<IzinTip> GetAktif();
        int? GetSaatlikIzinTipId();
    }
}
