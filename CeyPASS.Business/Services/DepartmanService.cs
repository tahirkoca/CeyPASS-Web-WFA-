using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.Business.Services
{
    /// <summary>Departman CRUD.</summary>
    public class DepartmanService:IDepartmanService
    {
        private readonly IDepartmanRepository _repo;

        public DepartmanService(IDepartmanRepository repo)
        {
            _repo = repo;
        }
        /// <inheritdoc />
        public List<LookupItem> GetAll(int? firmId = null)
        {
            var dt = _repo.GetAll();
            var list = new List<LookupItem>();
            if (dt == null) return list;

            foreach (DataRow r in dt.Rows)
            {
                list.Add(new LookupItem
                {
                    Id = r.Field<int>("DepartmanId"),
                    Ad = r.Field<string>("DepartmanAdi") ?? ""
                });
            }
            return list;
        }
        /// <inheritdoc />
        public List<DepartmanListDTO> GetListForAdmin()
        {
            var dt = _repo.GetAll();
            var list = new List<DepartmanListDTO>();
            if (dt == null) return list;
            foreach (DataRow r in dt.Rows)
            {
                list.Add(new DepartmanListDTO
                {
                    Id = r.Field<int>("DepartmanId"),
                    Ad = r.Field<string>("DepartmanAdi") ?? "",
                    Aciklama = r.Field<string>("Aciklama") ?? ""
                });
            }
            return list;
        }
        /// <inheritdoc />
        public DataRow? GetRowById(int id)
        {
            var dt = _repo.GetAll();
            foreach (DataRow r in dt.Rows)
                if (r.Field<int>("DepartmanId") == id) return r;
            return null;
        }
        /// <inheritdoc />
        public int GetNextId() => _repo.GetNextId();
        /// <inheritdoc />
        public bool Add(int id, string ad, string aciklama) => _repo.Insert(id, ad, aciklama);
        /// <inheritdoc />
        public bool Update(int id, string ad, string aciklama) => _repo.Update(id, ad, aciklama);
        /// <inheritdoc />
        public bool Delete(int id) => _repo.Delete(id);
    }
}
