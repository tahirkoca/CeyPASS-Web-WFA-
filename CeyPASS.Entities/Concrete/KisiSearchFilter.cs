using System.Collections.Generic;

namespace CeyPASS.Entities.Concrete
{
    /// <summary>
    /// Kişi Ara modal — alan bazlı arama + ana ekran bağlam filtreleri.
    /// </summary>
    public class KisiSearchFilter
    {
        public int FirmaId { get; set; }
        public bool? PuantajYapilirMi { get; set; }
        public int? IsyeriId { get; set; }
        public IReadOnlyList<int> IsyeriIdIn { get; set; }
        public bool SadeceIstenCikanlar { get; set; }

        public string AdSoyadKart { get; set; }
        public string Sicil { get; set; }
        public string TcKimlikNo { get; set; }
        public string Email { get; set; }
        public int? DepartmanId { get; set; }
        public int? PozisyonId { get; set; }
        public int? CalismaStatuId { get; set; }
    }
}
