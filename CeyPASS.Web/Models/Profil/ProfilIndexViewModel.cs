using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Web.Models.Profil
{
    public class ProfilIndexViewModel
    {
        public string SicilNo { get; set; } = "";
        public bool HasUstYetkiliPanel { get; set; }
        public KisiDetay? Kisi { get; set; }
        public string? FotografDataUrl { get; set; }

        public string? DepartmanAdi { get; set; }
        public string? PozisyonAdi { get; set; }
        public bool? YemekHakkiVar { get; set; }
        public int? GunlukYemekAdedi { get; set; }

        public List<KisiHareketListRow> GirisCikisHareketleri { get; set; } = new List<KisiHareketListRow>();

        public int HareketPage { get; set; } = 1;
        public int HareketPageSize { get; set; } = 20;
        public int HareketTotalCount { get; set; }
        public int HareketTotalPages { get; set; } = 1;
    }
}

