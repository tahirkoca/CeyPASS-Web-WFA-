using System.Collections.Generic;

namespace CeyPASS.Entities.Concrete
{
    /// <summary>
    /// Personel ekranı üst filtrelerinden modal'a aktarılan bağlam.
    /// </summary>
    public class KisiAraContext
    {
        public int FirmaId { get; set; }
        public string FirmaAdi { get; set; } = "";
        public int? IsyeriId { get; set; }
        public IReadOnlyList<int> IsyeriIdIn { get; set; }
        public string IsyeriAdi { get; set; } = "";
        public bool SadeceIstenCikanlar { get; set; }
        public bool? PuantajYapilirMi { get; set; }
        public string CalismaDurumuMetni { get; set; } = "";
        public string PuantajMetni { get; set; } = "";

        public KisiSearchFilter ToSearchFilter()
        {
            return new KisiSearchFilter
            {
                FirmaId = FirmaId,
                PuantajYapilirMi = PuantajYapilirMi,
                IsyeriId = IsyeriId,
                IsyeriIdIn = IsyeriIdIn,
                SadeceIstenCikanlar = SadeceIstenCikanlar
            };
        }
    }
}
