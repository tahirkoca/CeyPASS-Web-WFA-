using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>İzin tipi tanımları.</summary>
    public interface IIzinTipRepository
    {
        /// <summary>Aktif Izin Tipleri sorgularını getirir.</summary>
        List<IzinTip> GetAktifIzinTipleri();     
        /// <summary>Saatlik Kullanilabilir Tip Id sorgularını getirir.</summary>
        int? GetSaatlikKullanilabilirTipId();
    }
}
