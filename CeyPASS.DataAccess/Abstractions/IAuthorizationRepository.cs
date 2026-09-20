namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Sayfa/yetki tipi bazlı kullanıcı yetki kontrolü.</summary>
    public interface IAuthorizationRepository
    {
        /// <summary>Kullanicinin belirtilen sayfa ve yetki tipinde izni olup olmadigini kontrol eder.</summary>
        bool CheckPermission(int kullaniciId, string sayfaAdi, string yetkiTipi);
    }
}
