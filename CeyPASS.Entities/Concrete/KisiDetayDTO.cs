namespace CeyPASS.Entities.Concrete

{

    /// <summary>Canlı izleme / son geçiş kartında gösterilen personel özeti.</summary>

    public sealed class KisiDetayDTO

    {

        public string AdSoyad { get; set; }

        public string Unvan { get; set; }

        public string Isyeri { get; set; }

        public byte[] Foto { get; set; }

    }

}

