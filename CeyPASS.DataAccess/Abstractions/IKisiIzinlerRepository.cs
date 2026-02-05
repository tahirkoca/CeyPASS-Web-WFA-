using CeyPASS.Entities.Concrete;
using System;
using System.Data;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface IKisiIzinlerRepository
    {
        DataTable GetIzinleri(string personelId, DateTime baslangic, DateTime bitis);
        KisiIzin GetById(int kisiIzinId);
        bool Insert(KisiIzin x);
        bool Update(KisiIzin x);
        DataTable GetIzinRaporu(int? firmaId, string personelId, int? izinTipId, DateTime bas, DateTime bit);
        bool PasifYap(int kisiIzinId);
        DataTable GetByPerson(string personelId, DateTime? bas = null, DateTime? bit = null);
    }
}
