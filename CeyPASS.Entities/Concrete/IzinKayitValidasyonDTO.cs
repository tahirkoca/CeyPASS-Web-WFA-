using System;

namespace CeyPASS.Entities.Concrete
{
    public class IzinKayitValidasyonDTO
    {
        public bool SaatlikIzinMi { get; set; }
        public string PersonelId { get; set; }
        public int? IzinTipId { get; set; }
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }
        public TimeSpan? BaslangicSaati { get; set; }
        public TimeSpan? BitisSaati { get; set; }
    }
}
