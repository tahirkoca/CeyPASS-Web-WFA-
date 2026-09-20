namespace CeyPASS.Entities.Concrete

{

    /// <summary>Son geçiş (turnike) olayı; canlı izleme kartı için.</summary>

    public sealed class LastPassDTO

    {

        public int PersonelId { get; set; }

        public string? AdSoyad { get; set; }

        public byte[]? Foto { get; set; }      

        public string? IsyeriAdi { get; set; }

        public string? Unvan { get; set; }

        public System.DateTime Zaman { get; set; }

        public bool GirisMi { get; set; }

        public string? TerminalAdi { get; set; }

    }

}

