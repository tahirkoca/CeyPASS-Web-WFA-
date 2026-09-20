using System;

namespace CeyPASS.Entities.Concrete
{
    /// <summary>Puantaj grid satırı (okuma); onay durumu int olarak gelir.</summary>
    public class PuantajGunRow
    {
        public DateTime? Tarih { get; set; }
        public string VardiyaTuru { get; set; }
        public TimeSpan? IlkGiris { get; set; }
        public TimeSpan? SonCikis { get; set; }
        public TimeSpan? VardiyaBaslangic { get; set; }
        public TimeSpan? VardiyaBitis { get; set; }
        /// <summary><see cref="OnayDurumu"/> değeri; null ise henüz işlenmemiş.</summary>
        public int? OnayDurumu { get; set; }
        /// <summary>Manuel düzeltilmiş fazla mesai (dakika).</summary>
        public int? DuzenlenmisFMDakika { get; set; }
        public string Aciklama { get; set; }
        public string CalismaTipi { get; set; }
        public decimal? Saat { get; set; }
        public int? SaatlikIzinDakika { get; set; }
    }
}
