namespace CeyPASS.DataAccess.Abstractions
{
    public interface IYemekhaneRepository
    {
        void InsertLimit(string personelId, int gunlukLimit);
        void UpsertLimit(string personelId, int gunlukLimit);
        void PasifEtByPersonel(string personelId);
        void MovePersonelId(string oldPersonelId, string newPersonelId);
    }
}
