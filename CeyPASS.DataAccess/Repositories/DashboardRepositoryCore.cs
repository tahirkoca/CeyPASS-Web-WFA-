using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Globalization;

namespace CeyPASS.DataAccess.Repositories
{
    public class DashboardRepositoryCore : IDashboardRepository
    {
        private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

        private readonly CeyPASSDataConnectionCore _context;

        public DashboardRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        private static int FindOrdinal(IDataRecord r, params string[] names)
        {
            for (int i = 0; i < r.FieldCount; i++)
            {
                var col = r.GetName(i);
                foreach (var name in names)
                {
                    if (string.Equals(col, name, StringComparison.OrdinalIgnoreCase))
                        return i;
                }
            }
            return -1;
        }

        private static int GetInt32Safe(IDataRecord r, params string[] names)
        {
            int i = FindOrdinal(r, names);
            if (i < 0 || r.IsDBNull(i)) return 0;
            return Convert.ToInt32(r.GetValue(i), CultureInfo.InvariantCulture);
        }

        private static string GetStringSafe(IDataRecord r, params string[] names)
        {
            int i = FindOrdinal(r, names);
            if (i < 0 || r.IsDBNull(i)) return string.Empty;
            return Convert.ToString(r.GetValue(i), Tr) ?? string.Empty;
        }

        private static DateTime GetDateTimeSafe(IDataRecord r, params string[] names)
        {
            int i = FindOrdinal(r, names);
            if (i < 0 || r.IsDBNull(i)) return DateTime.MinValue;

            var val = r.GetValue(i);
            if (val is DateTime dt) return dt;
            if (val is DateTimeOffset dto) return dto.DateTime;

            var s = Convert.ToString(val, Tr)?.Trim();
            if (string.IsNullOrEmpty(s)) return DateTime.MinValue;

            if (DateTime.TryParseExact(s, new[] { "dd.MM.yyyy", "d.M.yyyy", "yyyy-MM-dd", "dd/MM/yyyy" },
                    Tr, DateTimeStyles.None, out var parsedExact))
                return parsedExact;

            if (DateTime.TryParse(s, Tr, DateTimeStyles.None, out var parsed))
                return parsed;

            return DateTime.MinValue;
        }

