namespace CeyPASS.Business.Abstractions
{
    public interface IAuthorizationService
    {
        bool Can(string sayfaAdi, string yetkiTipi);
        bool ViewAbility(string page);
        bool CreateAbility(string page);
        bool UpdateAbility(string page);
        bool DeleteAbility(string page);
        bool ExportAbility(string page);
        bool ApproveAbility(string page);
    }
}
