using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    public interface IKullaniciFirmaIsyeriYetkiService
    {
        List<FirmaIsyeriYetkiDTO> GetYetkiler(int kullaniciId);

        /// <summary>Rapor SP @IsyeriIdList için yetkili işyeri CSV (0 dahil).</summary>
        string BuildIsyeriIdListCsv(int firmaId, List<FirmaIsyeriYetkiDTO> yetkiler, bool isAdmin, IReadOnlyList<int> firmaIsyeriIds);
    }
}
