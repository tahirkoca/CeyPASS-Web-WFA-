using System;

namespace CeyPASS.Entities.Concrete
{
    /// <summary>Personel kısa liste öğesi (combo/autocomplete).</summary>
    public class KisiListItem
    {
        public string PersonelId { get; set; } = "";
        public string AdSoyad { get; set; } = "";
        public DateTime? IstenCikisTarihi { get; set; }
        public override string ToString() => AdSoyad;
    }
}
