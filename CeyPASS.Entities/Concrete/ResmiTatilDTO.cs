using System;

namespace CeyPASS.Entities.Concrete
{
    public class ResmiTatilDTO
    {
        public DateTime Tarih { get; set; }
        public string Ad { get; set; }
        public decimal? CalismaSaati { get; set; }
        public string ListeMetni => $"{Tarih:dd.MM.yyyy ddd} - {Ad} ({CalismaSaati?.ToString("0.##") ?? "-"} saat)";
    }
}
