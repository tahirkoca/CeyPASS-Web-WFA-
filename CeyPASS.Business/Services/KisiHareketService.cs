using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.Business.Services
{
    /// <summary>Geçiş hareketi sorgu ve manuel kayıt.</summary>
    public class KisiHareketService : IKisiHareketService
    {
        public IKisiHareketRepository _repo;

        public KisiHareketService(IKisiHareketRepository repo)
        {
            _repo = repo;
        }
        /// <inheritdoc />
        public List<KisiHareketDTO> GetLastMovesByFirma(int top, int firmaId)
        {
            return _repo.GetLastMovesByFirma(top, firmaId);
        }
        /// <inheritdoc />
        public List<KisiHareketDTO> GetLastMovesByFirmaYemekhane(int top, int firmaId)
        {
            return _repo.GetLastMovesByFirmaYemekhane(top, firmaId);
        }
        /// <inheritdoc />
        public List<KisiHareketDTO> GetLastMovesByFirmaArac(int top, int firmaId)
        {
            return _repo.GetLastMovesByFirmaArac(top, firmaId);
        }
        /// <inheritdoc />
        public DataTable GetByPersons(List<int> personIds, DateTime bas, DateTime bit,bool onlyAktif, bool onlyPasif, bool onlyYemekhane, int firmaId) => _repo.GetByPersons(personIds, bas, bit, onlyAktif, onlyPasif, onlyYemekhane, firmaId);
        /// <inheritdoc />
        public List<KisiHareketListRow> GetByPersonsPaged(List<int> personIds, DateTime bas, DateTime bit, bool onlyAktif, bool onlyPasif, bool onlyYemekhane, int firmaId, int page, int pageSize, out int totalCount)
            => _repo.GetByPersonsPaged(personIds, bas, bit, onlyAktif, onlyPasif, onlyYemekhane, firmaId, page, pageSize, out totalCount);
        /// <inheritdoc />
        public bool InsertManual(int firmaId, int personelId, DateTime tarih, string tip) => _repo.InsertManual(firmaId, personelId, tarih, tip);
        /// <inheritdoc />
        public bool UpdateManual(int id, DateTime tarih, string tip) => _repo.UpdateManual(id, tarih, tip);
        /// <inheritdoc />
        public bool PasifYap(int id) => _repo.PasifYap(id);
        /// <inheritdoc />
        public bool AktifYap(int id) => _repo.AktifYap(id);
        /// <inheritdoc />
        public DataTable GetAktifKisilerWithSicil(int firmaId, bool puantajYapilirMi = true) => _repo.GetAktifKisilerWithSicil(firmaId, puantajYapilirMi);
    }
}
