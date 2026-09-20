using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    /// <summary>CokluSicilBaglantilari erişimi.</summary>
    public class CokluSicilRepositoryCore : ICokluSicilRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public CokluSicilRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        /// <summary>By Ana Personel Id sorgularını getirir.</summary>
        public List<CokluSicilBaglantiDTO> GetByAnaPersonelId(int anaPersonelId, bool yalnizcaAktif = false)
        {
            var q = _context.CokluSicilBaglantilari.AsNoTracking()
                .Where(x => x.AnaPersonelId == anaPersonelId);
            if (yalnizcaAktif)
                q = q.Where(x => x.AktifMi);

            return MapBaglantilar(q.ToList());
        }

        /// <summary>By Hedef Personel Id sorgularını getirir.</summary>
        public List<CokluSicilBaglantiDTO> GetByHedefPersonelId(int hedefPersonelId, bool yalnizcaAktif = false)
        {
            var q = _context.CokluSicilBaglantilari.AsNoTracking()
                .Where(x => x.HedefPersonelId == hedefPersonelId);
            if (yalnizcaAktif)
                q = q.Where(x => x.AktifMi);

            return MapBaglantilar(q.ToList());
        }

        /// <summary>Hedef Adaylari sorgularını getirir.</summary>
        public List<CokluSicilHedefAdayDTO> GetHedefAdaylari(string tcKimlikNo, int anaPersonelId)
        {
            if (string.IsNullOrWhiteSpace(tcKimlikNo))
                return new List<CokluSicilHedefAdayDTO>();

            var tc = tcKimlikNo.Trim();
            var mevcutBaglantilar = _context.CokluSicilBaglantilari.AsNoTracking()
                .Where(x => x.AnaPersonelId == anaPersonelId)
                .AsEnumerable()
                .GroupBy(x => x.HedefPersonelId)
                .ToDictionary(g => g.Key, g => g.First());

            var baskaAnaAktif = _context.CokluSicilBaglantilari.AsNoTracking()
                .Where(x => x.AktifMi && x.AnaPersonelId != anaPersonelId)
                .AsEnumerable()
                .GroupBy(x => x.HedefPersonelId)
                .ToDictionary(g => g.Key, g => g.First().AnaPersonelId);

            var kisiler = _context.Kisiler.AsNoTracking()
                .Where(k => k.TcKimlikNo == tc && k.PuantajYapilirMi == true)
                .ToList();

            var firmaIds = kisiler.Where(k => k.FirmaId.HasValue).Select(k => k.FirmaId!.Value).Distinct().ToList();
            var isyeriIds = kisiler.Where(k => k.IsyeriId.HasValue).Select(k => k.IsyeriId!.Value).Distinct().ToList();
            var bolumIds = kisiler.Where(k => k.BolumId.HasValue).Select(k => k.BolumId!.Value).Distinct().ToList();
            var firmaMap = firmaIds.Count == 0
                ? new Dictionary<int, string>()
                : _context.Firmalar.AsNoTracking()
                    .Where(f => firmaIds.Contains(f.FirmaId))
                    .AsEnumerable()
                    .GroupBy(f => f.FirmaId)
                    .ToDictionary(g => g.Key, g => g.First().FirmaAdi);
            var isyeriMap = isyeriIds.Count == 0
                ? new Dictionary<int, string>()
                : _context.Isyerler.AsNoTracking()
                    .Where(i => i.IsyeriId.HasValue && isyeriIds.Contains(i.IsyeriId.Value))
                    .AsEnumerable()
                    .GroupBy(i => i.IsyeriId!.Value)
                    .ToDictionary(g => g.Key, g => g.First().IsyeriAdi ?? "");
            var bolumMap = bolumIds.Count == 0
                ? new Dictionary<int, string>()
                : _context.Bolumler.AsNoTracking()
                    .Where(b => b.BolumId.HasValue && bolumIds.Contains(b.BolumId.Value))
                    .AsEnumerable()
                    .GroupBy(b => b.BolumId!.Value)
                    .ToDictionary(g => g.Key, g => g.First().BolumAdi ?? "");

            var result = new List<CokluSicilHedefAdayDTO>();
            foreach (var k in kisiler)
            {
                if (!int.TryParse(k.PersonelId, out var pid) || pid == anaPersonelId)
                    continue;

                mevcutBaglantilar.TryGetValue(pid, out var bag);
                baskaAnaAktif.TryGetValue(pid, out var baskaAnaId);

                var bagliAktifBuAna = bag?.AktifMi == true;
                var bagliAktifBaskaAna = baskaAnaId > 0 && !bagliAktifBuAna;
                var secilebilir = !bagliAktifBuAna && !bagliAktifBaskaAna;
                string? engel = null;
                if (bagliAktifBuAna)
                    engel = "Bu ana sicile zaten aktif bağlı";
                else if (bagliAktifBaskaAna)
                    engel = $"Başka ana sicile bağlı (sicil {baskaAnaId})";

                result.Add(new CokluSicilHedefAdayDTO
                {
                    PersonelId = pid,
                    AdSoyad = $"{k.Ad} {k.Soyad}".Trim(),
                    FirmaId = k.FirmaId,
                    FirmaAdi = k.FirmaId.HasValue && firmaMap.TryGetValue(k.FirmaId.Value, out var fa) ? fa : null,
                    IsyeriId = k.IsyeriId,
                    IsyeriAdi = k.IsyeriId.HasValue && isyeriMap.TryGetValue(k.IsyeriId.Value, out var ia) ? ia : null,
                    BolumId = k.BolumId,
                    BolumAdi = k.BolumId.HasValue && bolumMap.TryGetValue(k.BolumId.Value, out var ba) ? ba : null,
                    IseGirisTarihi = k.IseGirisTarihi,
                    IstenCikisTarihi = k.IstenCikisTarihi,
                    PuantajYapilirMi = k.PuantajYapilirMi == true,
                    ZatenBagli = bag != null,
                    BagliAktif = bagliAktifBuAna,
                    BagliAnaPersonelId = bagliAktifBaskaAna ? baskaAnaId : null,
                    SecilebilirMi = secilebilir,
                    EngelMesaji = engel
                });
            }

            return result.OrderBy(x => x.AdSoyad).ToList();
        }

        /// <summary>Ozet sorgularını getirir.</summary>
        public CokluSicilOzetDTO GetOzet(int personelId)
        {
            var anaAktif = _context.CokluSicilBaglantilari.AsNoTracking()
                .Count(x => x.AnaPersonelId == personelId && x.AktifMi);
            var hedefKayit = _context.CokluSicilBaglantilari.AsNoTracking()
                .Where(x => x.HedefPersonelId == personelId && x.AktifMi)
                .Select(x => (int?)x.AnaPersonelId)
                .FirstOrDefault();

            return new CokluSicilOzetDTO
            {
                IsAnaSicil = anaAktif > 0 || _context.CokluSicilBaglantilari.AsNoTracking()
                    .Any(x => x.AnaPersonelId == personelId),
                IsHedefSicil = hedefKayit.HasValue,
                AktifHedefSayisi = anaAktif,
                AnaPersonelId = hedefKayit
            };
        }

        /// <summary>Upsert işlemini yazar veya günceller.</summary>
        public void Upsert(CokluSicilUpsertRequest request, int anaPersonelId, string tcKimlikNo, int? kullaniciId)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (anaPersonelId <= 0) throw new ArgumentException("Geçersiz ana personel.");
            if (request.HedefPersonelId <= 0) throw new ArgumentException("Geçersiz hedef personel.");
            if (anaPersonelId == request.HedefPersonelId)
                throw new InvalidOperationException("Ana ve hedef sicil aynı olamaz.");

            var tc = (tcKimlikNo ?? "").Trim();
            if (string.IsNullOrEmpty(tc))
                throw new InvalidOperationException("TC Kimlik No zorunludur.");

            var hedef = _context.Kisiler.AsNoTracking()
                .FirstOrDefault(k => k.PersonelId == request.HedefPersonelId.ToString());
            if (hedef == null)
                throw new InvalidOperationException("Hedef sicil Kişiler tablosunda bulunamadı.");
            if (!string.Equals(hedef.TcKimlikNo?.Trim(), tc, StringComparison.Ordinal))
                throw new InvalidOperationException("Hedef sicil aynı TC Kimlik No'ya sahip olmalıdır.");
            if (hedef.PuantajYapilirMi != true)
                throw new InvalidOperationException("Hedef sicilde Puantaj Yapılır işaretli olmalıdır.");

            if (request.AktarimGunSayisi < 1)
                throw new InvalidOperationException("Aktarım gün sayısı en az 1 olmalıdır.");

            if (request.AktifMi)
            {
                var baskaAna = _context.CokluSicilBaglantilari.AsNoTracking()
                    .FirstOrDefault(x => x.HedefPersonelId == request.HedefPersonelId
                        && x.AktifMi && x.AnaPersonelId != anaPersonelId);
                if (baskaAna != null)
                    throw new InvalidOperationException(
                        $"Sicil {request.HedefPersonelId} başka bir ana sicile bağlı (sicil {baskaAna.AnaPersonelId}).");
            }

            var mevcut = _context.CokluSicilBaglantilari
                .FirstOrDefault(x => x.AnaPersonelId == anaPersonelId && x.HedefPersonelId == request.HedefPersonelId);

            var now = DateTime.Now;
            if (mevcut == null)
            {
                var yeni = new CokluSicilBaglantilari
                {
                    TCKimlikNo = tc,
                    AnaPersonelId = anaPersonelId,
                    HedefPersonelId = request.HedefPersonelId,
                    FirmaId = request.FirmaId ?? hedef.FirmaId,
                    SirketId = request.SirketId ?? hedef.IsyeriId,
                    BolumId = request.BolumId ?? hedef.BolumId,
                    IseGirisTarihi = request.IseGirisTarihi ?? hedef.IseGirisTarihi,
                    IstenCikisTarihi = request.IstenCikisTarihi ?? hedef.IstenCikisTarihi,
                    AktarimGunSayisi = request.AktarimGunSayisi,
                    AktifMi = request.AktifMi,
                    Aciklama = request.Aciklama,
                    OlusturmaZamani = now,
                    OlusturanKullaniciId = kullaniciId ?? 0
                };
                _context.CokluSicilBaglantilari.Add(yeni);
            }
            else
            {
                mevcut.TCKimlikNo = tc;
                mevcut.FirmaId = request.FirmaId ?? hedef.FirmaId;
                mevcut.SirketId = request.SirketId ?? hedef.IsyeriId;
                mevcut.BolumId = request.BolumId ?? hedef.BolumId;
                mevcut.IseGirisTarihi = request.IseGirisTarihi ?? hedef.IseGirisTarihi;
                mevcut.IstenCikisTarihi = request.IstenCikisTarihi ?? hedef.IstenCikisTarihi;
                mevcut.AktarimGunSayisi = request.AktarimGunSayisi;
                mevcut.AktifMi = request.AktifMi;
                mevcut.Aciklama = request.Aciklama;
                mevcut.GuncellemeZamani = now;
                mevcut.GuncelleyenKullaniciId = kullaniciId;
            }

            _context.SaveChanges();
        }

        /// <summary>Aktif değerini ayarlar.</summary>
        public void SetAktif(int anaPersonelId, int hedefPersonelId, bool aktif, int? kullaniciId)
        {
            var mevcut = _context.CokluSicilBaglantilari
                .FirstOrDefault(x => x.AnaPersonelId == anaPersonelId && x.HedefPersonelId == hedefPersonelId);
            if (mevcut == null)
                throw new InvalidOperationException("Bağlantı bulunamadı.");

            mevcut.AktifMi = aktif;
            mevcut.GuncellemeZamani = DateTime.Now;
            mevcut.GuncelleyenKullaniciId = kullaniciId;
            _context.SaveChanges();
        }

        /// <summary>Ana sicile bağlı tüm hedef bağlantıları pasifleştirir.</summary>
        public void PasifleştirTümünü(int anaPersonelId, int? kullaniciId)
        {
            var liste = _context.CokluSicilBaglantilari
                .Where(x => x.AnaPersonelId == anaPersonelId && x.AktifMi)
                .ToList();
            if (liste.Count == 0) return;

            var now = DateTime.Now;
            foreach (var x in liste)
            {
                x.AktifMi = false;
                x.GuncellemeZamani = now;
                x.GuncelleyenKullaniciId = kullaniciId;
            }
            _context.SaveChanges();
        }

        /// <summary>Sicilin aktif çoklu sicil ana kaydı olup olmadığını kontrol eder.</summary>
        public bool IsAnaSicil(int personelId)
            => _context.CokluSicilBaglantilari.AsNoTracking()
                .Any(x => x.AnaPersonelId == personelId && x.AktifMi);

        /// <summary>Is Hedef Sicil işlemini gerçekleştirir.</summary>
        public bool IsHedefSicil(int personelId)
            => _context.CokluSicilBaglantilari.AsNoTracking()
                .Any(x => x.HedefPersonelId == personelId && x.AktifMi);

        /// <summary>Aktif Hedef Sayisi sorgularını getirir.</summary>
        public int GetAktifHedefSayisi(int anaPersonelId)
            => _context.CokluSicilBaglantilari.AsNoTracking()
                .Count(x => x.AnaPersonelId == anaPersonelId && x.AktifMi);

        private List<CokluSicilBaglantiDTO> MapBaglantilar(List<CokluSicilBaglantilari> rows)
        {
            if (rows.Count == 0) return new List<CokluSicilBaglantiDTO>();

            var hedefIds = rows.Select(r => r.HedefPersonelId.ToString()).Distinct().ToList();
            var kisiMap = _context.Kisiler.AsNoTracking()
                .Where(k => hedefIds.Contains(k.PersonelId))
                .AsEnumerable()
                .GroupBy(k => k.PersonelId)
                .ToDictionary(g => g.Key, g => g.First());

            var firmaIds = rows.Where(r => r.FirmaId.HasValue).Select(r => r.FirmaId!.Value)
                .Concat(kisiMap.Values.Where(k => k.FirmaId.HasValue).Select(k => k.FirmaId!.Value))
                .Distinct().ToList();
            var isyeriIds = rows.Where(r => r.SirketId.HasValue).Select(r => r.SirketId!.Value)
                .Concat(kisiMap.Values.Where(k => k.IsyeriId.HasValue).Select(k => k.IsyeriId!.Value))
                .Distinct().ToList();
            var bolumIds = rows.Where(r => r.BolumId.HasValue).Select(r => r.BolumId!.Value).Distinct().ToList();

            var firmaMap = firmaIds.Count == 0
                ? new Dictionary<int, string>()
                : _context.Firmalar.AsNoTracking().Where(f => firmaIds.Contains(f.FirmaId))
                    .AsEnumerable()
                    .GroupBy(f => f.FirmaId)
                    .ToDictionary(g => g.Key, g => g.First().FirmaAdi);
            var isyeriMap = isyeriIds.Count == 0
                ? new Dictionary<int, string>()
                : _context.Isyerler.AsNoTracking()
                    .Where(i => i.IsyeriId.HasValue && isyeriIds.Contains(i.IsyeriId.Value))
                    .AsEnumerable()
                    .GroupBy(i => i.IsyeriId!.Value)
                    .ToDictionary(g => g.Key, g => g.First().IsyeriAdi ?? "");
            var bolumMap = bolumIds.Count == 0
                ? new Dictionary<int, string>()
                : _context.Bolumler.AsNoTracking()
                    .Where(b => b.BolumId.HasValue && bolumIds.Contains(b.BolumId.Value))
                    .AsEnumerable()
                    .GroupBy(b => b.BolumId!.Value)
                    .ToDictionary(g => g.Key, g => g.First().BolumAdi ?? "");

            return rows.Select(r =>
            {
                kisiMap.TryGetValue(r.HedefPersonelId.ToString(), out var k);
                var adSoyad = k != null ? $"{k.Ad} {k.Soyad}".Trim() : "";
                var firmaId = r.FirmaId ?? k?.FirmaId;
                var isyeriId = r.SirketId ?? k?.IsyeriId;
                var bolumId = r.BolumId ?? k?.BolumId;
                return new CokluSicilBaglantiDTO
                {
                    TCKimlikNo = r.TCKimlikNo,
                    AnaPersonelId = r.AnaPersonelId,
                    HedefPersonelId = r.HedefPersonelId,
                    HedefAdSoyad = adSoyad,
                    FirmaId = firmaId,
                    FirmaAdi = firmaId.HasValue && firmaMap.TryGetValue(firmaId.Value, out var fa) ? fa : null,
                    SirketId = isyeriId,
                    IsyeriAdi = isyeriId.HasValue && isyeriMap.TryGetValue(isyeriId.Value, out var ia) ? ia : null,
                    BolumId = bolumId,
                    BolumAdi = bolumId.HasValue && bolumMap.TryGetValue(bolumId.Value, out var ba) ? ba : null,
                    IseGirisTarihi = r.IseGirisTarihi,
                    IstenCikisTarihi = r.IstenCikisTarihi,
                    AktarimGunSayisi = r.AktarimGunSayisi,
                    AktifMi = r.AktifMi,
                    Aciklama = r.Aciklama,
                    OlusturmaZamani = r.OlusturmaZamani,
                    OlusturanKullaniciId = r.OlusturanKullaniciId,
                    GuncellemeZamani = r.GuncellemeZamani,
                    GuncelleyenKullaniciId = r.GuncelleyenKullaniciId
                };
            }).OrderBy(x => x.HedefPersonelId).ToList();
        }
    }
}
