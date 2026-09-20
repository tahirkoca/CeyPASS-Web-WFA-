using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    /// <summary>Resmi tatil erişimi.</summary>
    public class ResmiTatilRepositoryCore : IResmiTatilRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public ResmiTatilRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        /// <summary>Doldur Sabit işlemini gerçekleştirir.</summary>
        public void DoldurSabit(int basYil, int bitYil)
        {
            var sql = "EXEC sp_ResmiTatiller_DoldurSabit @p0, @p1";

            _context.Database.ExecuteSqlRaw(sql,
                new Microsoft.Data.SqlClient.SqlParameter("@p0", basYil),
                new Microsoft.Data.SqlClient.SqlParameter("@p1", bitYil));
        }

        /// <summary>Ekle Veya Guncelle işlemini ekler.</summary>
        public void EkleVeyaGuncelle(DateTime tarih, string ad, decimal? calismaSaat)
        {
            var sql = "EXEC sp_ResmiTatilEkle @p0, @p1, @p2";

            var pTarih = new Microsoft.Data.SqlClient.SqlParameter("@p0", SqlDbType.DateTime)
            {
                Value = tarih.Date
            };

            var pAd = new Microsoft.Data.SqlClient.SqlParameter("@p1", ad ?? (object)DBNull.Value);

            var pSaat = new Microsoft.Data.SqlClient.SqlParameter("@p2", SqlDbType.Decimal)
            {
                Precision = 5,
                Scale = 2,
                Value = calismaSaat ?? (object)DBNull.Value
            };

            _context.Database.ExecuteSqlRaw(sql, pTarih, pAd, pSaat);
        }

        /// <summary>List sorgularını getirir.</summary>
        public List<ResmiTatilDTO> GetList(int? yil = null)
        {
            var query = _context.ResmiTatiller.AsNoTracking().AsQueryable();

            if (yil.HasValue)
            {
                int y = yil.Value;
                query = query.Where(x => x.Tarih.Year == y);
            }

            return query
                .OrderBy(x => x.Tarih)
                .Select(x => new ResmiTatilDTO
                {
                    Tarih = x.Tarih,
                    Ad = x.Ad ?? string.Empty,
                    CalismaSaati = x.CalismaSaati
                })
                .ToList();
        }
    }
}
