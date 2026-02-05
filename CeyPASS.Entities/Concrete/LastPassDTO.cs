namespace CeyPASS.Entities.Concrete
{
    public sealed class LastPassDTO
    {
        public int PersonelId { get; set; }
        public string? AdSoyad { get; set; }
        public byte[]? Foto { get; set; }      
        public string? DepartmanAdi { get; set; }
        public string? Unvan { get; set; }
        public System.DateTime Zaman { get; set; }
        public bool GirisMi { get; set; }
        public string? TerminalAdi { get; set; }
    }
}
