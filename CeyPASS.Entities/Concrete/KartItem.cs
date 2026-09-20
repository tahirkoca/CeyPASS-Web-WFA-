namespace CeyPASS.Entities.Concrete
{
    /// <summary>Fiziksel kart lookup (cihaz kart id + görünen ad).</summary>
    public class KartItem
    {
        public string KartId { get; set; }
        public string KartNo { get; set; }
        public string KartAdi { get; set; }
    }
}
