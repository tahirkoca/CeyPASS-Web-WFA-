using System;

namespace CeyPASS.Entities.Concrete
{
    /// <summary>Bir günün aktif ilk Giriş ve son Çıkış hareket uçları.</summary>
    public class PuantajGunHareketUctanUcaDTO
    {
        public int? GirisHareketId { get; set; }
        public int? GirisCihazId { get; set; }
        public DateTime? GirisTarih { get; set; }

        public int? CikisHareketId { get; set; }
        public int? CikisCihazId { get; set; }
        public DateTime? CikisTarih { get; set; }
    }

    /// <summary>Puantaj gün düzenlemede giriş veya çıkış uç bilgisi (null = taraf kapalı).</summary>
    public class PuantajGunHareketUcu
    {
        public int? HareketId { get; set; }
        public int CihazId { get; set; }
        public DateTime TarihSaat { get; set; }
    }
}
