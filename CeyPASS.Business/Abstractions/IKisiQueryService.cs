using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    public interface IKisiQueryService
    {
        List<KisiListItem> GetAktifKisilerByFirma(int firmId, string? search = null, bool? puantajYapilirMi = true, int? isyeriId = null, IReadOnlyList<int>? isyeriIdIn = null, bool sadeceIstenCikanlar = false);
        List<KisiListItem> GetAktifKisilerByFirmaPaged(int firmId, string? search, bool? puantajYapilirMi, int? isyeriId, IReadOnlyList<int>? isyeriIdIn, bool sadeceIstenCikanlar, int page, int pageSize, out int totalCount);
        List<KisiSearchResultItem> SearchKisilerPaged(KisiSearchFilter filter, int page, int pageSize, out int totalCount);
        KisiDetay GetKisiDetay(string personelId);
        (KisiDetay? detay, bool isPuantajsizKart) GetDetayOrPuantajsizKart(string id);
    }
}
