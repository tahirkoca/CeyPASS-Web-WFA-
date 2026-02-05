namespace CeyPASS.Business.Abstractions
{
    public interface IFazlaMesaiService
    {
        int Yuvarla30(int dakika);
        int HesaplaSistemFm(int erkenDakika, int gecDakika);
    }
}
