namespace CeyPASS.DataAccess.Abstractions
{
    public interface IUstYetkiliRepository
    {
        string? GetUstYetkili(string personelId);
        System.Collections.Generic.List<CeyPASS.Entities.Concrete.UstYetkili> GetAll();
        bool EkleVeyaGuncelle(string personelId, string ustYetkiliPersonelId);
        bool Sil(string personelId);
        System.Collections.Generic.List<string> GetSubordinates(string ustYetkiliPersonelId);
        bool AnySubordinates(string ustYetkiliPersonelId);
    }
}

