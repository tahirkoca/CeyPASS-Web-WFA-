using System;

namespace CeyPASS.Entities.Concrete
{
    /// <summary>Onaylanmış veya manuel girilmiş personel izin kaydı.</summary>
    public class KisiIzin
    {
        public int? KisiIzinId { get; set; }          
        public int FirmaId { get; set; }
        public string PersonelId { get; set; }        
        public int IzinId { get; set; }               
        public DateTime Baslangic { get; set; }
        public DateTime Bitis { get; set; }
        /// <summary>Toplam izin süresi dakika; saatlik izinde anlamlı.</summary>
        public int SureDakika { get; set; }           
        public string Aciklama { get; set; }
        public bool SaatlikIzinMi { get; set; }
        public int OlusturanKullaniciId { get; set; }
    }
}
