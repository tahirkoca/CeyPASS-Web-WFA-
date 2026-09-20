using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>İzin tipi tanımları.</summary>
    public interface IIzinTipService
    {
        /// <summary>Aktif izin tiplerini listeler.</summary>
        List<IzinTip> GetAktif();

        /// <summary>Saatlik izin tipinin kimliği; yoksa null.</summary>
        int? GetSaatlikIzinTipId();
    }
}
