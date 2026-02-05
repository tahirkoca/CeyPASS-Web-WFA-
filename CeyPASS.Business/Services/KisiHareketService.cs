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
        public DataTable GetByPersons(List<int> personIds, DateTime bas, DateTime bit,bool onlyAktif, bool onlyPasif, bool onlyYemekhane, int firmaId) => _repo.GetByPersons(personIds, bas, bit, onlyAktif, onlyPasif, onlyYemekhane, firmaId);
        public bool InsertManual(int firmaId, int personelId, DateTime tarih, string tip) => _repo.InsertManual(firmaId, personelId, tarih, tip);
        public bool UpdateManual(int id, DateTime tarih, string tip) => _repo.UpdateManual(id, tarih, tip);
        public bool PasifYap(int id) => _repo.PasifYap(id);
        public DataTable GetAktifKisilerWithSicil(int firmaId)=>_repo.GetAktifKisilerWithSicil(firmaId);
    }
}
