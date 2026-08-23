using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    public class RaporRepositoryCore : IRaporRepository
    {
        private static readonly ConcurrentDictionary<string, IReadOnlyList<string>> ParameterCache =
            new ConcurrentDictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);

        private readonly CeyPASSDataConnectionCore _context;

        public RaporRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        public List<RaporTanimi> RaporlariGetir()
        {
            return _context.RaporTanimlari
                .Where(r => r.AktifMi == true)
                .OrderByDescending(r => r.RaporAdi)
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

        public IReadOnlyList<string> GetProcedureParameterNames(string procedureAdi)
        {
            if (string.IsNullOrWhiteSpace(procedureAdi))
                return Array.Empty<string>();

            var key = procedureAdi.Trim();
            return ParameterCache.GetOrAdd(key, LoadProcedureParameterNames);
        }

        private IReadOnlyList<string> LoadProcedureParameterNames(string procedureAdi)
        {
            var names = new List<string>();
            var conn = _context.Database.GetDbConnection();
            var openedHere = false;
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                    openedHere = true;
                }

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT p.name
FROM sys.parameters p
WHERE p.object_id = OBJECT_ID(@proc)
  AND p.name IS NOT NULL
  AND p.name <> N''";
                    var p = cmd.CreateParameter();
                    p.ParameterName = "@proc";
                    p.Value = procedureAdi;
                    cmd.Parameters.Add(p);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            names.Add(reader.GetString(0));
                    }
                }
            }
            finally
            {
                if (openedHere && conn.State == ConnectionState.Open)
                    conn.Close();
            }

            return names;
        }

        public DataTable RaporuCalistir(string procedureAdi, Dictionary<string, object> parametreler)
        {
            var dt = new DataTable { TableName = "RaporData" };
            var allowed = GetProcedureParameterNames(procedureAdi);
            var allowedSet = new HashSet<string>(
                allowed.Select(n => n.StartsWith("@") ? n : "@" + n),
                StringComparer.OrdinalIgnoreCase);

            var conn = _context.Database.GetDbConnection();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = procedureAdi;
                cmd.CommandType = CommandType.StoredProcedure;

                if (parametreler != null)
                {
                    foreach (var param in parametreler)
                    {
                        var name = param.Key.StartsWith("@") ? param.Key : "@" + param.Key;
                        if (allowedSet.Count > 0 && !allowedSet.Contains(name))
                            continue;

                        var p = cmd.CreateParameter();
                        p.ParameterName = name;
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
