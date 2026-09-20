namespace CeyPASS.Web.Models.CanliIzleme
{
    /// <summary>Canli izleme giris form modeli.</summary>
    public class CanliIzlemeLoginModel
    {
        public int FirmaId { get; set; }
        public string KullaniciAdi { get; set; }
        public string Sifre { get; set; }
    }
}

