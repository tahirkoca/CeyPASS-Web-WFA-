namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Üst yetkili (hiyerarşi) tanımları.</summary>
    public interface IUstYetkiliRepository
    {
        /// <summary>Ust Yetkili sorgularını getirir.</summary>
        string? GetUstYetkili(string personelId);
        System.Collections.Generic.List<CeyPASS.Entities.Concrete.UstYetkili> GetAll();
        /// <summary>Ekle Veya Guncelle işlemini gerçekleştirir.</summary>
        bool EkleVeyaGuncelle(string personelId, string ustYetkiliPersonelId);
        /// <summary>Sil işlemini gerçekleştirir.</summary>
        bool Sil(string personelId);
        System.Collections.Generic.List<string> GetSubordinates(string ustYetkiliPersonelId);
        /// <summary>Any Subordinates işlemini gerçekleştirir.</summary>
        bool AnySubordinates(string ustYetkiliPersonelId);
    }
}

