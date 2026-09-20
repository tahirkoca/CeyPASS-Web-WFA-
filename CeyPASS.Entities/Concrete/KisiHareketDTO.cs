using System;



namespace CeyPASS.Entities.Concrete

{

    /// <summary>Personel hareket geçmişi özet satırı (API/UI).</summary>

    public sealed class KisiHareketDTO

    {

        public DateTime Tarih { get; set; }

        public string? AdSoyad { get; set; }

        public string? Isyeri { get; set; }

        public string? Unvan { get; set; }

        public string? CihazAdi { get; set; }

        public int PersonelId { get; set; }

    }

}