        private static void SplitAdSoyad(string full, out string ad, out string soyad)
        {
            ad = full?.Trim() ?? string.Empty;
            soyad = string.Empty;
            if (string.IsNullOrWhiteSpace(ad)) return;

            var idx = ad.LastIndexOf(' ');
            if (idx <= 0) return;

            soyad = ad[(idx + 1)..].Trim();
            ad = ad[..idx].Trim();
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

                void AddParam(string name, DbType type, object value)
                {
                    var p = cmd.CreateParameter();
                    p.ParameterName = name;
                    p.DbType = type;
                    p.Value = value ?? DBNull.Value;
                    cmd.Parameters.Add(p);
                }

                AddParam("@FirmaIdList", DbType.String, firmaIdCsv ?? (object)DBNull.Value);
                AddParam("@IsyeriIdList", DbType.String, DBNull.Value);
                AddParam("@Gun", DbType.Date, gun.Date);
                AddParam("@AyBas", DbType.Date, ayBas.Date);
                AddParam("@AySon", DbType.Date, aySon.Date);
                AddParam("@TolBasSaat", DbType.Double, tolBasSaat);
                AddParam("@TolBitSaat", DbType.Double, tolBitSaat);
                AddParam("@AnlikLimit", DbType.Int32, anlikLimit);

                if (conn.State != ConnectionState.Open)
                    conn.Open();

                using (var rdr = cmd.ExecuteReader())
                {
                    // RS1: Geç kalanlar
                    while (rdr.Read())
                    {
                        result.LateList.Add(new GecKalanlarDashboard
                        {
                            PersonelId = GetInt32Safe(rdr, "PersonelId", "Sicil No"),
                            Ad = GetStringSafe(rdr, "Ad", "Adı"),
                            Soyad = GetStringSafe(rdr, "Soyad", "Soyadı"),
                            FirmaId = GetInt32Safe(rdr, "FirmaId"),
                            IsyeriId = GetInt32Safe(rdr, "IsyeriId"),
                            FirmaAdi = GetStringSafe(rdr, "FirmaAdi", "Firma Adı"),
                            IsyeriAdi = GetStringSafe(rdr, "IsyeriAdi", "İşyeri Adı"),
                            FazlaDakika = GetInt32Safe(rdr, "FazlaDakika", "Geç Kalınan Dakika")
                        });
                    }

                    // RS2: Doğum günleri
                    if (rdr.NextResult())
                    {
                        while (rdr.Read())
                        {
                            result.Birthdays.Add(new DogumGunleriDashboard
                            {
                                PersonelId = GetInt32Safe(rdr, "PersonelId", "Sicil No"),
                                Ad = GetStringSafe(rdr, "Ad", "Adı"),
                                Soyad = GetStringSafe(rdr, "Soyad", "Soyadı"),
                                FirmaId = GetInt32Safe(rdr, "FirmaId"),
                                IsyeriId = GetInt32Safe(rdr, "IsyeriId"),
                                FirmaAdi = GetStringSafe(rdr, "FirmaAdi", "Firma Adı"),
                                IsyeriAdi = GetStringSafe(rdr, "IsyeriAdi", "İşyeri Adı"),
                                BuYilDogumGunu = GetDateTimeSafe(rdr, "BuYilDogumGunu", "Bu Yıl Doğum Günü"),
                                Gun = GetInt32Safe(rdr, "Gun", "Gün"),
                                Ay = GetInt32Safe(rdr, "Ay"),
                                Yas = GetInt32Safe(rdr, "Yas", "Yaş")
                            });
                        }
                    }

                    // RS3: İşe başlayanlar (Ad/Soyad ayrı veya "Adı Soyadı" birleşik; tarih string olabilir)
                    if (rdr.NextResult())
                    {
                        while (rdr.Read())
                        {
                            var ad = GetStringSafe(rdr, "Ad", "Adı");
                            var soyad = GetStringSafe(rdr, "Soyad", "Soyadı");
                            var adSoyad = GetStringSafe(rdr, "Adı Soyadı", "AdSoyad");
                            if (string.IsNullOrWhiteSpace(ad) && !string.IsNullOrWhiteSpace(adSoyad))
                                SplitAdSoyad(adSoyad, out ad, out soyad);

                            result.NewHires.Add(new IseBaslayanlarDashboard
                            {
                                PersonelId = GetInt32Safe(rdr, "PersonelId", "Sicil No"),
                                Ad = ad,
                                Soyad = soyad,
                                FirmaId = GetInt32Safe(rdr, "FirmaId"),
                                IsyeriId = GetInt32Safe(rdr, "IsyeriId"),
                                FirmaAdi = GetStringSafe(rdr, "FirmaAdi", "Firma Adı"),
                                IsyeriAdi = GetStringSafe(rdr, "IsyeriAdi", "İşyeri Adı"),
                                BaslamaTarihi = GetDateTimeSafe(rdr, "BaslamaTarihi", "Başlama Tarihi")
                            });
                        }
                    }

                    // RS4: İşten ayrılanlar
                    if (rdr.NextResult())
                    {
                        while (rdr.Read())
                        {
                            var ad = GetStringSafe(rdr, "Ad", "Adı");
                            var soyad = GetStringSafe(rdr, "Soyad", "Soyadı");
                            var adSoyad = GetStringSafe(rdr, "Adı Soyadı", "AdSoyad");
                            if (string.IsNullOrWhiteSpace(ad) && !string.IsNullOrWhiteSpace(adSoyad))
                                SplitAdSoyad(adSoyad, out ad, out soyad);

                            result.Resignations.Add(new IstenAyrilanlarDashboard
                            {
                                PersonelId = GetInt32Safe(rdr, "PersonelId", "Sicil No"),
                                Ad = ad,
                                Soyad = soyad,
                                FirmaId = GetInt32Safe(rdr, "FirmaId"),
                                IsyeriId = GetInt32Safe(rdr, "IsyeriId"),
                                FirmaAdi = GetStringSafe(rdr, "FirmaAdi", "Firma Adı"),
                                IsyeriAdi = GetStringSafe(rdr, "IsyeriAdi", "İşyeri Adı"),
                                AyrilmaTarihi = GetDateTimeSafe(rdr, "AyrilmaTarihi", "Ayrılma Tarihi")
                            });
                        }
                    }

                    // RS5: KPI kartları
                    if (rdr.NextResult() && rdr.Read())
                    {
                        result.Cards.GirisYapan = GetInt32Safe(rdr, "GirisYapan", "Hareketi Bulunanlar");
                        result.Cards.Iceridekiler = GetInt32Safe(rdr, "Iceridekiler", "İçeridekiler");
                        result.Cards.GecKalanlar = GetInt32Safe(rdr, "GecKalanlar", "Geç Kalanlar");
                        result.Cards.Disaridakiler = GetInt32Safe(rdr, "Disaridakiler", "Dışarıdakiler");
                        result.Cards.Devamsizlar = GetInt32Safe(rdr, "Devamsizlar", "Devamsızlar");
                        result.Cards.Izinli = GetInt32Safe(rdr, "Izinli", "İzinliler");
                        result.Cards.IseBaslayan = GetInt32Safe(rdr, "IseBaslayan", "İşe Başlayanlar");
                        result.Cards.IstenAyrilan = GetInt32Safe(rdr, "IstenAyrilan", "İşten Ayrılanlar");
                    }
                }
            }

            return result;
        }
    }
}
