using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface IKullaniciFirmaIsyeriYetkiRepository
    {
        List<FirmaIsyeriYetkiDTO> GetYetkiler(int kullaniciId);
    }
}
