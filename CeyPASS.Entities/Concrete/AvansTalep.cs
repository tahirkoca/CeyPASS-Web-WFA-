using System;

namespace CeyPASS.Entities.Concrete
{
    public class AvansTalep
    {
        public int AvansId { get; set; }
        public string PersonelId { get; set; } = "";
        public decimal Miktar { get; set; }
        public string? Aciklama { get; set; }
        public DateTime TalepTarihi { get; set; }
        public AvansDurumu Durum { get; set; }
        public int? OnaylayanId { get; set; }
        public DateTime? OnayTarihi { get; set; }
        public string? OnayAciklama { get; set; }
    }
}

