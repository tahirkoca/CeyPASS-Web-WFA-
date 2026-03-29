using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.Web.Models.Profil
{
    public class KisiIzinGecmisRow
    {
        public int KisiIzinId { get; set; }
        public DateTime Baslangic { get; set; }
        public DateTime Bitis { get; set; }
        public decimal SureSaat { get; set; }
        public string? Aciklama { get; set; }
        public DateTime? IslenmeTarihi { get; set; }
        public bool SaatlikIzinMi { get; set; }
    }

    public class IzinlerimViewModel
    {
        public List<IzinTalep> Talepler { get; set; } = new List<IzinTalep>();
        public List<KisiIzinGecmisRow> OnayliIzinler { get; set; } = new List<KisiIzinGecmisRow>();

        public int TaleplerPage { get; set; } = 1;
        public int TaleplerPageSize { get; set; } = 3;
        public int TaleplerTotalCount { get; set; }
        public int TaleplerTotalPages { get; set; } = 1;

        public int GecmisPage { get; set; } = 1;
        public int GecmisPageSize { get; set; } = 3;
        public int GecmisTotalCount { get; set; }
        public int GecmisTotalPages { get; set; } = 1;
    }
}

