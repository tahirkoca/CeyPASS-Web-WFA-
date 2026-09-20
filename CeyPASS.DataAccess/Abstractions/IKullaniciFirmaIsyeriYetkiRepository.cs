using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Kullanıcı firma/işyeri yetki matrisi.</summary>
    public interface IKullaniciFirmaIsyeriYetkiRepository
    {
        /// <summary>Yetkiler sorgularını getirir.</summary>
        List<FirmaIsyeriYetkiDTO> GetYetkiler(int kullaniciId);
    }
}
