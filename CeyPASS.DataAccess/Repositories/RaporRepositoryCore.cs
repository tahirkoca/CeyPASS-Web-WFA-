using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    public class RaporRepositoryCore : IRaporRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public RaporRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        public List<RaporTanimi> RaporlariGetir()
        {
            return _context.RaporTanimlari
                .Where(r => r.AktifMi == true)
                .Select(r => new RaporTanimi
                {
                    Id = r.Id,
                    RaporAdi = r.RaporAdi,
                    ProcedureAdi = r.ProcedureAdi,
                    Aciklama = r.Aciklama,
                    AktifMi = r.AktifMi ?? false
                })
                .ToList();
        }

        public DataTable RaporuCalistir(string procedureAdi, Dictionary<string, object> parametreler)
        {
            var dt = new DataTable { TableName = "RaporData" };

            var conn = _context.Database.GetDbConnection();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = procedureAdi;
                cmd.CommandType = CommandType.StoredProcedure;

                if (parametreler != null)
                {
                    foreach (var param in parametreler)
                    {
                        var p = cmd.CreateParameter();
                        p.ParameterName = param.Key;
                        p.Value = param.Value ?? DBNull.Value;
                        cmd.Parameters.Add(p);
                    }
                }

                if (conn.State != ConnectionState.Open)
                    conn.Open();

                using (var da = new Microsoft.Data.SqlClient.SqlDataAdapter((Microsoft.Data.SqlClient.SqlCommand)cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }
    }
}
