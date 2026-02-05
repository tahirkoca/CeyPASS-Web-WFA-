using System;

namespace CeyPASS.Entities.Concrete
{
    public sealed class KisiHareketDTO
    {
        public DateTime Tarih { get; set; }
        public string? AdSoyad { get; set; }
        public string? Departman { get; set; }
        public string? Unvan { get; set; }
        public string? CihazAdi { get; set; }
        public int PersonelId { get; set; }
    }
}
