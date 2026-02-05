using CeyPASS.Entities.Concrete;

namespace CeyPASS.Web.Models
{
    public class FirmaSelectViewModel
    {
        public List<Firma>? Firmalar { get; set; }
        public int SelectedFirmaId { get; set; }
        public bool ShowCombo { get; set; }
    }
}
