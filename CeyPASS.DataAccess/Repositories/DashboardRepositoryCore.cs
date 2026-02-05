using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;

namespace CeyPASS.DataAccess.Repositories
{
    public class DashboardRepositoryCore : IDashboardRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public DashboardRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        private static int GetInt32Safe(IDataRecord r, string name)
        {
            int i = r.GetOrdinal(name);
            if (r.IsDBNull(i)) return 0;
            return Convert.ToInt32(r.GetValue(i));
        }

        private static string GetStringSafe(IDataRecord r, string name)
        {
            int i = r.GetOrdinal(name);
            return r.IsDBNull(i) ? string.Empty : Convert.ToString(r.GetValue(i));
        }

        private static DateTime GetDateTimeSafe(IDataRecord r, string name)
        {
            int i = r.GetOrdinal(name);
            if (r.IsDBNull(i)) return DateTime.MinValue;
            return Convert.ToDateTime(r.GetValue(i));
        }

        public DashboardResult ExecuteDashboard(string firmaIdCsv, DateTime gun, DateTime ayBas, DateTime aySon, double tolBasSaat, double tolBitSaat, int anlikLimit)
        {
            var result = new DashboardResult();

            var conn = _context.Database.GetDbConnection();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "dbo.sp_DashboardAnaEkran";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 60;

                var p1 = cmd.CreateParameter();
                p1.ParameterName = "@FirmaIdList";
                p1.DbType = DbType.String;
                p1.Value = firmaIdCsv ?? (object)DBNull.Value;
                cmd.Parameters.Add(p1);

                var p2 = cmd.CreateParameter();
                p2.ParameterName = "@Gun";
                p2.DbType = DbType.Date;
                p2.Value = gun.Date;
                cmd.Parameters.Add(p2);

                var p3 = cmd.CreateParameter();
                p3.ParameterName = "@AyBas";
                p3.DbType = DbType.Date;
                p3.Value = ayBas.Date;
                cmd.Parameters.Add(p3);

                var p4 = cmd.CreateParameter();
                p4.ParameterName = "@AySon";
                p4.DbType = DbType.Date;
                p4.Value = aySon.Date;
                cmd.Parameters.Add(p4);

                var p5 = cmd.CreateParameter();
                p5.ParameterName = "@TolBasSaat";
                p5.DbType = DbType.Double;
                p5.Value = tolBasSaat;
                cmd.Parameters.Add(p5);

                var p6 = cmd.CreateParameter();
                p6.ParameterName = "@TolBitSaat";
                p6.DbType = DbType.Double;
                p6.Value = tolBitSaat;
                cmd.Parameters.Add(p6);

                var p7 = cmd.CreateParameter();
                p7.ParameterName = "@AnlikLimit";
                p7.DbType = DbType.Int32;
                p7.Value = anlikLimit;
                cmd.Parameters.Add(p7);

                if (conn.State != ConnectionState.Open)
                    conn.Open();

                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var dto = new GecKalanlarDashboard
                        {
                            PersonelId = GetInt32Safe(rdr, "PersonelId"),
                            Ad = GetStringSafe(rdr, "Ad"),
                            Soyad = GetStringSafe(rdr, "Soyad"),
                            FirmaId = GetInt32Safe(rdr, "FirmaId"),
                            IsyeriId = GetInt32Safe(rdr, "IsyeriId"),
                            FazlaDakika = GetInt32Safe(rdr, "FazlaDakika")
                        };
                        result.LateList.Add(dto);
                    }

                    if (rdr.NextResult())
                    {
                        while (rdr.Read())
                        {
                            var dto = new DogumGunleriDashboard
                            {
                                PersonelId = GetInt32Safe(rdr, "PersonelId"),
                                Ad = GetStringSafe(rdr, "Ad"),
                                Soyad = GetStringSafe(rdr, "Soyad"),
                                FirmaId = GetInt32Safe(rdr, "FirmaId"),
                                IsyeriId = GetInt32Safe(rdr, "IsyeriId"),
                                BuYilDogumGunu = GetDateTimeSafe(rdr, "BuYilDogumGunu"),
                                Gun = GetInt32Safe(rdr, "Gun"),
                                Ay = GetInt32Safe(rdr, "Ay"),
                                Yas = GetInt32Safe(rdr, "Yas")
                            };
                            result.Birthdays.Add(dto);
                        }
                    }

                    if (rdr.NextResult())
                    {
                        while (rdr.Read())
                        {
                            var dto = new IseBaslayanlarDashboard
                            {
                                PersonelId = GetInt32Safe(rdr, "PersonelId"),
                                Ad = GetStringSafe(rdr, "Ad"),
                                Soyad = GetStringSafe(rdr, "Soyad"),
                                FirmaId = GetInt32Safe(rdr, "FirmaId"),
                                IsyeriId = GetInt32Safe(rdr, "IsyeriId"),
                                BaslamaTarihi = GetDateTimeSafe(rdr, "BaslamaTarihi")
                            };
                            result.NewHires.Add(dto);
                        }
                    }

                    if (rdr.NextResult())
                    {
                        while (rdr.Read())
                        {
                            var dto = new IstenAyrilanlarDashboard
                            {
                                PersonelId = GetInt32Safe(rdr, "PersonelId"),
                                Ad = GetStringSafe(rdr, "Ad"),
                                Soyad = GetStringSafe(rdr, "Soyad"),
                                FirmaId = GetInt32Safe(rdr, "FirmaId"),
                                IsyeriId = GetInt32Safe(rdr, "IsyeriId"),
                                AyrilmaTarihi = GetDateTimeSafe(rdr, "AyrilmaTarihi")
                            };
                            result.Resignations.Add(dto);
                        }
                    }

                    if (rdr.NextResult() && rdr.Read())
                    {
                        result.Cards.GirisYapan = GetInt32Safe(rdr, "GirisYapan");
                        result.Cards.Iceridekiler = GetInt32Safe(rdr, "Iceridekiler");
                        result.Cards.GecKalanlar = GetInt32Safe(rdr, "GecKalanlar");
                        result.Cards.Disaridakiler = GetInt32Safe(rdr, "Disaridakiler");
                        result.Cards.Devamsizlar = GetInt32Safe(rdr, "Devamsizlar");
                        result.Cards.Izinli = GetInt32Safe(rdr, "Izinli");
                        result.Cards.IseBaslayan = GetInt32Safe(rdr, "IseBaslayan");
                        result.Cards.IstenAyrilan = GetInt32Safe(rdr, "IstenAyrilan");
                    }
                }
            }

            return result;
        }
    }
}
