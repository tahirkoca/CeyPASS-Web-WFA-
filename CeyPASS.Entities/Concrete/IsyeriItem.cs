namespace CeyPASS.Entities.Concrete
{
    public class IsyeriItem
    {
        public int FirmaId { get; }
        public int IsyeriId { get; }
        public string Ad { get; }
        public IsyeriItem(int firmaId, int isyeriId, string ad)
        {
            FirmaId = firmaId; IsyeriId = isyeriId; Ad = ad ?? "";
        }
        public override string ToString()
        {
            return $"[{FirmaId}] {Ad} ({IsyeriId})";
        }
    }
}
