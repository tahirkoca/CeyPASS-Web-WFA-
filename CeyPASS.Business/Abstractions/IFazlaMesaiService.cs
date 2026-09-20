namespace CeyPASS.Business.Abstractions
{
    /// <summary>Erken giriş/geç çıkıştan sistem fazla mesai dakikası hesabı.</summary>
    public interface IFazlaMesaiService
    {
        /// <summary>Dakikayı en yakın 30 dakikaya yuvarlar.</summary>
        int Yuvarla30(int dakika);

        /// <summary>Yuvarlanmış erken + geç dakikadan toplam sistem FM.</summary>
        int HesaplaSistemFm(int erkenDakika, int gecDakika);
    }
}
