namespace CeyPASS.DataAccess.Abstractions
{
    public interface IAuthorizationRepository
    {
        bool CheckPermission(int kullaniciId, string sayfaAdi, string yetkiTipi);
    }
}
