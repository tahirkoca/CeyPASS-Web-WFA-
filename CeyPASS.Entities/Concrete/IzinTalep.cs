using System;

namespace CeyPASS.Entities.Concrete
{
    public class IzinTalep
    {
        public int TalepId { get; set; }
        public string PersonelId { get; set; } = "";
        public int FirmaId { get; set; }
        public int? IzinTipId { get; set; }
        public DateTime Baslangic { get; set; }
        public DateTime Bitis { get; set; }
        public bool SaatlikIzinMi { get; set; }
        public string? Aciklama { get; set; }
        public string? IzinAdres { get; set; }
        public string? TelefonNo { get; set; }
        public DateTime TalepTarihi { get; set; }

        public int? TalepImzaKullaniciId { get; set; }
        public DateTime? TalepImzaTarihi { get; set; }

        public string? UstYetkiliPersonelId { get; set; }
        public IzinOnayDurumu? UstYetkiliOnayDurumu { get; set; }
        public DateTime? UstYetkiliOnayTarihi { get; set; }
        public string? UstYetkiliAciklama { get; set; }

        public IzinOnayDurumu? IkOnayDurumu { get; set; }
        public int? IkOnaylayanKullaniciId { get; set; }
        public DateTime? IkOnayTarihi { get; set; }
        public string? IkAciklama { get; set; }

        public int? SonucKisiIzinId { get; set; }

        public bool KullanimImzaIstenen { get; set; }
        public DateTime? KullanimImzaIstenmeTarihi { get; set; }
        public int? KullanimImzaIstenmeKullaniciId { get; set; }
        public int? KullanimImzaKullaniciId { get; set; }
        public DateTime? KullanimImzaTarihi { get; set; }
    }
}

