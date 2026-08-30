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
        List<KisiHareketDTO> GetLastMovesByFirmaArac(int top, int firmaId);
        /// <param name="firmaId">Personel listesi için; personIds doluysa hareket sorgusunda FirmaId filtresi uygulanmaz (tüm firmalardaki hareketler).</param>
        DataTable GetByPersons(List<int> personIds, DateTime bas, DateTime bit, bool onlyAktif, bool onlyPasif, bool onlyYemekhane, int firmaId);
        /// <param name="firmaId">Personel listesi için; personIds doluysa hareket sorgusunda FirmaId filtresi uygulanmaz (tüm firmalardaki hareketler).</param>
        List<KisiHareketListRow> GetByPersonsPaged(List<int> personIds, DateTime bas, DateTime bit, bool onlyAktif, bool onlyPasif, bool onlyYemekhane, int firmaId, int page, int pageSize, out int totalCount);
        bool InsertManual(int firmaId, int personelId, DateTime tarih, string tip);
        bool UpdateManual(int id, DateTime tarih, string tip);
        bool PasifYap(int id);
        bool AktifYap(int id);
        DataTable GetAktifKisilerWithSicil(int firmaId, bool puantajYapilirMi = true);
    }
}
