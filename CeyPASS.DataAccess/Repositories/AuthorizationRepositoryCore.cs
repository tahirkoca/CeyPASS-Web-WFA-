using CeyPASS.DataAccess.Abstractions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    public class AuthorizationRepositoryCore : IAuthorizationRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public AuthorizationRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        public bool CheckPermission(int kullaniciId, string sayfaAdi, string yetkiTipi)
        {
            var sql = @"
EXEC dbo.sp_OnayKontrolMekanizmasi 
    @KullaniciId = @p0, 
    @SayfaAdi = @p1, 
    @YetkiTipi = @p2";

            var result = _context.Database
                .SqlQueryRaw<bool?>(sql,
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", kullaniciId),
                    new Microsoft.Data.SqlClient.SqlParameter("@p1", sayfaAdi ?? (object)DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@p2", yetkiTipi ?? (object)DBNull.Value))
                .AsEnumerable()
                .FirstOrDefault();

            return result == true;
        }
    }
}
