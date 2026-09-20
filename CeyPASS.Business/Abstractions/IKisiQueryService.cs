using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Personel listeleme, arama ve detay sorguları.</summary>
    public interface IKisiQueryService
    {
        /// <summary>Firmadaki aktif personel listesi; isteğe bağlı bölüm filtresi.</summary>
        List<KisiListItem> GetAktifKisilerByFirma(int firmId, string? search = null, bool? puantajYapilirMi = true, int? isyeriId = null, IReadOnlyList<int>? isyeriIdIn = null, bool sadeceIstenCikanlar = false, int? bolumId = null);

        /// <summary>Sayfalanmış aktif personel listesi; isteğe bağlı bölüm filtresi.</summary>
        List<KisiListItem> GetAktifKisilerByFirmaPaged(int firmId, string? search, bool? puantajYapilirMi, int? isyeriId, IReadOnlyList<int>? isyeriIdIn, bool sadeceIstenCikanlar, int page, int pageSize, out int totalCount, int? bolumId = null);

        /// <summary>Gelişmiş filtre ile personel arama.</summary>
        List<KisiSearchResultItem> SearchKisilerPaged(KisiSearchFilter filter, int page, int pageSize, out int totalCount);

        /// <summary>Personel detay bilgisi.</summary>
        KisiDetay GetKisiDetay(string personelId);

        /// <summary>Personel veya puantajsız kart detayı.</summary>
        (KisiDetay? detay, bool isPuantajsizKart) GetDetayOrPuantajsizKart(string id);
    }
}
