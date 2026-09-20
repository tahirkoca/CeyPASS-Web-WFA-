namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Yemekhane geçiş, limit ve engelleme sorguları.</summary>
    public interface IYemekhaneRepository
    {
        /// <summary>Insert Limit işlemini gerçekleştirir.</summary>
        void InsertLimit(string personelId, int gunlukLimit);
        /// <summary>Upsert Limit işlemini gerçekleştirir.</summary>
        void UpsertLimit(string personelId, int gunlukLimit);
        /// <summary>Pasif Et By Personel işlemini gerçekleştirir.</summary>
        void PasifEtByPersonel(string personelId);
        /// <summary>Move Personel Id işlemini gerçekleştirir.</summary>
        void MovePersonelId(string oldPersonelId, string newPersonelId);
        /// <summary>Son Gunluk Limit sorgularını getirir.</summary>
        int? GetSonGunlukLimit(string personelId);
    }
}
