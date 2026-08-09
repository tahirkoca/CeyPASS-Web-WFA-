using System;

namespace CeyPASS.Entities.Concrete
{
    public class GecmisZiyaretciItem
    {
        public string AdSoyad { get; set; } = "";
        public string TCKimlikNo { get; set; }
        public string ZiyaretEdilenKisi { get; set; }
        public string Plaka { get; set; }
        public DateTime SonZiyaret { get; set; }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Plaka))
                return AdSoyad;
            return $"{AdSoyad} ({Plaka.Trim()})";
        }
    }
}
