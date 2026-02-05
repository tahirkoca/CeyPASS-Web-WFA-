namespace CeyPASS.Entities.Concrete
{
    public class Cihaz
    {
        public int CihazId { get; set; }          
        public int FirmaId { get; set; }
        public string CihazAdi { get; set; }
        public string IPAdres { get; set; }
        public int Port { get; set; } = 4370;
        public string Notlar { get; set; }       
        public int CihazTipi { get; set; }        
        public bool AktifMi { get; set; } = true;
        public bool BaglandiMi { get; set; }     
    }
}
