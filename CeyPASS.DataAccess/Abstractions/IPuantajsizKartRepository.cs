using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface IPuantajsizKartRepository
    {
        PuantajsizKart GetByKartId(string kartId);
        List<PuantajsizKart> GetByFirmaOrderByName(int firmaId);
        void Insert(string kartId, string kartNo, string kartAdi, int firmaId, string calismaSekliCsv, bool ziyaretciMi = true, bool aracKartMi = false, bool taseronCalisanMi = false);
        bool Exists(string kartId);
        void UpsertByKartNo(string kartNo, int firmaId, string kartAdi, string calismaSekliCsv);
        void PasifEtByKartNo(string kartNo);
        void PasifEtByKartId(string kartId);
        List<KartItem> GetAktifKartlarForSync();
        List<int> GetAktifKartIdler();
        void MoveKartId(string oldKartId, string newKartId);
        void UpdateByKartId(string kartId, string kartAdi, string kartNo, string calismaSekli);
    }
}
