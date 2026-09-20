using CeyPASS.Entities.Concrete;

namespace CeyPASS.Web.Models
{
    /// <summary>Firma secim dropdown modeli.</summary>
    public class FirmaSelectViewModel
    {
        public List<Firma>? Firmalar { get; set; }
        public int SelectedFirmaId { get; set; }
        public bool ShowCombo { get; set; }
    }
}
