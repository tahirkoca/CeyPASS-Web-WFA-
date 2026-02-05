using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    public interface IKisiQueryService
    {
        List<KisiListItem> GetAktifKisilerByFirma(int firmId, string? search = null, bool? puantajYapilirMi = true, int? isyeriId = null);
        KisiDetay GetKisiDetay(string personelId);
        (KisiDetay? detay, bool isPuantajsizKart) GetDetayOrPuantajsizKart(string id);
    }
}
