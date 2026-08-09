using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.Business.Services
{
    public class AracKartiService : IAracKartiService
    {
        private readonly IKisiRepository _kisiRepo;
        private readonly IPuantajsizKartAtamaRepository _atamaRepo;

        public AracKartiService(IKisiRepository kisiRepo, IPuantajsizKartAtamaRepository atamaRepo)
        {
            _kisiRepo = kisiRepo;
            _atamaRepo = atamaRepo;
        }

        public List<KisiListItem> GetCardsForNew(int firmaId)
        {
            var tumKartlar = _kisiRepo.GetAktifByFirma(firmaId, null, puantajYapilirMi: false, isyeriId: null, aracKartiMi: true);
            var sonuc = new List<KisiListItem>();
            foreach (var k in tumKartlar)
            {
                if (string.IsNullOrWhiteSpace(k.PersonelId)) continue;
                if (_atamaRepo.ExistsActiveForCard(k.PersonelId))
                    continue;
                sonuc.Add(k);
            }
            return sonuc;
        }

        public List<PuantajsizKartAtama> GetTodayActiveAssignments(DateTime now, int firmaId)
        {
            return _atamaRepo.GetTodayActive(now, firmaId, aracKartiMi: true);
        }

        public int CreateAssignment(int firmaId, string personelId, string adSoyad, DateTime girisSaati, string aciklama, string tcKimlikNo, string ziyaretEdilenKisi, string plaka)
        {
            if (string.IsNullOrWhiteSpace(adSoyad))
                throw new ArgumentException("Ad soyad boş olamaz.", nameof(adSoyad));

            if (!_atamaRepo.CardBelongsToFirma(personelId, firmaId))
                throw new InvalidOperationException("Seçilen kart bu firmaya ait değil.");

            if (_atamaRepo.ExistsActiveForCard(personelId))
                throw new InvalidOperationException("Bu karta ait aktif bir atama zaten var. Önce çıkış veriniz.");

            var tc = string.IsNullOrWhiteSpace(tcKimlikNo) ? null : tcKimlikNo.Trim();

            return _atamaRepo.Insert(new PuantajsizKartAtama
            {
                KartId = personelId,
                MisafirAdSoyad = adSoyad.Trim(),
                TCKimlikNo = tc,
                ZiyaretEdilenKisi = string.IsNullOrWhiteSpace(ziyaretEdilenKisi) ? null : ziyaretEdilenKisi.Trim(),
                Baslangic = girisSaati,
                Bitis = null,
                Notlar = string.IsNullOrWhiteSpace(aciklama) ? "" : aciklama.Trim(),
                Plaka = string.IsNullOrWhiteSpace(plaka) ? null : plaka.Trim().ToUpperInvariant()
            });
        }

        public void UpdateAssignment(int atamaId, string adSoyad, DateTime girisSaati, DateTime? cikisSaati, string aciklama, string tcKimlikNo, string ziyaretEdilenKisi, string plaka)
        {
            var rec = _atamaRepo.GetById(atamaId);
            if (rec == null)
                throw new InvalidOperationException("Güncellenecek kayıt bulunamadı.");

            if (string.IsNullOrWhiteSpace(adSoyad))
                throw new ArgumentException("Ad soyad boş olamaz.", nameof(adSoyad));

            rec.MisafirAdSoyad = adSoyad.Trim();
            rec.Baslangic = girisSaati;
            rec.Bitis = cikisSaati;
            rec.Notlar = string.IsNullOrWhiteSpace(aciklama) ? "" : aciklama.Trim();
            rec.TCKimlikNo = string.IsNullOrWhiteSpace(tcKimlikNo) ? null : tcKimlikNo.Trim();
            rec.ZiyaretEdilenKisi = string.IsNullOrWhiteSpace(ziyaretEdilenKisi) ? null : ziyaretEdilenKisi.Trim();
            rec.Plaka = string.IsNullOrWhiteSpace(plaka) ? null : plaka.Trim().ToUpperInvariant();

            _atamaRepo.Update(rec);
        }

        public PuantajsizKartAtama GetBilgisiByTc(string tcKimlikNo)
        {
            if (string.IsNullOrWhiteSpace(tcKimlikNo))
                return null;

            return _atamaRepo.GetSonAtamaByTcKimlikNo(tcKimlikNo.Trim());
        }

        public List<GecmisZiyaretciItem> SearchGecmisZiyaretciler(int firmaId, string adFilter)
        {
            return _atamaRepo.GetGecmisZiyaretciler(firmaId, adFilter, ziyaretciMi: null, aracKartiMi: true);
        }
    }
}
