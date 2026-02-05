using System;

namespace CeyPASS.Entities.Concrete
{
    public class CalismaSekli
    {
        public int Id { get; set; }
        public int FirmaId { get; set; }
        public string Ad { get; set; }
        public TimeSpan Baslangic { get; set; }
        public TimeSpan Bitis { get; set; }
        public TimeSpan BaslangicTolerans { get; set; }
        public TimeSpan BitisTolerans { get; set; }
        public TimeSpan YemekAktiflestirme { get; set; }
        public override string ToString() => Ad;
    }

}
