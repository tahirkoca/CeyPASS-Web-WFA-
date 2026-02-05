using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface IKisiHareketRepository
    {
        List<KisiHareketDTO> GetLastMovesByFirma(int top, int firmaId);
        List<KisiHareketDTO> GetLastMovesByFirmaYemekhane(int top, int firmaId);
        DataTable GetByPersons(List<int> personIds, DateTime bas, DateTime bit, bool onlyAktif, bool onlyPasif, bool onlyYemekhane, int firmaId);
        bool InsertManual(int firmaId, int personelId, DateTime tarih, string tip);
        bool UpdateManual(int id, DateTime tarih, string tip);
        bool PasifYap(int id);
        DataTable GetAktifKisilerWithSicil(int firmaId);
    }
}
