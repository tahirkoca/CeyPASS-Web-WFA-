using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.Business.Services
{
    public class KisiHareketService : IKisiHareketService
    {
        public IKisiHareketRepository _repo;

        public KisiHareketService(IKisiHareketRepository repo)
        {
            _repo = repo;
        }
        public List<KisiHareketDTO> GetLastMovesByFirma(int top, int firmaId)
        {
            return _repo.GetLastMovesByFirma(top, firmaId);
        }
        public List<KisiHareketDTO> GetLastMovesByFirmaYemekhane(int top, int firmaId)
        {
            return _repo.GetLastMovesByFirmaYemekhane(top, firmaId);
        }
        public List<KisiHareketDTO> GetLastMovesByFirmaArac(int top, int firmaId)
        {
            return _repo.GetLastMovesByFirmaArac(top, firmaId);
        }
        public DataTable GetByPersons(List<int> personIds, DateTime bas, DateTime bit,bool onlyAktif, bool onlyPasif, bool onlyYemekhane, int firmaId) => _repo.GetByPersons(personIds, bas, bit, onlyAktif, onlyPasif, onlyYemekhane, firmaId);
        public List<KisiHareketListRow> GetByPersonsPaged(List<int> personIds, DateTime bas, DateTime bit, bool onlyAktif, bool onlyPasif, bool onlyYemekhane, int firmaId, int page, int pageSize, out int totalCount)
            => _repo.GetByPersonsPaged(personIds, bas, bit, onlyAktif, onlyPasif, onlyYemekhane, firmaId, page, pageSize, out totalCount);
        public bool InsertManual(int firmaId, int personelId, DateTime tarih, string tip) => _repo.InsertManual(firmaId, personelId, tarih, tip);
        public bool UpdateManual(int id, DateTime tarih, string tip) => _repo.UpdateManual(id, tarih, tip);
        public bool PasifYap(int id) => _repo.PasifYap(id);
        public bool AktifYap(int id) => _repo.AktifYap(id);
        public DataTable GetAktifKisilerWithSicil(int firmaId, bool puantajYapilirMi = true) => _repo.GetAktifKisilerWithSicil(firmaId, puantajYapilirMi);
    }
}
