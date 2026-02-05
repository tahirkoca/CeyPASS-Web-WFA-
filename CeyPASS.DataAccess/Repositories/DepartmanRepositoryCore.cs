using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    public class DepartmanRepositoryCore : IDepartmanRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public DepartmanRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        public List<LookupItem> GetByFirma()
        {
            return _context.Departmanlar
                .OrderBy(d => d.DepartmanAdi)
                .Select(d => new LookupItem
                {
                    Id = d.DepartmanId,
                    Ad = d.DepartmanAdi
                })
                .ToList();
        }

        public DataTable GetAll()
        {
            var list = _context.Departmanlar
                .OrderBy(d => d.DepartmanAdi)
                .Select(d => new
                {
                    d.DepartmanId,
                    d.DepartmanAdi,
                    d.Aciklama
                })
                .ToList();

            var dt = new DataTable();
            dt.Columns.Add("DepartmanId", typeof(int));
            dt.Columns.Add("DepartmanAdi", typeof(string));
            dt.Columns.Add("Aciklama", typeof(string));

            foreach (var x in list)
            {
                dt.Rows.Add(x.DepartmanId, x.DepartmanAdi, x.Aciklama);
            }
            return dt;
        }

        public int GetNextId()
        {
            var sql = "SELECT ISNULL(MAX(DepartmanId),0)+1 FROM Departmanlar";

            return _context.Database
                .SqlQueryRaw<int>(sql)
                .AsEnumerable()
                .Single();
        }

        public bool Insert(int id, string ad, string aciklama)
        {
            var entity = new CeyPASS.DataAccess.Departmanlar
            {
                DepartmanId = id,
                DepartmanAdi = ad,
                Aciklama = aciklama
            };

            _context.Departmanlar.Add(entity);
            return _context.SaveChanges() > 0;
        }

        public bool Update(int id, string ad, string aciklama)
        {
            var entity = _context.Departmanlar
                .FirstOrDefault(d => d.DepartmanId == id);

            if (entity == null)
                return false;

            entity.DepartmanAdi = ad;
            entity.Aciklama = aciklama;

            return _context.SaveChanges() > 0;
        }

        public bool Delete(int id)
        {
            var entity = _context.Departmanlar
                .FirstOrDefault(d => d.DepartmanId == id);

            if (entity == null)
                return false;

            _context.Departmanlar.Remove(entity);
            return _context.SaveChanges() > 0;
        }
    }
}
