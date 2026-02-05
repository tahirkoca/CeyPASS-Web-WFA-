namespace CeyPASS.Entities.Concrete
{
    public class PuantajTipDTO
    {
        public string Kod { get; set; } = "";
        public string Ad { get; set; } = "";
        public decimal? VarsayilanSaat { get; set; }
        public string AdKod => $"{Ad} ({Kod})";
    }
}
