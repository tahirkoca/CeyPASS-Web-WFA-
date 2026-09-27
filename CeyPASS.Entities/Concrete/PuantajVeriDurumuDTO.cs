namespace CeyPASS.Entities.Concrete
{
    /// <summary>Aylık puantaj ekranı firma/işyeri veri durumu özeti.</summary>
    public sealed class PuantajVeriDurumuDTO
    {
        public int ToplamPersonel { get; set; }
        public int ToplamBeklenen { get; set; }
        public int ToplamGirilen { get; set; }
        public int ToplamEksik { get; set; }

        public int? SecilenPersonelId { get; set; }
        public string? SecilenAdSoyad { get; set; }
        public int? KisiBeklenen { get; set; }
        public int? KisiGirilen { get; set; }
        public int? KisiEksik { get; set; }

        /// <summary>Ayda aktif personel yoksa bilgilendirme metni.</summary>
        public string? Mesaj { get; set; }
    }
}
