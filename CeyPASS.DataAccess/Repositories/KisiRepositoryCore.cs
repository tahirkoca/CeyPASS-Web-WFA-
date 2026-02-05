using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    public class KisiRepositoryCore : IKisiRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public KisiRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        public bool Exists(string personelId)
        {
            return _context.Kisiler.Any(k => k.PersonelId == personelId);
        }

        public List<KisiListItem> GetAktifByFirma(int firmId, string search = null, bool? puantajYapilirMi = true, int? isyeriId = null)
        {
            var q = _context.Kisiler
                .Where(k => k.FirmaId == firmId && k.IstenCikisTarihi == null);

            if (puantajYapilirMi.HasValue)
                q = q.Where(k => k.PuantajYapilirMi == puantajYapilirMi.Value);

            if (isyeriId.HasValue)
                q = q.Where(k => k.IsyeriId == isyeriId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                q = q.Where(k =>
                    (k.Ad ?? "").Contains(search) ||
                    (k.Soyad ?? "").Contains(search) ||
                    (k.PersonelId ?? "").Contains(search) ||
                    (k.TcKimlikNo ?? "").Contains(search));
            }

            return q
                .OrderBy(k => k.Ad)
                .ThenBy(k => k.Soyad)
                .Select(k => new KisiListItem
                {
                    PersonelId = k.PersonelId,
                    AdSoyad = ((k.Ad ?? "") + " " + (k.Soyad ?? "")).Trim()
                })
                .ToList();
        }

        public KisiDetay GetDetay(string personelId)
        {
            var k = _context.Kisiler.FirstOrDefault(x => x.PersonelId == personelId);
            if (k == null) return null;

            var limit = _context.YemekhaneGirisLimitler
                .FirstOrDefault(y => y.PersonelId == personelId && y.AktifMi == true);

            bool yemekHakkiVar = (limit != null);
            int? gunlukLimit = limit?.GunlukLimit;

            string taseronKartNo = null;
            if (int.TryParse(personelId, out var pidInt))
            {
                taseronKartNo = _context.TaseronKartlari
                    .Where(t => t.PersonelId == pidInt && t.AktifMi)
                    .OrderByDescending(t => t.OlusturmaTarihi)
                    .Select(t => t.TaseronId)
                    .FirstOrDefault();
            }

            int? calismaStatusuId = null;
            if (k.CalismaStatusu != null)
            {
                if (int.TryParse(k.CalismaStatusu.ToString(), out var cs))
                    calismaStatusuId = cs;
            }

            var detay = new KisiDetay
            {
                PersonelId = k.PersonelId,
                Ad = k.Ad,
                Soyad = k.Soyad,
                KartNo = k.KartNo,
                TcKimlikNo = k.TcKimlikNo,
                PozisyonId = k.PozisyonId,
                DepartmanId = k.DepartmanId,
                FirmaId = (int)k.FirmaId,
                IsyeriId = k.IsyeriId,
                BolumId = k.BolumId,
                DogumTarihi = k.DogumTarihi,
                IseGirisTarihi = k.IseGirisTarihi,
                IstenCikisTarihi = k.IstenCikisTarihi,
                CalismaStatusuId = calismaStatusuId,
                CalismaSekliCsv = k.CalismaSekli,
                CepTel = k.CepTel,
                Email = k.Email,
                Fotograf = k.Fotograf,
                PuantajYapilabilir = (bool)k.PuantajYapilirMi,
                FirmaPersoneli = true,
                YemekHakkiVar = yemekHakkiVar,
                GunlukYemekAdedi = gunlukLimit,
                TaseronKartNo = taseronKartNo
            };
            return detay;
        }

        public void SetIstenCikisTarihi(string personelId, DateTime tarih)
        {
            var sql = @"
UPDATE dbo.Kisiler
   SET IstenCikisTarihi = @p0,
       PuantajYapilirMi = 0
 WHERE PersonelId = @p1";

            _context.Database.ExecuteSqlRaw(sql,
                new Microsoft.Data.SqlClient.SqlParameter("@p0", tarih.Date),
                new Microsoft.Data.SqlClient.SqlParameter("@p1", personelId));
        }

        public List<Kisi> GetKisilerForPuantaj(int firmaId, int isyeriId, int yil, int ay)
        {
            var ayBasi = new DateTime(yil, ay, 1);
            var aySonu = new DateTime(yil, ay, DateTime.DaysInMonth(yil, ay));

            var q = _context.Kisiler
                .Where(k => k.FirmaId == firmaId
                            && k.IsyeriId == isyeriId
                            && k.IseGirisTarihi <= aySonu
                            && (k.IstenCikisTarihi == null || k.IstenCikisTarihi >= ayBasi)
                            && k.PuantajYapilirMi == true)
                .OrderBy(k => k.Ad)
                .ThenBy(k => k.Soyad);

            return q.Select(k => new Kisi
            {
                PersonelId = k.PersonelId,
                Ad = k.Ad,
                Soyad = k.Soyad,
                FirmaId = (int)k.FirmaId,
                IsyeriId = k.IsyeriId ?? 0,
                IseGirisTarihi = (DateTime)k.IseGirisTarihi,
                IstenCikisTarihi = k.IstenCikisTarihi
            }).ToList();
        }

        private void UpsertTaseronKart(int personelId, string kartNo)
        {
            if (string.IsNullOrWhiteSpace(kartNo))
            {
                var aktifler = _context.TaseronKartlari
                    .Where(t => t.PersonelId == personelId && t.AktifMi)
                    .ToList();

                foreach (var t in aktifler)
                {
                    t.AktifMi = false;
                    t.GuncellemeTarihi = DateTime.Now;
                }

                return;
            }

            kartNo = kartNo.Trim();

            var digerAktifler = _context.TaseronKartlari
                .Where(t => t.PersonelId == personelId
                            && t.AktifMi
                            && t.TaseronId != kartNo)
                .ToList();

            foreach (var t in digerAktifler)
            {
                t.AktifMi = false;
                t.GuncellemeTarihi = DateTime.Now;
            }

            var mevcut = _context.TaseronKartlari
                .FirstOrDefault(t => t.PersonelId == personelId && t.TaseronId == kartNo);

            if (mevcut != null)
            {
                mevcut.AktifMi = true;
                mevcut.GuncellemeTarihi = DateTime.Now;
            }
            else
            {
                var yeni = new CeyPASS.DataAccess.TaseronKartlari
                {
                    PersonelId = personelId,
                    TaseronId = kartNo,
                    AktifMi = true,
                    OlusturmaTarihi = DateTime.Now,
                    GuncellemeTarihi = null
                };
                _context.TaseronKartlari.Add(yeni);
            }
        }

        public bool Update(Kisi k, string originalPersonelId, bool fotoDirty, string firmaDisiKartNo = null)
        {
            if (k == null) throw new ArgumentNullException(nameof(k));

            var oldId = (originalPersonelId ?? "").Trim();
            var newId = (k.PersonelId ?? "").Trim();

            if (string.IsNullOrWhiteSpace(oldId))
                throw new ArgumentException("originalPersonelId boş olamaz.", nameof(originalPersonelId));
            if (string.IsNullOrWhiteSpace(newId))
                throw new ArgumentException("Yeni PersonelId boş olamaz.", nameof(k.PersonelId));

            using (var tx = _context.Database.BeginTransaction())
            {
                try
                {
                    if (!_context.Kisiler.Any(x => x.PersonelId == oldId))
                        return false;

                    if (!string.Equals(oldId, newId, StringComparison.OrdinalIgnoreCase))
                    {
                        if (_context.Kisiler.Any(x => x.PersonelId == newId))
                            throw new InvalidOperationException($"'{newId}' PersonelId zaten mevcut. Değiştirilemez.");
                    }

                    var sql = @"
UPDATE dbo.Kisiler
   SET PersonelId        = @p1,
       Ad               = @p2,
       Soyad            = @p3,
       KartNo           = @p4,
       TcKimlikNo       = @p5,
       PozisyonId       = @p6,
       DogumTarihi      = @p7,
       DepartmanId      = @p8,
       IseGirisTarihi   = @p9,
       IstenCikisTarihi = @p10,
       CalismaStatusu   = @p11,
       FirmaId          = @p12,
       IsyeriId         = @p13,
       BolumId          = @p14,
       CalismaSekli     = @p15,
       CepTel           = @p16,
       Email            = @p17,
       PuantajYapilirMi = @p18,
       Fotograf         = CASE WHEN @p19 = 1 THEN @p20 ELSE Fotograf END
 WHERE PersonelId = @p0";

                    var parameters = new[]
                    {
                        new Microsoft.Data.SqlClient.SqlParameter("@p0", SqlDbType.NVarChar) { Value = oldId },
                        new Microsoft.Data.SqlClient.SqlParameter("@p1", SqlDbType.NVarChar) { Value = newId },
                        new Microsoft.Data.SqlClient.SqlParameter("@p2", SqlDbType.NVarChar) { Value = k.Ad ?? (object)DBNull.Value },
                        new Microsoft.Data.SqlClient.SqlParameter("@p3", SqlDbType.NVarChar) { Value = k.Soyad ?? (object)DBNull.Value },
                        new Microsoft.Data.SqlClient.SqlParameter("@p4", SqlDbType.NVarChar) { Value = k.KartNo ?? (object)DBNull.Value },
                        new Microsoft.Data.SqlClient.SqlParameter("@p5", SqlDbType.NVarChar) { Value = k.TcKimlikNo ?? (object)DBNull.Value },
                        new Microsoft.Data.SqlClient.SqlParameter("@p6", SqlDbType.Int) { Value = k.PozisyonId ?? (object)DBNull.Value },
                        new Microsoft.Data.SqlClient.SqlParameter("@p7", SqlDbType.DateTime) { Value = k.DogumTarihi ?? (object)DBNull.Value },
                        new Microsoft.Data.SqlClient.SqlParameter("@p8", SqlDbType.Int) { Value = k.DepartmanId ?? (object)DBNull.Value },
                        new Microsoft.Data.SqlClient.SqlParameter("@p9", SqlDbType.DateTime) { Value = (object)k.IseGirisTarihi },
                        new Microsoft.Data.SqlClient.SqlParameter("@p10", SqlDbType.DateTime) { Value = k.IstenCikisTarihi ?? (object)DBNull.Value },
                        new Microsoft.Data.SqlClient.SqlParameter("@p11", SqlDbType.NVarChar) { Value = k.CalismaStatusu ?? (object)DBNull.Value },
                        new Microsoft.Data.SqlClient.SqlParameter("@p12", SqlDbType.Int) { Value = k.FirmaId },
                        new Microsoft.Data.SqlClient.SqlParameter("@p13", SqlDbType.Int) { Value = k.IsyeriId ?? (object)DBNull.Value },
                        new Microsoft.Data.SqlClient.SqlParameter("@p14", SqlDbType.Int) { Value = k.BolumId ?? (object)DBNull.Value },
                        new Microsoft.Data.SqlClient.SqlParameter("@p15", SqlDbType.NVarChar) { Value = k.CalismaSekli ?? (object)DBNull.Value },
                        new Microsoft.Data.SqlClient.SqlParameter("@p16", SqlDbType.NVarChar) { Value = k.CepTel ?? (object)DBNull.Value },
                        new Microsoft.Data.SqlClient.SqlParameter("@p17", SqlDbType.NVarChar) { Value = k.Email ?? (object)DBNull.Value },
                        new Microsoft.Data.SqlClient.SqlParameter("@p18", SqlDbType.Bit) { Value = k.PuantajYapilirMi },
                        new Microsoft.Data.SqlClient.SqlParameter("@p19", SqlDbType.Bit) { Value = fotoDirty ? 1 : 0 },
                        new Microsoft.Data.SqlClient.SqlParameter("@p20", SqlDbType.Image) { Value = k.Fotograf ?? (object)DBNull.Value }
                    };

                    _context.Database.ExecuteSqlRaw(sql, parameters);

                    if (!string.Equals(oldId, newId, StringComparison.OrdinalIgnoreCase))
                    {
                        var sql1 = "UPDATE dbo.YemekhaneGirisLimitler SET PersonelId=@p1 WHERE PersonelId=@p0";
                        _context.Database.ExecuteSqlRaw(sql1,
                            new Microsoft.Data.SqlClient.SqlParameter("@p0", oldId),
                            new Microsoft.Data.SqlClient.SqlParameter("@p1", newId));

                        var kartIdExists = _context.PuantajsizKartlar.Any(x => x.KartId == newId);
                        if (!kartIdExists)
                        {
                            var sql2 = "UPDATE dbo.PuantajsizKartlar SET KartId=@p1 WHERE KartId=@p0";
                            _context.Database.ExecuteSqlRaw(sql2,
                                new Microsoft.Data.SqlClient.SqlParameter("@p0", oldId),
                                new Microsoft.Data.SqlClient.SqlParameter("@p1", newId));
                        }
                    }

                    if (firmaDisiKartNo != null && int.TryParse(newId, out var pidInt))
                        UpsertTaseronKart(pidInt, firmaDisiKartNo);

                    _context.SaveChanges();
                    tx.Commit();
                    return true;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public void Insert(Kisi k, string firmaDisiKartNo = null)
        {
            var entity = new CeyPASS.DataAccess.Kisiler
            {
                PersonelId = k.PersonelId,
                Ad = k.Ad,
                Soyad = k.Soyad,
                KartNo = k.KartNo,
                TcKimlikNo = k.TcKimlikNo,
                PozisyonId = k.PozisyonId,
                DogumTarihi = k.DogumTarihi,
                DepartmanId = k.DepartmanId,
                IseGirisTarihi = k.IseGirisTarihi,
                IstenCikisTarihi = k.IstenCikisTarihi,
                CalismaStatusu = k.CalismaStatusu,
                FirmaId = k.FirmaId,
                IsyeriId = k.IsyeriId,
                CalismaSekli = k.CalismaSekli,
                CepTel = k.CepTel,
                Fotograf = k.Fotograf,
                KayitTarihi = DateTime.Now,
                Email = k.Email,
                PuantajYapilirMi = k.PuantajYapilirMi,
                BolumId = k.BolumId
            };

            _context.Kisiler.Add(entity);

            if (firmaDisiKartNo != null && int.TryParse(k.PersonelId, out var pidInt))
            {
                UpsertTaseronKart(pidInt, firmaDisiKartNo);
            }

            _context.SaveChanges();
        }

        public KisiAdSoyad GetAdSoyadByPersonelId(string personelId)
        {
            var k = _context.Kisiler.FirstOrDefault(x => x.PersonelId == personelId);
            if (k == null) return null;

            return new KisiAdSoyad
            {
                Ad = k.Ad ?? "",
                Soyad = k.Soyad ?? ""
            };
        }

        public List<PersonelCihazItem> GetAktifKartliPersonellerForSync()
        {
            var q = _context.Kisiler
                .Where(k => k.IstenCikisTarihi == null && k.KartNo != null);

            return q.Select(k => new PersonelCihazItem
            {
                SicilNo = k.PersonelId ?? "",
                AdSoyad = ((k.Ad ?? "") + " " + (k.Soyad ?? "")).Trim(),
                KartNo = k.KartNo ?? ""
            }).ToList();
        }

        public List<PersonelAdSoyad> GetAktifPersonellerIdAd()
        {
            var raw = _context.Kisiler
                .Where(k => k.IstenCikisTarihi == null)
                .Select(k => new { k.PersonelId, k.Ad, k.Soyad })
                .ToList();

            var list = new List<PersonelAdSoyad>();

            foreach (var r in raw)
            {
                if (!int.TryParse(r.PersonelId, out var id))
                    continue;

                list.Add(new PersonelAdSoyad
                {
                    Id = id,
                    AdSoyad = $"{(r.Ad ?? "")} {(r.Soyad ?? "")}".Trim()
                });
            }

            return list;
        }

        public KisiDetayDTO GetById(int kisiId)
        {
            string pid = kisiId.ToString();

            var k = _context.Kisiler.FirstOrDefault(x => x.PersonelId == pid);
            if (k == null) return null;

            CeyPASS.DataAccess.Departmanlar? dept = null;
            CeyPASS.DataAccess.Pozisyonlar? poz = null;

            if (k.DepartmanId.HasValue)
                dept = _context.Departmanlar.FirstOrDefault(d => d.DepartmanId == k.DepartmanId.Value);

            if (k.PozisyonId.HasValue)
                poz = _context.Pozisyonlar.FirstOrDefault(p => p.PozisyonId == k.PozisyonId.Value);

            return new KisiDetayDTO
            {
                AdSoyad = $"{(k.Ad ?? "")} {(k.Soyad ?? "")}".Trim(),
                Unvan = poz?.PozisyonAdi,
                Departman = dept?.DepartmanAdi,
                Foto = k.Fotograf
            };
        }
    }
}
