using System;

namespace CeyPASS.Entities.Concrete
{
    public class IzinKayitValidasyonDTO
    {
        public bool SaatlikIzinMi { get; set; }
        /// <summary>Yıllık izin yarım gün (08:30-12:30 / 14:00-18:00, 225 dk).</summary>
        public bool YarimGunYillikIzinMi { get; set; }
        public string PersonelId { get; set; }
        public int? IzinTipId { get; set; }
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }
        public TimeSpan? BaslangicSaati { get; set; }
        public TimeSpan? BitisSaati { get; set; }
    }
}
