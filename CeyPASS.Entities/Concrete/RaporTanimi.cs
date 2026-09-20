namespace CeyPASS.Entities.Concrete
{
    /// <summary>Dinamik rapor kataloğu; çalıştırılacak stored procedure adı.</summary>
    public class RaporTanimi
    {
        public int Id { get; set; }
        public string RaporAdi { get; set; }
        public string ProcedureAdi { get; set; }
        public string Aciklama { get; set; }
        public bool AktifMi { get; set; }
    }

}
