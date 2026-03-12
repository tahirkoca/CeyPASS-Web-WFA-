using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    public interface IKisiQueryService
    {
        List<KisiListItem> GetAktifKisilerByFirma(int firmId, string? search = null, bool? puantajYapilirMi = true, int? isyeriId = null);
        List<KisiListItem> GetAktifKisilerByFirmaPaged(int firmId, string? search, bool? puantajYapilirMi, int? isyeriId, int page, int pageSize, out int totalCount);
        KisiDetay GetKisiDetay(string personelId);
        (KisiDetay? detay, bool isPuantajsizKart) GetDetayOrPuantajsizKart(string id);
    }
}
